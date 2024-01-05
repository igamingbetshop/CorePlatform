using Newtonsoft.Json;

namespace IqSoft.CP.Integration.Payments.Models.Piastrix
{
    public class PayoutOutput : BaseOutput
    {
        [JsonProperty(PropertyName = "data")]
        public PayoutData Data { get; set; }
    }

    public class PayoutData
    {
        [JsonProperty(PropertyName = "balance")]
        public decimal Balance { get; set; }

        [JsonProperty(PropertyName = "id")]
        public int ExternalTransactionId { get; set; }

        [JsonProperty(PropertyName = "payee_account")]
        public string PayeeAccount { get; set; }

        [JsonProperty(PropertyName = "payee_currency")]
        public string PayeeCurrency { get; set; }

        [JsonProperty(PropertyName = "shop")]
        public int Shop { get; set; }

        [JsonProperty(PropertyName = "shop_currency")]
        public string ShopCurrency { get; set; }

        [JsonProperty(PropertyName = "write_off_amount")]
        public decimal WriteOffAmount { get; set; }

        [JsonProperty(PropertyName = "status")]
        public int Status { get; set; }

        [JsonProperty(PropertyName = "payee_receive")]
        public decimal ReceiveAmount { get; set; }


    }
}
