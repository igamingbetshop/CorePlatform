using System;
using System.Linq;
using IqSoft.CP.Common.Models;

namespace IqSoft.CP.DAL.Filters.Clients
{
    public class FilterClientExclusion : FilterBase<fnReportByClientExclusion>
    {
        public int? PartnerId { get; set; }
        public FiltersOperation PartnerIds { get; set; }
        public FiltersOperation ClientIds { get; set; }
        public FiltersOperation Usernames { get; set; }
        public FiltersOperation DepositLimitDailys { get; set; }
        public FiltersOperation DepositLimitWeeklys { get; set; }
        public FiltersOperation DepositLimitMonthlys { get; set; }
        public FiltersOperation TotalBetAmountLimitDailys { get; set; }
        public FiltersOperation TotalBetAmountLimitWeeklys { get; set; }
        public FiltersOperation TotalBetAmountLimitMonthlys { get; set; }
        public FiltersOperation TotalLossLimitDailys { get; set; }
        public FiltersOperation TotalLossLimitWeeklys { get; set; }
        public FiltersOperation TotalLossLimitMonthlys { get; set; }
        public FiltersOperation SystemDepositLimitDailys { get; set; }
        public FiltersOperation SystemDepositLimitWeeklys { get; set; }
        public FiltersOperation SystemDepositLimitMonthlys { get; set; }
        public FiltersOperation SystemTotalBetAmountLimitDailys { get; set; }
        public FiltersOperation SystemTotalBetAmountLimitWeeklys { get; set; }
        public FiltersOperation SystemTotalBetAmountLimitMonthlys { get; set; }
        public FiltersOperation SystemTotalLossLimitDailys { get; set; }
        public FiltersOperation SystemTotalLossLimitWeeklys { get; set; }
        public FiltersOperation SystemTotalLossLimitMonthlys { get; set; }
        public FiltersOperation SessionLimits { get; set; }
        public FiltersOperation SystemSessionLimits { get; set; }

        protected override IQueryable<fnReportByClientExclusion> CreateQuery(IQueryable<fnReportByClientExclusion> objects, Func<IQueryable<fnReportByClientExclusion>, IOrderedQueryable<fnReportByClientExclusion>> orderBy = null)
        {
            if (PartnerId.HasValue)
                objects = objects.Where(x => x.PartnerId == PartnerId);
            FilterByValue(ref objects, PartnerIds, "PartnerId");
            FilterByValue(ref objects, ClientIds, "ClientId");
            FilterByValue(ref objects, Usernames, "Usernames");
            FilterByValue(ref objects, DepositLimitDailys, "DepositLimitDaily");
            FilterByValue(ref objects, DepositLimitWeeklys, "DepositLimitWeekly");
            FilterByValue(ref objects, DepositLimitMonthlys, "DepositLimitMonthly");
            FilterByValue(ref objects, TotalBetAmountLimitDailys, "TotalBetAmountLimitDaily");
            FilterByValue(ref objects, TotalBetAmountLimitWeeklys, "TotalBetAmountLimitWeekly");
            FilterByValue(ref objects, TotalBetAmountLimitMonthlys, "TotalBetAmountLimitMonthly");
            FilterByValue(ref objects, TotalLossLimitDailys, "TotalLossLimitDaily");
            FilterByValue(ref objects, TotalLossLimitWeeklys, "TotalLossLimitWeekly");
            FilterByValue(ref objects, TotalLossLimitMonthlys, "TotalLossLimitMonthly");
            FilterByValue(ref objects, SystemDepositLimitDailys, "SystemDepositLimitDaily");
            FilterByValue(ref objects, SystemDepositLimitWeeklys, "SystemDepositLimitWeekly");
            FilterByValue(ref objects, SystemDepositLimitMonthlys, "SystemDepositLimitMonthly");
            FilterByValue(ref objects, SystemTotalBetAmountLimitDailys, "SystemTotalBetAmountLimitDaily");
            FilterByValue(ref objects, SystemTotalBetAmountLimitWeeklys, "SystemTotalBetAmountLimitWeekly");
            FilterByValue(ref objects, SystemTotalBetAmountLimitMonthlys, "SystemTotalBetAmountLimitMonthly");
            FilterByValue(ref objects, SystemTotalLossLimitDailys, "SystemTotalLossLimitDaily");
            FilterByValue(ref objects, SystemTotalLossLimitWeeklys, "SystemTotalLossLimitWeekly");
            FilterByValue(ref objects, SystemTotalLossLimitMonthlys, "SystemTotalLossLimitMonthly");
            FilterByValue(ref objects, SessionLimits, "SessionLimit");
            FilterByValue(ref objects, SystemSessionLimits, "SystemSessionLimit");

            return base.FilteredObjects(objects, orderBy);
        }

        public IQueryable<fnReportByClientExclusion> FilterObjects(IQueryable<fnReportByClientExclusion> clientExclusions,
                                                                   Func<IQueryable<fnReportByClientExclusion>, 
                                                                   IOrderedQueryable<fnReportByClientExclusion>> orderBy = null)
        {
            clientExclusions = CreateQuery(clientExclusions, orderBy);
            return clientExclusions;
        }

        public long SelectedObjectsCount(IQueryable<fnReportByClientExclusion> clientExclusions)
        {
            clientExclusions = CreateQuery(clientExclusions);
            return clientExclusions.Count();
        }
    }
}
