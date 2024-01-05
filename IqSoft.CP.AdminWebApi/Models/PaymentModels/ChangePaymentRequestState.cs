using System.Collections.Generic;

namespace IqSoft.CP.AdminWebApi.Models.PaymentModels
{
    public class ChangePaymentRequestState
    {
        public long PaymentRequestId { get; set; }
        public string Comment { get; set; }
        public int CashDeskId { get; set; }
		public string Parameters { get; set; }
        public bool SendEmail { get; set; }
        public List<decimal> Installments { get; set; }
    }
}
