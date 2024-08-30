using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.PaymentGateway.Models.Omno
{
    public class PaymentInput
    {

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public double Amount { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty(PropertyName = "country")]
        public string Country { get; set; }

        [JsonProperty(PropertyName = "customerId")]
        public string CustomerId { get; set; }

        [JsonProperty(PropertyName = "initialAmount")]
        public double InitialAmount { get; set; }

        [JsonProperty(PropertyName = "initialCurrency")]
        public string InitialCurrency { get; set; }

        [JsonProperty(PropertyName = "paymentLog")]
        public object PaymentLog { get; set; }

        [JsonProperty(PropertyName = "billingData")]
        public billing BillingData { get; set; }

        [JsonProperty(PropertyName = "paymentTransactionRequests")]
        public object PaymentTransactionRequests { get; set; }

        [JsonProperty(PropertyName = "paymentSystemLog")]
        public object PaymentSystemLog { get; set; }

        [JsonProperty(PropertyName = "orderId")]
        public string OrderId { get; set; }
    }

    public class billing
    {
        [JsonProperty(PropertyName = "firstName")]
        public string FirstName { get; set; }

        [JsonProperty(PropertyName = "lastName")]
        public string LastName { get; set; }

        [JsonProperty(PropertyName = "address1")]
        public string Address { get; set; }

        [JsonProperty(PropertyName = "city")]
        public string City { get; set; }


        [JsonProperty(PropertyName = "state")]
        public string State { get; set; }


        [JsonProperty(PropertyName = "country")]
        public string Country { get; set; }

        [JsonProperty(PropertyName = "postalCode")]
        public string PostalCode { get; set; }

        [JsonProperty(PropertyName = "phone")]
        public string Phone { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }
    }
}