using MonitoringService.Models;
using Nancy.Hosting.Self;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MonitoringService
{
    internal class MonitoringService : IDisposable
    {
        private NancyHost _host;
        private Dictionary<int, ReportingClient> _clients = new Dictionary<int, ReportingClient>();

        internal List<ReportingClient> GetClients() => _clients.Select(c => c.Value).ToList();

        private Thread _threadLoop;

        internal MonitoringService()
        {
            try
            {
                _host = new NancyHost(new Uri("http://localhost:9001"));
                _host.Start();

                _threadLoop = new Thread(ThreadLoop);
                _threadLoop.Start();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void ThreadLoop()
        {
            try
            {
                while (true)
                {
#if !DEBUG
                    Thread.Sleep(1000 * 60 * 5);
#else
                    Thread.Sleep(1000 * 30);
#endif
                    if (_clients.Count == 0)
                        continue;

                    Console.WriteLine($"Updating status of {_clients.Count} servers...");

                    foreach(var client in _clients.Values.ToList())
                    {
                        try
                        {
                            var httpClient = new HttpClient();
                            httpClient.GetStringAsync(client.StatusUrl);
                            Console.WriteLine($"{client.UniqueId} responded");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to contact {client.UniqueId}");
                            Console.WriteLine(ex);

                            _clients.Remove(client.UniqueId);
                        }
                    }

                    Console.WriteLine("Finished updating servers");
                }
            }
            catch (ThreadAbortException) { }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        internal async void AddClient(int id, string url)
        {
            if(_clients.ContainsKey(id))
            {
                var client = _clients[id];
                if (client.BindAddress == url)
                    return;

                _clients.Remove(id);
            }

            var reportingClient = new ReportingClient()
            {
                BindAddress = url,
                UniqueId = id
            };

            try
            {
                var httpClient = new HttpClient();
                var version = await httpClient.GetStringAsync(reportingClient.VersionUrl);

                if (String.IsNullOrEmpty(version))
                    throw new Exception("Invalid version");

                if (!Version.TryParse(version, out var parsedVersion))
                    throw new Exception("Invalid version");

                reportingClient.Version = parsedVersion;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Failed to add [{id}]{url}");
                Console.WriteLine(ex);

                return;
            }

            _clients.Add(id, reportingClient);

            Console.WriteLine($"Added [{id}]{url} in version {reportingClient.Version} to monitoring");
        }

        internal void BroadcastRequest(string request)
        {
            foreach(var client in _clients.Values)
            {
                Request(client, request);
            }
        }

        internal void Request(int id, string request)
        {
            if (id < 0)
                return;

            if (!_clients.ContainsKey(id))
                return;

            Request(_clients[id], request);
        }

        internal void Request(ReportingClient client, string request)
        {
            try
            {
                var httpClient = new HttpClient();
                httpClient.SendAsync(new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"{client.FQDN}/{request}")
                }).ContinueWith(delegate(Task<HttpResponseMessage> response)
                {
                    httpClient.Dispose();
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send request to {client.UniqueId}");
                Console.WriteLine(ex);
            }
        }

        public void Dispose()
        {
            _host?.Dispose();
        }
    }
}
