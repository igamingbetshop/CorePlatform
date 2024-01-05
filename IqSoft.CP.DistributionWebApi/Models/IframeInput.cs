namespace IqSoft.CP.DistributionWebApi.Models
{
    public class IframeInput
    {
        public int PartnerId { get; set; }
        public int ClientId { get; set; }
        public string Token { get; set; }
        public string LanguageId { get; set; }
        public string RedirectUrl { get; set; }
        public string ResourcesUrl { get; set; }
        public string PlatformName { get; set; }
        public string Domain { get; set; }
    }
}