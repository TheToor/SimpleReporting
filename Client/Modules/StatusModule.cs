using Nancy;
using System.Reflection;

namespace Client.Modules
{
    public class StatusModule : NancyModule
    {
        public StatusModule() : base("/status")
        {
            Get["/"] = _ =>
            {
                return 200;
            };

            Get["/info"] = _ =>
            {
                return
                    Assembly
                    .GetExecutingAssembly()
                    .GetName()
                    .Version
                    .ToString();
            };
        }
    }
}
