namespace IqSoft.CP.ProductGateway.Models.Nucleus
{
    public class BonusInput
    {
        public string userId { get; set; }
        public string token { get; set; }
        public string bonusId { get; set; }
        public string amount { get; set; }
        public string transactionId { get; set; }
        public string hash { get; set; }
    }
}