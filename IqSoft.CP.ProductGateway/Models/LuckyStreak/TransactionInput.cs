using System;

namespace IqSoft.CP.ProductGateway.Models.LuckyStreak
{
	public class TransactionInput
	{
		public MoveFunds data { get; set; }
	}

	public class MoveFunds
	{
		public string operatorId { get; set; }
		public string transactionRequestId { get; set; }
		public string username { get; set; }
		public string eventType { get; set; }
		public string eventSubType { get; set; }
		public string eventId { get; set; }
		public DateTime eventTime { get; set; }
		public string gameId { get; set; }
		public string gameType { get; set; }
		public string roundId { get; set; }
		public object eventDetails { get; set; }
		public string direction { get; set; }
		public string currency { get; set; }
		public decimal amount { get; set; }
		public string additionalParams { get; set; }
		public string abortedTransactionRequestId { get; set; }
	}

	public class EventDetails
	{
		public string refTransactionId { get; set; }
		public string providerId { get; set; }
		public string roundId { get; set; }
	}
}