using System;

namespace IqSoft.CP.WebSiteWebApi.Models
{
    [Serializable]
    public class GeoInfo
    {
        public int Id { get; set; }
        public string CountryCode { get; set; }
        public string LanguageId { get; set; }
    }
}