using System;

namespace IqSoft.CP.AdminWebApi.Models.ReportModels.BetShop
{
    public class ApiShiftReportElement
    {
		public int Id { get; set; }

        public int BetShopId { get; set; }

		public int BetShopGroupId { get; set; }
		
		public string BetShopName { get; set; }

		public string BetShopGroupName { get; set; }

		public int CashdeskId { get; set; }

        public string CashdeskName { get; set; }

		public int CashierId { get; set; }

		public string FirstName { get; set; }

        public string LastName { get; set; }

		public decimal BetAmount { get; set; }

		public decimal PayedWinAmount { get; set; }

		public decimal DepositAmount { get; set; }

		public decimal WithdrawAmount { get; set; }

		public decimal DebitCorrectionAmount { get; set; }

		public decimal CreditCorrectionAmount { get; set; }

		public decimal EndAmount { get; set; }

		public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int ShiftNumber { get; set; }

        public string PartnerName { get; set; }

        public int OperationTypeId { get; set; }

        public decimal? BonusAmount { get; set; }
    }
}