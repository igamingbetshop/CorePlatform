namespace IqSoft.CP.BetShopWebApi.Models.Common
{
	public class CreatePaymentRequest : PlatformRequestBase
	{
		public int CashDeskId { get; set; }
		public int CashierId { get; set; }
		public int ClientId { get; set; }
		public decimal Amount { get; set; }
	}
}