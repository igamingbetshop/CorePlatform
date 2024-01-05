using System;

namespace IqSoft.CP.DAL.Models.Cache
{
    [Serializable]
    public class BllPartner
    {
        public int Id { get; set; }

        public string Name { get; set; }
        
        public string CurrencyId { get; set; }
        
        public string SiteUrl { get; set; }
        
        public string AdminSiteUrl { get; set; }
        
        public int State { get; set; }
        
        public long SessionId { get; set; }
        
        public System.DateTime CreationTime { get; set; }
        
        public System.DateTime LastUpdateTime { get; set; }
        
        public System.TimeSpan AccountingDayStartTime { get; set; }
        
        public int ClientMinAge { get; set; }
        
        public string PasswordRegExp { get; set; }
        
        public int VerificationType { get; set; }
        
        public int EmailVerificationCodeLength { get; set; }
        
        public int MobileVerificationCodeLength { get; set; }
        
        public decimal UnusedAmountWithdrawPercent { get; set; }
        
        public int UserSessionExpireTime { get; set; }
        
        public int UnpaidWinValidPeriod { get; set; }
        
        public int VerificationKeyActiveMinutes { get; set; }
        
        public decimal AutoApproveBetShopDepositMaxAmount { get; set; }
        
        public int ClientSessionExpireTime { get; set; }

        public decimal AutoApproveWithdrawMaxAmount { get; set; }

        public decimal AutoConfirmWithdrawMaxAmount { get; set; }
    }
}
