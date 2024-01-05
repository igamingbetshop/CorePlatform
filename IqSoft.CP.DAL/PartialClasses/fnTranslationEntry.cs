using IqSoft.CP.DAL.Interfaces;

namespace IqSoft.CP.DAL
{
    public partial class fnTranslationEntry : IBase
    {
        public long ObjectId
        {
            get
            {
                return TranslationId;
            }
        }
    }
}
