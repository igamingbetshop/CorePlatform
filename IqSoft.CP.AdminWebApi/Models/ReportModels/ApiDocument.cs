using System;

namespace IqSoft.CP.AdminWebApi.Models.ReportModels
{
    public class ApiDocument
    {
        public long Id { get; set; }
        public int ClientId { get; set; }
        public decimal Amount { get; set; }
        public decimal ConvertedAmount { get; set; }
        public string CurrencyId { get; set; }
        public string ExternalTransactionId { get; set; }
        public string RoundId { get; set; }
        public int State { get; set; }
        public int OperationTypeId { get; set; }
        public string OperationType{ get; set; }
        public int? TypeId { get; set; }
        public string TypeName { get; set; }
        public long? PaymentRequestId { get; set; }
        public int? PaymentSystemId { get; set; }
        public string PaymentSystemName { get; set; }
        public int? GameProviderId { get; set; }
        public string GameProviderName { get; set; }
        public int? ProductId { get; set; }
        public string ProductName { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
    }
}