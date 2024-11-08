﻿using System;

namespace IqSoft.CP.AdminWebApi.Models.ClientModels
{
    public class NewClientModel
    {
        public string Email { get; set; }
        public string CurrencyId { get; set; }
        public string UserName { get; set; }
        public string NickName { get; set; }
        public string Password { get; set; }
        public int PartnerId { get; set; }
        public int? Gender { get; set; }
        public DateTime BirthDate { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DocumentNumber { get; set; }
        public bool IsDocumentVerified { get; set; }
        public string DocumentIssuedBy { get; set; }
        public string Address { get; set; }
        public string MobileNumber { get; set; }
        public string MobileCode { get; set; }
        public string LanguageId { get; set; }
        public bool SendMail { get; set; }
        public bool SendSms { get; set; }
        public int RegionId { get; set; }
        public int? CountryId { get; set; }
        public string CityName { get; set; }
        public string ZipCode { get; set; }
        public string Info { get; set; }
        public int? Citizenship { get; set; }
        public int? JobArea { get; set; }
        public int? BetShopId { get; set; }
        public int?[] BetShopPaymentSystems { get; set; }
        public string Apartment { get; set; }
        public string BuildingNumber { get; set; }
        public string SecondName { get; set; }
        public bool SendPromotions { get; set; }
    }
}