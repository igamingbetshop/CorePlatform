using System;

namespace IqSoft.CP.DAL.Models.Cache
{
    [Serializable]
    public class BllObjectType
    {
        public int Id { get; set; }

        public string Name { get; set; }
        
        public bool SaveChangeHistory { get; set; }
        
        public bool HasTranslation { get; set; }
    }
}
