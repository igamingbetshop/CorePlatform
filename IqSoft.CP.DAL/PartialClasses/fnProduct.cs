using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Interfaces;
using System.Collections.Generic;

namespace IqSoft.CP.DAL
{
    public partial class fnProduct : IBase
    {
        public long ObjectId
        {
            get
            {
                return Id;
            }
        }

        public int ObjectTypeId
        {
            get
            {
                return (int)ObjectTypes.fnProduct;
            }
        }

        public bool IsNewObject { get; set; }
        public int NewId { get; set; }
        public string ExternalCategory { get; set; }
        public List<ProductCountrySetting> ProductCountrySettings { get; set; }
        public string WebImage { get; set; }
        public string MobileImage { get; set; }
        public string BackgroundImage { get; set; }
    }
}