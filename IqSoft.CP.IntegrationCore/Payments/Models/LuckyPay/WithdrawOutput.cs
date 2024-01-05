using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.Integration.Payments.Models.LuckyPay
{
    public class WithdrawOutput
    {
        public int status { get; set; }
        public string transactionId { get; set; }
        public string message { get; set; }
    }
}
