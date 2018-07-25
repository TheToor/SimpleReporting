using Client.Models;
using Client.Modules;
using System;
using System.Net.Http;
using System.Timers;

namespace Client.Services
{
    internal class AnnounceService
    {
        private Settings _settings;

        private Timer _announceRetryTimer;
        private Timer _isAliveTimer;

        internal AnnounceService(Settings settings)
        {
            _settings = settings;

            TryAnnounce();
        }

        private async void TryAnnounce()
        {
            try
            {
                var url = $"{_settings.AnnounceUrl}announce/{_settings.UniqueId}/{API.BindAddress}";
                var client = new HttpClient();
                await client.GetStringAsync(url);

                if (_announceRetryTimer != null)
                {
                    _announceRetryTimer.Dispose();
                    _announceRetryTimer = null;
                }

#if !DEBUG
                _isAliveTimer = new Timer(1000 * 60 * 5);
#else
                _isAliveTimer = new Timer(1000 * 5);
#endif
                _isAliveTimer.Elapsed += IsAliveTimerElapsed;
                _isAliveTimer.AutoReset = true;
                _isAliveTimer.Start();
            }
            catch (Exception)
            {
                if (_announceRetryTimer != null)
                    _announceRetryTimer.Dispose();
#if !DEBUG
                _announceRetryTimer = new Timer(1000 * 60 * 5);
#else
                _announceRetryTimer = new Timer(1000 * 5);
#endif
                _announceRetryTimer.Elapsed += delegate (object sender, ElapsedEventArgs e)
                {
                    TryAnnounce();
                };
                _announceRetryTimer.Start();
            }
        }

        private void IsAliveTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (StatusModule.LastUpdate == DateTime.MinValue)
                return;

            if(StatusModule.LastUpdate.AddMinutes(15) < DateTime.Now)
            {
                // Received no alive check from the monitoring service within 15 minutes
                // Lets assume the service is gone
                _isAliveTimer.Dispose();
                _isAliveTimer = null;

                TryAnnounce();
            }
        }
    }
}
