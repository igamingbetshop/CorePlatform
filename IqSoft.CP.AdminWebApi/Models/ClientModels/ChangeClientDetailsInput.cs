using IqSoft.CP.Common.Models.AffiliateModels;
using System;

namespace IqSoft.CP.AdminWebApi.ClientModels.Models
{
    public class ChangeClientDetailsInput : ClientAffiliateModel
    {
        public int Id { get; set; }
        public DateTime? BirthDate { get; set; }
        public int? CategoryId { get; set; }
        public int? UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NickName { get; set; }
        public string DocumentNumber { get; set; }
        public int? DocumentType { get; set; }
        public string DocumentIssuedBy { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string MobileNumber { get; set; }
        public string LanguageId { get; set; }
        public int? RegionId { get; set; }
        public int? CountryId { get; set; }
        public int? DistrictId { get; set; }
        public int? CityId { get; set; }
        public string City { get; set; }
        public int? TownId { get; set; }
        public string ZipCode { get; set; }
        public bool? IsDocumentVerified { get; set; }
        public bool? IsEmailVerified { get; set; }
        public bool? IsMobileNumberVerified { get; set; }
        public string PhoneNumber { get; set; }
        public int? Gender { get; set; }
        public bool? SendMail { get; set; }
        public bool? SendSms { get; set; }
        public bool? SendPromotions { get; set; }
        public bool? CallToPhone { get; set; }
        public int? State { get; set; }
        public string Comment { get; set; }
        public string Info { get; set; }
        public int? BetShopId { get; set; }
        public int? Citizenship { get; set; }
        public int? JobArea { get; set; }
        public string SecondName { get; set; }
        public string SecondSurname { get; set; }
        public string BuildingNumber { get; set; }
        public string Apartment { get; set; }
        public string USSDPin { get; set; }
        public int? Title { get; set; }
        public int? ReferralType { get; set; }
    }
}