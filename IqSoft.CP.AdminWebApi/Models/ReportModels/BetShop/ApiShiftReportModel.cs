using IqSoft.CP.Common.Models;

namespace IqSoft.CP.AdminWebApi.Models.ReportModels.BetShop
{
    public class ApiShiftReportModel : PagedModel<ApiShiftReportElement>
    {
        public decimal? TotalAmount { get; set; }

        public decimal? TotalBonusAmount { get; set; }

        public decimal? TotalBetAmount { get; set; }

        public decimal? TotalPayedWinAmount { get; set; }

        public decimal? TotalDepositAmount { get; set; }

        public decimal? TotalWithdrawAmount { get; set; }

        public decimal? TotalDebitCorrectionAmount { get; set; }

        public decimal? TotalCreditCorrectionAmount { get; set; }
    }
}