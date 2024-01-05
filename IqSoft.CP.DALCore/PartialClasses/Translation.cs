using IqSoft.CP.DAL.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace IqSoft.CP.DAL
{
    public partial class Translation : IBase
    {
        [NotMapped]
        public long ObjectId
        {
            get
            {
                return Id;
            }
        }
    }
}