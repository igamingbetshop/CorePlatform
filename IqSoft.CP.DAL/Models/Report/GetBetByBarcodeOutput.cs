using System;
namespace IqSoft.CP.DAL.Models.Report
{
    public class GetBetByBarcodeOutput
    {
        public long BetDocumentId { get; set; }
        public string BetExternalTransactionId { get; set; }
        public int? CashDeskId { get; set; }
        public int? BetTypeId { get; set; }
        public decimal? PossibleWin { get; set; }
        public int ProductId { get; set; }
        public int? GameProviderId { get; set; }
        public string TicketInfo { get; set; }
        public int? CashierId { get; set; }
        public DateTime BetDate { get; set; }
        public DateTime? PayDate { get; set; }
        public decimal BetAmount { get; set; }
        public long? TicketNumber { get; set; }
        public string ProductName { get; set; }
        public string RoundId { get; set; }
        public int State { get; set; }
        public DateTime? WinDate { get; set; }
        public decimal WinAmount { get; set; }
        public long Barcode { get; set; }
    }
}
