namespace IqSoft.CP.BetShopGatewayWebApi.Models
{
    public class GetReportByBetInput : ApiFilterBase
    {
        public int CashierId { get; set; }
        public int CashDeskId { get; set; }
        public int? ProductId { get; set; }
        public long? Barcode { get; set; }
        public int? State { get; set; }
    }
}