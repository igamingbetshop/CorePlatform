namespace IqSoft.CP.AdminWebApi.Models.ReportModels.BetShop
{
    public class ApiBetShopGame
    {
        public int GameId { get; set; }

        public string GameName { get; set; }

        public decimal BetAmount { get; set; }

        public decimal WinAmount { get; set; }

        public string CurrencyId { get; set; }

        public int Count { get; set; }

        public decimal Profit
        {
            get { return BetAmount - WinAmount; }
        }

        public decimal ProfitPercent
        {
            get { return BetAmount == 0 ? 0 : (BetAmount - WinAmount) * 100 / BetAmount; }
        }
    }
}