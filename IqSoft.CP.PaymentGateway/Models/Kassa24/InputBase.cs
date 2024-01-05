using System;
using System.Runtime.Serialization;

namespace IqSoft.CP.PaymentGateway.Models.Kassa24
{
    public class InputBase
    {
        [DataMember(Name = "action")]
        public string Action { get; set; }

        [DataMember(Name = "number")]
        public string Number { get; set; }

        [DataMember(Name = "type")]
        public int Type { get; set; }

        [DataMember(Name = "amount")]
        public decimal Amount { get; set; }
        
        [DataMember(Name = "receipt")]
        public string Receipt { get; set; }

        [DataMember(Name = "date")]
        public DateTime Date { get; set; }

        [DataMember(Name = "commission")]
        public decimal Commission { get; set; }
    }
}
