using Newtonsoft.Json;
using System;

namespace IqSoft.CP.Integration.Payments.Models.Qaicash
{

    public class PaymentOutput
    {
        [JsonProperty(PropertyName = "depositTransaction")]
        public DepositTransaction DepositTransaction { get; set; }

        [JsonProperty(PropertyName = "paymentPageSession")]
        public Paymentpagesession PaymentPageSession { get; set; }
        public bool Success { get; set; }
        public string OrderId { get; set; }
        public string MessageAuthenticationCode { get; set; }
        public string Message { get; set; }
    }

    public class DepositTransaction
    {
        public string OrderId { get; set; }
        public string TransactionId { get; set; }
        public DateTime DateCreated { get; set; }
        public string DepositMethod { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; }
        public string DepositorUserId { get; set; }
        public string MessageAuthenticationCode { get; set; }
    }

    public class Paymentpagesession
    {
        public string SessionToken { get; set; }
        public DateTime Expires { get; set; }
        public string SessionType { get; set; }
        public string OrderId { get; set; }
        public string PaymentPageUrl { get; set; }
    }
}
