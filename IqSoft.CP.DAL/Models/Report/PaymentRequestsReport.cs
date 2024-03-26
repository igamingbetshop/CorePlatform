using IqSoft.CP.Common.Models;
namespace IqSoft.CP.DAL.Models.Report
{
    public class PaymentRequestsReport : PagedModel<fnPaymentRequest>
    {
        public decimal TotalAmount { get; set; }
        public decimal TotalFinalAmount { get; set; }

        public int TotalUniquePlayers { get; set; }
    }
}
