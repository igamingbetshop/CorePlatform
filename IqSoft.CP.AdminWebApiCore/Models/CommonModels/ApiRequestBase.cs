namespace IqSoft.CP.AdminWebApi.Models.CommonModels
{
    public class ApiRequestBase
    {
        public string Token { get; set; }

        public string LanguageId { get; set; }

        public double TimeZone { get; set; }
    }
}