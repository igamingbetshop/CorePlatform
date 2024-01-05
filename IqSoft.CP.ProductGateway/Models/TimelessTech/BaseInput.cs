using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Web;

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

	public class AuthenticateInput
	{
		[JsonProperty(PropertyName = "token")]
		public string Token { get; set; }
	}

	public class BalanceInput
	{
		[JsonProperty(PropertyName = "token")]
		public string Token { get; set; }

		[JsonProperty(PropertyName = "user_id")]
		public string UserId { get; set; }

		[JsonProperty(PropertyName = "currency_code")]
		public string CurrencyCode { get; set; }
	}

	public class CancelInput
	{
		[JsonProperty(PropertyName = "transaction_id")]
		public int TransactionId { get; set; }

		[JsonProperty(PropertyName = "round_id")]
		public int RoundId { get; set; }

		[JsonProperty(PropertyName = "round_finished")]
		public bool? RoundFinished { get; set; }

		[JsonProperty(PropertyName = "game_id")]
		public int GameId { get; set; }

		[JsonProperty(PropertyName = "user_id")]
		public string UserId { get; set; }
	}
	public class StatusInput
	{
		[JsonProperty(PropertyName = "transaction_type")]
		public string TransactionType { get; set; }

		[JsonProperty(PropertyName = "transaction_id")]
		public int TransactionId { get; set; }

		[JsonProperty(PropertyName = "transaction_date")]
		public string TransactionDate { get; set; }

		[JsonProperty(PropertyName = "transaction_ts")]
		public string TransactionTs { get; set; }

		[JsonProperty(PropertyName = "round_id")]
		public int RoundId { get; set; }

		[JsonProperty(PropertyName = "user_id")]
		public string UserId { get; set; }
	}
	public class TransactionInput
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
		public int TransactionId { get; set; }

		[JsonProperty(PropertyName = "transaction_timestamp")]
		public string TransactionTimestamp { get; set; }

		[JsonProperty(PropertyName = "round_id")]
		public int RoundId { get; set; }

		[JsonProperty(PropertyName = "round_finished")]
		public bool RoundFinished { get; set; }

		[JsonProperty(PropertyName = "game_id")]
		public int GameId { get; set; }

		[JsonProperty(PropertyName = "user_id")]
		public string UserId { get; set; }

		[JsonProperty(PropertyName = "token")]
		public string Token { get; set; }
	}
}