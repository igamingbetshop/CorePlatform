using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace IqSoft.CP.DAL
{
    public partial class GameProvider : IBase
    {
        [NotMapped]
        public long ObjectId
        {
            get { return Id; }
        }

        [NotMapped]
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