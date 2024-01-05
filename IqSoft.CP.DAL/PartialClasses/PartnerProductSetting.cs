using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Interfaces;

namespace IqSoft.CP.DAL
{
    public partial class PartnerProductSetting : IBase
    {
        public long ObjectId
        {
            get { return Id; }
        }

        public int ObjectTypeId
        {
            get { return (int)ObjectTypes.PartnerProductSetting; }
        }
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