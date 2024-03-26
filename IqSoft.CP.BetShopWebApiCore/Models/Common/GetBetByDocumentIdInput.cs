namespace IqSoft.CP.BetShopWebApi.Models.Common
{
	public class GetBetByDocumentIdInput : PlatformRequestBase
	{
		public long DocumentId { get; set; }

		public bool IsForPrint { get; set; }
	}
}