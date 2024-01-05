using System.ComponentModel.DataAnnotations.Schema;

namespace IqSoft.CP.DAL
{
    public partial class Jackpot
    {
        [NotMapped]
        public int LeftBorder { get; set;}
        [NotMapped]
        public int RightBorder { get; set;}
    }
}
