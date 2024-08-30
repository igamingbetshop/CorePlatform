namespace IqSoft.CP.ProductGateway.Models.ImperiumGames
{
	public class BaseOutput
	{
		public string status { get; set; } = "success";
		public string error { get; set; } = string.Empty;
	}
	public class BalanceOutput : BaseOutput
	{
		public string login { get; set; }
		public string balance { get; set; }
		public string currency { get; set; }
	}

	public class TransactionOutput : BalanceOutput
	{
		public string operationId { get; set; }
	}
}