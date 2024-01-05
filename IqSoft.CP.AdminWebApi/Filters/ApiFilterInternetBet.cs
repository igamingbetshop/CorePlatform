using System;

namespace IqSoft.NGGP.WebApplications.AdminWebApi.Filters
{
    public class ApiFilterInternetBet : ApiFilterBase
    {
        public long? Id { get; set; }

        public int? ClientId { get; set; }

        public string BetExternalTransactionId { get; set; }

        public string WinExternalTransactionId { get; set; }

        public int? GameProviderId { get; set; }

        public int? ProductId { get; set; }

        public long? Barcode { get; set; }

        public int? State { get; set; }

        public DateTime? BetDateFrom { get; set; }

        public DateTime? BetDateBefore { get; set; }
    }
}