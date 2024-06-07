namespace IqSoft.CP.DAL.Models.Job
{
   public class AffiliateReportInput
    {
        public int BrandId { get; set; }
        public string AffiliateId { get; set; }
        public long CustomerId { get; set; }
        public string BTag { get; set; }
        public string CountryCode { get; set; }
        public string RegistrationDate { get; set; }
        public string LanguageId { get; set; }
    }
}
