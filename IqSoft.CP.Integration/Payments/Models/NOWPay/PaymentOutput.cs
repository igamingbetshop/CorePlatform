using System;

namespace IqSoft.CP.Integration.Payments.Models.NOWPay
{
    public class PaymentOutput
    {
        public string Payment_id { get; set; }
        public string Payment_status { get; set; }
        public string Pay_address { get; set; }
        public double Price_amount { get; set; }
        public string Price_currency { get; set; }
        public double Pay_amount { get; set; }
        public double Amount_received { get; set; }
        public string Pay_currency { get; set; }
        public string Order_id { get; set; }
        public string Order_description { get; set; }
        public string Ipn_callback_url { get; set; }
        public string Purchase_id { get; set; }
        public string Smart_contract { get; set; }
        public string Network { get; set; }
        public string Network_precision { get; set; }
        public string Time_limit { get; set; }
        public string Burning_percent { get; set; }
    }
}