namespace IqSoft.CP.ProductGateway.Models.LuckyStreak
{
	public class BalanceInput
	{
		public Balance data { get; set; }
	}

	public class Balance
	{
		public string username { get; set; }
		public string operatorId { get; set; }
		public string currency { get; set; }
		public string additionalParams { get; set; }
		//public int gameId { get; set; }
	}
}