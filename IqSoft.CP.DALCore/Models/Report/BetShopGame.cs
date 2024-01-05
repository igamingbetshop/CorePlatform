namespace IqSoft.CP.DAL.Models.Report
{
    public class BetShopGame
    {
        public int GameId { get; set; }

        public string GameName { get; set; }

        public decimal BetAmount { get; set; }

        public decimal WinAmount { get; set; }

        public string CurrencyId { get; set; }
        public int Count { get; set; }
    }
}
