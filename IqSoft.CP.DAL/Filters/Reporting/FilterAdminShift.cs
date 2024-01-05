using System;
using System.Linq;
using IqSoft.CP.Common.Models;

namespace IqSoft.CP.DAL.Filters.Reporting
{
    public class FilterAdminShift : FilterBase<fnAdminShiftReport>
    {
        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public FiltersOperation Ids { get; set; }

        public FiltersOperation BetShopIds { get; set; }

		public FiltersOperation BetShopGroupIds { get; set; }

		public FiltersOperation BetShopNames { get; set; }

		public FiltersOperation BetShopGroupNames { get; set; }

		public FiltersOperation CashierIds { get; set; }

		public FiltersOperation CashdeskIds { get; set; }

        public FiltersOperation FirstNames { get; set; }

		public FiltersOperation BetAmounts { get; set; }

		public FiltersOperation PayedWinAmounts { get; set; }

		public FiltersOperation DepositAmounts { get; set; }

		public FiltersOperation WithdrawAmounts { get; set; }

		public FiltersOperation DebitCorrectionAmounts { get; set; }

		public FiltersOperation CreditCorrectionAmounts { get; set; }

		public FiltersOperation EndAmounts { get; set; }

        public FiltersOperation StartDates { get; set; }

        public FiltersOperation EndDates { get; set; }

        public FiltersOperation ShiftNumbers { get; set; }

        public FiltersOperation PartnerIds { get; set; }

        public FiltersOperation BonusAmounts { get; set; }

        protected override IQueryable<fnAdminShiftReport> CreateQuery(IQueryable<fnAdminShiftReport> objects, Func<IQueryable<fnAdminShiftReport>, IOrderedQueryable<fnAdminShiftReport>> orderBy = null)
        {
            objects = objects.Where(x => x.StartDate >= FromDate && x.StartDate < ToDate);

            FilterByValue(ref objects, Ids, "ShiftId");
            FilterByValue(ref objects, BetShopIds, "BetShopId");
            FilterByValue(ref objects, BetShopGroupIds, "BetShopGroupId");
            FilterByValue(ref objects, BetShopGroupNames, "BetShopGroupName");
            FilterByValue(ref objects, BetShopNames, "BetShopName");
            FilterByValue(ref objects, CashdeskIds, "CashdeskId");
            FilterByValue(ref objects, CashierIds, "CashierId");
            FilterByValue(ref objects, FirstNames, "FirstName");
			FilterByValue(ref objects, BetAmounts, "BetAmount");
			FilterByValue(ref objects, PayedWinAmounts, "PayedWinAmount");
			FilterByValue(ref objects, DepositAmounts, "DepositAmount");
			FilterByValue(ref objects, WithdrawAmounts, "WithdrawAmount");
			FilterByValue(ref objects, DebitCorrectionAmounts, "DebitCorrectionAmount");
			FilterByValue(ref objects, CreditCorrectionAmounts, "CreditCorrectionAmount");
			FilterByValue(ref objects, EndAmounts, "EndAmount");
            FilterByValue(ref objects, StartDates, "StartDate");
            FilterByValue(ref objects, EndDates, "EndDate");
            FilterByValue(ref objects, ShiftNumbers, "ShiftNumber");
            FilterByValue(ref objects, PartnerIds, "PartnerId");
            FilterByValue(ref objects, BonusAmounts, "BonusAmount");

            return base.FilteredObjects(objects, orderBy);
        }

        public IQueryable<fnAdminShiftReport> FilterObjects(IQueryable<fnAdminShiftReport> shiftReports, Func<IQueryable<fnAdminShiftReport>, IOrderedQueryable<fnAdminShiftReport>> orderBy = null)
        {
            shiftReports = CreateQuery(shiftReports, orderBy);
            return shiftReports;
        }

        public long SelectedObjectsCount(IQueryable<fnAdminShiftReport> shiftReports)
        {
            shiftReports = CreateQuery(shiftReports);
            return shiftReports.Count();
        }
    }
}
