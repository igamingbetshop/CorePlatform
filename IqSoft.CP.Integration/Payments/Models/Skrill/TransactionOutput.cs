using System.Xml.Serialization;

namespace IqSoft.CP.Integration.Payments.Models.Skrill
{
    [XmlType("response")]
    public class TransactionOutput
    {
        [XmlElement("sid")]
        public string Sid { get; set; }

        [XmlElement("transaction")]
        public Transaction TransactionData { get; set; }

        [XmlElement("error")]
        public Error ErrorObj { get; set; }

    }

    public class Error
    {
        [XmlElement("error_msg")]
        public string ErrorMessage { get; set; }
    }

    public class Transaction
    {
        [XmlElement("amount")]
        public decimal Amount { get; set; }

        [XmlElement("currency")]
        public string Currency { get; set; }

        [XmlElement("id")]
        public string TransactionId { get; set; }

        /// <summary>
        /// Numeric value of the transaction status: 
        /// 1 – scheduled (if beneficiary is not yet registered at Skrill) 
        /// 2 ‐ processed (if beneficiary is registered)
        /// </summary>
        [XmlElement("status")]
        public int Status { get; set; }

        /// <summary>
        /// Text value of the transaction status.
        /// </summary>
        [XmlElement("status_msg")]
        public string StatusMessage { get; set; }
    }
}