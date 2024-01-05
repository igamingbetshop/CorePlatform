namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class CreateDepositRequestInput
    {
        public int PaymentSystemId { get; set; }

        public int ClientId { get; set; }

        public int PartnerId { get; set; }

        public decimal Amount { get; set; }

        public string CurrencyId { get; set; }

        public string Info { get; set; }

        public string PaymentForm { get; set; }

        public string ImageName { get; set; }

        public int? BonusId { get; set; }
    }
}