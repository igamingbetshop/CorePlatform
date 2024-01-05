namespace IqSoft.CP.BetShopGatewayWebApi.Models
{
    public class GetPaymentRequestsInput : RequestBase
    {
        public int? ClientId { get; set; }
        public string DocumentNumber { get; set; }
        public string DocumentIssuedBy { get; set; }
        public int CashDeskId { get; set; }
        public string CashCode { get; set; }
    }
}