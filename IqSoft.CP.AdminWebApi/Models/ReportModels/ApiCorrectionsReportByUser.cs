namespace IqSoft.CP.AdminWebApi.Models.ReportModels
{
    public class ApiCorrectionsReportByUser
    {
        public int PartnerId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public decimal TotalCost { get; set; }
        public decimal CurrentBalance { get; set; }
    }
}