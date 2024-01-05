using IqSoft.CP.DAL.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace IqSoft.CP.DAL
{
    public partial class ObjectChangeHistory :IBase
    {
        [NotMapped]
        public string FirstName { get; set; }

        [NotMapped]
        public string LastName { get; set; }
    }
}
