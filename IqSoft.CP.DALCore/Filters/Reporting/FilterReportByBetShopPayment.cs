using System;
using System.Linq;

namespace IqSoft.CP.DAL.Filters.Reporting
{
    public class FilterReportByBetShopPayment : FilterBase<fnReportByBetShopOperation>
    {
        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public FiltersOperation BetShopIds { get; set; }

        public FiltersOperation GroupIds { get; set; }

        public FiltersOperation BetShopNames { get; set; }

        public FiltersOperation PendingDepositCounts { get; set; }

        public FiltersOperation PendingDepositAmounts { get; set; }

        public FiltersOperation PayedDepositCounts { get; set; }

        public FiltersOperation PayedDepositAmounts { get; set; }

        public FiltersOperation CanceledDepositCounts { get; set; }

        public FiltersOperation CanceledDepositAmounts { get; set; }

        public FiltersOperation PendingWithdrawalCounts { get; set; }

        public FiltersOperation PendingWithdrawalAmounts { get; set; }

        public FiltersOperation PayedWithdrawalCounts { get; set; }

        public FiltersOperation PayedWithdrawalAmounts { get; set; }

        public FiltersOperation CanceledWithdrawalCounts { get; set; }

        public FiltersOperation CanceledWithdrawalAmounts { get; set; }

        protected override IQueryable<fnReportByBetShopOperation> CreateQuery(IQueryable<fnReportByBetShopOperation> objects, Func<IQueryable<fnReportByBetShopOperation>, IOrderedQueryable<fnReportByBetShopOperation>> orderBy = null)
        {

            FilterByValue(ref objects, BetShopIds, "Id");
            FilterByValue(ref objects, GroupIds, "GroupId");
            FilterByValue(ref objects, PendingDepositCounts, "TotalPandingDepositsCount");
            FilterByValue(ref objects, BetShopNames, "BetShopName");
            FilterByValue(ref objects, PendingDepositAmounts, "TotalPandingDepositsAmount");
            FilterByValue(ref objects, PayedDepositCounts, "TotalPayedDepositsCount");
            FilterByValue(ref objects, PayedDepositAmounts, "TotalPayedDepositsAmount");
            FilterByValue(ref objects, CanceledDepositCounts, "TotalCanceledDepositsCount");
            FilterByValue(ref objects, CanceledDepositAmounts, "TotalCanceledDepositsAmount");
            FilterByValue(ref objects, PendingWithdrawalCounts, "TotalPandingWithdrawalsCount");
            FilterByValue(ref objects, PendingWithdrawalAmounts, "TotalPandingWithdrawalsAmount");
            FilterByValue(ref objects, PayedWithdrawalCounts, "TotalPayedWithdrawalsCount");
            FilterByValue(ref objects, PayedWithdrawalAmounts, "TotalPayedWithdrawalsAmount");
            FilterByValue(ref objects, CanceledWithdrawalCounts, "TotalCanceledWithdrawalsCount");
            FilterByValue(ref objects, CanceledWithdrawalAmounts, "TotalCanceledWithdrawalsAmount");

            return base.FilteredObjects(objects, orderBy);
        }

        public IQueryable<fnReportByBetShopOperation> FilterObjects(IQueryable<fnReportByBetShopOperation> objects, Func<IQueryable<fnReportByBetShopOperation>, IOrderedQueryable<fnReportByBetShopOperation>> orderBy = null)
        {
            objects = CreateQuery(objects, orderBy);
            return objects;
        }
    }
}