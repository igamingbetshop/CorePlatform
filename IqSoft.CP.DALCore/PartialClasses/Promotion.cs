using System.ComponentModel.DataAnnotations.Schema;

namespace IqSoft.CP.DAL
{
    public partial class Promotion
    {
        [NotMapped]
        public string Title { get; set; }

        [NotMapped]
        public string Description { get; set; }

        [NotMapped]
        public string Content { get; set; }

    }
}
