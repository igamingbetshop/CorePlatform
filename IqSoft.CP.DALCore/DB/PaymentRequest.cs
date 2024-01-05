﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace IqSoft.CP.DAL
{
    public partial class PaymentRequest
    {
        public PaymentRequest()
        {
            PaymentRequestHistories = new HashSet<PaymentRequestHistory>();
            PaymentRequestTransfers = new HashSet<PaymentRequestTransfer>();
        }

        public long Id { get; set; }
        public int ClientId { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyId { get; set; }
        public int Status { get; set; }
        public int? PartnerPaymentSettingId { get; set; }
        public int? BetShopId { get; set; }
        public int? CashDeskId { get; set; }
        public int? CashierId { get; set; }
        public string Parameters { get; set; }
        public int PaymentSystemId { get; set; }
        public string Info { get; set; }
        public int Type { get; set; }
        public string ExternalTransactionId { get; set; }
        public int? UserId { get; set; }
        public long? SessionId { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public string CashCode { get; set; }
        public long? Date { get; set; }
        public int? ActivatedBonusType { get; set; }
        public decimal? CommissionAmount { get; set; }
        public string CardNumber { get; set; }
        public int? SegmentId { get; set; }
        public string CountryCode { get; set; }

        public virtual BetShop BetShop { get; set; }
        public virtual CashDesk CashDesk { get; set; }
        public virtual Client Client { get; set; }
        public virtual Currency Currency { get; set; }
        public virtual PartnerPaymentSetting PartnerPaymentSetting { get; set; }
        public virtual PaymentSystem PaymentSystem { get; set; }
        public virtual User User { get; set; }
        public virtual ICollection<PaymentRequestHistory> PaymentRequestHistories { get; set; }
        public virtual ICollection<PaymentRequestTransfer> PaymentRequestTransfers { get; set; }
    }
}