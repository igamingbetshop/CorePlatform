using System;
using System.Collections.Generic;

namespace IqSoft.CP.Common.Models.WebSiteModels.Filters
{
    public class ApiFilterPaymentRequest : ApiFilterBase
    {
        public long? Id { get; set; }

        public int ClientId { get; set; }

        public int? PartnerPaymentSettingId { get; set; }

        public int? PaymentSystemId { get; set; }

        public int? Type { get; set; }

        public string CurrencyId { get; set; }

        public List<int> Statuses { get; set; }

        public int? BetShopId { get; set; }

        public DateTime? CreatedFrom { get; set; }

        public DateTime? CreatedBefore { get; set; }
    }
}