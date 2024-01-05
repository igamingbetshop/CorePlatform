using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;

namespace IqSoft.NGGP.WebApplications.PaymentGateway.Models.WooPay
{
    public class LoginInput
    {
        [XmlElement("username")]   
        public string UserName { get; set; }

        [XmlElement("password")] 
        public string Password { get; set; }
    }
}