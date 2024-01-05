using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Products.Models.Ezugi
{
    public class ReportOutput
    {
        [JsonProperty(PropertyName = "request_date")]
        public string RequestDate { get; set; }

        [JsonProperty(PropertyName = "data")]
        public List<Data> Datas { get; set; }
    }

    public class Data
    {
        [JsonProperty(PropertyName = "RoundID")]
        public string RoundId { get; set; }

        [JsonProperty(PropertyName = "TableID")]
        public string TableId { get; set; }

        [JsonProperty(PropertyName = "DealerID")]
        public string DealerId { get; set; }

        [JsonProperty(PropertyName = "DealerName")]
        public string DealerName { get; set; }

        [JsonProperty(PropertyName = "RoundString")]
        public string RoundString { get; set; }

        [JsonProperty(PropertyName = "PlayersNum")]
        public int PlayersNumber { get; set; }

        [JsonProperty(PropertyName = "RoundDateTime")]
        public string RoundDateTime { get; set; }
    }
}
    