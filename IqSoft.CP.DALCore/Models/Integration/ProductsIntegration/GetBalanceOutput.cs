namespace IqSoft.CP.DAL.Models.Integration.ProductsIntegration
{
    public class GetBalanceOutput : ResponseBase
    {
        public decimal AvailableBalance { get; set; }
        public string CurrencyId { get; set; }
    }
}