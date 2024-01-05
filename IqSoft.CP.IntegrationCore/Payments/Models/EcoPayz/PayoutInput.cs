namespace IqSoft.CP.Integration.Payments.Models.EcoPayz
{
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://schemas.xmlsoap.org/soap/envelope/", IsNullable = false)]
    public partial class Envelope
    {
        public EnvelopeBody Body { get; set; }
    }

    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    public partial class EnvelopeBody
    {

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.ecocard.com/merchantAPI/")]
        public Payout Payout { get; set; }
    }

    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.ecocard.com/merchantAPI/")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.ecocard.com/merchantAPI/", IsNullable = false)]
    public partial class Payout
    {
        public PayoutPayoutRequest PayoutRequest { get; set; }
    }

    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.ecocard.com/merchantAPI/")]
    public partial class PayoutPayoutRequest
    {
        public int MerchantID { get; set; }
        public string MerchantPassword { get; set; }
        public int MerchantAccountNumber { get; set; }
        public int ClientAccountNumber { get; set; }
        public decimal Amount { get; set; }
        public string TxID { get; set; }
        public string Currency { get; set; }
        public string ClientAccountNumberAtMerchant { get; set; }
        public string TxBatchNumber { get; set; }
        public string TransactionDescription { get; set; }
    }
}
