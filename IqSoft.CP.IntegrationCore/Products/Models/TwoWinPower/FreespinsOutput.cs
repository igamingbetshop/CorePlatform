using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Products.Models.TwoWinPower
{
    public class FreespinsOutput
    {
        [JsonProperty(PropertyName = "denominations")]
        public decimal[] Denominations { get; set; }

        [JsonProperty(PropertyName = "bets")]
        public List<Freespin> FreespinBets { get; set; }
    }

    public class Freespin
    {
        [JsonProperty(PropertyName = "bet_id")]
        public string BetId { get; set; }

        [JsonProperty(PropertyName = "bet_per_line")]
        public int BetPerLine { get; set; }

        [JsonProperty(PropertyName = "lines")]
        public int Lines { get; set; }
    }
}
