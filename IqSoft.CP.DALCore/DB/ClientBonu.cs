﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace IqSoft.CP.DAL
{
    public partial class ClientBonu
    {
        public int Id { get; set; }
        public int BonusId { get; set; }
        public int ClientId { get; set; }
        public int Status { get; set; }
        public decimal BonusPrize { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime? AwardingTime { get; set; }
        public decimal? TurnoverAmountLeft { get; set; }
        public decimal? FinalAmount { get; set; }
        public DateTime? CalculationTime { get; set; }
        public DateTime? ValidUntil { get; set; }
        public int? TriggerId { get; set; }
        public long? CreationDate { get; set; }
        public int? ReuseNumber { get; set; }

        public virtual Bonu Bonus { get; set; }
        public virtual Client Client { get; set; }
        public virtual TriggerSetting Trigger { get; set; }
    }
}