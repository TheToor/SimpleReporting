using Nancy;

namespace MonitoringService.Modules
{
    public class AnnounceModule : NancyModule
    {
        public AnnounceModule() : base("/announce")
        {
            Get["/{id:int}/{url}"] = parameters =>
            {
                var id = (int)parameters.id;
                var callbackUrl = (string)parameters.url;

                Program.Service.AddClient(id, callbackUrl);

                return 200;
            };
        }
    }
}
