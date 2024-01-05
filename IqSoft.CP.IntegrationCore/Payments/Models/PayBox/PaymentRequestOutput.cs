using System.Xml.Serialization;

namespace IqSoft.CP.Integration.Payments.Models.PayBox
{
    [XmlType("response")]
    public class PaymentRequestOutput : BaseOutput
    {
        [XmlElement("pg_redirect_url_type")]
        public string pg_redirect_url_type { get; set; }
    }
}
