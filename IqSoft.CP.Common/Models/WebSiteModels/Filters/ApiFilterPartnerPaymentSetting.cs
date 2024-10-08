using System;

namespace IqSoft.CP.Common.Models.WebSiteModels.Filters
{
    public class ApiFilterPartnerPaymentSetting : ApiFilterBase
    {
        public int? Id { get; set; }

		public int ClientId { get; set; }

        public int? PaymentSystemId { get; set; }

        public string CurrencyId { get; set; }

        public int? Status { get; set; }

        public DateTime? CreatedFrom { get; set; }

        public DateTime? CreatedBefore { get; set; }
    }
}