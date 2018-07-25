using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Client.Models
{
    [Serializable()]
    [XmlRoot("Settings")]
    public class Settings
    {
        [XmlElement("PageSwitchTime")]
        public int PageSwitchTime { get; set; }
        [XmlElement("TopMost")]
        public bool TopMost { get; set; }

        [XmlElement("Proxy")]
        public ProxySettings Proxy { get; set; }

        [XmlArray("Urls")]
        [XmlArrayItem("Site", typeof(UrlSettings))]
        public List<UrlSettings> Urls { get; set; }
    }
}
