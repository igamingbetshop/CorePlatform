using System;

namespace IqSoft.NGGP.WebApplications.AdminWebApi.Filters
{
    public class ApiFilterBetShopBet : ApiFilterBase
    {
        public long? Id { get; set; }

        public int? CashierId { get; set; }
        
        public string BetExternalTransactionId { get; set; }

        public string WinExternalTransactionId { get; set; }

        public int? GameProviderId { get; set; }

        public int? ProductId { get; set; }

        public long? Barcode { get; set; }

        public int? State { get; set; }

        public DateTime? BetDateFrom { get; set; }

        public DateTime? BetDateBefore { get; set; }

        public int? CashDeskId { get; set; }

        public int? BetShopGroupId { get; set; }

        public int? BetShopId { get; set; }

        public string CurrencyId { get; set; }

        public long? TicketNumber { get; set; }

        public string BetShopName { get; set; }

        public int? PartnerId { get; set; }

        public string ProductName { get; set; }

        public DateTime? WinDateFrom { get; set; }

        public DateTime? WinDateBefore { get; set; }

        public DateTime? PayDateFrom { get; set; }

        public DateTime? PayDateBefore { get; set; }

    }
}