using System.ComponentModel.DataAnnotations.Schema;

namespace IqSoft.CP.DAL
{
    public partial class SecurityQuestion
    {
        [NotMapped]
        public string QuestionText { get; set; }
    }
}
