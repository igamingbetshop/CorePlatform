using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Instapay
{
    public class CepbankOutput : PaymentOutput
    {
        [JsonProperty(PropertyName = "signature")]
        public string Signature { get; set; }

        [JsonProperty(PropertyName = "data")]
        public Data Datas { get; set; }

    }

    public class Data
    {
        [JsonProperty(PropertyName = "method")]
        public string Method { get; set; }

        [JsonProperty(PropertyName = "params")]
        public Param Params { get; set; }

        [JsonProperty(PropertyName = "redirect_url")]
        public string RedirectUrl { get; set; }
    }

    public class Param
    {
        [JsonProperty(PropertyName = "hash")]
        public string Hash { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
    }
}