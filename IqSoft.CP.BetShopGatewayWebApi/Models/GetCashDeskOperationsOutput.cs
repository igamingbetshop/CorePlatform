using System;
using System.Collections.Generic;

namespace IqSoft.CP.BetShopGatewayWebApi.Models
{
    public class CashDeskOperationsOutput : ApiResponseBase
    {
        public DateTime StartTime{ get; set; }
        public DateTime EndTime{ get; set; }
        public List<CashDeskOperation> Operations { get; set; }
    }

    public class CashDeskOperation
    {
        public long Id { get; set; }
        public string ExternalTransactionId { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyId { get; set; }
        public int State { get; set; }
        public string Info { get; set; }
        public int? Creator { get; set; }
        public int? CashDeskId { get; set; }
        public long? TicketNumber { get; set; }
        public string TicketInfo { get; set; }
        public int? CashierId { get; set; }
        public DateTime CreationTime { get; set; }
        public string OperationTypeName { get; set; }
        public string CashDeskName { get; set; }
        public string BetShopName { get; set; }
        public int BetShopId { get; set; }
        public int? ClientId { get; set; }
        public long? PaymentRequestId { get; set; }
    }
}