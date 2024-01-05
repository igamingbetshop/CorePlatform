using Newtonsoft.Json;
using System;

namespace IqSoft.CP.PaymentGateway.Models.Ngine
{
    public class Transfer
    {
        [JsonProperty(PropertyName = "CustPIN")]
        public string CustPIN { get; set; }

        [JsonProperty(PropertyName = "CustPassword")]
        public string CustPassword { get; set; }

        [JsonProperty(PropertyName = "Amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "TransType")]
        public string TransType { get; set; }

        [JsonProperty(PropertyName = "ProcessorName")]
        public string ProcessorName { get; set; }

        [JsonProperty(PropertyName = "TransactionID")]
        public int TransactionID { get; set; }

        [JsonProperty(PropertyName = "TransDate")]
        public DateTime TransDate { get; set; }

        [JsonProperty(PropertyName = "TransNote")]
        public string TransNote { get; set; }

        [JsonProperty(PropertyName = "CardType")]
        public string CardType { get; set; }

        [JsonProperty(PropertyName = "CardNumber")]
        public string CardNumber { get; set; }

        [JsonProperty(PropertyName = "Descriptor")]
        public string Descriptor { get; set; }

        [JsonProperty(PropertyName = "IPAddress")]
        public string IPAddress { get; set; }

        [JsonProperty(PropertyName = "BonusAccepted")]
        public string BonusAccepted { get; set; }

        [JsonProperty(PropertyName = "Bonus")]
        public string Bonus { get; set; }

        [JsonProperty(PropertyName = "CurrencyCode")]
        public string CurrencyCode { get; set; }

        [JsonProperty(PropertyName = "ErrorCode")]
        public string ErrorCode { get; set; }

        [JsonProperty(PropertyName = "ErrorDescription")]
        public string ErrorDescription { get; set; }
    }

    public class TransferInput
    {
        [JsonProperty(PropertyName = "transfer")]
        public Transfer Transfer { get; set; }
    }
}