using System;

namespace IqSoft.CP.Common.Models.WebSiteModels.Filters
{
    public class ApiFilterClient : ApiFilterBase
    {
        public int? Id { get; set; }

        public string Email { get; set; }

        public string UserName { get; set; }

        public string CurrencyId { get; set; }

        public int? Gender { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string DocumentNumber { get; set; }

        public string DocumentIssuedBy { get; set; }

        public string Address { get; set; }

        public string MobileNumber { get; set; }

        public string AffiliateId { get; set; }

        public DateTime? CreatedFrom { get; set; }

        public DateTime? CreatedBefore { get; set; }
    }
}