using System.Windows;
using System.Windows.Input;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal static ClientApp Client;

        private WindowState _fullScreenWindowState;

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
            }
        }
    }
}
