using Nancy;

namespace Reporting.Client.Modules
{
    public class TabModule : NancyModule
    {
        public TabModule() : base("/tab")
        {
            Get["{id:int}"] = parameters =>
            {
                MainWindow.Client.GetBrowserWrapper().SetTab(parameters.id);
                return 200;
            };

            Get["next"] = _ =>
            {
                MainWindow.Client.GetBrowserWrapper().NextTab();
                return 200;
            };

            Get["prev"] = _ =>
            {
                MainWindow.Client.GetBrowserWrapper().PreviousTab();
                return 200;
            };
        }
    }
}
