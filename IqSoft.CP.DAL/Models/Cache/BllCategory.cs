using System;
namespace IqSoft.CP.DAL.Models.Cache
{
    [Serializable]
    public class BllCategory
    {
        public int Id { get; set; }

        public int ObjectTypeId { get; set; }
        
        public string Name { get; set; }
        
        public decimal Percent { get; set; }
        
        public int? PartneId { get; set; }
    }
}
