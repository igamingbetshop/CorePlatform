namespace IqSoft.CP.BetShopWebApi.Models.Common
{
	public class GetShiftReportInput : ApiRequestBase
	{

		public int? CashierId { get; set; }

		public int CashDeskId { get; set; }
	}
}