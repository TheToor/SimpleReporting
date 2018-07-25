using System;
using System.Xml.Serialization;

namespace Client.Models
{
    [Serializable()]
    public class UrlSettings
    {
        [XmlElement("Url")]
        public string Url { get; set; }

        [XmlElement("RequiresLogin")]
        public bool RequiresLogin { get; set; }
        [XmlElement("Username")]
        public string Username { get; set; }
        [XmlElement("Password")]
        public string Password { get; set; }
        [XmlElement("UsernameElement")]
        public string UsernameElement { get; set; }
        [XmlElement("PasswordElement")]
        public string PasswordElement { get; set; }
        [XmlElement("SubmitElement")]
        public string SubmitElement { get; set; }
        [XmlElement("Timeout")]
        public int Timeout { get; set; }

        internal bool Authentificated { get; set; }
    }
}
