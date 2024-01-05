using System;
using System.Linq;
using IqSoft.CP.DAL.Models.Cache;

namespace IqSoft.CP.DAL.Filters
{
    public class FilterRealTime : FilterBase<BllOnlineClient>
    {
        public int? PartnerId { get; set; }

        public FiltersOperation ClientIds { get; set; }

        public FiltersOperation LanguageIds { get; set; }

        public FiltersOperation Names { get; set; }

        public FiltersOperation UserNames { get; set; }

        public FiltersOperation Categories { get; set; }

        public FiltersOperation RegionIds { get; set; }

        public FiltersOperation Currencies { get; set; }

        public FiltersOperation LoginIps { get; set; }

        public FiltersOperation Balances { get; set; }

        public FiltersOperation TotalDepositsCounts { get; set; }

        public FiltersOperation TotalDepositsAmounts { get; set; }

        public FiltersOperation TotalWithdrawalsCounts { get; set; }

        public FiltersOperation TotalWithdrawalsAmounts { get; set; }

        public FiltersOperation TotalBetsCounts { get; set; }

        public FiltersOperation GGRs { get; set; }

        protected override IQueryable<BllOnlineClient> CreateQuery(IQueryable<BllOnlineClient> objects, Func<IQueryable<BllOnlineClient>, IOrderedQueryable<BllOnlineClient>> orderBy = null)
        {
            if (PartnerId.HasValue)
                objects = objects.Where(x => x.PartnerId == PartnerId.Value);




            FilterByValue(ref objects, ClientIds, "Id");
            FilterByValue(ref objects, LanguageIds, "SessionLanguage");
            FilterByValue(ref objects, UserNames, "UserName");
            FilterByValue(ref objects, Categories, "CategoryId");
            FilterByValue(ref objects, RegionIds, "RegionId");
            FilterByValue(ref objects, Currencies, "CurrencyId");
            FilterByValue(ref objects, LoginIps, "LoginIp");
            FilterByValue(ref objects, Balances, "Balance");
            FilterByValue(ref objects, TotalDepositsCounts, "TotalDepositsCount");
            FilterByValue(ref objects, TotalDepositsAmounts, "TotalDepositsAmount");
            FilterByValue(ref objects, TotalWithdrawalsCounts, "TotalWithdrawalsCount");
            FilterByValue(ref objects, TotalWithdrawalsAmounts, "TotalWithdrawalsAmount");
            FilterByValue(ref objects, TotalBetsCounts, "TotalBetsCount");
            FilterByValue(ref objects, GGRs, "GGR");
            FilterByValue(ref objects, Names, "FirstName", "LastName");

            return base.FilteredObjects(objects, orderBy);
        }

        public IQueryable<BllOnlineClient> FilterObjects(IQueryable<BllOnlineClient> BllOnlineClients, Func<IQueryable<BllOnlineClient>, IOrderedQueryable<BllOnlineClient>> orderBy = null)
        {
            BllOnlineClients = CreateQuery(BllOnlineClients, orderBy);
            return BllOnlineClients;
        }

        public long SelectedObjectsCount(IQueryable<BllOnlineClient> BllOnlineClients)
        {
            BllOnlineClients = CreateQuery(BllOnlineClients);
            return BllOnlineClients.Count();
        }
    }
}
