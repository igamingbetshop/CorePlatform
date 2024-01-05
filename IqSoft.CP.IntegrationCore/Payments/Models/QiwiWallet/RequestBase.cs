using Newtonsoft.Json;
using System.Xml.Serialization;

namespace IqSoft.CP.Integration.Payments.Models.QiwiWallet
{
  public class RequestBase
    {
        [JsonProperty(PropertyName = "amount")]
        public string Amount { get; set; }
        /// <summary>
        /// currensy id
        /// </summary>
        [JsonProperty(PropertyName = "ccy")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "user")]
        public string User { get; set; }

        [JsonProperty(PropertyName = "comment")]
        public string Comment { get; set; }
    }
}
