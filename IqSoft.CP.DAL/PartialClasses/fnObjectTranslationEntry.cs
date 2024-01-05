using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Interfaces;
using Newtonsoft.Json;

namespace IqSoft.CP.DAL
{
    public partial class fnObjectTranslationEntry : IBase
    {
        [JsonIgnore]
        public long ObjectId
        {
            get { return TranslationId ?? 0; }
        }
    }
}
