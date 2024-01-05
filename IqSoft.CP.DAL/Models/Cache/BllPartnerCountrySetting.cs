using System;

namespace IqSoft.CP.DAL.Models.Cache
{
    [Serializable]
    public class BllPartnerCountrySetting
    {
        public int Id { get; set; }
        public int PartnerId { get; set; }
        public int RegionId { get; set; }
        public int Type { get; set; }
        public string CountryNickName { get; set; }
        public string CountryName { get; set; }
        public string IsoCode { get; set; }
        public string IsoCode3 { get; set; }
    }
}
