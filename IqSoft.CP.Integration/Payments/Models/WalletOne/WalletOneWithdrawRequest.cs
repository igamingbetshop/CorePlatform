using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.WalletOne
{
    public class WalletOneWithdrawRequest : RequestInput
    {
        [JsonProperty(PropertyName = "Key")]
        public string Key { get; set; }

        [JsonProperty(PropertyName = "RequestMethod")]
        public string RequestMethod { get; set; }

        [JsonProperty(PropertyName = "Url")]
        public string Url { get; set; }
    }
}
