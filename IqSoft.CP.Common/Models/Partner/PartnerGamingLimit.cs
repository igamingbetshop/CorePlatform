namespace IqSoft.CP.Common.Models.Partner
{
    public class PartnerGamingLimit
    {
        public string CurrencyId { get; set; }
        public decimal? DefaultDepositLimitMonthly { get; set; }
        public decimal? DefaultUnverifiedDepositLimitMonthly { get; set; }
        public decimal? MinUnverifiedDepositLimitMonthly { get; set; }
        public decimal? DefaultLossPercentMonthly { get; set; }
        public decimal? MinDepositLimitMonthly { get; set; }
        public decimal? MaxDepositLimitMonthly { get; set; }
    }
}