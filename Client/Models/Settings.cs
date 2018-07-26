using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Client.Models
{
    [Serializable()]
    [XmlRoot("Settings")]
    public class Settings
    {
        [XmlElement("UniqueId")]
        public int UniqueId { get; set; } = -1;
        [XmlElement("AnnounceUrl")]
        public string AnnounceUrl { get; set; }
        [XmlElement("BindHost")]
        public string BindHost { get; set; }
        [XmlElement("BindPort")]
        public int BindPort { get; set; }

        [XmlElement("PageSwitchTime")]
        public int PageSwitchTime { get; set; }
        [XmlElement("TopMost")]
        public bool TopMost { get; set; }
        [XmlElement("DisableScreenSaver")]
        public bool DisableScreenSaver { get; set; } = true;

        [XmlElement("Proxy")]
        public ProxySettings Proxy { get; set; }

        [XmlArray("Urls")]
        [XmlArrayItem("Site", typeof(UrlSettings))]
        public List<UrlSettings> Urls { get; set; }
    }
}
