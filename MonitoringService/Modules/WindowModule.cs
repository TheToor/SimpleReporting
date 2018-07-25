using MonitoringService.Models;
using Nancy;

namespace MonitoringService.Modules
{
    public class WindowModule : NancyModule
    {
        public WindowModule() : base("/window")
        {
            Get["enableFullscreen"] = _ =>
            {
                Program.Service.BroadCastRequest(ReportingClient.GetEnableFullscreenUrl());
                return 200;
            };

            Get["enableFullscreen/{client:int}"] = parameters =>
            {
                var client = (int)parameters.client;
                Program.Service.Request(client, ReportingClient.GetEnableFullscreenUrl());
                return 200;
            };

            Get["disableFullscreen"] = _ =>
            {
                Program.Service.BroadCastRequest(ReportingClient.GetDisableFullscreenUrl());
                return 200;
            };

            Get["disableFullscreen/{client:int}"] = parameters =>
            {
                var client = (int)parameters.client;
                Program.Service.Request(client, ReportingClient.GetDisableFullscreenUrl());
                return 200;
            };

            Get["toggleFullscreen"] = _ =>
            {
                Program.Service.BroadCastRequest(ReportingClient.GetToggleFullscreenUrl());
                return 200;
            };

            Get["toggleFullscreen/{client:int}"] = parameters =>
            {
                var client = (int)parameters.client;
                Program.Service.Request(client, ReportingClient.GetToggleFullscreenUrl());
                return 200;
            };
        }
    }
}
