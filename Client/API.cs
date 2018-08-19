using Reporting.Client.Models;
using Nancy.Hosting.Self;
using System;

namespace Reporting.Client
{
    internal class API : IDisposable
    {
        internal static string BindAddress;

        private NancyHost _host;

        internal API(Settings settings)
        {
            BindAddress = $"{settings.BindHost.Replace("http://", "").Replace("www.", "")}:{settings.BindPort}";
            var uri = new Uri($"http://{BindAddress}/");

            _host = new NancyHost(uri);
            _host.Start();
        }

        public void Dispose()
        {
            _host?.Dispose();
        }
    }
}
