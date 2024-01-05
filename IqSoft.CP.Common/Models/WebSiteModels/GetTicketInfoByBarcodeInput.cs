namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class GetTicketInfoByBarcodeInput : ApiRequestBase
    {
        public long Barcode { get; set; }
        public string Credentials { get; set; }
    }
}