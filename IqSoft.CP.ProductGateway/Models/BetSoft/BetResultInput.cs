namespace IqSoft.CP.ProductGateway.Models.BetSoft
{
    public class BetResultInput
    {
        public int userId { get; set; }

        public string token { get; set; }

        public string bet { get; set; }

        public string win { get; set; }

        public string roundId { get; set; }

        public string gameId { get; set; }

        public bool isRoundFinished { get; set; }

        public string hash { get; set; }

        public string gameSessionId { get; set; }

        public int negativeBet { get; set; }
    }
}