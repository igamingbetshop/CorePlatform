using Newtonsoft.Json;

namespace IqSoft.CP.ProductGateway.Models.ISoftBet
{
    public class BaseOutput
    {
        /// <summary>
        /// Should have value "success" for every correctly processed Request
        /// and  
        /// "error" in case of some Error on the Wallet side
        /// </summary>
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }
    }
}