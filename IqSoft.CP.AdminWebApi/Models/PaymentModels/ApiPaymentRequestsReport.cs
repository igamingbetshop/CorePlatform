using System.Collections.Generic;

namespace IqSoft.CP.AdminWebApi.Models.PaymentModels
{
    public class ApiPaymentRequestsReport
    {
        public List<ApiPaymentRequest> Entities { get; set; }

        public long Count { get; set; }

        public decimal? TotalAmount { get; set; }

        public int TotalUniquePlayers { get; set; }
    }
}