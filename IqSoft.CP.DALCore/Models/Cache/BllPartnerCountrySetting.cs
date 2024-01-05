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
    }
}
