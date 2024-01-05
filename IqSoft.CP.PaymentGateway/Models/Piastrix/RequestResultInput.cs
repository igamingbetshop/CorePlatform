namespace IqSoft.CP.PaymentGateway.Models.Piastrix
{
    public class RequestResultInput
    {
        public string status { get; set; }

        public int shop_id { get; set; }

        public long shop_order_id { get; set; }

        public string description { get; set; }

        public decimal shop_amount { get; set; }

        public decimal shop_refund { get; set; }

        public string shop_currency { get; set; }

        public long payment_id { get; set; }

        public decimal client_price { get; set; }

        public string ps_currency { get; set; }

        public string payway { get; set; }

        public string phone { get; set; }

        public string ps_data { get; set; }

        public string created { get; set; }

        public string processed { get; set; }

        public string addons { get; set; }

        public string sign { get; set; }

        public string failed_url { get; set; }

        public string success_url { get; set; }

        public string payer_id { get; set; }
    }
}