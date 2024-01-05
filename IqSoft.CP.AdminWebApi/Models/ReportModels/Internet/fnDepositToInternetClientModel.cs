﻿using System;

namespace IqSoft.CP.AdminWebApi.Models.ReportModels.Internet
{
    public class FnDepositToInternetClientModel
    {
        public long Id { get; set; }
        public string ExternalTransactionId { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyId { get; set; }
        public int State { get; set; }
        public int OperationTypeId { get; set; }
        public long? ParentId { get; set; }
        public long? PaymentRequestId { get; set; }
        public string Info { get; set; }
        public int? Creator { get; set; }
        public int? CashDeskId { get; set; }
        public int? PartnerPaymentSettingId { get; set; }
        public int? PartnerProductId { get; set; }
        public int? GameProviderId { get; set; }
        public int? ClientId { get; set; }
        public long? ExternalOperationId { get; set; }
        public long? TicketNumber { get; set; }
        public string TicketInfo { get; set; }
        public int? UserId { get; set; }
        public long SessionId { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public int PartnerId { get; set; }
        public string ClientLastName { get; set; }
        public string ClientFirstName { get; set; }
        public string ClientUserName { get; set; }
        public string PaymentSystemName { get; set; }
        public int PaymentSystemId { get; set; }
        public int? BetShopId { get; set; }
        public string BetShopName { get; set; }
    }
}
