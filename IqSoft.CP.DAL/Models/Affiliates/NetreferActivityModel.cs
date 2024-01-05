namespace IqSoft.CP.DAL.Models.Affiliates
{
    public class NetreferActivityModel
    {
        public int BrandID { get; set; }
        public int CustomerId { get; set; }
        public string ActivityDate { get; set; }
        public string CurrencyId { get; set; }
        public int ProductId { get; set; }
        public string BTag { get; set; }
        public decimal GrossRevenue { get; set; }
        public decimal Bonuses { get; set; }
        public decimal Adjustments { get; set; }
        public decimal Deposit { get; set; }
        public decimal Turnover { get; set; }
        public decimal Withdrawals { get; set; }
        public decimal Payout { get; set; }
        public decimal Transactions { get; set; }
    }
}