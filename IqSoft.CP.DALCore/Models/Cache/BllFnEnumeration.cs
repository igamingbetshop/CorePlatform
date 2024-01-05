using System;

namespace IqSoft.CP.DAL.Models.Cache
{
    [Serializable]
    public class BllFnEnumeration
    {
        public int Id { get; set; }
        
        public string EnumType { get; set; }
        
        public string NickName { get; set; }
        
        public int Value { get; set; }
        
        public long TranslationId { get; set; }
        
        public string Text { get; set; }
        
        public string LanguageId { get; set; }
    }
}
