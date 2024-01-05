using System;
using System.Xml.Serialization;

namespace IqSoft.CP.Integration.Payments.Models.PayBox
{
    [XmlType("request")]
    public class CheckBalanceOutput
    {
        [XmlElement("pg_balance")]
        public string pg_balance { get; set; }

        [XmlElement("pg_status")]
        public string pg_status { get; set; }

        [XmlElement("pg_salt")]
        public string pg_salt { get; set; }

        [XmlElement("pg_sig")]
        public string pg_sig { get; set; }
    }
}
