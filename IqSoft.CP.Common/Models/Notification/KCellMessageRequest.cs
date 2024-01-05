using Newtonsoft.Json;

namespace IqSoft.CP.Common.Models
{
    public class KCellMessageRequest
    {
        [JsonProperty(PropertyName = "client_message_id")]
        public string ClientMessageId { get; set; }

        [JsonProperty(PropertyName = "time_bounds")]
        public string TimeBounds { get; set; }

        [JsonProperty(PropertyName = "sender")]
        public string Sender { get; set; }

        [JsonProperty(PropertyName = "recipient")]
        public string Recipient { get; set; }

        [JsonProperty(PropertyName = "message_text")]
        public string MessageText { get; set; }
    }
}
