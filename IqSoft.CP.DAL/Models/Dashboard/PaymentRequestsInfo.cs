using System;
using System.Collections.Generic;

namespace IqSoft.CP.DAL.Models.Dashboard
{
    public class PaymentRequestsInfo
    {
        public int Status { get; set; }

        public int TotalPlayersCount { get; set; }

        public decimal TotalAmount { get; set; }

        public List<PaymentDailyInfo> DailyInfo { get; set; }

        public List<PaymentInfo> PaymentRequests { get; set; }
    }

    public class PaymentInfo
    {
        public int PaymentSystemId { get; set; }

        public string PaymentSystemName { get; set; }

        public decimal TotalAmount { get; set; }

        public int TotalRequestsCount { get; set; }

        public int TotalPlayersCount { get; set; }

        public List<PaymentDailyInfo> DailyInfo { get; set; }
    }

    public class PaymentDailyInfo
    {
        public long LongDate { get; set; }
        
        public DateTime Date { get; set; }

        public decimal TotalAmount { get; set; }

        public int TotalRequestsCount { get; set; }

        public int TotalPlayersCount { get; set; }
    }
}