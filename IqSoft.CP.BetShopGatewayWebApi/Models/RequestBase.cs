namespace IqSoft.CP.BetShopGatewayWebApi.Models
{
    public class RequestBase : RequestInfo
    {
        public string Token { get;set;}
        public string Ip { get; set; }
        public string Country { get; set; }
    }
}