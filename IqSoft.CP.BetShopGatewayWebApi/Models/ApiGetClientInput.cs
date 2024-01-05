namespace IqSoft.CP.BetShopGatewayWebApi.Models
{
    public class ApiGetClientInput : RequestBase
    {
        public int? ClientId { get; set; }

        public int CashDeskId { get; set; }
        
        public string UserName { get; set; }
        
        public string DocumentNumber { get; set; }
        
        public string Email { get; set; }
    }
}