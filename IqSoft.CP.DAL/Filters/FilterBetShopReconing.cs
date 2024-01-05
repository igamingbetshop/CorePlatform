using IqSoft.CP.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using IqSoft.CP.Common.Models;

namespace IqSoft.CP.DAL.Filters
{
    public class FilterfnBetShopReconing : FilterBase<fnBetShopReconing>
    {
        public int? PartnerId { get; set; }

        public List<FiltersOperationType> Ids { get; set; }

        public List<FiltersOperationType> UserIds { get; set; }

        public List<FiltersOperationType> Currencies { get; set; }

        public List<FiltersOperationType> BetShopIds { get; set; }

        public List<FiltersOperationType> BetShopNames { get; set; }

        public List<FiltersOperationType> BetShopAvailiableBalances { get; set; }

        public List<FiltersOperationType> Amounts { get; set; }

        public List<FiltersOperationType> CreationTimes { get; set; }

        protected override IQueryable<fnBetShopReconing> CreateQuery(IQueryable<fnBetShopReconing> objects, Func<IQueryable<fnBetShopReconing>,
            IOrderedQueryable<fnBetShopReconing>> orderBy = null)
        {
            if (PartnerId.HasValue)
                objects = objects.Where(x => x.PartnerId == PartnerId.Value);

            #region Ids

            if (Ids != null && Ids.Any())
            {
                foreach (var item in Ids)
                {
                    switch (item.OperationTypeId)
                    {
                        case (int)FilterOperations.IsEqualTo:
                            objects = objects.Where(x => x.Id == item.IntValue);
                            break;
                        case (int)FilterOperations.IsGreaterThenOrEqualTo:
                            objects = objects.Where(x => x.Id >= item.IntValue);
                            break;
                        case (int)FilterOperations.IsGreaterThen:
                            objects = objects.Where(x => x.Id > item.IntValue);
                            break;
                        case (int)FilterOperations.IsLessThenOrEqualTo:
                            objects = objects.Where(x => x.Id <= item.IntValue);
                            break;
                        case (int)FilterOperations.IsLessThen:
                            objects = objects.Where(x => x.Id < item.IntValue);
                            break;
                        case (int)FilterOperations.IsNotEqualTo:
                            objects = objects.Where(x => x.Id != item.IntValue);
                            break;
                    }
                }
            }

            #endregion

            #region UserIds

            if (UserIds != null && UserIds.Any())
            {
                foreach (var item in UserIds)
                {
                    switch (item.OperationTypeId)
                    {
                        case (int)FilterOperations.IsEqualTo:
                            objects = objects.Where(x => x.UserId == item.IntValue);
                            break;
                        case (int)FilterOperations.IsGreaterThenOrEqualTo:
                            objects = objects.Where(x => x.UserId >= item.IntValue);
                            break;
                        case (int)FilterOperations.IsGreaterThen:
                            objects = objects.Where(x => x.UserId > item.IntValue);
                            break;
                        case (int)FilterOperations.IsLessThenOrEqualTo:
                            objects = objects.Where(x => x.UserId <= item.IntValue);
                            break;
                        case (int)FilterOperations.IsLessThen:
                            objects = objects.Where(x => x.UserId < item.IntValue);
                            break;
                        case (int)FilterOperations.IsNotEqualTo:
                            objects = objects.Where(x => x.UserId != item.IntValue);
                            break;
                    }
                }
            }

            #endregion

            #region Currencies

            if (Currencies != null && Currencies.Any())
            {
                foreach (var item in Currencies)
                {
                    switch (item.OperationTypeId)
                    {
                        case (int)FilterOperations.IsEqualTo:
                            objects = objects.Where(x => x.CurrencyId == item.StringValue);
                            break;
                        case (int)FilterOperations.IsNotEqualTo:
                            objects = objects.Where(x => x.CurrencyId != item.StringValue);
                            break;
                    }
                }
            }

            #endregion

            #region BetShopIds

            if (BetShopIds != null && BetShopIds.Any())
            {
                foreach (var item in BetShopIds)
                {
                    switch (item.OperationTypeId)
                    {
                        case (int)FilterOperations.IsEqualTo:
                            objects = objects.Where(x => x.BetShopId == item.IntValue);
                            break;
                        case (int)FilterOperations.IsGreaterThenOrEqualTo:
                            objects = objects.Where(x => x.BetShopId >= item.IntValue);
                            break;
                        case (int)FilterOperations.IsGreaterThen:
                            objects = objects.Where(x => x.BetShopId > item.IntValue);
                            break;
                        case (int)FilterOperations.IsLessThenOrEqualTo:
                            objects = objects.Where(x => x.BetShopId <= item.IntValue);
                            break;
                        case (int)FilterOperations.IsLessThen:
                            objects = objects.Where(x => x.BetShopId < item.IntValue);
                            break;
                        case (int)FilterOperations.IsNotEqualTo:
                            objects = objects.Where(x => x.BetShopId != item.IntValue);
                            break;
                    }
                }
            }

            #endregion

            #region BetShopNames


            #endregion

            #region BetShopAvailiableBalances

            if (BetShopAvailiableBalances != null && BetShopAvailiableBalances.Any())
            {
                foreach (var item in BetShopAvailiableBalances)
                {
                    switch (item.OperationTypeId)
                    {
                        case (int)FilterOperations.IsEqualTo:
                            objects = objects.Where(x => x.BetShopAvailiableBalance == item.DecimalValue);
                            break;
                        case (int)FilterOperations.IsGreaterThenOrEqualTo:
                            objects = objects.Where(x => x.BetShopAvailiableBalance >= item.DecimalValue);
                            break;
                        case (int)FilterOperations.IsGreaterThen:
                            objects = objects.Where(x => x.BetShopAvailiableBalance > item.DecimalValue);
                            break;
                        case (int)FilterOperations.IsLessThenOrEqualTo:
                            objects = objects.Where(x => x.BetShopAvailiableBalance <= item.DecimalValue);
                            break;
                        case (int)FilterOperations.IsLessThen:
                            objects = objects.Where(x => x.BetShopAvailiableBalance < item.DecimalValue);
                            break;
                        case (int)FilterOperations.IsNotEqualTo:
                            objects = objects.Where(x => x.BetShopAvailiableBalance != item.DecimalValue);
                            break;
                    }
                }
            }

            #endregion

            #region Amounts

            if (Amounts != null && Amounts.Any())
            {
                foreach (var item in Amounts)
                {
                    switch (item.OperationTypeId)
                    {
                        case (int)FilterOperations.IsEqualTo:
                            objects = objects.Where(x => x.Amount == item.DecimalValue);
                            break;
                        case (int)FilterOperations.IsGreaterThenOrEqualTo:
                            objects = objects.Where(x => x.Amount >= item.DecimalValue);
                            break;
                        case (int)FilterOperations.IsGreaterThen:
                            objects = objects.Where(x => x.Amount > item.DecimalValue);
                            break;
                        case (int)FilterOperations.IsLessThenOrEqualTo:
                            objects = objects.Where(x => x.Amount <= item.DecimalValue);
                            break;
                        case (int)FilterOperations.IsLessThen:
                            objects = objects.Where(x => x.Amount < item.DecimalValue);
                            break;
                        case (int)FilterOperations.IsNotEqualTo:
                            objects = objects.Where(x => x.Amount != item.DecimalValue);
                            break;
                    }
                }
            }

            #endregion

            #region CreationTimes

            if (CreationTimes != null && CreationTimes.Any())
            {
                foreach (var item in CreationTimes)
                {
                    switch (item.OperationTypeId)
                    {
                        case (int)FilterOperations.IsEqualTo:
                            objects = objects.Where(x => x.CreationTime == item.DateTimeValue);
                            break;
                        case (int)FilterOperations.IsGreaterThenOrEqualTo:
                            objects = objects.Where(x => x.CreationTime >= item.DateTimeValue);
                            break;
                        case (int)FilterOperations.IsGreaterThen:
                            objects = objects.Where(x => x.CreationTime > item.DateTimeValue);
                            break;
                        case (int)FilterOperations.IsLessThenOrEqualTo:
                            objects = objects.Where(x => x.CreationTime <= item.DateTimeValue);
                            break;
                        case (int)FilterOperations.IsLessThen:
                            objects = objects.Where(x => x.CreationTime < item.DateTimeValue);
                            break;
                        case (int)FilterOperations.IsNotEqualTo:
                            objects = objects.Where(x => x.CreationTime != item.DateTimeValue);
                            break;
                    }
                }
            }

            #endregion

            return base.FilteredObjects(objects, orderBy);
        }

        public IQueryable<fnBetShopReconing> FilterObjects(IQueryable<fnBetShopReconing> betShopReconings, Func<IQueryable<fnBetShopReconing>,
            IOrderedQueryable<fnBetShopReconing>> orderBy = null)
        {
            betShopReconings = CreateQuery(betShopReconings, orderBy);
            return betShopReconings;
        }

        public long SelectedObjectsCount(IQueryable<fnBetShopReconing> betShopReconings)
        {
            betShopReconings = CreateQuery(betShopReconings);
            return betShopReconings.Count();
        }
    }
}
