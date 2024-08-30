using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.Integration.Payments.Models.GetaPay
{
    public class PaymentOutput
    {
        [JsonProperty(PropertyName = "success")]
        public bool IsSuccess { get; set; }

        [JsonProperty(PropertyName = "payout_id")]
        public string PayoutId { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }
        
    }
}
