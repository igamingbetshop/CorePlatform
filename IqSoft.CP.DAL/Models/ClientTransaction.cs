namespace IqSoft.CP.DAL.Models
{
    public class ClientOperation
    {
        public int? ClientId { get; set; }

        public decimal Amount { get; set; }

        public string ExternalTransactionId { get; set; }

        public long? ExternalOperationId { get; set; }

        public int? ProductId { get; set; }

        public int? PartnerProductId { get; set; }

        public int OperationTypeId { get; set; }

        public int? State { get; set; }

        public int? GameProviderId { get; set; }

        public int? PaymentSettingId { get; set; }

        public long? ParentDocumentId { get; set; }

        public int PartnerId { get; set; }

        public int CashDeskId { get; set; }

        public string CurrencyId { get; set; }

        public string Info { get; set; }

        public int? DeviceTypeId { get; set; }

        public int? TypeId { get; set; }

        public decimal? PossibleWin { get; set; }

        public string RoundId { get; set; }

        public long? AccountId { get; set; }

        public int AccountTypeId { get; set; }

        public int? SelectionsCount { get; set; }
        public int? Creator { get; set; }
        public int? UserId { get; set; }
    }
}
