using System.Collections.Generic;

namespace IqSoft.CP.DAL.Models.Integration.ProductsIntegration
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

        public int? DeviceTypeId { get; set; }

        public int? TypeId { get; set; }

        public List<FinOperationInputItem> OperationItems { get; set; }
    }
}
