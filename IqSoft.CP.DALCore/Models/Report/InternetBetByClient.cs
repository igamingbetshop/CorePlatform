namespace IqSoft.CP.DAL.Models.Report
{
    public class InternetBetByClient
    {
        public int ClientId { get; set; }

        public string UserName { get; set; }

		public int State { get; set; }

        public int TotalBetsCount { get; set; }

        public decimal TotalBetsAmount { get; set; }

        public decimal TotalWinsAmount { get; set; }

        public string Currency { get; set; }

        public decimal GGR { get; set; }

        public decimal MaxBetAmount { get; set; }

        public int TotalDepositsCount { get; set; }

        public decimal TotalDepositsAmount { get; set; }

        public int TotalWithdrawalsCount { get; set; }

        public decimal TotalWithdrawalsAmount { get; set; }

        public decimal Balance { get; set; }
    }
}
