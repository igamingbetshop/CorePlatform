﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace IqSoft.CP.DAL
{
    public partial class fnBetShopBetForDashboard
    {
        public string CurrencyId { get; set; }
        public int? ProviderId { get; set; }
        public decimal? TotalBetAmount { get; set; }
        public decimal? TotalWinAmount { get; set; }
        public int? TotalCount { get; set; }
        public int PartnerId { get; set; }
    }
}
