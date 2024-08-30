namespace IqSoft.CP.DAL.Models
{
    public class ClientCorrectionInput
    {
        public decimal Amount { get; set; }
        public long? AccountId { get; set; }
        public int? AccountTypeId { get; set; }
        public string CurrencyId { get; set; }
        public int ClientId { get; set; }
        public long? ExternalOperationId { get; set; }
        public string ExternalTransactionId { get; set; }
        public string Info { get; set; }
        public int OperationTypeId { get; set; }
        public int? ProductId { get; set; }
        public bool IsFromAgent { get; set; } 
    }
}
