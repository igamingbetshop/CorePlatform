using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace IqSoft.CP.DAL
{
    public partial class PartnerProductSetting : IBase
    {
        [NotMapped]
        public long ObjectId
        {
            get { return Id; }
        }

        [NotMapped]
        public int ObjectTypeId
        {
            get { return (int)ObjectTypes.PartnerProductSetting; }
        }
        [NotMapped]
        public string Comment { get; set; }
        public bool ShouldSerializeProduct()
        {
            return false;
        }

        public bool ShouldSerializeLimits()
        {
            return false;
        }

        public bool ShouldSerializePartner()
        {
            return false;
        }

        public bool ShouldSerializeDocuments()
        {
            return false;
        }
    }
}