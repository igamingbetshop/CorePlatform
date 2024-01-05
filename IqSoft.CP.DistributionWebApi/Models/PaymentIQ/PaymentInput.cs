namespace IqSoft.CP.DistributionWebApi.Models.PaymentIQ
{
    public class PaymentInput
    {
        public string MerchantId { get; set; }
        public string UserId { get; set; }
        public string SessionId { get; set; }
        public string Lang { get; set; }
        public string Environment { get; set; }
        public string Method { get; set; }
        public string PartnerName { get; set; }
        public string Cashier { get; set; }
        public decimal Amount { get; set; }
        public long PaymentRequestId { get; set; }
        public string ProviderType​ { get; set; }
    }
}