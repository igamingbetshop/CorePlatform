namespace IqSoft.CP.AdminWebApi.Models.UserModels
{
    public partial class ApiUserSettings
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public bool? AllowAutoPT { get; set; }
        public string CalculationPeriod { get; set; }
        public decimal? AgentMaxCredit { get; set; }
        public System.DateTime CreationTime { get; set; }
        public System.DateTime LastUpdateTime { get; set; }
        public bool AllowOutright { get; set; }
        public bool AllowDoubleCommission { get; set; }
        public string LevelLimits { get; set; }
        public string CountLimits { get; set; }
        public int? ParentState { get; set; }
        public string Comment { get; set; }
        public int? OddsType { get; set; }
    }
}