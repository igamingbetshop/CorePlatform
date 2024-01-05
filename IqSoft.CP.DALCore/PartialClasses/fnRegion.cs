using System;
using System.ComponentModel.DataAnnotations.Schema;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Interfaces;

namespace IqSoft.CP.DAL
{
    [Serializable]
    public partial class  fnRegion : IBase
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
                return (int)ObjectTypes.fnRegion;
            }
        }
    }
}
