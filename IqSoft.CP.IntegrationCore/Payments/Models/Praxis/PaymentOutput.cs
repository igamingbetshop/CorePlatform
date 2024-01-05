using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Praxis
{
    internal class PaymentOutput
    {
        public int Status { get; set; }
        public string Description { get; set; }

        [JsonProperty(PropertyName = "redirect_url")]
        public string RedirectUrl { get; set; }
        public Customer Customer { get; set; }
        public Session Session { get; set; }
        public string Version { get; set; }
        public int Timestamp { get; set; }
    }
    public class Customer
    {
        public string Customer_token { get; set; }
        public string Country { get; set; }
        public string First_name { get; set; }
        public string Last_name { get; set; }
        public int Avs_alert { get; set; }
        public int Verification_alert { get; set; }
    }

    public class Session
    {
        public string Auth_token { get; set; }
        public string Intent { get; set; }
        public string Session_status { get; set; }
        public string Order_id { get; set; }
        public string Currency { get; set; }
        public int Amount { get; set; }
        public double? Conversion_rate { get; set; }
        public string Processed_currency { get; set; }
        public int Processed_amount { get; set; }
        public string Payment_method { get; set; }
        public object Gateway { get; set; }
        public string Cid { get; set; }
        public string Variable1 { get; set; }
        public string Variable2 { get; set; }
        public object Variable3 { get; set; }
    }
}
