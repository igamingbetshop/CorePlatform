using System;
using System.Linq;

namespace IqSoft.CP.DAL.Filters
{
    public class FilterGameProvider : FilterBase<GameProvider>
    {
        public int? Id { get; set; }
        
        public string Name { get; set; }

        protected override IQueryable<GameProvider> CreateQuery(IQueryable<GameProvider> objects, Func<IQueryable<GameProvider>, IOrderedQueryable<GameProvider>> orderBy = null)
        {
            if (Id.HasValue)
                objects = objects.Where(x => x.Id == Id.Value);
            if (!string.IsNullOrWhiteSpace(Name))
                objects = objects.Where(x => x.Name.Contains(Name));
            return base.FilteredObjects(objects);
        }

        public IQueryable<GameProvider> FilterObjects(IQueryable<GameProvider> gameProviders)
        {
            gameProviders = CreateQuery(gameProviders);
            return gameProviders;
        }

        public long SelectedObjectsCount(IQueryable<GameProvider> gameProviders)
        {
            gameProviders = CreateQuery(gameProviders);
            return gameProviders.Count();
        }
    }
}
