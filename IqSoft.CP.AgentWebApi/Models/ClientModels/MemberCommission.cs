using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.AgentWebApi.Models.ClientModels
{
    public class MemberCommission
    {
        public int Level { get; set; }
        public decimal Commission { get; set; }

        public decimal CommissionLeft { get; set; }
    }
}