using System.Collections.Generic;

namespace IqSoft.CP.AdminWebApi.Models.AgentModels
{
    public class ApiAgentCommission
    {
        public int? Id { get; set; }
        public int AgentId { get; set; }
        public int ProductId { get; set; }
        public decimal? Percent { get; set; }
        public string TurnoverPercent { get; set; }
        public List<ApiTurnoverPercent> TurnoverPercentsList { get; set; }
    }

    public class ApiTurnoverPercent
    {
        public int FromCount { get; set; }
        public int ToCount { get; set; }
        public decimal Percent { get; set; }
    }
}