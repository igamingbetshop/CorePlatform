using System;

namespace IqSoft.CP.Common.Models.WebSiteModels.Filters
{
    public class ApiFilterClientMessage : ApiFilterBase
    {
        public long? Id { get; set; }

        public int? ClientId { get; set; }

        public long? ParentId { get; set; }

        public int? Type { get; set; }

        public int? State { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }
    }
}