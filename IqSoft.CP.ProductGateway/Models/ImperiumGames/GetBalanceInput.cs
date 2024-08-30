namespace IqSoft.CP.ProductGateway.Models.ImperiumGames
{
	public class GetBalanceInput
	{
		public string hall { get; set; }
		public string key { get; set; }
		public string login { get; set; }
		public string cmd { get; set; }
	}

	public class WriteBetInput : GetBalanceInput
	{
		public string bet { get; set; }
		public string win { get; set; }
		public double? winLose { get; set; }
		public string tradeId { get; set; }
		public string betInfo { get; set; }
		public string gameId { get; set; }
		public string matrix { get; set; }
		public string date { get; set; }
		public string WinLines { get; set; }
	}
}