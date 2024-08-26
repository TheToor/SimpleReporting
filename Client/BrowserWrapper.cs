using CefSharp;
using CefSharp.Wpf;
using Reporting.Client.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Reporting.Client
{
    internal class BrowserWrapper : IDisposable
    {
        private readonly MainWindow _form;

        private readonly SemaphoreSlim _browserSync = new SemaphoreSlim(1, 1);
        private int _browserCounter;
        private int _currentTab;
        private readonly Dictionary<int, ChromiumWebBrowser> _browsers = new Dictionary<int, ChromiumWebBrowser>();
        private readonly Dictionary<int, UrlSettings> _browserSettings = new Dictionary<int, UrlSettings>();

        internal BrowserWrapper(MainWindow mainform, Settings settings)
        {
            _form = mainform;

            var cefSettings = new CefSettings();

            if (settings.Proxy == null || settings.Proxy.ProxyUrl == "no")
            {
                cefSettings.CefCommandLineArgs.Add("no-proxy-server", "1");
            }
            else if (settings.Proxy.ProxyUrl == "auto")
            {
                cefSettings.CefCommandLineArgs.Add("proxy-auto-detect", "1");
            }
            else if (settings.Proxy.ProxyUrl.StartsWith("pac://"))
            {
                var url = settings.Proxy.ProxyUrl.Replace("pac://", "");
                cefSettings.CefCommandLineArgs.Add("proxy-pac-url", url);
            }
            else
            {
                cefSettings.CefCommandLineArgs.Add("proxy-server", $"{settings.Proxy.ProxyUrl}:{settings.Proxy.ProxyPort}");
            }

            cefSettings.RootCachePath = Path.Combine(Environment.GetEnvironmentVariable("Temp"), "MonitoringClient");
            if(!Directory.Exists(cefSettings.RootCachePath))
            {
                Directory.CreateDirectory(cefSettings.RootCachePath);
            }
            
            Cef.Initialize(cefSettings);
        }

        internal void CreateTab(UrlSettings urlsettings)
        {
            try
            { 
                _browserSync.Wait();

                var browser = new ChromiumWebBrowser();
                _form.RootGrid.Children.Add(browser);
                browser.VerticalAlignment = VerticalAlignment.Stretch;
                browser.HorizontalAlignment = HorizontalAlignment.Stretch;
                browser.Visibility = Visibility.Hidden;

                browser.IsBrowserInitializedChanged += OnBrowserInitialized;
                browser.LoadingStateChanged += OnBrowserLoadingStateChanged;

                var id = _browserCounter++;
                browser.Uid = id.ToString();

                _browsers.Add(id, browser);
                _browserSettings.Add(id, urlsettings);
            }
            finally
            {
                _browserSync.Release();
            }
        }

        internal void NextTab()
        {
            if (_browsers.Count <= 1)
            {
                return;
            }

            try
            {
                _browserSync.Wait();

                var tab = _currentTab + 1;
                if (tab >= _browsers.Count)
                {
                    tab = 0;
                }

                SetTab(tab);
            }
            finally
            {
                _browserSync.Release();
            }
        }

        internal void PreviousTab()
        {
            if (_browsers.Count <= 1)
            {
                return;
            }

            try
            {
                _browserSync.Wait();

                var tab = _currentTab - 1;
                if (tab < 0)
                    tab = _browsers.Count - 1;

                SetTab(tab);
            }
            finally
            {
                _browserSync.Release();
            }
        }

        internal void RefreshCurrentTab()
        {
            try
            {
                var currentBrowser = _browsers.ElementAt(_currentTab).Value;
                currentBrowser.Reload(true);
            }
            catch(Exception) { }
        }

        internal void SetTab(int id)
        {
            if (_browsers.Count <= 1)
            {
                return;
            }

            if (_currentTab == id)
            {
                return;
            }

            if (id < 0 || id >= _browsers.Count)
            {
                return;
            }

            _form.Dispatcher.Invoke(delegate ()
            {
                var currentBrowser = _browsers.ElementAt(_currentTab);
                currentBrowser.Value.Visibility = Visibility.Hidden;

                var newBrowser = _browsers.ElementAt(id).Value;
                newBrowser.Visibility = Visibility.Visible;
                newBrowser.InvalidateVisual();
                _currentTab = id;
            });
        }

        private void OnBrowserLoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            var browser = (ChromiumWebBrowser)sender;
            if (!e.IsLoading)
            {
                return;
            }

            //This comes from a different thread so we need to switch to the main thread to work with the browser element
            _form.Dispatcher.Invoke(delegate ()
            {
                try
                {
                    _browserSync.Wait();

                    var id = Convert.ToInt32(browser.Uid);
                    if (!_browserSettings.ContainsKey(id))
                    {
                        return;
                    }

                    var settings = _browserSettings[id];

                    if (settings.RequiresLogin && !settings.Authentificated)
                    {
                        settings.Authentificated = true;
                        Task.Factory.StartNew(async delegate
                        {
                            if (settings.Timeout > 0)
                                await Task.Delay(settings.Timeout * 1000);

                            var usernameSelector = settings.UsernameElement.StartsWith("#")
                                ? $"var loginInput = document.getElementById(\"{settings.UsernameElement.Replace("#", "")}\");"
                                : $"var loginInput = (document.getElementsByClassName(\"{settings.UsernameElement.Replace(".", "")}\"))[0];";

                            var passwordSelector = settings.PasswordElement.StartsWith("#")
                                ? $"var passwordInput = document.getElementById(\"{settings.PasswordElement.Replace("#", "")}\");"
                                : $"var passwordInput = (document.getElementsByClassName(\"{settings.PasswordElement.Replace(".", "")}\"))[0];";

                            var submitSelector = settings.SubmitElement.StartsWith("#")
                                ? $"var submitButton = document.getElementById(\"{settings.SubmitElement.Replace("#", "")}\");"
                                : $"var submitButton = (document.getElementsByClassName(\"{settings.SubmitElement.Replace(".", "")}\"))[0];";

                            var script = usernameSelector;
                            script += passwordSelector;
                            script += submitSelector;
                            script += $"loginInput.value = \"{settings.Username}\";";
                            script += $"passwordInput.value = \"{settings.Password}\";";
                            script += $"submitButton.click();";

                            await browser.EvaluateScriptAsync(script);
                        });
                    }

                    //Display the first url by default
                    if (id == 0)
                    {
                        browser.Visibility = Visibility.Visible;
                    }

                    if (settings.CloseAfter > 0)
                    {
                        Task.Factory.StartNew(async delegate
                        {
                            await Task.Delay(settings.CloseAfter * 1000);

                            try
                            {
                                await _browserSync.WaitAsync();

                                if (_currentTab == id)
                                {
                                    NextTab();
                                }

                                _browserCounter--;
                                _browsers.Remove(id);
                                _browserSettings.Remove(id);
                                _form.RootGrid.Children.Remove(browser);
                                browser.Dispose();
                            }
                            finally
                            {
                                _browserSync.Release();
                            }
                        });
                    }
                }
                finally
                {
                    _browserSync.Release();
                }
            });
        }

        private async void OnBrowserInitialized(object sender, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                await _browserSync.WaitAsync();

                var browser = (ChromiumWebBrowser)sender;
                var id = Convert.ToInt32(browser.Uid);
                var settings = _browserSettings[id];

                if (settings.Delay > 0)
                {
                    await Task.Delay(settings.Delay * 1000);
                }

                browser.Load(settings.Url);
            }
            finally
            {
                _browserSync.Release();
            }
        }

        public void Dispose()
        {
            foreach(var browser in _browsers.Values)
            {
                browser.Dispose();
            }

            _browsers.Clear();
        }
    }
}
