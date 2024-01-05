namespace IqSoft.CP.AgentWebApi.Models.Affiliate
{
    public class ApiNotificationInput : ApiRequestBase
    {
        public string Email { get; set; }
        public string Code { get; set; }
        public int Type { get; set; }
    }
}
