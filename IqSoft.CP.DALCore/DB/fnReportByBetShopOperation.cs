﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace IqSoft.CP.DAL
{
    public partial class fnReportByBetShopOperation
    {
        public int Id { get; set; }
        public int PartnerId { get; set; }
        public int GroupId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public int? TotalPandingDepositsCount { get; set; }
        public decimal? TotalPandingDepositsAmount { get; set; }
        public int? TotalPayedDepositsCount { get; set; }
        public decimal? TotalPayedDepositsAmount { get; set; }
        public int? TotalCanceledDepositsCount { get; set; }
        public decimal? TotalCanceledDepositsAmount { get; set; }
        public int? TotalPandingWithdrawalsCount { get; set; }
        public decimal? TotalPandingWithdrawalsAmount { get; set; }
        public int? TotalPayedWithdrawalsCount { get; set; }
        public decimal? TotalPayedWithdrawalsAmount { get; set; }
        public int? TotalCanceledWithdrawalsCount { get; set; }
        public decimal? TotalCanceledWithdrawalsAmount { get; set; }
    }
}
