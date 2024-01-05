using System;

namespace IqSoft.CP.AdminWebApi.Models.ReportModels.Internet
{
    public class ApiReportByClientDetailsElement
    {
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public int RegionId { get; set; }

        public int Status { get; set; }

        public string Email { get; set; }

        public string LanguageId { get; set; }

        public string DocumentNumber { get; set; }

        public int Gender { get; set; }

        public string Address { get; set; }

        public string PhoneNumber { get; set; }

        public string MobileNumber { get; set; }

        public string ZipCode { get; set; }

        public string CurrencyId { get; set; }

        public string RegistrationIp { get; set; }

        public bool IsDocumentVerified { get; set; }

        public bool SendMail { get; set; }

        public bool SendSms { get; set; }

        public bool SendPromotions { get; set; }

        public DateTime? BirthDate { get; set; }

        public DateTime? RegistrationDate { get; set; }

        public DateTime? FirstDepositDate { get; set; }

        public DateTime? LastDepositDate { get; set; }

        public decimal LastDepositAmount { get; set; }

        public decimal Balance { get; set; }

        public decimal WithdrawableBalance { get; set; }

        public decimal TotalBetsAmount { get; set; }

        public int TotalDepositsCount { get; set; }

        public decimal TotalDepositsAmount { get; set; }

        public decimal TotalWithdrawalsAmount { get; set; }

        public int PandingWithdrawalsCount { get; set; }
    }
}