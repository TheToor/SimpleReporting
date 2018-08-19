using Reporting.Extension.PRTG.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Xml.Serialization;

namespace Reporting.Extension.PRTG.Services
{
    internal class PRTGService
    {
        internal static int SensorsStatusDown;
        internal static int SensorsStatusWarning;
        internal static int SensorsStatusUp;
        internal static readonly List<sensorItem> Sensors = new List<sensorItem>();


        private string _prtgBaseUrl;

        private string _prtgUser;
        private string _prtgPasswordHash;

        private string _apiUrl
        {
            get
            {
                return $"{_prtgBaseUrl}/api";
            }
        }

        //Unknown=1, Collecting=2, Up=3, Warning=4, Down=5, NoProbe=6, PausedbyUser=7, PausedbyDependency=8, PausedbySchedule=9, Unusual=10, PausedbyLicense=11, PausedUntil=12, DownAcknowledged=13, DownPartial=14
        private string SensorStatusErrorUrl
        {
            get
            {
                return $"{_apiUrl}/table.xml?content=sensor&username={_prtgUser}&passhash={_prtgPasswordHash}&filter_status=5";
            }
        }

        private string SensorsStatusWarningUrl
        {
            get
            {
                return $"{_apiUrl}/table.xml?content=sensor&username={_prtgUser}&passhash={_prtgPasswordHash}&filter_status=4";
            }
        }

        private string SensorsStatusOKUrl
        {
            get
            {
                return $"{_apiUrl}/table.xml?content=sensor&username={_prtgUser}&passhash={_prtgPasswordHash}&filter_status=3";
            }
        }

        internal PRTGService(string baseUrl, string user, string passwordHash)
        {
            _prtgBaseUrl = baseUrl;
            _prtgUser = user;
            _prtgPasswordHash = passwordHash;

            GetData();
        }

        private void GetData()
        {
            Sensors.Clear();

            GetSensorsStatusDown();
            GetSensorsStatusWarning();
            GetSensorsStatusUp();
        }

        private T DownloadXML<T>(string url)
        {
            try
            {
#if DEBUG
                Console.WriteLine($"Downloading: {url}");
#endif
                using (var webClient = new WebClient())
                {
                    var data = webClient.DownloadString(url);
                    var document = ParseXml<T>(data);
                    return document;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                return default(T);
            }
        }

        private T ParseXml<T>(string data)
        {
            var serializer = new XmlSerializer(typeof(T));
            using (var reader = new StringReader(data))
            {
                return (T)(serializer.Deserialize(reader));
            }
        }

        private void GetSensorsStatusDown()
        {
            var sensors = DownloadXML<sensor>(SensorStatusErrorUrl);
            if (sensors == null)
                return;
            SensorsStatusDown = sensors.totalcount;
            if(sensors.item != null)
                Sensors.AddRange(sensors.item);
        }

        private void GetSensorsStatusWarning()
        {
            var sensors = DownloadXML<sensor>(SensorsStatusWarningUrl);
            if (sensors == null)
                return;
            SensorsStatusWarning = sensors.totalcount;
            if (sensors.item != null)
                Sensors.AddRange(sensors.item);
        }

        private void GetSensorsStatusUp()
        {
            var sensors = DownloadXML<sensor>(SensorsStatusOKUrl);
            if (sensors == null)
                return;
            SensorsStatusUp = sensors.totalcount;
        }
    }
}
