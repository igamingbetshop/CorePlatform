using System;

namespace IqSoft.CP.PaymentGateway.Models.NOWPay
{
    public class PaymentInput
    {
        public string Payment_id { get; set; }
        public string Payment_status { get; set; }
        public string Pay_address { get; set; }
        public decimal Price_amount { get; set; }
        public string Price_currency { get; set; }
        public decimal Pay_amount { get; set; }
        public string Pay_currency { get; set; }
        public string Order_id { get; set; }
        public string Order_description { get; set; }
        public string Ipn_callback_url { get; set; }
        public DateTime Created_at { get; set; }
        public DateTime Updated_at { get; set; }
        public string Purchase_id { get; set; }
        public decimal Outcome_amount { get; set; }
        public decimal Actually_paid { get; set; }
    }
}
