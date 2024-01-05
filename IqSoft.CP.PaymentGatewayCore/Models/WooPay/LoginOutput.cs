using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;

namespace IqSoft.NGGP.WebApplications.PaymentGateway.Models.WooPay
{
    public class LoginOutput : BaseOutput
    {
        [XmlElement("session")]
        public string Session { get; set; }

        [XmlElement("id")]
        public int UserId { get; set; }

        [XmlElement("username")]
        public string UserName { get; set; }

        [XmlElement("type")]
        public int UserType { get; set; }

        [XmlElement("roles")]
        public string[] Roles { get; set; }

        [XmlElement("AvatarVersion")]
        public string AvatarVersion { get; set; }

        [XmlElement("avatarName")]
        public string AvatarName { get; set; }
    }
}