namespace IqSoft.CP.AdminWebApi.Models.BetShopModels
{
    public class PayBetShopDebtModel
    {
        public int BetshopId { get; set; }

        public string CurrencyId { get; set; }

        public decimal Amount { get; set; }
    }
}