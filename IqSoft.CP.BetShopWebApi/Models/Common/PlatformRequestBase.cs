namespace IqSoft.CP.BetShopWebApi.Models.Common
{
	public class PlatformRequestBase : RequestInfo
    {
        public int PartnerId { get; set; }
        public string Token { get; set; }
		public int CashDeskId { get; set; }
	}
}