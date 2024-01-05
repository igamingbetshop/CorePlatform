using System;
using System.Linq;
using IqSoft.CP.Common.Models;

namespace IqSoft.CP.DAL.Filters
{
    public class FilterCorrection : FilterBase<fnCorrection>
    {
        public int? ClientId { get; set; }
        public long? AccountId { get; set; }
        public int? UserId { get; set; }
        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public FiltersOperation Ids { get; set; }
        public FiltersOperation ClientIds { get; set; }
        public FiltersOperation ClientUserNames { get; set; }
        public FiltersOperation Amounts { get; set; }
        public FiltersOperation CurrencyIds { get; set; }
        public FiltersOperation States { get; set; }
        public FiltersOperation UserIds { get; set; }
        public FiltersOperation OperationTypeNames { get; set; }
        public FiltersOperation OperationTypeIds { get; set; }
        public FiltersOperation FirstNames { get; set; }
        public FiltersOperation LastNames { get; set; }
        public FiltersOperation Creators { get; set; }
        public FiltersOperation ProductNames { get; set; }

        protected override IQueryable<fnCorrection> CreateQuery(IQueryable<fnCorrection> objects, Func<IQueryable<fnCorrection>, IOrderedQueryable<fnCorrection>> orderBy = null)
        {
            if (ClientId.HasValue)
                objects = objects.Where(x => x.ClientId == ClientId.Value);
            else if (UserId.HasValue)
                objects = objects.Where(x => x.UserId == UserId.Value);
            var fDate = FromDate.Year * 1000000 + FromDate.Month * 10000 + FromDate.Day * 100 + FromDate.Hour;
            var tDate = ToDate.Year * 1000000 + ToDate.Month * 10000 + ToDate.Day * 100 + ToDate.Hour;
            objects = objects.Where(x => x.Date >= fDate);
            objects = objects.Where(x => x.Date < tDate);
            if(AccountId != null)
            {
                objects = objects.Where(x => x.AccountId == AccountId.Value);
            }

            FilterByValue(ref objects, Ids, "Id");
            FilterByValue(ref objects, ClientIds, "ClientId");
            FilterByValue(ref objects, ClientUserNames, "ClientUserName");
            FilterByValue(ref objects, Amounts, "Amount");
            FilterByValue(ref objects, UserIds, "UserId");
            FilterByValue(ref objects, Creators, "Creator");
            FilterByValue(ref objects, CurrencyIds, "CurrencyId");
            FilterByValue(ref objects, States, "State");
            FilterByValue(ref objects, OperationTypeNames, "OperationTypeName");
            FilterByValue(ref objects, OperationTypeIds, "DocumentTypeId");
            FilterByValue(ref objects, FirstNames, "FirstName");
            FilterByValue(ref objects, LastNames, "LastName");
            FilterByValue(ref objects, ProductNames, "ProductNickName");

            return base.FilteredObjects(objects, orderBy);
        }

        public IQueryable<fnCorrection> FilterObjects(IQueryable<fnCorrection> documents, Func<IQueryable<fnCorrection>, IOrderedQueryable<fnCorrection>> orderBy = null)
        {
            return CreateQuery(documents, orderBy);
        }

        public long SelectedObjectsCount(IQueryable<fnCorrection> documents, Func<IQueryable<fnCorrection>, IOrderedQueryable<fnCorrection>> orderBy = null)
        {
            documents = CreateQuery(documents, orderBy);
            return documents.Count();
        }
    }
}
