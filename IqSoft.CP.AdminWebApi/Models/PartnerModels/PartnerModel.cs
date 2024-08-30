using System;
using IqSoft.CP.Common.Attributes;
using IqSoft.CP.Common.Models;

namespace IqSoft.CP.AdminWebApi.Models.PartnerModels
{
    public class PartnerModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string CurrencyId { get; set; }

        public string SiteUrl { get; set; }

        public int State { get; set; }

        [NotExcelProperty]
        public string StateName { get; set; }

        public DateTime CreationTime { get; set; }

        public DateTime LastUpdateTime { get; set; }

        public string AdminSiteUrl { get; set; }

        [NotExcelProperty]
        public TimeSpan AccountingDayStartTime { get; set; }

        [NotExcelProperty]
        public int ClientMinAge { get; set; }

        [NotExcelProperty]
        public string PasswordRegExp { get; set; }

        [NotExcelProperty]
        public int VerificationType { get; set; }

        [NotExcelProperty]
        public int EmailVerificationCodeLength { get; set; }

        [NotExcelProperty]
        public int MobileVerificationCodeLength { get; set; }

        [NotExcelProperty]
        public decimal UnusedAmountWithdrawPercent { get; set; }

        [NotExcelProperty]
        public int UserSessionExpireTime { get; set; }

        [NotExcelProperty]
        public int UnpaidWinValidPeriod { get; set; }

        [NotExcelProperty]
        public int VerificationKeyActiveMinutes { get; set; }

        [NotExcelProperty]
        public decimal AutoApproveBetShopDepositMaxAmount { get; set; }

        [NotExcelProperty]
        public int ClientSessionExpireTime { get; set; }

        public decimal AutoApproveWithdrawMaxAmount{ get; set; }

        public decimal AutoConfirmWithdrawMaxAmount { get; set; }

        [NotExcelProperty]
        public RegExProperty PasswordRegExProperty { get; set; }

        public int VipLevel { get; set; }
    }
}