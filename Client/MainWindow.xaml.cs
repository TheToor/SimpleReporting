using System;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace Reporting.Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal static ClientApp Client;

        private WindowState _fullScreenWindowState;
        private Timer _helpTimer;

        public MainWindow()
        {
            InitializeComponent();

            Client = new ClientApp(this);

            if (App.StartFullScreen)
                EnableFullScreen();
        }

        private void OnWindowUnloaded(object sender, RoutedEventArgs e)
        {
            Client?.Dispose();
        }

        internal void ToggleFullScreen()
        {
            this.Dispatcher.Invoke(delegate ()
            {
                if (this.WindowStyle == WindowStyle.None)
                    DisableFullScreen(true);
                else
                    EnableFullScreen(true);
            });
        }

        internal void EnableFullScreen(bool safe = false)
        {
            if(!safe)
            {
                this.Dispatcher.Invoke(delegate ()
                {
                    this.WindowStyle = WindowStyle.None;
                    _fullScreenWindowState = this.WindowState;
                    this.WindowState = WindowState.Maximized;
                });
                return;
            }

            this.WindowStyle = WindowStyle.None;
            _fullScreenWindowState = this.WindowState;
            this.WindowState = WindowState.Maximized;
        }

        internal void DisableFullScreen(bool safe = false)
        {
            if (App.DisableFullScreenExit)
                return;

            if(!safe)
            {
                this.Dispatcher.Invoke(delegate ()
                {
                    this.WindowStyle = WindowStyle.SingleBorderWindow;
                    this.WindowState = _fullScreenWindowState;
                });
                return;
            }

            this.WindowStyle = WindowStyle.SingleBorderWindow;
            this.WindowState = _fullScreenWindowState;
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            switch(e.Key)
            {
                case Key.F11:
                    {
                        ToggleFullScreen();
                    }
                    break;

                case Key.F1:
                    {
                        Client.GetBrowserWrapper().PreviousTab();
                    }
                    break;

                case Key.F2:
                    {
                        Client.GetBrowserWrapper().NextTab();
                    }
                    break;

                case Key.F3:
                    {
                        Client.ToggleTimer();
                    }
                    break;

                case Key.F5:
                    {
                        Client.GetBrowserWrapper().RefreshCurrentTab();
                    }
                    break;

                default:
                    {
                        ShowHelpScreen();
                    }
                    break;
            }
        }

        private void ShowHelpScreen()
        {
            _helpTimer?.Dispose();

            HelpGrid.Visibility = Visibility.Visible;

            _helpTimer = new Timer(5000);
            _helpTimer.Elapsed += delegate (object sender, ElapsedEventArgs args)
            {
                this.Dispatcher.Invoke(delegate ()
                {
                    HelpGrid.Visibility = Visibility.Hidden;
                });
            };
            _helpTimer.Start();
        }
    }
}
