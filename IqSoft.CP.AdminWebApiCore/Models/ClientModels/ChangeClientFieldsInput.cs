using System;
using IqSoft.CP.AdminWebApi.Filters;

namespace IqSoft.CP.AdminWebApi.ClientModels.Models
{
    public class ChangeClientFieldsInput
    {
        public ApiFilterClient FilterClient { get; set; }

        public int Action { get; set; }

        public string CurrencyId { get; set; }

        public string Password { get; set; }

        public int Gender { get; set; }

        public int State { get; set; }

        public DateTime BirthDate { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string DocumentNumber { get; set; }

        public int DocumentType { get; set; }

        public string DocumentIssuedBy { get; set; }

        public string Address { get; set; }

        public string Email { get; set; }

        public string MobileNumber { get; set; }

        public string PhoneNumber { get; set; }

        public string LanguageId { get; set; }

        public int RegionId { get; set; }

        public string ZipCode { get; set; }

        public int CategoryId { get; set; }

        public int SecurityQuestionId { get; set; }

        public string SecurityAnswer { get; set; }

        public bool IsDocumentVerified { get; set; }

        public bool IsEmailVerified { get; set; }

        public bool IsMobileVerified { get; set; }

        public bool SendMail { get; set; }

        public bool SendSms { get; set; }

        public bool SendPromotions { get; set; }

        public bool CallToPhone { get; set; }

        public string Info { get; set; }
    }
}