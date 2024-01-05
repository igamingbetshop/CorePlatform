namespace IqSoft.CP.DAL.Models.Integration.ProductsIntegration
{
    public class FinOperationOutputItem
    {
        public int ClientId { get; set; }
        public decimal Balance { get; set; }
        public decimal CurrentLimit { get; set; }
        public int CashDeskId { get; set; }
        public string CurrencyId { get; set; }
        public long? TicketNumber { get; set; }
        public long BetId { get; set; }
        public long BarCode { get; set; }
        public int? BonusId { get; set; }
        public decimal PartnerMainCurrencyRate { get; set; }
    }
}
