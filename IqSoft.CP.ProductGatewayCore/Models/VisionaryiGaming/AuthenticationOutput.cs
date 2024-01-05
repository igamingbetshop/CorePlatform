using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.ProductGateway.Models.VisionaryiGaming
{

    public class AuthenticateOutput
    {
        [JsonProperty(PropertyName = "AuthenticateResponse")]
        public List<AuthOutput> AuthenticateResponse { get; set; }
    }

    public class BatchDebitFundsOutput
    {
        [JsonProperty(PropertyName = "BatchDebitFundsResponse")]
        public List<BalanceOutput> BatchDebitFundsResponse { get; set; }
    }

    public class BatchCreditFundsOutput
    {
        [JsonProperty(PropertyName = "BatchCreditFundsResponse")]
        public List<BalanceOutput> BatchCreditFundsResponse { get; set; }
    }

    public class BatchGetBalanceOutput
    {
        [JsonProperty(PropertyName = "BatchGetBalanceResponse")]
        public List<BalanceOutput> BatchGetBalanceResponse { get; set; }
    }
    public class AuthOutput : BalanceOutput
    {
        [JsonProperty(PropertyName = "username")]
        public string UserName { get; set; }

        [JsonProperty(PropertyName = "screenname")]
        public string ScreenName { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "siteID")]
        public string SiteID { get; set; }

        [JsonProperty(PropertyName = "flag")]
        public string Flag { get; set; }

        [JsonProperty(PropertyName = "agentID")]
        public string AgentID { get; set; }

        [JsonProperty(PropertyName = "game")]
        public string Game { get; set; }

        [JsonProperty(PropertyName = "table")]
        public string Table { get; set; }
    }
}