using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Paylado
{
	public class PayoutOutput
	{
		[JsonProperty(PropertyName = "ResultStatus")]
		public string ResultStatus { get; set; }

		[JsonProperty(PropertyName = "ResultCode")]
		public string ResultCode { get; set; }

		[JsonProperty(PropertyName = "ResultMessage")]
		public string ResultMessage { get; set; }

		[JsonProperty(PropertyName = "TransactionId")]
		public string TransactionId { get; set; }
	}
}
