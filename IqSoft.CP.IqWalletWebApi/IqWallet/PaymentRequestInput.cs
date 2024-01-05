using IqSoft.CP.DAL;

namespace IqSoft.CP.IqWalletWebApi.Models.IqWallet
{
    public class PaymentRequestInput
    {
		public int MerchantId { get; set; }

		public long MerchantPaymentId { get; set; }

		public string MobileNumber { get; set; }

		public string Currency { get; set; }

		public Client MerchantClient { get; set; }

        public string Amount { get; set; }

		public string Sign { get; set; }
	}
}