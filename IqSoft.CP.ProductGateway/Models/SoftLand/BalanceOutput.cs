namespace IqSoft.CP.ProductGateway.Models.SoftLand
{
	public class BalanceOutput
	{
		public long PlayerId { get; set; }
		public string Currency { get; set; } 
		public decimal Balance { get; set; }
		public decimal? BonusBalance { get; set; }
		public int? ErrorCode { get; set; }
		public string ErrorMessage { get; set; }
	}
}