namespace IqSoft.CP.ProductGateway.Models.PlaynGo.Input
{
    public class Balance 
    {
        public string externalId { get; set; }
        public string productId { get; set; }
        public string currency { get; set; }
        public string gameId { get; set; }
        public string externalGameSessionId { get; set; }
        public string accessToken { get; set; }
    }
}