using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models.Cache;

namespace IqSoft.CP.Integration.Payments.Models.IqWallet
{
    public class PaymentInput
    {
        public int MerchantId { get; set; }

        public long MerchantPaymentId { get; set; }

        public string MobileNumber { get; set; }

        public string Currency { get; set; }

        public BllClient MerchantClient { get; set; }

        public string Amount { get; set; }

        public string Sign { get; set; }
    }
}
