namespace IqSoft.CP.AdminWebApi.Models.ReportModels
{
    public class ApiReportByProvidersElement
    {
        public int PartnerId { get; set; }

        public string ProviderName { get; set; }

        public string Currency { get; set; }

        public int TotalBetsCount { get; set; }

        public decimal TotalBetsAmount { get; set; }

        public decimal TotalWinsAmount { get; set; }

        public int TotalUncalculatedBetsCount { get; set; }

        public decimal TotalUncalculatedBetsAmount { get; set; }

        public decimal GGR { get; set; }

        public decimal BetsCountPercent { get; set; }

        public decimal BetsAmountPercent { get; set; }

        public decimal GGRPercent { get; set; }
    }
}
