namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class PartnerPaymentSystemInfo
    {
        public int PaymentSystemId { get; set; }
        public decimal DebitPercent { get; set; }
        public decimal CreditPercent { get; set; }
        public int State { get; set; }
        public string CurrencyId { get; set; }
        public string PaymentSystemName { get; set; }
        public int PaymentSystemPriority { get; set; }
        public int PaymentSystemRequestType { get; set; }
        public int ContentType { get; set; }
    }
}