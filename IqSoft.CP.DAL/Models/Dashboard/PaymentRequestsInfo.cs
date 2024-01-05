using System.Collections.Generic;

namespace IqSoft.CP.DAL.Models.Dashboard
{
    public class PaymentRequestsInfo
    {
        public int Status { get; set; }

        public int TotalPlayersCount { get; set; }

        public List<PaymentInfo> PaymentRequests { get; set; }
    }

    public class PaymentInfo
    {
        public string CurrencyId { get; set; }

        public int PaymentSystemId { get; set; }

        public string PaymentSystemName { get; set; }

        public decimal TotalAmount { get; set; }

        public int TotalRequestsCount { get; set; }

        public int TotalPlayersCount { get; set; }
    }
}