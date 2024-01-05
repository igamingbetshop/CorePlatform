using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Platforms.Models.OASIS
{
    public class LongTermLockInput : ShortTermLockInput
    {
        [JsonProperty(PropertyName = "lock_reason")]
        public string LockReason { get; set; }

        [JsonProperty(PropertyName = "lock_duration")]
        public string LockDuration { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "username")]
        public string Username { get; set; }

        [JsonProperty(PropertyName = "reg_date ")]
        public string RegDate { get; set; }

    }
}
