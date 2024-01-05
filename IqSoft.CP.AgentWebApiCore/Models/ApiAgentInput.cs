using IqSoft.CP.AgentWebApi.Filters;

namespace IqSoft.CP.AgentWebApi.Models
{
    public class ApiAgentInput : ApiFilterBase
    {
        public int? Id { get; set; }
        public string AgentIdentifier { get; set; }
        public int? ParentId { get; set; }
        public int? Level { get; set; }
        public int? Type { get; set; }
        public int? State { get; set; }
        public bool? AllowDoubleCommission { get; set; }
        public bool? WithClients { get; set; }
        public bool? IsFromSuspend { get; set; }
        public string CurrencyId { get; set; }
        public bool WithDownlines { get; set; }
    }
}