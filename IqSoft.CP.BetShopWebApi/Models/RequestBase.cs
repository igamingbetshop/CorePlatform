using IqSoft.CP.BetShopWebApi.Models.Common;

namespace IqSoft.CP.BetShopWebApi.Models
{
	public class RequestBase : RequestInfo
	{
		public bool IsFromServer { get; set; }
		public string ServerCredentials { get; set; }
		public string Method { get; set; }
		public string Token { get; set; }
		public int CashDeskId { get; set; }
		public int PartnerId { get; set; }
		public string RequestObject { get; set; }
	}
}