using System;

namespace MonitoringService
{
    class Program
    {
        internal static MonitoringService Service;
        static void Main(string[] args)
        {
            Service = new MonitoringService();

            var exit = false;
            do
            {
                var key = Console.ReadKey();

                if (key.Key == ConsoleKey.Q)
                    exit = true;
            }
            while (!exit);

            Service.Dispose();
        }
    }
}
