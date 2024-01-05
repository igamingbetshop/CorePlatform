using System.Xml.Serialization;

namespace IqSoft.CP.Integration.Payments.Models.PayBox
{
    public class WithdrawRequestOutput : BaseOutput
    {
        [XmlElement("pg_merchant_id")]
        public string pg_merchant_id { get; set; }
    }
}
