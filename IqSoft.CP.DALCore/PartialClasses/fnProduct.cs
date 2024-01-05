using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Interfaces;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace IqSoft.CP.DAL
{
    public partial class fnProduct : IBase
    {
        [NotMapped]
        public long ObjectId
        {
            get
            {
                return Id;
            }
        }

        [NotMapped]
        public int ObjectTypeId
        {
            get
            {
                return (int)ObjectTypes.fnProduct;
            }
        }
        [NotMapped]
        public bool IsNewObject { get; set; }
        [NotMapped]
        public int NewId { get; set; }
        [NotMapped]
        public string ExternalCategory { get; set; }
        [NotMapped]
        public List<ProductCountrySetting> ProductCountrySettings { get; set; }
    }
}