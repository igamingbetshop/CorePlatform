using System;

namespace IqSoft.CP.AgentWebApi.Models.User
{
    public class ApiOnlineUser
    {
        public int Id { get; set; }
        public string Ip { get; set; }
        public DateTime StartTime { get; set; }
    }
}