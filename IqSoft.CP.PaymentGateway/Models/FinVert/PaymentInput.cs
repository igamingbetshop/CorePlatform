using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.PaymentGateway.Models.FinVert
{  

    public class PaymentInput
    {
        [JsonProperty(PropertyName = "responseCode")]
        public string ResponseCode { get; set; }

        [JsonProperty(PropertyName = "responseMessage")]
        public string ResponseMessage { get; set; }

        [JsonProperty(PropertyName = "3dsUrl")]
        public string ThreeDomainSequreUrl { get; set; }

        [JsonProperty(PropertyName = "data")]
        public Data Data { get; set; }
    }
    public class Data
    {
        [JsonProperty(PropertyName = "transaction")]
        public TransactionData Transaction { get; set; }

        [JsonProperty(PropertyName = "client")]
        public ClientData Client { get; set; }
    }
    public class TransactionData
    {
        [JsonProperty(PropertyName = "order_id")]
        public string OrderId { get; set; }

        [JsonProperty(PropertyName = "customer_order_id")]
        public object CustomerOrderId { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public string Amount { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }
    }

    public class ClientData
    {
        [JsonProperty(PropertyName = "first_name")]
        public string FirstName { get; set; }

        [JsonProperty(PropertyName = "last_name")]
        public string LastName { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "phone_no")]
        public object PhoneNo { get; set; }

        [JsonProperty(PropertyName = "address")]
        public object Address { get; set; }

        [JsonProperty(PropertyName = "zip")]
        public object Zip { get; set; }

        [JsonProperty(PropertyName = "city")]
        public object City { get; set; }

        [JsonProperty(PropertyName = "state")]
        public object State { get; set; }

        [JsonProperty(PropertyName = "country")]
        public string Country { get; set; }
    }
}