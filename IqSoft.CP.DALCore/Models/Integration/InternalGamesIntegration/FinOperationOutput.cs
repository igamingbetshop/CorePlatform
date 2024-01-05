using System.Collections.Generic;

namespace IqSoft.NGGP.DAL.Models.Integration.InternalGamesIntegration
{
    public class FinOperationOutput : OutputBase
    {
        public long TransactionId { get; set; }

        public List<FinOperationOutputItem> OperationItems { get; set; }
    }
}
