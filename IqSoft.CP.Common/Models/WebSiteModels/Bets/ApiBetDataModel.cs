namespace IqSoft.CP.Common.Models.WebSiteModels.Bets
{
    public class ApiBetDataModel
    {
        public int PartnerId { get; set; }
        public int ProductId { get; set; }
        public string CurrencyId{ get; set; }
        public decimal Amount { get; set; }
    }
}
