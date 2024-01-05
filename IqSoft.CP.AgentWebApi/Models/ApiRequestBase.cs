namespace IqSoft.CP.AgentWebApi.Models
{
    public class ApiRequestBase
    {
        public string Token { get; set; }
        public string LanguageId { get; set; }
        public double TimeZone { get; set; }
    }
}