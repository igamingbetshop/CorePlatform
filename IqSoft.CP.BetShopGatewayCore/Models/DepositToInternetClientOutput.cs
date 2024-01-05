using System;
namespace IqSoft.CP.BetShopGatewayWebApi.Models
{
    public class DepositToInternetClientOutput : FinOperationResponse
    {
        public long TransactionId { get; set; }

        public int ClientId { get; set; }
        public string ClientUserName { get; set; }

        public string DocumentNumber { get; set; }

        public decimal Amount { get; set; }

        public int Status { get; set; }

        public DateTime DepositDate { get; set; }
    }
}