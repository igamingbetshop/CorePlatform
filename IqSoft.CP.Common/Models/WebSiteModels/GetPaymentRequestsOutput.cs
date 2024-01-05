using System.Collections.Generic;

namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class GetPaymentRequestsOutput
    {
        public long Count { get; set; }
        public List<PaymentRequestModel> PaymentRequests { get; set; }
    }
}