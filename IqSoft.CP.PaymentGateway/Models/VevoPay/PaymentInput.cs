using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.PaymentGateway.Models.VevoPay
{
    public class PaymentInput
    {
        [JsonProperty(PropertyName = "islem")]
        public string Process { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "firma_key")]
        public string FirmaKey { get; set; }

        [JsonProperty(PropertyName = "kullanici_id")]
        public int UserId { get; set; }

        [JsonProperty(PropertyName = "kullanici_isim")]
        public string NameSurname { get; set; }

        [JsonProperty(PropertyName = "durum")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "durumMesaj")]
        public string StatusMessage { get; set; }

        [JsonProperty(PropertyName = "tutar")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "referans")]
        public string Reference { get; set; }

        [JsonProperty(PropertyName = "tarih")]
        public DateTime Date { get; set; }

        [JsonProperty(PropertyName = "token")]
        public string Token { get; set; }

        [JsonProperty(PropertyName = "yontem")]
        public string Method { get; set; }

        [JsonProperty(PropertyName = "hashcode")]
        public string HashCode { get; set; }
    }
}