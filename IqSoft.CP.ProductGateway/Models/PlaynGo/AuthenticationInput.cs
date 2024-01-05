namespace IqSoft.CP.ProductGateway.Models.PlaynGo
{
    public class AuthenticateInput
    {
        public string username { get; set; }
        public string productId { get; set; }
        public string CID { get; set; }
        public string clientIP { get; set; }
        public string contextId { get; set; }
        public string accessToken { get; set; }
        public string language { get; set; }
        public string gameId { get; set; }
        public string channel { get; set; }
    }
}