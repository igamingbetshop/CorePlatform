using System;
using System.Collections.Generic;

namespace IqSoft.CP.Common.Models.Filters
{
    public class ApiFiltersOperationType
    {
        public int OperationTypeId { get; set; }

        public string StringValue { get; set; }

        public long IntValue { get; set; }

        public decimal DecimalValue { get; set; }

        public DateTime DateTimeValue { get; set; }

        public List<int> ArrayValue { get; set; }
    }
}