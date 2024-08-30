namespace IqSoft.CP.Common.Models
{
    public class ApiRequestBase
    {
        public int PartnerId { get; set; }
        public string LanguageId { get; set; }
		public double TimeZone { get; set; }
		public string Ip { get; set; }
		public string CountryCode { get; set; }
		public string Domain { get; set; }
		public int OSType { get; set; }
		public string Source { get; set; }
        public string Token { get; set; }
        public bool IsAgent { get; set; }
    }
}