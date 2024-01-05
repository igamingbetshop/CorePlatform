using IqSoft.CP.ProductGateway.Models.IqSoft;

namespace IqSoft.CP.ProductGateway.Models.WinSystems
{
    public class AuthorizationOutput : ApiResponseBase
    {
        public string ClientId { get; set; }    
        public string CurrencyId { get; set; }    
        public decimal Balance { get; set; }  
        public bool IsValidPlayer { get; set; }
    }
}