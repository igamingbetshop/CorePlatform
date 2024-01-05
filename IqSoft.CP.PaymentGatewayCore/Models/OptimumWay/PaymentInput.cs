using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.OptimumWay
{
    public class PaymentInput
    {
        [JsonProperty(PropertyName = "result")]
        public string Result { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "adapterMessage")]
        public string AdapterMessage { get; set; }

        [JsonProperty(PropertyName = "uuid")]
        public string UUID { get; set; }

        [JsonProperty(PropertyName = "merchantTransactionId")]
        public string MerchantTransactionId { get; set; }

        [JsonProperty(PropertyName = "purchaseId")]
        public string PurchaseId { get; set; }

        [JsonProperty(PropertyName = "transactionType")]
        public string TransactionType { get; set; }

        [JsonProperty(PropertyName = "paymentMethod")]
        public string PaymentMethod { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        //[JsonProperty(PropertyName = "returnData")]
        //public Returndata returnData { get; set; }
    }


    //public class Returndata
    //{
    //    public string _TYPE { get; set; }
    //    public string type { get; set; }
    //    public string cardHolder { get; set; }
    //    public string expiryMonth { get; set; }
    //    public string expiryYear { get; set; }
    //    public string binDigits { get; set; }
    //    public string firstSixDigits { get; set; }
    //    public string lastFourDigits { get; set; }
    //    public string fingerprint { get; set; }
    //    public string threeDSecure { get; set; }
    //    public string binBrand { get; set; }
    //    public string binBank { get; set; }
    //    public string binType { get; set; }
    //    public string binLevel { get; set; }
    //    public string binCountry { get; set; }
    //}

}