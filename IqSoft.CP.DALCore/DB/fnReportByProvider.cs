﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace IqSoft.CP.DAL
{
    public partial class fnReportByProvider
    {
        public string ProviderName { get; set; }
        public string Currency { get; set; }
        public int? TotalBetsCount { get; set; }
        public decimal? TotalBetsAmount { get; set; }
        public decimal? TotalWinsAmount { get; set; }
        public int? TotalUncalculatedBetsCount { get; set; }
        public decimal? TotalUncalculatedBetsAmount { get; set; }
        public decimal? GGR { get; set; }
        public int? PartnerId { get; set; }
        public int? ClientId { get; set; }
        public int? AffiliateReferralId { get; set; }
    }
}