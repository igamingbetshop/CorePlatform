namespace IqSoft.CP.DistributionWebApi.Models.PaymentProcessing
{
    public class PaymentProcessingInput
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string HolderName { get; set; }
        public string BillingAddress { get; set; }
        public string CountryCode { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
        public string ResponseUrl { get; set; }
        public string RedirectUrl { get; set; }
        public string CancelUrl { get; set; }
        public string PartnerDomain { get; set; }
        public string ResourcesUrl { get; set; }
        public string LanguageId { get; set; }
        public string PayAddress { get; set; }
        public string PartnerId { get; set; }
        public string PaymentSystemName { get; set; }
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
    }
}