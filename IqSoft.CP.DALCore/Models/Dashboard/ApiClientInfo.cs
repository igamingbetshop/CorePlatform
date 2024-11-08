﻿namespace IqSoft.CP.DAL.Models.PlayersDashboard
{
    public class ApiClientInfo
    {
        public int ClientId { get; set; }
        public string UserName { get; set; }
        public int PartnerId { get; set; }
        public string CurrencyId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? AffiliatePlatformId { get; set; }
        public string AffiliateId { get; set; }
        public string AffiliateReferralId { get; set; }
        public decimal TotalWithdrawalAmount { get; set; }
        public decimal WithdrawalsCount { get; set; }
        public decimal TotalDepositAmount { get; set; }
        public decimal DepositsCount { get; set; }
        public decimal TotalBetAmount { get; set; }
        public decimal BetsCount { get; set; }
        public decimal TotalWinAmount { get; set; }
        public decimal WinsCount { get; set; }
        public decimal GGR { get; set; }
        public decimal NGR { get; set; }
        public decimal TotalDebitCorrection { get; set; }
        public decimal DebitCorrectionsCount { get; set; }
        public decimal TotalCreditCorrection { get; set; }
        public decimal CreditCorrectionsCount { get; set; }
    }
}
