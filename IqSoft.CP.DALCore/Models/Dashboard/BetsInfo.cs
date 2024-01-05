namespace IqSoft.CP.DAL.Models.Dashboard
{
    public class BetsInfo
    {
        public string CurrencyId { get; set; }

		public int DocumentState { get; set; }

        public int DeviceTypeId { get; set; }

        public decimal TotalBetsAmount { get; set; }

        public decimal TotalWinsAmount { get; set; }

        public decimal TotalBetsCount { get; set; }

        public decimal TotalPlayersCount { get; set; }

        public decimal TotalGGR { get; set; }

        public decimal TotalBetsFromWebSite { get; set; }

        public decimal TotalBetsCountFromWebSite { get; set; }

        public decimal TotalPlayersCountFromWebSite { get; set; }

        public decimal TotalGGRFromWebSite { get; set; }

        public decimal TotalBetsFromMobile { get; set; }

        public decimal TotalBetsCountFromMobile { get; set; }

        public decimal TotalPlayersCountFromMobile { get; set; }

        public decimal TotalGGRFromMobile { get; set; }

        public decimal TotalBetsFromWap { get; set; }

        public decimal TotalBetsCountFromWap { get; set; }

        public decimal TotalPlayersCountFromWap { get; set; }

        public decimal TotalGGRFromWap { get; set; }
    }
}