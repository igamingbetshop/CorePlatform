﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace IqSoft.CP.DAL
{
    public partial class fnAgentTurnoverProfit
    {
        public int RecieverAgentId { get; set; }
        public string RecieverAgentPath { get; set; }
        public int AgentId { get; set; }
        public int ProductId { get; set; }
        public int? ProductGroupId { get; set; }
        public string ProductGroupName { get; set; }
        public int? SelectionsCount { get; set; }
        public decimal? TotalBetAmount { get; set; }
        public decimal? TotalWinAmount { get; set; }
        public string TurnoverPercent { get; set; }
    }
}
