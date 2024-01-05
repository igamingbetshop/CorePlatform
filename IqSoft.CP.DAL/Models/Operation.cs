using System.Collections.Generic;

namespace IqSoft.CP.DAL.Models
{
    public class Operation
    {
        public int? BonusId { get; set; }

        public long? ReuseNumber { get; set; }

        public bool? FreezeBonusBalance { get; set; }

        public decimal Amount { get; set; }

        public string CurrencyId { get; set; }

        public string ExternalTransactionId { get; set; }

        public long? ExternalOperationId { get; set; }

        public long? TicketNumber { get; set; }

        public string TicketInfo { get; set; }

        public int? CashDeskId { get; set; }

        public long? PaymentRequestId { get; set; }

        public int? State { get; set; }

        public long? ParentId { get; set; }

        public int? ClientId { get; set; }

        public int? UserId { get; set; }

        public int? Creator { get; set; }

        public int? PartnerProductId { get; set; }

        public int? ProductId { get; set; }

        public int? GameProviderId { get; set; }

        public int? PartnerPaymentSettingId { get; set; }

        public int Type { get; set; }

        public string Info { get; set; }

        public int? DeviceTypeId { get; set; }

        public int? DocumentTypeId { get; set; }

        public decimal? PossibleWin { get; set; }

        public string RoundId { get; set; }

        public long? SessionId { get; set; }

        public List<OperationItem> OperationItems { get; set; }
    }
}
