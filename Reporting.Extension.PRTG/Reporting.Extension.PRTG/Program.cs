using Nancy.Hosting.Self;
using Reporting.Extension.PRTG.Services;
using System;

namespace Reporting.Extension.PRTG
{
    class Program
    {
        private static PRTGService _prtgService;
        private static NancyHost _host;

        private static string BaseUrl = "http://localhost:9000";

        static void Main(string[] args)
        {
            _prtgService = new PRTGService("https://prtg.paessler.com", "demo", "1746949339");
            _host = new NancyHost(new Uri(BaseUrl));

            try
            {
                _host.Start();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                Console.ReadKey();
                Environment.Exit(-1);
            }

            Console.WriteLine($"Started on {BaseUrl}");

#if DEBUG
            Console.WriteLine($"Down: {PRTGService.SensorsStatusDown}");
            Console.WriteLine($"Warning: {PRTGService.SensorsStatusWarning}");
            Console.WriteLine($"Up: {PRTGService.SensorsStatusUp}");

            foreach (var sensor in PRTGService.Sensors)
                Console.WriteLine($"{sensor.sensor}: {sensor.status}");
#endif

            while (Console.ReadKey().Key != ConsoleKey.Q)
            {
                //Wait for quit   
            }

            Console.WriteLine("Closing");
            _host.Dispose();
        }
    }
}
