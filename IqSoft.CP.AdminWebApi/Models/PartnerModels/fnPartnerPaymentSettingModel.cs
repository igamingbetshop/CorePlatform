namespace IqSoft.CP.AdminWebApi.Models.PartnerModels
{
    public class FnPartnerPaymentSettingModel
    {
        public int Id { get; set; }
        public int PartnerId { get; set; }
        public int PaymentSystemId { get; set; }
        public decimal Commission { get; set; }
        public int State { get; set; }
        public string CurrencyId { get; set; }
        public long SessionId { get; set; }
        public System.DateTime CreationTime { get; set; }
        public System.DateTime LastUpdateTime { get; set; }
        public string PaymentSystemName { get; set; }
        public int PaymenSystemType { get; set; }

        public int Type { get; set; }
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public string Info { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int Priority { get; set; }
    }
}