using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.AgentWebApi.Models.User
{
    public class ApiUserState
    {
        public int AdminMenuId { get; set; }
        public int GridIndex { get; set; }
        public string State { get; set; }
    }
}