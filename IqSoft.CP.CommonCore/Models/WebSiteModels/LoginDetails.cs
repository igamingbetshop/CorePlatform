namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class LoginDetails : ApiRequestBase
    {
        public string ClientIdentifier { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }
		public int? DeviceType { get; set; }
        public int? ExternalPlatformId { get; set; }
        public string TerminalId { get; set; }
        public int? BetShopId { get; set; }
    }
}