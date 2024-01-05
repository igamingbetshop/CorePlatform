using System.Collections.Generic;

namespace IqSoft.CP.AgentWebApi.Models.User
{
    public class ApiAgentSettings
    {
        public int ObjectId { get; set; }
        public List<int> CalculationPeriod { get; set; }
        public bool? AllowOutright { get; set; }
        public bool? AllowAutoPT { get; set; }
        public bool? AllowDoubleCommission { get; set; }

    }
}