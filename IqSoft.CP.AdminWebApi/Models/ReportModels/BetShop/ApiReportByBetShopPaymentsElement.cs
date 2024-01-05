namespace IqSoft.CP.AdminWebApi.Models.ReportModels.BetShop
{
    public class ApiReportByBetShopPaymentsElement
    {
        public int Id { get; set; }

		public int GroupId { get; set; }

		public string Name { get; set; }

        public string Address { get; set; }

        public int TotalPendingDepositsCount { get; set; }

        public decimal TotalPendingDepositsAmount { get; set; }

        public int TotalPayedDepositsCount { get; set; }

        public decimal TotalPayedDepositsAmount { get; set; }

        public int TotalCanceledDepositsCount { get; set; }

        public decimal TotalCanceledDepositsAmount { get; set; }

        public int TotalPendingWithdrawalsCount { get; set; }

        public decimal TotalPendingWithdrawalsAmount { get; set; }

        public int TotalPayedWithdrawalsCount { get; set; }

        public decimal TotalPayedWithdrawalsAmount { get; set; }

        public int TotalCanceledWithdrawalsCount { get; set; }

        public decimal TotalCanceledWithdrawalsAmount { get; set; }
    }
}
