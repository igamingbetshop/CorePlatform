namespace IqSoft.CP.DAL.Models.Dashboard
{
    public class PlayersInfo
    {
        public int VisitorsCount { get; set; }

        public int SignUpsCount { get; set; }

        public int TotalPlayersCount { get; set; }

        public int ReturnsCount { get; set; }

        public int DepositsCount { get; set; }

        public decimal TotalBonusAmount { get; set; }

        public decimal TotalCashoutAmount { get; set; }

        public decimal TotalBetAmount { get; set; }

        public decimal AverageBet { get; set; }

        public decimal MaxBet { get; set; }

        public decimal MaxWin { get; set; }

        public decimal MaxWinBet { get; set; }
    }
}