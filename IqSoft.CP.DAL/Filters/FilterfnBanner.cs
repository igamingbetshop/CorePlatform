using IqSoft.CP.Common.Models;
using System;
using System.Linq;

namespace IqSoft.CP.DAL.Filters
{
    public class FilterfnBanner : FilterBase<fnBanner>
    {
        public int? PartnerId { get; set; }
        public bool? IsEnabled { get; set; }
        public bool? ShowDescription { get; set; }
        public bool? ShowRegistration { get; set; }
        public bool? ShowLogin { get; set; }
        public int? Visibility { get; set; }
        public FiltersOperation StartDates { get; set; }
        public FiltersOperation EndDates { get; set; }
        public FiltersOperation Ids { get; set; }
        public FiltersOperation PartnerIds { get; set; }
        public FiltersOperation Orders { get; set; }
        public FiltersOperation NickNames { get; set; }
        public FiltersOperation Images { get; set; }
        public FiltersOperation Types { get; set; }
        public FiltersOperation FragmentNames { get; set; }
        public FiltersOperation Heads { get; set; }
        public FiltersOperation Bodies { get; set; }

        protected override IQueryable<fnBanner> CreateQuery(IQueryable<fnBanner> objects, Func<IQueryable<fnBanner>, IOrderedQueryable<fnBanner>> orderBy = null)
        {
            if (PartnerId.HasValue)
                objects = objects.Where(x => x.PartnerId == PartnerId);
            if (IsEnabled.HasValue)
                objects = objects.Where(x => x.IsEnabled == IsEnabled);
            if (ShowDescription.HasValue)
                objects = objects.Where(x => x.ShowDescription == ShowDescription);
            if (ShowRegistration.HasValue)
                objects = objects.Where(x => x.ButtonType.HasValue && Convert.ToBoolean(x.ButtonType.ToString().Select(y => y.Equals('1')).AsEnumerable().ElementAtOrDefault(1)) == ShowRegistration);
            if (ShowLogin.HasValue)
                objects = objects.Where(x => x.ButtonType.HasValue && Convert.ToBoolean(x.ButtonType.ToString().Select(y => y.Equals('1')).AsEnumerable().ElementAtOrDefault(2)) == ShowLogin);
            if (Visibility.HasValue)
            {
                if (Visibility.Value == -1)
                    objects = objects.Where(x => x.Visibility == null || x.Visibility == "[]");
                else
                    objects = objects.Where(x => x.Visibility.Contains("[" + Visibility + "]") || x.Visibility.Contains("," + Visibility + ",") ||
                                                 x.Visibility.Contains("[" + Visibility + ",") || x.Visibility.Contains("," + Visibility + "]"));
            }

            FilterByValue(ref objects, Ids, "Id");
            FilterByValue(ref objects, PartnerIds, "PartnerId");
            FilterByValue(ref objects, Orders, "Order");
            FilterByValue(ref objects, NickNames, "NickName");
            FilterByValue(ref objects, Images, "Image");
            FilterByValue(ref objects, Types, "Type");
            FilterByValue(ref objects, FragmentNames, "FragmentName");
            FilterByValue(ref objects, Bodies, "Body");
            FilterByValue(ref objects, Heads, "Head");
            FilterByValue(ref objects, Heads, "Head");
            FilterByValue(ref objects, StartDates, "StartDate");
            FilterByValue(ref objects, EndDates, "EndDate");
            return base.FilteredObjects(objects, orderBy);
        }

        public IQueryable<fnBanner> FilterObjects(IQueryable<fnBanner> fnBanners, Func<IQueryable<fnBanner>, IOrderedQueryable<fnBanner>> orderBy = null)
        {
            return CreateQuery(fnBanners, orderBy);
        }
        public long SelectedObjectsCount(IQueryable<fnBanner> fnBanner)
        {
            return CreateQuery(fnBanner).Count();
        }
    }
}