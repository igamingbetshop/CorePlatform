using System;
using System.Collections.Generic;
using System.Linq;
using IqSoft.CP.Common.Models;

namespace IqSoft.CP.DAL.Filters
{
    public class FilterfnUser : FilterBase<fnUser>
    {
        public int? PartnerId { get; set; }

        public int? ParentId { get; set; }
        public int? IdentityId { get; set; }

        public List<int> Types { get; set; }

        public FiltersOperation Ids { get; set; }

        public FiltersOperation FirstNames { get; set; }

        public FiltersOperation LastNames { get; set; }

        public FiltersOperation UserNames { get; set; }
        public FiltersOperation NickNames { get; set; }
        public FiltersOperation MobileNumbers { get; set; }

        public FiltersOperation Emails { get; set; }

        public FiltersOperation Genders { get; set; }

        public FiltersOperation Currencies { get; set; }

        public FiltersOperation LanguageIds { get; set; }

        public FiltersOperation UserStates { get; set; }

        public FiltersOperation UserTypes { get; set; }
        public FiltersOperation UserRoles { get; set; }


        protected override IQueryable<fnUser> CreateQuery(IQueryable<fnUser> objects, Func<IQueryable<fnUser>, IOrderedQueryable<fnUser>> orderBy = null)
        {
            if (IdentityId.HasValue)
                objects = objects.Where(x => x.Id != IdentityId.Value);
            if (PartnerId.HasValue)
                objects = objects.Where(x => x.PartnerId == PartnerId.Value);
            if (ParentId.HasValue)
                objects = objects.Where(x => x.ParentId == ParentId.Value);
            if (Types != null && Types.Any())
                objects = objects.Where(x => Types.Contains(x.Type));

            FilterByValue(ref objects, Ids, "Id");
            FilterByValue(ref objects, FirstNames, "FirstName");
            FilterByValue(ref objects, LastNames, "LastName");
            FilterByValue(ref objects, UserNames, "UserName");
            FilterByValue(ref objects, NickNames, "NickName");
            FilterByValue(ref objects, MobileNumbers, "MobileNumber");
            FilterByValue(ref objects, Emails, "Email");
            FilterByValue(ref objects, Genders, "Gender");
            FilterByValue(ref objects, Currencies, "CurrencyId");
            FilterByValue(ref objects, LanguageIds, "LanguageId");
            FilterByValue(ref objects, UserStates, "State");
            FilterByValue(ref objects, UserTypes, "Type");
            FilterByValue(ref objects, UserRoles, "UserRoles");


            return base.FilteredObjects(objects, orderBy);
        }

        public IQueryable<fnUser> FilterObjects(IQueryable<fnUser> users, Func<IQueryable<fnUser>, IOrderedQueryable<fnUser>> orderBy = null)
        {
            users = CreateQuery(users, orderBy);
            return users;
        }

        public long SelectedObjectsCount(IQueryable<fnUser> users)
        {
            users = CreateQuery(users);
            return users.Count();
        }
    }
}
