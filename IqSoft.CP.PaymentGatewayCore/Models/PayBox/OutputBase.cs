using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace IqSoft.CP.PaymentGateway.Models.PayBox
{
    [XmlType("response")]
    public class OutputBase
    {
        [XmlElement("pg_salt")]
        public string Salt { get; set; }

        [XmlElement("pg_status")]
        public string Status { get; set; }

        [XmlElement("pg_description")]
        public string Description { get; set; }

        [XmlElement("pg_error_description")]
        public string ErrorDescription { get; set; }

        [XmlElement("pg_sig")]
        public string Signature { get; set; }

    }
}