namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class QuickClientModel : ApiRequestBase
    {
        public string MobileNumber { get; set; }

        public string Email { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string CurrencyId { get; set; }

        public string ReCaptcha { get; set; }

        public string PromoCode { get; set; }

        public string RefId { get; set; }
        public string AgentCode { get; set; }

        public string AffiliateId { get; set; }

        public int? AffiliatePlatformId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }
        public bool? TermsConditionsAccepted { get; set; }
        public string SMSCode { get; set; }
        public int? BirthYear { get; set; }
        public int? BirthMonth { get; set; }
        public int? BirthDay { get; set; }
    }
}