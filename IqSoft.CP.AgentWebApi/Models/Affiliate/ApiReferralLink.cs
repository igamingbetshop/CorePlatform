using System;
using System.Collections.Generic;

namespace IqSoft.CP.AgentWebApi.Models.Affiliate
{
    public class ApiReferralLink
    {
        public int Id { get; set; }

        public string Url { get; set; }

        public int Status { get; set; }

        public DateTime CreationTime { get; set; }

        public DateTime? LastCalculationTime { get; set; }
    }
}