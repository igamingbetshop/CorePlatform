﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace IqSoft.CP.DAL
{
    public partial class fnAgentProfitReport
    {
        public int? FromAgentId { get; set; }
        public int? ToAgentId { get; set; }
        public int? ProductGroupId { get; set; }
        public decimal? TotalTurnoverProfit { get; set; }
        public decimal? TotalGGRProfit { get; set; }
        public long? CreationDate { get; set; }
    }
}