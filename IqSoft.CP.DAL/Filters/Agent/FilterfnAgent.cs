using System;
using System.Linq;
using IqSoft.CP.Common.Models;

namespace IqSoft.CP.DAL.Filters.Agent
{
    public class FilterfnAgent : FilterBase<fnAgent>
    {
        public DateTime? CreatedFrom { get; set; }

        public DateTime? CreatedBefore { get; set; }

        public FiltersOperation Ids { get; set; }

        public FiltersOperation FirstNames { get; set; }

        public FiltersOperation LastNames { get; set; }

        public FiltersOperation UserNames { get; set; }

        public FiltersOperation Emails { get; set; }

        public FiltersOperation Genders { get; set; }

        public FiltersOperation States { get; set; }

        public FiltersOperation UserStates { get; set; }

        public FiltersOperation ClientCounts { get; set; }

        public FiltersOperation Balances { get; set; }

        protected override IQueryable<fnAgent> CreateQuery(IQueryable<fnAgent> objects, Func<IQueryable<fnAgent>, IOrderedQueryable<fnAgent>> orderBy = null)
        {
            FilterByValue(ref objects, Ids, "Id");
            FilterByValue(ref objects, FirstNames, "FirstName");
            FilterByValue(ref objects, LastNames, "LastName");
            FilterByValue(ref objects, UserNames, "UserName");
            FilterByValue(ref objects, Emails, "Email");
            FilterByValue(ref objects, Genders, "Gender");
            FilterByValue(ref objects, UserStates, "State");
            FilterByValue(ref objects, ClientCounts, "ClientCount");
            FilterByValue(ref objects, Balances, "Balance");

            return base.FilteredObjects(objects, orderBy);
        }

        public IQueryable<fnAgent> FilterObjects(IQueryable<fnAgent> agents, Func<IQueryable<fnAgent>, IOrderedQueryable<fnAgent>> orderBy = null)
        {
            agents = CreateQuery(agents, orderBy);
            return agents;
        }

        public long SelectedObjectsCount(IQueryable<fnAgent> agents)
        {
            agents = CreateQuery(agents);
            return agents.Count();
        }
    }
}
