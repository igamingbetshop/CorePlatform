using Newtonsoft.Json;
using System;

namespace IqSoft.CP.Integration.Products.Models.GSoft
{
    public class CasinoBetData : BetDataBase
    {
        [JsonProperty(PropertyName = "TableNo")]
        public string TableNumber { get; set; }

        [JsonProperty(PropertyName = "HandNo")]
        public string HandNumber { get; set; }

        [JsonProperty(PropertyName = "ShoeNo")]
        public string ShoeNumber { get; set; }

        [JsonProperty(PropertyName = "AfterAmount")]
        public decimal AfterAmount { get; set; }
    }
}