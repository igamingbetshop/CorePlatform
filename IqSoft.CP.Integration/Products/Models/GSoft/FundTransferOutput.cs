using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Products.Models.GSoft
{
    public class FundTransferOutput : BaseResponse
    {
        [JsonProperty(PropertyName = "Data")]
        public TransferData Data { get; set; }
	}
}