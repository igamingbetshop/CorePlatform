using System.Collections.Generic;

namespace IqSoft.CP.AgentWebApi.Filters
{
    public class ApiFiltersOperation
    {
        public List<ApiFiltersOperationType> ApiOperationTypeList { get; set; }
        public bool IsAnd { get; set; }
    }
}