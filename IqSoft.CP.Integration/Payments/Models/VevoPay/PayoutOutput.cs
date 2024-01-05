using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.Integration.Payments.Models.VevoPay
{
    public class PayoutOutput
    {
        [JsonProperty(PropertyName = "apistatus")]
        public string ApiStatus { get; set; }

        [JsonProperty(PropertyName = "Token")]
        public string Token { get; set; }

        [JsonProperty(PropertyName ="message")]
        public Message Message { get; set; }

    }
    public class Message
    {
        [JsonProperty(PropertyName ="en")]
        public string En { get; set; }
        [JsonProperty(PropertyName = "tr")]
        public string Tr { get; set; }
    }
}
