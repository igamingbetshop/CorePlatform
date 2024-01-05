using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace IqSoft.NGGP.WebApplications.PaymentGateway.Models.WooPay
{
    public class BaseOutput
    {
        [XmlElement("error_code")]
        public int ErrorCode { get; set; }
    }
}