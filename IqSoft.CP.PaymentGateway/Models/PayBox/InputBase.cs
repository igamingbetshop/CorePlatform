using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace IqSoft.CP.PaymentGateway.Models.PayBox
{
    [XmlType("request")]
    public class InputBase
    {
        [XmlElement("pg_salt")]
        public string Salt { get; set; }

        [XmlElement("pg_order_id")]
        public long TransactionId { get; set; }

        [XmlElement("pg_payment_id")]
        public string ExternalTransactionId { get; set; }

        [XmlElement("userid")]
        public string UserId { get; set; }

        [XmlElement("pg_sig")]
        public string Signature { get; set; }
    }
}