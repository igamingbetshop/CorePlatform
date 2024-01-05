using System.ComponentModel.DataAnnotations.Schema;

namespace IqSoft.CP.DAL
{
   public partial class fnBanner
    {
        [NotMapped]
        public string FragmentName { get; set; }
    }
}
