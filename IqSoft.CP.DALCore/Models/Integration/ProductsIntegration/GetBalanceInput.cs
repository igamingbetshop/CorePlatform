namespace IqSoft.CP.DAL.Models.Integration.ProductsIntegration
{
    public class GetBalanceInput : InputBase
    {
        public string Token { get; set; }
        public int ClientId { get; set; }
        public string CurrencyId { get; set; }
    }
}