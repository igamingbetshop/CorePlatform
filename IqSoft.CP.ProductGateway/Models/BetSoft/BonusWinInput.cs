namespace IqSoft.CP.ProductGateway.Models.BetSoft
{
    public class BonusWinInput
    {
        public string token { get; set; }
        public string hash { get; set; }
        public int userId { get; set; }
        public string transactionId { get; set; }
        public int amount { get; set; }
        public int bonusId { get; set; }
    }
}