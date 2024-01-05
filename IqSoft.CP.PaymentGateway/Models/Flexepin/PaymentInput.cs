using Newtonsoft.Json;
using System.Collections.Generic;

namespace IqSoft.CP.PaymentGateway.Models.Flexepin
{
    public class PaymentInput : BaseData
    {
        [JsonProperty(PropertyName = "initiator")]
        public string Initiator { get; set; }

        [JsonProperty(PropertyName = "transactionNo")]
        public string TransactionNo { get; set; }

        [JsonProperty(PropertyName = "externalTransactionNo")]
        public string ExternalTransactionNo { get; set; }

        [JsonProperty(PropertyName = "qty")]
        public string Qty { get; set; }

        [JsonProperty(PropertyName = "value")]
        public decimal VoucherValue { get; set; }

        [JsonProperty(PropertyName = "customer_id")]
        public string CustomerId { get; set; }

        [JsonProperty(PropertyName = "session_id")]
        public string PaymentRequestId { get; set; }

        [JsonProperty(PropertyName = "voucherPins")]
        public List<VoucherInput> VoucherPins { get; set; }

    }

    public class VoucherInput
    {
        [JsonProperty(PropertyName = "voucherPin")]
        public string VoucherPin { get; set; }

        [JsonProperty(PropertyName = "voucherSerialNumber")]
        public string VoucherSerialNumber { get; set; }
    }
}