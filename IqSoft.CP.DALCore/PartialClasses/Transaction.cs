using IqSoft.CP.DAL.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace IqSoft.CP.DAL
{
    public partial class Transaction : IBase
    {
        [NotMapped]
        public long ObjectId
        {
            get { return Id; }
        }

        [NotMapped]
        public int ObjectTypeId
        {
            get { return (int)Common.Enums.ObjectTypes.Transaction; }
        }

        [NotMapped]
        public int AccountTypeId { get; set; }
    }
}