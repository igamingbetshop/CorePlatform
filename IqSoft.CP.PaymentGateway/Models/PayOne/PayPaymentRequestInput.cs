namespace IqSoft.CP.PaymentGateway.Models.PayOne
{
    public class PayPaymentRequestInput : WithdrawalRequestInput
    {
        public string MerchantOrderId {get;set;}
        public string BankTransactionId {get;set;}
        public int OrderId {get;set;}
        public int State { get; set; }
        public string Description { get; set; }
    }
}