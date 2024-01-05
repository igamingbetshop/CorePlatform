using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.BetShopGatewayWebApi.Models
{
    public class AssignPinInput : RequestBase
    {
        public int ClientId { get; set; }
        public int CashDeskId { get; set; }
    }
}
