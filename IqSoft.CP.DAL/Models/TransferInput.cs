namespace IqSoft.CP.DAL.Models
{
   public class TransferInput
    {
        public decimal Amount { get; set; }
        public string CurrencyId { get; set; }
        public int? UserId { get; set; }
        public int? AffiliateId { get; set; }
        public int ObjectId { get; set; }
        public int ObjectTypeId { get; set; }
        public int? FromUserId { get; set; }
        public string Info { get; set; }
        public string ExternalTransactionId { get; set; }
        public int? ProductId { get; set; }
    }
}
