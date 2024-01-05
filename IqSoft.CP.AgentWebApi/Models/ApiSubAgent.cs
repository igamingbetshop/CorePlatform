using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IqSoft.CP.AgentWebApi.Models
{
    public class ApiSubAgent
    {
        public int Id { get; set; } 
        public string UserName { get; set; }
        public int? Level { get; set; }
        public int? ParentId { get; set; }
        public List<object> Childs { get; set; }
    }
}