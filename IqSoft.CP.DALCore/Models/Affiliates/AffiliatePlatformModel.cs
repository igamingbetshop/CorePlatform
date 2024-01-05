namespace IqSoft.CP.DAL.Models.Affiliates
{
    public class AffiliatePlatformModel
    {
        public int PartnerId { get; set; }
        public int ClientId { get; set; }
        public int AffiliateId { get; set; }
        public string AffiliateName { get; set; }
        public string ClickId { get; set; }
        public System.DateTime RegistrationDate { get; set; }
        public string CountryCode { get; set; }
        public string Language { get; set; }
        public string CurrencyId { get; set; }
        public System.DateTime? KickOffTime { get; set; }
        public System.DateTime LastExecutionTime { get; set; }
        public int StepInHours { get; set; }

    }
}
