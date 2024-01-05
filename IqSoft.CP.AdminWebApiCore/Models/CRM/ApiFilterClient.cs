namespace IqSoft.CP.AdminWebApi.Models.CRM
{
    public class ApiFilterClient : ApiBaseFilter
    {        
        public int? ClientId { get; set; }
        public int? AffiliatePlatformId { get; set; }
        public string AffiliateId { get; set; }
        public string AffiliateReferralId { get; set; }
    }
}