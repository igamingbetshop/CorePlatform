namespace IqSoft.CP.BetShopWebApi.Models.Common
{
	public class GetCashDeskCurrentBalanceOutput : ClientRequestResponseBase
	{
		public decimal Balance { get; set; }
		public decimal TerminalBalance { get; set; }
		public decimal CashDeskBalance { get; set; }
		public decimal CurrentLimit { get; set; }
	}
}