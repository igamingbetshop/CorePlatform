using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.FugaPay
{
    public class OutputBase
    {
        [JsonProperty(PropertyName = "resultCode")]
        public int ResultCode { get; set; }

        [JsonProperty(PropertyName = "resultMessage")]
        public string ResultMessage { get; set; }

        [JsonProperty(PropertyName = "messageID")]
        public string MessageID { get; set; }
    }
}
