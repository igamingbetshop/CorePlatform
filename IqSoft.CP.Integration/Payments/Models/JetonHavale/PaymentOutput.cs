using Newtonsoft.Json;
using System;

namespace IqSoft.CP.Integration.Payments.Models.JetonHavale
{
	public class PaymentOutput
	{
		[JsonProperty(PropertyName = "status")]
		public bool Status { get; set; }

		[JsonProperty(PropertyName = "deposit")]
		public Deposit Deposit { get; set; }

		[JsonProperty(PropertyName = "token")]
		public string Token { get; set; }
	}
	public class CustomerDetails
	{
		[JsonProperty(PropertyName = "fullName")]
		public string FullName { get; set; }

		[JsonProperty(PropertyName = "username")]
		public string Username { get; set; }

		[JsonProperty(PropertyName = "id")]
		public string Id { get; set; }
	}

	public class Deposit
	{
		[JsonProperty(PropertyName = "customer")]
		public CustomerDetails Customer { get; set; }

		[JsonProperty(PropertyName = "transactionId")]
		public string TransactionId { get; set; }

		[JsonProperty(PropertyName = "amount")]
		public decimal Amount { get; set; }

		[JsonProperty(PropertyName = "partner")]
		public string Partner { get; set; }

		[JsonProperty(PropertyName = "status")]
		public string Status { get; set; }

		[JsonProperty(PropertyName = "_id")]
		public string Id { get; set; }

		[JsonProperty(PropertyName = "created_at")]
		public DateTime CreatedAt { get; set; }

		[JsonProperty(PropertyName = "updated_at")]
		public DateTime UpdatedAt { get; set; }

		[JsonProperty(PropertyName = "__v")]
		public int V { get; set; }
	}
}
