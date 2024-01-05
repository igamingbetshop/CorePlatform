using System;
using System.Linq;
using IqSoft.CP.Common.Models;

namespace IqSoft.CP.DAL.Filters
{
    public class FilterUser : FilterBase<User>
    {
        public int? PartnerId { get; set; }

        public FiltersOperation Ids { get; set; }

        public FiltersOperation FirstNames { get; set; }

        public FiltersOperation LastNames { get; set; }

        public FiltersOperation UserNames { get; set; }

        public FiltersOperation Emails { get; set; }

        public FiltersOperation Genders { get; set; }

        public FiltersOperation Currencies { get; set; }

        public FiltersOperation LanguageIds { get; set; }

        public FiltersOperation UserStates { get; set; }

        public FiltersOperation UserTypes { get; set; }

        protected override IQueryable<User> CreateQuery(IQueryable<User> objects, Func<IQueryable<User>, IOrderedQueryable<User>> orderBy = null)
        {
            if (PartnerId.HasValue)
                objects = objects.Where(x => x.PartnerId == PartnerId.Value);

            FilterByValue(ref objects, Ids, "Id");
            FilterByValue(ref objects, FirstNames, "FirstName");
            FilterByValue(ref objects, LastNames, "LastName");
            FilterByValue(ref objects, UserNames, "UserName");
            FilterByValue(ref objects, Emails, "Email");
            FilterByValue(ref objects, Genders, "Gender");
            FilterByValue(ref objects, Currencies, "CurrencyId");
            FilterByValue(ref objects, LanguageIds, "LanguageId");
            FilterByValue(ref objects, UserStates, "State");
            FilterByValue(ref objects, UserTypes, "Type");


            return base.FilteredObjects(objects, orderBy);
        }

        public IQueryable<User> FilterObjects(IQueryable<User> users, Func<IQueryable<User>, IOrderedQueryable<User>> orderBy = null)
        {
            users = CreateQuery(users, orderBy);
            return users;
        }

        public long SelectedObjectsCount(IQueryable<User> users)
        {
            users = CreateQuery(users);
            return users.Count();
        }
    }
}
