using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.ProductGateway.Models.TVBet
{
	public class TransactionInput
	{
		[JsonProperty(PropertyName = "ti")]
		public long UnixTime { get; set; }

		[JsonProperty(PropertyName = "si")]
		public string Signature { get; set; }

		[JsonProperty(PropertyName = "to")]
		public string Token { get; set; }

		[JsonProperty(PropertyName = "bid")]
		public long TransactionId { get; set; }

		[JsonProperty(PropertyName = "tt")]
		public int TransactionType { get; set; }

		[JsonProperty(PropertyName = "sm")]
		public string Amount { get; set; }

		[JsonProperty(PropertyName = "ed")]
		public ExtensionData ExtensionData { get; set; }
	}

	public class ExtensionData
	{
		[JsonProperty(PropertyName = "gms")]
		public List<Game> Games { get; set; }
	}

	public class Game
	{
		[JsonProperty(PropertyName = "gt")]
		public int GameExternalId { get; set; }

		[JsonProperty(PropertyName = "gid")]
		public string RoundId { get; set; }
	}
}