using Reporting.Service.Models;
using Nancy;

namespace Reporting.Service.Modules
{
    public class TabModule : NancyModule
    {
        public TabModule() : base("/tab")
        {
            Get["{id:int}"] = parameters =>
            {
                var tabId = (int)parameters.Id;
                Program.Service.BroadcastRequest(ReportingClient.GetTabUrl(tabId));
                return 200;
            };

            Get["{id:int}/{client:int}"] = parameters =>
            {
                var tabId = (int)parameters.id;
                var client = (int)parameters.client;
                Program.Service.Request(client, ReportingClient.GetTabUrl(tabId));
                return 200;
            };

            Get["next"] = _ =>
            {
                Program.Service.BroadcastRequest(ReportingClient.GetTabNextUrl());
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
                Program.Service.BroadcastRequest(ReportingClient.GetTabPreviousUrl());
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
