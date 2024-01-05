using System;
using System.Runtime.Serialization;

namespace IqSoft.CP.ProductGateway.Models.Singular
{
    [DataContract(Name = "exchangerateItem")]
    public class ExchangeRateOutput
    {
        [DataMember(Name = "CurrencyId")]
        public short CurrencyId { get; set; }

        [DataMember(Name ="Buy")]
        public decimal BuyRate { get; set; }

        [DataMember(Name ="Sell")]
        public decimal SellRate { get; set; }

        [DataMember(Name = "ModificationDate")]
        public DateTime ModificationDate { get; set; }
    }
}