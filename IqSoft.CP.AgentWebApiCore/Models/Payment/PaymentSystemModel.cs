using System;

namespace IqSoft.CP.AgentWebApi.Models.Payment
{ 
    public class PaymentSystemModel
    {
        public int Id { get; set; }

        public string Name { get; set; }
        
        public DateTime CreationTime { get; set; }

        public DateTime LastUpdateTime { get; set; }

        public int PeriodicityOfRequest { get; set; }

        public int PaymentRequestSendCount { get; set; }

        public int Type { get; set; }
    }
}