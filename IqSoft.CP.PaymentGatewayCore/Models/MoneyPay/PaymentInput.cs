using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.PaymentGateway.Models.MoneyPay
{
    public class PaymentInput
    {
        public string Transaction_id { get; set; }
        public string Transaction_currency { get; set; }
        public string Transaction_amount { get; set; }
        public string Merchant_id { get; set; }
        public string Version { get; set; }
        public string Channel_id { get; set; }
        public string Result_code { get; set; }
        public string Response_code { get; set; }
        public string Order_id { get; set; }
        public string Sign { get; set; }
        public string Pay_ext1 { get; set; }
        public string Flag { get; set; }
        public string Pay_ext2 { get; set; }
        public string Auth_trans { get; set; }
        public string Response_message { get; set; }
        
    }
}
