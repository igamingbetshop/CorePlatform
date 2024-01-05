using IqSoft.CP.DAL.Interfaces;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace IqSoft.CP.DAL
{
    public partial class fnObjectTranslationEntry : IBase
    {
        [JsonIgnore]
        [NotMapped]
        public long ObjectId
        {
            get { return TranslationId ?? 0; }
        }
    }
}
