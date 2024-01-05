using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Payments.Models.Mifinity
{
    public class PaymentOutput
    {
        [JsonProperty(PropertyName = "payload")]
        public List<Payload> Payload { get; set; }
    }

    public class Payload
    {
        [JsonProperty(PropertyName = "traceId")]
        public string TraceId { get; set; }

        [JsonProperty(PropertyName = "initializationToken")]
        public string InitializationToken { get; set; }
    }
}