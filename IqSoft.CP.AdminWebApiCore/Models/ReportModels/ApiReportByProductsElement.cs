namespace IqSoft.CP.AdminWebApi.Models.ReportModels
{
    public class ApiReportByProductsElement
    {
        public int ClientId { get; set; }

        public string ClientFirstName { get; set; }

        public string ClientLastName { get; set; }

        public string Currency { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; }

        public int DeviceTypeId { get; set; }

        public string ProviderName { get; set; }

        public decimal TotalBetsAmount { get; set; }

        public decimal TotalWinsAmount { get; set; }

        public int TotalBetsCount { get; set; }

        public int TotalUncalculatedBetsCount { get; set; }

        public decimal TotalUncalculatedBetsAmount { get; set; }

        public decimal GGR { get; set; }
    }
}
