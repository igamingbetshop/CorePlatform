using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.IXOPay
{
    public class PayoutInput
    {
        [JsonProperty(PropertyName = "merchantTransactionId")]
        public string MerchantTransactionId { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public string Amount { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "callbackUrl")]
        public string CallbackUrl { get; set; }

        [JsonProperty(PropertyName = "transactionToken")]
        public string TransactionToken { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "referenceUuid")]
        public string ReferenceUuid { get; set; }

        [JsonProperty(PropertyName = "additionalId1")]
        public string AdditionalId1 { get; set; }

        [JsonProperty(PropertyName = "customer")]
        public Customer Customer { get; set; }
    }

    public class Customer
    {

        [JsonProperty(PropertyName = "identification")]
        public string Identification { get; set; }

        [JsonProperty(PropertyName = "firstName")]
        public string FirstName { get; set; }

        [JsonProperty(PropertyName = "lastName")]
        public string LastName { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "ipAddress")]
        public string IpAddress { get; set; }

        [JsonProperty(PropertyName = "billingCountry")]
        public string BillingCountry { get; set; }
    }
    }
