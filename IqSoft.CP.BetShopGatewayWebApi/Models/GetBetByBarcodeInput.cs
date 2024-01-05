namespace IqSoft.CP.BetShopGatewayWebApi.Models
{
    public class GetBetByBarcodeInput : RequestBase
    {
        public long Barcode { get; set; }

        public int CashDeskId { get; set; }
    }
}