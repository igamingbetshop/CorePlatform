using System;

namespace IqSoft.CP.AdminWebApi.Filters
{
    public class ApiFilterPartner : ApiFilterBase
    {
        public int? Id { get; set; }

        public string Name { get; set; }

        public string CurrencyId { get; set; }

        public int? State { get; set; }

        public string AdminSiteUrl { get; set; }

        public DateTime? CreatedFrom { get; set; }

        public DateTime? CreatedBefore { get; set; }

        public string SiteUrl { get; set; }

        public System.DateTime LastUpdateTime { get; set; }

        public Nullable<System.TimeSpan> AccountingDayStartTime { get; set; }

        public Nullable<int> ClientMinAge { get; set; }

        public string PasswordRegExp { get; set; }

        public Nullable<int> VerificationType { get; set; }

        public Nullable<int> EmailVerificationCodeLength { get; set; }

        public Nullable<int> MobileVerificationCodeLength { get; set; }

        public Nullable<decimal> UnusedAmountWithdrawPercent { get; set; }

        public Nullable<int> UserSessionExpireTime { get; set; }

        public Nullable<int> UnpaidWinValidPeriod { get; set; }

        public Nullable<int> VerificationKeyActiveMinutes { get; set; }

        public Nullable<decimal> AutoApproveBetShopDepositMaxAmount { get; set; }

        public Nullable<int> ClientSessionExpireTime { get; set; }
    }
}