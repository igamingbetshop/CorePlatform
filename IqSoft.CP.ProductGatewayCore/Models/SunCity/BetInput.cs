using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace IqSoft.CP.ProductGateway.Models.SunCity
{
    public class BetInput
    {
        [JsonProperty(PropertyName = "testmode")]
        public bool testmode { get; set; }

        [JsonProperty(PropertyName = "transactional")]
        public bool transactional { get; set; }

        [JsonProperty(PropertyName = "transactions")]
        public List<Transaction> Transactions { get; set; }
    }

    public class Transaction
    {
        [JsonProperty(PropertyName = "userid")]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "authtoken")]
        public string AuthToken { get; set; }

        [JsonProperty(PropertyName = "brandcode")]
        public string BrandCode { get; set; }

        [JsonProperty(PropertyName = "amt")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "cur")]
        public string currency { get; set; }

        [JsonProperty(PropertyName = "ipaddress")]
        public string IpAddress { get; set; }

        [JsonProperty(PropertyName = "ptxid")]
        public string ptxid { get; set; }

        [JsonProperty(PropertyName = "refptxid")]
        public string refptxid { get; set; }
        
        [JsonProperty(PropertyName = "txtype")]
        public int txtype { get; set; }

        [JsonProperty(PropertyName = "timestamp")]
        public DateTime timestamp { get; set; }

        [JsonProperty(PropertyName = "platformtype")]
        public int platformtype { get; set; }
        
        [JsonProperty(PropertyName = "gpcode")]
        public string gpcode { get; set; }

        [JsonProperty(PropertyName = "gamecode")]
        public string gamecode { get; set; }

        [JsonProperty(PropertyName = "gamename")]
        public string gamename { get; set; }

        [JsonProperty(PropertyName = "gametype")]
        public int gametype { get; set; }

        [JsonProperty(PropertyName = "externalgameid")]
        public string externalgameid { get; set; }

        [JsonProperty(PropertyName = "roundid")]
        public string roundid { get; set; }

        [JsonProperty(PropertyName = "externalroundid")]
        public string externalroundid { get; set; }

        [JsonProperty(PropertyName = "betid")]
        public string betid { get; set; }

        [JsonProperty(PropertyName = "externalbetid")]
        public string externalbetid { get; set; }

        [JsonProperty(PropertyName = "senton")]
        public DateTime senton { get; set; }

        [JsonProperty(PropertyName = "isclosinground")]
        public bool isclosinground { get; set; }

        [JsonProperty(PropertyName = "ggr")]
        public decimal GGR { get; set; }

        [JsonProperty(PropertyName = "turnover")]
        public decimal turnover { get; set; }

        [JsonProperty(PropertyName = "unsettledbets")]
        public decimal unsettledbets { get; set; }

        [JsonProperty(PropertyName = "walletcode")]
        public string walletcode { get; set; }

        [JsonProperty(PropertyName = "bonustype")]
        public int bonustype { get; set; }

        [JsonProperty(PropertyName = "bonuscode")]
        public string bonuscode { get; set; }

        [JsonProperty(PropertyName = "desc")]
        public string desc { get; set; }
    }

}