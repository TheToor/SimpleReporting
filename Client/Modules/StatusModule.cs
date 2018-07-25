using Nancy;
using System;
using System.Reflection;

namespace Client.Modules
{
    public class StatusModule : NancyModule
    {
        internal static DateTime LastUpdate = DateTime.MinValue;

        public StatusModule() : base("/status")
        {
            Get["/"] = _ =>
            {
                LastUpdate = DateTime.Now;
                return 200;
            };

            Get["/version"] = _ =>
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
