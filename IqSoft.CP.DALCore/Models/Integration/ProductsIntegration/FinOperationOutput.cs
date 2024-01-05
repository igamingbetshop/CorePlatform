using System.Collections.Generic;

namespace IqSoft.CP.DAL.Models.Integration.ProductsIntegration
{
    public class FinOperationOutput : ResponseBase
    {
        public long TransactionId { get; set; }

        public List<FinOperationOutputItem> OperationItems { get; set; }
    }
}
