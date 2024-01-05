using System.Runtime.Serialization;

namespace IqSoft.CP.ProductGateway.Models.Singular
{
    [DataContract(Name = "amountResponseItem")]
    public class AmountResponse
    {
        [DataMember(Name = "statusCode")]
        public int StatusCode { get; set; }

        [DataMember(Name = "amount")]
        public decimal Amount { get; set; }
    }
}