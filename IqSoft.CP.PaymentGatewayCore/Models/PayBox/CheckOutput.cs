using System.Xml.Serialization;

namespace IqSoft.CP.PaymentGateway.Models.PayBox
{
    public class CheckOutput : OutputBase
    {
        [XmlElement("pg_timeout")]
        public int Timeout { get; set; }

        [XmlElement("pg_error_code")]
        public int ErrorCode { get; set; }
    }
}