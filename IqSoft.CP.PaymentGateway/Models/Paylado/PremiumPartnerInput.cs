using System;
using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.Paylado
{
	public class PremiumPartnerInput
	{
		[JsonProperty(PropertyName = "amount")]
		public int amount { get; set; }

		[JsonProperty(PropertyName = "currency")]
		public string currency { get; set; }

		[JsonProperty(PropertyName = "error")]
		public int error { get; set; }

		[JsonProperty(PropertyName = "error_text")]
		public string error_text { get; set; }

		[JsonProperty(PropertyName = "external_customer_id")]
		public string external_customer_id { get; set; }

		[JsonProperty(PropertyName = "external_reference_id")]
		public string external_reference_id { get; set; }

		[JsonProperty(PropertyName = "id")]
		public string id { get; set; }

		[JsonProperty(PropertyName = "receiver_account")]
		public string receiver_account { get; set; }

		[JsonProperty(PropertyName = "sender_account")]
		public string sender_account { get; set; }

		[JsonProperty(PropertyName = "sender_first_name")]
		public string sender_first_name { get; set; }

		[JsonProperty(PropertyName = "sender_last_name")]
		public string sender_last_name { get; set; }

		[JsonProperty(PropertyName = "status")]
		public int status { get; set; }

		[JsonProperty(PropertyName = "status_text")]
		public string status_text { get; set; }

		[JsonProperty(PropertyName = "timestamp")]
		public DateTime timestamp { get; set; }

		[JsonProperty(PropertyName = "type")]
		public string type { get; set; }
	}
}