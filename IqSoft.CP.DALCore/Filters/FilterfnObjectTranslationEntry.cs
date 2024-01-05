using System;
using System.Collections.Generic;
using System.Linq;

namespace IqSoft.CP.DAL.Filters
{
    public class FilterfnObjectTranslationEntry : FilterBase<fnObjectTranslationEntry>
    {
        public int ObjectTypeId { get; set; }

        public List<string> SelectedLanguages { get; set; }

        public string SearchText { get; set; }

        protected override IQueryable<fnObjectTranslationEntry> CreateQuery(IQueryable<fnObjectTranslationEntry> objects,
            Func<IQueryable<fnObjectTranslationEntry>, IOrderedQueryable<fnObjectTranslationEntry>> orderBy = null)
        {

            return base.FilteredObjects(objects, orderBy);
        }

        public IQueryable<fnObjectTranslationEntry> FilterObjects(IQueryable<fnObjectTranslationEntry> translations,
            Func<IQueryable<fnObjectTranslationEntry>, IOrderedQueryable<fnObjectTranslationEntry>> orderBy = null)
        {
            translations = CreateQuery(translations, orderBy);
            return translations;
        }

        public long SelectedObjectsCount(IQueryable<fnObjectTranslationEntry> translations)
        {
            translations = CreateQuery(translations);
            return translations.Count();
        }
    }
}
