using System;
using System.Collections.Generic;
using System.Linq;

namespace IqSoft.CP.DAL.Filters
{
    public class FilterfnTranslationEntry : FilterBase<fnTranslationEntry>
    {
        public long? ObjectId { get; set; }

        public int? ObjectTypeId { get; set; }

        public List<string> SelectedLanguages { get; set; }

        public string SearchText { get; set; }

        public DateTime? CreatedFrom { get; set; }

        public DateTime? CreatedBefore { get; set; }

        protected override IQueryable<fnTranslationEntry> CreateQuery(IQueryable<fnTranslationEntry> objects,
            Func<IQueryable<fnTranslationEntry>, IOrderedQueryable<fnTranslationEntry>> orderBy = null)
        {
            if (ObjectId.HasValue)
                objects = objects.Where(x => x.ObjectId == ObjectId.Value);
            if (ObjectTypeId.HasValue)
                objects = objects.Where(x => x.ObjectTypeId == ObjectTypeId.Value);
            if (SelectedLanguages != null && SelectedLanguages.Count != 0)
                objects = objects.Where(x => SelectedLanguages.Contains(x.LanguageId));
            if (!string.IsNullOrEmpty(SearchText))
                objects = objects.Where(x => x.EnglishText.Contains(SearchText));
            return base.FilteredObjects(objects, orderBy);
        }

        public IQueryable<fnTranslationEntry> FilterObjects(IQueryable<fnTranslationEntry> translations,
            Func<IQueryable<fnTranslationEntry>, IOrderedQueryable<fnTranslationEntry>> orderBy = null)
        {
            translations = CreateQuery(translations, orderBy);
            return translations;
        }

        public long SelectedObjectsCount(IQueryable<fnTranslationEntry> translations)
        {
            translations = CreateQuery(translations);
            return translations.Count();
        }
    }
}
