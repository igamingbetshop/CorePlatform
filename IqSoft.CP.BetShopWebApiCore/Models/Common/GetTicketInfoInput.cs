namespace IqSoft.CP.BetShopWebApi.Models.Common
{
	public class GetTicketInfoInput : PlatformRequestBase
	{
		public string Code { get; set; }
		public string TicketId { get; set; }
		public string ProductToken { get; set; }
	}
}