using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.Integration.Products.Models.BlueOcean
{
    public class LoginOutput
    {
        public string id { get; set; }
        public string username { get; set; }
        public string balance { get; set; }
        public string currencycode { get; set; }
        public string created { get; set; }
        public string agent_balance { get; set; }
        public string sessionid { get; set; }
    }
}
