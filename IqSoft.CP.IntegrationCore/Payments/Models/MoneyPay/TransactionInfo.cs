using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.Integration.Payments.Models.MoneyPay
{
    public class TransactionInfo
    {
        public string Version { get; set; }
        public string Merchant_id { get; set; }
        public string Channel_id { get; set; }
        public string Order_id { get; set; }
        public int Refund_order_id { get; set; }
        public string Result { get; set; }
        public string Error_info { get; set; }
    }
}
