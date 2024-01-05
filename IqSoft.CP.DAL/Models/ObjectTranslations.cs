using System;
using System.Collections.Generic;

namespace IqSoft.CP.DAL.Models
{
    public class ObjectTranslations
    {
        public int Id { get; set; }
        public string NickName { get; set; }
        public long TranslationId { get; set; }
        public List<ObjectTranslationEntry> Translations { get; set; }
    }

    public class ObjectTranslationEntry
    {
        public long Id { get; set; }
        public string Language { get; set; }
        public string LanguageId { get; set; }
        public string Text { get; set; }
    }
}
