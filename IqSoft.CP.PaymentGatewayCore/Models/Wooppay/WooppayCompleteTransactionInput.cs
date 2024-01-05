using System.Xml.Serialization;

namespace IqSoft.CP.PaymentGateway.Models.Wooppay
{
    [XmlType("request")]
    public class WooppayCompleteTransactionInput : WooppayTransactionInput
    {             
        [XmlElement("transactionSuccess")]
        public bool TransactionSuccess { get; set; }
    }
}

