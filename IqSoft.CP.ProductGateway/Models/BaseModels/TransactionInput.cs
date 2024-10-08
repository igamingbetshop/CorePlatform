using IqSoft.CP.DAL.Models.Cache;

namespace IqSoft.CP.ProductGateway.Models.BaseModels
{
    public class TransactionInput
    {
        public BllClient Client { get; set; }
        public int ProviderId { get; set; }
        public string ProductExternalId { get; set; }
        public long SessionId { get; set; }
        public long SessionParentId { get; set; }
        public int SessionDeviceType { get; set; }
        public decimal Amount { get; set; }         
        public string TransactionId { get; set; }
        public string CreditTransactionId { get; set; }
        public bool ThrowDuplicateTransaction { get; set; } = true;
        public string RoundId { get; set; }
        public bool IsRoundClosed { get; set; } = false;
        public bool IsFreeSpin { get; set; } = false;
    }
}