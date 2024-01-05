using System;
using System.Linq;
using IqSoft.CP.Common.Models;

namespace IqSoft.CP.DAL.Filters.Reporting
{
    public class FilterReportByUserTransaction : FilterBase<fnReportByUserTransaction>
    {
        public int? PartnerId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public FiltersOperation PartnerIds { get; set; }
        public FiltersOperation UserIds { get; set; }
        public FiltersOperation Usernames { get; set; }
        public FiltersOperation NickNames { get; set; }
        public FiltersOperation UserFirstNames { get; set; }
        public FiltersOperation UserLastNames { get; set; }
        public FiltersOperation FromUserIds { get; set; }
        public FiltersOperation FromUsernames { get; set; }
        public FiltersOperation ClientIds { get; set; }
        public FiltersOperation ClientUsernames { get; set; }
        public FiltersOperation OperationTypeIds { get; set; }
        public FiltersOperation Amounts { get; set; }
        public FiltersOperation CurrencyIds { get; set; }

        protected override IQueryable<fnReportByUserTransaction> CreateQuery(IQueryable<fnReportByUserTransaction> objects, Func<IQueryable<fnReportByUserTransaction>, IOrderedQueryable<fnReportByUserTransaction>> orderBy = null)
        {
            if (PartnerId.HasValue)
                objects = objects.Where(x => x.PartnerId == PartnerId.Value);

            FilterByValue(ref objects, PartnerIds, "PartnerId");
            FilterByValue(ref objects, UserIds, "UserId");
            FilterByValue(ref objects, Usernames, "Username");
            FilterByValue(ref objects, NickNames, "NickName");
            FilterByValue(ref objects, UserFirstNames, "UserFirstName");
            FilterByValue(ref objects, UserLastNames, "UserLastName");
            FilterByValue(ref objects, FromUserIds, "FromUserId");
            FilterByValue(ref objects, FromUsernames, "FromUsername");
            FilterByValue(ref objects, ClientIds, "ClientId");
            FilterByValue(ref objects, ClientUsernames, "ClientUsername");
            FilterByValue(ref objects, OperationTypeIds, "OperationTypeId");
            FilterByValue(ref objects, Amounts, "Amount");
            FilterByValue(ref objects, CurrencyIds, "CurrencyId");

            return base.FilteredObjects(objects, orderBy);
        }

        public IQueryable<fnReportByUserTransaction> FilterObjects(IQueryable<fnReportByUserTransaction> objects, Func<IQueryable<fnReportByUserTransaction>, IOrderedQueryable<fnReportByUserTransaction>> orderBy = null)
        {
            objects = CreateQuery(objects, orderBy);
            return objects;
        }
        public long SelectedObjectsCount(IQueryable<fnReportByUserTransaction> userTransaction, Func<IQueryable<fnReportByUserTransaction>, IOrderedQueryable<fnReportByUserTransaction>> orderBy = null)
        {
            userTransaction = CreateQuery(userTransaction, orderBy);
            return userTransaction.Count();
        }
    }
}
