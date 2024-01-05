namespace IqSoft.CP.Integration.Payments.Models.DPOPay
{
    [System.Xml.Serialization.XmlRootAttribute("API3G", Namespace = "", IsNullable = false)]
    public class VerifyInput
    {
        public string CompanyToken { get; set; }
        public string Request { get; set; }
        public string TransactionToken { get; set; }
    }
}