namespace IqSoft.CP.Common.Models.AdminModels
{
    public class PaymentRequestInput
    {
        public long? AccountId { get; set; }
        public int ClientId { get; set; }
        public int PaymentSystemId { get; set; }
        public decimal Amount { get; set; }
        public string ExternalTransactionId { get; set; }
        public int? BonusId { get; set; }
        public string PromoCode { get; set; }
    }
}