using System.Collections.Generic;

namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class GetPaymentRequestsOutput : ApiResponseBase
    {
        public long Count { get; set; }
        public List<PaymentRequestModel> PaymentRequests { get; set; }
    }
}