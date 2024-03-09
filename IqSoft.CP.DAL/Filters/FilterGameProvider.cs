using System;
using System.Linq;

namespace IqSoft.CP.DAL.Filters
{
    public class FilterGameProvider : FilterBase<GameProvider>
    {
        public int? Id { get; set; }
        public int? ParentId { get; set; }        
        public int? PartnerId { get; set; }        
        public int? SettingPartnerId { get; set; }        
        public string Name { get; set; }
        public bool? IsActive { get; set; }

        protected override IQueryable<GameProvider> CreateQuery(IQueryable<GameProvider> objects, Func<IQueryable<GameProvider>, IOrderedQueryable<GameProvider>> orderBy = null)
        {
            if (Id.HasValue)
                objects = objects.Where(x => x.Id == Id.Value);
            if (!string.IsNullOrWhiteSpace(Name))
                objects = objects.Where(x => x.Name.Contains(Name));
            if (IsActive.HasValue)
                objects = objects.Where(x => x.IsActive == IsActive);
            return base.FilteredObjects(objects);
        }

        public IQueryable<GameProvider> FilterObjects(IQueryable<GameProvider> gameProviders)
        {
            return CreateQuery(gameProviders);
        }

        public long SelectedObjectsCount(IQueryable<GameProvider> gameProviders)
        {
            return CreateQuery(gameProviders).Count();
        }
    }
}
