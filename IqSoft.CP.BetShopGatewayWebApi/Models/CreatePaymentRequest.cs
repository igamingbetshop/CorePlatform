namespace IqSoft.CP.BetShopGatewayWebApi.Models
{
	public class CreatePaymentRequest : RequestBase
	{
		public int CashDeskId { get; set; }
		public int CashierId { get; set; }
		public int ClientId { get; set; }
		public decimal Amount { get; set; }
	}
}