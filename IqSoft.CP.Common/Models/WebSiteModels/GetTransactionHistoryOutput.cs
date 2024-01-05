using System.Collections.Generic;

namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class GetTransactionHistoryOutput
    {
        public long Count { get; set; }
        public List<TransactionModel> Transactions { get; set; }
    }
}