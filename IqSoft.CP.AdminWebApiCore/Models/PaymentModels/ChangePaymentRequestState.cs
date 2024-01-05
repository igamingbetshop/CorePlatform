namespace IqSoft.CP.AdminWebApi.Models.PaymentModels
{
    public class ChangePaymentRequestState
    {
        public long PaymentRequestId { get; set; }
        public string Comment { get; set; }
        public int CashDeskId { get; set; }
		public string Parameters { get; set; }
        public bool SendEmail { get; set; }
    }
}
