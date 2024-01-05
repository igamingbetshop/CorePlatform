namespace IqSoft.CP.DAL.Filters.Agent
{
    public class AgentReportByProduct
    {
        public int? AgentId { get; set; }
        public int? ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal TotalBetsAmount { get; set; }
        public decimal TotalWinsAmount { get; set; }
        public int TotalBetsCount { get; set; }
        public decimal GGR { get; set; }
        public decimal Profit { get; set; }
        public decimal? TurnoverProfit { get; set; }
    }
}
