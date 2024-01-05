using IqSoft.CP.DAL.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace IqSoft.CP.DAL
{
    public partial class fnTranslationEntry : IBase
    {
        [NotMapped]
        public long ObjectId
        {
            get
            {
                return TranslationId;
            }
        }
    }
}
