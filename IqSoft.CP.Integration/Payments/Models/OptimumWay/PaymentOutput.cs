﻿using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.Integration.Payments.Models.OptimumWay
{
    public class PaymentOutput
    {
        [JsonProperty(PropertyName = "success")]
        public bool Success { get; set; }

        [JsonProperty(PropertyName = "uuid")]
        public string UUID { get; set; }

        [JsonProperty(PropertyName = "purchaseId")]
        public string PurchaseId { get; set; }

        [JsonProperty(PropertyName = "returnType")]
        public string ReturnType { get; set; }

        [JsonProperty(PropertyName = "redirectUrl")]
        public string RedirectUrl { get; set; }

        [JsonProperty(PropertyName = "paymentMethod")]
        public string PaymentMethod { get; set; }

        [JsonProperty(PropertyName = "error")]
        public List<ErrorOutput> Error { get; set; }
    }

    public class ErrorOutput
    {
        [JsonProperty(PropertyName = "errorCode")]
        public string ErrorCode { get; set; }

        [JsonProperty(PropertyName = "errorMessage")]
        public string ErrorMessage { get; set; }

        [JsonProperty(PropertyName = "adapterCode")]
        public string AdapterCode { get; set; }

        [JsonProperty(PropertyName = "adapterMessage")]
        public string AdapterMessage { get; set; }
    }
}