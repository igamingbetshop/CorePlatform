using System;

namespace IqSoft.CP.DAL.Models.Cache
{
    [Serializable]
    public class BllGameProviderSetting
    {
        public long Id { get; set; }
        public long ObjectId { get; set; }
        public int ObjectTypeId { get; set; }
        public int GameProviderId { get; set; }
        public int State { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public int? Order { get; set; }
    }
}
