using System;

namespace IqSoft.CP.AutomationTest.Models
{
    public class ApiFiltersOperationType
    {
        public int OperationTypeId { get; set; }

        public string StringValue { get; set; }

        public long IntValue { get; set; }

        public decimal DecimalValue { get; set; }

        public DateTime DateTimeValue { get; set; }
    }
}