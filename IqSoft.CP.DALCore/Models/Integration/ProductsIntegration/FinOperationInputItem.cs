namespace IqSoft.CP.DAL.Models.Integration.ProductsIntegration
{
    public class FinOperationInputItem
    {
        public int ClientId { get; set; }

        public int CashDeskId { get; set; }

        public string Token { get; set; }

        public decimal Amount { get; set; }

        public int Type { get; set; }
    }
}
