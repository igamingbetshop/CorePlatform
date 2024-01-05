using System;
using System.Linq;

namespace IqSoft.CP.DAL.Filters.Reporting
{
    public class FilterReportByAgentTranfer : FilterBase<fnReportByAgentTransfer>
    {
        public int? PartnerId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public FiltersOperation PartnerIds { get; set; }
        public FiltersOperation UserIds { get; set; }
        public FiltersOperation UserNames { get; set; }
        public FiltersOperation NickNames { get; set; }
        public FiltersOperation TotoalProfits { get; set; }
        public FiltersOperation TotalDebits { get; set; }
        public FiltersOperation Balances { get; set; }

        protected override IQueryable<fnReportByAgentTransfer> CreateQuery(IQueryable<fnReportByAgentTransfer> objects, Func<IQueryable<fnReportByAgentTransfer>, IOrderedQueryable<fnReportByAgentTransfer>> orderBy = null)
        {
            if (PartnerId.HasValue)
                objects = objects.Where(x => x.PartnerId == PartnerId.Value);

            FilterByValue(ref objects, PartnerIds, "PartnerId");
            FilterByValue(ref objects, UserIds, "UserId");
            FilterByValue(ref objects, UserNames, "UserName");
            FilterByValue(ref objects, NickNames, "NickName");
            FilterByValue(ref objects, TotoalProfits, "TotoalProfit");
            FilterByValue(ref objects, TotalDebits, "TotalDebit");
            FilterByValue(ref objects, Balances, "Balances");

            return base.FilteredObjects(objects, orderBy);
        }

        public IQueryable<fnReportByAgentTransfer> FilterObjects(IQueryable<fnReportByAgentTransfer> objects, Func<IQueryable<fnReportByAgentTransfer>, IOrderedQueryable<fnReportByAgentTransfer>> orderBy = null)
        {
            objects = CreateQuery(objects, orderBy);
            return objects;
        }
        public long SelectedObjectsCount(IQueryable<fnReportByAgentTransfer> agentTranfer, Func<IQueryable<fnReportByAgentTransfer>, IOrderedQueryable<fnReportByAgentTransfer>> orderBy = null)
        {
            agentTranfer = CreateQuery(agentTranfer, orderBy);
            return agentTranfer.Count();
        }
    }
}
