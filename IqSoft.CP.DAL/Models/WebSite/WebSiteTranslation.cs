using System.Collections.Generic;

namespace IqSoft.CP.DAL.Models.WebSite
{
   public class WebSiteTranslation
    {
        public int ItemId { get; set; }
        public string NickName { get; set; }
		public long TranslationId { get; set; }
		public List<WebSiteTranslationEntry> Translations { get; set; }
    }

    public class WebSiteTranslationEntry
    {
		public long Id { get; set; }
		public string Language { get; set; }
		public string LanguageId { get; set; }
        public string Text { get; set; }
    }
}
