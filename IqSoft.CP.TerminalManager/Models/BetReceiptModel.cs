namespace IqSoft.CP.TerminalManager.Models
{
    public class BetReceiptModel : PrintInputBase
    {       
        public string TicketNumber { get; set; }
        public string BetType { get; set; }
        public string ShopAddress { get; set; }
        public string Barcode { get; set; }
        public bool IsDuplicate { get; set; }
        public BetData BetDetails { get; set; }
    }

    public class BetData
    {
        public string BetAmountLabel { get; set; }
        public string BetAmount { get; set; }

        public string FeeLabel { get; set; }
        public string FeeValue { get; set; }

        public string BetsNumberLabel { get; set; }
        public string BetsNumber { get; set; }

        public string AmountPerBetLabel { get; set; }
        public string AmountPerBet { get; set; }

        public string TotalAmountLabel { get; set; }
        public string TotalAmount { get; set; }

        public string PossibleWinLabel { get; set; }
        public string PossibleWin { get; set; }
        public List<BetSelection> Selections { get; set; }

    }

    public class BetSelection
    {
        public string Id { get; set; }
        public string Team1 { get; set; }
        public string Team2 { get; set; }
        public string MatchDate { get; set; }
        public string MatchTime { get; set; }
        public string MatchName { get; set; } // market name 
        public string Coefficient { get; set; }
        public string Score { get; set; }
        public string CurrentTime { get; set; }
        public string EventInfoLabel { get; set; }
        public string EventInfo { get; set; }
        public string RoundIdLabel { get; set; }
        public string RoundId { get; set; }
        public string SelectionName { get; set; }
    }
}