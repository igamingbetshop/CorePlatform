﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace IqSoft.CP.DAL
{
    public partial class BetShop
    {
        public BetShop()
        {
            BetShopReconings = new HashSet<BetShopReconing>();
            CashDesks = new HashSet<CashDesk>();
            PaymentRequests = new HashSet<PaymentRequest>();
        }

        public int Id { get; set; }
        public int GroupId { get; set; }
        public int Type { get; set; }
        public string Name { get; set; }
        public string CurrencyId { get; set; }
        public string Address { get; set; }
        public int RegionId { get; set; }
        public int PartnerId { get; set; }
        public int State { get; set; }
        public int DailyTicketNumber { get; set; }
        public decimal DefaultLimit { get; set; }
        public decimal CurrentLimit { get; set; }
        public long SessionId { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public decimal? BonusPercent { get; set; }
        public bool PrintLogo { get; set; }
        public string Ips { get; set; }

        public virtual Currency Currency { get; set; }
        public virtual BetShopGroup Group { get; set; }
        public virtual Partner Partner { get; set; }
        public virtual UserSession Session { get; set; }
        public virtual ICollection<BetShopReconing> BetShopReconings { get; set; }
        public virtual ICollection<CashDesk> CashDesks { get; set; }
        public virtual ICollection<PaymentRequest> PaymentRequests { get; set; }
    }
}