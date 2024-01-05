using System;

namespace IqSoft.CP.AdminWebApi.Models.BetShopModels
{
    public class BetshopGroupModel : BetShopSetting
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public int? ParentId { get; set; }
        public int PartnerId { get; set; }
        public string Path { get; set; }
        public int State { get; set; }
        public bool IsLeaf { get; set; }
    }
}