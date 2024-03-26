using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.TimelessTech
{
	public class BaseInput
	{
		[JsonProperty(PropertyName = "command")]
		public string Command { get; set; }

		[JsonProperty(PropertyName = "request_timestamp")]
		public string RequestTimestamp { get; set; }

		[JsonProperty(PropertyName = "hash")]
		public string Hash { get; set; }

		[JsonProperty(PropertyName = "data")]
		public object DataInput { get; set; }
	}

	public class DataBaseInput
	{
		[JsonProperty(PropertyName = "token")]
		public string Token { get; set; }

        [JsonProperty(PropertyName = "user_id")]
        public string UserId { get; set; }
    }

	public class RoundInput : DataBaseInput
    {
		[JsonProperty(PropertyName = "transaction_id")]
		public long TransactionId { get; set; }

		[JsonProperty(PropertyName = "round_id")]
		public string RoundId { get; set; }

		[JsonProperty(PropertyName = "round_finished")]
		public bool? RoundFinished { get; set; }

		[JsonProperty(PropertyName = "game_id")]
		public int GameId { get; set; }
	}


	public class StatusInput : DataBaseInput
    {
		[JsonProperty(PropertyName = "transaction_type")]
		public string TransactionType { get; set; }

		[JsonProperty(PropertyName = "transaction_id")]
		public long TransactionId { get; set; }

		[JsonProperty(PropertyName = "transaction_date")]
		public string TransactionDate { get; set; }

		[JsonProperty(PropertyName = "transaction_ts")]
		public string TransactionTs { get; set; }

		[JsonProperty(PropertyName = "round_id")]
		public long RoundId { get; set; }
	}

	public class FinishRoundInput : DataBaseInput
    {		
		[JsonProperty(PropertyName = "round_id")]
		public long RoundId { get; set; }

		[JsonProperty(PropertyName = "round_ts")]
		public string RoundTransaction { get; set; }

		[JsonProperty(PropertyName = "game_id")]
		public int GameId { get; set; }
	}
	public class TransactionInput : DataBaseInput
    {
		[JsonProperty(PropertyName = "transaction_type")]
		public string TransactionType { get; set; }

		[JsonProperty(PropertyName = "reason")]
		public string Reason { get; set; }

		[JsonProperty(PropertyName = "amount")]
		public decimal Amount { get; set; }

		[JsonProperty(PropertyName = "currency_code")]
		public string CurrencyCode { get; set; }

		[JsonProperty(PropertyName = "transaction_id")]
		public long TransactionId { get; set; }

		[JsonProperty(PropertyName = "transaction_timestamp")]
		public string TransactionTimestamp { get; set; }

		[JsonProperty(PropertyName = "round_id")]
		public long RoundId { get; set; }

		[JsonProperty(PropertyName = "round_finished")]
		public bool? RoundFinished { get; set; }

		[JsonProperty(PropertyName = "game_id")]
		public int GameId { get; set; }
	}
}