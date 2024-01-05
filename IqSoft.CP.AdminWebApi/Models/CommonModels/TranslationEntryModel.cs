namespace IqSoft.CP.AdminWebApi.Models.CommonModels
{
    public class TranslationEntryModel
    {
        public long Id { get; set; }
        public int ObjectTypeId { get; set; }
        public long TranslationId { get; set; }
        public string LanguageId { get; set; }
        public string Text { get; set; }
    }
}