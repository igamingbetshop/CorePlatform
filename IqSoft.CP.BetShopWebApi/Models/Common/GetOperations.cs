namespace IqSoft.CP.BetShopWebApi.Models.Common
{
    public class GetOperationsInput : ApiRequestBase
	{
		public int CashierId { get; set; }

		public int CashDeskId { get; set; }

		public long? Barcode { get; set; }
	}
}