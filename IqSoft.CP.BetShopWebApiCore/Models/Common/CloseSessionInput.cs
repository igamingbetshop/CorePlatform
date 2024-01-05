namespace IqSoft.CP.BetShopWebApi.Models.Common
{
	public class CloseSessionInput : PlatformRequestBase
	{
		public int Id { get; set; }

		public int CashDeskId { get; set; }

		public int? ProductId { get; set; }
	}
}