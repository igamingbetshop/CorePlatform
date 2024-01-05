using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.BetShopWebApi.Models.Common
{
    public class AssignPinInput : PlatformRequestBase
    {
        public int ClientId { get; set; }
        public int CashDeskId { get; set; }
    }
}
