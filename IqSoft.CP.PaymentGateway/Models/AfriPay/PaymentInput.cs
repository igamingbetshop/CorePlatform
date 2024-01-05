using System.Runtime.Serialization;

namespace IqSoft.CP.PaymentGateway.Models.AfriPay
{
    public class PaymentInput
    {
        [DataMember(Name = "refNo")]
        public string refNo { get; set; }

        [DataMember(Name = "amount")]
        public string Amount { get; set; }

        [DataMember(Name = "descriptor")]
        public string Descriptor { get; set; }

        [DataMember(Name = "order.id")]
        public string OrderId { get; set; }

        [DataMember(Name = "transaction.id")]
        public string TransactionId { get; set; }

        [DataMember(Name = "amount_currency")]
        public string AmountCurrency { get; set; }

        [DataMember(Name = "final_amount")]
        public string FinalAmount { get; set; }

        [DataMember(Name = "status")]
        public int Status { get; set; }
    }
}