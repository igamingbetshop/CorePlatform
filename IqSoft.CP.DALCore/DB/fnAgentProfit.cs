﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace IqSoft.CP.DAL
{
    public partial class fnAgentProfit
    {
        public int? ProductGroupId { get; set; }
        public string ProductGroupName { get; set; }
        public int ProductId { get; set; }
        public int RecieverAgentId { get; set; }
        public int AgentId { get; set; }
        public decimal? TotalProfit { get; set; }
        public decimal? TotalBetAmount { get; set; }
        public decimal? TotalWinAmount { get; set; }
    }
}