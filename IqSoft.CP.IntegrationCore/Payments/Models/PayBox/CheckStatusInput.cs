using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace IqSoft.CP.Integration.Payments.Models.PayBox
{
    public class CheckStatusInput
    {
        [XmlElement("pg_merchant_id")]
        public string pg_merchant_id { get; set; }

        [XmlElement("pg_payment_id")]
        public string pg_payment_id { get; set; }

        [XmlElement("pg_salt")]
        public string pg_salt { get; set; }

        [XmlElement("pg_sig")]
        public string pg_sig { get; set; }
    }
}
