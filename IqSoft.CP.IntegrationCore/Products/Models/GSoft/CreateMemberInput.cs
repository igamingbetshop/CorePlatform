using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Products.Models.GSoft
{
    public class CreateMemberInput //: BaseInput
    {
        [JsonProperty(PropertyName = "OpCode")]
        public string OpCode { get; set; }

        [JsonProperty(PropertyName = "PlayerName")]
        public string PlayerName { get; set; }

        [JsonProperty(PropertyName = "SecurityToken")]
        public string SecurityToken { get; set; }

        [JsonProperty(PropertyName = "FirstName")]
        public string FirstName { get; set; }

        [JsonProperty(PropertyName = "LastName")]
        public string LastName { get; set; }

        [JsonProperty(PropertyName = "OddsType")]
        public int OddsType { get; set; }

        [JsonProperty(PropertyName = "MaxTransfer")]
        public decimal MaxTransfer { get; set; }

        [JsonProperty(PropertyName = "MinTransfer")]
        public decimal MinTransfer { get; set; }
    }
}