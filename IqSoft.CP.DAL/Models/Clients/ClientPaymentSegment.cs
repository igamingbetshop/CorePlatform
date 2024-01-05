using System;

namespace IqSoft.CP.DAL.Models.Clients
{
   public class ClientPaymentSegment
    {
        public int? PaymentSegmentId { get; set; }
        public string Name { get; set; }
        public int Priority { get; set; }
        public DateTime? LastUpdateTime { get; set; }
        public decimal DepositMinAmount { get; set; }
        public decimal DepositMaxAmount { get; set; }
        public decimal WithdrawMinAmount { get; set; }
        public decimal WithdrawMaxAmount { get; set; }
        public string CurrencyId { get; set; }
        public string ApiUrl { get; set; }
        public string ApiKey { get; set; }
        public int Status { get; set; }
    }
}
