﻿using Reporting.Client.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;

namespace Reporting.Client
{
    internal class ClientApp : IDisposable
    {
        private Settings _settings;

        private readonly BrowserWrapper _browserWrapper;

        private Timer _pageSwitchTimer;

        private readonly MainWindow _form;

        internal enum EXECUTION_STATE : uint
        {
            ES_CONTINUOUS = 0x80000000,
            ES_SYSTEM_REQUIRED = 0x00000001,
            ES_DISPLAY_REQUIRED = 0x00000002
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern uint SetThreadExecutionState([In] uint esFlags);

        internal void EnableFullScreen() => _form.EnableFullScreen();
        internal void DisableFullScreen() => _form.DisableFullScreen();
        internal void ToggleFullScreen() => _form.ToggleFullScreen();

        internal ClientApp(MainWindow mainWindow)
        {
            _form = mainWindow;

            ReadSettings();

            _browserWrapper = new BrowserWrapper(mainWindow, _settings);

            if (_settings.TopMost)
            {
                mainWindow.Topmost = true;
            }

            if (_settings.PageSwitchTime > 0)
            {
                CreatePageSwitchTimer();
                _pageSwitchTimer.Start();
            }

            if (_settings.DisableScreenSaver)
            {
                SetThreadExecutionState((uint)(EXECUTION_STATE.ES_CONTINUOUS | EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_SYSTEM_REQUIRED));
            }

            LoadTabs();
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
                        ProxyUrl = "auto",
                        ProxyUser = ""
                    },
                    Urls = new List<UrlSettings>()
                    {
                        new UrlSettings()
                        {
                            Url = "www.example.com"
                        }
                    },
                    BindHost = "localhost",
                    BindPort = 9000,
                    DisableScreenSaver = true,
                    PageSwitchTime = 30,
                    TopMost = true,
                    UniqueId = new Random().Next(1, int.MaxValue)
                };

                SaveSettings();
                MessageBox.Show("Please fill out the settings.xml");
                Environment.Exit(0);
                return;
            }

            var serializer = new XmlSerializer(typeof(Settings));

            try
            {
                using (var reader = new StreamReader("settings.xml"))
                {
                    _settings = (Settings)serializer.Deserialize(reader);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Error while reading settings: {ex}");
                Environment.Exit(-1);
            }

            if (_settings.UniqueId == -1)
            {
                MessageBox.Show("UniqueID cannot be -1!");
                Environment.Exit(0);
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
