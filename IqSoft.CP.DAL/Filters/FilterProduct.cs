using System;
using System.Linq;

namespace IqSoft.CP.DAL.Filters
{
    public class FilterProduct : FilterBase<Product>
    {
        public int? Id { get; set; }

        public int? GameProviderId { get; set; }

        public int? PaymentSystemId { get; set; }

        public int? ParentId { get; set; }

        public string Description { get; set; }

        public string ExternalId { get; set; }

        protected override IQueryable<Product> CreateQuery(IQueryable<Product> objects, Func<IQueryable<Product>, IOrderedQueryable<Product>> orderBy = null)
        {
            if (Id.HasValue)
                objects = objects.Where(x => x.Id == Id.Value);
            if (GameProviderId.HasValue)
                objects = objects.Where(x => x.GameProviderId == GameProviderId.Value);
            if (PaymentSystemId.HasValue)
                objects = objects.Where(x => x.PaymentSystemId == PaymentSystemId.Value);
            if (ParentId.HasValue)
                objects = objects.Where(x => x.ParentId == ParentId.Value);
            if (!string.IsNullOrWhiteSpace(Description))
                objects = objects.Where(x => x.NickName.Contains(Description));
            if (!string.IsNullOrWhiteSpace(ExternalId))
                objects = objects.Where(x => x.ExternalId == ExternalId);
            return base.FilteredObjects(objects, orderBy);
        }

        public IQueryable<Product> FilterObjects(IQueryable<Product> products)
        {
            products = CreateQuery(products);
            return products;
        }

        public long SelectedObjectsCount(IQueryable<Product> products)
        {
            products = CreateQuery(products);
            return products.Count();
        }
    }
}