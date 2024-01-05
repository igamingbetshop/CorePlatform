using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.SantimaPay
{
	public class Transaction
	{
		[JsonProperty(PropertyName = "txnId")]
		public string TxnId { get; set; }

		[JsonProperty(PropertyName = "thirdPartyId")]
		public string ThirdPartyId { get; set; }

		[JsonProperty(PropertyName = "merId")]
		public string MerId { get; set; }

		[JsonProperty(PropertyName = "merName")]
		public string MerName { get; set; }

		[JsonProperty(PropertyName = "address")]
		public string Address { get; set; }

		[JsonProperty(PropertyName = "amount")]
		public string Amount { get; set; }

		[JsonProperty(PropertyName = "currency")]
		public string Currency { get; set; }

		[JsonProperty(PropertyName = "reason")]
		public string Reason { get; set; }

		[JsonProperty(PropertyName = "msisdn")]
		public string Msisdn { get; set; }

		[JsonProperty(PropertyName = "accountNumber")]
		public string AccountNumber { get; set; }

		[JsonProperty(PropertyName = "paymentVia")]
		public string PaymentVia { get; set; }

		[JsonProperty(PropertyName = "refId")]
		public string RefId { get; set; }

		[JsonProperty(PropertyName = "successRedirectUrl")]
		public string SuccessRedirectUrl { get; set; }

		[JsonProperty(PropertyName = "failureRedirectUrl")]
		public string FailureRedirectUrl { get; set; }

		[JsonProperty(PropertyName = "message")]
		public string Message { get; set; }

		[JsonProperty(PropertyName = "status")]
		public string Status { get; set; }

		[JsonProperty(PropertyName = "receiverWalletID")]
		public string ReceiverWalletID { get; set; }
	}
}