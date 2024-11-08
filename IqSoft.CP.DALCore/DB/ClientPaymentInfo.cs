﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace IqSoft.CP.DAL
{
    public partial class ClientPaymentInfo
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string AccountNickName { get; set; }
        public string ClientFullName { get; set; }
        public string CardNumber { get; set; }
        public DateTime? CardExpireDate { get; set; }
        public string BankName { get; set; }
        public string BankIBAN { get; set; }
        public string BranchName { get; set; }
        public string BankAccountNumber { get; set; }
        public int Type { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public string WalletNumber { get; set; }
        public int? PartnerPaymentSystemId { get; set; }
        public int? State { get; set; }

        public virtual Client Client { get; set; }
        public virtual PartnerPaymentSetting PartnerPaymentSystem { get; set; }
    }
}