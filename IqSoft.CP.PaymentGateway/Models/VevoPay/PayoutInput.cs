using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.PaymentGateway.Models.VevoPay
{
    public class PayoutInput
    {
        [JsonProperty(PropertyName = "Process")]
        public string Process { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "firma_key")]
        public string FirmaKey { get; set; }

        [JsonProperty(PropertyName = "UserID")]
        public int UserId { get; set; }

        [JsonProperty(PropertyName = "NameSurname")]
        public string NameSurname { get; set; }

        [JsonProperty(PropertyName = "Status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "StatusMessage")]
        public string StatusMessage { get; set; }

        [JsonProperty(PropertyName = "Amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "Reference")]
        public string Reference { get; set; }

        [JsonProperty(PropertyName = "Date")]
        public DateTime Date { get; set; }

        [JsonProperty(PropertyName = "Token")]
        public string Token { get; set; }

        [JsonProperty(PropertyName = "Method")]
        public string Method { get; set; }

        [JsonProperty(PropertyName = "Hashcode")]
        public string HashCode { get; set; }
    }
}