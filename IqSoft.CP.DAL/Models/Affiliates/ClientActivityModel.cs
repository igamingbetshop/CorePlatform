namespace IqSoft.CP.DAL.Models.Affiliates
{
    public class ClientActivityModel
    {     
        public int BrandId { get; set; }
        public int CustomerId { get; set; }
        public string CurrencyId { get; set; }
        public string BTag { get; set; }
        public int Category { get; set; }
        public string ActivityDate { get; set; }

        public decimal SportTotalBetsAmount { get; set; }
        public decimal SportBonusBetsAmount { get; set; }
        public decimal SportTotalWinsAmount { get; set; }
        public decimal SportBonusWinsAmount { get; set; }
        public decimal SportGrossRevenue { get; set; }
        public decimal CasinoTotalBetsAmount { get; set; }
        public decimal CasinoBonusBetsAmount { get; set; }
        public decimal CasinoTotalWinsAmount { get; set; }
        public decimal CasinoBonusWinsAmount { get; set; }
        public decimal CasinoGrossRevenue { get; set; }
        public int TotalBetsCount { get; set; }

        public decimal FirstDepositAmount { get; set; }
        public decimal ManualDepositAmount { get; set; }
        public decimal TotalDepositsAmount { get; set; }
        public int TotalDepositsCount { get; set; }
        public decimal TotalWithdrawalsAmount { get; set; }
        public int TotalWithdrawalsCount { get; set; }
    }
}