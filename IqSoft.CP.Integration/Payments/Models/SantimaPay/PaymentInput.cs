using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.SantimaPay
{
	public class PaymentInput
	{
		[JsonProperty(PropertyName = "id")]
		public long Id { get; set; }

		[JsonProperty(PropertyName = "reason")]
		public string Reason { get; set; }

		[JsonProperty(PropertyName = "merchantId")]
		public string MerchantId { get; set; }

		[JsonProperty(PropertyName = "signedToken")]
		public string SignedToken { get; set; }

		[JsonProperty(PropertyName = "successRedirectUrl")]
		public string SuccessRedirectUrl { get; set; }

		[JsonProperty(PropertyName = "failureRedirectUrl")]
		public string FailureRedirectUrl { get; set; }

		[JsonProperty(PropertyName = "notifyUrl")]
		public string NotifyUrl { get; set; }
	}
}
