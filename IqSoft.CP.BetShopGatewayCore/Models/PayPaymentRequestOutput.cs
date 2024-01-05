using System;

namespace IqSoft.CP.BetShopGatewayWebApi.Models
{
    public class PayPaymentRequestOutput : FinOperationResponse
    {
        public long TransactionId { get; set; }

        public int ClientId { get; set; }

        public string UserName { get; set; }

        public string DocumentNumber { get; set; }

        public decimal Amount { get; set; }

        public DateTime PayDate { get; set; }
    }
}