using System.ComponentModel.DataAnnotations.Schema;

namespace IqSoft.CP.DAL
{
    public partial class ClientIdentity 
    {
        [NotMapped]
        public bool HasNote { get; set; }
    }
}
