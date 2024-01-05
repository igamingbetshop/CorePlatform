namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class GetPartnerPaymentSystemsInput
    {
        public int PartnerId { get; set; }
        public int PaymentSystemId { get; set; }
        public string CurrencyId { get; set; }
    }
}