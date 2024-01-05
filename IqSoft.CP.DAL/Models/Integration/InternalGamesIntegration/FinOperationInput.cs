using System.Collections.Generic;

namespace IqSoft.NGGP.DAL.Models.Integration.InternalGamesIntegration
{
    public class FinOperationInput : InputBase
    {
        public string CurrencyId { get; set; }
        public string RoundId { get; set; }
        public string GameId { get; set; }
        public string Info { get; set; }
        public int? TransactionTypeId { get; set; }
        public string TransactionId { get; set; }
        public string CreditTransactionId { get; set; }
        public List<FinOperationInputItem> OperationItems { get; set; }
    }
}
