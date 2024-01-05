using System;
using System.Xml.Serialization;

namespace IqSoft.CP.PaymentGateway.Models.ApcoPay
{
    [Serializable]
    [XmlRoot(ElementName = "Transaction")]
    public class Transaction
    {
        [XmlAttribute(AttributeName = "hash")]
        public string Hash { get; set; }

        public int ORef { get; set; }

        public string Result { get; set; }

        public string AuthCode { get; set; }

        public string CardInput { get; set; }

        public string pspid { get; set; }

        public string Status3DS { get; set; }

        public string Currency { get; set; }

        public string Value { get; set; }

        public CardDetails ExtendedData { get; set; }

        public string UDF1 { get; set; }

        public string UDF2 { get; set; }

        public string UDF3 { get; set; }
    }

    public class CardDetails
    {
        public string CardNum { get; set; }

        public string CardExpiry { get; set; }

        public string CardHName { get; set; }

        public string Acq { get; set; }

        public string Source { get; set; }

        public string CardCountry { get; set; }

        public string CardType { get; set; }

        public string Email { get; set; }
    }
}