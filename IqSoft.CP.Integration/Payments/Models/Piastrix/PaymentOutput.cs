using Newtonsoft.Json;
using System;

namespace IqSoft.CP.Integration.Payments.Models.Piastrix
{
    public class PaymentOutput : BaseOutput
    {
        [JsonProperty(PropertyName = "data")]
        public PaymentData Data { get; set; }
    }

    public class OutputData
    {
        [JsonProperty(PropertyName = "en")]
        public string en { get; set; }

        [JsonProperty(PropertyName = "ru")]
        public string ru { get; set; }

        [JsonProperty(PropertyName = "created")]
        public DateTime? CreatedDate { get; set; }

        [JsonProperty(PropertyName = "lifetime")]
        public int? LifeTime { get; set; }

        [JsonProperty(PropertyName = "payer_account")]
        public string PayerAccount { get; set; }

        [JsonProperty(PropertyName = "payer_currency")]
        public string PayerCurrency { get; set; }

        [JsonProperty(PropertyName = "payer_price")]
        public decimal? Price { get; set; }

        [JsonProperty(PropertyName = "shop_amount")]
        public decimal? ShopAmount { get; set; }

        [JsonProperty(PropertyName = "shop_currency")]
        public string ShopCurrency { get; set; }

        [JsonProperty(PropertyName = "shop_id")]
        public int? ShopId { get; set; }

        [JsonProperty(PropertyName = "shop_order_id")]
        public long? ShopOrderId { get; set; }

        [JsonProperty(PropertyName = "shop_refund")]
        public decimal? ShopRefund { get; set; }

        [JsonProperty(PropertyName = "failUrl")]
        public string failUrl { get; set; }

        [JsonProperty(PropertyName = "shop")]
        public string shop { get; set; }

        [JsonProperty(PropertyName = "successUrl")]
        public string successUrl { get; set; }

        [JsonProperty(PropertyName = "transaction")]
        public string transaction { get; set; }

        [JsonProperty(PropertyName = "customerNumber")]
        public string customerNumber { get; set; }

        [JsonProperty(PropertyName = "orderNumber")]
        public string orderNumber { get; set; }

        [JsonProperty(PropertyName = "paymentType")]
        public string paymentType { get; set; }

        [JsonProperty(PropertyName = "scid")]
        public string scid { get; set; }

        [JsonProperty(PropertyName = "shopArticleId")]
        public string shopArticleId { get; set; }

        [JsonProperty(PropertyName = "shopDefaultUrl")]
        public string shopDefaultUrl { get; set; }

        [JsonProperty(PropertyName = "shopFailURL")]
        public string shopFailURL { get; set; }

        [JsonProperty(PropertyName = "shopId")]
        public string shopId { get; set; }

        [JsonProperty(PropertyName = "shopSuccessURL")]
        public string shopSuccessURL { get; set; }

        [JsonProperty(PropertyName = "sum")]
        public string sum { get; set; }

        [JsonProperty(PropertyName = "lang")]
        public string lang { get; set; }

        [JsonProperty(PropertyName = "m_curorderid")]
        public string m_curorderid { get; set; }

        [JsonProperty(PropertyName = "m_historyid")]
        public string m_historyid { get; set; }

        [JsonProperty(PropertyName = "m_historytm")]
        public string m_historytm { get; set; }

        [JsonProperty(PropertyName = "referer")]
        public string referer { get; set; }

        [JsonProperty(PropertyName = "session_id")]
        public string session_id { get; set; }

        [JsonProperty(PropertyName = "orderId")]
        public string orderId { get; set; }

        [JsonProperty(PropertyName = "NOPAYMENT_URL")]
        public string NOPAYMENT_URL { get; set; }

        [JsonProperty(PropertyName = "PAYEE_ACCOUNT")]
        public string PAYEE_ACCOUNT { get; set; }

        [JsonProperty(PropertyName = "PAYEE_NAME")]
        public string PAYEE_NAME { get; set; }

        [JsonProperty(PropertyName = "PAYMENT_AMOUNT")]
        public decimal? PAYMENT_AMOUNT { get; set; }

        [JsonProperty(PropertyName = "PAYMENT_ID")]
        public string PAYMENT_ID { get; set; }

        [JsonProperty(PropertyName = "PAYMENT_UNITS")]
        public string PAYMENT_UNITS { get; set; }

        [JsonProperty(PropertyName = "PAYMENT_URL")]
        public string PAYMENT_URL { get; set; }

        [JsonProperty(PropertyName = "STATUS_URL")]
        public string STATUS_URL { get; set; }

        [JsonProperty(PropertyName = "invoiceGuId")]
        public string invoiceGuId { get; set; }

    }

    public class PaymentData
    {
        [JsonProperty(PropertyName = "Data")]
        public OutputData RequestData { get; set; }
      
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }       

        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }

        [JsonProperty(PropertyName = "method")]
        public string Method { get; set; }
    }
}
