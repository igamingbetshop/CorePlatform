using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.PaymentGateway.Models.LuckyPay
{
    public class PaymentStatusInput
    {
        public long transactionId { get; set; }
        public int status { get; set; }
        public string mobileNumber { get; set; }
        public int partnerId { get; set; }
        public int clientId { get; set; }
        public decimal amount { get; set; }
    }
}