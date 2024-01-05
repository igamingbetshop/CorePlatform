using System;

namespace IqSoft.CP.Common.Models.UserModels
{
   public class AgentSubAccount : AgentEmployeePermissionModel
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Nickname { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime CreationTime { get; set; }
    }
}
