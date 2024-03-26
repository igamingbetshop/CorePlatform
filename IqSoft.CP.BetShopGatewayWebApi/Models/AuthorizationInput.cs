namespace IqSoft.CP.BetShopGatewayWebApi.Models
{
    public class AuthorizationInput : RequestBase
    {
        public string UserName { get; set; }

        public string Password { get; set; }

        public string Hash { get; set; }

        public string HostName { get; set; }

        public string CardNumber { get; set; }

        public string ExternalId { get; set; }
    }
}