namespace IqSoft.CP.DAL.Models.Integration.ProductsIntegration
{
    public class AuthorizationInput : InputBase
    {
        public string ClientIdentifier { get; set; }

        public string Password { get; set; }
        
        public string Ip { get; set; }
        
        public string Token { get; set; }

        public int? ProductId { get; set; }
    }
}