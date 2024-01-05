using System;

namespace IqSoft.CP.AdminWebApi.Filters
{
    public class ApiFilterCashDesk : ApiFilterBase
    {
        public int? Id { get; set; }

        public int? BetShopId { get; set; }

        public string Name { get; set; }

        public DateTime? CreatedFrom { get; set; }

        public DateTime? CreatedBefore { get; set; }
    }
}
