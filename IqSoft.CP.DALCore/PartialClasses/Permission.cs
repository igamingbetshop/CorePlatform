using IqSoft.CP.DAL.Interfaces;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IqSoft.CP.DAL
{
    [Serializable]
    public partial class Permission : IBase
    {
        [NotMapped]
        public long ObjectId
        {
            get { return 1; }
        }
    }
}