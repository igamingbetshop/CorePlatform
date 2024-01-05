﻿using System;

namespace IqSoft.CP.Common.Models.WebSiteModels.Clients
{
    public class ApiPaymentLimit
    {
        public int ClientId { get; set; }

        public int? MaxDepositsCountPerDay { get; set; }
        public decimal? MaxDepositAmount { get; set; }
        public decimal? MaxTotalDepositsAmountPerDay { get; set; }
        public decimal? MaxTotalDepositsAmountPerWeek { get; set; }
        public decimal? MaxTotalDepositsAmountPerMonth { get; set; }
        public decimal? MaxWithdrawAmount { get; set; }
        public decimal? MaxTotalWithdrawsAmountPerDay { get; set; }
        public decimal? MaxTotalWithdrawsAmountPerWeek { get; set; }
        public decimal? MaxTotalWithdrawsAmountPerMonth { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }
    }
}