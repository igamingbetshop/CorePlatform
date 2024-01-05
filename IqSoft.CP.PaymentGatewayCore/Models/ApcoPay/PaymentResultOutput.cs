using System.Xml.Serialization;

namespace IqSoft.CP.PaymentGateway.Models.ApcoPay
{
    public class PaymentResultOutput
    {
        public TransactionOutput Transaction { get; set; }
    }

    [XmlRoot(ElementName = "Transaction")]
    public class TransactionOutput
    {
        public string ORef { get; set; }

        public string Result { get; set; }
    }
}