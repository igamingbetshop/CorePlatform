using IqSoft.CP.DAL.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace IqSoft.CP.DAL
{
    public partial class fnActionLog : IBase
    {
        [NotMapped]
        long IBase.ObjectId
        {
            get { return (long)ObjectId; }
        }
    }
}