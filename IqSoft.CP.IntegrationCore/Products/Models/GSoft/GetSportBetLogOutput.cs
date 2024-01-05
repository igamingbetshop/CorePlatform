using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Products.Models.GSoft
{
    public class GetSportBetLogOutput : BaseResponse
    {
        [JsonProperty(PropertyName = "Data")]
        public List<SportBetData> Data { get; set; }
    }
}