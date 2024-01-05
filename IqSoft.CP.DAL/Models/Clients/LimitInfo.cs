

namespace IqSoft.CP.DAL.Models.Clients
{
    public class LimitInfo
    {
        public int ClientId { get; set; }
        public int? DailyDepositLimitPercent { get; set; }
        public decimal? DailyDepositLimitAmountLeft { get; set; }
        public int? WeeklyDepositLimitPercent { get; set; }
        public decimal? WeeklyDepositLimitAmountLeft { get; set; }
        public int? MonthlyDepositLimitPercent { get; set; }
        public decimal? MonthlyDepositLimitAmountLeft { get; set; }
        public int? DailyBetLimitPercent { get; set; }
        public int? WeeklyBetLimitPercent { get; set; }
        public int? MonthlyBetLimitPercent { get; set; }
    }
}
