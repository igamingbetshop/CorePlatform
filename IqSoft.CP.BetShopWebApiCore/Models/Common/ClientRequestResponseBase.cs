namespace IqSoft.CP.BetShopWebApi.Models.Common
{
	public class ClientRequestResponseBase
	{
		public int ResponseCode { get; set; }

		public string Description { get; set; }

		public object ResponseObject { get; set; }
	}
}