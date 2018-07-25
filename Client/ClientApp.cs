using Client.Models;
using Client.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Timers;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;

namespace Client
{
    internal class ClientApp : IDisposable
    {
        private Settings _settings;

        private BrowserWrapper _browserWrapper;
        private API _api;

        private Timer _pageSwitchTimer;

        private MainWindow _form;

        private AnnounceService _announceService;

        internal void EnableFullScreen() => _form.EnableFullScreen();
        internal void DisableFullScreen() => _form.DisableFullScreen();
        internal void ToggleFullScreen() => _form.ToggleFullScreen();

        internal ClientApp(MainWindow mainWindow)
        {
            _form = mainWindow;

            ReadSettings();

            _browserWrapper = new BrowserWrapper(mainWindow, _settings);
            _api = new API(_settings);

            if (_settings.TopMost)
                mainWindow.Topmost = true;

            if (_settings.PageSwitchTime > 0)
            {
                CreatePageSwitchTimer();
                _pageSwitchTimer.Start();
            }

            LoadTabs();

            if (!String.IsNullOrEmpty(_settings.AnnounceUrl))
                _announceService = new AnnounceService(_settings);
        }

        private void PageSwitchTimerElapsed(object sender, ElapsedEventArgs e)
        {
            _browserWrapper.NextTab();
        }

        private void LoadTabs()
        {
            foreach(var url in _settings.Urls)
            {
                _browserWrapper.CreateTab(url);
            }
        }

        private void ReadSettings()
        {
            if(!File.Exists("settings.xml"))
            {
                _settings = new Settings()
                {
                    Proxy = new ProxySettings()
                    {
                        ProxyPassword = "",
                        ProxyPort = 0,
                        ProxyUrl = "",
                        ProxyUser = ""
                    },
                    Urls = new List<UrlSettings>()
                    {
                        new UrlSettings()
                        {
                            Url = "www.example.com"
                        }
                    }
                };

                SaveSettings();
                MessageBox.Show("Please fill out the settings.xml");
                Environment.Exit(0);
                return;
            }

            var serializer = new XmlSerializer(typeof(Settings));

            using (var reader = new StreamReader("settings.xml"))
            {
                _settings = (Settings)serializer.Deserialize(reader);
            }

            if (_settings.UniqueId == -1)
            {
                MessageBox.Show("UniqueID cannot be -1!");
                Environment.Exit(0);
                return;
            }
        }

        private void SaveSettings()
        {
            var serializer = new XmlSerializer(typeof(Settings));
            var lines = "";

            using (var stringWriter = new StringWriter())
            {
                using (var writer = XmlWriter.Create(stringWriter, new XmlWriterSettings() { Indent = true }))
                {
                    serializer.Serialize(writer, _settings);
                    lines = stringWriter.ToString();
                }
            }

            File.WriteAllText("settings.xml", lines);
        }

        internal BrowserWrapper GetBrowserWrapper() => _browserWrapper;

        internal void CreatePageSwitchTimer()
        {
            _pageSwitchTimer = new Timer(_settings.PageSwitchTime * 1000);
            _pageSwitchTimer.Elapsed += PageSwitchTimerElapsed;
            _pageSwitchTimer.AutoReset = true;
        }

        internal void ToggleTimer()
        {
            if (_pageSwitchTimer == null)
                return;

            _pageSwitchTimer.Enabled = !_pageSwitchTimer.Enabled;
        }

        public void Dispose()
        {
            _browserWrapper?.Dispose();
            _pageSwitchTimer?.Dispose();
        }
    }
}
