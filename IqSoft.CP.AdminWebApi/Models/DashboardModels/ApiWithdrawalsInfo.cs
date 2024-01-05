﻿using System.Collections.Generic;

namespace IqSoft.CP.AdminWebApi.Models.DashboardModels
{
    public class ApiWithdrawalsInfo
    {
        public int Status { get; set; }

        public int TotalPlayersCount { get; set; }

        public List<ApiWithdrawalInfo> Withdrawals { get; set; }
    }

    public class ApiWithdrawalInfo
    {
        public int PaymentSystemId { get; set; }

        public string PaymentSystemName { get; set; }

        public decimal TotalAmount { get; set; }

        public int TotalWithdrawalsCount { get; set; }

        public int TotalPlayersCount { get; set; }
    }
}