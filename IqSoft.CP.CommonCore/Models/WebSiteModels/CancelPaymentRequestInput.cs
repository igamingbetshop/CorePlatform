namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class CancelPaymentRequestInput
    {
        public long RequestId { get; set; }
        public int ClientId { get; set; }
        public string Comment { get; set; }
    }
}