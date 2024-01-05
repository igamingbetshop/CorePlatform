namespace IqSoft.CP.DistributionWebApi.Models.PerfectMoney
{
    public class PaymentRequestInput
    {
        public string MerchantId { get; set; }
        public string MerchantName { get; set; }
        public decimal? Amount { get; set; }
        public string CurrencyId { get; set; }
        public string PaymentRequestId { get; set; }
        public string StatusUrl { get; set; }
        public string PaymentUrl { get; set; }
        public string PaymentMethod { get; set; }
        public string Language { get; set; }
        public string InputData { get; set; }
        public long? VoucherNumber { get; set; }
        public long? ActivationCode { get; set; }
    }
}