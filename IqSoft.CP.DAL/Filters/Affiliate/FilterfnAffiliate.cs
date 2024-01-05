using IqSoft.CP.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IqSoft.CP.DAL.Filters.Affiliate
{
    public class FilterfnAffiliate : FilterBase<fnAffiliate>
    {
        public int? PartnerId { get; set; }

        public DateTime? CreatedFrom { get; set; }

        public DateTime? CreatedBefore { get; set; }

        public FiltersOperation Ids { get; set; }

        public FiltersOperation Emails { get; set; }

        public FiltersOperation UserNames { get; set; }

        public FiltersOperation FirstNames { get; set; }

        public FiltersOperation LastNames { get; set; }

        public FiltersOperation MobileNumbers { get; set; }

        public FiltersOperation RegionIds { get; set; }
        
        public FiltersOperation States { get; set; }

        public FiltersOperation CreationTimes { get; set; }

        protected override IQueryable<fnAffiliate> CreateQuery(IQueryable<fnAffiliate> objects, Func<IQueryable<fnAffiliate>, IOrderedQueryable<fnAffiliate>> orderBy = null)
        {
            if (PartnerId.HasValue)
                objects = objects.Where(x => x.PartnerId == PartnerId.Value);
            if (CreatedFrom.HasValue)
                objects = objects.Where(x => x.CreationTime >= CreatedFrom.Value);
            if (CreatedBefore.HasValue)
                objects = objects.Where(x => x.CreationTime < CreatedBefore.Value);

            FilterByValue(ref objects, Ids, "Id");
            FilterByValue(ref objects, Emails, "Email");
            FilterByValue(ref objects, UserNames, "UserName");
            FilterByValue(ref objects, FirstNames, "FirstName");
            FilterByValue(ref objects, LastNames, "LastName");
            FilterByValue(ref objects, MobileNumbers, "MobileNumber");
            FilterByValue(ref objects, RegionIds, "RegionId");
            FilterByValue(ref objects, States, "State");

            return base.FilteredObjects(objects, orderBy);
        }

        public IQueryable<fnAffiliate> FilterObjects(IQueryable<fnAffiliate> affiliates, Func<IQueryable<fnAffiliate>, IOrderedQueryable<fnAffiliate>> orderBy = null)
        {
            affiliates = CreateQuery(affiliates, orderBy);
            return affiliates;
        }

        public long SelectedObjectsCount(IQueryable<fnAffiliate> affiliates)
        {
            affiliates = CreateQuery(affiliates);
            return affiliates.Count();
        }
    }
}
