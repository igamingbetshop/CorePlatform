using System;

namespace IqSoft.CP.ProductGateway.Models.LuckyStreak
{
	public class BaseOutput
	{
		public object data { get; set; }
		public Error errors { get; set; }
	}

	public class ValidateOutput
	{
		public string userName { get; set; }
		public string currency { get; set; }
		public string language { get; set; }
		public string nickname { get; set; }
		public decimal balance { get; set; }
		public DateTime lastUpdateDate { get; set; }
		public DateTime balanceTimestamp { get; set; }
	}

	public class BalanceOutput
	{
		public string currency { get; set; }
		public decimal balance { get; set; }
		public DateTime balanceTimestamp { get; set; }
	}

	public class TransactionOutput : BalanceOutput
	{
		public string refTransactionId { get; set; }
	}

	public class Error
	{
		public string code { get; set; }
		public string title { get; set; }
	}
}