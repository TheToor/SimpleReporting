using System.Collections.Generic;

namespace Reporting.Extension.PRTG.Models
{
    public class Alarm1Response
    {
        public int Errors { get; set; }
        public int Warnings { get; set; }
        public int OK { get; set; }
        public List<sensorItem> Sensors { get; set; }
    }
}
