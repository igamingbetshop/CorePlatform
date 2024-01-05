using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace IqSoft.CP.PaymentGateway.Models.CyberPlat
{
    public class InputBase
    {
        [DataMember(Name = "action")]
        public string Action { get; set; }

        [DataMember(Name = "additional")]
        public string Additional { get; set; }

        [DataMember(Name = "sign")]
        public int? Sign { get; set; }

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

        [DataMember(Name = "mes")] // Cancel Reason
        public int? Mes { get; set; } 
    }
}
