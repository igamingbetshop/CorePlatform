using IqSoft.CP.Common.Helpers;
using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.IqWallet
{
    public class PaymentRequestInput
    {
        public int Status { get; set; }

        public string Amount { get; set; }

        public long MerchantPaymentId { get; set; }
        
        public long PaymentId { get; set; }

        public string Sign { get; set; }

        public int ErrorCode { get; set; }

        public string ErrorDescription { get; set; }
    }
}