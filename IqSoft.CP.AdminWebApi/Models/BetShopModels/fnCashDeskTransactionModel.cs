using System;

namespace IqSoft.CP.AdminWebApi.Models.BetShopModels
{
    public class CashDeskTransactionModel
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
        public DateTime CreationTime { get; set; }
        public string OperationTypeName { get; set; }
        public string CashDeskName { get; set; }
        public string BetShopName { get; set; }
        public int BetShopId { get; set; }
        public int? CashierId { get; set; }
        public int PartnerId { get; set; }
        public int OperationTypeId { get; set; }
    }
}