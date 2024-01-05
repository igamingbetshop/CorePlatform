
namespace IqSoft.CP.PaymentGateway.Models.DPOPay
{
    public class PaymentInput
    {

        public string TransID { get; set; }
        public string CCDapproval { get; set; }
        public string PnrID { get; set; }
        public string TransactionToken { get; set; }
        public string CompanyRef { get; set; }
        public string RedirectUrl { get; set; }
    }
}