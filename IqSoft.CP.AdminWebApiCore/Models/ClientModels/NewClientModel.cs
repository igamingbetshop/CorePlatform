using System;

namespace IqSoft.CP.AdminWebApi.Models.ClientModels
{
    public class NewClientModel
    {
        public string Email { get; set; }
        public string CurrencyId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int PartnerId { get; set; }
        public int Gender { get; set; }
        public DateTime BirthDate { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DocumentNumber { get; set; }
        public string DocumentIssuedBy { get; set; }
        public string Address { get; set; }
        public string MobileNumber { get; set; }
        public string LanguageId { get; set; }
        public bool SendMail { get; set; }
        public bool SendSms { get; set; }
        public int RegionId { get; set; }
        public string PhoneNumber { get; set; }
        public string Info { get; set; }
        public int? Citizenship { get; set; }
        public int? JobArea { get; set; }
    }
}