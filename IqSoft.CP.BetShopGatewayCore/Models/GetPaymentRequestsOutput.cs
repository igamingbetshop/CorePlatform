using System;
using System.Collections.Generic;

namespace IqSoft.CP.BetShopGatewayWebApi.Models
{
    public class GetPaymentRequestsOutput : ApiResponseBase
    {
        public List<PaymentRequest> PaymentRequests { get; set; }
    }

    public class PaymentRequest
    {
        public long Id { get; set; }

        public int ClientId { get; set; }

        public string ClientFirstName { get; set; }

        public string ClientLastName { get; set; }

        public string ClientEmail { get; set; }

        public string UserName { get; set; }

        public string DocumentNumber { get; set; }

        public decimal Amount { get; set; }

        public string CurrencyId { get; set; }

        public string Info { get; set; }

        public DateTime CreationTime { get; set; }
    }
}
