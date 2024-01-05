namespace IqSoft.CP.ProductGateway.Models.BetSoft
{
    public class RefundBetInput
    {
        public int userId { get; set; }
        public string token { get; set; }
        public string casinoTransactionId { get; set; }
        public string hash { get; set; }
    }
}