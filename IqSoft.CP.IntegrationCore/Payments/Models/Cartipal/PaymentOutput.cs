using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Cartipal
{
   public class PaymentOutput
    {
        [JsonProperty(PropertyName = "status")]      
        public int Status { get; set; }

        [JsonProperty(PropertyName = "invoice_key")]
        public string InvoiceKey { get; set; }

        [JsonProperty(PropertyName = "deposit_id")]
        public string DepositId { get; set; }

        [JsonProperty(PropertyName = "errorDescription")]
        public string ErrorDescription { get; set; }
    }
}
