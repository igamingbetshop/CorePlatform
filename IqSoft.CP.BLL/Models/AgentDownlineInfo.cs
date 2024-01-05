using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.BLL.Models
{
    public class AgentDownlineInfo
    {
        public int Level { get; set; }
        public int Count { get; set; }
        public decimal MaxCredit { get; set; }
    }
}
