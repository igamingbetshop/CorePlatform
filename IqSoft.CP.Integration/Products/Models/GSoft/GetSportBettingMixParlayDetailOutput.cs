using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Products.Models.GSoft
{
    public class GetSportBettingMixParlayDetailOutput : BaseResponse
    {
        [JsonProperty(PropertyName ="Data")]
        public List<ParlayData> Data { get; set; }
    }
}