namespace IqSoft.CP.AdminWebApi.Models.ClientModels
{
    public class ApiClientExclusion
    {
        public int PartnerId { get; set; }
        public int ClientId { get; set; }
        public string Username { get; set; }
        public decimal? DepositLimitDaily { get; set; }
        public decimal? DepositLimitWeekly { get; set; }
        public decimal? DepositLimitMonthly { get; set; }
        public decimal? TotalBetAmountLimitDaily { get; set; }
        public decimal? TotalBetAmountLimitWeekly { get; set; }
        public decimal? TotalBetAmountLimitMonthly { get; set; }
        public decimal? TotalLossLimitDaily { get; set; }
        public decimal? TotalLossLimitWeekly { get; set; }
        public decimal? TotalLossLimitMonthly { get; set; }
        public decimal? SystemDepositLimitDaily { get; set; }
        public decimal? SystemDepositLimitWeekly { get; set; }
        public decimal? SystemDepositLimitMonthly { get; set; }
        public decimal? SystemTotalBetAmountLimitDaily { get; set; }
        public decimal? SystemTotalBetAmountLimitWeekly { get; set; }
        public decimal? SystemTotalBetAmountLimitMonthly { get; set; }
        public decimal? SystemTotalLossLimitDaily { get; set; }
        public decimal? SystemTotalLossLimitWeekly { get; set; }
        public decimal? SystemTotalLossLimitMonthly { get; set; }
        public decimal? SessionLimit { get; set; }
        public decimal? SystemSessionLimit { get; set; }
    }
}