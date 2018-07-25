using Nancy;

namespace Client.Modules
{
    public class WindowModule : NancyModule
    {
        public WindowModule() : base("/window")
        {
            Get["enableFullscreen"] = _ =>
            {
                MainWindow.Client.EnableFullScreen();
                return 200;
            };

            Get["disableFullscreen"] = _ =>
            {
                MainWindow.Client.DisableFullScreen();
                return 200;
            };

            Get["toggleFullscreen"] = _ =>
            {
                MainWindow.Client.ToggleFullScreen();
                return 200;
            };
        }
    }
}
