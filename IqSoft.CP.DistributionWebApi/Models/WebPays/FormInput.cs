namespace IqSoft.CP.DistributionWebApi.Models.WebPays
{
    public class FormInput
    {
        public string public_key { get; set; }
        public string terNO { get; set; }
        public string retrycount { get; set; }
        public string unique_reference { get; set; }
        public string source_url { get; set; }
        public string bill_amt { get; set; }
        public string bill_currency { get; set; }
        public string product_name { get; set; }
        public string mop { get; set; }
        public string fullname { get; set; }
        public string bill_email { get; set; }
        public string bill_address { get; set; }
        public string bill_city { get; set; }
        public string bill_state { get; set; }
        public string bill_country { get; set; }
        public string bill_zip { get; set; }
        public string bill_phone { get; set; }
        public string reference { get; set; }
        public string webhook_url { get; set; }
        public string return_url { get; set; }
    }
}