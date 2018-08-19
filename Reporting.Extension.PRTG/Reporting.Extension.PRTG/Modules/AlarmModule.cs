using Nancy;
using Newtonsoft.Json;
using Reporting.Extension.PRTG.Models;
using Reporting.Extension.PRTG.Services;
using System.Linq;

namespace Reporting.Extension.PRTG.Modules
{
    public class AlarmModule : NancyModule
    {
        public AlarmModule() : base("/alarm")
        {
            this.After.AddItemToEndOfPipeline((ctx) =>
            {
                ctx.Response
                    .WithHeader("Access-Control-Allow-Origin", "*")
                    .WithHeader("Access-Control-Allow-Methods", "GET")
                    .WithHeader("Access-Control-Allow-Headers", "Accept, Origin, Content-type");
            });

            Get["/1"] = _ =>
            {
                var copy = PRTGService.Sensors.ToList();
                var response = new Alarm1Response()
                {
                    Errors = PRTGService.SensorsStatusDown,
                    Warnings = PRTGService.SensorsStatusWarning,
                    OK = PRTGService.SensorsStatusUp,
                    Sensors = copy
                };

                return JsonConvert.SerializeObject(response);
            };
        }
    }
}
