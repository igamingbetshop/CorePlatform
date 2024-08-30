using IqSoft.CP.Common;
using IqSoft.CP.Common.Models.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.AdminWebApi.Filters.Reporting
{
    public class ApiFilterReportByClientsDetails : ApiFilterBase
    {
        public ApiFiltersOperation Ids { get; set; }

        public ApiFiltersOperation FirstNames { get; set; }

        public ApiFiltersOperation LastNames { get; set; }

        public ApiFiltersOperation RegionIds { get; set; }

        public ApiFiltersOperation Statuses { get; set; }

        public ApiFiltersOperation Emails { get; set; }

        public ApiFiltersOperation LanguageIds { get; set; }

        public ApiFiltersOperation Genders { get; set; }

        public ApiFiltersOperation Phones { get; set; }

        public ApiFiltersOperation Mobiles { get; set; }

        public ApiFiltersOperation ZipCodes { get; set; }

        public ApiFiltersOperation Currencies { get; set; }

        public ApiFiltersOperation RegistrationIps { get; set; }

        public ApiFiltersOperation DocumentNumbers { get; set; }

        public ApiFiltersOperation IsDocumentVerifyeds { get; set; }

        public ApiFiltersOperation SendSmses { get; set; }

        public ApiFiltersOperation SendMails { get; set; }

        public ApiFiltersOperation SendPromotions { get; set; }

        public ApiFiltersOperation RegistrationDates { get; set; }

        public ApiFiltersOperation BirthDates { get; set; }

        public ApiFiltersOperation Balances { get; set; }

        public ApiFiltersOperation TotalBetsAmounts { get; set; }

        public ApiFiltersOperation TotalDepositsAmounts { get; set; }

        public ApiFiltersOperation TotalWithdrawalsAmounts { get; set; }

        public ApiFiltersOperation TotalDepositsCounts { get; set; }

        public ApiFiltersOperation FirstDepositDates { get; set; }

        public ApiFiltersOperation LastDepositDates { get; set; }

        public ApiFiltersOperation LastDepositAmounts { get; set; }

        public ApiFiltersOperation PandingWithdrawalsCounts { get; set; }

        public ApiFiltersOperation WithdrawableBalances { get; set; }
    }
}