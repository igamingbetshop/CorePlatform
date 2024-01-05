namespace IqSoft.CP.ProductGateway.Models.SoftLand
{
	public class OperationResponse
	{
		public string PlatformTransactionId { get; set; }
		public long TransactionId { get; set; }
		public decimal Balance { get; set; }
		public decimal? BonusBalance { get; set; }
		public int? ErrorCode { get; set; }
		public string ErrorMessage { get; set; }

	}
}