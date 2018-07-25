using MonitoringService.Models;
using Nancy;

namespace MonitoringService.Modules
{
    public class TabModule : NancyModule
    {
        public TabModule() : base("/tab")
        {
            Get["{id:int}"] = parameters =>
            {
                var tabId = (int)parameters.Id;
                Program.Service.BroadCastRequest(ReportingClient.GetTabUrl(tabId));
                return 200;
            };

            Get["{id:int}/{client:id}"] = parameters =>
            {
                var tabId = (int)parameters.id;
                var client = (int)parameters.client;
                Program.Service.Request(client, ReportingClient.GetTabUrl(tabId));
                return 200;
            };

            Get["next"] = _ =>
            {
                Program.Service.BroadCastRequest(ReportingClient.GetTabNextUrl());
                return 200;
            };
            
            Get["next/{client:int}"] = parameters =>
            {
                var client = (int)parameters.client;
                Program.Service.Request(client, ReportingClient.GetTabNextUrl());
                return 200;
            };

            Get["prev"] = _ =>
            {
                Program.Service.BroadCastRequest(ReportingClient.GetTabPreviousUrl());
                return 200;
            };

            Get["prev/{client:int}"] = parameters =>
            {
                var client = (int)parameters.client;
                Program.Service.Request(client, ReportingClient.GetTabPreviousUrl());
                return 200;
            };
        }
    }
}
