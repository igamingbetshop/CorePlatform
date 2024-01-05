using IqSoft.CP.DAL.Interfaces;

namespace IqSoft.CP.DAL
{
    public partial class Product : IBase
    {
        public long ObjectId
        {
            get { return Id; }
        }

        public int ObjectTypeId
        {
            get { return (int)Common.Enums.ObjectTypes.Product; }
        }

        public bool ShouldSerializeProduct1()
        {
            return false;
        }

        public bool ShouldSerializeProduct2()
        {
            return false;
        }

        public bool ShouldSerializePartnerProductSettings()
        {
            return false;
        }

        public bool ShouldSerializeClientClassifications()
        {
            return false;
        }

        public bool ShouldSerializeTranslation()
        {
            return false;
        }

        public bool ShouldSerializeClientSessions()
        {
            return false;
        }
    }
}