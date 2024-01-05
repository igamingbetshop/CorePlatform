using System.ComponentModel.DataAnnotations.Schema;

namespace IqSoft.CP.DAL
{
    public partial class Bet 
    {
        [NotMapped]
        public int PartnerId { get; set; }
    }
}
