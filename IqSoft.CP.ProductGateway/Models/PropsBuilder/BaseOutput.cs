using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.ProductGateway.Models.PropsBuilder
{
    public class BaseOutput
    {
        [JsonProperty(PropertyName = "balance")]
        public decimal Balance { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "timeout")]
        public int TimeOut { get; set; }

        [JsonProperty(PropertyName = "agentInfo")]
        public List<AgentInfo> AgentsInfo  { get; set; }

        [JsonProperty(PropertyName = "error")]
        public string ErrorDescription { get; set; }
    }

    public class AgentInfo
    {
        [JsonProperty(PropertyName = "agentId")]
        public string AgentId { get; set; }

        [JsonProperty(PropertyName = "masterAgentId")]
        public string MasterAgentId { get; set; }

        [JsonProperty(PropertyName = "level")]
        public int Level { get; set; }
    }
}
