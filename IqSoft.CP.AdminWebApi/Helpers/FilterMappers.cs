using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Filters;
using IqSoft.CP.DAL.Filters.Reporting;
using IqSoft.CP.DAL.Filters.PaymentRequests;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.AdminWebApi.Filters;
using IqSoft.CP.AdminWebApi.Filters.Bets;
using IqSoft.CP.AdminWebApi.Filters.PaymentRequests;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.AdminWebApi.Filters.Reporting;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.AdminWebApi.Models.ReportModels;
using IqSoft.CP.DAL.Filters.Clients;
using IqSoft.CP.AdminWebApi.Models.ContentModels;
using IqSoft.CP.AdminWebApi.Models.LanguageModels;
using IqSoft.CP.AdminWebApi.Models.ReportModels.BetShop;
using IqSoft.CP.Common.Models.Filters;
using IqSoft.CP.Common.Models.AdminModels;
using IqSoft.CP.AdminWebApi.Filters.Clients;
using IqSoft.CP.DAL.Filters.Affiliate;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DataWarehouse.Filters;
using IqSoft.CP.AdminWebApi.Filters.Affiliate;
using IqSoft.CP.DataWarehouse;
using IqSoft.CP.DAL.Filters.Messages;
using IqSoft.CP.AdminWebApi.Filters.Messages;

namespace IqSoft.CP.AdminWebApi.Helpers
{
    public static class FilterMappers
    {
        #region Reporting

        #region Internet Reports

        public static FilterfnDocument MapToFilterfnDocument(this ApiFilterfnDocument filter, double timeZone)
        {
            return new FilterfnDocument
            {
                ClientId = filter.ClientId,
                FromDate = filter.FromDate.GetUTCDateFromGMT(timeZone),
                ToDate = filter.ToDate.GetUTCDateFromGMT(timeZone),
                AccountId = filter.AccountId,
                Ids = filter.Ids == null ? new FiltersOperation() : filter.Ids.MapToFiltersOperation(timeZone),
                ExternalTransactionIds = filter.ExternalTransactionIds == null ? new FiltersOperation() : filter.ExternalTransactionIds.MapToFiltersOperation(timeZone),
                Amounts = filter.Amounts == null ? new FiltersOperation() : filter.Amounts.MapToFiltersOperation(timeZone),
                States = filter.States == null ? new FiltersOperation() : filter.States.MapToFiltersOperation(timeZone),
                OperationTypeIds = filter.OperationTypeIds == null ? new FiltersOperation() : filter.OperationTypeIds.MapToFiltersOperation(timeZone),
                PaymentRequestIds = filter.PaymentRequestIds == null ? new FiltersOperation() : filter.PaymentRequestIds.MapToFiltersOperation(timeZone),
                PaymentSystemIds = filter.PaymentSystemIds == null ? new FiltersOperation() : filter.PaymentSystemIds.MapToFiltersOperation(timeZone),
                PaymentSystemNames = filter.PaymentSystemNames == null ? new FiltersOperation() : filter.PaymentSystemNames.MapToFiltersOperation(timeZone),
                RoundIds = filter.RoundIds == null ? new FiltersOperation() : filter.RoundIds.MapToFiltersOperation(timeZone),
                ProductIds = filter.ProductIds == null ? new FiltersOperation() : filter.ProductIds.MapToFiltersOperation(timeZone),
                ProductNames = filter.ProductNames == null ? new FiltersOperation() : filter.ProductNames.MapToFiltersOperation(timeZone),
                GameProviderIds = filter.GameProviderIds == null ? new FiltersOperation() : filter.GameProviderIds.MapToFiltersOperation(timeZone),
                GameProviderNames = filter.GameProviderNames == null ? new FiltersOperation() : filter.GameProviderNames.MapToFiltersOperation(timeZone),
                LastUpdateTimes = filter.LastUpdateTimes == null ? new FiltersOperation() : filter.LastUpdateTimes.MapToFiltersOperation(timeZone),
                SkipCount = filter.SkipCount,
                TakeCount = filter.TakeCount,
                OrderBy = filter.OrderBy,
                FieldNameToOrderBy = filter.FieldNameToOrderBy,

            };
        }

        public static FilterInternetBet MapToFilterInternetBet(this ApiFilterInternetBet filter, double timeZone)
        {
            if (!string.IsNullOrEmpty(filter.FieldNameToOrderBy))
            {
                var orderBy = filter.FieldNameToOrderBy;
                switch (orderBy)
                {
                    case "OriginalBetAmount":
                        filter.FieldNameToOrderBy = "BetAmount";
                        break;
                    case "OriginalWinAmount":
                        filter.FieldNameToOrderBy = "WinAmount";
                        break;
                    case "OriginalBonusAmount":
                        filter.FieldNameToOrderBy = "BonusAmount";
                        break;
                    case "OriginalBonusWinAmount":
                        filter.FieldNameToOrderBy = "BonusWinAmount";
                        break;
                    default:
                        break;
                }
            }

            return new FilterInternetBet
            {
                PartnerId = filter.PartnerId,
                ClientId = filter.ClientId,
                AccountId = filter.AccountId,
                FromDate = filter.BetDateFrom.GetUTCDateFromGMT(timeZone),
                ToDate = filter.BetDateBefore.GetUTCDateFromGMT(timeZone),
                BetDocumentIds = filter.BetDocumentIds == null ? new FiltersOperation() : filter.BetDocumentIds.MapToFiltersOperation(timeZone),
                ClientIds = filter.ClientIds == null ? new FiltersOperation() : filter.ClientIds.MapToFiltersOperation(timeZone),
                UserIds = filter.UserIds == null ? new FiltersOperation() : filter.UserIds.MapToFiltersOperation(timeZone),
                Names = filter.Names == null ? new FiltersOperation() : filter.Names.MapToFiltersOperation(timeZone),
                ClientUserNames = filter.UserNames == null ? new FiltersOperation() : filter.UserNames.MapToFiltersOperation(timeZone),
                Categories = filter.Categories == null ? new FiltersOperation() : filter.Categories.MapToFiltersOperation(timeZone),
                ProductIds = filter.ProductIds == null ? new FiltersOperation() : filter.ProductIds.MapToFiltersOperation(timeZone),
                ProductNames = filter.ProductNames == null ? new FiltersOperation() : filter.ProductNames.MapToFiltersOperation(timeZone),
                ProviderNames = filter.ProviderNames == null ? new FiltersOperation() : filter.ProviderNames.MapToFiltersOperation(timeZone),
                SubproviderIds = filter.SubproviderIds == null ? new FiltersOperation() : filter.SubproviderIds.MapToFiltersOperation(timeZone),
                SubproviderNames = filter.SubproviderNames == null ? new FiltersOperation() : filter.SubproviderNames.MapToFiltersOperation(timeZone),
                CurrencyIds = filter.CurrencyIds == null ? new FiltersOperation() : filter.CurrencyIds.MapToFiltersOperation(timeZone),
                RoundIds = filter.RoundIds == null ? new FiltersOperation() : filter.RoundIds.MapToFiltersOperation(timeZone),
                DeviceTypes = filter.DeviceTypes == null ? new FiltersOperation() : filter.DeviceTypes.MapToFiltersOperation(timeZone),
                ClientIps = filter.ClientIps == null ? new FiltersOperation() : filter.ClientIps.MapToFiltersOperation(timeZone),
                Countries = filter.Countries == null ? new FiltersOperation() : filter.Countries.MapToFiltersOperation(timeZone),
                States = filter.States == null ? new FiltersOperation() : filter.States.MapToFiltersOperation(timeZone),
                BetTypes = filter.BetTypes == null ? new FiltersOperation() : filter.BetTypes.MapToFiltersOperation(timeZone),
                PossibleWins = filter.PossibleWins == null ? new FiltersOperation() : filter.PossibleWins.MapToFiltersOperation(timeZone),
                BetAmounts = filter.BetAmounts == null ? new FiltersOperation() : filter.BetAmounts.MapToFiltersOperation(timeZone),
                OriginalBetAmounts = filter.OriginalBetAmounts == null ? new FiltersOperation() : filter.OriginalBetAmounts.MapToFiltersOperation(timeZone),
                Coefficients = filter.Coefficients == null ? new FiltersOperation() : filter.Coefficients.MapToFiltersOperation(timeZone),
                WinAmounts = filter.WinAmounts == null ? new FiltersOperation() : filter.WinAmounts.MapToFiltersOperation(timeZone),
                OriginalWinAmounts = filter.OriginalWinAmounts == null ? new FiltersOperation() : filter.OriginalWinAmounts.MapToFiltersOperation(timeZone),
                BetDates = filter.BetDates == null ? new FiltersOperation() : filter.BetDates.MapToFiltersOperation(timeZone),
                WinDates = filter.CalculationDates == null ? new FiltersOperation() : filter.CalculationDates.MapToFiltersOperation(timeZone),
                LastUpdateTimes = filter.LastUpdateTimes == null ? new FiltersOperation() : filter.LastUpdateTimes.MapToFiltersOperation(timeZone),
                BonusIds = filter.BonusIds == null ? new FiltersOperation() : filter.BonusIds.MapToFiltersOperation(timeZone),
                GGRs = filter.GGRs == null ? new FiltersOperation() : filter.GGRs.MapToFiltersOperation(timeZone),
                Rakes = filter.Rakes == null ? new FiltersOperation() : filter.Rakes.MapToFiltersOperation(timeZone),
                BonusAmounts = filter.BonusAmounts == null ? new FiltersOperation() : filter.BonusAmounts.MapToFiltersOperation(timeZone),
                OriginalBonusAmounts = filter.OriginalBonusAmounts == null ? new FiltersOperation() : filter.OriginalBonusAmounts.MapToFiltersOperation(timeZone),
                BonusWinAmounts = filter.BonusWinAmounts == null ? new FiltersOperation() : filter.BonusWinAmounts.MapToFiltersOperation(timeZone),
                OriginalBonusWinAmounts = filter.OriginalBonusWinAmounts == null ? new FiltersOperation() : filter.OriginalBonusWinAmounts.MapToFiltersOperation(timeZone),
                Balances = filter.Balances == null ? new FiltersOperation() : filter.Balances.MapToFiltersOperation(timeZone),
                TotalBetsCounts = filter.TotalBetsCounts == null ? new FiltersOperation() : filter.TotalBetsCounts.MapToFiltersOperation(timeZone),
                TotalBetsAmounts = filter.TotalBetsAmounts == null ? new FiltersOperation() : filter.TotalBetsAmounts.MapToFiltersOperation(timeZone),
                TotalWinsAmounts = filter.TotalWinsAmounts == null ? new FiltersOperation() : filter.TotalWinsAmounts.MapToFiltersOperation(timeZone),
                MaxBetAmounts = filter.MaxBetAmounts == null ? new FiltersOperation() : filter.MaxBetAmounts.MapToFiltersOperation(timeZone),
                TotalDepositsCounts = filter.TotalDepositsCounts == null ? new FiltersOperation() : filter.TotalDepositsCounts.MapToFiltersOperation(timeZone),
                TotalDepositsAmounts = filter.TotalDepositsAmounts == null ? new FiltersOperation() : filter.TotalDepositsAmounts.MapToFiltersOperation(timeZone),
                TotalWithdrawalsCounts = filter.TotalWithdrawalsCounts == null ? new FiltersOperation() : filter.TotalWithdrawalsCounts.MapToFiltersOperation(timeZone),
                TotalWithdrawalsAmounts = filter.TotalWithdrawalsAmounts == null ? new FiltersOperation() : filter.TotalWithdrawalsAmounts.MapToFiltersOperation(timeZone),
                SkipCount = filter.SkipCount,
                TakeCount = Math.Min(filter.TakeCount, 5000),
                OrderBy = filter.OrderBy,
                FieldNameToOrderBy = filter.FieldNameToOrderBy,
                AgentId = filter.AgentId
            };
        }

        public static FilterInternetGame MapToFilterInternetGame(this ApiFilterInternetBet filter, double timeZone)
        {
            return new FilterInternetGame
            {
                PartnerId = filter.PartnerId,
                FromDate = filter.BetDateFrom.GetUTCDateFromGMT(timeZone),
                ToDate = filter.BetDateBefore.GetUTCDateFromGMT(timeZone),
                ProductIds = filter.ProductIds == null ? new FiltersOperation() : filter.ProductIds.MapToFiltersOperation(timeZone),
                ProductNames = filter.ProductNames == null ? new FiltersOperation() : filter.ProductNames.MapToFiltersOperation(timeZone),
                Currencies = filter.CurrencyIds == null ? new FiltersOperation() : filter.CurrencyIds.MapToFiltersOperation(timeZone),
                BetAmounts = filter.BetAmounts == null ? new FiltersOperation() : filter.BetAmounts.MapToFiltersOperation(timeZone),
                WinAmounts = filter.WinAmounts == null ? new FiltersOperation() : filter.WinAmounts.MapToFiltersOperation(timeZone),
                OriginalBetAmounts = filter.OriginalBetAmounts == null ? new FiltersOperation() : filter.OriginalBetAmounts.MapToFiltersOperation(timeZone),
                OriginalWinAmounts = filter.OriginalWinAmounts == null ? new FiltersOperation() : filter.OriginalWinAmounts.MapToFiltersOperation(timeZone),
                GGRs = filter.GGRs == null ? new FiltersOperation() : filter.GGRs.MapToFiltersOperation(timeZone),
                SkipCount = filter.SkipCount,
                TakeCount = Math.Min(filter.TakeCount, 5000),
                OrderBy = filter.OrderBy,
                FieldNameToOrderBy = filter.FieldNameToOrderBy
            };
        }

        #endregion

        #region BetShop Reports

        public static FilterReportByClientIdentity MapToFilterClientIdentity(this ApiFilterReportByClientIdentity filter, double timeZone)
        {
            return new FilterReportByClientIdentity
            {
                FromDate = filter.FromDate.GetUTCDateFromGMT(timeZone),
                ToDate = filter.ToDate.GetUTCDateFromGMT(timeZone),
                SkipCount = filter.SkipCount,
                TakeCount = filter.TakeCount,
                OrderBy = filter.OrderBy,
                FieldNameToOrderBy = filter.FieldNameToOrderBy,
                HasNote = filter.HasNote,
                Ids = filter.Ids == null ? new FiltersOperation() : filter.Ids.MapToFiltersOperation(timeZone),
                ClientIds = filter.ClientIds == null ? new FiltersOperation() : filter.ClientIds.MapToFiltersOperation(timeZone),
                UserNames = filter.UserNames == null ? new FiltersOperation() : filter.UserNames.MapToFiltersOperation(timeZone),
                PartnerIds = filter.PartnerIds == null ? new FiltersOperation() : filter.PartnerIds.MapToFiltersOperation(timeZone),
                UserIds = filter.UserIds == null ? new FiltersOperation() : filter.UserIds.MapToFiltersOperation(timeZone),
                DocumentTypeIds = filter.DocumentTypeIds == null ? new FiltersOperation() : filter.DocumentTypeIds.MapToFiltersOperation(timeZone),
                Statuses = filter.States == null ? new FiltersOperation() : filter.States.MapToFiltersOperation(timeZone),
                ExpirationTimes = filter.ExpirationTimes == null ? new FiltersOperation() : filter.ExpirationTimes.MapToFiltersOperation(timeZone),
                CreationTimes = filter.CreationTimes == null ? new FiltersOperation() : filter.CreationTimes.MapToFiltersOperation(timeZone),
                LastUpdateTimes = filter.LastUpdateTimes == null ? new FiltersOperation() : filter.LastUpdateTimes.MapToFiltersOperation(timeZone)
            };
        }

        public static FilterBetShopBet MapToFilterBetShopBet(this ApiFilterBetShopBet filter, double timeZone)
        {
            return new FilterBetShopBet
            {
                SkipCount = filter.SkipCount,
                TakeCount = Math.Min(filter.TakeCount, 5000),
                PartnerId = filter.PartnerId,
                FromDate = filter.BetDateFrom.GetUTCDateFromGMT(timeZone),
                ToDate = filter.BetDateBefore.GetUTCDateFromGMT(timeZone),
                BetShopGroupIds = !filter.BetShopGroupId.HasValue ? new FiltersOperation() :
                                  new ApiFiltersOperation
                                  {
                                      IsAnd = true,
                                      ApiOperationTypeList = new List<ApiFiltersOperationType>
                                      {new ApiFiltersOperationType{OperationTypeId =1, IntValue = (int)filter.BetShopGroupId } }
                                  }.MapToFiltersOperation(timeZone),
                Ids = filter.Ids == null ? new FiltersOperation() : filter.Ids.MapToFiltersOperation(timeZone),
                CashierIds = filter.CashierIds == null ? new FiltersOperation() : filter.CashierIds.MapToFiltersOperation(timeZone),
                CashDeskIds = filter.CashDeskIds == null ? new FiltersOperation() : filter.CashDeskIds.MapToFiltersOperation(timeZone),
                BetShopIds = filter.BetShopIds == null ? new FiltersOperation() : filter.BetShopIds.MapToFiltersOperation(timeZone),
                BetShopNames = filter.BetShopNames == null ? new FiltersOperation() : filter.BetShopNames.MapToFiltersOperation(timeZone),
                BetShopGroupNames = filter.BetShopGroupNames == null ? new FiltersOperation() : filter.BetShopGroupNames.MapToFiltersOperation(timeZone),
                ProductIds = filter.ProductIds == null ? new FiltersOperation() : filter.ProductIds.MapToFiltersOperation(timeZone),
                ProductNames = filter.ProductNames == null ? new FiltersOperation() : filter.ProductNames.MapToFiltersOperation(timeZone),
                ProviderIds = filter.ProviderIds == null ? new FiltersOperation() : filter.ProviderIds.MapToFiltersOperation(timeZone),
                ProviderNames = filter.ProviderNames == null ? new FiltersOperation() : filter.ProviderNames.MapToFiltersOperation(timeZone),
                Currencies = filter.Currencies == null ? new FiltersOperation() : filter.Currencies.MapToFiltersOperation(timeZone),
                RoundIds = filter.RoundIds == null ? new FiltersOperation() : filter.RoundIds.MapToFiltersOperation(timeZone),
                States = filter.States == null ? new FiltersOperation() : filter.States.MapToFiltersOperation(timeZone),
                BetTypes = filter.BetTypes == null ? new FiltersOperation() : filter.BetTypes.MapToFiltersOperation(timeZone),
                PossibleWins = filter.PossibleWins == null ? new FiltersOperation() : filter.PossibleWins.MapToFiltersOperation(timeZone),
                BetAmounts = filter.BetAmounts == null ? new FiltersOperation() : filter.BetAmounts.MapToFiltersOperation(timeZone),
                WinAmounts = filter.WinAmounts == null ? new FiltersOperation() : filter.WinAmounts.MapToFiltersOperation(timeZone),
                OriginalBetAmounts = filter.OriginalBetAmounts == null ? new FiltersOperation() : filter.OriginalBetAmounts.MapToFiltersOperation(timeZone),
                OriginalWinAmounts = filter.OriginalWinAmounts == null ? new FiltersOperation() : filter.OriginalWinAmounts.MapToFiltersOperation(timeZone),
                Barcodes = filter.Barcodes == null ? new FiltersOperation() : filter.Barcodes.MapToFiltersOperation(timeZone),
                TicketNumbers = filter.TicketNumbers == null ? new FiltersOperation() : filter.TicketNumbers.MapToFiltersOperation(timeZone),
                OrderBy = filter.OrderBy,
                FieldNameToOrderBy = filter.FieldNameToOrderBy
            };
        }

        public static FilterBetShopBet MapToFilterBetShopBet(this ApiFilterReportByBetShop filter, double timeZone)
        {
            return new FilterBetShopBet
            {
                PartnerId = filter.PartnerId,
                FromDate = filter.BetDateFrom.GetUTCDateFromGMT(timeZone),
                ToDate = filter.BetDateBefore.GetUTCDateFromGMT(timeZone),
                ProductIds = filter.ProductIds == null ? new FiltersOperation() : filter.ProductIds.MapToFiltersOperation(timeZone),
                BetShopIds = filter.BetShopIds == null ? new FiltersOperation() : filter.BetShopIds.MapToFiltersOperation(timeZone),
                BetShopGroupIds = filter.BetShopGroupIds == null ? new FiltersOperation() : filter.BetShopGroupIds.MapToFiltersOperation(timeZone),
                BetShopNames = filter.BetShopNames == null ? new FiltersOperation() : filter.BetShopNames.MapToFiltersOperation(timeZone),
                Currencies = filter.Currencies == null ? new FiltersOperation() : filter.Currencies.MapToFiltersOperation(timeZone)
            };
        }

        public static FilterReportByBetShopPayment MapToFilterReportByBetShopPayment(this ApiFilterReportByBetShopPayment filter, double timeZone)
        {
            return new FilterReportByBetShopPayment
            {
                SkipCount = filter.SkipCount,
                TakeCount = filter.TakeCount,
                FromDate = filter.FromDate.GetUTCDateFromGMT(timeZone),
                ToDate = filter.ToDate.GetUTCDateFromGMT(timeZone),
                BetShopIds = filter.BetShopIds == null ? new FiltersOperation() : filter.BetShopIds.MapToFiltersOperation(timeZone),
                GroupIds = filter.GroupIds == null ? new FiltersOperation() : filter.GroupIds.MapToFiltersOperation(timeZone),
                BetShopNames = filter.BetShopNames == null ? new FiltersOperation() : filter.BetShopNames.MapToFiltersOperation(timeZone),
                PendingDepositCounts = filter.TotalPendingDepositsCounts == null ? new FiltersOperation() : filter.TotalPendingDepositsCounts.MapToFiltersOperation(timeZone),
                PendingDepositAmounts = filter.TotalPendingDepositsAmounts == null ? new FiltersOperation() : filter.TotalPendingDepositsAmounts.MapToFiltersOperation(timeZone),
                PayedDepositCounts = filter.TotalPayedDepositsCounts == null ? new FiltersOperation() : filter.TotalPayedDepositsCounts.MapToFiltersOperation(timeZone),
                PayedDepositAmounts = filter.TotalPayedDepositsAmounts == null ? new FiltersOperation() : filter.TotalPayedDepositsAmounts.MapToFiltersOperation(timeZone),
                CanceledDepositCounts = filter.TotalCanceledDepositsCounts == null ? new FiltersOperation() : filter.TotalCanceledDepositsCounts.MapToFiltersOperation(timeZone),
                CanceledDepositAmounts = filter.TotalCanceledDepositsAmounts == null ? new FiltersOperation() : filter.TotalCanceledDepositsAmounts.MapToFiltersOperation(timeZone),
                PendingWithdrawalCounts = filter.TotalPendingWithdrawalsCounts == null ? new FiltersOperation() : filter.TotalPendingWithdrawalsCounts.MapToFiltersOperation(timeZone),
                PendingWithdrawalAmounts = filter.TotalPendingWithdrawalsAmounts == null ? new FiltersOperation() : filter.TotalPendingWithdrawalsAmounts.MapToFiltersOperation(timeZone),
                PayedWithdrawalCounts = filter.TotalPayedWithdrawalsCounts == null ? new FiltersOperation() : filter.TotalPayedWithdrawalsCounts.MapToFiltersOperation(timeZone),
                PayedWithdrawalAmounts = filter.TotalPayedWithdrawalsAmounts == null ? new FiltersOperation() : filter.TotalPayedWithdrawalsAmounts.MapToFiltersOperation(timeZone),
                CanceledWithdrawalCounts = filter.TotalCanceledWithdrawalsCounts == null ? new FiltersOperation() : filter.TotalCanceledWithdrawalsCounts.MapToFiltersOperation(timeZone),
                CanceledWithdrawalAmounts = filter.TotalCanceledWithdrawalsAmounts == null ? new FiltersOperation() : filter.TotalCanceledWithdrawalsAmounts.MapToFiltersOperation(timeZone)
            };
        }

        public static FilterReportByBetShopLimitChanges MapToFilterReportByBetShopLimitChanges(this ApiFilterBetShopLimitChanges filter, double timeZone)
        {
            return new FilterReportByBetShopLimitChanges
            {
                SkipCount = filter.SkipCount,
                TakeCount = filter.TakeCount,
                OrderBy = filter.OrderBy,
                FromDate = filter.FromDate.GetUTCDateFromGMT(timeZone),
                ToDate = filter.ToDate.GetUTCDateFromGMT(timeZone),
                BetShopIds = filter.BetShopIds == null ? new FiltersOperation() : filter.BetShopIds.MapToFiltersOperation(timeZone),
                UserIds = filter.UserIds == null ? new FiltersOperation() : filter.UserIds.MapToFiltersOperation(timeZone)
            };
        }

        public static ApiBetShopLimitChanges MapToApiBetShopLimitChanges(this ObjectDataChangeHistory input, double timeZone)
        {
            return new ApiBetShopLimitChanges
            {
                Id = input.Id,
                UserId = input.UserId,
                BetShopId = input.ObjectId,
                LimitValue = input.NumericValue,
                CreationTime = input.CreationTime.GetGMTDateFromUTC(timeZone)
            };
        }

        public static FilterReportByBonus MapToFilterReportByBonus(this ApiFilterReportByBonus filter, double timeZone)
        {
            if (!string.IsNullOrEmpty(filter.FieldNameToOrderBy))
            {
                var orderBy = filter.FieldNameToOrderBy;
                switch (orderBy)
                {
                    case "BonusType":
                        filter.FieldNameToOrderBy = "Type";
                        break;
                    case "BonusName":
                        filter.FieldNameToOrderBy = "Name";
                        break;
                    case "ClientBonusStatus":
                        filter.FieldNameToOrderBy = "Status";
                        break;
                    default:
                        break;
                }
            }

            return new FilterReportByBonus
            {
                FromDate = filter.FromDate.GetUTCDateFromGMT(timeZone),
                ToDate = filter.ToDate.GetUTCDateFromGMT(timeZone),
                SkipCount = filter.SkipCount,
                TakeCount = filter.TakeCount,
                OrderBy = filter.OrderBy,
                FieldNameToOrderBy = filter.FieldNameToOrderBy,
                Ids = filter.Ids == null ? new FiltersOperation() : filter.Ids.MapToFiltersOperation(timeZone),
                PartnerIds = filter.PartnerIds == null ? new FiltersOperation() : filter.PartnerIds.MapToFiltersOperation(timeZone),
                BonusIds = filter.BonusIds == null ? new FiltersOperation() : filter.BonusIds.MapToFiltersOperation(timeZone),
                BonusNames = filter.BonusNames == null ? new FiltersOperation() : filter.BonusNames.MapToFiltersOperation(timeZone),
                BonusTypes = filter.BonusTypes == null ? new FiltersOperation() : filter.BonusTypes.MapToFiltersOperation(timeZone),
                BonusStatuses = filter.BonusStatuses == null ? new FiltersOperation() : filter.BonusStatuses.MapToFiltersOperation(timeZone),
                ClientIds = filter.ClientIds == null ? new FiltersOperation() : filter.ClientIds.MapToFiltersOperation(timeZone),
                UserNames = filter.UserNames == null ? new FiltersOperation() : filter.UserNames.MapToFiltersOperation(timeZone),
                FirstNames = filter.FirstNames == null ? new FiltersOperation() : filter.FirstNames.MapToFiltersOperation(timeZone),
                LastNames = filter.LastNames == null ? new FiltersOperation() : filter.LastNames.MapToFiltersOperation(timeZone),
                CurrencyIds = filter.CurrencyIds == null ? new FiltersOperation() : filter.CurrencyIds.MapToFiltersOperation(timeZone),
                Emails = filter.Emails == null ? new FiltersOperation() : filter.Emails.MapToFiltersOperation(timeZone),
                MobileNumbers = filter.MobileNumbers == null ? new FiltersOperation() : filter.MobileNumbers.MapToFiltersOperation(timeZone),
                CategoryIds = filter.CategoryIds == null ? new FiltersOperation() : filter.CategoryIds.MapToFiltersOperation(timeZone),
                BonusPrizes = filter.BonusPrizes == null ? new FiltersOperation() : filter.BonusPrizes.MapToFiltersOperation(timeZone),
                SpinsCounts = filter.SpinsCounts == null ? new FiltersOperation() : filter.SpinsCounts.MapToFiltersOperation(timeZone),
                TurnoverAmountLefts = filter.TurnoverAmountLefts == null ? new FiltersOperation() : filter.TurnoverAmountLefts.MapToFiltersOperation(timeZone),
                RemainingCredits = filter.RemainingCredits == null ? new FiltersOperation() : filter.RemainingCredits.MapToFiltersOperation(timeZone),
                WageringTargets = filter.WageringTargets == null ? new FiltersOperation() : filter.WageringTargets.MapToFiltersOperation(timeZone),
                FinalAmounts = filter.FinalAmounts == null ? new FiltersOperation() : filter.FinalAmounts.MapToFiltersOperation(timeZone),
                ClientBonusStatuses = filter.Statuses == null ? new FiltersOperation() : filter.Statuses.MapToFiltersOperation(timeZone),
                AwardingTimes = filter.AwardingTimes == null ? new FiltersOperation() : filter.AwardingTimes.MapToFiltersOperation(timeZone),
                CalculationTimes = filter.CalculationTimes == null ? new FiltersOperation() : filter.CalculationTimes.MapToFiltersOperation(timeZone),
                CreationTimes = filter.CreationTimes == null ? new FiltersOperation() : filter.CreationTimes.MapToFiltersOperation(timeZone),
                ValidUntils = filter.ValidUntils == null ? new FiltersOperation() : filter.ValidUntils.MapToFiltersOperation(timeZone)
            };
        }

        public static FilterReportByfnClientSession MapToFilterReportByfnClientSession(this ApiFilterReportByClientSession filter, double timeZone)
        {
            return new FilterReportByfnClientSession
            {
                FromDate = filter.FromDate.GetUTCDateFromGMT(timeZone),
                ToDate = filter.ToDate.GetUTCDateFromGMT(timeZone),
                SkipCount = filter.SkipCount,
                TakeCount = filter.TakeCount,
                OrderBy = filter.OrderBy,
                FieldNameToOrderBy = filter.FieldNameToOrderBy,
                PartnerId = filter.PartnerId,
                Ids = filter.Ids == null ? new FiltersOperation() : filter.Ids.MapToFiltersOperation(timeZone),
                PartnerIds = filter.PartnerIds == null ? new FiltersOperation() : filter.PartnerIds.MapToFiltersOperation(timeZone),
                ClientIds = filter.ClientIds == null ? new FiltersOperation() : filter.ClientIds.MapToFiltersOperation(timeZone),
                FirstNames = filter.FirstNames == null ? new FiltersOperation() : filter.FirstNames.MapToFiltersOperation(timeZone),
                LastNames = filter.LastNames == null ? new FiltersOperation() : filter.LastNames.MapToFiltersOperation(timeZone),
                UserNames = filter.UserNames == null ? new FiltersOperation() : filter.UserNames.MapToFiltersOperation(timeZone),
                LanguageIds = filter.LanguageIds == null ? new FiltersOperation() : filter.LanguageIds.MapToFiltersOperation(timeZone),
                Ips = filter.Ips == null ? new FiltersOperation() : filter.Ips.MapToFiltersOperation(timeZone),
                Countries = filter.Countries == null ? new FiltersOperation() : filter.Countries.MapToFiltersOperation(timeZone),
                States = filter.States == null ? new FiltersOperation() : filter.States.MapToFiltersOperation(timeZone),
                LogoutTypes = filter.LogoutTypes == null ? new FiltersOperation() : filter.LogoutTypes.MapToFiltersOperation(timeZone),
                ProductIds = filter.ProductIds == null ? new FiltersOperation() : filter.ProductIds.MapToFiltersOperation(timeZone),
                StartTimes = filter.StartTimes == null ? new FiltersOperation() : filter.StartTimes.MapToFiltersOperation(timeZone),
                LastUpdateTimes = filter.LastUpdateTimes == null ? new FiltersOperation() : filter.LastUpdateTimes.MapToFiltersOperation(timeZone),
                EndTimes = filter.EndTimes == null ? new FiltersOperation() : filter.EndTimes.MapToFiltersOperation(timeZone)
            };
        }

        public static FilterReportByClientSession MapToFilterReportByClientSession(this ApiFilterReportByClientSession filter, double timeZone)
        {
            return new FilterReportByClientSession
            {
                FromDate = filter.FromDate.GetUTCDateFromGMT(timeZone),
                ToDate = filter.ToDate.GetUTCDateFromGMT(timeZone),
                SkipCount = filter.SkipCount,
                TakeCount = filter.TakeCount,
                OrderBy = filter.OrderBy,
                FieldNameToOrderBy = filter.FieldNameToOrderBy,
                ClientId = filter.ClientId,
                ProductId = filter.ProductId,
                Ids = filter.Ids == null ? new FiltersOperation() : filter.Ids.MapToFiltersOperation(timeZone),
                LanguageIds = filter.LanguageIds == null ? new FiltersOperation() : filter.LanguageIds.MapToFiltersOperation(timeZone),
                Ips = filter.Ips == null ? new FiltersOperation() : filter.Ips.MapToFiltersOperation(timeZone),
                Countries = filter.Countries == null ? new FiltersOperation() : filter.Countries.MapToFiltersOperation(timeZone),
                States = filter.States == null ? new FiltersOperation() : filter.States.MapToFiltersOperation(timeZone),
                LogoutTypes = filter.LogoutTypes == null ? new FiltersOperation() : filter.LogoutTypes.MapToFiltersOperation(timeZone),
                ProductIds = filter.ProductIds == null ? new FiltersOperation() : filter.ProductIds.MapToFiltersOperation(timeZone),
                StartTimes = filter.StartTimes == null ? new FiltersOperation() : filter.StartTimes.MapToFiltersOperation(timeZone),
                LastUpdateTimes = filter.LastUpdateTimes == null ? new FiltersOperation() : filter.LastUpdateTimes.MapToFiltersOperation(timeZone),
                EndTimes = filter.EndTimes == null ? new FiltersOperation() : filter.EndTimes.MapToFiltersOperation(timeZone)
            };
        }

        public static FilterReportByUserSession MapToFilterReportByUserSession(this ApiFilterReportByUserSession filter, double timeZone)
        {
            return new FilterReportByUserSession
            {
                FromDate = filter.FromDate.GetUTCDateFromGMT(timeZone),
                ToDate = filter.ToDate.GetUTCDateFromGMT(timeZone),
                SkipCount = filter.SkipCount,
                TakeCount = filter.TakeCount,
                OrderBy = filter.OrderBy,
                FieldNameToOrderBy = filter.FieldNameToOrderBy,
                PartnerId = filter.PartnerId,
                UserId = filter.UserId,
                Type = filter.Type,
                Ids = filter.Ids == null ? new FiltersOperation() : filter.Ids.MapToFiltersOperation(timeZone),
                PartnerIds = filter.PartnerIds == null ? new FiltersOperation() : filter.PartnerIds.MapToFiltersOperation(timeZone),
                UserIds = filter.UserIds == null ? new FiltersOperation() : filter.UserIds.MapToFiltersOperation(timeZone),
                FirstNames = filter.FirstNames == null ? new FiltersOperation() : filter.FirstNames.MapToFiltersOperation(timeZone),
                LastNames = filter.LastNames == null ? new FiltersOperation() : filter.LastNames.MapToFiltersOperation(timeZone),
                UserNames = filter.UserNames == null ? new FiltersOperation() : filter.UserNames.MapToFiltersOperation(timeZone),
                Emails = filter.Emails == null ? new FiltersOperation() : filter.Emails.MapToFiltersOperation(timeZone),
                LanguageIds = filter.LanguageIds == null ? new FiltersOperation() : filter.LanguageIds.MapToFiltersOperation(timeZone),
                Ips = filter.Ips == null ? new FiltersOperation() : filter.Ips.MapToFiltersOperation(timeZone),
                Types = filter.Types == null ? new FiltersOperation() : filter.Types.MapToFiltersOperation(timeZone),
                States = filter.States == null ? new FiltersOperation() : filter.States.MapToFiltersOperation(timeZone),
                LogoutTypes = filter.LogoutTypes == null ? new FiltersOperation() : filter.LogoutTypes.MapToFiltersOperation(timeZone),
                EndTimes = filter.EndTimes == null ? new FiltersOperation() : filter.EndTimes.MapToFiltersOperation(timeZone)
            };
        }
        public static FilterClientExclusion MapToFilterClientExclusion(this ApiFilterReportByClientExclusion filter)
        {
            return new FilterClientExclusion
            {
                SkipCount = filter.SkipCount,
                TakeCount = filter.TakeCount,
                OrderBy = filter.OrderBy,
                FieldNameToOrderBy = filter.FieldNameToOrderBy,
                PartnerIds = filter.PartnerIds == null ? new FiltersOperation() : filter.PartnerIds.MapToFiltersOperation(filter.TimeZone),
                ClientIds = filter.ClientIds == null ? new FiltersOperation() : filter.ClientIds.MapToFiltersOperation(filter.TimeZone),
                Usernames = filter.Usernames == null ? new FiltersOperation() : filter.Usernames.MapToFiltersOperation(filter.TimeZone),
                DepositLimitDailys = filter.DepositLimitDailys == null ? new FiltersOperation() : filter.DepositLimitDailys.MapToFiltersOperation(filter.TimeZone),
                DepositLimitWeeklys = filter.DepositLimitWeeklys == null ? new FiltersOperation() : filter.DepositLimitWeeklys.MapToFiltersOperation(filter.TimeZone),
                DepositLimitMonthlys = filter.DepositLimitMonthlys == null ? new FiltersOperation() : filter.DepositLimitMonthlys.MapToFiltersOperation(filter.TimeZone),
                TotalBetAmountLimitDailys = filter.TotalBetAmountLimitDailys == null ? new FiltersOperation() : filter.TotalBetAmountLimitDailys.MapToFiltersOperation(filter.TimeZone),
                TotalBetAmountLimitWeeklys = filter.TotalBetAmountLimitWeeklys == null ? new FiltersOperation() : filter.TotalBetAmountLimitWeeklys.MapToFiltersOperation(filter.TimeZone),
                TotalBetAmountLimitMonthlys = filter.TotalBetAmountLimitMonthlys == null ? new FiltersOperation() : filter.TotalBetAmountLimitMonthlys.MapToFiltersOperation(filter.TimeZone),
                TotalLossLimitDailys = filter.TotalLossLimitDailys == null ? new FiltersOperation() : filter.TotalLossLimitDailys.MapToFiltersOperation(filter.TimeZone),
                TotalLossLimitWeeklys = filter.TotalLossLimitWeeklys == null ? new FiltersOperation() : filter.TotalLossLimitWeeklys.MapToFiltersOperation(filter.TimeZone),
                TotalLossLimitMonthlys = filter.TotalLossLimitMonthlys == null ? new FiltersOperation() : filter.TotalLossLimitMonthlys.MapToFiltersOperation(filter.TimeZone),
                SystemDepositLimitDailys = filter.SystemDepositLimitDailys == null ? new FiltersOperation() : filter.SystemDepositLimitDailys.MapToFiltersOperation(filter.TimeZone),
                SystemDepositLimitWeeklys = filter.SystemDepositLimitWeeklys == null ? new FiltersOperation() : filter.SystemDepositLimitWeeklys.MapToFiltersOperation(filter.TimeZone),
                SystemDepositLimitMonthlys = filter.SystemDepositLimitMonthlys == null ? new FiltersOperation() : filter.SystemDepositLimitMonthlys.MapToFiltersOperation(filter.TimeZone),
                SystemTotalBetAmountLimitDailys = filter.SystemTotalBetAmountLimitDailys == null ? new FiltersOperation() : filter.SystemTotalBetAmountLimitDailys.MapToFiltersOperation(filter.TimeZone),
                SystemTotalBetAmountLimitWeeklys = filter.SystemTotalBetAmountLimitWeeklys == null ? new FiltersOperation() : filter.SystemTotalBetAmountLimitWeeklys.MapToFiltersOperation(filter.TimeZone),
                SystemTotalBetAmountLimitMonthlys = filter.SystemTotalBetAmountLimitMonthlys == null ? new FiltersOperation() : filter.SystemTotalBetAmountLimitMonthlys.MapToFiltersOperation(filter.TimeZone),
                SystemTotalLossLimitDailys = filter.SystemTotalLossLimitDailys == null ? new FiltersOperation() : filter.SystemTotalLossLimitDailys.MapToFiltersOperation(filter.TimeZone),
                SystemTotalLossLimitWeeklys = filter.SystemTotalLossLimitWeeklys == null ? new FiltersOperation() : filter.SystemTotalLossLimitWeeklys.MapToFiltersOperation(filter.TimeZone),
                SystemTotalLossLimitMonthlys = filter.SystemTotalLossLimitMonthlys == null ? new FiltersOperation() : filter.SystemTotalLossLimitMonthlys.MapToFiltersOperation(filter.TimeZone),
                SessionLimits = filter.SessionLimits == null ? new FiltersOperation() : filter.SessionLimits.MapToFiltersOperation(filter.TimeZone),
                SystemSessionLimits = filter.SystemSessionLimits == null ? new FiltersOperation() : filter.SystemSessionLimits.MapToFiltersOperation(filter.TimeZone)
            };
        }

        public static FilterReportByObjectChangeHistory MapToFilterObjectChangeHistory(this ApiFilterReportByObjectChangeHistory filter, double timeZone)
        {
            return new FilterReportByObjectChangeHistory
            {
                FromDate = filter.FromDate.GetUTCDateFromGMT(timeZone),
                ToDate = filter.ToDate.GetUTCDateFromGMT(timeZone),
                SkipCount = filter.SkipCount,
                TakeCount = filter.TakeCount,
                OrderBy = filter.OrderBy,
                FieldNameToOrderBy = filter.FieldNameToOrderBy,
                PartnerId = filter.PartnerId,
                ObjectId = filter.ObjectId,
                ObjectTypeId = filter.ObjectTypeId,
                Ids = filter.Ids == null ? new FiltersOperation() : filter.Ids.MapToFiltersOperation(timeZone),
                ObjectIds = filter.ObjectIds == null ? new FiltersOperation() : filter.ObjectIds.MapToFiltersOperation(timeZone),
                UserIds = filter.UserIds == null ? new FiltersOperation() : filter.UserIds.MapToFiltersOperation(timeZone)
            };
        }
        public static ApiReportByObjectChangeHistory MapToApiReportByObjectChangeHistory(this spObjectChangeHistory input, double timeZone)
        {
            return new ApiReportByObjectChangeHistory
            {
                Id = input.Id,
                ObjectId = input.ObjectId,
                ObjectTypeId = input.ObjectTypeId,
                Object = input.Object,
                Comment = input.Comment,
                ChangeDate = input.ChangeDate.GetGMTDateFromUTC(timeZone),
                PartnerId = input.PartnerId,
                UserId = input.UserId,
                FirstName = input.FirstName,
                LastName = input.LastName
            };
        }

        #endregion

        #region BusinessIntelligence Reports

        public static FilterReportByProvider MapToFilterReportByProvider(this ApiFilterReportByProvider filter, double timeZone)
        {
            return new FilterReportByProvider
            {
                FromDate = filter.FromDate.GetUTCDateFromGMT(timeZone),
                ToDate = filter.ToDate.GetUTCDateFromGMT(timeZone),
                AgentId = filter.AgentId,
                ProviderNames = filter.ProviderNames == null ? new FiltersOperation() : filter.ProviderNames.MapToFiltersOperation(timeZone),
                Currencies = filter.Currencies == null ? new FiltersOperation() : filter.Currencies.MapToFiltersOperation(timeZone),
                TotalBetsCounts = filter.TotalBetsCounts == null ? new FiltersOperation() : filter.TotalBetsCounts.MapToFiltersOperation(timeZone),
                TotalBetsAmounts = filter.TotalBetsAmounts == null ? new FiltersOperation() : filter.TotalBetsAmounts.MapToFiltersOperation(timeZone),
                TotalWinsAmounts = filter.TotalWinsAmounts == null ? new FiltersOperation() : filter.TotalWinsAmounts.MapToFiltersOperation(timeZone),
                TotalUncalculatedBetsCounts = filter.TotalUncalculatedBetsCounts == null ? new FiltersOperation() : filter.TotalUncalculatedBetsCounts.MapToFiltersOperation(timeZone),
                TotalUncalculatedBetsAmounts = filter.TotalUncalculatedBetsAmounts == null ? new FiltersOperation() : filter.TotalUncalculatedBetsAmounts.MapToFiltersOperation(timeZone),
                GGRs = filter.GGRs == null ? new FiltersOperation() : filter.GGRs.MapToFiltersOperation(timeZone)
            };
        }

        public static FilterReportByPaymentSystem MapToFilterReportByPaymentSystem(this ApiFilterReportByPaymentSystem filter, double timeZone)
        {
            return new FilterReportByPaymentSystem
            {
                PartnerId = filter.PartnerId,
                FromDate = filter.FromDate.GetUTCDateFromGMT(timeZone),
                ToDate = filter.ToDate.GetUTCDateFromGMT(timeZone),
                PartnerIds = filter.PartnerIds == null ? new FiltersOperation() : filter.PartnerIds.MapToFiltersOperation(timeZone),
                PaymentSystemIds = filter.PaymentSystemIds == null ? new FiltersOperation() : filter.PaymentSystemIds.MapToFiltersOperation(timeZone),
                PaymentSystemNames = filter.PaymentSystemNames == null ? new FiltersOperation() : filter.PaymentSystemNames.MapToFiltersOperation(timeZone),
                Statuses = filter.Statuses == null ? new FiltersOperation() : filter.Statuses.MapToFiltersOperation(timeZone),
                Counts = filter.Counts == null ? new FiltersOperation() : filter.Counts.MapToFiltersOperation(timeZone),
                TotalAmounts = filter.TotalAmounts == null ? new FiltersOperation() : filter.TotalAmounts.MapToFiltersOperation(timeZone)
            };
        }
        public static FilterReportByPartner MapToFilterReportByPartner(this ApiFilterReportByPartner filter, double timeZone)
        {
            return new FilterReportByPartner
            {
                FromDate = filter.FromDate.GetUTCDateFromGMT(timeZone),
                ToDate = filter.ToDate.GetUTCDateFromGMT(timeZone),
                PartnerIds = filter.PartnerIds == null ? new FiltersOperation() : filter.PartnerIds.MapToFiltersOperation(timeZone),
                PartnerNames = filter.PartnerNames == null ? new FiltersOperation() : filter.PartnerNames.MapToFiltersOperation(timeZone),
                TotalBetAmounts = filter.TotalBetAmounts == null ? new FiltersOperation() : filter.TotalBetAmounts.MapToFiltersOperation(timeZone),
                TotalBetsCounts = filter.TotalBetsCounts == null ? new FiltersOperation() : filter.TotalBetsCounts.MapToFiltersOperation(timeZone),
                TotalWinAmounts = filter.TotalWinAmounts == null ? new FiltersOperation() : filter.TotalWinAmounts.MapToFiltersOperation(timeZone),
                TotalGGRs = filter.TotalGGRs == null ? new FiltersOperation() : filter.TotalGGRs.MapToFiltersOperation(timeZone)
            };
        }

        public static FilterReportByUserTransaction MapToFilterReportByUserTransaction(this ApiFilterReportByUserTransaction filter, double timeZone)
        {
            return new FilterReportByUserTransaction
            {
                PartnerId = filter.PartnerId,
                FromDate = filter.FromDate.GetUTCDateFromGMT(timeZone),
                ToDate = filter.ToDate.GetUTCDateFromGMT(timeZone),
                PartnerIds = filter.PartnerIds == null ? new FiltersOperation() : filter.PartnerIds.MapToFiltersOperation(timeZone),
                UserIds = filter.UserIds == null ? new FiltersOperation() : filter.UserIds.MapToFiltersOperation(timeZone),
                Usernames = filter.Usernames == null ? new FiltersOperation() : filter.Usernames.MapToFiltersOperation(timeZone),
                NickNames = filter.NickNames == null ? new FiltersOperation() : filter.NickNames.MapToFiltersOperation(timeZone),
                UserFirstNames = filter.UserFirstNames == null ? new FiltersOperation() : filter.UserFirstNames.MapToFiltersOperation(timeZone),
                UserLastNames = filter.UserLastNames == null ? new FiltersOperation() : filter.UserLastNames.MapToFiltersOperation(timeZone),
                FromUserIds = filter.FromUserIds == null ? new FiltersOperation() : filter.FromUserIds.MapToFiltersOperation(timeZone),
                FromUsernames = filter.FromUsernames == null ? new FiltersOperation() : filter.FromUsernames.MapToFiltersOperation(timeZone),
                ClientIds = filter.ClientIds == null ? new FiltersOperation() : filter.ClientIds.MapToFiltersOperation(timeZone),
                ClientUsernames = filter.ClientUsernames == null ? new FiltersOperation() : filter.ClientUsernames.MapToFiltersOperation(timeZone),
                OperationTypeIds = filter.OperationTypeIds == null ? new FiltersOperation() : filter.OperationTypeIds.MapToFiltersOperation(timeZone),
                Amounts = filter.Amounts == null ? new FiltersOperation() : filter.Amounts.MapToFiltersOperation(timeZone),
                CurrencyIds = filter.CurrencyIds == null ? new FiltersOperation() : filter.CurrencyIds.MapToFiltersOperation(timeZone)
            };
        }

        public static DataWarehouse.Filters.FilterUserCorrection MapToFilterReportByUserCorrection(this Filters.Reporting.ApiFilterUserCorrection filter, double timeZone)
        {
            return new DataWarehouse.Filters.FilterUserCorrection
            {
                PartnerId = filter.PartnerId,
                FromDate = filter.FromDate.GetUTCDateFromGMT(timeZone),
                ToDate = filter.ToDate.GetUTCDateFromGMT(timeZone),
                PartnerIds = filter.PartnerIds == null ? new FiltersOperation() : filter.PartnerIds.MapToFiltersOperation(timeZone),
                UserIds = filter.UserIds == null ? new FiltersOperation() : filter.UserIds.MapToFiltersOperation(timeZone),
                UserNames = filter.UserNames == null ? new FiltersOperation() : filter.UserNames.MapToFiltersOperation(timeZone),
                TotalDebits = filter.TotalDebits == null ? new FiltersOperation() : filter.TotalDebits.MapToFiltersOperation(timeZone),
                TotalCredits = filter.TotalCredits == null ? new FiltersOperation() : filter.TotalCredits.MapToFiltersOperation(timeZone),
                Balances = filter.Balances == null ? new FiltersOperation() : filter.Balances.MapToFiltersOperation(timeZone)
            };
        }


        public static FilterReportByProduct MapToFilterReportByProduct(this ApiFilterReportByProduct filter, double timeZone)
        {
            return new FilterReportByProduct
            {
                FromDate = filter.FromDate.GetUTCDateFromGMT(timeZone),
                ToDate = filter.ToDate.GetUTCDateFromGMT(timeZone),
                ClientIds = filter.ClientIds == null ? new FiltersOperation() : filter.ClientIds.MapToFiltersOperation(timeZone),
                ClientNames = filter.ClientNames == null ? new FiltersOperation() : filter.ClientNames.MapToFiltersOperation(timeZone),
                Currencies = filter.Currencies == null ? new FiltersOperation() : filter.Currencies.MapToFiltersOperation(timeZone),
                ProductIds = filter.ProductIds == null ? new FiltersOperation() : filter.ProductIds.MapToFiltersOperation(timeZone),
                ProductNames = filter.ProductNames == null ? new FiltersOperation() : filter.ProductNames.MapToFiltersOperation(timeZone),
                DeviceTypeIds = filter.DeviceTypeIds == null ? new FiltersOperation() : filter.DeviceTypeIds.MapToFiltersOperation(timeZone),
                ProviderNames = filter.ProviderNames == null ? new FiltersOperation() : filter.ProviderNames.MapToFiltersOperation(timeZone),
                TotalBetsAmounts = filter.TotalBetsAmounts == null ? new FiltersOperation() : filter.TotalBetsAmounts.MapToFiltersOperation(timeZone),
                TotalWinsAmounts = filter.TotalWinsAmounts == null ? new FiltersOperation() : filter.TotalWinsAmounts.MapToFiltersOperation(timeZone),
                TotalBetsCounts = filter.TotalBetsCounts == null ? new FiltersOperation() : filter.TotalBetsCounts.MapToFiltersOperation(timeZone),
                TotalUncalculatedBetsCounts = filter.TotalUncalculatedBetsCounts == null ? new FiltersOperation() : filter.TotalUncalculatedBetsCounts.MapToFiltersOperation(timeZone),
                TotalUncalculatedBetsAmounts = filter.TotalUncalculatedBetsAmounts == null ? new FiltersOperation() : filter.TotalUncalculatedBetsAmounts.MapToFiltersOperation(timeZone),
                GGRs = filter.GGRs == null ? new FiltersOperation() : filter.GGRs.MapToFiltersOperation(timeZone)
            };
        }

        #endregion

        #region BusinessAudit Reports

        public static FilterReportByActionLog MapToFilterReportByActionLog(this ApiFilterReportByActionLog filter, double timeZone)
        {
            return new FilterReportByActionLog
            {
                FromDate = filter.FromDate.GetUTCDateFromGMT(timeZone),
                ToDate = filter.ToDate.GetUTCDateFromGMT(timeZone),
                Ids = filter.Ids == null ? new FiltersOperation() : filter.Ids.MapToFiltersOperation(timeZone),
                ActionNames = filter.ActionNames == null ? new FiltersOperation() : filter.ActionNames.MapToFiltersOperation(timeZone),
                ActionGroups = filter.ActionGroups == null ? new FiltersOperation() : filter.ActionGroups.MapToFiltersOperation(timeZone),
                UserIds = filter.UserIds == null ? new FiltersOperation() : filter.UserIds.MapToFiltersOperation(timeZone),
                Domains = filter.Domains == null ? new FiltersOperation() : filter.Domains.MapToFiltersOperation(timeZone),
                Ips = filter.Ips == null ? new FiltersOperation() : filter.Ips.MapToFiltersOperation(timeZone),
                Sources = filter.Sources == null ? new FiltersOperation() : filter.Sources.MapToFiltersOperation(timeZone),
                Countries = filter.Countries == null ? new FiltersOperation() : filter.Countries.MapToFiltersOperation(timeZone),
                SessionIds = filter.SessionIds == null ? new FiltersOperation() : filter.SessionIds.MapToFiltersOperation(timeZone),
                Languages = filter.Languages == null ? new FiltersOperation() : filter.Languages.MapToFiltersOperation(timeZone),
                ResultCodes = filter.ResultCodes == null ? new FiltersOperation() : filter.ResultCodes.MapToFiltersOperation(timeZone),
                Pages = filter.Pages == null ? new FiltersOperation() : filter.Pages.MapToFiltersOperation(timeZone),
                Descriptions = filter.Descriptions == null ? new FiltersOperation() : filter.Descriptions.MapToFiltersOperation(timeZone),
                SkipCount = filter.SkipCount,
                TakeCount = filter.TakeCount,
                OrderBy = filter.OrderBy,
                FieldNameToOrderBy = filter.FieldNameToOrderBy
            };
        }

        public static List<ApiReportByActionLog> MapToApiReportByActionLog(this IEnumerable<fnActionLog> input, double timeZone)
        {
            return input.Select(x => x.MapToApiReportByActionLog(timeZone)).ToList();
        }

        public static ApiReportByActionLog MapToApiReportByActionLog(this fnActionLog input, double timeZone)
        {
            return new ApiReportByActionLog
            {
                Id = input.Id,
                UserId = input.ObjectId,
                ActionName = input.ActionName,
                ActionGroup = input.ActionGroup,
                Domain = input.Domain,
                Source = input.Source,
                Country = input.Country,
                Ip = input.Ip,
                SessionId = input.SessionId,
                Language = input.Language,
                ResultCode = input.ResultCode,
                Description = input.Description,
                Page = input.Page,
                Info = input.Info,
                CreationTime = input.CreationTime.GetGMTDateFromUTC(timeZone)
            };
        }

        public static FilterReportByPopupStatistics MapToFilterReportByPopupStatistics(this ApiFilterReportByPopupStatistics filter, double timeZone)
        {
            return new FilterReportByPopupStatistics
            {
                PopupId = filter.PopupId,
                FromDate = filter.FromDate.GetUTCDateFromGMT(timeZone),
                ToDate = filter.ToDate.GetUTCDateFromGMT(timeZone),
                Ids = filter.Ids == null ? new FiltersOperation() : filter.Ids.MapToFiltersOperation(timeZone),
                PartnerIds = filter.PartnerIds == null ? new FiltersOperation() : filter.PartnerIds.MapToFiltersOperation(timeZone),
                NickNames = filter.NickNames == null ? new FiltersOperation() : filter.NickNames.MapToFiltersOperation(timeZone),
                Types = filter.Types == null ? new FiltersOperation() : filter.Types.MapToFiltersOperation(timeZone),
                DeviceTypes = filter.DeviceTypes == null ? new FiltersOperation() : filter.DeviceTypes.MapToFiltersOperation(timeZone),
                States = filter.States == null ? new FiltersOperation() : filter.States.MapToFiltersOperation(timeZone),
                CreationTimes = filter.CreationTimes == null ? new FiltersOperation() : filter.CreationTimes.MapToFiltersOperation(timeZone),
                LastUpdateTimes = filter.LastUpdateTimes == null ? new FiltersOperation() : filter.LastUpdateTimes.MapToFiltersOperation(timeZone),
                Vieweds = filter.Vieweds == null ? new FiltersOperation() : filter.Vieweds.MapToFiltersOperation(timeZone),
                Closeds = filter.Closeds == null ? new FiltersOperation() : filter.Closeds.MapToFiltersOperation(timeZone),
                Redirecteds = filter.Redirecteds == null ? new FiltersOperation() : filter.Redirecteds.MapToFiltersOperation(timeZone),
                SkipCount = filter.SkipCount,
                TakeCount = filter.TakeCount,
                OrderBy = filter.OrderBy,
                FieldNameToOrderBy = filter.FieldNameToOrderBy
            };
        }

        #endregion

        #region Accounting Reports

        public static FilterPartnerPaymentsSummary MapToFilterPartnerPaymentsSummary(this ApiFilterPartnerPaymentsSummary filter, double timeZone)
        {
            return new FilterPartnerPaymentsSummary
            {
                PartnerId = filter.PartnerId,
                Type = filter.Type,
                FromDate = filter.FromDate.GetUTCDateFromGMT(timeZone),
                ToDate = filter.ToDate.GetUTCDateFromGMT(timeZone)
            };
        }

        #endregion

        #region Affiliates And Agents

        public static FilterfnAffiliateTransaction ToFilterfnAffiliateTransaction(this ApiFilterfnAgentTransaction filter, double timeZone)
        {
            var currentDate = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(filter.FieldNameToOrderBy))
            {
                var orderBy = filter.FieldNameToOrderBy;
                switch (orderBy)
                {
                    case "ProductGroupId":
                        filter.FieldNameToOrderBy = "ProductId";
                        break;
                    default:
                        break;
                }
            }
            return new FilterfnAffiliateTransaction
            {
                FromDate = filter.FromDate.GetUTCDateFromGMT(timeZone) ??
                currentDate.AddDays((filter.IsYesterday.HasValue && filter.IsYesterday.Value) ? -2 : -1),
                ToDate = filter.ToDate.GetUTCDateFromGMT(timeZone) ?? currentDate.AddDays(1),
                Ids = filter.Ids == null ? new FiltersOperation() : filter.Ids.MapToFiltersOperation(timeZone),
                ExternalTransactionIds = filter.ExternalTransactionIds == null ? new FiltersOperation() : filter.ExternalTransactionIds.MapToFiltersOperation(timeZone),
                Amounts = filter.Amounts == null ? new FiltersOperation() : filter.Amounts.MapToFiltersOperation(timeZone),
                CurrencyIds = filter.CurrencyIds == null ? new FiltersOperation() : filter.CurrencyIds.MapToFiltersOperation(timeZone),
                ProductIds = filter.ProductIds == null ? new FiltersOperation() : filter.ProductIds.MapToFiltersOperation(timeZone),
                ProductNames = filter.ProductNames == null ? new FiltersOperation() : filter.ProductNames.MapToFiltersOperation(timeZone),
                TransactionTypes = filter.TransactionTypes == null ? new FiltersOperation() : filter.TransactionTypes.MapToFiltersOperation(timeZone),
                CreationTimes = filter.CreationTimes == null ? new FiltersOperation() : filter.CreationTimes.MapToFiltersOperation(timeZone),
                LastUpdateTimes = filter.LastUpdateTimes == null ? new FiltersOperation() : filter.LastUpdateTimes.MapToFiltersOperation(timeZone),
                SkipCount = filter.SkipCount,
                TakeCount = Math.Min(filter.TakeCount, 5000),
                OrderBy = filter.OrderBy,
                FieldNameToOrderBy = filter.FieldNameToOrderBy
            };
        }

        public static FilterfnAgentTransaction ToFilterfnAgentTransaction(this ApiFilterfnAgentTransaction filter, double timeZone)
        {
            var currentDate = DateTime.UtcNow;
            return new FilterfnAgentTransaction
            {
                FromDate = filter.FromDate.GetUTCDateFromGMT(timeZone) ??
                currentDate.AddDays((filter.IsYesterday.HasValue && filter.IsYesterday.Value) ? -2 : -1),
                ToDate = filter.ToDate.GetUTCDateFromGMT(timeZone) ?? currentDate.AddDays(1),
                UserState = filter.UserState,
                Ids = filter.Ids == null ? new FiltersOperation() : filter.Ids.MapToFiltersOperation(timeZone),
                FromUserIds = filter.FromUserIds == null ? new FiltersOperation() : filter.FromUserIds.MapToFiltersOperation(timeZone),
                UserIds = filter.UserIds == null ? new FiltersOperation() : filter.UserIds.MapToFiltersOperation(timeZone),
                ExternalTransactionIds = filter.ExternalTransactionIds == null ? new FiltersOperation() : filter.ExternalTransactionIds.MapToFiltersOperation(timeZone),
                Amounts = filter.Amounts == null ? new FiltersOperation() : filter.Amounts.MapToFiltersOperation(timeZone),
                CurrencyIds = filter.CurrencyIds == null ? new FiltersOperation() : filter.CurrencyIds.MapToFiltersOperation(timeZone),
                States = filter.States == null ? new FiltersOperation() : filter.States.MapToFiltersOperation(timeZone),
                OperationTypeIds = filter.OperationTypeIds == null ? new FiltersOperation() : filter.OperationTypeIds.MapToFiltersOperation(timeZone),
                ProductIds = filter.ProductIds == null ? new FiltersOperation() : filter.ProductIds.MapToFiltersOperation(timeZone),
                ProductNames = filter.ProductNames == null ? new FiltersOperation() : filter.ProductNames.MapToFiltersOperation(timeZone),
                TransactionTypes = filter.TransactionTypes == null ? new FiltersOperation() : filter.TransactionTypes.MapToFiltersOperation(timeZone),
                SkipCount = filter.SkipCount,
                TakeCount = Math.Min(filter.TakeCount, 5000),
                OrderBy = filter.OrderBy,
                FieldNameToOrderBy = filter.FieldNameToOrderBy
            };
        }

        #endregion

        #endregion

        #region Clients

        public static FilterAccountsBalanceHistory MapToFilterAccountsBalanceHistory(this ApiFilterAccountsBalanceHistory filter, double timeZone)
        {
            return new FilterAccountsBalanceHistory
            {
                FromDate = filter.FromDate.GetUTCDateFromGMT(timeZone),
                ToDate = filter.ToDate.GetUTCDateFromGMT(timeZone),
                ClientId = filter.ClientId,
                UserId = filter.UserId,
                AccountId = filter.AccountId
            };
        }

        public static FilterfnDuplicateClient MapToFilterDuplicateClient(this ApiFilterfnDuplicateClient filter, double timeZone)
        {
            if (!string.IsNullOrEmpty(filter.FieldNameToOrderBy))
            {
                var orderBy = filter.FieldNameToOrderBy;
                switch (orderBy)
                {
                    case "MatchDate":
                        filter.FieldNameToOrderBy = "LastUpdateTime";
                        break;
                    default:
                        break;
                }
            }

            return new FilterfnDuplicateClient
            {
                ClientId = filter.ClientId,
                DuplicatedClientIds = filter.DuplicatedClientIds == null ? new FiltersOperation() : filter.DuplicatedClientIds.MapToFiltersOperation(timeZone),
                DuplicatedDatas = filter.DuplicatedDatas == null ? new FiltersOperation() : filter.DuplicatedDatas.MapToFiltersOperation(timeZone),
                MatchDates = filter.MatchDates == null ? new FiltersOperation() : filter.MatchDates.MapToFiltersOperation(timeZone),
                SkipCount = filter.SkipCount,
                TakeCount = filter.TakeCount,
                OrderBy = filter.OrderBy,
                FieldNameToOrderBy = filter.FieldNameToOrderBy
            };
        }

        public static FilterClientGame MapToFilterClientGame(this ApiFilterClientGame filter, double timeZone)
        {
            return new FilterClientGame
            {
                PartnerId = filter.PartnerId,
                FromDate = filter.FromDate.GetUTCDateFromGMT(timeZone),
                ToDate = filter.ToDate.GetUTCDateFromGMT(timeZone),
                ClientIds = filter.ClientIds == null ? new FiltersOperation() : filter.ClientIds.MapToFiltersOperation(timeZone),
                FirstNames = filter.FirstNames == null ? new FiltersOperation() : filter.FirstNames.MapToFiltersOperation(timeZone),
                LastNames = filter.LastNames == null ? new FiltersOperation() : filter.LastNames.MapToFiltersOperation(timeZone),
                ProductIds = filter.ProductIds == null ? new FiltersOperation() : filter.ProductIds.MapToFiltersOperation(timeZone),
                ProductNames = filter.ProductNames == null ? new FiltersOperation() : filter.ProductNames.MapToFiltersOperation(timeZone),
                ProviderNames = filter.ProviderNames == null ? new FiltersOperation() : filter.ProviderNames.MapToFiltersOperation(timeZone),
                Currencies = filter.CurrencyIds == null ? new FiltersOperation() : filter.CurrencyIds.MapToFiltersOperation(timeZone),
            };
        }

        #endregion

        #region FilterBetshop

        public static FilterfnBetShop MaptToFilterfnBetShop(this ApiFilterBetShop filter, double timeZone)
        {
            return new FilterfnBetShop
            {
                SkipCount = filter.SkipCount,
                TakeCount = filter.TakeCount,
                CreatedBefore = filter.CreatedBefore.GetUTCDateFromGMT(timeZone),
                CreatedFrom = filter.CreatedFrom.GetUTCDateFromGMT(timeZone),
                PartnerId = filter.PartnerId,
                Ids = filter.Ids == null ? new FiltersOperation() : filter.Ids.MapToFiltersOperation(timeZone),
                GroupIds = filter.GroupIds == null ? new FiltersOperation() : filter.GroupIds.MapToFiltersOperation(timeZone),
                States = filter.States == null ? new FiltersOperation() : filter.States.MapToFiltersOperation(timeZone),
                CurrencyIds = filter.CurrencyIds == null ? new FiltersOperation() : filter.CurrencyIds.MapToFiltersOperation(timeZone),
                Names = filter.Names == null ? new FiltersOperation() : filter.Names.MapToFiltersOperation(timeZone),
                Addresses = filter.Addresses == null ? new FiltersOperation() : filter.Addresses.MapToFiltersOperation(timeZone),
                Balances = filter.Balances == null ? new FiltersOperation() : filter.Balances.MapToFiltersOperation(timeZone),
                CurrentLimits = filter.CurrentLimits == null ? new FiltersOperation() : filter.CurrentLimits.MapToFiltersOperation(timeZone),
                AgentIds = filter.AgentIds == null ? new FiltersOperation() : filter.AgentIds.MapToFiltersOperation(timeZone),
                MaxCopyCounts = filter.MaxCopyCounts == null ? new FiltersOperation() : filter.MaxCopyCounts.MapToFiltersOperation(timeZone),
                MaxWinAmounts = filter.MaxWinAmounts == null ? new FiltersOperation() : filter.MaxWinAmounts.MapToFiltersOperation(timeZone),
                MinBetAmounts = filter.MinBetAmounts == null ? new FiltersOperation() : filter.MinBetAmounts.MapToFiltersOperation(timeZone),
                MaxEventCountPerTickets = filter.MaxEventCountPerTickets == null ? new FiltersOperation() : filter.MaxEventCountPerTickets.MapToFiltersOperation(timeZone),
                CommissionTypes = filter.CommissionTypes == null ? new FiltersOperation() : filter.CommissionTypes.MapToFiltersOperation(timeZone),
                CommissionRates = filter.CommissionRates == null ? new FiltersOperation() : filter.CommissionRates.MapToFiltersOperation(timeZone),
                AnonymousBets = filter.AnonymousBets == null ? new FiltersOperation() : filter.AnonymousBets.MapToFiltersOperation(timeZone),
                AllowCashouts = filter.AllowCashouts == null ? new FiltersOperation() : filter.AllowCashouts.MapToFiltersOperation(timeZone),
                AllowLives = filter.AllowLives == null ? new FiltersOperation() : filter.AllowLives.MapToFiltersOperation(timeZone),
                UsePins = filter.UsePins == null ? new FiltersOperation() : filter.UsePins.MapToFiltersOperation(timeZone),
                OrderBy = filter.OrderBy,
                FieldNameToOrderBy = filter.FieldNameToOrderBy
            };
        }

        #endregion

        #region FilterBetshopGroup

        public static FilterBetShopGroup MaptToFilterBetShopGroup(this ApiFilterBetShopGroup filterBetShopGroup)
        {
            return new FilterBetShopGroup
            {
                Id = filterBetShopGroup.Id,
                TakeCount = filterBetShopGroup.TakeCount,
                SkipCount = filterBetShopGroup.SkipCount,
                Name = filterBetShopGroup.Name,
                ParentId = filterBetShopGroup.ParentId,
                PartnerId = filterBetShopGroup.PartnerId,
                IsRoot = filterBetShopGroup.IsRoot,
                IsLeaf = filterBetShopGroup.IsLeaf,
                State = filterBetShopGroup.State,
                OrderBy = filterBetShopGroup.OrderBy,
                FieldNameToOrderBy = filterBetShopGroup.FieldNameToOrderBy
            };
        }

        public static List<FilterBetShopGroup> MapToFilterBetShopGroups(this IEnumerable<ApiFilterBetShopGroup> filterBetShopGroups)
        {
            return filterBetShopGroups.Select(MaptToFilterBetShopGroup).ToList();
        }

        #endregion

        #region FilterCashDesk

        public static FilterfnCashDesk MapToFilterfnCashDesk(this ApiFilterCashDesk cashDesk)
        {
            return new FilterfnCashDesk
            {
                Id = cashDesk.Id,
                BetShopId = cashDesk.BetShopId,
                Name = cashDesk.Name,
                CreatedBefore = cashDesk.CreatedBefore,
                CreatedFrom = cashDesk.CreatedFrom,
                SkipCount = cashDesk.SkipCount,
                TakeCount = cashDesk.TakeCount,
                OrderBy = cashDesk.OrderBy,
                FieldNameToOrderBy = cashDesk.FieldNameToOrderBy
            };
        }

        public static List<FilterfnCashDesk> MapToFilterCashDesks(this IEnumerable<ApiFilterCashDesk> filterCashDesks)
        {
            return filterCashDesks.Select(MapToFilterfnCashDesk).ToList();
        }

        #endregion

        #region FilterfnTranslationEntry

        public static FilterfnObjectTranslationEntry MaptToFilterTranslation(this ApiFilterTranslationEntry filterTranslationEntry)
        {
            return new FilterfnObjectTranslationEntry
            {
                ObjectTypeId = filterTranslationEntry.ObjectTypeId,
                TakeCount = filterTranslationEntry.TakeCount,
                SkipCount = filterTranslationEntry.SkipCount,
                SelectedLanguages = filterTranslationEntry.SelectedLanguages,
                SearchText = filterTranslationEntry.SearchText,
                SearchLanguage = filterTranslationEntry.SearchLanguage
            };
        }

        public static List<FilterfnObjectTranslationEntry> MapToFilterTranslations(this IEnumerable<ApiFilterTranslationEntry> filterTranslationEntries)
        {
            return filterTranslationEntries.Select(MaptToFilterTranslation).ToList();
        }

        #endregion

        #region FilterUser
        public static FilterUser MaptToFilterUser(this ApiFilterUser filter)
        {
            return new FilterUser
            {
                PartnerId = filter.PartnerId,
                Ids = filter.Ids == null ? new FiltersOperation() : filter.Ids.MapToFiltersOperation(filter.TimeZone),
                FirstNames = filter.FirstNames == null ? new FiltersOperation() : filter.FirstNames.MapToFiltersOperation(filter.TimeZone),
                LastNames = filter.LastNames == null ? new FiltersOperation() : filter.LastNames.MapToFiltersOperation(filter.TimeZone),
                UserNames = filter.UserNames == null ? new FiltersOperation() : filter.UserNames.MapToFiltersOperation(filter.TimeZone),
                Emails = filter.Emails == null ? new FiltersOperation() : filter.Emails.MapToFiltersOperation(filter.TimeZone),
                Genders = filter.Genders == null ? new FiltersOperation() : filter.Genders.MapToFiltersOperation(filter.TimeZone),
                Currencies = filter.Currencies == null ? new FiltersOperation() : filter.Currencies.MapToFiltersOperation(filter.TimeZone),
                LanguageIds = filter.LanguageIds == null ? new FiltersOperation() : filter.LanguageIds.MapToFiltersOperation(filter.TimeZone),
                UserStates = filter.UserStates == null ? new FiltersOperation() : filter.UserStates.MapToFiltersOperation(filter.TimeZone),
                UserTypes = filter.UserTypes == null ? new FiltersOperation() : filter.UserTypes.MapToFiltersOperation(filter.TimeZone),
                SkipCount = filter.SkipCount,
                TakeCount = filter.TakeCount,
                OrderBy = filter.OrderBy,
                FieldNameToOrderBy = filter.FieldNameToOrderBy
            };
        }
        public static FilterfnUser ToFilterfnUser(this ApiFilterUser filter)
        {
            if (!string.IsNullOrEmpty(filter.FieldNameToOrderBy))
            {
                var orderBy = filter.FieldNameToOrderBy;
                switch (orderBy)
                {
                    case "UserType":
                        filter.FieldNameToOrderBy = "Type";
                        break;
                    default:
                        break;
                }
            }

            return new FilterfnUser
            {
                PartnerId = filter.PartnerId,
                Ids = filter.Ids == null ? new FiltersOperation() : filter.Ids.MapToFiltersOperation(filter.TimeZone),
                FirstNames = filter.FirstNames == null ? new FiltersOperation() : filter.FirstNames.MapToFiltersOperation(filter.TimeZone),
                LastNames = filter.LastNames == null ? new FiltersOperation() : filter.LastNames.MapToFiltersOperation(filter.TimeZone),
                UserNames = filter.UserNames == null ? new FiltersOperation() : filter.UserNames.MapToFiltersOperation(filter.TimeZone),
                Emails = filter.Emails == null ? new FiltersOperation() : filter.Emails.MapToFiltersOperation(filter.TimeZone),
                Genders = filter.Genders == null ? new FiltersOperation() : filter.Genders.MapToFiltersOperation(filter.TimeZone),
                Currencies = filter.Currencies == null ? new FiltersOperation() : filter.Currencies.MapToFiltersOperation(filter.TimeZone),
                LanguageIds = filter.LanguageIds == null ? new FiltersOperation() : filter.LanguageIds.MapToFiltersOperation(filter.TimeZone),
                UserStates = filter.UserStates == null ? new FiltersOperation() : filter.UserStates.MapToFiltersOperation(filter.TimeZone),
                UserTypes = filter.UserTypes == null ? new FiltersOperation() : filter.UserTypes.MapToFiltersOperation(filter.TimeZone),
                UserRoles = filter.UserRoles == null ? new FiltersOperation() : filter.UserRoles.MapToFiltersOperation(filter.TimeZone),
                SkipCount = filter.SkipCount,
                TakeCount = filter.TakeCount,
                OrderBy = filter.OrderBy,
                FieldNameToOrderBy = filter.FieldNameToOrderBy
            };
        }

        public static FilterfnUser ToFilterfnUser(this ApiFilterfnAgent filter, double timeZone)
        {
            if (!string.IsNullOrEmpty(filter.FieldNameToOrderBy))
            {
                var orderBy = filter.FieldNameToOrderBy;
                switch (orderBy)
                {
                    case "UserType":
                        filter.FieldNameToOrderBy = "Type";
                        break;
                    default:
                        break;
                }
            }

            return new FilterfnUser
            {
                FromDate = filter.FromDate.GetUTCDateFromGMT(timeZone),
                ToDate = filter.ToDate.GetUTCDateFromGMT(timeZone),
                PartnerId = filter.PartnerId,
                ParentId = filter.ParentId,
                Ids = filter.Ids == null ? new FiltersOperation() : filter.Ids.MapToFiltersOperation(timeZone),
                FirstNames = filter.FirstNames == null ? new FiltersOperation() : filter.FirstNames.MapToFiltersOperation(timeZone),
                LastNames = filter.LastNames == null ? new FiltersOperation() : filter.LastNames.MapToFiltersOperation(timeZone),
                UserNames = filter.UserNames == null ? new FiltersOperation() : filter.UserNames.MapToFiltersOperation(timeZone),
                Emails = filter.Emails == null ? new FiltersOperation() : filter.Emails.MapToFiltersOperation(timeZone),
                Genders = filter.Genders == null ? new FiltersOperation() : filter.Genders.MapToFiltersOperation(timeZone),
                Currencies = filter.Currencies == null ? new FiltersOperation() : filter.Currencies.MapToFiltersOperation(timeZone),
                LanguageIds = filter.LanguageIds == null ? new FiltersOperation() : filter.LanguageIds.MapToFiltersOperation(timeZone),
                SkipCount = filter.SkipCount,
                TakeCount = filter.TakeCount,
                OrderBy = filter.OrderBy,
                FieldNameToOrderBy = filter.FieldNameToOrderBy
            };
        }

        public static DAL.Filters.FilterUserCorrection MapToFilterUserCorrection(this Filters.ApiFilterUserCorrection filter, double timeZone)
        {
            return new DAL.Filters.FilterUserCorrection
            {
                UserId = filter.UserId,
                TakeCount = filter.TakeCount,
                SkipCount = filter.SkipCount,
                FromDate = filter.FromDate.GetUTCDateFromGMT(timeZone),
                ToDate = filter.ToDate.GetUTCDateFromGMT(timeZone),
                OrderBy = filter.OrderBy,
                FieldNameToOrderBy = filter.FieldNameToOrderBy,
                Ids = filter.Ids == null ? new FiltersOperation() : filter.Ids.MapToFiltersOperation(timeZone),
                Creators = filter.FromUserIds == null ? new FiltersOperation() : filter.FromUserIds.MapToFiltersOperation(timeZone),
                Amounts = filter.Amounts == null ? new FiltersOperation() : filter.Amounts.MapToFiltersOperation(timeZone),
                States = filter.States == null ? new FiltersOperation() : filter.States.MapToFiltersOperation(timeZone),
                CurrencyIds = filter.CurrencyIds == null ? new FiltersOperation() : filter.CurrencyIds.MapToFiltersOperation(timeZone),
                UserIds = filter.UserIds == null ? new FiltersOperation() : filter.UserIds.MapToFiltersOperation(timeZone),
                OperationTypeNames = filter.OperationTypeNames == null ? new FiltersOperation() : filter.OperationTypeNames.MapToFiltersOperation(timeZone),
                CreatorFirstNames = filter.FirstNames == null ? new FiltersOperation() : filter.FirstNames.MapToFiltersOperation(timeZone),
                CreatorLastNames = filter.LastNames == null ? new FiltersOperation() : filter.LastNames.MapToFiltersOperation(timeZone)
            };
        }

        public static ApiUserCorrections MapToApiUserCorrections(this PagedModel<fnUserCorrection> input, double timeZone, int? userId)
        {
            return new ApiUserCorrections
            {
                Count = input.Count,
                Entities = input.Entities.Select(x => x.MapToApiUserCorrection(timeZone, userId)).ToList()
            };
        }
        public static ApiUserCorrection MapToApiUserCorrection(this fnUserCorrection input, double timeZone, int? userId)
        {
            return new ApiUserCorrection
            {
                Id = input.Id,
                Amount = input.Amount,
                CurrencyId = input.CurrencyId,
                State = input.State,
                Info = input.Info,
                UserId = input.UserId,
                FromUserId = input.Creator,
                CreationTime = input.CreationTime.GetGMTDateFromUTC(timeZone),
                LastUpdateTime = input.LastUpdateTime.GetGMTDateFromUTC(timeZone),
                OperationTypeName = input.OperationTypeName,
                CreatorFirstName = input.CreatorFirstName,
                CreatorLastName = input.CreatorLastName,
                UserFirstName = input.UserFirstName,
                UserLastName = input.UserLastName,
                ClientFirstName = input.ClientFirstName,
                ClientLastName = input.ClientLastName,
                ClientId = input.ClientId,
                FirstName = input.Creator == userId ? (input.UserId == null ? input.ClientFirstName : input.UserFirstName) : input.CreatorFirstName,
                LastName = input.Creator == userId ? (input.UserId == null ? input.ClientLastName : input.UserLastName) : input.CreatorLastName,
                HasNote = input.HasNote ?? false
            };
        }

        #endregion

        #region FilterClient

        public static List<FilterClient> MapToFilterClients(this IEnumerable<Filters.ApiFilterClient> filterClients)
        {
            return filterClients.Select(MapToFilterClient).ToList();
        }

        public static FilterClient MapToFilterClient(this Filters.ApiFilterClient filterClient)
        {
            return new FilterClient
            {
                Id = filterClient.Id,
                Email = filterClient.Email,
                UserName = filterClient.UserName,
                CurrencyId = filterClient.CurrencyId,
                PartnerId = filterClient.PartnerId,
                Gender = filterClient.Gender,
                FirstName = filterClient.FirstName,
                LastName = filterClient.LastName,
                DocumentNumber = filterClient.DocumentNumber,
                DocumentIssuedBy = filterClient.DocumentIssuedBy,
                Address = filterClient.Address,
                MobileNumber = filterClient.MobileNumber,
                LanguageId = filterClient.LanguageId,
                Info = filterClient.Info,
                CreatedFrom = filterClient.CreatedFrom,
                CreatedBefore = filterClient.CreatedBefore,
                TakeCount = filterClient.TakeCount,
                SkipCount = filterClient.SkipCount
            };
        }

        #endregion

        #region FilterfnClient

        public static FilterfnClient MapToFilterfnClient(this ApiFilterfnClient filter, double timeZone)
        {
            if (!string.IsNullOrEmpty(filter.FieldNameToOrderBy))
            {
                var orderBy = filter.FieldNameToOrderBy;
                switch (orderBy)
                {
                    case "MobileCode":
                        filter.FieldNameToOrderBy = "PhoneNumber";
                        break;
                    default:
                        break;
                }
            }

            return new FilterfnClient
            {
                PartnerId = filter.PartnerId,
                AgentId = filter.AgentId,
                IsDocumentVerified = filter.IsDocumentVerified,
                CreatedFrom = filter.CreatedFrom.GetUTCDateFromGMT(timeZone),
                CreatedBefore = filter.CreatedBefore.GetUTCDateFromGMT(timeZone),
                UnderMonitoringTypes = filter.UnderMonitoringTypes?.ToString(),
                Duplicated = filter.Duplicated,
                Ids = filter.Ids == null ? new FiltersOperation() : filter.Ids.MapToFiltersOperation(timeZone),
                Emails = filter.Emails == null ? new FiltersOperation() : filter.Emails.MapToFiltersOperation(timeZone),
                UserNames = filter.UserNames == null ? new FiltersOperation() : filter.UserNames.MapToFiltersOperation(timeZone),
                Currencies = filter.Currencies == null ? new FiltersOperation() : filter.Currencies.MapToFiltersOperation(timeZone),
                Genders = filter.Genders == null ? new FiltersOperation() : filter.Genders.MapToFiltersOperation(timeZone),
                FirstNames = filter.FirstNames == null ? new FiltersOperation() : filter.FirstNames.MapToFiltersOperation(timeZone),
                LastNames = filter.LastNames == null ? new FiltersOperation() : filter.LastNames.MapToFiltersOperation(timeZone),
                NickNames = filter.NickNames == null ? new FiltersOperation() : filter.NickNames.MapToFiltersOperation(timeZone),
                SecondNames = filter.SecondNames == null ? new FiltersOperation() : filter.SecondNames.MapToFiltersOperation(timeZone),
                SecondSurnames = filter.SecondSurnames == null ? new FiltersOperation() : filter.SecondSurnames.MapToFiltersOperation(timeZone),
                DocumentNumbers = filter.DocumentNumbers == null ? new FiltersOperation() : filter.DocumentNumbers.MapToFiltersOperation(timeZone),
                DocumentIssuedBys = filter.DocumentIssuedBys == null ? new FiltersOperation() : filter.DocumentIssuedBys.MapToFiltersOperation(timeZone),
                LanguageIds = filter.LanguageIds == null ? new FiltersOperation() : filter.LanguageIds.MapToFiltersOperation(timeZone),
                Categories = filter.Categories == null ? new FiltersOperation() : filter.Categories.MapToFiltersOperation(timeZone),
                MobileNumbers = filter.MobileNumbers == null ? new FiltersOperation() : filter.MobileNumbers.MapToFiltersOperation(timeZone),
                ZipCodes = filter.ZipCodes == null ? new FiltersOperation() : filter.ZipCodes.MapToFiltersOperation(timeZone),
                Cities = filter.Cities == null ? new FiltersOperation() : filter.Cities.MapToFiltersOperation(timeZone),
                PhoneNumbers = filter.MobileCodes == null ? new FiltersOperation() : filter.MobileCodes.MapToFiltersOperation(timeZone),
                RegionIds = filter.RegionIds == null ? new FiltersOperation() : filter.RegionIds.MapToFiltersOperation(timeZone),
                CountryIds = filter.CountryIds == null ? new FiltersOperation() : filter.CountryIds.MapToFiltersOperation(timeZone),
                BirthDates = filter.BirthDates == null ? new FiltersOperation() : filter.BirthDates.MapToFiltersOperation(timeZone),
                Ages = filter.Ages == null ? new FiltersOperation() : filter.Ages.MapToFiltersOperation(timeZone),
                RegionIsoCodes = filter.RegionIsoCodes == null ? new FiltersOperation() : filter.RegionIsoCodes.MapToFiltersOperation(timeZone),
                States = filter.States == null ? new FiltersOperation() : filter.States.MapToFiltersOperation(timeZone),
                CreationTimes = filter.CreationTimes == null ? new FiltersOperation() : filter.CreationTimes.MapToFiltersOperation(timeZone),
                RealBalances = filter.RealBalances == null ? new FiltersOperation() : filter.RealBalances.MapToFiltersOperation(timeZone),
                BonusBalances = filter.BonusBalances == null ? new FiltersOperation() : filter.BonusBalances.MapToFiltersOperation(timeZone),
                GGRs = filter.GGRs == null ? new FiltersOperation() : filter.GGRs.MapToFiltersOperation(timeZone),
                NETGamings = filter.NETGamings == null ? new FiltersOperation() : filter.NETGamings.MapToFiltersOperation(timeZone),
                AffiliatePlatformIds = filter.AffiliatePlatformIds == null ? new FiltersOperation() : filter.AffiliatePlatformIds.MapToFiltersOperation(timeZone),
                AffiliateIds = filter.AffiliateIds == null ? new FiltersOperation() : filter.AffiliateIds.MapToFiltersOperation(timeZone),
                AffiliateReferralIds = filter.AffiliateReferralIds == null ? new FiltersOperation() : filter.AffiliateReferralIds.MapToFiltersOperation(timeZone),
                CharacterLevels = filter.CharacterLevels == null ? new FiltersOperation() : filter.CharacterLevels.MapToFiltersOperation(timeZone),
                UserIds = filter.UserIds == null ? new FiltersOperation() : filter.UserIds.MapToFiltersOperation(timeZone),
                LastDepositDates = filter.LastDepositDates == null ? new FiltersOperation() : filter.LastDepositDates.MapToFiltersOperation(timeZone),
                LastUpdateTimes = filter.LastUpdateTimes == null ? new FiltersOperation() : filter.LastUpdateTimes.MapToFiltersOperation(timeZone),
                LastSessionDates = filter.LastSessionDates == null ? new FiltersOperation() : filter.LastSessionDates.MapToFiltersOperation(timeZone),
                SkipCount = filter.SkipCount,
                TakeCount = filter.TakeCount,
                OrderBy = filter.OrderBy,
                FieldNameToOrderBy = filter.FieldNameToOrderBy,
                Message = filter.Message,
                Subject = filter.Subject

            };
        }

        public static FilterfnSegmentClient MapToFilterfnSegmentClient(this ApiFilterfnSegmentClient filter, double timeZone)
        {
            return new FilterfnSegmentClient
            {
                PartnerId = filter.PartnerId,
                SegmentId = filter.SegmentId,
                CreatedFrom = filter.CreatedFrom.GetUTCDateFromGMT(timeZone),
                CreatedBefore = filter.CreatedBefore.GetUTCDateFromGMT(timeZone),
                Ids = filter.Ids == null ? new FiltersOperation() : filter.Ids.MapToFiltersOperation(timeZone),
                Emails = filter.Emails == null ? new FiltersOperation() : filter.Emails.MapToFiltersOperation(timeZone),
                UserNames = filter.UserNames == null ? new FiltersOperation() : filter.UserNames.MapToFiltersOperation(timeZone),
                Currencies = filter.Currencies == null ? new FiltersOperation() : filter.Currencies.MapToFiltersOperation(timeZone),
                Genders = filter.Genders == null ? new FiltersOperation() : filter.Genders.MapToFiltersOperation(timeZone),
                FirstNames = filter.FirstNames == null ? new FiltersOperation() : filter.FirstNames.MapToFiltersOperation(timeZone),
                LastNames = filter.LastNames == null ? new FiltersOperation() : filter.LastNames.MapToFiltersOperation(timeZone),
                SecondNames = filter.SecondNames == null ? new FiltersOperation() : filter.SecondNames.MapToFiltersOperation(timeZone),
                SecondSurnames = filter.SecondSurnames == null ? new FiltersOperation() : filter.SecondSurnames.MapToFiltersOperation(timeZone),
                DocumentNumbers = filter.DocumentNumbers == null ? new FiltersOperation() : filter.DocumentNumbers.MapToFiltersOperation(timeZone),
                DocumentIssuedBys = filter.DocumentIssuedBys == null ? new FiltersOperation() : filter.DocumentIssuedBys.MapToFiltersOperation(timeZone),
                LanguageIds = filter.LanguageIds == null ? new FiltersOperation() : filter.LanguageIds.MapToFiltersOperation(timeZone),
                Categories = filter.Categories == null ? new FiltersOperation() : filter.Categories.MapToFiltersOperation(timeZone),
                MobileNumbers = filter.MobileNumbers == null ? new FiltersOperation() : filter.MobileNumbers.MapToFiltersOperation(timeZone),
                ZipCodes = filter.ZipCodes == null ? new FiltersOperation() : filter.ZipCodes.MapToFiltersOperation(timeZone),
                IsDocumentVerifieds = filter.IsDocumentVerifieds == null ? new FiltersOperation() : filter.IsDocumentVerifieds.MapToFiltersOperation(timeZone),
                PhoneNumbers = filter.PhoneNumbers == null ? new FiltersOperation() : filter.PhoneNumbers.MapToFiltersOperation(timeZone),
                RegionIds = filter.RegionIds == null ? new FiltersOperation() : filter.RegionIds.MapToFiltersOperation(timeZone),
                BirthDates = filter.BirthDates == null ? new FiltersOperation() : filter.BirthDates.MapToFiltersOperation(timeZone),
                States = filter.States == null ? new FiltersOperation() : filter.States.MapToFiltersOperation(timeZone),
                CreationTimes = filter.CreationTimes == null ? new FiltersOperation() : filter.CreationTimes.MapToFiltersOperation(timeZone),
                UserIds = filter.UserIds == null ? new FiltersOperation() : filter.UserIds.MapToFiltersOperation(timeZone),
                AffiliatePlatformIds = filter.AffiliatePlatformIds == null ? new FiltersOperation() : filter.AffiliatePlatformIds.MapToFiltersOperation(timeZone),
                AffiliateIds = filter.AffiliateIds == null ? new FiltersOperation() : filter.AffiliateIds.MapToFiltersOperation(timeZone),
                AffiliateReferralIds = filter.AffiliateReferralIds == null ? new FiltersOperation() : filter.AffiliateReferralIds.MapToFiltersOperation(timeZone),
                SegmentIds = filter.SegmentIds == null ? new FiltersOperation() : filter.SegmentIds.MapToFiltersOperation(timeZone),
                SkipCount = filter.SkipCount,
                TakeCount = filter.TakeCount,
                OrderBy = filter.OrderBy,
                FieldNameToOrderBy = filter.FieldNameToOrderBy
            };
        }

        #endregion

        #region FilterAffiliates

        public static FilterfnAffiliate MapToFilterfnAffiliate(this ApiFilterfnAffiliate filter, double timeZone)
        {
            return new FilterfnAffiliate
            {
                PartnerId = filter.PartnerId,
                CreatedFrom = filter.CreatedFrom.GetUTCDateFromGMT(timeZone),
                CreatedBefore = filter.CreatedBefore.GetUTCDateFromGMT(timeZone),
                Ids = filter.Ids == null ? new FiltersOperation() : filter.Ids.MapToFiltersOperation(timeZone),
                Emails = filter.Emails == null ? new FiltersOperation() : filter.Emails.MapToFiltersOperation(timeZone),
                UserNames = filter.UserNames == null ? new FiltersOperation() : filter.UserNames.MapToFiltersOperation(timeZone),
                FirstNames = filter.FirstNames == null ? new FiltersOperation() : filter.FirstNames.MapToFiltersOperation(timeZone),
                LastNames = filter.LastNames == null ? new FiltersOperation() : filter.LastNames.MapToFiltersOperation(timeZone),
                MobileNumbers = filter.MobileNumbers == null ? new FiltersOperation() : filter.MobileNumbers.MapToFiltersOperation(timeZone),
                RegionIds = filter.RegionIds == null ? new FiltersOperation() : filter.RegionIds.MapToFiltersOperation(timeZone),
                States = filter.States == null ? new FiltersOperation() : filter.States.MapToFiltersOperation(timeZone),
                CreationTimes = filter.CreationTimes == null ? new FiltersOperation() : filter.CreationTimes.MapToFiltersOperation(timeZone)
            };
        }
        public static FilterfnAffiliateCorrection MapToFilterAffiliateCorrection(this ApiFilterAffiliateCorrection filter, double timeZone)
        {
            return new FilterfnAffiliateCorrection
            {
                AffiliateId = filter.AffiliateId,
                FromDate = filter.FromDate.GetUTCDateFromGMT(timeZone),
                ToDate = filter.ToDate.GetUTCDateFromGMT(timeZone),
                FieldNameToOrderBy = filter.FieldNameToOrderBy,
                Ids = filter.Ids == null ? new FiltersOperation() : filter.Ids.MapToFiltersOperation(timeZone),
                PartnerIds = filter.PartnerIds == null ? new FiltersOperation() : filter.PartnerIds.MapToFiltersOperation(timeZone),
                AffiliateIds = filter.AffiliateIds == null ? new FiltersOperation() : filter.AffiliateIds.MapToFiltersOperation(timeZone),
                FirstNames = filter.FirstNames == null ? new FiltersOperation() : filter.FirstNames.MapToFiltersOperation(timeZone),
                LastNames = filter.LastNames == null ? new FiltersOperation() : filter.LastNames.MapToFiltersOperation(timeZone),
                Amounts = filter.Amounts == null ? new FiltersOperation() : filter.Amounts.MapToFiltersOperation(timeZone),
                CurrencyIds = filter.CurrencyIds == null ? new FiltersOperation() : filter.CurrencyIds.MapToFiltersOperation(timeZone),
                Creators = filter.Creators == null ? new FiltersOperation() : filter.Creators.MapToFiltersOperation(timeZone),
                CreationTimes = filter.CreationTimes == null ? new FiltersOperation() : filter.CreationTimes.MapToFiltersOperation(timeZone),
                LastUpdateTimes = filter.LastUpdateTimes == null ? new FiltersOperation() : filter.LastUpdateTimes.MapToFiltersOperation(timeZone),
                OperationTypeNames = filter.OperationTypeNames == null ? new FiltersOperation() : filter.OperationTypeNames.MapToFiltersOperation(timeZone),
                CreatorFirstNames = filter.CreatorFirstNames == null ? new FiltersOperation() : filter.CreatorFirstNames.MapToFiltersOperation(timeZone),
                CreatorLastNames = filter.CreatorLastNames == null ? new FiltersOperation() : filter.CreatorLastNames.MapToFiltersOperation(timeZone),
                DocumentTypeIds = filter.DocumentTypeIds == null ? new FiltersOperation() : filter.DocumentTypeIds.MapToFiltersOperation(timeZone),
                ClientIds = filter.ClientIds == null ? new FiltersOperation() : filter.ClientIds.MapToFiltersOperation(timeZone),
                ClientFirstNames = filter.ClientFirstNames == null ? new FiltersOperation() : filter.ClientFirstNames.MapToFiltersOperation(timeZone),
                ClientLastNames = filter.ClientLastNames == null ? new FiltersOperation() : filter.ClientLastNames.MapToFiltersOperation(timeZone),
                TakeCount = filter.TakeCount,
                SkipCount = filter.SkipCount,
                OrderBy = filter.OrderBy
            };
        }
        #endregion

        #region Filter Client Correction

        public static FilterCorrection MapToFilterCorrection(this ApiFilterClientCorrection filter, double timeZone)
        {
            return new FilterCorrection
            {
                ClientId = filter.ClientId,
                AccountId = filter.AccountId,
                TakeCount = filter.TakeCount,
                SkipCount = filter.SkipCount,
                FromDate = filter.FromDate.GetUTCDateFromGMT(timeZone),
                ToDate = filter.ToDate.GetUTCDateFromGMT(timeZone),
                OrderBy = filter.OrderBy,
                FieldNameToOrderBy = filter.FieldNameToOrderBy,
                Ids = filter.Ids == null ? new FiltersOperation() : filter.Ids.MapToFiltersOperation(timeZone),
                ClientIds = filter.ClientIds == null ? new FiltersOperation() : filter.ClientIds.MapToFiltersOperation(timeZone),
                ClientUserNames = filter.ClientUserNames == null ? new FiltersOperation() : filter.ClientUserNames.MapToFiltersOperation(timeZone),
                Amounts = filter.Amounts == null ? new FiltersOperation() : filter.Amounts.MapToFiltersOperation(timeZone),
                States = filter.States == null ? new FiltersOperation() : filter.States.MapToFiltersOperation(timeZone),
                CurrencyIds = filter.CurrencyIds == null ? new FiltersOperation() : filter.CurrencyIds.MapToFiltersOperation(timeZone),
                Creators = filter.UserIds == null ? new FiltersOperation() : filter.UserIds.MapToFiltersOperation(timeZone),
                OperationTypeNames = filter.OperationTypeNames == null ? new FiltersOperation() : filter.OperationTypeNames.MapToFiltersOperation(timeZone),
                OperationTypeIds = filter.OperationTypeIds == null ? new FiltersOperation() : filter.OperationTypeIds.MapToFiltersOperation(timeZone),
                FirstNames = filter.FirstNames == null ? new FiltersOperation() : filter.FirstNames.MapToFiltersOperation(timeZone),
                LastNames = filter.LastNames == null ? new FiltersOperation() : filter.LastNames.MapToFiltersOperation(timeZone),
                ProductNames = filter.ProductNames == null ? new FiltersOperation() : filter.ProductNames.MapToFiltersOperation(timeZone)
            };
        }


        #endregion

        #region FilterClientMessage

        public static FilterfnClientLog MapToFilterClientLog(this ApiFilterClientLog filter, double timeZone)
        {
            return new FilterfnClientLog
            {
                SkipCount = filter.SkipCount,
                TakeCount = filter.TakeCount,
                Ids = filter.Ids == null ? new FiltersOperation() : filter.Ids.MapToFiltersOperation(timeZone),
                ClientIds = filter.ClientIds == null ? new FiltersOperation() : filter.ClientIds.MapToFiltersOperation(timeZone),
                Actions = filter.Actions == null ? new FiltersOperation() : filter.Actions.MapToFiltersOperation(timeZone),
                UserIds = filter.UserIds == null ? new FiltersOperation() : filter.UserIds.MapToFiltersOperation(timeZone),
                Ips = filter.Ips == null ? new FiltersOperation() : filter.Ips.MapToFiltersOperation(timeZone),
                Pages = filter.Pages == null ? new FiltersOperation() : filter.Pages.MapToFiltersOperation(timeZone),
                SessionIds = filter.SessionIds == null ? new FiltersOperation() : filter.SessionIds.MapToFiltersOperation(timeZone),
                CreatedFrom = filter.CreatedFrom.GetUTCDateFromGMT(timeZone),
                CreatedBefore = filter.CreatedBefore.GetUTCDateFromGMT(timeZone),
                OrderBy = filter.OrderBy,
                FieldNameToOrderBy = filter.FieldNameToOrderBy
            };
        }

        public static MessageTemplateModel MapToMessageTemplateModel(this MessageTemplate messageTemplate)
        {
            return new MessageTemplateModel
            {
                Id = messageTemplate.Id,
                PartnerId = messageTemplate.PartnerId,
                NickName = messageTemplate.NickName,
                ClientInfoType = messageTemplate.ClientInfoType,
                ExternalTemplateId = messageTemplate.ExternalTemplateId,
                State = messageTemplate.State
            };
        }

        public static MessageTemplate MapToMessageTemplate(this MessageTemplateModel messageTemplateModel)
        {
            return new MessageTemplate
            {
                Id = messageTemplateModel.Id.HasValue ? messageTemplateModel.Id.Value : 0,
                PartnerId = messageTemplateModel.PartnerId,
                NickName = messageTemplateModel.NickName,
                ClientInfoType = messageTemplateModel.ClientInfoType,
                ExternalTemplateId = messageTemplateModel.ExternalTemplateId,
                State = messageTemplateModel.State
            };
        }

        #endregion

        #region FilterPartner

        public static FilterPartner MapToFilterPartner(this ApiFilterPartner filter, double timeZone)
        {
            return new FilterPartner
            {
                Id = filter.Id,
                Name = filter.Name,
                CurrencyId = filter.CurrencyId,
                State = filter.State,
                AdminSiteUrl = filter.AdminSiteUrl,
                CreatedFrom = filter.CreatedFrom.GetUTCDateFromGMT(timeZone),
                CreatedBefore = filter.CreatedBefore.GetUTCDateFromGMT(timeZone),
                SkipCount = filter.SkipCount,
                TakeCount = filter.TakeCount,
                OrderBy = filter.OrderBy,
                FieldNameToOrderBy = filter.FieldNameToOrderBy
            };
        }

        #endregion

        #region FiltersOperation

        public static FiltersOperation MapToFiltersOperation(this ApiFiltersOperation apiFiltersOperation, double timeZone)
        {
            if (apiFiltersOperation.ApiOperationTypeList.Count == 0)
            {
                apiFiltersOperation.ApiOperationTypeList.Add(new ApiFiltersOperationType { OperationTypeId = (int)FilterOperations.IsNull });
            }
            return new FiltersOperation
            {
                IsAnd = apiFiltersOperation.IsAnd,
                OperationTypeList = apiFiltersOperation.ApiOperationTypeList.MapToFiltersOperationTypes(timeZone)
            };
        }

        #endregion

        #region OperationTypes

        public static List<FiltersOperationType> MapToFiltersOperationTypes(this List<ApiFiltersOperationType> apiFiltersOperationTypes, double timeZone)
        {
            return apiFiltersOperationTypes.Select(x => x.MapToFiltersOperationType(timeZone)).ToList();
        }

        public static FiltersOperationType MapToFiltersOperationType(this ApiFiltersOperationType apiFiltersOperationType, double timeZone)
        {
            return new FiltersOperationType
            {
                OperationTypeId = apiFiltersOperationType.OperationTypeId,
                StringValue = apiFiltersOperationType.ArrayValue != null && apiFiltersOperationType.ArrayValue.Any() ?
                              string.Join(",", apiFiltersOperationType.ArrayValue) : apiFiltersOperationType.StringValue,
                IntValue = apiFiltersOperationType.IntValue,
                DecimalValue = apiFiltersOperationType.DecimalValue,
                DateTimeValue = apiFiltersOperationType.DateTimeValue.GetUTCDateFromGMT(timeZone)
            };
        }

        #endregion

        #region FilterPaymentRequest

        public static FilterfnPaymentRequest MapToFilterfnPaymentRequest(this ApiFilterfnPaymentRequest filter, double timeZone)
        {
            if (!string.IsNullOrEmpty(filter.FieldNameToOrderBy))
            {
                var orderBy = filter.FieldNameToOrderBy;
                switch (orderBy)
                {
                    case "ExternalId":
                        filter.FieldNameToOrderBy = "ExternalTransactionId";
                        break;
                    case "State":
                        filter.FieldNameToOrderBy = "Status";
                        break;
                    default:
                        break;
                }
            }
            var fDate = filter.FromDate.GetUTCDateFromGMT(timeZone);
            var tDate = filter.ToDate.GetUTCDateFromGMT(timeZone);
            return new FilterfnPaymentRequest
            {
                PartnerId = filter.PartnerId,
                FromDate = fDate == null ? 0 : (long)fDate.Value.Year * 100000000 + (long)fDate.Value.Month * 1000000 + (long)fDate.Value.Day * 10000 + (long)fDate.Value.Hour * 100 + fDate.Value.Minute,
                ToDate = tDate == null ? 0 : (long)tDate.Value.Year * 100000000 + (long)tDate.Value.Month * 1000000 + (long)tDate.Value.Day * 10000 + (long)tDate.Value.Hour * 100 + tDate.Value.Minute,
                Type = filter.Type,
                HasNote = filter.HasNote,
                AgentId = filter.AgentId,
                AccountIds = filter.AccountId == null ? null : new List<long> { filter.AccountId.Value },
                Ids = filter.Ids == null ? new FiltersOperation() : filter.Ids.MapToFiltersOperation(timeZone),
                UserNames = filter.UserNames == null ? new FiltersOperation() : filter.UserNames.MapToFiltersOperation(timeZone),
                Names = filter.Names == null ? new FiltersOperation() : filter.Names.MapToFiltersOperation(timeZone),
                FirstNames = filter.FirstNames == null ? new FiltersOperation() : filter.FirstNames.MapToFiltersOperation(timeZone),
                LastNames = filter.LastNames == null ? new FiltersOperation() : filter.LastNames.MapToFiltersOperation(timeZone),
                CreatorNames = filter.CreatorNames == null ? new FiltersOperation() : filter.CreatorNames.MapToFiltersOperation(timeZone),
                ClientIds = filter.ClientIds == null ? new FiltersOperation() : filter.ClientIds.MapToFiltersOperation(timeZone),
                ClientEmails = filter.Emails == null ? new FiltersOperation() : filter.Emails.MapToFiltersOperation(timeZone),
                UserIds = filter.UserIds == null ? new FiltersOperation() : filter.UserIds.MapToFiltersOperation(timeZone),
                PartnerPaymentSettingIds = filter.PartnerPaymentSettingIds == null ? new FiltersOperation() : filter.PartnerPaymentSettingIds.MapToFiltersOperation(timeZone),
                PaymentSystemIds = filter.PaymentSystemIds == null ? new FiltersOperation() : filter.PaymentSystemIds.MapToFiltersOperation(timeZone),
                Currencies = filter.CurrencyIds == null ? new FiltersOperation() : filter.CurrencyIds.MapToFiltersOperation(timeZone),
                States = filter.States == null ? new FiltersOperation() : filter.States.MapToFiltersOperation(timeZone),
                Types = filter.Types == null ? new FiltersOperation() : filter.Types.MapToFiltersOperation(timeZone),
                BetShopIds = filter.BetShopIds == null ? new FiltersOperation() : filter.BetShopIds.MapToFiltersOperation(timeZone),
                BetShopNames = filter.BetShopNames == null ? new FiltersOperation() : filter.BetShopNames.MapToFiltersOperation(timeZone),
                Amounts = filter.Amounts == null ? new FiltersOperation() : filter.Amounts.MapToFiltersOperation(timeZone),
                FinalAmounts = filter.FinalAmounts == null ? new FiltersOperation() : filter.FinalAmounts.MapToFiltersOperation(timeZone),
                CreationTimes = filter.CreationTimes == null ? new FiltersOperation() : filter.CreationTimes.MapToFiltersOperation(timeZone),
                AffiliatePlatformIds = filter.AffiliatePlatformIds == null ? new FiltersOperation() : filter.AffiliatePlatformIds.MapToFiltersOperation(timeZone),
                AffiliateIds = filter.AffiliateIds == null ? new FiltersOperation() : filter.AffiliateIds.MapToFiltersOperation(timeZone),
                ActivatedBonusTypes = filter.ActivatedBonusTypes == null ? new FiltersOperation() : filter.ActivatedBonusTypes.MapToFiltersOperation(timeZone),
                CommissionAmounts = filter.CommissionAmounts == null ? new FiltersOperation() : filter.CommissionAmounts.MapToFiltersOperation(timeZone),
                CardNumbers = filter.CardNumbers == null ? new FiltersOperation() : filter.CardNumbers.MapToFiltersOperation(timeZone),
                CountryCodes = filter.CountryCodes == null ? new FiltersOperation() : filter.CountryCodes.MapToFiltersOperation(timeZone),
                SegmentNames = filter.SegmentNames == null ? new FiltersOperation() : filter.SegmentNames.MapToFiltersOperation(timeZone),
                SegmentIds = filter.SegmentIds == null ? new FiltersOperation() : filter.SegmentIds.MapToFiltersOperation(timeZone),
                LastUpdateTimes = filter.LastUpdateTimes == null ? new FiltersOperation() : filter.LastUpdateTimes.MapToFiltersOperation(timeZone),
                ExternalTransactionIds = filter.ExternalIds == null ? new FiltersOperation() : filter.ExternalIds.MapToFiltersOperation(timeZone),
                TakeCount = filter.TakeCount,
                SkipCount = filter.SkipCount,
                OrderBy = filter.OrderBy,
                FieldNameToOrderBy = filter.FieldNameToOrderBy
            };
        }

        #endregion

        #region FilterPaymentRequest

        public static FilterRole MapToFilterFilterRole(this ApiFilterRole filterRole)
        {
            return new FilterRole
            {
                Id = filterRole.Id,
                Name = filterRole.Name,
                PermissionIds = filterRole.PermissionIds,
                SkipCount = filterRole.SkipCount,
                TakeCount = filterRole.TakeCount
            };
        }

        #endregion

        #region FilterProducts

        public static FilterProduct MapToFilterProduct(this ApiFilterProduct product)
        {
            return new FilterProduct
            {
                Id = product.Id,
                Description = product.Description,
                ExternalId = product.ExternalId,
                GameProviderId = product.GameProviderId,
                ParentId = product.ParentId,
                PaymentSystemId = product.PaymentSystemId,
                SkipCount = product.SkipCount,
                TakeCount = product.TakeCount
            };
        }

        public static List<FilterProduct> MapToFilterProducts(this IEnumerable<ApiFilterProduct> products)
        {
            return products.Select(MapToFilterProduct).ToList();
        }
        #endregion

        #region FilterfnProduct

        public static FilterfnProduct MapToFilterfnProduct(this ApiFilterfnProduct filter)
        {
            if (!string.IsNullOrEmpty(filter.FieldNameToOrderBy))
            {
                var orderBy = filter.FieldNameToOrderBy;
                switch (orderBy)
                {
                    case "Description":
                        filter.FieldNameToOrderBy = "NickName";
                        break;
                    case "GameProviderId":
                        filter.FieldNameToOrderBy = "ProductGameProviderId";
                        break;
                    default:
                        break;
                }
            }
            var filterfnProduct = new FilterfnProduct
            {
                ParentId = filter.ParentId,
                ProductId = filter.ProductId,
                Pattern = filter.Pattern,
                IsProviderActive = filter.IsProviderActive,
                IsForMobile = filter.IsForMobile,
                IsForDesktop = filter.IsForDesktop,
                Ids = filter.Ids == null ? new FiltersOperation() : filter.Ids.MapToFiltersOperation(filter.TimeZone),
                Names = filter.Names == null ? new FiltersOperation() : filter.Names.MapToFiltersOperation(filter.TimeZone),
                Descriptions = filter.Descriptions == null ? new FiltersOperation() : filter.Descriptions.MapToFiltersOperation(filter.TimeZone),
                ExternalIds = filter.ExternalIds == null ? new FiltersOperation() : filter.ExternalIds.MapToFiltersOperation(filter.TimeZone),
                States = filter.States == null ? new FiltersOperation() : filter.States.MapToFiltersOperation(filter.TimeZone),
                GameProviderIds = filter.GameProviderIds == null ? new FiltersOperation() : filter.GameProviderIds.MapToFiltersOperation(filter.TimeZone),
                SubProviderIds = filter.SubProviderIds == null ? new FiltersOperation() : filter.SubProviderIds.MapToFiltersOperation(filter.TimeZone),
                FreeSpinSupports = filter.FreeSpinSupports == null ? new FiltersOperation() : filter.FreeSpinSupports.MapToFiltersOperation(filter.TimeZone),
                Jackpots = filter.Jackpots == null ? new FiltersOperation() : filter.Jackpots.MapToFiltersOperation(filter.TimeZone),
                RTPs = filter.RTPs == null ? new FiltersOperation() : filter.RTPs.MapToFiltersOperation(filter.TimeZone),
                SkipCount = filter.SkipCount,
                TakeCount = filter.TakeCount,
                OrderBy = filter.OrderBy,
                FieldNameToOrderBy = filter.FieldNameToOrderBy
            };
            return filterfnProduct;
        }

        public static FilterGameProvider MapToFilterGameProvider(this ApiFilterGameProvider filter)
        {
            return new FilterGameProvider
            {
                Id = filter.Id,
                ParentId = filter.ParentId,
                PartnerId = filter.PartnerId,
                SettingPartnerId = filter.SettingPartnerId,
                Name = filter.Name,
                IsActive = filter.IsActive
            };
        }

        #endregion

        #region FilterBetShopReconing

        public static FilterfnBetShopReconing MapToFilterBetShopReconing(this ApiFilterBetShopReconing filter)
        {
            return new FilterfnBetShopReconing
            {
                SkipCount = filter.SkipCount,
                TakeCount = filter.TakeCount,
                Ids = filter.Ids == null ? new List<FiltersOperationType>() : filter.Ids.MapToFiltersOperationTypes(filter.TimeZone),
                UserIds = filter.UserIds == null ? new List<FiltersOperationType>() : filter.UserIds.MapToFiltersOperationTypes(filter.TimeZone),
                Currencies = filter.Currencies == null ? new List<FiltersOperationType>() : filter.Currencies.MapToFiltersOperationTypes(filter.TimeZone),
                BetShopIds = filter.BetShopIds == null ? new List<FiltersOperationType>() : filter.BetShopIds.MapToFiltersOperationTypes(filter.TimeZone),
                BetShopNames = filter.BetShopNames == null ? new List<FiltersOperationType>() : filter.BetShopNames.MapToFiltersOperationTypes(filter.TimeZone),
                BetShopAvailiableBalances = filter.BetShopAvailiableBalances == null ? 
                    new List<FiltersOperationType>() : filter.BetShopAvailiableBalances.MapToFiltersOperationTypes(filter.TimeZone),
                Amounts = filter.Amounts == null ? new List<FiltersOperationType>() : filter.Amounts.MapToFiltersOperationTypes(filter.TimeZone),
                CreationTimes = filter.CreationTimes == null ? new List<FiltersOperationType>() : filter.CreationTimes.MapToFiltersOperationTypes(filter.TimeZone)
            };
        }

        public static List<FilterfnBetShopReconing> MapToFilterBetShopReconings(this IEnumerable<ApiFilterBetShopReconing> filters)
        {
            return filters.Select(MapToFilterBetShopReconing).ToList();
        }
        #endregion

        #region FilterCashDeskTransaction

        public static FilterCashDeskTransaction MapToFilterCashDeskTransaction(this ApiFilterCashDeskTransaction filter, double timeZone)
        {
            return new FilterCashDeskTransaction
            {
                FromDate = filter.CreatedFrom.GetUTCDateFromGMT(timeZone),
                ToDate = filter.CreatedBefore.GetUTCDateFromGMT(timeZone),
                Ids = filter.Ids == null ? new FiltersOperation() : filter.Ids.MapToFiltersOperation(timeZone),
                BetShopNames = filter.BetShopNames == null ? new FiltersOperation() : filter.BetShopNames.MapToFiltersOperation(timeZone),
                CashDeskIds = filter.CashDeskIds == null ? new FiltersOperation() : filter.CashDeskIds.MapToFiltersOperation(timeZone),
                CashierIds = filter.CashierIds == null ? new FiltersOperation() : filter.CashierIds.MapToFiltersOperation(timeZone),
                BetShopIds = filter.BetShopIds == null ? new FiltersOperation() : filter.BetShopIds.MapToFiltersOperation(timeZone),
                OperationTypeNames = filter.OperationTypeNames == null ? new FiltersOperation() : filter.OperationTypeNames.MapToFiltersOperation(timeZone),
                Amounts = filter.Amounts == null ? new FiltersOperation() : filter.Amounts.MapToFiltersOperation(timeZone),
                Currencies = filter.Currencies == null ? new FiltersOperation() : filter.Currencies.MapToFiltersOperation(timeZone),
                CreationTimes = filter.CreationTimes == null ? new FiltersOperation() : filter.CreationTimes.MapToFiltersOperation(timeZone),
                TakeCount = filter.TakeCount,
                SkipCount = filter.SkipCount
            };
        }
        #endregion

        #region FilterfnPartnerPaymentSetting

        public static FilterfnPartnerPaymentSetting MapToFilterfnPartnerPaymentSetting(this ApiFilterfnPartnerPaymentSetting filter, double timeZone)
        {
            return new FilterfnPartnerPaymentSetting
            {
                Id = filter.Id,
                Status = filter.Status,
                Type = filter.Type,
                PaymentSystemId = filter.PaymentSystemId,
                PartnerId = filter.PartnerId,
                CurrencyId = filter.CurrencyId,
                CreatedFrom = filter.CreatedFrom.GetUTCDateFromGMT(timeZone),
                CreatedBefore = filter.CreatedBefore.GetUTCDateFromGMT(timeZone)
            };
        }
        #endregion

        #region FilterDashboard

        public static FilterDashboard MapToFilterDashboard(this ApiFilterDashboard filter, double timeZone)
        {
            var fromDay = filter.FromDate ?? DateTime.UtcNow;//No need to consider the timezone
            var toDay = filter.ToDate ?? DateTime.UtcNow;
            return new FilterDashboard
            {
                PartnerId = filter.PartnerId,
                FromDate = fromDay,
                ToDate = toDay,
                FromDay = (long)fromDay.Year * 10000 + (long)fromDay.Month * 100 + (long)fromDay.Day,
                ToDay = (long)toDay.Year * 10000 + (long)toDay.Month * 100 + (long)toDay.Day
            };
        }

        public static FilterRealTime MapToFilterRealTime(this ApiFilterRealTime filter, double timeZone)
        {
            return new FilterRealTime
            {
                PartnerId = filter.PartnerId,
                ClientIds = filter.ClientIds == null ? new FiltersOperation() : filter.ClientIds.MapToFiltersOperation(timeZone),
                LanguageIds = filter.LanguageIds == null ? new FiltersOperation() : filter.LanguageIds.MapToFiltersOperation(timeZone),
                Names = filter.Names == null ? new FiltersOperation() : filter.Names.MapToFiltersOperation(timeZone),
                UserNames = filter.UserNames == null ? new FiltersOperation() : filter.UserNames.MapToFiltersOperation(timeZone),
                Categories = filter.Categories == null ? new FiltersOperation() : filter.Categories.MapToFiltersOperation(timeZone),
                RegionIds = filter.RegionIds == null ? new FiltersOperation() : filter.RegionIds.MapToFiltersOperation(timeZone),
                Currencies = filter.Currencies == null ? new FiltersOperation() : filter.Currencies.MapToFiltersOperation(timeZone),
                LoginIps = filter.LoginIps == null ? new FiltersOperation() : filter.LoginIps.MapToFiltersOperation(timeZone),
                Balances = filter.Balances == null ? new FiltersOperation() : filter.Balances.MapToFiltersOperation(timeZone),
                TotalDepositsCounts = filter.TotalDepositsCounts == null ? new FiltersOperation() : filter.TotalDepositsCounts.MapToFiltersOperation(timeZone),
                TotalDepositsAmounts = filter.TotalDepositsAmounts == null ? new FiltersOperation() : filter.TotalDepositsAmounts.MapToFiltersOperation(timeZone),
                TotalWithdrawalsCounts = filter.TotalWithdrawalsCounts == null ? new FiltersOperation() : filter.TotalWithdrawalsCounts.MapToFiltersOperation(timeZone),
                TotalWithdrawalsAmounts = filter.TotalWithdrawalsAmounts == null ? new FiltersOperation() : filter.TotalWithdrawalsAmounts.MapToFiltersOperation(timeZone),
                TotalBetsCounts = filter.TotalBetsCounts == null ? new FiltersOperation() : filter.TotalBetsCounts.MapToFiltersOperation(timeZone),
                GGRs = filter.GGRs == null ? new FiltersOperation() : filter.GGRs.MapToFiltersOperation(timeZone),
                TakeCount = filter.TakeCount,
                SkipCount = filter.SkipCount,
                OrderBy = filter.OrderBy,
                FieldNameToOrderBy = filter.FieldNameToOrderBy
            };
        }

        #endregion

        #region fnClientDashboard

        public static FilterfnClientDashboard MapToFilterfnClientDashboard(this ApiFilterfnClientDashboard filter, double timeZone)
        {
            return new FilterfnClientDashboard
            {
                PartnerId = filter.PartnerId,
                FromDate = filter.FromDate,//No need to consider the timezone
                ToDate = filter.ToDate,
                ClientIds = filter.ClientIds == null ? new FiltersOperation() : filter.ClientIds.MapToFiltersOperation(timeZone),
                UserNames = filter.UserNames == null ? new FiltersOperation() : filter.UserNames.MapToFiltersOperation(timeZone),
                PartnerIds = filter.PartnerIds == null ? new FiltersOperation() : filter.PartnerIds.MapToFiltersOperation(timeZone),
                CurrencyIds = filter.CurrencyIds == null ? new FiltersOperation() : filter.CurrencyIds.MapToFiltersOperation(timeZone),
                FirstNames = filter.FirstNames == null ? new FiltersOperation() : filter.FirstNames.MapToFiltersOperation(timeZone),
                LastNames = filter.LastNames == null ? new FiltersOperation() : filter.LastNames.MapToFiltersOperation(timeZone),
                Emails = filter.Emails == null ? new FiltersOperation() : filter.Emails.MapToFiltersOperation(timeZone),
                AffiliatePlatformIds = filter.AffiliatePlatformIds == null ? new FiltersOperation() : filter.AffiliatePlatformIds.MapToFiltersOperation(timeZone),
                AffiliateIds = filter.AffiliateIds == null ? new FiltersOperation() : filter.AffiliateIds.MapToFiltersOperation(timeZone),
                AffiliateReferralIds = filter.AffiliateReferralIds == null ? new FiltersOperation() : filter.AffiliateReferralIds.MapToFiltersOperation(timeZone),
                TotalWithdrawalAmounts = filter.TotalWithdrawalAmounts == null ? new FiltersOperation() : filter.TotalWithdrawalAmounts.MapToFiltersOperation(timeZone),
                WithdrawalsCounts = filter.WithdrawalsCounts == null ? new FiltersOperation() : filter.WithdrawalsCounts.MapToFiltersOperation(timeZone),
                TotalDepositAmounts = filter.TotalDepositAmounts == null ? new FiltersOperation() : filter.TotalDepositAmounts.MapToFiltersOperation(timeZone),
                DepositsCounts = filter.DepositsCounts == null ? new FiltersOperation() : filter.DepositsCounts.MapToFiltersOperation(timeZone),
                TotalBetAmounts = filter.TotalBetAmounts == null ? new FiltersOperation() : filter.TotalBetAmounts.MapToFiltersOperation(timeZone),
                TotalBetsCounts = filter.TotalBetsCounts == null ? new FiltersOperation() : filter.TotalBetsCounts.MapToFiltersOperation(timeZone),
                SportBetsCounts = filter.SportBetsCounts == null ? new FiltersOperation() : filter.SportBetsCounts.MapToFiltersOperation(timeZone),
                TotalWinAmounts = filter.TotalWinAmounts == null ? new FiltersOperation() : filter.TotalWinAmounts.MapToFiltersOperation(timeZone),
                WinsCounts = filter.WinsCounts == null ? new FiltersOperation() : filter.WinsCounts.MapToFiltersOperation(timeZone),
                GGRs = filter.GGRs == null ? new FiltersOperation() : filter.GGRs.MapToFiltersOperation(timeZone),
                NGRs = filter.NGRs == null ? new FiltersOperation() : filter.NGRs.MapToFiltersOperation(timeZone),
                TotalDebitCorrections = filter.TotalDebitCorrections == null ? new FiltersOperation() : filter.TotalDebitCorrections.MapToFiltersOperation(timeZone),
                DebitCorrectionsCounts = filter.DebitCorrectionsCounts == null ? new FiltersOperation() : filter.DebitCorrectionsCounts.MapToFiltersOperation(timeZone),
                TotalCreditCorrections = filter.TotalCreditCorrections == null ? new FiltersOperation() : filter.TotalCreditCorrections.MapToFiltersOperation(timeZone),
                CreditCorrectionsCounts = filter.CreditCorrectionsCounts == null ? new FiltersOperation() : filter.CreditCorrectionsCounts.MapToFiltersOperation(timeZone),
                ComplementaryBalances = filter.ComplementaryBalances == null ? new FiltersOperation() : filter.ComplementaryBalances.MapToFiltersOperation(timeZone),
                SkipCount = filter.SkipCount,
                TakeCount = filter.TakeCount,
                OrderBy = filter.OrderBy,
                FieldNameToOrderBy = filter.FieldNameToOrderBy
            };
        }

        #endregion

        #region FilterNote

        public static FilterNote MapToFilterNote(this ApiFilterNote note)
        {
            return new FilterNote
            {
                Id = note.Id,
                State = note.State,
                Message = note.Message,
                ObjectId = note.ObjectId,
                ObjectTypeId = note.ObjectTypeId,
                Type = note.Type,
                FromDate = note.FromDate,
                ToDate = note.ToDate
            };
        }

        public static List<FilterNote> MapToNoteModels(this IEnumerable<ApiFilterNote> models)
        {
            return models.Select(MapToFilterNote).ToList();
        }
        #endregion

        #region Shift Reports

        public static FilterAdminShift MapToFilterfnAdminShiftReport(this ApiFilterShiftReport filter, double timeZone)
        {
            return new FilterAdminShift
            {
                FromDate = filter.FromDate.GetUTCDateFromGMT(timeZone),
                ToDate = filter.ToDate.GetUTCDateFromGMT(timeZone),
                Ids = filter.Ids == null ? new FiltersOperation() : filter.Ids.MapToFiltersOperation(timeZone),
                BetShopIds = filter.BetShopIds == null ? new FiltersOperation() : filter.BetShopIds.MapToFiltersOperation(timeZone),
                BetShopGroupIds = filter.BetShopGroupIds == null ? new FiltersOperation() : filter.BetShopGroupIds.MapToFiltersOperation(timeZone),
                BetShopNames = filter.BetShopNames == null ? new FiltersOperation() : filter.BetShopNames.MapToFiltersOperation(timeZone),
                BetShopGroupNames = filter.BetShopGroupNames == null ? new FiltersOperation() : filter.BetShopGroupNames.MapToFiltersOperation(timeZone),
                CashierIds = filter.CashierIds == null ? new FiltersOperation() : filter.CashierIds.MapToFiltersOperation(timeZone),
                CashdeskIds = filter.CashdeskIds == null ? new FiltersOperation() : filter.CashdeskIds.MapToFiltersOperation(timeZone),
                FirstNames = filter.FirstNames == null ? new FiltersOperation() : filter.FirstNames.MapToFiltersOperation(timeZone),
                EndAmounts = filter.EndAmounts == null ? new FiltersOperation() : filter.EndAmounts.MapToFiltersOperation(timeZone),
                BetAmounts = filter.BetAmounts == null ? new FiltersOperation() : filter.BetAmounts.MapToFiltersOperation(timeZone),
                PayedWinAmounts = filter.PayedWinAmounts == null ? new FiltersOperation() : filter.PayedWinAmounts.MapToFiltersOperation(timeZone),
                DepositAmounts = filter.DepositAmounts == null ? new FiltersOperation() : filter.DepositAmounts.MapToFiltersOperation(timeZone),
                WithdrawAmounts = filter.WithdrawAmounts == null ? new FiltersOperation() : filter.WithdrawAmounts.MapToFiltersOperation(timeZone),
                DebitCorrectionAmounts = filter.DebitCorrectionAmounts == null ? new FiltersOperation() : filter.DebitCorrectionAmounts.MapToFiltersOperation(timeZone),
                CreditCorrectionAmounts = filter.CreditCorrectionAmounts == null ? new FiltersOperation() : filter.CreditCorrectionAmounts.MapToFiltersOperation(timeZone),
                StartDates = filter.StartDates == null ? new FiltersOperation() : filter.StartDates.MapToFiltersOperation(timeZone),
                EndDates = filter.EndDates == null ? new FiltersOperation() : filter.EndDates.MapToFiltersOperation(timeZone),
                ShiftNumbers = filter.ShiftNumbers == null ? new FiltersOperation() : filter.ShiftNumbers.MapToFiltersOperation(timeZone),
                PartnerIds = filter.PartnerIds == null ? new FiltersOperation() : filter.PartnerIds.MapToFiltersOperation(timeZone),
                BonusAmounts = filter.BonusAmounts == null ? new FiltersOperation() : filter.BonusAmounts.MapToFiltersOperation(timeZone),

                SkipCount = filter.SkipCount,
                TakeCount = filter.TakeCount,

                OrderBy = filter.OrderBy,
                FieldNameToOrderBy = filter.FieldNameToOrderBy
            };
        }

        public static ApiShiftReportModel MapToApiShiftReportModel(this AdminShiftReportOutput report, double timeZone)
        {
            return new ApiShiftReportModel
            {
                Count = report.Count,
                TotalAmount = report.TotalAmount,
                TotalBonusAmount = report.TotalBonusAmount,
                TotalBetAmount = report.TotalBetAmount,
                TotalPayedWinAmount = report.TotalPayedWinAmount,
                TotalDepositAmount = report.TotalDepositAmount,
                TotalWithdrawAmount = report.TotalWithdrawAmount,
                TotalDebitCorrectionAmount = report.TotalDebitCorrectionAmount,
                TotalCreditCorrectionAmount = report.TotalCreditCorrectionAmount,
                Entities = report.Entities.Select(x => x.MapToApiShiftReportElement(timeZone)).ToList()
            };
        }

        public static ApiShiftReportElement MapToApiShiftReportElement(this fnAdminShiftReport element, double timeZone)
        {
            return new ApiShiftReportElement
            {
                Id = element.ShiftId,
                BetShopId = element.BetShopId,
                BetShopGroupId = element.BetShopGroupId,
                BetShopName = element.BetShopName,
                BetShopGroupName = element.BetShopGroupName,
                CashdeskId = element.CashdeskId,
                CashdeskName = element.CashdeskName,
                CashierId = element.CashierId,
                FirstName = element.FirstName,
                LastName = element.LastName,
                BetAmount = element.BetAmount,
                PayedWinAmount = element.PayedWinAmount,
                DepositAmount = element.DepositAmount,
                WithdrawAmount = element.WithdrawAmount,
                DebitCorrectionAmount = element.DebitCorrectionAmount,
                CreditCorrectionAmount = element.CreditCorrectionAmount,
                EndAmount = element.EndAmount ?? 0,
                StartDate = element.StartDate.GetGMTDateFromUTC(timeZone),
                EndDate = element.EndDate.GetGMTDateFromUTC(timeZone),
                ShiftNumber = element.ShiftNumber ?? 0,
                PartnerName = element.PartnerName,
                BonusAmount = element.BonusAmount
            };
        }

        #endregion

        #region Content
        public static FilterfnBanner MaptToFilterfnBanner(this ApiFilterBanner filter)
        {
            return new FilterfnBanner
            {
                SkipCount = filter.SkipCount,
                TakeCount = filter.TakeCount,
                PartnerId = filter.PartnerId,
                IsEnabled = filter.IsEnabled,
                ShowDescription = filter.ShowDescription,
                Visibility = filter.Visibility,
                Ids = filter.Ids == null ? new FiltersOperation() : filter.Ids.MapToFiltersOperation(filter.TimeZone),
                PartnerIds = filter.PartnerIds == null ? new FiltersOperation() : filter.PartnerIds.MapToFiltersOperation(filter.TimeZone),
                Orders = filter.Orders == null ? new FiltersOperation() : filter.Orders.MapToFiltersOperation(filter.TimeZone),
                NickNames = filter.NickNames == null ? new FiltersOperation() : filter.NickNames.MapToFiltersOperation(filter.TimeZone),
                Images = filter.Images == null ? new FiltersOperation() : filter.Images.MapToFiltersOperation(filter.TimeZone),
                Types = filter.Types == null ? new FiltersOperation() : filter.Types.MapToFiltersOperation(filter.TimeZone),
                FragmentNames = filter.FragmentNames == null ? new FiltersOperation() : filter.FragmentNames.MapToFiltersOperation(filter.TimeZone),
                Heads = filter.Heads == null ? new FiltersOperation() : filter.Heads.MapToFiltersOperation(filter.TimeZone),
                Bodies = filter.Bodies == null ? new FiltersOperation() : filter.Bodies.MapToFiltersOperation(filter.TimeZone),
                StartDates = filter.StartDates == null ? new FiltersOperation() : filter.StartDates.MapToFiltersOperation(filter.TimeZone),
                EndDates = filter.EndDates == null ? new FiltersOperation() : filter.EndDates.MapToFiltersOperation(filter.TimeZone),
                OrderBy = filter.OrderBy,
                FieldNameToOrderBy = filter.FieldNameToOrderBy
            };
        }

        public static FilterPopup MaptToFilterPopup(this ApiFilterPopup filter)
        {
            return new FilterPopup
            {
                SkipCount = filter.SkipCount,
                TakeCount = filter.TakeCount,
                PartnerId = filter.PartnerId,
                Ids = filter.Ids == null ? new FiltersOperation() : filter.Ids.MapToFiltersOperation(filter.TimeZone),
                PartnerIds = filter.PartnerIds == null ? new FiltersOperation() : filter.PartnerIds.MapToFiltersOperation(filter.TimeZone),
                NickNames = filter.NickNames == null ? new FiltersOperation() : filter.NickNames.MapToFiltersOperation(filter.TimeZone),
                States = filter.States == null ? new FiltersOperation() : filter.States.MapToFiltersOperation(filter.TimeZone),
                Types = filter.Types == null ? new FiltersOperation() : filter.Types.MapToFiltersOperation(filter.TimeZone),
                Orders = filter.Orders == null ? new FiltersOperation() : filter.Orders.MapToFiltersOperation(filter.TimeZone),
                Pages = filter.Pages == null ? new FiltersOperation() : filter.Pages.MapToFiltersOperation(filter.TimeZone),
                DeviceTypes = filter.DeviceTypes == null ? new FiltersOperation() : filter.DeviceTypes.MapToFiltersOperation(filter.TimeZone),
                StartDates = filter.StartDates == null ? new FiltersOperation() : filter.StartDates.MapToFiltersOperation(filter.TimeZone),
                FinishDates = filter.FinishDates == null ? new FiltersOperation() : filter.FinishDates.MapToFiltersOperation(filter.TimeZone),
                CreationTimes = filter.CreationTimes == null ? new FiltersOperation() : filter.CreationTimes.MapToFiltersOperation(filter.TimeZone),
                LastUpdateTimes = filter.LastUpdateTimes == null ? new FiltersOperation() : filter.LastUpdateTimes.MapToFiltersOperation(filter.TimeZone),
                OrderBy = filter.OrderBy,
                FieldNameToOrderBy = filter.FieldNameToOrderBy
            };
        }

        #endregion

        #region Languages

        public static List<PartnerLanguageSettingModel> MapToPartnerLanguageSettingModels(this IEnumerable<PartnerLanguageSetting> partnerLanguages, double timeZone)
        {
            return partnerLanguages.Select(x => x.MapToPartnerLanguageSettingModel(timeZone)).ToList();
        }

        public static PartnerLanguageSettingModel MapToPartnerLanguageSettingModel(this PartnerLanguageSetting partnerLanguage, double timeZone)
        {
            return new PartnerLanguageSettingModel
            {
                Id = partnerLanguage.Id,
                PartnerId = partnerLanguage.PartnerId,
                LanguageId = partnerLanguage.LanguageId,
                State = partnerLanguage.State,
                Order = partnerLanguage.Order,
                CreationTime = partnerLanguage.CreationTime.GetGMTDateFromUTC(timeZone),
                LastUpdateTime = partnerLanguage.LastUpdateTime.GetGMTDateFromUTC(timeZone)
            };
        }

        #endregion

        public static FilterAnnouncement MapToFilterAnnouncement(this ApiFilterAnnouncement filter, double timeZone)
        {
            return new FilterAnnouncement
            {
                PartnerId = filter.PartnerId,
                Type = filter.Type,
                TakeCount = filter.TakeCount,
                SkipCount = filter.SkipCount,
                FromDate = filter.FromDate.GetUTCDateFromGMT(timeZone),
                ToDate = filter.ToDate.GetUTCDateFromGMT(timeZone),
                OrderBy = filter.OrderBy,
                FieldNameToOrderBy = filter.FieldNameToOrderBy,
                Ids = filter.Ids == null ? new FiltersOperation() : filter.Ids.MapToFiltersOperation(timeZone),
                PartnerIds = filter.PartnerIds == null ? new FiltersOperation() : filter.PartnerIds.MapToFiltersOperation(timeZone),
                UserIds = filter.UserIds == null ? new FiltersOperation() : filter.UserIds.MapToFiltersOperation(timeZone),
                Types = filter.Types == null ? new FiltersOperation() : filter.Types.MapToFiltersOperation(timeZone),
                ReceiverTypes = filter.ReceiverTypes == null ? new FiltersOperation() : filter.ReceiverTypes.MapToFiltersOperation(timeZone),
                States = filter.States == null ? new FiltersOperation() : filter.States.MapToFiltersOperation(timeZone)
            };
        }

        public static ApiReportByPaymentSystem MapToFilterReportByPaymentSystem(this fnReportByPaymentSystem input)
        {
            return new ApiReportByPaymentSystem
            {
                PartnerId = input.PartnerId,
                PartnerName = input.PartnerName,
                PaymentSystemId = input.PaymentSystemId,
                PaymentSystemName = input.PaymentSystemName,
                Status = input.Status,
                Count = input.Count ?? 0,
                TotalAmount = input.TotalAmount ?? 0
            };
        }

        public static ApiReportByPartner MapToFilterReportByPartner(this fnReportByPartner input)
        {
            return new ApiReportByPartner
            {
                PartnerId = input.PartnerId,
                PartnerName = input.PartnerName,
                TotalBetAmount = input.TotalBetAmount ?? 0,
                TotalBetsCount = input.TotalBetsCount ?? 0,
                TotalWinAmount = input.TotalWinAmount ?? 0,
                TotalGGR = input.TotalGGR ?? 0
            };
        }

        public static FilterTicket MapToFilterTicket(this ApiFilterTicket filter, double timeZone)
        {
            return new FilterTicket
            {
                PartnerId = filter.PartnerId,
                Ids = filter.Ids == null ? new FiltersOperation() : filter.Ids.MapToFiltersOperation(timeZone),
                ClientIds = filter.ClientIds == null ? new FiltersOperation() : filter.ClientIds.MapToFiltersOperation(timeZone),
                PartnerIds = filter.PartnerIds == null ? new FiltersOperation() : filter.PartnerIds.MapToFiltersOperation(timeZone),
                PartnerNames = filter.PartnerNames == null ? new FiltersOperation() : filter.PartnerNames.MapToFiltersOperation(timeZone),
                Subjects = filter.Subjects == null ? new FiltersOperation() : filter.Subjects.MapToFiltersOperation(timeZone),
                UserNames = filter.UserNames == null ? new FiltersOperation() : filter.UserNames.MapToFiltersOperation(timeZone),
                UserIds = filter.UserIds == null ? new FiltersOperation() : filter.UserIds.MapToFiltersOperation(timeZone),
                UserFirstNames = filter.UserFirstNames == null ? new FiltersOperation() : filter.UserFirstNames.MapToFiltersOperation(timeZone),
                UserLastNames = filter.UserLastNames == null ? new FiltersOperation() : filter.UserLastNames.MapToFiltersOperation(timeZone),
                Statuses = filter.Statuses == null ? new FiltersOperation() : filter.Statuses.MapToFiltersOperation(timeZone),
                Types = filter.Types == null ? new FiltersOperation() : filter.Types.MapToFiltersOperation(timeZone),
                State = filter.State,
                UnreadsOnly = filter.UnreadsOnly,
                CreatedBefore = filter.CreatedBefore.GetUTCDateFromGMT(timeZone),
                CreatedFrom = filter.CreatedFrom.GetUTCDateFromGMT(timeZone),
                TakeCount = filter.TakeCount,
                SkipCount = filter.SkipCount,
                OrderBy = filter.OrderBy,
                FieldNameToOrderBy = filter.FieldNameToOrderBy
            };
        }

        public static FilterClientMessage MapToFilterClientMessage(this ApiFilterObjectMessage filter, double timeZone)
        {
            return new FilterClientMessage
            {
                FromDate = filter.FromDate.GetUTCDateFromGMT(timeZone),
                ToDate = filter.ToDate.GetUTCDateFromGMT(timeZone),
                PartnerIds = filter.PartnerIds == null ? new FiltersOperation() : filter.PartnerIds.MapToFiltersOperation(timeZone),
                MessageIds = filter.MessageIds == null ? new FiltersOperation() : filter.MessageIds.MapToFiltersOperation(timeZone),
                Ids = filter.Ids == null ? new FiltersOperation() : filter.Ids.MapToFiltersOperation(timeZone),
                UserNames = filter.UserNames == null ? new FiltersOperation() : filter.UserNames.MapToFiltersOperation(timeZone),
                MobileOrEmails = filter.MobileOrEmails == null ? new FiltersOperation() : filter.MobileOrEmails.MapToFiltersOperation(timeZone),
                Subjects = filter.Subjects == null ? new FiltersOperation() : filter.Subjects.MapToFiltersOperation(timeZone),
                Messages = filter.Messages == null ? new FiltersOperation() : filter.Messages.MapToFiltersOperation(timeZone),
                MessageTypes = filter.MessageTypes == null ? new FiltersOperation() : filter.MessageTypes.MapToFiltersOperation(timeZone),
                Statuses = filter.Statuses == null ? new FiltersOperation() : filter.Statuses.MapToFiltersOperation(timeZone),
                TakeCount = filter.TakeCount,
                SkipCount = filter.SkipCount,
                OrderBy = filter.OrderBy,
                FieldNameToOrderBy = filter.FieldNameToOrderBy
            };
        }

        public static FilterEmail MapToFilterEmail(this ApiFilterEmail filter, double timeZone)
        {
            return new FilterEmail
            {
                Ids = filter.Ids == null ? new FiltersOperation() : filter.Ids.MapToFiltersOperation(timeZone),
                PartnerIds = filter.PartnerIds == null ? new FiltersOperation() : filter.PartnerIds.MapToFiltersOperation(timeZone),
                Subjects = filter.Subjects == null ? new FiltersOperation() : filter.Subjects.MapToFiltersOperation(timeZone),
                Statuses = filter.Statuses == null ? new FiltersOperation() : filter.Statuses.MapToFiltersOperation(timeZone),
                Receiver = filter.Receiver == null ? new FiltersOperation() : filter.Receiver.MapToFiltersOperation(timeZone),
                CreatedBefore = filter.CreatedBefore.GetUTCDateFromGMT(timeZone),
                CreatedFrom = filter.CreatedFrom.GetUTCDateFromGMT(timeZone),
                TakeCount = filter.TakeCount,
                SkipCount = filter.SkipCount,
                OrderBy = filter.OrderBy,
                FieldNameToOrderBy = filter.FieldNameToOrderBy,
                ObjectId = filter.ObjectId,
                ObjectTypeId = filter.ObjectTypeId
            };
        }

        public static FilterfnPartnerProductSetting MapTofnPartnerProductSettings(this ApiFilterPartnerProductSetting filter, double timeZone)
        {
            return new FilterfnPartnerProductSetting
            {
                SkipCount = filter.SkipCount,
                TakeCount = filter.TakeCount,
                PartnerId = filter.PartnerId,
                ProviderId = filter.ProviderId,
                CategoryIds = filter.CategoryIds?.ToString(),
                HasImages = filter.HasImages,
                HasDemo = filter.HasDemo,
                IsForDesktop = filter.IsForDesktop,
                IsForMobile = filter.IsForMobile,
                Ids = filter.Ids == null ? new FiltersOperation() : filter.Ids.MapToFiltersOperation(timeZone),
                ProductIds = filter.ProductIds == null ? new FiltersOperation() : filter.ProductIds.MapToFiltersOperation(timeZone),
                ProductDescriptions = filter.ProductDescriptions == null ? new FiltersOperation() : filter.ProductDescriptions.MapToFiltersOperation(timeZone),
                ProductNames = filter.ProductNames == null ? new FiltersOperation() : filter.ProductNames.MapToFiltersOperation(timeZone),
                ProductExternalIds = filter.ProductExternalIds == null ? new FiltersOperation() : filter.ProductExternalIds.MapToFiltersOperation(timeZone),
                ProductGameProviders = filter.GameProviderIds == null ? new FiltersOperation() : filter.GameProviderIds.MapToFiltersOperation(timeZone),
                SubProviderIds = filter.SubProviderIds == null ? new FiltersOperation() : filter.SubProviderIds.MapToFiltersOperation(timeZone),
                Jackpots = filter.Jackpots == null ? new FiltersOperation() : filter.Jackpots.MapToFiltersOperation(timeZone),
                States = filter.States == null ? new FiltersOperation() : filter.States.MapToFiltersOperation(timeZone),
                Percents = filter.Percents == null ? new FiltersOperation() : filter.Percents.MapToFiltersOperation(timeZone),
                OpenModes = filter.OpenModes == null ? new FiltersOperation() : filter.OpenModes.MapToFiltersOperation(timeZone),
                RTPs = filter.RTPs == null ? new FiltersOperation() : filter.RTPs.MapToFiltersOperation(timeZone),
                ExternalIds = filter.ExternalIds == null ? new FiltersOperation() : filter.ExternalIds.MapToFiltersOperation(timeZone),
                Volatilities = filter.Volatilities == null ? new FiltersOperation() : filter.Volatilities.MapToFiltersOperation(timeZone),
                Ratings = filter.Ratings == null ? new FiltersOperation() : filter.Ratings.MapToFiltersOperation(timeZone),
                ProductIsLeaf = filter.ProductIsLeaf == null ? new FiltersOperation() : filter.ProductIsLeaf.MapToFiltersOperation(timeZone),
                OrderBy = filter.OrderBy,
                FieldNameToOrderBy = filter.FieldNameToOrderBy
            };
        }

        public static FilterfnPartnerProductSetting MapTofnPartnerProductSettings(this ApiFilterfnProduct input)
        {
            return new FilterfnPartnerProductSetting
            {
                SkipCount = input.SkipCount,
                TakeCount = input.TakeCount,
                IsForDesktop = input.IsForDesktop,
                IsForMobile = input.IsForMobile,
                ParentId = input.ParentId,
                Ids = input.Ids == null ? new FiltersOperation() : input.Ids.MapToFiltersOperation(input.TimeZone),
                ProductIds = input.ProductIds == null ? new FiltersOperation() : input.ProductIds.MapToFiltersOperation(input.TimeZone),
                ProductGameProviders = input.GameProviderIds == null ? new FiltersOperation() : input.GameProviderIds.MapToFiltersOperation(input.TimeZone),
                SubProviderIds = input.SubProviderIds == null ? new FiltersOperation() : input.SubProviderIds.MapToFiltersOperation(input.TimeZone),
                Jackpots = input.Jackpots == null ? new FiltersOperation() : input.Jackpots.MapToFiltersOperation(input.TimeZone),
                States = input.States == null ? new FiltersOperation() : input.States.MapToFiltersOperation(input.TimeZone),
                Percents = input.Percents == null ? new FiltersOperation() : input.Percents.MapToFiltersOperation(input.TimeZone),
                RTPs = input.RTPs == null ? new FiltersOperation() : input.RTPs.MapToFiltersOperation(input.TimeZone),
                ExternalIds = input.ExternalIds == null ? new FiltersOperation() : input.ExternalIds.MapToFiltersOperation(input.TimeZone),
                OrderBy = input.OrderBy,
                FieldNameToOrderBy = input.FieldNameToOrderBy,
                ProductId = input.ProductId,
                Pattern = input.Pattern,
                IsProviderActive = input.IsProviderActive
            };
        }

        public static FilterfnProduct MapTofnProductSettings(this ApiFilterPartnerProductSetting input)
        {
            return new FilterfnProduct
            {
                SkipCount = input.SkipCount,
                TakeCount = input.TakeCount,
                HasImages = input.HasImages,
                IsForDesktop = input.IsForDesktop,
                IsForMobile = input.IsForMobile,
                Ids = input.Ids == null ? new FiltersOperation() : input.Ids.MapToFiltersOperation(input.TimeZone),
                Descriptions = input.ProductDescriptions == null ? new FiltersOperation() : input.ProductDescriptions.MapToFiltersOperation(input.TimeZone),
                Names = input.ProductNames == null ? new FiltersOperation() : input.ProductNames.MapToFiltersOperation(input.TimeZone),
                ExternalIds = input.ProductExternalIds == null ? new FiltersOperation() : input.ProductExternalIds.MapToFiltersOperation(input.TimeZone),
                GameProviderIds = input.GameProviderIds == null ? new FiltersOperation() : input.GameProviderIds.MapToFiltersOperation(input.TimeZone),
                SubProviderIds = input.SubProviderIds == null ? new FiltersOperation() : input.SubProviderIds.MapToFiltersOperation(input.TimeZone),
                States = input.States == null ? new FiltersOperation() : input.States.MapToFiltersOperation(input.TimeZone),
                Jackpots = input.Jackpots == null ? new FiltersOperation() : input.Jackpots.MapToFiltersOperation(input.TimeZone),
                RTPs = input.RTPs == null ? new FiltersOperation() : input.RTPs.MapToFiltersOperation(input.TimeZone),
            };
        }
        public static FilterfnProduct MapToFilterfnProduct(this FilterPartnerProductsMapping product)
        {
            return new FilterfnProduct
            {
                Ids = product.Ids,
                Names = product.Names,
                Descriptions = product.Descriptions,
                GameProviderIds = product.GameProviderIds,
                States = product.States,
            };
        }
    }
}