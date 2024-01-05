using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.PremierCashier
{
    public class PaymentOutput
    {
        [JsonProperty(PropertyName = "cmd")]
        public string Cmd { get; set; }

        [JsonProperty(PropertyName = "code")]
        public int Code { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "time_elapsed_ms")]
        public string TimeElapsedMs { get; set; }

        [JsonProperty(PropertyName = "data")]
        public RedirectData Data { get; set; }
    }

    public class RedirectData
    {
        [JsonProperty(PropertyName = "redirect_url")]
        public string Url { get; set; }
    }
}
