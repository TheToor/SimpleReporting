using System;
using System.Linq;
using System.Windows;

namespace Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        internal static bool StartFullScreen = false;
        internal static bool DisableFullScreenExit = false;
        internal static bool DisableAPI = false;

        protected override void OnStartup(StartupEventArgs e)
        {
            var arguments = e.Args;

            if(arguments.Contains("--help") || arguments.Contains("-h"))
            {
                MessageBox.Show("Command Line Arguments:\n--help or -h\n--fullscreen or -f \n--disable-fullscreen-exit or -dfe\n--no-api or -na");
                Environment.Exit(0);
            }

            if (arguments.Contains("--fullscreen") || arguments.Contains("-f"))
                StartFullScreen = true;

            if (arguments.Contains("--disable-fullscreen-exit") || arguments.Contains("-dfe"))
                DisableFullScreenExit = true;

            if (arguments.Contains("--no-api") || arguments.Contains("-na"))
                DisableAPI = true;
        }
    }
}
