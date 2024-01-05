using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.AgentWebApi.Models.Downline
{
    public class LevelLimit
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal? MinLimit { get; set; }
        public decimal? Limit { get; set; }
        public decimal? CurrentLimit { get; set; }
    }
}