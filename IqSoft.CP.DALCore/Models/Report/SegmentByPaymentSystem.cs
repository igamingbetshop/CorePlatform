namespace IqSoft.CP.DAL.Models.Report
{
    public class SegmentByPaymentSystem
    {
        public int SegmentId { get; set; }
        public string SegmentName { get; set; }
        public int PaymentRequestsCount { get; set; }
        public decimal TotalAmount { get; set; }

    }
}
