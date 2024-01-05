using System.Collections.Generic;

namespace IqSoft.CP.AdminWebApi.Models.PaymentModels
{
    public class PaymentModel
    {
        public List<int> PaymentSystemIds { get; set; }

        public int PartnerId { get; set; }
    }
}