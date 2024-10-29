using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.PaymentGateway.Models.WzrdPay
{
    public class PaymentInput
    {
        [JsonProperty(PropertyName = "data")]
        public DataModel Data { get; set; }

       

        //[JsonProperty(PropertyName = "included")]
        public List<DataModel> Included { get; set; }
    }

    public class DataModel
    {
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "attributes")]
        public Attribute Attributes { get; set; }

    }

    public class Attribute
    {

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "resolution")]
        public string Resolution { get; set; }

        [JsonProperty(PropertyName = "moderation_required")]
        public bool ModerationRequired { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "payment_amount")]
        public decimal? PaymentAmount { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "service_amount")]
        public decimal? ServiceAmount { get; set; }

        [JsonProperty(PropertyName = "payment_service_amount")]
        public decimal? Payment_Service_Amount { get; set; }


        [JsonProperty(PropertyName = "exchange_rate")]
        public decimal? exchange_rate { get; set; }

        [JsonProperty(PropertyName = "service_currency")]
        public string ServiceCurrency { get; set; }


        [JsonProperty(PropertyName = "reference_id")]
        public string ReferenceId { get; set; }

        [JsonProperty(PropertyName = "test_mode")]
        public bool TestMode { get; set; }

        [JsonProperty(PropertyName = "fee")]
        public double Fee { get; set; }

        [JsonProperty(PropertyName = "deposit")]
        public decimal Deposit { get; set; }

        [JsonProperty(PropertyName = "processed")]
        public string Processed { get; set; }

        [JsonProperty(PropertyName = "processed_amount")]
        public int? ProcessedAmount { get; set; }

        [JsonProperty(PropertyName = "refunded_amount")]
        public int? RefundedAmount { get; set; }
        [JsonProperty(PropertyName = "refunded_fee")]
        public int? RefundedFee { get; set; }

        [JsonProperty(PropertyName = "processed_deposit")]
        public int? ProcessedDeposit { get; set; }

        [JsonProperty(PropertyName = "serial_number")]
        public string SerialNumber { get; set; }

        [JsonProperty(PropertyName = "flow_data")]
        public FlowData FlowData { get; set; }

        [JsonProperty(PropertyName = "flow")]
        public string Flow { get; set; }

        [JsonProperty(PropertyName = "hpp_url")]
        public string HppUrl { get; set; }

        [JsonProperty(PropertyName = "payment_flow")]
        public string PaymentFlow { get; set; }

        [JsonProperty(PropertyName = "created")]
        public int Created { get; set; }

        [JsonProperty(PropertyName = "updated")]
        public int Updated { get; set; }

        [JsonProperty(PropertyName = "payload")]
        public Payload Payload { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "descriptor")]
        public string descriptor { get; set; }

        [JsonProperty(PropertyName = "callback_url")]
        public string CallbackUrl { get; set; }

        [JsonProperty(PropertyName = "return_url")]
        public string ReturnUrl { get; set; }

        [JsonProperty(PropertyName = "original_data")]
        public /*OriginalDataModel*/object OriginalData { get; set; }

        [JsonProperty(PropertyName = "rrn")]
        public string Rrn { get; set; }

        [JsonProperty(PropertyName = "approval_code")]
        public string ApprovalCode { get; set; }

        [JsonProperty(PropertyName = "reserved_amount")]
        public string ReservedAmount { get; set; }

        [JsonProperty(PropertyName = "reserve_expires")]
        public string ReserveExpires { get; set; }


        [JsonProperty(PropertyName = "unreserved")]
        public string Unreserved { get; set; }

        [JsonProperty(PropertyName = "source")]
        public string Source { get; set; }


    }
    public class Metadata
    {
        [JsonProperty(PropertyName = "key")]
        public string Key { get; set; }
        [JsonProperty(PropertyName = "merchant_url")]
        public string MerchantUrl { get; set; }
    }

    public class OriginalDataModel
    {
        [JsonProperty(PropertyName = "external_id")]
        public string ExternalId { get; set; }

        [JsonProperty(PropertyName = "merchant_id")]
        public string MerchantId { get; set; }

        [JsonProperty(PropertyName = "provider_id")]
        public string ProviderId { get; set; }


        [JsonProperty(PropertyName = "external_mid")]
        public string ExternalMid { get; set; }

        [JsonProperty(PropertyName = "provider_code")]
        public string ProviderCode { get; set; }
    }

    public class FlowData
    {
        [JsonProperty(PropertyName = "action")]
        public string Action { get; set; }

        [JsonProperty(PropertyName = "method")]
        public string Method { get; set; }

        [JsonProperty(PropertyName = "params")]
        public object[] Parameters { get; set; }

        [JsonProperty(PropertyName = "metadata")]
        public Metadata Metadata { get; set; }
    }

    public class Payload
    {
        [JsonProperty(PropertyName = "token")]
        public string Token { get; set; }

        [JsonProperty(PropertyName = "client_ip")]
        public string ClientIp { get; set; }

        [JsonProperty(PropertyName = "payment_card")]
        public PaymentCard PaymentCard { get; set; }

        
    }

    public class PaymentCard
    {
        [JsonProperty(PropertyName = "last")]
        public string last { get; set; }

        [JsonProperty(PropertyName = "mask")]
        public string Mask { get; set; }

        [JsonProperty(PropertyName = "brand")]
        public string Brand { get; set; }

        [JsonProperty(PropertyName = "first")]
        public string First { get; set; }

        [JsonProperty(PropertyName = "holder")]
        public string Holder { get; set; }


        [JsonProperty(PropertyName = "network")]
        public string Network { get; set; }

        [JsonProperty(PropertyName = "expiry_year")]
        public string ExpiryYear { get; set; }


        [JsonProperty(PropertyName = "issuer_name")]
        public string IssuerName { get; set; }


        [JsonProperty(PropertyName = "expiry_month")]
        public string ExpiryMonth { get; set; }


        [JsonProperty(PropertyName = "issuer_country")]
        public string IssuerCountry { get; set; }
    }
}