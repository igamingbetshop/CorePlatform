using System;

namespace IqSoft.CP.BetShopGatewayWebApi.Models
{
    public class GetReportByBetInput : RequestBase
    {
        public DateTime BetDateFrom { get; set; }
        public DateTime BetDateBefore { get; set; }
        public int CashierId { get; set; }
        public int CashDeskId { get; set; }
        public int? ProductId { get; set; }
        public long? Barcode { get; set; }
        public int? State { get; set; }
    }
}