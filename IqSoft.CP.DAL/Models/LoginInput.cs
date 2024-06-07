namespace IqSoft.CP.DAL.Models
{
    public class LoginInput
    {
        public int PartnerId { get; set; }
        public string Identifier { get; set; }
        public string Password { get; set; }
        public string Ip { get; set; }
        public int DeviceType { get; set; }
        public string LanguageId { get; set; }
        public string CountryCode { get; set; }
        public string Source { get; set; }
        public double TimeZone { get; set; }
        public int? ExternalPlatformType { get; set; }
        public int UserType { get; set; }
        public int? CashDeskId { get; set; }
        public string ReCaptcha { get; set; }
    }
}