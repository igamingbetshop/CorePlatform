using IqSoft.CP.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.AdminWebApi.Filters.Reporting
{
    public class ApiFilterReportByBetShopPayment : ApiFilterBase
    {
        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public ApiFiltersOperation BetShopIds { get; set; }

		public ApiFiltersOperation GroupIds { get; set; }

		public ApiFiltersOperation BetShopNames { get; set; }

        public ApiFiltersOperation TotalPendingDepositsCounts { get; set; }

        public ApiFiltersOperation TotalPendingDepositsAmounts { get; set; }

        public ApiFiltersOperation TotalPayedDepositsCounts { get; set; }

        public ApiFiltersOperation TotalPayedDepositsAmounts { get; set; }

        public ApiFiltersOperation TotalCanceledDepositsCounts { get; set; }

        public ApiFiltersOperation TotalCanceledDepositsAmounts { get; set; }

        public ApiFiltersOperation TotalPendingWithdrawalsCounts { get; set; }

        public ApiFiltersOperation TotalPendingWithdrawalsAmounts { get; set; }

        public ApiFiltersOperation TotalPayedWithdrawalsCounts { get; set; }

        public ApiFiltersOperation TotalPayedWithdrawalsAmounts { get; set; }

        public ApiFiltersOperation TotalCanceledWithdrawalsCounts { get; set; }

        public ApiFiltersOperation TotalCanceledWithdrawalsAmounts { get; set; }
    }
}