﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace IqSoft.CP.DAL
{
    public partial class fnFirstTimePaymentRequest
    {
        public long Id { get; set; }
        public int PartnerId { get; set; }
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
        public long? SessionId { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public string PaymentSystemName { get; set; }
    }
}
