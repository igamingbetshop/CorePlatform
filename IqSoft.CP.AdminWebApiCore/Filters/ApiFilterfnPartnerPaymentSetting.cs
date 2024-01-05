using System;

namespace IqSoft.CP.AdminWebApi.Filters
{
    public class ApiFilterfnPartnerPaymentSetting
    {
        public int? Id { get; set; }

        public int? PartnerId { get; set; }

        public int? PaymentSystemId { get; set; }

        public string CurrencyId { get; set; }

        public int? Status { get; set; }

        public DateTime? CreatedFrom { get; set; }

        public DateTime? CreatedBefore { get; set; }
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public bool? AllowMultipleClientsPerPaymentInfo { get; set; }
        public bool? AllowMultiplePaymentInfoes { get; set; }
    }
}