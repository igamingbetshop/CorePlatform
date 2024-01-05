using System;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Interfaces;

namespace IqSoft.CP.DAL
{
    [Serializable]
    public partial class Region : IBase
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
                return (int)ObjectTypes.Region;
            }
        }
    }
}
