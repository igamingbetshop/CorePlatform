namespace IqSoft.CP.DAL.Models.Integration.ProductsIntegration
{
    public class FinOperationOutputItem
    {
        public string ClientId { get; set; }
        public decimal Balance { get; set; }
        public decimal CurrentLimit { get; set; }
        public int CashDeskId { get; set; }
        public string CurrencyId { get; set; }
        public long? TicketNumber { get; set; }
        public string BetId { get; set; }
        public long BarCode { get; set; }
        public int? BonusId { get; set; }
        public long? AccountId { get; set; }
        public decimal PartnerMainCurrencyRate { get; set; }
    }
}
