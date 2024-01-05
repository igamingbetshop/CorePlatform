using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.Integration.Payments.Models.FinalPay
{
    public class PayoutOutput
    {
        public string Checksum { get; set; }
        public Data Data { get; set; }
        public string State { get; set; }
        public string Msg { get; set; }
    }
   
}

