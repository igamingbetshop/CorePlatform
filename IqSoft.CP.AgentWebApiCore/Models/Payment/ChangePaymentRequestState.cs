namespace IqSoft.CP.AgentWebApi.Models.Payment
{
    public class ChangePaymentRequestState
    {
        public long PaymentRequestId { get; set; }
        public string Comment { get; set; }
        public int CashDeskId { get; set; }
		public string Parameters { get; set; }
    }
}
