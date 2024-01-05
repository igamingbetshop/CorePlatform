using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Products.Models.GSoft
{
    class CheckPlayerBalanceOutput: BaseResponse
    {
        [JsonProperty(PropertyName = "Data")]
        public List<PlayerData> Data { get; set; }
    }
}
