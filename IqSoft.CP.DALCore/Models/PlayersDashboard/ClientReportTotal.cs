namespace IqSoft.CP.DAL.Models.PlayersDashboard
{
    public class ClientReportTotal
    {
        public decimal TotalWithdrawalsAmount { get; set; }
        public decimal TotalDepositsAmount { get; set; }
        public decimal TotalBetsAmount { get; set; }
        public decimal TotalWinsAmount { get; set; }
        public decimal TotalGGRs { get; set; }
        public decimal TotalNGRs { get; set; }
        public decimal TotalDebitCorrections { get; set; }
        public decimal TotalCreditCorrections { get; set; }
    }
}
