using System;

namespace IqSoft.CP.DAL.Models.Cache
{
    [Serializable]
    public class BllRegionItem
    {
        public int? Id { get; set; }
        public int? ParentId { get; set; }
        public int? TypeId { get; set; }
        public string IsoCode { get; set; }
        public string IsoCode3 { get; set; }
    }
}
