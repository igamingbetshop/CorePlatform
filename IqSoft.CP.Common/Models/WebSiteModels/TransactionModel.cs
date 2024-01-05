using System;

namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class TransactionModel
    {
        public long Id { get; set; }
        
        public long AccountId { get; set; }
        
        public decimal Amount { get; set; }
        
        public int Type { get; set; }
        
        public long DocumentId { get; set; }
        
        public int OperationTypeId { get; set; }
        
        public DateTime CreationTime { get; set; }
        
        public string Info { get; set; }
        
        public int? PartnerPaymentSettingId { get; set; }
        
        public long? PaymentRequestId { get; set; }
        
        public int DocumentState { get; set; }
        
        public int? ClientId { get; set; }
        
        public int? GameProviderId { get; set; }
        
        public string OperationTypeName { get; set; }
        
        public string PaymentSystemName { get; set; }
        
        public string ProductName { get; set; }
        
        public string GameProviderName { get; set; }
        
        public string CurrencyId { get; set; }

        public string AccountTypeName { get; set; }

        public decimal BalanceBefore { get; set; }

        public decimal BalanceAfter { get; set; }
    }
}