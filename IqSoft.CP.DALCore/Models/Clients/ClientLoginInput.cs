namespace IqSoft.CP.DAL.Models.Clients
{
    public class ClientLoginInput
    {
        public int PartnerId { get; set; }
        public string ClientIdentifier { get; set; }
        public string Password { get; set; }
        public string Ip { get; set; }
        public int DeviceType { get; set; }
        public string LanguageId { get; set; }
        public string CountryCode { get; set; }
        public string Source { get; set; }
        public double TimeZone { get; set; }
        public int? ExternalPlatformType { get; set; }
    }
}