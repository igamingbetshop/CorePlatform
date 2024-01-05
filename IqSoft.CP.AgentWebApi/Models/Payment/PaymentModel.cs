using System.Collections.Generic;

namespace IqSoft.CP.AgentWebApi.Models.Payment
{
    public class PaymentModel
    {
        public List<int> PaymentSystemIds { get; set; }
        public int PartnerId { get; set; }
    }
}