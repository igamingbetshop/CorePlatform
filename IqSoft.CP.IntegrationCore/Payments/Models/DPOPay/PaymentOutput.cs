
namespace IqSoft.CP.Integration.Payments.Models.DPOPay
{
    [System.Xml.Serialization.XmlRootAttribute("API3G", Namespace = "", IsNullable = false)]
    public class PaymentOutput
    {
        public string Result { get; set; }
        public string ResultExplanation { get; set; }
        public string TransToken { get; set; }
        public string TransRef { get; set; }
    }
}
