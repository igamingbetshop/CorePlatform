using IqSoft.CP.DAL.Interfaces;
using System;

namespace IqSoft.CP.DAL
{
    [Serializable]
    public partial class Permission : IBase
    {
        public long ObjectId
        {
            get { return 1; }
        }
    }
}