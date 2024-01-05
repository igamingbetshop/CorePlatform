using System.Runtime.Serialization;

namespace IqSoft.CP.ProductGateway.Models.Singular
{
    [DataContract(Name = "withdrawResponseItem")]
    public class WithdrawOutput : BaseOutput
    {
        [DataMember(Name = "transactionID")]
        public string TransactionId { get; set; }

        [DataMember(Name = "totalAmount")]
        public decimal TotalAmount { get; set; }

        [DataMember(Name = "percentAmount")]
        public decimal PercentAmount { get; set; }

        [DataMember(Name = "percent")]
        public double Percent { get; set; }
    }
}