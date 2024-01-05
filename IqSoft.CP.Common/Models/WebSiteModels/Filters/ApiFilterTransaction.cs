using System;
using System.Collections.Generic;

namespace IqSoft.CP.Common.Models.WebSiteModels.Filters
{
    public class ApiFilterTransaction : ApiFilterBase
    {
        public int? Id { get; set; }

        public int? ObjectTypeId { get; set; }

        public int? ClientId { get; set; }

        public int? ObjectId { get; set; }

        public long? DocumentId { get; set; }
        
        public string CurrencyId { get; set; }

        public List<long> AccountIds { get; set; }

        public int? OperationTypeId { get; set; }

        public List<int> OperationTypeIds { get; set; }

        public DateTime CreatedFrom { get; set; }

        public DateTime CreatedBefore { get; set; }
    }
}