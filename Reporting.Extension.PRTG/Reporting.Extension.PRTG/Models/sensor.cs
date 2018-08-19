using Newtonsoft.Json;

namespace Reporting.Extension.PRTG.Models
{

    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class sensor
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("prtg-version")]
        public string prtgversion { get; set; }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("item")]
        public sensorItem[] item { get; set; }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int totalcount { get; set; }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int listend { get; set; }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class sensorItem
    {

        [JsonIgnore]
        /// <remarks/>
        public string group { get; set; }

        [JsonIgnore]
        /// <remarks/>
        public string device { get; set; }

        [JsonProperty("N")]
        /// <remarks/>
        public string sensor { get; set; }

        [JsonProperty("S")]
        /// <remarks/>
        public string status { get; set; }

        [JsonIgnore]
        /// <remarks/>
        public byte status_raw { get; set; }
    }
}
