using System;
using System.Linq;

namespace IqSoft.CP.DAL.Filters
{
    public class FilterUserCorrection : FilterBase<fnUserCorrection>
    {
        public int? ClientId { get; set; }
        public int? UserId { get; set; }
        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public FiltersOperation Ids { get; set; }
        public FiltersOperation ClientIds { get; set; }
        public FiltersOperation Amounts { get; set; }
        public FiltersOperation CurrencyIds { get; set; }
        public FiltersOperation States { get; set; }
        public FiltersOperation UserIds { get; set; }
        public FiltersOperation OperationTypeNames { get; set; }
        public FiltersOperation CreatorFirstNames { get; set; }
        public FiltersOperation CreatorLastNames { get; set; }
        public FiltersOperation UserFirstNames { get; set; }
        public FiltersOperation UserLastNames { get; set; }
        public FiltersOperation ClientFirstNames { get; set; }
        public FiltersOperation ClientLastNames { get; set; }
        public FiltersOperation Creators { get; set; }
        public FiltersOperation ProductNames { get; set; }

        protected override IQueryable<fnUserCorrection> CreateQuery(IQueryable<fnUserCorrection> objects, Func<IQueryable<fnUserCorrection>, 
            IOrderedQueryable<fnUserCorrection>> orderBy = null)
        {
            if (ClientId.HasValue)
                objects = objects.Where(x => x.ClientId == ClientId.Value);
            else if (UserId.HasValue)
                objects = objects.Where(x => x.UserId == UserId.Value || x.Creator == UserId.Value);
            var fDate = FromDate.Year * 1000000 + FromDate.Month * 10000 + FromDate.Day * 100 + FromDate.Hour;
            var tDate = ToDate.Year * 1000000 + ToDate.Month * 10000 + ToDate.Day * 100 + ToDate.Hour;
            objects = objects.Where(x => x.Date >= fDate);
            objects = objects.Where(x => x.Date < tDate);

            FilterByValue(ref objects, Ids, "Id");
            FilterByValue(ref objects, ClientIds, "ClientId");
            FilterByValue(ref objects, Amounts, "Amount");
            FilterByValue(ref objects, UserIds, "UserId");
            FilterByValue(ref objects, Creators, "Creator");
            FilterByValue(ref objects, CurrencyIds, "CurrencyId");
            FilterByValue(ref objects, States, "State");
            FilterByValue(ref objects, OperationTypeNames, "OperationTypeName");
            FilterByValue(ref objects, CreatorFirstNames, "CreatorFirstName");
            FilterByValue(ref objects, CreatorLastNames, "CreatorLastName");
            FilterByValue(ref objects, UserFirstNames, "UserFirstName");
            FilterByValue(ref objects, UserLastNames, "UserLastName");
            FilterByValue(ref objects, ClientFirstNames, "ClientFirstName");
            FilterByValue(ref objects, ClientLastNames, "ClientLastName");
            FilterByValue(ref objects, ProductNames, "ProductNickName");

            return base.FilteredObjects(objects, orderBy);
        }

        public IQueryable<fnUserCorrection> FilterObjects(IQueryable<fnUserCorrection> documents, Func<IQueryable<fnUserCorrection>, IOrderedQueryable<fnUserCorrection>> orderBy = null)
        {
            return CreateQuery(documents, orderBy);
        }

        public long SelectedObjectsCount(IQueryable<fnUserCorrection> documents, Func<IQueryable<fnUserCorrection>, IOrderedQueryable<fnUserCorrection>> orderBy = null)
        {
            documents = CreateQuery(documents, orderBy);
            return documents.Count();
        }
    }
}
