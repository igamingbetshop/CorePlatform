namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class RequestBase : ApiRequestBase
    {
        public string Controller { get; set; }

        public string Method { get; set; }

        public int ClientId { get; set; }

        public string Token { get; set; }

        public string Position { get; set; }

        public string ProductId { get; set; }

        public string RequestData { get; set; }

        public string Credentials { get; set; }

        public bool IsAgent { get; set; }
    }
}