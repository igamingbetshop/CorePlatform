using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.Stripe
{
	public class PaymentInput
	{
		[JsonProperty(PropertyName = "id")]
		public string Id { get; set; }

		[JsonProperty(PropertyName = "object")]
		public string Object { get; set; }

		[JsonProperty(PropertyName = "api_version")]
		public string ApiVersion { get; set; }

		[JsonProperty(PropertyName = "created")]
		public int Created { get; set; }

		[JsonProperty(PropertyName = "data")]
		public Data Data { get; set; }

		[JsonProperty(PropertyName = "type")]
		public string Type { get; set; }
	}

	public class Data
	{
		[JsonProperty(PropertyName = "object")]
		public Object Object { get; set; }
	}

	public class Object
	{
		[JsonProperty(PropertyName = "Id")]
		public string Id { get; set; }

		[JsonProperty(PropertyName = "payment_intent")]
		public string PaymentIntent { get; set; }

		[JsonProperty(PropertyName = "metadata")]
		public Metadata Metadata { get; set; }
	}

	public class Metadata
	{
		[JsonProperty(PropertyName = "payment_request_id")]
		public string PaymentRequestId { get; set; }

	}
}