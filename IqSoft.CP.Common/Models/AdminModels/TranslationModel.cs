using System.Collections.Generic;

namespace IqSoft.CP.Common.Models.AdminModels
{
    public class TranslationEntry
    {
        public long ObjectId { get; set; }

        public string Text { get; set; }

        public string LanguageId { get; set; }
    }

    public class TranslationModel
    {
        public long TranslationId { get; set; }

        public int ObjectTypeId { get; set; }

		public string NickName { get; set; }

		public List<TranslationEntry> TranslationEntries { get; set; }
    }
}