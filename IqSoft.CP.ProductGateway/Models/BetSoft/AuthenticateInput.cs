namespace IqSoft.CP.ProductGateway.Models.BetSoft
{
    public class AuthenticateInput
    {
        public string token { get; set; }

        public string hash { get; set; }

        public string clientType { get; set; }
    }
}