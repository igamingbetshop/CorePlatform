namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class GetBalanceInput
    {
        public int ClientId { get; set; }

        public int PartnerId { get; set; }

        public string CurrencyId { get; set; }
    }
}