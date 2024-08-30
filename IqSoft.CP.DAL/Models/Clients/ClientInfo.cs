using System;

namespace IqSoft.CP.DAL.Models.Clients
{
    public class ClientInfo
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public int CategoryId { get; set; }
        public string CurrencyId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NickName { get; set; }
        public string SecondName { get; set; }
        public string SecondSurname { get; set; }
        public string Email { get; set; }
        public DateTime RegistrationDate { get; set; }
        public int Status { get; set; }
        public decimal Balance { get; set; }
        public decimal BonusBalance { get; set; }
        public decimal WithdrawableBalance { get; set; }
        public decimal CompPointBalance { get; set; }
        public decimal GGR { get; set; }
        public decimal Rake { get; set; }
        public decimal NGR { get; set; }
        public int TotalDepositsCount { get; set; }
        public decimal TotalDepositsAmount { get; set; }
        public decimal TotalDepositsPartnerConvertedAmount { get; set; }
        public decimal FailedDepositsCount { get; set; }
        public int TotalWithdrawalsCount { get; set; }
        public decimal TotalWithdrawalsAmount { get; set; }
        public decimal TotalWithdrawalsPartnerConvertedAmount { get; set; }
        public decimal FailedDepositsAmount { get; set; }
        public decimal TotalDebitCorrection { get; set; }
        public decimal TotalCreditCorrection { get; set; }

        public decimal TotalBetsCount { get; set; }
        public decimal SportBetsCount { get; set; }
        public decimal CasinoBetsCount { get; set; }
        public decimal TotalBetsPartnerConvertedAmount { get; set; }
        public int Risk { get; set; }
        public bool IsOnline { get; set; }
        public bool IsDocumentVerified { get; set; }
    }
}
