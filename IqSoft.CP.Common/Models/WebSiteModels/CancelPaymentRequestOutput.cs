namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class CancelPaymentRequestOutput : ApiResponseBase
    {
        public decimal RequestAmount { get; set; }

        public int PaymentSystemId { get; set; }

        public ApiBalance ApiBalance { get; set; }
    }
}