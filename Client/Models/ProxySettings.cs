using System;
using System.Xml.Serialization;

namespace Client.Models
{
    [Serializable()]
    public class ProxySettings
    {
        [XmlElement("ProxyUrl")]
        public string ProxyUrl { get; set; }
        [XmlElement("ProxyPort")]
        public int ProxyPort { get; set; }
        [XmlElement("ProxyUser")]
        public string ProxyUser { get; set; }
        [XmlElement("ProxyPassword")]
        public string ProxyPassword { get; set; }
    }
}
