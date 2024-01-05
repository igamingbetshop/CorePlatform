namespace IqSoft.CP.AdminWebApi.Models.UserModels
{
    public class UserIdentityInput
    {
        public int PartnerId { get; set; }
        public int UserId { get; set; }
        public string ApiKey { get; set; }
        public double TimeZone { get; set; }
        public string LanguageId { get; set; }
    }
}