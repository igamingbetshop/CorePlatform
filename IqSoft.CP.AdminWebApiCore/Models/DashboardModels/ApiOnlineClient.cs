using System;

namespace IqSoft.CP.AdminWebApi.Models.DashboardModels
{
    public class ApiOnlineClient
    {
        public int? Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public int? RegionId { get; set; }
        public string CurrencyId { get; set; }
        public int? PartnerId { get; set; }
        public bool? IsDocumentVerified { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public string PartnerName { get; set; }
        public int? CategoryId { get; set; }
        public bool? HasNote { get; set; }
        public string LoginIp { get; set; }
        public int? SessionTime { get; set; }
        public string SessionLanguage { get; set; }
        public string CurrentPage { get; set; }
        public int? TotalDepositsCount { get; set; }
        public int? CanceledDepositsCount { get; set; }
        public int? PendingDepositsCount { get; set; }
        public decimal PendingDepositsAmount { get; set; }
        public int? LastDepositState { get; set; }
        public decimal TotalDepositsAmount { get; set; }
        public int? TotalWithdrawalsCount { get; set; }
        public int? PendingWithdrawalsCount { get; set; }
        public decimal PendingWithdrawalsAmount { get; set; }
        public decimal TotalWithdrawalsAmount { get; set; }
        public int? TotalBetsCount { get; set; }
        public decimal? GGR { get; set; }
        public decimal Balance { get; set; }
    }
}