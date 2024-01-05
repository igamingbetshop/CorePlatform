namespace IqSoft.CP.AgentWebApi.Models.Affiliate
{
    public class RecoverPasswordOutput : ApiResponseBase
    {
        public int AffiliateId { get; set; }

        public string AffiliateEmail { get; set; }

        public string AffiliateFirstName { get; set; }

        public string AffiliateLastName { get; set; }
    }
}