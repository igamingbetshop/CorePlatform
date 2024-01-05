using System.Collections.Generic;

namespace IqSoft.CP.AdminWebApi.Filters
{
    public class ApiFilterTranslationEntry: ApiFilterBase
    {
        public int ObjectTypeId { get; set; }

        public List<string> SelectedLanguages { get; set; }

        public string SearchText { get; set; }
    }
}