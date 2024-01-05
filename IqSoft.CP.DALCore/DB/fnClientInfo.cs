﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace IqSoft.CP.DAL
{
    public partial class fnClientInfo
    {
        public int? Id { get; set; }
        public string UserName { get; set; }
        public int? CategoryId { get; set; }
        public string CurrencyId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public int? Status { get; set; }
        public decimal? Balance { get; set; }
        public decimal? WithdrawableBalance { get; set; }
        public decimal? TotalBetsAmount { get; set; }
        public decimal? GGR { get; set; }
        public int? TotalDepositsCount { get; set; }
        public decimal? TotalDepositsAmount { get; set; }
        public int? TotalWithdrawalsCount { get; set; }
        public decimal? TotalWithdrawalsAmount { get; set; }
        public int? FailedDepositsCount { get; set; }
        public decimal? FailedDepositsAmount { get; set; }
    }
}