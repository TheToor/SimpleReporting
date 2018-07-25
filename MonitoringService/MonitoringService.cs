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
        private Dictionary<int, ReportingClient> clients = new Dictionary<int, ReportingClient>();

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
                    if (clients.Count == 0)
                        continue;

                    Console.WriteLine($"Updating status of {clients.Count} servers...");

                    foreach(var client in clients.Values.ToList())
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

                            clients.Remove(client.UniqueId);
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

        internal void AddClient(int id, string url)
        {
            if(clients.ContainsKey(id))
            {
                var client = clients[id];
                if (client.BindAddress == url)
                    return;

                clients.Remove(id);
            }

            clients.Add(id, new ReportingClient()
            {
                BindAddress = url,
                UniqueId = id
            });

            Console.WriteLine($"Added [{id}]{url} to monitoring");
        }

        internal void BroadCastRequest(string request)
        {
            foreach(var client in clients.Values)
            {
                Request(client, request);
            }
        }

        internal void Request(int id, string request)
        {
            if (id < 0 || id >= clients.Count)
                return;

            if (!clients.ContainsKey(id))
                return;

            Request(clients[id], request);
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
