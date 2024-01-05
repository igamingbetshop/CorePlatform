using Newtonsoft.Json;

namespace IqSoft.CP.PaymentGateway.Models.Cartipal
{
    public class PaymentOutput
    {
        [JsonProperty(PropertyName = "invoice_key")]
        public string InvoiceKey { get; set; }

        [JsonProperty(PropertyName = "bank_code")]
        public string BankCode { get; set; }
        
        [JsonProperty(PropertyName = "card_number")]
        public string CardNumber { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "inProcess")]
        public int InProcess { get; set; }

        [JsonProperty(PropertyName = "errorDescription")]
        public string ErrorDescription { get; set; }
    }
}