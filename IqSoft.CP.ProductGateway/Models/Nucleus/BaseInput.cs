namespace IqSoft.CP.ProductGateway.Models.Nucleus
{
    public class BaseInput
    {
        public string userId { get; set; }
        public string casinoTransactionId { get; set; }
        public string bet { get; set; } //in the format: bet_amount|transactionId
        public string win { get; set; } //in the format: win_amount|transactionId
        public string roundId { get; set; }
        public string gameId { get; set; }
        public bool? isRoundFinished { get; set; }
        public string hash { get; set; }
        public string gameSessionId { get; set; }
        public string token { get; set; }

    }
}