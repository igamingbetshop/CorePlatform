﻿using IqSoft.CP.Common.Enums;
using LinqKit;
using System;
using System.Linq;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.Report;

namespace IqSoft.CP.DataWarehouse.Filters
{
    public class FilterInternetBet : FilterBase<fnInternetBet>
    {
        public int? PartnerId { get; set; }

        public int? AgentId { get; set; }

        public int? ClientId { get; set; }

        public long? AccountId { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public FiltersOperation BetDocumentIds { get; set; }

        public FiltersOperation ClientIds { get; set; }

        public FiltersOperation UserIds { get; set; }

        public FiltersOperation Names { get; set; }
        public FiltersOperation ClientFirstNames { get; set; }
        public FiltersOperation ClientLastNames { get; set; }
        public FiltersOperation ClientUserNames { get; set; }

        public FiltersOperation Categories { get; set; }

        public FiltersOperation ProductIds { get; set; }

        public FiltersOperation ProductNames { get; set; }

        public FiltersOperation ProviderNames { get; set; }

        public FiltersOperation SubproviderIds { get; set; }
        
        public FiltersOperation SubproviderNames { get; set; }

        public FiltersOperation CurrencyIds { get; set; }

        public FiltersOperation RoundIds { get; set; }

        public FiltersOperation DeviceTypes { get; set; }

        public FiltersOperation ClientIps { get; set; }

        public FiltersOperation Countries { get; set; }

        public FiltersOperation States { get; set; }

        public FiltersOperation BetTypes { get; set; }

        public FiltersOperation PossibleWins { get; set; }

        public FiltersOperation BetAmounts { get; set; }
        public FiltersOperation OriginalBetAmounts { get; set; }

        public FiltersOperation Coefficients { get; set; }

        public FiltersOperation WinAmounts { get; set; }
        public FiltersOperation OriginalWinAmounts { get; set; }

        public FiltersOperation BonusIds { get; set; }

        public FiltersOperation BetDates { get; set; }
        public FiltersOperation WinDates { get; set; }
        public FiltersOperation LastUpdateTimes { get; set; }

        public FiltersOperation GGRs { get; set; }

        public FiltersOperation Rakes { get; set; }
        public FiltersOperation BonusAmounts { get; set; }
        public FiltersOperation OriginalBonusAmounts { get; set; }
        public FiltersOperation BonusWinAmounts { get; set; }
        public FiltersOperation OriginalBonusWinAmounts { get; set; }

        public FiltersOperation Balances { get; set; }

        public FiltersOperation TotalBetsCounts { get; set; }

        public FiltersOperation TotalBetsAmounts { get; set; }

        public FiltersOperation TotalWinsAmounts { get; set; }

        public FiltersOperation MaxBetAmounts { get; set; }

        public FiltersOperation TotalDepositsCounts { get; set; }

        public FiltersOperation TotalDepositsAmounts { get; set; }

        public FiltersOperation TotalWithdrawalsCounts { get; set; }

        public FiltersOperation TotalWithdrawalsAmounts { get; set; }

        public override void CreateQuery(ref IQueryable<fnInternetBet> objects, bool order, bool orderByDate = false)
        {
            if (PartnerId.HasValue)
                objects = objects.Where(x => x.PartnerId == PartnerId.Value);
            if (ClientId.HasValue)
                objects = objects.Where(x => x.ClientId == ClientId.Value);

            var fDate = FromDate.Year * 1000000 + FromDate.Month * 10000 + FromDate.Day * 100 + FromDate.Hour;
            objects = objects.Where(x => x.Date >= fDate);
            if (ToDate != null)
            {
                objects = objects.Where(x => x.BetDate < ToDate);
            }
            if (AgentId.HasValue)
            {
                var agentValue = "/" + AgentId.Value + "/";
                objects = objects.Where(x => x.UserPath.Contains(agentValue));
            }
            if (AccountId != null)
                objects = objects.Where(x => x.AccountId == AccountId.Value);

            FilterByValue(ref objects, BetDocumentIds, "BetDocumentId");
            FilterByValue(ref objects, ClientIds, "ClientId");
            FilterByValue(ref objects, UserIds, "UserId");
            FilterByValue(ref objects, Names, "ClientFirstName", "ClientLastName");
            FilterByValue(ref objects, ClientFirstNames, "ClientFirstName");
            FilterByValue(ref objects, ClientLastNames, "ClientLastName");
            FilterByValue(ref objects, ClientUserNames, "ClientUserName");
            FilterByValue(ref objects, Categories, "ClientCategoryId");
            FilterByValue(ref objects, ProductIds, "ProductId");
            FilterByValue(ref objects, ProductNames, "ProductName");
            FilterByValue(ref objects, ProviderNames, "ProviderName");
            FilterByValue(ref objects, SubproviderIds, "SubproviderId");
            FilterByValue(ref objects, SubproviderNames, "SubproviderName");
            FilterByValue(ref objects, CurrencyIds, "CurrencyId");
            FilterByValue(ref objects, RoundIds, "RoundId");
            FilterByValue(ref objects, DeviceTypes, "DeviceTypeId");
            FilterByValue(ref objects, ClientIps, "ClientIp");
            FilterByValue(ref objects, Countries, "Country");
            FilterByValue(ref objects, States, "State");
            FilterByValue(ref objects, BetTypes, "BetTypeId");
            FilterByValue(ref objects, PossibleWins, "PossibleWin");
            FilterByValue(ref objects, BetAmounts, "BetAmount");
            FilterByValue(ref objects, OriginalBetAmounts, "BetAmount");
            FilterByValue(ref objects, Coefficients, "Coefficient");
            FilterByValue(ref objects, WinAmounts, "WinAmount");
            FilterByValue(ref objects, OriginalWinAmounts, "WinAmount");
            FilterByValue(ref objects, BonusIds, "BonusId");
            FilterByValue(ref objects, LastUpdateTimes, "LastUpdateTime");
            FilterByValue(ref objects, Rakes, "Rake");
            FilterByValue(ref objects, BonusAmounts, "BonusAmount");
            FilterByValue(ref objects, OriginalBonusAmounts, "BonusAmount");
            FilterByValue(ref objects, BonusWinAmounts, "BonusWinAmount");
            FilterByValue(ref objects, OriginalBonusWinAmounts, "BonusWinAmount");

            if (BetDates != null && BetDates.OperationTypeList != null && BetDates.OperationTypeList.Any())
            {
                foreach (var item in BetDates.OperationTypeList)
                {
                    item.IntValue = (long)item.DateTimeValue.Year * 1000000 + (long)item.DateTimeValue.Month * 10000 + 
                        (long)item.DateTimeValue.Day * 100 + (long)item.DateTimeValue.Hour;
                }
            }
            FilterByValue(ref objects, BetDates, "Date");
            FilterByValue(ref objects, WinDates, "WinDate");

            base.FilteredObjects(ref objects, order, orderByDate, "BetDocumentId");
        }

        private IQueryable<InternetBetByClient> CreateQueryForResultObjects(IQueryable<InternetBetByClient> objects, 
            Func<IQueryable<InternetBetByClient>, IOrderedQueryable<InternetBetByClient>> orderBy = null)
        {
            FilterByValue(ref objects, Balances, "Balance");
            FilterByValue(ref objects, TotalBetsCounts, "TotalBetsCount");
            FilterByValue(ref objects, TotalBetsAmounts, "TotalBetsAmount");
            FilterByValue(ref objects, TotalWinsAmounts, "TotalWinsAmount");
            FilterByValue(ref objects, MaxBetAmounts, "MaxBetAmount");
            FilterByValue(ref objects, TotalDepositsCounts, "TotalDepositsCount");
            FilterByValue(ref objects, TotalDepositsAmounts, "TotalDepositsAmount");
            FilterByValue(ref objects, TotalWithdrawalsCounts, "TotalWithdrawalsCount");
            FilterByValue(ref objects, TotalWithdrawalsAmounts, "TotalWithdrawalsAmount");

            #region GGRs

            if (GGRs != null && GGRs.OperationTypeList != null && GGRs.OperationTypeList.Any())
            {
                if (GGRs.IsAnd)
                {
                    foreach (var item in GGRs.OperationTypeList)
                    {
                        switch (item.OperationTypeId)
                        {
                            case (int)FilterOperations.IsEqualTo:
                                objects = objects.Where(x => x.TotalBetsAmount - x.TotalWinsAmount == item.DecimalValue);
                                break;
                            case (int)FilterOperations.IsGreaterThenOrEqualTo:
                                objects = objects.Where(x => x.TotalBetsAmount - x.TotalWinsAmount >= item.DecimalValue);
                                break;
                            case (int)FilterOperations.IsGreaterThen:
                                objects = objects.Where(x => x.TotalBetsAmount - x.TotalWinsAmount > item.DecimalValue);
                                break;
                            case (int)FilterOperations.IsLessThenOrEqualTo:
                                objects = objects.Where(x => x.TotalBetsAmount - x.TotalWinsAmount <= item.DecimalValue);
                                break;
                            case (int)FilterOperations.IsLessThen:
                                objects = objects.Where(x => x.TotalBetsAmount - x.TotalWinsAmount < item.DecimalValue);
                                break;
                            case (int)FilterOperations.IsNotEqualTo:
                                objects = objects.Where(x => x.TotalBetsAmount - x.TotalWinsAmount != item.DecimalValue);
                                break;
                        }
                    }
                }
                else
                {
                    var predicate = PredicateBuilder.New<InternetBetByClient>(false);
                    foreach (var item in GGRs.OperationTypeList)
                    {
                        switch (item.OperationTypeId)
                        {
                            case (int)FilterOperations.IsEqualTo:
                                predicate = predicate.Or(x => x.TotalBetsAmount - x.TotalWinsAmount == item.DecimalValue);
                                break;
                            case (int)FilterOperations.IsGreaterThenOrEqualTo:
                                predicate = predicate.Or(x => x.TotalBetsAmount - x.TotalWinsAmount >= item.DecimalValue);
                                break;
                            case (int)FilterOperations.IsGreaterThen:
                                predicate = predicate.Or(x => x.TotalBetsAmount - x.TotalWinsAmount > item.DecimalValue);
                                break;
                            case (int)FilterOperations.IsLessThenOrEqualTo:
                                predicate = predicate.Or(x => x.TotalBetsAmount - x.TotalWinsAmount <= item.DecimalValue);
                                break;
                            case (int)FilterOperations.IsLessThen:
                                predicate = predicate.Or(x => x.TotalBetsAmount - x.TotalWinsAmount < item.DecimalValue);
                                break;
                            case (int)FilterOperations.IsNotEqualTo:
                                predicate = predicate.Or(x => x.TotalBetsAmount - x.TotalWinsAmount != item.DecimalValue);
                                break;
                        }
                    }
                    objects = objects.AsExpandable().Where(predicate);
                }
            }

            #endregion

            if (TakeCount != 0)
            {
                objects = objects.OrderBy(x => x.ClientId);
                objects = objects.Skip(SkipCount * TakeCount).Take(TakeCount);
            }

            if (orderBy != null)
            {
                objects = orderBy(objects);
            }

            return objects;
        }
        
        public IQueryable<InternetBetByClient> FilterResultObjects(IQueryable<InternetBetByClient> objects, Func<IQueryable<InternetBetByClient>, IOrderedQueryable<InternetBetByClient>> orderBy = null)
        {
            objects = CreateQueryForResultObjects(objects, orderBy);
            return objects;
        }

        public long SelectedObjectsCount(IQueryable<fnInternetBet> internetBets)
        {
            CreateQuery(ref internetBets, false);
            return internetBets.Count();
        }

        public FilterInternetBet Copy()
        {
            return new FilterInternetBet
            {
                SkipCount = base.SkipCount,
                TakeCount = base.TakeCount,
                CheckPermissionResuts = base.CheckPermissionResuts,

                PartnerId = PartnerId,
                FromDate = FromDate,
                ToDate = ToDate,
                //Ids = Ids == null ? null : Ids.Select(x => x.Copy()).ToList(),
                //ClientIds = ClientIds == null ? null : ClientIds.Select(x => x.Copy()).ToList(),
                //Names = Names == null ? null : Names.Select(x => x.Copy()).ToList(),
                //UserNames = UserNames == null ? null : UserNames.Select(x => x.Copy()).ToList(),
                //Categories = Categories == null ? null : Categories.Select(x => x.Copy()).ToList(),
                //ProductIds = ProductIds == null ? null : ProductIds.Select(x => x.Copy()).ToList(),
                //ProductNames = ProductNames == null ? null : ProductNames.Select(x => x.Copy()).ToList(),
                //ProviderNames = ProviderNames == null ? null : ProviderNames.Select(x => x.Copy()).ToList(),
                //Currencies = Currencies == null ? null : Currencies.Select(x => x.Copy()).ToList(),
                //RoundIds = RoundIds == null ? null : RoundIds.Select(x => x.Copy()).ToList(),
                //DeviceTypes = DeviceTypes == null ? null : DeviceTypes.Select(x => x.Copy()).ToList(),
                //ClientIps = ClientIps == null ? null : ClientIps.Select(x => x.Copy()).ToList(),
                //Countries = Countries == null ? null : Countries.Select(x => x.Copy()).ToList(),
                //States = States == null ? null : States.Select(x => x.Copy()).ToList(),
                //BetTypes = BetTypes == null ? null : BetTypes.Select(x => x.Copy()).ToList(),
                //PossibleWins = PossibleWins == null ? null : PossibleWins.Select(x => x.Copy()).ToList(),
                //BetAmounts = BetAmounts == null ? null : BetAmounts.Select(x => x.Copy()).ToList(),
                //WinAmounts = WinAmounts == null ? null : WinAmounts.Select(x => x.Copy()).ToList(),
                //BetDates = BetDates == null ? null : BetDates.Select(x => x.Copy()).ToList(),
                //GGRs = GGRs == null ? null : GGRs.Select(x => x.Copy()).ToList(),
                //Balances = Balances == null ? null : Balances.Select(x => x.Copy()).ToList(),
                //TotalBetsCounts = TotalBetsCounts == null ? null : TotalBetsCounts.Select(x => x.Copy()).ToList(),
                //TotalBetsAmounts = TotalBetsAmounts == null ? null : TotalBetsAmounts.Select(x => x.Copy()).ToList(),
                //TotalWinsAmounts = TotalWinsAmounts == null ? null : TotalWinsAmounts.Select(x => x.Copy()).ToList(),
                //MaxBetAmounts = MaxBetAmounts == null ? null : MaxBetAmounts.Select(x => x.Copy()).ToList(),
                //TotalDepositsCounts = TotalDepositsCounts == null ? null : TotalDepositsCounts.Select(x => x.Copy()).ToList(),
                //TotalDepositsAmounts = TotalDepositsAmounts == null ? null : TotalDepositsAmounts.Select(x => x.Copy()).ToList(),
                //TotalWithdrawalsCounts = TotalWithdrawalsCounts == null ? null : TotalWithdrawalsCounts.Select(x => x.Copy()).ToList(),
                //TotalWithdrawalsAmounts = TotalWithdrawalsAmounts == null ? null : TotalWithdrawalsAmounts.Select(x => x.Copy()).ToList()
            };
        }
    }
}
