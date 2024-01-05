namespace IqSoft.CP.BetShopGatewayWebApi.Models
{
    public class RequestBase
    {
        public string Token { get;set;}
        public double TimeZone { get;set;}
        public string LanguageId { get; set; }
        public int PartnerId { get; set; }
    }
}