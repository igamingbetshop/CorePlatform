namespace IqSoft.CP.BetShopWebApi.Models.Common
{
	public class PlatformRequestBase
	{
		public string Token { get; set; }

		public double TimeZone { get; set; }

		public string LanguageId { get; set; }

		public int PartnerId { get; set; }
	}
}