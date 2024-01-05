using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Skrill
{
    public class PrepareInput
    {
        /// <summary>
        ///  The required action. In the first step,  this is ‘prepare’.
        /// </summary>
        public string action { get; set; }

        /// <summary>
        /// Merchant email address
        /// </summary>
        public string email { get; set; }

        public string password { get; set; }

        public decimal amount { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string currency { get; set; }
        
        /// <summary>
        /// Recipient’s email address
        /// </summary>
        public string bnf_email { get; set; }

        /// <summary>
        /// Subject of the notification email
        /// </summary>
        public string subject { get; set; }

        /// <summary>
        /// Comment to be included in the  notification email.
        /// </summary>
        public string note { get; set; }

        /// <summary>
        ///  Your reference ID (must be unique if  submitted).
        /// </summary>
        public string frn_trn_id { get; set; }
    }
}