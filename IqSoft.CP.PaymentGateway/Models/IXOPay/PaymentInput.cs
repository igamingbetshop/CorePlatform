using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.IXOPay
{
    public class PaymentInput
    {
        [JsonProperty(PropertyName = "result")]
        public string Result { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "code")]
        public string Code { get; set; }
        
        [JsonProperty(PropertyName = "adapterMessage")]
        public string AdapterMessage { get; set; }

        [JsonProperty(PropertyName = "adaptercode")]
        public string AdapterCode { get; set; }

        [JsonProperty(PropertyName = "uuid")]
        public string Uuid { get; set; }

        [JsonProperty(PropertyName = "merchantTransactionId")]
        public string MerchantTransactionId { get; set; }

        [JsonProperty(PropertyName = "purchaseId")]
        public string PurchaseId { get; set; }

        [JsonProperty(PropertyName = "transactionType")]
        public string TransactionType { get; set; }

        [JsonProperty(PropertyName = "paymentMethod")]
        public string PaymentMethod { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public string Amount { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "referenceUuid")]
        public string ReferenceUuid { get; set; }

		[JsonProperty(PropertyName = "returnData")]
		public ReturnData ReturnData { get; set; }
	}

	public class ReturnData
	{
		[JsonProperty(PropertyName = "firstSixDigits")]
		public string FirstSixDigits { get; set; }

		[JsonProperty(PropertyName = "lastFourDigits")]
		public string LastFourDigits { get; set; }
	}
}