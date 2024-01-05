using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.BetShopWebApi.Models.Common
{
    public class GetErrorInput : PlatformRequestBase
    {
        public int ErrorId { get; set; }
    }
}
