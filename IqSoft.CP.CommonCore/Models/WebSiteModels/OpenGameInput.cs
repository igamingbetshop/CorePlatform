namespace IqSoft.CP.Common.Models.WebSiteModels
{
    public class OpenGameInput
    {
		public int PartnerId { get; set; }
        public string LanguageId { get; set; }
        public string Token { get; set; }
        public bool IsForMobile { get; set; }
        public int GameId { get; set; }
        public string RoundId { get; set; }
        public string Domain { get; set; }
        public string Credentials { get; set; }
        public string CountryCode { get; set; }
        public string Callback { get; set; }
    }
}