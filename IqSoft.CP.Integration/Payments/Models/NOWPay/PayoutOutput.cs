using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.Integration.Payments.Models.NOWPay
{
    public class PayoutOutput
    {
        public string Id { get; set; }
        public List<Withdrawal> Withdrawals { get; set; }
    }
    public class Withdrawal
    {
        public bool Is_request_payouts { get; set; }
        public string Id { get; set; }
        public string Batch_withdrawal_id { get; set; }
        public string Status { get; set; }
        public object Error { get; set; }
        public string Currency { get; set; }
        public string Amount { get; set; }
        public string Address { get; set; }
        public object Extra_id { get; set; }
        public object Hash { get; set; }
        public string Ipn_callback_url { get; set; }
        public DateTime created_at { get; set; }
        public object Requested_at { get; set; }
        public object Updated_at { get; set; }
        public object Unique_external_id { get; set; }
    }
}
