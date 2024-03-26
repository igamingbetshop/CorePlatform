namespace IqSoft.CP.BetShopWebApi.Models.Common
{
	public class RequestInfo
	{
		public double? TimeZone { get; set; }
		public string LanguageId { get; set; }
        public string Ip { get; set; }
        public string Country { get; set; }
        public int PartnerId { get; set; }
    }
}