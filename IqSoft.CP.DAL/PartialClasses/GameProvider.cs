using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Interfaces;
using System.Collections.Generic;

namespace IqSoft.CP.DAL
{
    public partial class GameProvider : IBase
    {
        public long ObjectId
        {
            get { return Id; }
        }

        public int ObjectTypeId
        {
            get { return (int)ObjectTypes.GameProvider; }
        }

        public bool ShouldSerializeExternalOperations()
        {
            return false;
        }

        public bool ShouldSerializePartnerKeys()
        {
            return false;
        }

        public bool ShouldSerializeDocuments()
        {
            return false;
        }
    }
}