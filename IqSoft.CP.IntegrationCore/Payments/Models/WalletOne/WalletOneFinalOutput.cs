using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.WalletOne
{
   public class WalletOneFinalOutput
    {
        [JsonProperty(PropertyName = "PaymentId")]
       public int PaymentId { get; set; }

        [JsonProperty(PropertyName = "State")]
        public WalletOneRequestState State { get; set; }
    }

   public class WalletOneRequestState
   {
       [JsonProperty(PropertyName = "StateId")]
       public string StateId { get; set; }

       [JsonProperty(PropertyName = "Description")]
       public string Description { get; set; }
   }
}
