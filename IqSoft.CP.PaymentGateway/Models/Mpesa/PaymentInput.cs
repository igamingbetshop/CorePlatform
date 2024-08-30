using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.PaymentGateway.Models.Mpesa
{
    public class PaymentInput
    {
        public BodyModel Body { get; set; }       
    }

    public class BodyModel
    {
        [JsonProperty(PropertyName = "stkCallback")]
        public CallbackInput StkCallback { get; set; }
    }

    public class CallbackInput
    {
        public string MerchantRequestID { get; set; }
        public string CheckoutRequestID { get; set; }
        public int ResultCode { get; set; }
        public string ResultDesc { get; set; }
        public Metadata CallbackMetadata { get; set; }
    }

    public class Metadata
    {
        public List<Item> Item { get; set; }
    }

    public class Item
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}