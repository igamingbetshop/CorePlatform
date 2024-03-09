namespace IqSoft.CP.BetShopGatewayWebApi.Models
{
    public class ReportByBetInput : ApiFilterBase
    {
        public int? ProductId { get; set; }
        public long? Barcode { get; set; }
        public int? State { get; set; }
    }
}