using System;
using System.Collections.Generic;

namespace IqSoft.CP.Common.Models.AdminModels
{
    public class ApiAdminClientSetting
    {
        public int ClientId { get; set; }
        public decimal? UnusedAmountWithdrawPercent { get; set; }
        public object PayoutLimit { get; set; }
        public string CasinoLayout { get; set; }
        public string PasswordChangedDate { get; set; }
        public TermsConditions TermsConditionsAcceptanceVersion { get; set; }
        public bool SelfExcluded { get; set; }
        public bool SystemExcluded { get; set; }
        public string AMLStatus { get; set; }
        public bool AMLProhibited { get; set; }
        public bool? AMLVerified { get; set; }
        public decimal? AMLPercent { get; set; }
        public bool JCJProhibited { get; set; }
        public bool DocumentVerified { get; set; }
        public bool DocumentExpired { get; set; }
        public bool CautionSuspension { get; set; }
        public bool BlockedForInactivity { get; set; }
        public bool BlockedForBonuses { get; set; }
        public StatusModel ExternalStatus { get; set; }
        public List<VerificationService> VerificationServices { get; set; }
        public StatusModel PEPSanctioned { get; set; } 
        public StatusModel UnderHighRiskCountry { get; set; }
        public bool? IsAffiliateManager { get; set; }
        public bool Restricted { get; set; }
        public bool Younger { get; set; }
    }

    public class TermsConditions
    {
        public string Version { get; set; }
        public DateTime? AcceptedAt { get; set; }
    }

    public class StatusModel
    {
        public string Status { get; set; }
        public DateTime? CheckedAt { get; set; }
    }

    public class VerificationService
    {
        public string Name { get; set; }
        public string Status { get; set; }
        public DateTime? CheckedAt { get; set; }
    }
}