using System;
using System.Collections.Generic;

namespace IqSoft.NGGP.WebApplications.AdminWebApi.Filters
{
    public class ApiFilterPaymentRequest : ApiFilterBase
    {
        public long? Id { get; set; }

        public int? ClientId { get; set; }

        public int? PaymentSystemId { get; set; }

        public string CurrencyId { get; set; }
        
        public List<int> Statuses { get; set; }

        public int? BetShopId { get; set; }
        
        public DateTime? CreatedFrom { get; set; }

        public DateTime? CreatedBefore { get; set; }
    }
}