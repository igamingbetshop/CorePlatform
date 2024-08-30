using IqSoft.CP.Common.Models.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.AdminWebApi.Filters.Reporting
{
    public class ApiFilterShiftReport : ApiFilterBase
    {
		public int? PartnerId { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

		public ApiFiltersOperation Ids { get; set; }

		public ApiFiltersOperation BetShopIds { get; set; }

		public ApiFiltersOperation BetShopGroupIds { get; set; }

		public ApiFiltersOperation BetShopNames { get; set; }

		public ApiFiltersOperation BetShopGroupNames { get; set; }

		public ApiFiltersOperation CashdeskIds { get; set; }

		public ApiFiltersOperation CashierIds { get; set; }

		public ApiFiltersOperation FirstNames { get; set; }

		public ApiFiltersOperation EndAmounts { get; set; }

		public ApiFiltersOperation BetAmounts { get; set; }

		public ApiFiltersOperation PayedWinAmounts { get; set; }

		public ApiFiltersOperation DepositAmounts { get; set; }

		public ApiFiltersOperation WithdrawAmounts { get; set; }

		public ApiFiltersOperation DebitCorrectionAmounts { get; set; }

		public ApiFiltersOperation CreditCorrectionAmounts { get; set; }
		
        public ApiFiltersOperation StartDates { get; set; }

        public ApiFiltersOperation EndDates { get; set; }

        public ApiFiltersOperation ShiftNumbers { get; set; }

        public ApiFiltersOperation PartnerIds { get; set; }

        public ApiFiltersOperation BonusAmounts { get; set; }
    }
}