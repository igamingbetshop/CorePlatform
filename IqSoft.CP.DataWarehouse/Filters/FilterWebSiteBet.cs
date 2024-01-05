using LinqKit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IqSoft.CP.DataWarehouse.Filters
{
    public class FilterWebSiteBet : FilterBase<fnInternetBet>
    {
        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public int ClientId { get; set; }
		
        public List<int> ProductIds { get; set; }

        public int? State { get; set; }

        public int? GroupId { get; set; }

        public List<long> AccountIds { get; set; }

        protected override IQueryable<fnInternetBet> CreateQuery(IQueryable<fnInternetBet> objects, Func<IQueryable<fnInternetBet>, IOrderedQueryable<fnInternetBet>> orderBy = null)
        {
            var fDate = FromDate.Year * 1000000 + FromDate.Month * 10000 + FromDate.Day * 100 + FromDate.Hour;         
            objects = objects.Where(x => x.Date >= fDate);
            var tDate = ToDate.Year * 1000000 + ToDate.Month * 10000 + ToDate.Day * 100 + ToDate.Hour;
            objects = objects.Where(x => x.Date < tDate);
			objects = objects.Where(x => x.ClientId == ClientId);
			if(State != null)
				objects = objects.Where(x => x.State == State.Value);
            if(GroupId != null)
                objects = objects.Where(x => x.ProductCategoryId == GroupId.Value);

            if (ProductIds != null && ProductIds.Any())
			{
				var predicate = PredicateBuilder.New<fnInternetBet>(false);
				foreach (var item in ProductIds)
				{
					predicate = predicate.Or(x => x.ProductId == item);
				}
				objects = objects.Where(predicate);
			}
            if (AccountIds != null && AccountIds.Any())
            {
                var predicate = PredicateBuilder.New<fnInternetBet>(false);
                foreach (var item in AccountIds)
                {
                    predicate = predicate.Or(x => x.AccountId == item);
                }
                objects = objects.Where(predicate);
            }
            return base.FilteredObjects(objects, orderBy);
        }

        public IQueryable<fnInternetBet> FilterObjects(IQueryable<fnInternetBet> internetBets, Func<IQueryable<fnInternetBet>, IOrderedQueryable<fnInternetBet>> orderBy = null)
        {
            internetBets = CreateQuery(internetBets, orderBy);
            return internetBets;
        }

        public long SelectedObjectsCount(IQueryable<fnInternetBet> internetBets)
        {
            internetBets = CreateQuery(internetBets);
            return internetBets.Count();
        }
    }
}
