namespace IqSoft.CP.DAL.Models.Report
{
    public class InternetBetsByClientReport : PagedModel<InternetBetByClient>
    {
        public decimal? TotalBetCount { get; set; }

        public decimal? TotalBetAmount { get; set; }

        public decimal? TotalWinAmount { get; set; }

        public decimal? TotalBalance { get; set; }

        public decimal? TotalDepositCount { get; set; }

        public decimal? TotalDepositAmount { get; set; }

        public decimal? TotalWithdrawCount { get; set; }

        public decimal? TotalWithdrawAmount { get; set; }

        public int? TotalCurrencyCount { get; set; }

        public decimal? TotalGGR { get; set; }
    }
}
