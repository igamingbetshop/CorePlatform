namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class ApiExternalApiInput : ApiRequestBase
    {
        public int ClientId { get; set; }
        public string Token { get; set; }
        public int Type { get; set; }
    }
}
