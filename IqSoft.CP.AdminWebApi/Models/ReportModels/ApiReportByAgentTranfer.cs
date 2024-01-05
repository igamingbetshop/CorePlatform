namespace IqSoft.CP.AdminWebApi.Models.ReportModels
{
    public class ApiReportByAgentTranfer
    {
        public int PartnerId { get; set; }
        public string PartnerName { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string NickName { get; set; }
        public decimal TotalProfit { get; set; }
        public decimal TotalDebit { get; set; }
        public decimal Balance { get; set; }
    }
}