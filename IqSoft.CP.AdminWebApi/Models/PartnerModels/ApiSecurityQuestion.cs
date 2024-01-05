using System;

namespace IqSoft.CP.AdminWebApi.Models.PartnerModels
{
    public class ApiSecurityQuestion
    {
        public int Id { get; set; }
        public int PartnerId { get; set; }
        public string NickName { get; set; }
        public string QuestionText { get; set; }
        public bool Status { get; set; }
        public long TranslationId { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
    }
}