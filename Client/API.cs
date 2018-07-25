using Nancy.Hosting.Self;
using System;

namespace Client
{
    internal class API : IDisposable
    {
        private NancyHost _host;

        internal API()
        {
            _host = new NancyHost(new Uri("http://localhost:9000/"));
            _host.Start();
        }

        public void Dispose()
        {
            _host?.Dispose();
        }
    }
}
