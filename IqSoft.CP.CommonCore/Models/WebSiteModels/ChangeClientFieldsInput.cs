using System;
using System.Collections.Generic;

namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class ChangeClientFieldsInput
    {
        public int ClientId { get; set; }
        public string CurrencyId { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NickName { get; set; }
        public string SecondName { get; set; }
        public string SecondSurname { get; set; }
        public string DocumentNumber { get; set; }
        public string DocumentIssuedBy { get; set; }
        public string Address { get; set; }
        public string MobileNumber { get; set; }
        public string PhoneNumber { get; set; }
        public string ZipCode { get; set; }
        public string LanguageId { get; set; }
        public int? Gender { get; set; }
        public bool? SendPromotions { get; set; }
        public bool? CallToPhone { get; set; }
        public bool? SendMail { get; set; }
        public bool? SendSms { get; set; }
        public int? RegionId { get; set; }
        public int? CountryId { get; set; }
        public int? CityId { get; set; }
        public string City { get; set; }
        public int? TownId { get; set; }
        public int CategoryId { get; set; }
        public int? Citizenship { get; set; }
        public List<SecurityQuestion> SecurityQuestions { get; set; }
    }
}