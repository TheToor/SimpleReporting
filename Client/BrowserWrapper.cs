using CefSharp;
using CefSharp.Wpf;
using Reporting.Client.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Reporting.Client
{
    internal class BrowserWrapper : IDisposable
    {
        private MainWindow _form;

        private object _browserLock = new object();
        private int _browserCounter;
        private int _currentTab;
        private Dictionary<int, ChromiumWebBrowser> _browsers = new Dictionary<int, ChromiumWebBrowser>();
        private Dictionary<int, UrlSettings> _browserSettings = new Dictionary<int, UrlSettings>();

        internal BrowserWrapper(MainWindow mainform, Settings settings)
        {
            _form = mainform;

            var cefSettings = new CefSettings();

            if (settings.Proxy == null || settings.Proxy.ProxyUrl == "no")
                cefSettings.CefCommandLineArgs.Add("no-proxy-server", "1");
            else if (settings.Proxy.ProxyUrl == "auto")
                cefSettings.CefCommandLineArgs.Add("proxy-auto-detect", "1");
            else if (settings.Proxy.ProxyUrl.StartsWith("pac://"))
            {
                var url = settings.Proxy.ProxyUrl.Replace("pac://", "");
                cefSettings.CefCommandLineArgs.Add("proxy-pac-url", url);
            }
            else
                cefSettings.CefCommandLineArgs.Add("proxy-server", $"{settings.Proxy.ProxyUrl}:{settings.Proxy.ProxyPort}");
            
            Cef.Initialize(cefSettings);
        }

        internal void CreateTab(UrlSettings urlsettings)
        {
            var browser = new ChromiumWebBrowser();
            _form.RootGrid.Children.Add(browser);
            browser.VerticalAlignment = VerticalAlignment.Stretch;
            browser.HorizontalAlignment = HorizontalAlignment.Stretch;
            browser.Visibility = Visibility.Hidden;

            browser.IsBrowserInitializedChanged += OnBrowserInitialized;
            browser.LoadingStateChanged += OnBrowserLoadingStateChanged;

            lock (_browserLock)
            {
                var id = _browserCounter++;
                browser.Uid = id.ToString();

                _browsers.Add(id, browser);
                _browserSettings.Add(id, urlsettings);
            }
        }

        internal void NextTab()
        {
            if (_browsers.Count <= 1)
                return;

            lock (_browserLock)
            {
                var tab = _currentTab + 1;
                if (tab >= _browsers.Count)
                    tab = 0;

                SetTab(tab);
            }
        }

        internal void PreviousTab()
        {
            if (_browsers.Count <= 1)
                return;

            lock (_browserLock)
            {
                var tab = _currentTab - 1;
                if (tab < 0)
                    tab = _browsers.Count - 1;

                SetTab(tab);
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
                return;

            if (_currentTab == id)
                return;

            if (id < 0 || id >= _browsers.Count)
                return;

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
                return;

            //This comes from a different thread so we need to switch to the main thread to work with the browser element
            _form.Dispatcher.Invoke(delegate ()
            {
                var id = Convert.ToInt32(browser.Uid);
                var settings = _browserSettings[id];

                if(settings.RequiresLogin && !settings.Authentificated)
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
            });
        }

        private async void OnBrowserInitialized(object sender, DependencyPropertyChangedEventArgs e)
        {
            var browser = (ChromiumWebBrowser)sender;
            var id = Convert.ToInt32(browser.Uid);
            var settings = _browserSettings[id];

            if (settings.Delay > 0)
                await Task.Delay(settings.Delay * 1000);

            browser.Load(settings.Url);
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
