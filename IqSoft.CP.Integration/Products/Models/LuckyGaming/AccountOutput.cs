using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Products.Models.LuckyGaming
{
    public class AccountOutput
    {
        [JsonProperty(PropertyName = "rtStatus")]
        public int Status { get; set; }

        [JsonProperty(PropertyName = "accountName")]
        public string AccountName { get; set; }

        [JsonProperty(PropertyName = "errorCode")]
        public int ErrorCode { get; set; }

        [JsonProperty(PropertyName = "timeStamp")]
        public int TimeStamp { get; set; }
    }
}
