using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.WalletOne
{
   public class WalletOneFormOutput
    {
       [JsonProperty(PropertyName = "PaymentId")]
       public string PaymentId { get; set; }

       [JsonProperty(PropertyName = "ProviderId")]
       public string ProviderId { get; set; }

       [JsonProperty(PropertyName = "State")]
       public WalletOneRequestState State { get; set; }

       [JsonProperty(PropertyName = "Step")]
       public int Step { get; set; }

       [JsonProperty(PropertyName = "StepCount")]
       public int StepCount { get; set; }

       [JsonProperty(PropertyName = "Form")]
       public WalletOneOutputForm Form { get; set; }
    }
}
