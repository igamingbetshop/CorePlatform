using IqSoft.CP.BetShopGatewayWebApi.Models;

namespace IqSoft.CP.BetShopWebApi.Models.Common
{
	public class GetBetShopBetsInput : ApiFilterBase
    {
		public int CashierId { get; set; }
		public int CashDeskId { get; set; }
		public int? ProductId { get; set; }
		public long? Barcode { get; set; }
		public int? State { get; set; }
	}
}