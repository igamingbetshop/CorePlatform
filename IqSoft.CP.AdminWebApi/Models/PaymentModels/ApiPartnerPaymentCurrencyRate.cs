namespace IqSoft.CP.AdminWebApi.Models.PaymentModels
{
    public class ApiPartnerPaymentCurrencyRate
    {
        public int Id { get; set; }
        public int PaymentSettingId { get; set; }
        public string CurrencyId { get; set; }
        public decimal Rate { get; set; }
    }
}