namespace IqSoft.CP.AdminWebApi.Models.ReportModels
{
    public class ApiReportByPartner
    {
        public int PartnerId { get; set; }
        public string PartnerName { get; set; }
        public decimal TotalBetAmount { get; set; }
        public int TotalBetsCount { get; set; }
        public decimal TotalWinAmount { get; set; }
        public decimal TotalGGR { get; set; }
    }
}