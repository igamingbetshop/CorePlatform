using System;
namespace IqSoft.CP.BetShopWebApi.Models.Common
{
    public class ApiClientOutput : ClientRequestResponseBase
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public bool IsEmailVerified { get; set; }
        public string CurrencyId { get; set; }
        public string UserName { get; set; }
        public int PartnerId { get; set; }
        public int RegionId { get; set; }
        public int CountryId { get; set; }
        public int Gender { get; set; }
        public DateTime BirthDate { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NickName { get; set; }       
        public string Address { get; set; }
        public string MobileNumber { get; set; }
        public string LanguageId { get; set; }
        public DateTime CreationTime { get; set; }
    }
}