using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Payments.Models.Interkassa
{
    public class PayoutOutput
    {
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "code")]
        public int Code { get; set; }

        [JsonProperty(PropertyName = "data")]
        public object Data { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }
    }

    public class Data
    {
        [JsonProperty(PropertyName = "transaction")]
        public Transaction Transaction { get; set; }
    }

    public class Transaction
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "opId")]
        public string OpId { get; set; }
    }

	public class PayoutResult
	{
		[JsonProperty(PropertyName = "id")]
		public string id { get; set; }

		[JsonProperty(PropertyName = "psTrnId")]
		public string psTrnId { get; set; }

		[JsonProperty(PropertyName = "purseId")]
		public string purseId { get; set; }

		[JsonProperty(PropertyName = "accountId")]
		public string accountId { get; set; }

		[JsonProperty(PropertyName = "coId")]
		public string coId { get; set; }

		[JsonProperty(PropertyName = "paymentNo")]
		public string paymentNo { get; set; }

		[JsonProperty(PropertyName = "paywayId")]
		public string paywayId { get; set; }

		[JsonProperty(PropertyName = "state")]
		public string state { get; set; }

		[JsonProperty(PropertyName = "result")]
		public string result { get; set; }

		[JsonProperty(PropertyName = "stateName")]
		public string stateName { get; set; }

		[JsonProperty(PropertyName = "currencyId")]
		public int currencyId { get; set; }
	}
}
