namespace IqSoft.CP.BetShopWebApi.Models.Common
{
	public class ApiClientLogin : ClientRequestResponseBase
	{
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}