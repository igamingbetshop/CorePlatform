using System.Collections.Generic;

namespace IqSoft.CP.AutomationTest.Models
{
    public class ApiFiltersOperation
    {
        public List<ApiFiltersOperationType> ApiOperationTypeList { get; set; }
        public bool IsAnd { get; set; }
    }
}