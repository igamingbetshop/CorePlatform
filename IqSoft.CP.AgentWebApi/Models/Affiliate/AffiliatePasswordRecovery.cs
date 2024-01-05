namespace IqSoft.CP.AgentWebApi.Models.Affiliate
{
    public class AffiliatePasswordRecovery : ApiRequestBase
    {
        public string RecoveryToken { get; set; }

        public string NewPassword { get; set; }
    }
}