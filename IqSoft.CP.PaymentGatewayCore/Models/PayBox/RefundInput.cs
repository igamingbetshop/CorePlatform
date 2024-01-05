namespace IqSoft.CP.PaymentGateway.Models.PayBox
{
    public class RefundInput : CheckInput
    {
        public decimal pg_net_amount { get; set; }

        public string pg_refund_date { get; set; }

        public string pg_refund_type { get; set; }

        public string pg_refund_system { get; set; }

        public long pg_refund_id { get; set; }
    }
}