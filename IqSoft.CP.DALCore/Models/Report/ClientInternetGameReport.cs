namespace IqSoft.CP.DAL.Models.Report
{
    public class ClientInternetGameReport
    {
        public int TotalRoundCount { get; set; }

        public int TotalTransactionCount { get; set; }

        public long TotalBetCount { get; set; }

        public decimal TotalBetAmount { get; set; }

        public int TotalCanceledBetCount { get; set; }

        public decimal TotalCanceledBetAmount { get; set; }

        public int TotalWinCount { get; set; }

        public decimal TotalWinAmount { get; set; }
    }
}
