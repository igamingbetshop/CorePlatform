using System;
using IqSoft.CP.Common.Attributes;

namespace IqSoft.CP.AgentWebApi.Models
{
    public class BetShopBetModel
    {
        public long BetDocumentId { get; set; }

        [NotExcelProperty]
        public string BetExternalTransactionId { get; set; }
        public long? TicketNumber { get; set; }
        public int State { get; set; }
        public string StateName { get; set; }
        public string BetInfo { get; set; }
        public int? CashDeskId { get; set; }
        public decimal BetAmount { get; set; }
        public decimal? WinAmount { get; set; }

        [NotExcelProperty]
        public string CurrencyId { get; set; }
        public int ProductId { get; set; }

        [NotExcelProperty]
        public int? GameProviderId { get; set; }
        public long? Barcode { get; set; }
        public int? CashierId { get; set; }
        public DateTime BetDate { get; set; }
        public DateTime? WinDate { get; set; }
        public DateTime? PayDate { get; set; }

        [NotExcelProperty]
        public int BetShopId { get; set; }
        public string BetShopName { get; set; }

        [NotExcelProperty]
        public int PartnerId { get; set; }
        public string ProductName { get; set; }
        public int BetShopGroupId { get; set; }
        public int? BetTypeId { get; set; }
        public decimal? PossibleWin { get; set; }
        public string ProviderName { get; set; }
        public decimal Profit { get; set; }

        [NotExcelProperty]
        public bool HasNote { get; set; }

        [NotExcelProperty]
        public string RoundId { get; set; }
    }
}