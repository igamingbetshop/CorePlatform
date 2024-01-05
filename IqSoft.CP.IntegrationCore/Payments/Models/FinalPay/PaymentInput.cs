using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.FinalPay
{
    public class PaymentInput
    {
        [JsonProperty(PropertyName = "data")]
        public RequestData DataDetails { get; set; }
    }

    public class RequestData
    {
        [JsonProperty(PropertyName = "request_type")]
        public string RequestType { get; set; }

        [JsonProperty(PropertyName = "refer_url")]
        public string ReturnUrl { get; set; }

        [JsonProperty(PropertyName = "lang")]
        public string Language { get; set; }

        [JsonProperty(PropertyName = "timestamp")]
        public string Timestamp { get; set; }

        [JsonProperty(PropertyName = "notification_url")]
        public string NotificationUrl { get; set; }

        [JsonProperty(PropertyName = "pay")]
        public Pay PayDetails { get; set; }

        [JsonProperty(PropertyName = "customer")]
        public Customer Customer { get; set; }

        [JsonProperty(PropertyName = "checksum")]
        public string CheckSum { get; set; }
    }

    public class Pay
    {
        [JsonProperty(PropertyName = "request_type")]
        public string RequestType { get; set; }

        [JsonProperty(PropertyName = "trans_ref")]
        public string OrderId { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public string Amount { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }
    }

    public class Customer
    {
        [JsonProperty(PropertyName = "customer_ref")]
        public string ClientId { get; set; }

        [JsonProperty(PropertyName = "first_name")]
        public string FirstName { get; set; }

        [JsonProperty(PropertyName = "last_name")]
        public string LastName { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "address")]
        public string Address { get; set; }

        [JsonProperty(PropertyName = "city")]
        public string City { get; set; }

        [JsonProperty(PropertyName = "country")]
        public string Country { get; set; }

        [JsonProperty(PropertyName = "zip")]
        public string Zip { get; set; }

        [JsonProperty(PropertyName = "phone")]
        public string Phone { get; set; }

        [JsonProperty(PropertyName = "dob")]
        public string BirthDate { get; set; }

        [JsonProperty(PropertyName = "requestor_ip")]
        public string RequestorIp { get; set; }
    }
}
