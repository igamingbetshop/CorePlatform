using System;

namespace IqSoft.CP.DAL.Models.Clients
{
    public class QuickRegistrationInput
    {
        public string UserName { get; set; }

        public string Password { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string EmailOrMobile { get; set; }

        public string CurrencyId { get; set; }

        public int PartnerId { get; set; }
        public int? UserId { get; set; }

        public string ReCaptcha { get; set; }

        public bool IsMobile { get; set; }

        public string Ip { get; set; }

        public string PromoCode { get; set; }

        public string CountryCode { get; set; }

        public bool GeneratedUsername { get; set; }
        public bool IsMobileNumberVerified { get; set; }

        public AffiliateReferral ReferralData { get; set; }
        public DateTime? Birthdate { get; set; }
    }
}
