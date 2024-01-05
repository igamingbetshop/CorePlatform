namespace IqSoft.CP.BetShopGatewayWebApi.Models
{
    public class GetBetByDocumentIdInput : RequestBase
    {
        public long DocumentId { get; set; }

        public bool IsForPrint { get; set; }

        public int CashDeskId { get; set; }
    }
}