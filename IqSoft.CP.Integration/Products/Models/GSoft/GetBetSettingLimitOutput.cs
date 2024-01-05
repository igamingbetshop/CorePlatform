using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Products.Models.GSoft
{
    public class GetBetSettingLimitOutput : BaseResponse
    {
        [JsonProperty(PropertyName = "Data")]
        public List<SettingData> Data { get; set; }
    }
}