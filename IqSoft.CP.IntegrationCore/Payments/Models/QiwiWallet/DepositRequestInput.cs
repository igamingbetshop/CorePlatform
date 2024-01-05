using System;
using Newtonsoft.Json;
using System.Xml.Serialization;

namespace IqSoft.CP.Integration.Payments.Models.QiwiWallet
{
    public class CreateRequestInput : RequestBase
    {
        [JsonProperty(PropertyName = "lifetime")]
        public string LifeTime { get; set; }

        [JsonProperty(PropertyName = "prv_name")]
        public string ProviderName { get; set; }

        [JsonProperty(PropertyName = "account")]
        public string Account { get; set; }

        [JsonProperty(PropertyName = "extras[ev_billno]")]
        public string Extras { get; set; }
    }


    public class DepositUrlInput
    {
        public string shop { get; set; }

        public string transaction { get; set; }
    }
}
