using System.Collections.Generic;

namespace IqSoft.CP.AgentWebApi.Models.User
{
    public class ApiAgentCommission
    {
        public int? Id { get; set; }
        public int? AgentId { get; set; }
        public int ProductId { get; set; }
        public decimal? Percent { get; set; }
        public string TurnoverPercent { get; set; }
        public int? ClientId { get; set; }
        public int? ClientGroup { get; set; }
        public string CurrencyId { get; set; }
        public List<ApiTurnoverPercent> TurnoverPercentsList { get; set; }
    }

    public class ApiTurnoverPercent
    {
        public int FromCount { get; set; }
        public int ToCount { get; set; }
        public decimal Percent { get; set; }
    }
}