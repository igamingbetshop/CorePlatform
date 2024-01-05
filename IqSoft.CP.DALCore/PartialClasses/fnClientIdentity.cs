using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace IqSoft.CP.DAL
{
    public partial class fnClientIdentity : IBase
    {
        [NotMapped]
        public long ObjectId
        {
            get { return ClientId; }
        }

        [NotMapped]
        public int ObjectTypeId
        {
            get { return (int)ObjectTypes.Client; }
        }
    }
}