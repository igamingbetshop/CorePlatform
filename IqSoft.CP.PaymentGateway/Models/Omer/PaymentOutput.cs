using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.PaymentGateway.Models.Omer
{
    public class PaymentOutput
    {
        [JsonProperty(PropertyName = "server_date")]
        public DateTime ServerDate { get; set; }

        [JsonProperty(PropertyName = "date")]
        public DateTime Date { get; set; }


        [JsonProperty(PropertyName = "journal")]
        public DateTime Journal { get; set; }

        [JsonProperty(PropertyName = "uuid")]
        public string UuId { get; set; }


        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }


        //Which list?>
        [JsonProperty(PropertyName = "errors")]
        public List<int> Errors { get; set; }


        [JsonProperty(PropertyName = "redirect_url")]
        public List<int> RedirectUrl { get; set; }

    }

    public class RefundResponse
    {
        [JsonProperty(PropertyName = "server_date")]
        public DateTime ServerDate { get; set; }

        [JsonProperty(PropertyName = "UuId")]
        public string UuId { get; set; }

        [JsonProperty(PropertyName = "refund_uuid")]
        public string RefundUuid { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "code")]
        public string Code { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }


        [JsonProperty(PropertyName = "amount_refunded")]
        public decimal AmountRefunded { get; set; }
    }


    public class TransactionStatusResponse
    {
        [JsonProperty(PropertyName = "server_date")]
        public DateTime ServerDate { get; set; }


        [JsonProperty(PropertyName = "date")]
        public DateTime Date { get; set; }

        [JsonProperty(PropertyName = "journal")]
        public string Journal { get; set; }

        [JsonProperty(PropertyName = "uuid")]
        public string UuId { get; set; }


        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "usd_amount")]
        public decimal UsdAmount { get; set; }

        [JsonProperty(PropertyName = "exchange_rate")]
        public decimal ExchangeRate { get; set; }

        [JsonProperty(PropertyName = "descriptor")]
        public string Descriptor { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }


        [JsonProperty(PropertyName = "refunded")]
        public bool Refunded { get; set; }


        [JsonProperty(PropertyName = "code")]
        public string Code { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "auth_code")]
        public string AuthCode { get; set; }
        [JsonProperty(PropertyName = "signature")]
        public string Signature { get; set; }
    }
}