namespace IqSoft.NGGP.WebApplications.BetShopGatewayWebApi.Models
{
    public class GetClientInput : RequestBase
    {
        public int ClientId { get; set; }
        public string UserName { get; set; }
        public string DocumentNumber { get; set; }
        public string Email { get; set; }
    }
}