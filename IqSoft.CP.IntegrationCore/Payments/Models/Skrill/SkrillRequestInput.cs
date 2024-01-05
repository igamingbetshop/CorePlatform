using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Skrill
{
    public class SkrillRequestInput
    {        
        [JsonProperty(PropertyName = "pay_to_email")]
        public string PayToEmail { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "recipient_description")]
        public string RecipientDescription { get; set; }

        [JsonProperty(PropertyName = "transaction_id")]
        public long TransactionId { get; set; }

        [JsonProperty(PropertyName = "return_url")]
        public string ReturnUrl { get; set; }

        [JsonProperty(PropertyName = "return_url_text")]
        public string ReturnUrlText { get; set; }

        /// Summary:
        /// <summary>
        /// Specifies a target in which the return_url value is displayed 
        /// upon successful payment from the customer. Default value is 1. 
        /// 1 = '_top'
        /// 2 = '_parent'
        /// 3 = '_self' 
        /// 4= '_blank'
        /// </summary>
        [JsonProperty(PropertyName = "return_url_target")]
        public int return_url_target { get; set; }

        [JsonProperty(PropertyName = "cancel_url")]
        public string CancelUrl { get; set; }

        /// Summary:
        /// <summary>
        /// Specifies a target in which the cancel_url value is displayed
        /// upon cancellation of payment by the customer. Default value is 1. 
        /// 1 = '_top' 
        /// 2 = '_parent' 
        /// 3 = '_self'
        /// 4= '_blank'
        /// </summary>
        [JsonProperty(PropertyName = "cancel_url_target")]
        public int CancelUrlTarget { get; set; }

        [JsonProperty(PropertyName = "status_url")]
        public string StatusUrl { get; set; }

        [JsonProperty(PropertyName = "language")]
        public string Language { get; set; }

        [JsonProperty(PropertyName = "logo_url")]
        public string LogoUrl { get; set; }

        [JsonProperty(PropertyName = "prepare_only")]
        public int PrepareOnly { get; set; }
    }
}
