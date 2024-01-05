using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.SantimaPay
{
	public class PayoutOutput
	{
		[JsonProperty(PropertyName = "txnId")]
		public string TxnId { get; set; }

		[JsonProperty(PropertyName = "reason")]
		public string Reason { get; set; }

		[JsonProperty(PropertyName = "amount")]
		public string Amount { get; set; }

		[JsonProperty(PropertyName = "merchantId")]
		public string MerchantId { get; set; }

		[JsonProperty(PropertyName = "thirdPartyId")]
		public string ThirdPartyId { get; set; }

		[JsonProperty(PropertyName = "merId")]
		public string MerId { get; set; }

		[JsonProperty(PropertyName = "merName")]
		public string MerName { get; set; }

		[JsonProperty(PropertyName = "address")]
		public string Address { get; set; }

		[JsonProperty(PropertyName = "currency")]
		public string Currency { get; set; }

		[JsonProperty(PropertyName = "reciverPhoneNumber")]
		public string ReciverPhoneNumber { get; set; }

		[JsonProperty(PropertyName = "reciverAccountNumber")]
		public string ReciverAccountNumber { get; set; }

		[JsonProperty(PropertyName = "paymentVia")]
		public string PaymentVia { get; set; }

		[JsonProperty(PropertyName = "clientReference")]
		public string ClientReference { get; set; }

		[JsonProperty(PropertyName = "message")]
		public string Message { get; set; }

		[JsonProperty(PropertyName = "status")]
		public string Status { get; set; }
	}
}
