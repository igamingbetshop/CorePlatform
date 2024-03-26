using IqSoft.CP.DAL.Models.Dashboard;
using System;
using System.Collections.Generic;

namespace IqSoft.CP.AdminWebApi.Models.DashboardModels
{
    public class ApiDepositsInfo
    {
        public int Status { get; set; }

        public int? TotalPlayersCount { get; set; }

        public decimal TotalAmount { get; set; }

        public List<ApiDepositDailyInfo> DailyInfo { get; set; }

        public List<ApiDepositInfo> Deposits { get; set; }
    }

    public class ApiDepositInfo
    {
        public int PaymentSystemId { get; set; }

        public string PaymentSystemName { get; set; }

        public decimal TotalAmount { get; set; }

        public int TotalDepositsCount { get; set; }

        public int TotalPlayersCount { get; set; }

        public List<ApiDepositDailyInfo> DailyInfo { get; set; }
    }

    public class ApiDepositDailyInfo
    {
        public DateTime Date { get; set; }

        public decimal TotalAmount { get; set; }

        public int TotalRequestsCount { get; set; }

        public int? TotalPlayersCount { get; set; }
    }
}