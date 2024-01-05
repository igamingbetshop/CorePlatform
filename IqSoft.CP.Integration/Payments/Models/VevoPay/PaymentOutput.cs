using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.Integration.Payments.Models.VevoPay
{
    public class PaymentOutput
    { 
        [JsonProperty(PropertyName = "iframe_bilgileri")]
        public IframeInformation IframeInfo { get; set; }        

        [JsonProperty(PropertyName = "hataMesaj")]
        public string ErrorMessage { get; set; }

        [JsonProperty(PropertyName = "apisorgu")]
        public string ApiQuery { get; set; }

        [JsonProperty(PropertyName = "apistatus")]
        public string ApiStatus{ get; set; }
    }
    
    public class IframeInformation
    {
        [JsonProperty(PropertyName = "link")]
        public string RedirectUrl { get; set; }

        [JsonProperty(PropertyName = "token")]
        public string Token { get; set; }

        [JsonProperty(PropertyName = "gecerlilik")]
        public string ValidityDate { get; set; }
    }
}
