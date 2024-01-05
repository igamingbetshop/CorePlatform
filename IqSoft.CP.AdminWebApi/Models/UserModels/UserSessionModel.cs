using System;

namespace IqSoft.CP.AdminWebApi.Models.UserModels
{
    public class UserSessionModel
    {
        public long Id { get; set; }
        public int? UserId { get; set; }
        public int PartnerId { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public int Type { get; set; }
        public string LanguageId { get; set; }
        public string Ip { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int State { get; set; }
        public int? LogoutType { get; set; }
    }
}