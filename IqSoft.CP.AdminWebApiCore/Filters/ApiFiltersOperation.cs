using IqSoft.CP.Common.Models.Filters;
using System.Collections.Generic;

namespace IqSoft.CP.AdminWebApi.Filters
{
    public class ApiFiltersOperation
    {
        public List<ApiFiltersOperationType> ApiOperationTypeList { get; set; }
        public bool IsAnd { get; set; }
    }
}