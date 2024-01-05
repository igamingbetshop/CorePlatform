namespace IqSoft.CP.AdminWebApi.Models.ReportModels
{
    public class ApiReportByPaymentSystem
    {
        public int PartnerId { get; set; }
        public string PartnerName { get; set; }
        public int PaymentSystemId { get; set; }
        public string PaymentSystemName { get; set; }
        public int Status { get; set; }
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
    }
}