using System;

namespace IqSoft.CP.AgentWebApi.Models.ClientModels
{
    public class ClientInfoModel
    {
        public int Id { get; set; }

        public string UserName { get; set; }

        public int CategoryId { get; set; }

        public string CurrencyId {get; set;}

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public DateTime RegistrationDate { get; set; }

        public int Status { get; set; }

        public decimal Balance { get; set; }

        public decimal WithdrawableBalance { get; set; }

        public decimal GGR { get; set; }
        public decimal NGR { get; set; }

        public int TotalDepositsCount { get; set; }

        public decimal TotalDepositsAmount { get; set; }

        public int TotalWithdrawalsCount { get; set; }

        public decimal TotalWithdrawalsAmount { get; set; }

        public decimal FailedDepositsCount { get; set; }

        public decimal FailedDepositsAmount { get; set; }

        public int Risk { get; set; }

        public bool IsOnline { get; set; }
        public bool AllowOutright { get; set; }
        public bool AllowDoubleCommission { get; set; }
    }
}