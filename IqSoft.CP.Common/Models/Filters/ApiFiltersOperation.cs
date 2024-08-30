using System.Collections.Generic;

namespace IqSoft.CP.Common.Models.Filters
{
    public class ApiFiltersOperation
    {
        public List<ApiFiltersOperationType> ApiOperationTypeList { get; set; }
        public bool IsAnd { get; set; }
    }
}