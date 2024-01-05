using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.Integration.Payments.Models.JetonHavale
{
	public class PayoutOutput
	{
		[JsonProperty(PropertyName = "status")]
		public bool Status { get; set; }

		[JsonProperty(PropertyName = "withdraw")]
		public Withdraw Withdraw { get; set; }
	}

	public class Withdraw
	{
		[JsonProperty(PropertyName = "customer")]
		public CustomerDetails Customer { get; set; }

		[JsonProperty(PropertyName = "bank")]
		public string Bank { get; set; }

		[JsonProperty(PropertyName = "iban")]
		public string Iban { get; set; }

		[JsonProperty(PropertyName = "transactionId")]
		public string TransactionId { get; set; }

		[JsonProperty(PropertyName = "amount")]
		public int Amount { get; set; }

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
