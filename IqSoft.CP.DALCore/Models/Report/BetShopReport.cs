namespace IqSoft.CP.DAL.Models.Report
{
    public class BetShopReport
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
    }
}