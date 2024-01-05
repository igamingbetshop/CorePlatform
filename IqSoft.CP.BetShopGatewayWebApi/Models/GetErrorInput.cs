using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.BetShopGatewayWebApi.Models
{
    public class GetErrorInput : RequestBase
    {
        public int ErrorId { get; set; }
    }
}
