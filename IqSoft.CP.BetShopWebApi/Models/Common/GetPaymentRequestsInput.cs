namespace IqSoft.CP.BetShopWebApi.Models.Common
{
	public class GetPaymentRequestsInput : ApiRequestBase
	{
		public int? ClientId { get; set; }

		public string DocumentNumber { get; set; }

		public string DocumentIssuedBy { get; set; }

		public int CashDeskId { get; set; }

		public string CashCode { get; set; }
	}
}