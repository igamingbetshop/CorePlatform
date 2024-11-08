﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace IqSoft.CP.DAL
{
    public partial class CashDeskShift
    {
        public int Id { get; set; }
        public int CashierId { get; set; }
        public int CashDeskId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public decimal StartAmount { get; set; }
        public decimal? EndAmount { get; set; }
        public decimal BetAmount { get; set; }
        public decimal PayedWinAmount { get; set; }
        public decimal DepositAmount { get; set; }
        public decimal WithdrawAmount { get; set; }
        public decimal DebitCorrectionAmount { get; set; }
        public decimal CreditCorrectionAmount { get; set; }
        public decimal BonusAmount { get; set; }
        public int State { get; set; }
        public int? Number { get; set; }

        public virtual CashDesk CashDesk { get; set; }
        public virtual User Cashier { get; set; }
    }
}