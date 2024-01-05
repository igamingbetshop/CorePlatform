namespace IqSoft.CP.BetShopWebApi.Models.Common
{
	public class GetCashDeskOperationsInput : ApiRequestBase
	{
		public int? LastShiftsNumber { get; set; }

		public int CashierId { get; set; }

		public int? CashDeskId { get; set; }

		public int? BetShopId { get; set; }
	}
}