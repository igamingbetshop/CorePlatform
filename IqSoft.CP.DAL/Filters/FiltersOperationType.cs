using System;

namespace IqSoft.CP.DAL.Filters
{
    public class FiltersOperationType
    {
        public int OperationTypeId { get; set; }

        public string StringValue { get; set; }

        public long IntValue { get; set; }

        public decimal DecimalValue { get; set; }

        public DateTime DateTimeValue { get; set; }

        public FiltersOperationType Copy()
        {
            return new FiltersOperationType
            {
                OperationTypeId = OperationTypeId,
                StringValue = StringValue,
                IntValue = IntValue,
                DecimalValue = DecimalValue,
                DateTimeValue = DateTimeValue
            };
        }
    }
}
