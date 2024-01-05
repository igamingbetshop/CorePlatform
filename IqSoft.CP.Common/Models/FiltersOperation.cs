using System.Collections.Generic;
using System.Linq;

namespace IqSoft.CP.Common.Models
{
    public class FiltersOperation
    {
        public FiltersOperation()
        {
            OperationTypeList = new List<FiltersOperationType>();
        }

        public List<FiltersOperationType> OperationTypeList { get; set; }

        public bool IsAnd { get; set; }

        public FiltersOperation Copy()
        {
            return new FiltersOperation
            {
                IsAnd = IsAnd,
                OperationTypeList = OperationTypeList?.Select(x => x.Copy()).ToList()
            };
        }
    }
}
