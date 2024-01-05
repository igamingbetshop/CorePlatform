namespace IqSoft.CP.AdminWebApi.Models.ReportModels.BetShop
{
    public class ApiBetShopReport
    {
        public string BetShopName { get; set; }

        public int BetShopId { get; set; }

        public int BetShopGroupId { get; set; }

        public decimal BetAmount { get; set; }

        public decimal WinAmount { get; set; }

        public string CurrencyId { get; set; }

        public decimal Profit
        {
            get { return BetAmount - WinAmount; }
        }

        public decimal ProfitPercent
        {
            get { return BetAmount == 0 ? 0 : (BetAmount - WinAmount) / BetAmount; }
        }
    }
}