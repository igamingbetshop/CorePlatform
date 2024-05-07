using IqSoft.CP.Common.Models.AgentModels;
using IqSoft.CP.Common.Models.Commission;
using System;
using System.Collections.Generic;

namespace IqSoft.CP.AgentWebApi.Models.ClientModels
{
    public class NewClientModel
    {
        public int? Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string MobileNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
        public string CloningUserName { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public int? Gender { get; set; }
        public int? BirthYear { get; set; }
        public int? BirthMonth { get; set; }
        public int? BirthDay { get; set; }

        public string DocumentNumber { get; set; }
        public string DocumentIssuedBy { get; set; }
        public string Address { get; set; }
        public string LanguageId { get; set; }
        public bool? SendMail { get; set; }
        public bool? SendSms { get; set; }
        public int? Country { get; set; }
        public bool? Closed { get; set; }
        public int? Count { get; set; }
        public int Group { get; set; }
        public bool? AllowOutright { get; set; }
        public bool? AllowDoubleCommission { get; set; }
        public decimal? MaxCredit { get; set; }
        public AsianCommissionPlan CommissionPlan { get; set; }
        public List<LevelLimit> LevelLimits { get; set; }
    }
}