﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace IqSoft.CP.DAL
{
    public partial class fnReportByPaymentSystem
    {
        public int PartnerId { get; set; }
        public string PartnerName { get; set; }
        public int PaymentSystemId { get; set; }
        public string PaymentSystemName { get; set; }
        public int Status { get; set; }
        public int? Count { get; set; }
        public decimal? TotalAmount { get; set; }
    }
}
