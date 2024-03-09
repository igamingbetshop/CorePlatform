namespace IqSoft.CP.BetShopWebApi.Models.Common
{
	public class PlatformCancelBetSelectionInput : PlatformRequestBase
	{
		public int MarketTypeId { get; set; }
		public int SelectionTypeId { get; set; }
		public long SelectionId { get; set; }
		public int GameId { get; set; }
		public int GameUnitId { get; set; }
	}
}