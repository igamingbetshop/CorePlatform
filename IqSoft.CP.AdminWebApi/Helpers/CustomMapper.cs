using System;
using System.Collections.Generic;
using System.Linq;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Filters;
using IqSoft.CP.DAL.Filters.Reporting;
using IqSoft.CP.DAL.Filters.PaymentRequests;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Report;
using IqSoft.CP.AdminWebApi.Filters;
using IqSoft.CP.AdminWebApi.Filters.Bets;
using IqSoft.CP.AdminWebApi.Filters.PaymentRequests;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.AdminWebApi.Models.DashboardModels;
using IqSoft.CP.DAL.Models.Dashboard;
using IqSoft.CP.AdminWebApi.Models.ClientModels;
using IqSoft.CP.AdminWebApi.Filters.Reporting;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.AdminWebApi.Models.ReportModels;
using IqSoft.CP.DAL.Filters.Clients;
using IqSoft.CP.DAL.Models.RealTime;
using IqSoft.CP.AdminWebApi.Models.UserModels;
using IqSoft.CP.AdminWebApi.Models.BetShopModels;
using IqSoft.CP.AdminWebApi.RealTimeModels.Models;
using IqSoft.CP.AdminWebApi.ClientModels.Models;
using IqSoft.CP.DAL.Models.User;
using IqSoft.CP.AdminWebApi.Models.RoleModels;
using IqSoft.CP.AdminWebApi.Models.BonusModels;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.AdminWebApi.Models.CommonModels;
using IqSoft.CP.AdminWebApi.Models.ContentModels;
using IqSoft.CP.AdminWebApi.Models.CurrencyModels;
using IqSoft.CP.AdminWebApi.Models.LanguageModels;
using IqSoft.CP.AdminWebApi.Models.NotificationModels;
using IqSoft.CP.AdminWebApi.Models.PartnerModels;
using IqSoft.CP.AdminWebApi.Models.PaymentModels;
using IqSoft.CP.AdminWebApi.Models.ProductModels;
using IqSoft.CP.AdminWebApi.Models.ReportModels.BetShop;
using IqSoft.CP.AdminWebApi.Models.ReportModels.Internet;
using PermissionModel = IqSoft.CP.AdminWebApi.Models.RoleModels.PermissionModel;
using IqSoft.CP.AdminWebApi.Filters.Messages;
using IqSoft.CP.DAL.Filters.Messages;
using IqSoft.CP.AdminWebApi.Models.AgentModels;
using Newtonsoft.Json;
using IqSoft.CP.AdminWebApi.Models;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.DAL.Models.Bonuses;
using IqSoft.CP.Common.Models.Filters;
using IqSoft.CP.Common.Models.AdminModels;
using IqSoft.CP.AdminWebApi.Filters.Clients;
using static IqSoft.CP.Common.Constants;
using IqSoft.CP.AdminWebApi.Models.CRM;
using IqSoft.CP.DAL.Filters.Affiliate;
using IqSoft.CP.Common.Models.AffiliateModels;
using IqSoft.CP.DAL.Models.Clients;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DataWarehouse;
using IqSoft.CP.DataWarehouse.Models;
using IqSoft.CP.DataWarehouse.Filters;
using Client = IqSoft.CP.DAL.Client;
using Document = IqSoft.CP.DAL.Document;
using User = IqSoft.CP.DAL.User;
using IqSoft.CP.Common.Models.Report;

namespace IqSoft.CP.AdminWebApi.Helpers
{
    public static class CustomMapper
    {
        public static ApiResponseBase MapToApiResponseBase(this ResponseBase input)
        {
            return new ApiResponseBase
            {
                ResponseCode = input.ResponseCode,
                Description = input.Description
            };
        }

        #region Models

        #region RealTime

        public static ApiRealTimeInfo MapToApiRealTimeInfo(this RealTimeInfo info)
        {
            return new ApiRealTimeInfo
            {
                OnlineClients = info.OnlineClients.Select(MapToApiOnlineClient).ToList(),
                Count = info.Count,
                TotalLoginsCount = info.TotalLoginsCount,
                TotalBetsCount = info.TotalBetsCount,
                TotalBetsAmount = info.TotalBetsAmount,
                TotalPlayersCount = info.TotalPlayersCount,
                ApprovedDepositsCount = info.ApprovedDepositsCount,
                ApprovedDepositsAmount = info.ApprovedDepositsAmount,
                ApprovedWithdrawalsCount = info.ApprovedWithdrawalsCount,
                ApprovedWithdrawalsAmount = info.ApprovedWithdrawalsAmount,
                WonBetsCount = info.WonBetsCount,
                WonBetsAmount = info.WonBetsAmount,
                LostBetsCount = info.LostBetsCount,
                LostBetsAmount = info.LostBetsAmount
            };
        }

        public static ApiOnlineClient MapToApiOnlineClient(this BllOnlineClient info)
        {
            return new ApiOnlineClient
            {
                Id = info.Id,
                FirstName = info.FirstName,
                LastName = info.LastName,
                UserName = info.UserName,
                RegionId = info.RegionId,
                CurrencyId = info.CurrencyId,
                PartnerId = info.PartnerId,
                IsDocumentVerified = info.IsDocumentVerified,
                RegistrationDate = info.RegistrationDate,
                PartnerName = info.PartnerName,
                CategoryId = info.CategoryId,
                HasNote = info.HasNote,
                LoginIp = info.LoginIp,
                SessionTime = info.SessionTime,
                SessionLanguage = info.SessionLanguage,
                CurrentPage = info.CurrentPage,
                TotalDepositsCount = info.TotalDepositsCount,
                CanceledDepositsCount = info.CanceledDepositsCount,
                PendingDepositsCount = info.PendingDepositsCount,
                PendingDepositsAmount = Math.Floor((info.PendingDepositsAmount ?? 0) * 100) / 100,
                LastDepositState = info.LastDepositState,
                TotalDepositsAmount = Math.Floor((info.TotalDepositsAmount ?? 0) * 100) / 100,
                TotalWithdrawalsCount = info.TotalWithdrawalsCount,
                PendingWithdrawalsCount = info.PendingWithdrawalsCount,
                PendingWithdrawalsAmount = Math.Floor((info.PendingWithdrawalsAmount ?? 0) * 100) / 100,
                TotalWithdrawalsAmount = Math.Floor((info.TotalWithdrawalsAmount ?? 0) * 100) / 100,
                TotalBetsCount = info.TotalBetsCount,
                GGR = info.GGR,
                Balance = Math.Floor((info.Balance ?? 0) * 100) / 100,
            };
        }

        #endregion

        #region Reporting

        #region Internet Reports

        public static InternetBetsReportModel MapToInternetBetsReportModel(this InternetBetsReport internetBetsReport, double timeZone)
        {
            return new InternetBetsReportModel
            {
                Count = internetBetsReport.Count,
                Entities = internetBetsReport.Entities.Select(x => x.MapToInternetBetModel(timeZone)).ToList()
            };
        }

        public static InternetBetModel MapToInternetBetModel(this fnInternetBet fnInternetBet, double timeZone)
        {
            return new InternetBetModel
            {
                BetDocumentId = fnInternetBet.BetDocumentId,
                //BetExternalTransactionId = fnInternetBet.BetExternalTransactionId,
                //WinExternalTransactionId = fnInternetBet.WinExternalTransactionId,
                State = fnInternetBet.State,
                BetInfo = string.Empty,
                ProductId = fnInternetBet.ProductId,
                GameProviderId = fnInternetBet.GameProviderId,
                SubproviderId = fnInternetBet.SubproviderId,
                SubproviderName = fnInternetBet.SubproviderName,
                TicketInfo = string.Empty,
                BetDate = fnInternetBet.BetDate.GetGMTDateFromUTC(timeZone),
                CalculationDate = fnInternetBet.WinDate?.GetGMTDateFromUTC(timeZone),
                ClientId = fnInternetBet.ClientId,
                UserId = fnInternetBet.UserId,
                ClientUserName = fnInternetBet.ClientUserName,
                ClientFirstName = fnInternetBet.ClientFirstName,
                ClientLastName = fnInternetBet.ClientLastName,
                BetAmount = fnInternetBet.BetAmount,
                OriginalBetAmount = fnInternetBet.OriginalBetAmount,
                Coefficient = fnInternetBet.Coefficient ?? 0,
                WinAmount = fnInternetBet.WinAmount,
                OriginalWinAmount = fnInternetBet.OriginalWinAmount,
                BonusAmount = fnInternetBet.BonusAmount,
                OriginalBonusAmount = fnInternetBet.OriginalBonusAmount,
                CurrencyId = fnInternetBet.CurrencyId,
                TicketNumber = fnInternetBet.TicketNumber,
                DeviceTypeId = fnInternetBet.DeviceTypeId,
                BetTypeId = fnInternetBet.BetTypeId,
                PossibleWin = fnInternetBet.State == (int)BetDocumentStates.Uncalculated ? fnInternetBet.PossibleWin : fnInternetBet.WinAmount,
                PartnerId = fnInternetBet.PartnerId,
                ProductName = fnInternetBet.ProductName,
                ProviderName = fnInternetBet.ProviderName,
                ClientIp = string.Empty,
                Country = string.Empty,
                ClientCategoryId = fnInternetBet.ClientCategoryId,
              //  HasNote = fnInternetBet.HasNote,
                RoundId = string.Empty,
                ClientHasNote = fnInternetBet.ClientHasNote,
                Profit = fnInternetBet.BetAmount - fnInternetBet.WinAmount,
                Rake = fnInternetBet.Rake,
                BonusId = fnInternetBet.BonusId,
                LastUpdateTime = fnInternetBet.LastUpdateTime.GetGMTDateFromUTC(timeZone)
            };
        }

        public static ApiInternetBetsByClientReport MapToApiInternetBetsByClient(this InternetBetsByClientReport bets)
        {
            return new ApiInternetBetsByClientReport
            {
                Entities = bets.Entities.Select(MapToApiInternetBetByClient).ToList(),
                Count = bets.Count,

                TotalBetCount = bets.TotalBetCount,
                TotalBetAmount = bets.TotalBetAmount,
                TotalWinAmount = bets.TotalWinAmount,
                TotalBalance = bets.TotalBalance,
                TotalDepositCount = bets.TotalDepositCount,
                TotalDepositAmount = bets.TotalDepositAmount,
                TotalWithdrawCount = bets.TotalWithdrawCount,
                TotalWithdrawAmount = bets.TotalWithdrawAmount,
                TotalCurrencyCount = bets.TotalCurrencyCount,
                TotalGGR = (decimal)Math.Round(Convert.ToDouble(bets.TotalGGR), 2)
            };
        }

        public static ApiInternetBetByClient MapToApiInternetBetByClient(this InternetBetByClient bet)
        {
            return new ApiInternetBetByClient
            {
                ClientId = bet.ClientId,
                UserName = bet.UserName,
                TotalBetsCount = bet.TotalBetsCount,
                TotalBetsAmount = bet.TotalBetsAmount,
                TotalWinsAmount = bet.TotalWinsAmount,
                Currency = bet.Currency,
                GGR = bet.GGR,
                MaxBetAmount = bet.MaxBetAmount,
                TotalDepositsCount = bet.TotalDepositsCount,
                TotalDepositsAmount = bet.TotalDepositsAmount,
                TotalWithdrawalsCount = bet.TotalWithdrawalsCount,
                TotalWithdrawalsAmount = bet.TotalWithdrawalsAmount,
                Balance = bet.Balance
            };
        }

        #endregion

        #region BetShop Reports

        public static BetshopBetsReportModel MapToBetshopBetsReportModel(this BetShopBets betsReport, double timeZone)
        {
            return new BetshopBetsReportModel
            {
                Count = betsReport.Count,
                TotalBetAmount = betsReport.TotalBetAmount,
                TotalWinAmount = betsReport.TotalWinAmount,
                TotalProfit = betsReport.TotalProfit,
                Entities = betsReport.Entities.Select(x => x.MapToBetShopBet(timeZone)).ToList()
            };
        }

        public static BetShopBetModel MapToBetShopBet(this fnBetShopBet fnBetShopBet, double timeZone)
        {
            return new BetShopBetModel
            {
                Barcode = fnBetShopBet.Barcode,
                CurrencyId = fnBetShopBet.CurrencyId,
                CashDeskId = fnBetShopBet.CashDeskId,
                BetDocumentId = fnBetShopBet.BetDocumentId,
                ProductId = fnBetShopBet.ProductId,
                State = fnBetShopBet.State,
                CashierId = fnBetShopBet.CashierId,
                GameProviderId = fnBetShopBet.GameProviderId,
                BetDate = fnBetShopBet.BetDate.GetGMTDateFromUTC(timeZone),
                BetExternalTransactionId = string.Empty,
                TicketNumber = fnBetShopBet.TicketNumber,
                BetAmount = fnBetShopBet.BetAmount,
                BetInfo = string.Empty,
                WinAmount = fnBetShopBet.WinAmount,
                WinDate = fnBetShopBet.WinDate.GetGMTDateFromUTC(timeZone),
                PayDate = fnBetShopBet.PayDate.GetGMTDateFromUTC(timeZone),
                BetShopId = fnBetShopBet.BetShopId,
                BetShopName = fnBetShopBet.BetShopName,
                PartnerId = fnBetShopBet.PartnerId,
                ProductName = fnBetShopBet.ProductName,
                BetShopGroupId = fnBetShopBet.BetShopGroupId,
                BetTypeId = fnBetShopBet.BetTypeId,
                PossibleWin = fnBetShopBet.PossibleWin,
                ProviderName = fnBetShopBet.ProviderName,
                HasNote = fnBetShopBet.HasNote,
                RoundId = string.Empty,
                Profit = fnBetShopBet.BetAmount - fnBetShopBet.WinAmount
            };
        }

        public static ApiBetShopReport MapToBetShopReportModel(this BetShopReport betsReport)
        {
            return new ApiBetShopReport
            {
                BetAmount = betsReport.BetAmount,
                WinAmount = betsReport.WinAmount,
                BetShopName = betsReport.BetShopName,
                BetShopId = betsReport.BetShopId,
                BetShopGroupId = betsReport.BetShopGroupId,
                CurrencyId = betsReport.CurrencyId
            };
        }

        public static ApiBetShopsReport MapToBetShopsReportModel(this BetShops report)
        {
            return new ApiBetShopsReport
            {
                TotalWinAmount = report.TotalWinAmount ?? 0,
                TotalBetAmount = report.TotalBetAmount ?? 0,
                TotalProfit = report.TotalProfit ?? 0,
                Entities = report.Entities.Select(MapToBetShopReportModel).ToList()
            };
        }

        public static ApiBetShopGamesReport ToApiBetShopGamesReport(this BetShopGames report)
        {
            return new ApiBetShopGamesReport
            {
                TotalWinAmount = report.TotalWinAmount ?? 0,
                TotalBetAmount = report.TotalBetAmount ?? 0,
                TotalOriginalWinAmount = report.TotalOriginalWinAmount ?? 0,
                TotalOriginalBetAmount = report.TotalOriginalBetAmount ?? 0,
                TotalProfit = (report.TotalBetAmount - report.TotalWinAmount) ?? 0,
                TotalBetCount = report.TotalBetCount,
                Entities = report.Entities.Select(ToApiBetShopGame).ToList()
            };
        }

        public static ApiBetShopGame ToApiBetShopGame(this BetShopGame item)
        {
            return new ApiBetShopGame
            {
                GameId = item.GameId,
                GameName = item.GameName,
                CurrencyId = item.CurrencyId,
                Count = item.Count,
                BetAmount = item.BetAmount,
                WinAmount = item.WinAmount,
                OriginalBetAmount = item.OriginalBetAmount,
                OriginalWinAmount = item.OriginalWinAmount
            };
        }

        public static ApiInternetGamesReport ToApiInternetGamesReport(this InternetGames report)
        {
            return new ApiInternetGamesReport
            {
                TotalWinAmount = report.TotalWinAmount ?? 0,
                TotalBetAmount = report.TotalBetAmount ?? 0,
                TotalOriginalWinAmount = report.TotalOriginalWinAmount ?? 0,
                TotalOriginalBetAmount = report.TotalOriginalBetAmount ?? 0,
                TotalProfit = (report.TotalBetAmount - report.TotalWinAmount) ?? 0,
                TotalBetCount = report.TotalBetCount,
                TotalSupplierFee = report.TotalSupplierFee,
                Entities = report.Entities.Select(ToApiInternetGame).ToList()
            };
        }

        public static ApiInternetGame ToApiInternetGame(this InternetGame item)
        {
            return new ApiInternetGame
            {
                ProductId = item.GameId,
                ProductName = item.GameName,
                CurrencyId = item.CurrencyId,
                Count = item.Count,
                BetAmount = item.BetAmount,
                WinAmount = item.WinAmount,
                OriginalBetAmount = item.OriginalBetAmount,
                OriginalWinAmount = item.OriginalWinAmount,
                ProviderId = item.ProviderId,
                ProviderName = item.ProviderName,
                SubproviderId = item.SubproviderId,
                SubproviderName = item.SubproviderName,
                SupplierPercent = item.SupplierPercent,
                SupplierFee = item.SupplierFee
            };
        }

        public static ApiReportByBetShopPaymentsElement MapToApiReportByBetShopPaymentsElement(this fnReportByBetShopOperation element)
        {
            return new ApiReportByBetShopPaymentsElement
            {
                Id = element.Id,
                GroupId = element.GroupId,
                Name = element.Name,
                Address = element.Address,
                TotalPendingDepositsCount = element.TotalPandingDepositsCount ?? 0,
                TotalPendingDepositsAmount = element.TotalPandingDepositsAmount ?? 0,
                TotalPayedDepositsCount = element.TotalPayedDepositsCount ?? 0,
                TotalPayedDepositsAmount = element.TotalPayedDepositsAmount ?? 0,
                TotalCanceledDepositsCount = element.TotalCanceledDepositsCount ?? 0,
                TotalCanceledDepositsAmount = element.TotalCanceledDepositsAmount ?? 0,
                TotalPendingWithdrawalsCount = element.TotalPandingWithdrawalsCount ?? 0,
                TotalPendingWithdrawalsAmount = element.TotalPandingWithdrawalsAmount ?? 0,
                TotalPayedWithdrawalsCount = element.TotalPayedWithdrawalsCount ?? 0,
                TotalPayedWithdrawalsAmount = element.TotalPayedWithdrawalsAmount ?? 0,
                TotalCanceledWithdrawalsCount = element.TotalCanceledWithdrawalsCount ?? 0,
                TotalCanceledWithdrawalsAmount = element.TotalCanceledWithdrawalsAmount ?? 0
            };
        }

        #endregion

        #region BusinessIntelligence Reports

        public static ApiReportByProvidersElement MapToApiReportByProvidersElement(this ReportByProvidersElement element)
        {
            return new ApiReportByProvidersElement
            {
                PartnerId = element.PartnerId,
                ProviderName = element.ProviderName,
                Currency = element.Currency,
                TotalBetsCount = element.TotalBetsCount,
                TotalBetsAmount = element.TotalBetsAmount,
                TotalWinsAmount = element.TotalWinsAmount,
                TotalUncalculatedBetsCount = element.TotalUncalculatedBetsCount,
                TotalUncalculatedBetsAmount = element.TotalUncalculatedBetsAmount,
                GGR = element.GGR,
                BetsCountPercent = element.BetsCountPercent,
                BetsAmountPercent = element.BetsAmountPercent,
                GGRPercent = element.GGRPercent
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

        public static ApiReportByAgentTranfer MapToFilterReportByAgentTranfer(this fnReportByAgentTransfer input)
        {
            return new ApiReportByAgentTranfer
            {
                PartnerId = input.PartnerId,
                PartnerName = input.PartnerName,
                UserId = input.UserId,
                UserName = input.UserName,
                NickName = input.NickName,
                TotalProfit = input.TotoalProfit ?? 0,
                TotalDebit = input.TotalDebit ?? 0,
                Balance = input.Balance ?? 0
            };
        }

        public static ApiReportByUserTransaction MapToApiReportByUserTransaction(this fnReportByUserTransaction input)
        {
            return new ApiReportByUserTransaction
            {
                PartnerId = input.PartnerId,
                PartnerName = input.PartnerName,
                UserId = input.UserId,
                Username = input.Username,
                NickName = input.NickName,
                UserFirstName = input.UserFirstName,
                UserLastName = input.UserLastName,
                FromUserId = input.FromUserId,
                FromUsername = input.FromUsername,
                ClientId = input.ClientId,
                ClientUsername = input.ClientUsername,
                OperationTypeId = input.OperationTypeId,
                OperationType = input.OperationType,
                Amount = input.Amount,
                CurrencyId = input.CurrencyId,
                CreationTime = input.CreationTime
            };
        }

        public static List<ApiReportByProductsElement> MapToApiReportByProductsElements(this List<fnReportByProduct> elements)
        {
            return elements.Select(MapToApiReportByProductsElement).ToList();
        }

        public static ApiReportByProductsElement MapToApiReportByProductsElement(this fnReportByProduct element)
        {
            return new ApiReportByProductsElement
            {
                ClientId = element.ClientId,
                ClientFirstName = element.ClientFirstName,
                ClientLastName = element.ClientLastName,
                Currency = element.Currency,
                ProductId = element.ProductId,
                ProductName = element.ProductName,
                DeviceTypeId = element.DeviceTypeId == null ? (int)Common.Enums.DeviceTypes.Desktop : element.DeviceTypeId.Value,
                ProviderName = element.ProviderName,
                TotalBetsAmount = element.TotalBetsAmount ?? 0,
                TotalWinsAmount = element.TotalWinsAmount ?? 0,
                TotalBetsCount = element.TotalBetsCount ?? 0,
                TotalUncalculatedBetsCount = element.TotalUncalculatedBetsCount ?? 0,
                TotalUncalculatedBetsAmount = element.TotalUncalculatedBetsAmount ?? 0,
                GGR = element.GGR ?? 0
            };
        }

        #endregion

        #region BusinessAudit Reports



        #endregion

        #endregion

        #region User

        public static List<UserModel> MapToUserModels(this IEnumerable<BllUser> users, double timeZone)
        {
            return users.Select(x => x.MapToUserModel(timeZone)).ToList();
        }

        public static UserModel MapToUserModel(this BllUser user, double timeZone)
        {
            return new UserModel
            {
                Id = user.Id,
                PartnerId = user.PartnerId,
                CreationTime = user.CreationTime.GetGMTDateFromUTC(timeZone),
                CurrencyId = user.CurrencyId,
                FirstName = user.FirstName,
                Gender = user.Gender,
                LastName = user.LastName,
                UserName = user.UserName,
                State = user.State,
                Type = user.Type,
                Email = user.Email,
                ParentId = user.ParentId,
                Level = user.Level
            };
        }

        public static UserModel MapToUserModel(this User user, double timeZone)
        {
            return new UserModel
            {
                Id = user.Id,
                PartnerId = user.PartnerId,
                CreationTime = user.CreationTime.GetGMTDateFromUTC(timeZone),
                LastUpdateTime = user.LastUpdateTime.GetGMTDateFromUTC(timeZone),
                CurrencyId = user.CurrencyId,
                FirstName = user.FirstName,
                Gender = user.Gender,
                LastName = user.LastName,
                UserName = user.UserName,
                NickName = user.NickName,
                MobileNumber = user.MobileNumber,
                State = user.State,
                Type = user.Type,
                Email = user.Email,
                ParentId = user.ParentId,
                LanguageId = user.LanguageId,
                Level = user.Level,
                OddsType = user.OddsType,
                Phone = user.Phone,
                CorrectionMaxAmount = user.CorrectionMaxAmount,
                CorrectionMaxAmountCurrency = user.CorrectionMaxAmountCurrency
            };
        }

        public static List<UserModel> MapToUserModels(this IEnumerable<fnUser> users, double timeZone)
        {
            return users.Select(x => x.MapToUserModel(timeZone)).ToList();
        }

        public static UserModel MapToUserModel(this fnUser user, double timeZone)
        {
            return new UserModel
            {
                Id = user.Id,
                PartnerId = user.PartnerId,
                CreationTime = user.CreationTime.GetGMTDateFromUTC(timeZone),
                CurrencyId = user.CurrencyId,
                FirstName = user.FirstName,
                Gender = user.Gender,
                LanguageId = user.LanguageId,
                LastName = user.LastName,
                UserName = user.UserName,
                State = user.State,
                Type = user.Type,
                Email = user.Email,
                ParentId = user.ParentId,
                MobileNumber = user.MobileNumber,
                Level = user.Level,
                UserRoles = user.UserRoles
            };
        }

        #region Agent

        public static List<ApiAgentCommission> MapToApiAgentCommissions(this IEnumerable<AgentCommission> agentCommissions)
        {
            return agentCommissions.Select(x => x.MapToApiAgentCommission()).ToList();
        }

        public static ApiAgentCommission MapToApiAgentCommission(this AgentCommission agentCommission)
        {
            var isNumber = Decimal.TryParse(agentCommission.TurnoverPercent, out decimal percent);
            var turnoverPercents = string.IsNullOrEmpty(agentCommission.TurnoverPercent) || isNumber ? null : JsonConvert.DeserializeObject<List<ApiTurnoverPercent>>(agentCommission.TurnoverPercent);
            return new ApiAgentCommission
            {
                Id = agentCommission.Id,
                AgentId = agentCommission.AgentId ?? 0,
                ProductId = agentCommission.ProductId,
                Percent = agentCommission.Percent,
                TurnoverPercent = isNumber ? agentCommission.TurnoverPercent :
                ((turnoverPercents == null || !turnoverPercents.Any()) ? string.Empty : String.Join(",", turnoverPercents.Select(x => string.Format("{0}-{1}|{2}", x.FromCount, x.ToCount, x.Percent))))
            };
        }

        public static AgentCommission MapToAgentCommission(this ApiAgentCommission agentCommission)
        {
            return new AgentCommission
            {
                AgentId = agentCommission.AgentId,
                ProductId = agentCommission.ProductId,
                Percent = agentCommission.Percent,
                TurnoverPercent = (agentCommission.TurnoverPercentsList != null && agentCommission.TurnoverPercentsList.Any()) ?
                JsonConvert.SerializeObject(agentCommission.TurnoverPercentsList) : agentCommission.TurnoverPercent
            };
        }
        #endregion

        public static User MapToUser(this UserModel user, double timeZone)
        {
            return new User
            {
                Id = user.Id,
                Password = user.Password,
                PartnerId = user.PartnerId,
                CreationTime = user.CreationTime.GetGMTDateFromUTC(timeZone),
                CurrencyId = user.CurrencyId,
                FirstName = user.FirstName,
                Gender = user.Gender,
                LanguageId = user.LanguageId,
                LastName = user.LastName,
                State = user.State,
                Type = user.Type,
                UserName = user.UserName,
                NickName = user.UserName,
                MobileNumber = user.MobileNumber,
                Email = user.Email,
                ParentId = user.ParentId,
                OddsType = user.OddsType,
                CorrectionMaxAmount = user.CorrectionMaxAmount,
                CorrectionMaxAmountCurrency = user.CorrectionMaxAmountCurrency
            };
        }

        #endregion

        #region Client

        public static ClientHistoryModel MapToClientHistoryModel(this ObjectChangeHistoryItem input, double timeZone)
        {
            return new ClientHistoryModel
            {
                Id = input.Id,
                Comment = input.Comment,
                ChangeDate = input.ChangeDate.GetGMTDateFromUTC(timeZone),
                FirstName = input.FirstName,
                LastName = input.LastName
            };
        }

        public static Client MapToClient(this ChangeClientDetailsInput input, Client client)
        {
            int regionId = input.RegionId ?? client.RegionId;
            if (input.TownId != null)
                regionId = input.TownId.Value;
            else if (input.CityId != null)
                regionId = input.CityId.Value;
            else if (input.DistrictId != null)
                regionId = input.DistrictId.Value;
            else if (input.CountryId != null)
                regionId = input.CountryId.Value;

            return new Client
            {
                Id = input.Id,
                Email = input.Email?.ToLower() ?? client.Email,
                FirstName = input.FirstName ?? client.FirstName,
                LastName = input.LastName ?? client.LastName,
                NickName = input.NickName ?? client.NickName,
                ZipCode = input.ZipCode ?? client.ZipCode?.Trim(),
                City = input.City ?? client.City,
                DocumentNumber = input.DocumentNumber ?? client.DocumentNumber,
                DocumentIssuedBy = input.DocumentIssuedBy ?? client.DocumentIssuedBy,
                Address = input.Address ?? client.Address,
                MobileNumber = input.MobileNumber ?? client.MobileNumber,
                PhoneNumber = input.PhoneNumber ?? client.PhoneNumber,
                LanguageId = input.LanguageId ?? client.LanguageId,
                DocumentType = input.DocumentType ?? client.DocumentType,
                Comment = input.Comment,
                State = input.State ?? client.State,
                CallToPhone = input.CallToPhone ?? client.CallToPhone,
                SendMail = input.SendMail ?? client.SendMail,
                SendSms = input.SendSms ?? client.SendSms,
                SendPromotions = input.SendPromotions ?? client.SendPromotions,
                IsEmailVerified = input.IsEmailVerified ?? client.IsEmailVerified,
                IsMobileNumberVerified = input.IsMobileNumberVerified ?? client.IsMobileNumberVerified,
                Gender = input.Gender ?? client.Gender,
                RegionId = regionId,
                CountryId = input.CountryId ?? client.CountryId,
                IsDocumentVerified = input.IsDocumentVerified ?? client.IsDocumentVerified,
                BirthDate = input.BirthDate ?? client.BirthDate,
                CategoryId = input.CategoryId ?? client.CategoryId,
                Info = input.Info ?? client.Info,
                BetShopId = input.BetShopId ?? client.BetShopId,
                Citizenship = input.Citizenship ?? client.Citizenship,
                JobArea = input.JobArea ?? client.JobArea,
                SecondName = input.SecondName ?? client.SecondName,
                SecondSurname = input.SecondSurname ?? client.SecondSurname,
                BuildingNumber = input.BuildingNumber ?? client.BuildingNumber,
                Apartment = input.Apartment ?? client.Apartment,
                UserId = input.UserId ?? client.UserId,
                USSDPin = input.USSDPin ?? client.USSDPin,
                Title = input.Title ?? client.Title
            };
        }

        public static Client MapToClient(this NewClientModel input)
        {
            return new Client
            {
                PartnerId = input.PartnerId,
                Email = string.IsNullOrWhiteSpace(input.Email) ? string.Empty : input.Email,
                UserName = string.IsNullOrWhiteSpace(input.UserName) ? CommonFunctions.GetRandomString(10) : input.UserName,
                NickName = input.NickName,
                IsMobileNumberVerified = false,
                IsEmailVerified = false,
                Password = input.Password,
                CurrencyId = input.CurrencyId,
                FirstName = input.FirstName,
                LastName = input.LastName,
                IsDocumentVerified = input.IsDocumentVerified,
                DocumentNumber = input.DocumentNumber,
                DocumentIssuedBy = input.DocumentIssuedBy,
                Address = input.Address,
                MobileNumber = string.IsNullOrWhiteSpace(input.MobileNumber) ? string.Empty : (input.MobileNumber.StartsWith("+") ? input.MobileNumber : "+" + input.MobileNumber),
                PhoneNumber = input.PhoneNumber,
                LanguageId = input.LanguageId,
                SendMail = input.SendMail,
                SendSms = input.SendSms,
                Gender = input.Gender,
                RegionId = input.RegionId,
                CountryId = input.CountryId,
                City = input.CityName,
                ZipCode = input.ZipCode,
                Apartment = input.Apartment,
                BuildingNumber = input.BuildingNumber,
                SecondName = input.SecondName,
                SendPromotions = input.SendPromotions,
                BirthDate = input.BirthDate,
                Info = input.Info,
                Citizenship = input.Citizenship,
                JobArea = input.JobArea,
                BetShopId = input.BetShopId
            };
        }

        public static ClientIdentity ToClientIdentity(this AddClientIdentityModel input, double timeZone)
        {
            var expDate = input.ExpirationTime.GetGMTDateFromUTC(timeZone);
            return new ClientIdentity
            {
                Id = input.Id,
                ClientId = input.ClientId,
                DocumentTypeId = input.DocumentTypeId,
                Status = input.State,
                UserId = input.UserId,
                ExpirationTime = (expDate == null || expDate == DateTime.MaxValue || expDate == DateTime.MinValue) ?
                    (DateTime?)null : new DateTime(expDate.Value.Year, expDate.Value.Month, expDate.Value.Day)
            };
        }

        public static ClientIdentityModel ToClientIdentityModel(this ClientIdentity clientIdentity, double timeZone)
        {
            return new ClientIdentityModel
            {
                Id = clientIdentity.Id,
                ClientId = clientIdentity.ClientId,
                ImagePath = clientIdentity.ImagePath,
                UserId = clientIdentity.UserId,
                CreationTime = clientIdentity.CreationTime.GetGMTDateFromUTC(timeZone),
                LastUpdateTime = clientIdentity.LastUpdateTime.GetGMTDateFromUTC(timeZone),
                DocumentTypeId = clientIdentity.DocumentTypeId,
                ExpirationTime = clientIdentity.ExpirationTime
            };
        }

        public static ClientIdentityModel ToClientIdentityModel(this fnClientIdentity clientIdentity, double timeZone)
        {
            return new ClientIdentityModel
            {
                Id = clientIdentity.Id,
                PartnerId = clientIdentity.PartnerId,
                ClientId = clientIdentity.ClientId,
                UserName = clientIdentity.UserName,
                ImagePath = clientIdentity.ImagePath,
                UserId = clientIdentity.UserId,
                UserFirstName = clientIdentity.UserFirstName,
                UserLastName = clientIdentity.UserLastName,
                CreationTime = clientIdentity.CreationTime.GetGMTDateFromUTC(timeZone),
                LastUpdateTime = clientIdentity.LastUpdateTime.GetGMTDateFromUTC(timeZone),
                DocumentTypeId = clientIdentity.DocumentTypeId,
                State = clientIdentity.Status,
                HasNote = clientIdentity.HasNote ?? false,
                ExpirationTime = clientIdentity.ExpirationTime
            };
        }

        public static List<Client> MapToClients(this IEnumerable<ClientModel> clientModels, double timeZone)
        {
            return clientModels.Select(x => x.MapToClient(timeZone)).ToList();
        }

        public static Client MapToClient(this ClientModel client, double timeZone)
        {
            return new Client
            {
                Id = client.Id,
                Email = client.Email,
                IsEmailVerified = client.IsEmailVerified,
                CurrencyId = client.CurrencyId,
                UserName = client.UserName,
                PartnerId = client.PartnerId,
                Gender = client.Gender,
                BirthDate = client.BirthDate,
                FirstName = client.FirstName,
                LastName = client.LastName,
                RegistrationIp = client.RegistrationIp,
                DocumentNumber = client.DocumentNumber,
                DocumentIssuedBy = client.DocumentIssuedBy,
                Address = client.Address,
                MobileNumber = client.MobileNumber,
                IsMobileNumberVerified = client.IsMobileNumberVerified,
                LanguageId = client.LanguageId,
                CreationTime = client.CreationTime.GetGMTDateFromUTC(timeZone),
                LastUpdateTime = client.LastUpdateTime.GetGMTDateFromUTC(timeZone),
                PhoneNumber = client.PhoneNumber,
                IsDocumentVerified = client.IsDocumentVerified
            };
        }

        public static List<ClientModel> MapToClientModels(this IEnumerable<Client> clients, bool hideClientContactInfo, double timeZone)
        {
            return clients.Select(x => x.MapToClientModel(hideClientContactInfo, timeZone)).ToList();
        }

        public static ClientModel MapToClientModel(this Client client, bool hideClientContactInfo, double timeZone)
        {
            var referralType = CacheManager.GetClientSettingByName(client.Id, Constants.ClientSettings.ReferralType);
            var underMonitoringTypes = CacheManager.GetClientSettingByName(client.Id, Constants.ClientSettings.UnderMonitoring);
            return new ClientModel
            {
                Id = client.Id,
                Email = hideClientContactInfo ? "*****" : client.Email,
                IsEmailVerified = client.IsEmailVerified,
                CurrencyId = client.CurrencyId,
                UserName = client.UserName,
                PartnerId = client.PartnerId,
                Gender = client.Gender,
                BirthDate = client.BirthDate,
                Age = (DateTime.UtcNow - client.BirthDate).Days / 365,
                FirstName = client.FirstName,
                LastName = client.LastName,
                NickName = client.NickName,
                RegistrationIp = client.RegistrationIp,
                DocumentNumber = client.DocumentNumber,
                DocumentIssuedBy = client.DocumentIssuedBy,
                Address = client.Address,
                MobileNumber = hideClientContactInfo ? "*****" : client.MobileNumber,
                IsMobileNumberVerified = client.IsMobileNumberVerified,
                LanguageId = client.LanguageId,
                CreationTime = client.CreationTime.GetGMTDateFromUTC(timeZone),
                LastUpdateTime = client.LastUpdateTime.GetGMTDateFromUTC(timeZone),
                RegionId = client.RegionId,
                CountryId = client.CountryId,
                SendMail = client.SendMail,
                SendSms = client.SendSms,
                PhoneNumber = client.PhoneNumber,
                IsDocumentVerified = client.IsDocumentVerified,
                CallToPhone = client.CallToPhone,
                SendPromotions = client.SendPromotions,
                State = client.State,
                CategoryId = client.CategoryId,
                UserId = client.UserId,
                ZipCode = client.ZipCode,
                City = client.City,
                Info = client.Info,
                DocumentType = client.DocumentType,
                HasNote = client.HasNote,
                Citizenship = client.Citizenship,
                JobArea = client.JobArea,
                FirstDepositDate = client.FirstDepositDate?.GetGMTDateFromUTC(timeZone),
                LastDepositDate = client.LastDepositDate?.GetGMTDateFromUTC(timeZone),
                LastDepositAmount = client.LastDepositAmount,
                BetShopId = client.BetShopId,
                SecondName = client.SecondName,
                SecondSurname = client.SecondSurname,
                BuildingNumber = client.BuildingNumber,
                Apartment = client.Apartment,
                AffiliateId = client.AffiliateReferral?.AffiliateId,
                AffiliatePlatformId = client.AffiliateReferral?.AffiliatePlatformId,
                RefId = client.AffiliateReferral?.RefId,
                ReferralType = (int?)referralType?.NumericValue,
                USSDPin = client.USSDPin,
                Title = client.Title,
                CharacterId = client.CharacterId,
                CharacterLevel = client.Character?.Order,
                CharacterName = client.Character?.NickName,
				UnderMonitoringTypes = underMonitoringTypes?.StringValue != null ? JsonConvert.DeserializeObject<List<int>>(underMonitoringTypes.StringValue) : null
            };
        }

        public static ClientInfoModel MapToClientInfoModel(this DAL.Models.Clients.ClientInfo client, bool hideContactInfo)
        {
            return new ClientInfoModel
            {
                Id = client.Id,
                UserName = client.UserName,
                CategoryId = client.CategoryId,
                CurrencyId = client.CurrencyId,
                FirstName = client.FirstName,
                LastName = client.LastName,
                NickName = client.NickName,
                Email = hideContactInfo ? "*****" : client.Email,
                RegistrationDate = client.RegistrationDate,
                Status = client.Status,
                Balance = client.Balance,
                BonusBalance = client.BonusBalance,
                WithdrawableBalance = client.WithdrawableBalance,
                GGR = client.GGR,
                NGR = client.NGR,
                Rake = client.Rake,
                TotalDepositsCount = client.TotalDepositsCount,
                TotalDepositsAmount = client.TotalDepositsAmount,
                TotalWithdrawalsCount = client.TotalWithdrawalsCount,
                TotalWithdrawalsAmount = client.TotalWithdrawalsAmount,
                FailedDepositsCount = client.FailedDepositsCount,
                FailedDepositsAmount = client.FailedDepositsAmount,
                Risk = client.Risk,
                IsOnline = client.IsOnline,
                IsDocumentVerified = client.IsDocumentVerified
            };
        }

        public static ApiAccountsBalanceHistoryElement MapToApiAccountsBalanceHistoryElement(this AccountsBalanceHistoryElement element, double timeZone)
        {
            return new ApiAccountsBalanceHistoryElement
            {
                TransactionId = element.TransactionId,
                DocumentId = element.DocumentId,
                AccountId = element.AccountId,
                AccountType = element.AccountType,
                BalanceBefore = element.BalanceBefore,
                OperationType = element.OperationType,
                OperationAmount = element.OperationAmount,
                BalanceAfter = element.BalanceAfter,
                OperationTime = element.OperationTime,//.GetGMTDateFromUTC(timeZone)
                PaymentSystemName = element.PaymentSystemName
            };
        }

        public static PaymentLimit MapToPaymentLimit(this ApiPaymentLimit paymentLimit)
        {
            return new PaymentLimit
            {
                ClientId = paymentLimit.ClientId,
                MaxDepositsCountPerDay = paymentLimit.MaxDepositsCountPerDay,
                MaxDepositAmount = paymentLimit.MaxDepositAmount,
                MaxTotalDepositsAmountPerDay = paymentLimit.MaxTotalDepositsAmountPerDay,
                MaxTotalDepositsAmountPerWeek = paymentLimit.MaxTotalDepositsAmountPerWeek,
                MaxTotalDepositsAmountPerMonth = paymentLimit.MaxTotalDepositsAmountPerMonth,
                MaxWithdrawAmount = paymentLimit.MaxWithdrawAmount,
                MaxTotalWithdrawsAmountPerDay = paymentLimit.MaxTotalWithdrawsAmountPerDay,
                MaxTotalWithdrawsAmountPerWeek = paymentLimit.MaxTotalWithdrawsAmountPerWeek,
                MaxTotalWithdrawsAmountPerMonth = paymentLimit.MaxTotalWithdrawsAmountPerMonth,
                StartTime = paymentLimit.StartTime,
                EndTime = paymentLimit.EndTime,
                RowState = paymentLimit.RowState
            };
        }

        public static ApiPaymentLimit MapToApiPaymentLimit(this PaymentLimit paymentLimit, int clientId)
        {
            return new ApiPaymentLimit
            {
                ClientId = clientId,
                MaxDepositsCountPerDay = paymentLimit == null ? null : paymentLimit.MaxDepositsCountPerDay,
                MaxDepositAmount = paymentLimit == null ? null : paymentLimit.MaxDepositAmount,
                MaxTotalDepositsAmountPerDay = paymentLimit == null ? null : paymentLimit.MaxTotalDepositsAmountPerDay,
                MaxTotalDepositsAmountPerWeek = paymentLimit == null ? null : paymentLimit.MaxTotalDepositsAmountPerWeek,
                MaxTotalDepositsAmountPerMonth = paymentLimit == null ? null : paymentLimit.MaxTotalDepositsAmountPerMonth,
                MaxWithdrawAmount = paymentLimit == null ? null : paymentLimit.MaxWithdrawAmount,
                MaxTotalWithdrawsAmountPerDay = paymentLimit == null ? null : paymentLimit.MaxTotalWithdrawsAmountPerDay,
                MaxTotalWithdrawsAmountPerWeek = paymentLimit == null ? null : paymentLimit.MaxTotalWithdrawsAmountPerWeek,
                MaxTotalWithdrawsAmountPerMonth = paymentLimit == null ? null : paymentLimit.MaxTotalWithdrawsAmountPerMonth,
                StartTime = paymentLimit == null ? null : paymentLimit.StartTime,
                EndTime = paymentLimit == null ? null : paymentLimit.EndTime
            };
        }

        public static ApiClientCorrections MapToApiClientCorrections(this PagedModel<fnCorrection> input, double timeZone)
        {
            return new ApiClientCorrections
            {
                Count = input.Count,
                Entities = input.Entities.Select(x => x.MapToApiClientCorrection(timeZone)).ToList()
            };
        }


        public static ApiClientCorrections MapToApiClientCorrections(this CorrectionsReport input, double timeZone)
        {
            return new ApiClientCorrections
            {
                Count = input.Count,
                Entities = input.Entities.Select(x => x.MapToApiClientCorrection(timeZone)).ToList(),
                TotalAmount = input.TotalAmount
            };
        }

        public static ApiClientCorrection MapToApiClientCorrection(this fnCorrection input, double timeZone)
        {
            return new ApiClientCorrection
            {
                Id = input.Id,
                Amount = input.Amount,
                CurrencyId = input.CurrencyId,
                State = input.State,
                Info = input.Info,
                ClientId = input.ClientId,
                ClientUserName = input.ClientUserName,
                UserId = input.Creator,
                CreationTime = input.CreationTime.GetGMTDateFromUTC(timeZone),
                LastUpdateTime = input.LastUpdateTime.GetGMTDateFromUTC(timeZone),
                OperationTypeName = input.OperationTypeName,
                UserName = input.UserName,
                FirstName = input.FirstName,
                LastName = input.LastName,
                AccoutTypeId = input.AccountTypeId,
                HasNote = input.HasNote ?? false,
                OperationTypeId = input.DocumentTypeId,
                ProductId = input.ProductId,
                ProductNickName = input.ProductNickName
            };
        }

        #endregion

        #region ClientSession

        public static List<ClientSessionModel> MapToClientSessionModels(this IEnumerable<fnClientSession> sessions, double timeZone)
        {
            return sessions.Select(x => x.MapToClientSessionModel(timeZone)).ToList();
        }

        public static ClientSessionModel MapToClientSessionModel(this fnClientSession session, double timeZone)
        {
            return new ClientSessionModel
            {
                Id = session.Id,
                PartnerId = session.PartnerId,
                ClientId = session.ClientId,
                FirstName = session.FirstName,
                LastName = session.LastName,
                UserName = session.UserName,
                Country = session.Country,
                DeviceType = session.DeviceType,
                Source = session.Source,
                LogoutType = session.LogoutType,
                State = session.State,
                Ip = session.Ip,
                LanguageId = session.LanguageId,
                EndTime = session.EndTime.GetGMTDateFromUTC(timeZone),
                StartTime = session.StartTime.GetGMTDateFromUTC(timeZone),
                LastUpdateTime = session.LastUpdateTime.GetGMTDateFromUTC(timeZone)
            };
        }

        public static UserSessionModel MapToUserSessionModel(this fnUserSession session, double timeZone)
        {
            return new UserSessionModel
            {
                Id = session.Id,
                PartnerId = session.PartnerId,
                UserId = session.UserId,
                FirstName = session.FirstName,
                LastName = session.LastName,
                UserName = session.UserName,
                Email = session.Email,
                Type = session.Type,
                LanguageId = session.LanguageId,
                Ip = session.Ip,
                State = session.State,
                LogoutType = session.LogoutType,
                StartTime = session.StartTime.GetGMTDateFromUTC(timeZone),
                EndTime = session.EndTime.GetGMTDateFromUTC(timeZone)
            };
        }

        public static List<ClientSessionModel> MapToClientSessionModels(this IEnumerable<ClientSession> sessions, double timeZone)
        {
            return sessions.Select(x => x.MapToClientSessionModel(timeZone)).ToList();
        }
        public static ClientSessionModel MapToClientSessionModel(this ClientSession session, double timeZone)
        {
            return new ClientSessionModel
            {
                Id = session.Id,
                ClientId = session.ClientId,
                Country = session.Country,
                ProductId = session.ProductId,
                DeviceType = session.DeviceType,
                Source = session.Source,
                LogoutType = session.LogoutType,
                State = session.State,
                Ip = session.Ip,
                LanguageId = session.LanguageId,
                EndTime = session.EndTime.GetGMTDateFromUTC(timeZone),
                StartTime = session.StartTime.GetGMTDateFromUTC(timeZone),
                LastUpdateTime = session.LastUpdateTime.GetGMTDateFromUTC(timeZone)
            };
        }

        #endregion

        #region Betshop

        public static ApiGetBetShopByIdOutput ToBetshopModel(this BetShop betShop, double timeZone)
        {
            return new ApiGetBetShopByIdOutput
            {
                Id = betShop.Id,
                Address = betShop.Address,
                CreationTime = betShop.CreationTime.GetGMTDateFromUTC(timeZone),
                LastUpdateTime = betShop.LastUpdateTime.GetGMTDateFromUTC(timeZone),
                CurrencyId = betShop.CurrencyId,
                GroupId = betShop.GroupId,
                PartnerId = betShop.PartnerId,
                SessionId = betShop.SessionId,
                State = betShop.State,
                CurrentLimit = betShop.CurrentLimit,
                DailyTicketNumber = betShop.DailyTicketNumber,
                DefaultLimit = betShop.DefaultLimit,
                GroupName = betShop.BetShopGroup != null ? betShop.BetShopGroup.Name : string.Empty,
                PartnerName = betShop.Partner != null ? betShop.Partner.Name : string.Empty,
                Name = betShop.Name,
                RegionId = betShop.RegionId,
                BonusPercent = betShop.BonusPercent ?? 0,
                PrintLogo = betShop.PrintLogo,
                Type = betShop.Type,
                AgentId = betShop.UserId,
                MaxCopyCount = betShop.MaxCopyCount,
                MaxWinAmount = betShop.MaxWinAmount,
                MinBetAmount = betShop.MinBetAmount,
                MaxEventCountPerTicket = betShop.MaxEventCountPerTicket,
                CommissionType = betShop.CommissionType,
                CommissionRate = betShop.CommissionRate,
                AnonymousBet = betShop.AnonymousBet,
                AllowCashout = betShop.AllowCashout,
                AllowLive = betShop.AllowLive,
                UsePin = betShop.UsePin,
                CashDeskModels = betShop.CashDesks == null ? new List<CashDeskModel>() :
                    betShop.CashDesks.Select(x => x.MapToCashDeskModel(timeZone)).ToList(),
                PaymentSystems = string.IsNullOrEmpty(betShop.PaymentSystems) ? new List<int>() : 
                    betShop.PaymentSystems.Split(',').Select(x => Convert.ToInt32(x)).ToList()
            };
        }

        public static ApiGetBetShopByIdOutput TofnBetshopModel(this fnBetShops betShop, double timeZone)
        {
            return new ApiGetBetShopByIdOutput
            {
                Id = betShop.Id,
                Address = betShop.Address,
                CreationTime = betShop.CreationTime.GetGMTDateFromUTC(timeZone),
                LastUpdateTime = betShop.LastUpdateTime.GetGMTDateFromUTC(timeZone),
                CurrencyId = betShop.CurrencyId,
                GroupId = betShop.GroupId,
                PartnerId = betShop.PartnerId,
                SessionId = betShop.SessionId,
                State = betShop.State,
                CurrentLimit = betShop.CurrentLimit,
                DailyTicketNumber = betShop.DailyTicketNumber,
                DefaultLimit = betShop.DefaultLimit,
                GroupName = betShop.GroupName,
                PartnerName = betShop.PartnerName,
                Name = betShop.Name,
                Balance = betShop.Balance,
                AgentId = betShop.AgentId,
                MaxCopyCount = betShop.MaxCopyCount,
                MaxWinAmount = betShop.MaxWinAmount,
                MinBetAmount = betShop.MinBetAmount,
                MaxEventCountPerTicket = betShop.MaxEventCountPerTicket,
                CommissionType = betShop.CommissionType,
                CommissionRate = betShop.CommissionRate,
                AnonymousBet = betShop.AnonymousBet,
                AllowCashout = betShop.AllowCashout,
                AllowLive = betShop.AllowLive,
                UsePin = betShop.UsePin,
                ExternalId = betShop.Ips
            };
        }

        public static BetShop MapToBetshop(this ApiGetBetShopByIdOutput input)
        {
            return new BetShop
            {
                Id = input.Id,
                Address = input.Address,
                CurrencyId = input.CurrencyId,
                GroupId = input.GroupId,
                SessionId = input.SessionId,
                CreationTime = input.CreationTime,
                LastUpdateTime = input.LastUpdateTime,
                PartnerId = input.PartnerId,
                State = input.State,
                DefaultLimit = input.DefaultLimit,
                CurrentLimit = input.CurrentLimit,
                DailyTicketNumber = input.DailyTicketNumber,
                RegionId = input.RegionId,
                Name = input.Name,
                Type = input.Type,
                BonusPercent = input.BonusPercent,
                PrintLogo = input.PrintLogo,
                UserId = input.AgentId,
                MaxCopyCount = input.MaxCopyCount,
                MaxWinAmount = input.MaxWinAmount,
                MinBetAmount = input.MinBetAmount,
                MaxEventCountPerTicket = input.MaxEventCountPerTicket,
                CommissionType = input.CommissionType,
                CommissionRate = input.CommissionRate,
                AnonymousBet = input.AnonymousBet,
                AllowCashout = input.AllowCashout,
                AllowLive = input.AllowLive,
                UsePin = input.UsePin,
                PaymentSystems = input.PaymentSystems == null ? string.Empty : string.Join(",", input.PaymentSystems)
            };
        }

        public static BetShop MapToBetshop(this ApiGetBetShopByIdOutput betShopModel, double timeZone)
        {
            return new BetShop
            {
                Id = betShopModel.Id,
                Address = betShopModel.Address,
                CreationTime = betShopModel.CreationTime.GetGMTDateFromUTC(timeZone),
                LastUpdateTime = betShopModel.LastUpdateTime.GetGMTDateFromUTC(timeZone),
                CurrencyId = betShopModel.CurrencyId,
                GroupId = betShopModel.GroupId,
                PartnerId = betShopModel.PartnerId,
                SessionId = betShopModel.SessionId,
                State = betShopModel.State,
                CurrentLimit = betShopModel.CurrentLimit,
                DailyTicketNumber = betShopModel.DailyTicketNumber,
                DefaultLimit = betShopModel.DefaultLimit,
                Name = betShopModel.Name
            };
        }

        public static List<BetShop> MapToBetshops(this IEnumerable<ApiGetBetShopByIdOutput> models)
        {
            return models.Select(x => x.MapToBetshop()).ToList();
        }

        public static BetShop MapToBetshopLimit(this ApiBetShopLimit betShopLimit)
        {
            return new BetShop
            {
                Id = betShopLimit.BetShopId,
                CurrentLimit = betShopLimit.CurrentLimit,
            };
        }

        #endregion

        #region CashDesk

        public static List<CashDeskModel> MapToCashDeskModels(this IEnumerable<CashDesk> cashDesks, double timeZone)
        {
            return cashDesks.Select(x => x.MapToCashDeskModel(timeZone)).ToList();
        }

        public static CashDeskModel MapToCashDeskModel(this BllCashDesk cashDesk, double timeZone)
        {
            return new CashDeskModel
            {
                Id = cashDesk.Id,
                BetShopId = cashDesk.BetShopId,
                CreationTime = cashDesk.CreationTime.GetGMTDateFromUTC(timeZone),
                LastUpdateTime = cashDesk.LastUpdateTime.GetGMTDateFromUTC(timeZone),
                Name = cashDesk.Name,
                State = cashDesk.State,
                EncryptionKey = cashDesk.EncryptIv,
                MacAddress = cashDesk.MacAddress,
                Restrictions = cashDesk.Restrictions
            };
        }

        public static CashDeskModel MapToCashDeskModel(this CashDesk cashDesk, double timeZone)
        {
            return new CashDeskModel
            {
                Id = cashDesk.Id,
                BetShopId = cashDesk.BetShopId,
                CreationTime = cashDesk.CreationTime.GetGMTDateFromUTC(timeZone),
                LastUpdateTime = cashDesk.LastUpdateTime.GetGMTDateFromUTC(timeZone),
                Name = cashDesk.Name,
                State = cashDesk.State,
                EncryptionKey = cashDesk.EncryptIv,
                MacAddress = cashDesk.MacAddress,
                Type = cashDesk.Type,
                Restrictions = cashDesk.Restrictions
            };
        }

        public static CashDeskModel MapTofnCashDeskModel(this fnCashDesks cashDesk, double timeZone)
        {
            return new CashDeskModel
            {
                Id = cashDesk.Id,
                BetShopId = cashDesk.BetShopId,
                CreationTime = cashDesk.CreationTime.GetGMTDateFromUTC(timeZone),
                LastUpdateTime = cashDesk.LastUpdateTime.GetGMTDateFromUTC(timeZone),
                Name = cashDesk.Name,
                State = cashDesk.State,
                EncryptionKey = cashDesk.EncryptIv,
                MacAddress = cashDesk.MacAddress,
                Balance = cashDesk.Balance,
                Type = cashDesk.Type,
                Restrictions = cashDesk.Restrictions
            };
        }

        public static CashDesk MapToCashDesk(this CashDeskModel cashDesk, double timeZone)
        {
            return new CashDesk
            {
                Id = cashDesk.Id,
                BetShopId = cashDesk.BetShopId,
                CreationTime = cashDesk.CreationTime.GetGMTDateFromUTC(timeZone),
                LastUpdateTime = cashDesk.LastUpdateTime.GetGMTDateFromUTC(timeZone),
                Name = cashDesk.Name,
                State = cashDesk.State,
                EncryptIv = cashDesk.EncryptionKey,
                EncryptPassword = (string.IsNullOrEmpty(cashDesk.EncryptionKey) || cashDesk.EncryptionKey.Length != 16) ? string.Empty : cashDesk.EncryptionKey.Substring(0, 8),
                EncryptSalt = (string.IsNullOrEmpty(cashDesk.EncryptionKey) || cashDesk.EncryptionKey.Length != 16) ? string.Empty : cashDesk.EncryptionKey.Substring(8, 8),
                MacAddress = cashDesk.MacAddress,
                CurrentCashierId = 1,
                Type = (cashDesk.Type <= 0 || cashDesk.Type > 3 ? (int)CashDeskTypes.Cashier : cashDesk.Type),
                Restrictions = cashDesk.Restrictions
            };
        }

        public static List<CashdeskTransactionsReportModel> MapToCashdeskTransactionsReportModels(this IEnumerable<CashdeskTransactionsReport> reports, double timeZone)
        {
            return reports.Select(x => x.MapToCashdeskTransactionsReportModel(timeZone)).ToList();
        }

        public static CashdeskTransactionsReportModel MapToCashdeskTransactionsReportModel(this CashdeskTransactionsReport report, double timeZone)
        {
            return new CashdeskTransactionsReportModel
            {
                Totals = report.Totals == null ? null : report.Totals.MapCashdeskTransactionsReportTotalsModel(),
                Entities = report.Entities == null ? null : report.Entities.MapToCashDeskTransactionModels(timeZone),
                Count = report.Count
            };
        }

        public static List<CashDeskTransactionModel> MapToCashDeskTransactionModels(this IEnumerable<fnCashDeskTransaction> cashDesks, double timeZone)
        {
            return cashDesks.Select(x => x.MapToCashDeskTransactionModel(timeZone)).ToList();
        }

        public static CashDeskTransactionModel MapToCashDeskTransactionModel(this fnCashDeskTransaction transaction, double timeZone)
        {
            return new CashDeskTransactionModel
            {
                Id = transaction.Id,
                ExternalTransactionId = transaction.ExternalTransactionId,
                Amount = transaction.Amount,
                CurrencyId = transaction.CurrencyId,
                State = transaction.State,
                Info = transaction.Info,
                Creator = transaction.Creator,
                CashDeskId = transaction.CashDeskId,
                TicketNumber = transaction.TicketNumber,
                TicketInfo = transaction.TicketInfo,
                CreationTime = transaction.CreationTime.GetGMTDateFromUTC(timeZone),
                OperationTypeName = transaction.OperationTypeName,
                CashDeskName = transaction.CashDeskName,
                BetShopName = transaction.BetShopName,
                BetShopId = transaction.BetShopId,
                CashierId = transaction.CashierId,
                PartnerId = transaction.PartnerId,
                OperationTypeId = transaction.OperationTypeId
            };
        }

        public static CashdeskTransactionsReportTotalsModel MapCashdeskTransactionsReportTotalsModel(
            this CashdeskTransactionsReportTotals reportTotals)
        {
            return new CashdeskTransactionsReportTotalsModel
            {
                CurrencyId = reportTotals.CurrencyId,
                OperationTypeId = reportTotals.OperationTypeId,
                OperationTypeName = reportTotals.OperationTypeName,
                Total = reportTotals.Total
            };
        }

        public static List<CashdeskTransactionsReportTotalsModel> MapCashdeskTransactionsReportTotalsModel(
            this IEnumerable<CashdeskTransactionsReportTotals> reportsTotals)
        {
            return reportsTotals.Select(MapCashdeskTransactionsReportTotalsModel).ToList();
        }
        #endregion

        #region BetshopGroup

        public static BetshopGroupModel MapToBetshopGroupModel(this BetShopGroup betShopGroup, double timeZone)
        {
            return new BetshopGroupModel
            {
                Id = betShopGroup.Id,
                CreationTime = betShopGroup.CreationTime.GetGMTDateFromUTC(timeZone),
                LastUpdateTime = betShopGroup.LastUpdateTime.GetGMTDateFromUTC(timeZone),
                Name = betShopGroup.Name,
                ParentId = betShopGroup.ParentId,
                PartnerId = betShopGroup.PartnerId,
                IsLeaf = betShopGroup.IsLeaf,
                Path = betShopGroup.Path,
                State = betShopGroup.State,
                MaxCopyCount = betShopGroup.MaxCopyCount,
                MaxWinAmount = betShopGroup.MaxWinAmount,
                MinBetAmount = betShopGroup.MinBetAmount,
                MaxEventCountPerTicket = betShopGroup.MaxEventCountPerTicket,
                CommissionType = betShopGroup.CommissionType,
                CommissionRate = betShopGroup.CommissionRate,
                AnonymousBet = betShopGroup.AnonymousBet,
                AllowCashout = betShopGroup.AllowCashout,
                AllowLive = betShopGroup.AllowLive,
                UsePin = betShopGroup.UsePin
            };
        }

        public static List<BetShopGroup> MapToBetShopGroups(this IEnumerable<BetshopGroupModel> betshopGroupModels, double timeZone)
        {
            return betshopGroupModels.Select(x => x.MapToBetShopGroup(timeZone)).ToList();
        }

        public static BetShopGroup MapToBetShopGroup(this BetshopGroupModel betShopGroup, double timeZone)
        {
            return new BetShopGroup
            {
                Id = betShopGroup.Id,
                CreationTime = betShopGroup.CreationTime.GetGMTDateFromUTC(timeZone),
                LastUpdateTime = betShopGroup.LastUpdateTime.GetGMTDateFromUTC(timeZone),
                IsLeaf = betShopGroup.IsLeaf,
                Name = betShopGroup.Name,
                ParentId = betShopGroup.ParentId,
                PartnerId = betShopGroup.PartnerId,
                Path = betShopGroup.Path,
                State = betShopGroup.State,
                MaxCopyCount = betShopGroup.MaxCopyCount,
                MaxWinAmount = betShopGroup.MaxWinAmount,
                MinBetAmount = betShopGroup.MinBetAmount,
                MaxEventCountPerTicket = betShopGroup.MaxEventCountPerTicket,
                CommissionType = betShopGroup.CommissionType,
                CommissionRate = betShopGroup.CommissionRate,
                AnonymousBet = betShopGroup.AnonymousBet,
                AllowCashout = betShopGroup.AllowCashout,
                AllowLive = betShopGroup.AllowLive,
                UsePin = betShopGroup.UsePin
            };
        }

        #endregion

        #region Translation

        public static fnTranslation MapToTranslation(this TranslationModel translationModel)
        {
            return new fnTranslation
            {
                TranslationId = translationModel.TranslationId,
                ObjectTypeId = translationModel.ObjectTypeId,

            };
        }

        public static List<fnTranslation> MapToTranslations(this IEnumerable<TranslationModel> translationModels)
        {
            return translationModels.Select(MapToTranslation).ToList();
        }

        public static TranslationModel MapToTranslationModel(this fnTranslation translation)
        {
            return new TranslationModel
            {
                TranslationId = translation.TranslationId,
                ObjectTypeId = translation.ObjectTypeId,
            };
        }

        public static List<TranslationModel> MapToTranslationModels(this IEnumerable<fnTranslationEntry> users)
        {
            return users.Select(MapToTranslationModel).ToList();
        }

        public static TranslationModel MapToTranslationModel(this fnTranslationEntry translation)
        {
            return new TranslationModel
            {
                TranslationId = translation.TranslationId,
                ObjectTypeId = translation.ObjectTypeId
            };
        }

        public static List<TranslationModel> MapToTranslationModels(this IEnumerable<fnTranslation> users)
        {
            return users.Select(MapToTranslationModel).ToList();
        }

        #endregion

        #region ObjectType

        public static List<ObjectTypeModel> MapToObjectTypeModels(this IEnumerable<ObjectType> objectTypes)
        {
            return objectTypes.Select(MapToObjectTypeModel).ToList();
        }

        public static ObjectTypeModel MapToObjectTypeModel(this ObjectType objectType)
        {
            return new ObjectTypeModel
            {
                Id = objectType.Id,
                Name = objectType.Name,
                SaveChangeHistory = objectType.SaveChangeHistory,
                HasTranslation = objectType.HasTranslation
            };
        }

        public static ObjectType MapToObjectType(this ObjectTypeModel objectTypeModel)
        {
            return new ObjectType
            {
                Id = objectTypeModel.Id,
                Name = objectTypeModel.Name,
                SaveChangeHistory = objectTypeModel.SaveChangeHistory
            };
        }

        public static List<ObjectType> MapToUser(this IEnumerable<ObjectTypeModel> objectTypeModels)
        {
            return objectTypeModels.Select(MapToObjectType).ToList();
        }

        #endregion

        #region TranslationEntry

        public static TranslationEntryModel MapToTranslationEntryModel(this fnTranslationEntry translationEntry)
        {
            return new TranslationEntryModel
            {
                TranslationId = translationEntry.TranslationId,
                ObjectTypeId = translationEntry.ObjectTypeId,
                LanguageId = translationEntry.LanguageId,
                Text = translationEntry.Text
            };
        }

        public static List<TranslationEntryModel> MapToTranslationEntryModels(this IEnumerable<fnTranslationEntry> translationEntries)
        {
            return translationEntries.Select(MapToTranslationEntryModel).ToList();
        }

        public static List<fnTranslation> MapToTranslationEntries(this IEnumerable<TranslationEntryModel> translationEntryModels)
        {
            return translationEntryModels.Select(x => x.MapToTranslationEntry()).ToList();
        }

        public static fnTranslation MapToTranslationEntry(this TranslationEntryModel translationEntryModel)
        {
            return new fnTranslation
            {
                TranslationId = translationEntryModel.TranslationId,
                ObjectTypeId = translationEntryModel.ObjectTypeId,
                LanguageId = translationEntryModel.LanguageId,
                Text = translationEntryModel.Text
            };
        }

        #endregion

        #region ClientMessage

        public static TicketMessageModel MapToTicketMessageModel(this TicketMessage ticketMessage, double timeZone)
        {
            return new TicketMessageModel
            {
                Id = ticketMessage.Id,
                Message = ticketMessage.Message,
                Type = ticketMessage.Type,
                CreationTime = ticketMessage.CreationTime.AddHours(timeZone),
                TicketId = ticketMessage.TicketId,
                UserId = ticketMessage.UserId,
                UserFirstName = ticketMessage.User == null ? string.Empty : ticketMessage.User.FirstName,
                UserLastName = ticketMessage.User == null ? string.Empty : ticketMessage.User.LastName
            };
        }

        public static List<TicketModel> MapToTickets(this IEnumerable<fnTicket> tickets, double timeZone, string languageId)
        {
            var statuses = CacheManager.GetEnumerations(Constants.EnumerationTypes.MessageTicketState, languageId);
            WebApiApplication.DbLogger.Info(Constants.EnumerationTypes.MessageTicketState + "_" + languageId + JsonConvert.SerializeObject(statuses));
            var types = CacheManager.GetEnumerations(Constants.EnumerationTypes.TicketTypes, languageId);
            WebApiApplication.DbLogger.Info(Constants.EnumerationTypes.TicketTypes + "_" + languageId + JsonConvert.SerializeObject(types));

            return tickets.Where(x => x.Type != (int)TicketTypes.Email && x.Type != (int)TicketTypes.Sms)
                          .Select(x => x.MapToTicketeModel(timeZone, statuses, types)).ToList();
        }

        public static TicketModel MapToTicketeModel(this fnTicket ticket, double timeZone, List<BllFnEnumeration> statuses, List<BllFnEnumeration> types)
        {
            return new TicketModel
            {
                Id = ticket.Id,
                ClientId = ticket.ClientId,
                PartnerId = ticket.PartnerId,
                Status = ticket.Status,
                Subject = ticket.Subject,
                Type = ticket.Type,
                CreationTime = ticket.CreationTime.AddHours(timeZone),
                LastMessageTime = ticket.LastMessageTime.AddHours(timeZone),
                UnreadMessagesCount = ticket.UserUnreadMessagesCount ?? 0,
                UserName = ticket.UserName,
                StatusName = statuses.FirstOrDefault(x => x.Value == ticket.Status)?.Text,
                TypeName = types.FirstOrDefault(x => x.Value == ticket.Type)?.Text,
                UserId = ticket.UserId,
                UserFirstName = ticket.UserFirstName,
                UserLastName = ticket.UserLastName
            };
        }

        public static FilterTicket MapToFilterTicket(this ApiFilterTicket filterTicket)
        {
            return new FilterTicket
            {
                PartnerId = filterTicket.PartnerId,
                Ids = filterTicket.Ids == null ? new FiltersOperation() : filterTicket.Ids.MapToFiltersOperation(),
                ClientIds = filterTicket.ClientIds == null ? new FiltersOperation() : filterTicket.ClientIds.MapToFiltersOperation(),
                PartnerIds = filterTicket.PartnerIds == null ? new FiltersOperation() : filterTicket.PartnerIds.MapToFiltersOperation(),
                PartnerNames = filterTicket.PartnerNames == null ? new FiltersOperation() : filterTicket.PartnerNames.MapToFiltersOperation(),
                Subjects = filterTicket.Subjects == null ? new FiltersOperation() : filterTicket.Subjects.MapToFiltersOperation(),
                UserNames = filterTicket.UserNames == null ? new FiltersOperation() : filterTicket.UserNames.MapToFiltersOperation(),
                UserIds = filterTicket.UserIds == null ? new FiltersOperation() : filterTicket.UserIds.MapToFiltersOperation(),
                UserFirstNames = filterTicket.UserFirstNames == null ? new FiltersOperation() : filterTicket.UserFirstNames.MapToFiltersOperation(),
                UserLastNames = filterTicket.UserLastNames == null ? new FiltersOperation() : filterTicket.UserLastNames.MapToFiltersOperation(),
                Statuses = filterTicket.Statuses == null ? new FiltersOperation() : filterTicket.Statuses.MapToFiltersOperation(),
                Types = filterTicket.Types,
                State = filterTicket.State,
                UnreadsOnly = filterTicket.UnreadsOnly,
                CreatedBefore = filterTicket.CreatedBefore,
                CreatedFrom = filterTicket.CreatedFrom,
                TakeCount = filterTicket.TakeCount,
                SkipCount = filterTicket.SkipCount,
                OrderBy = filterTicket.OrderBy,
                FieldNameToOrderBy = filterTicket.FieldNameToOrderBy
            };
        }

        public static ApiTicketMessage ToApiTicketMessage(this TicketMessage ticketMessage, double timeZone)
        {
            return new ApiTicketMessage
            {
                Id = ticketMessage.Id,
                Message = ticketMessage.Message,
                Type = ticketMessage.Type,
                CreationTime = ticketMessage.CreationTime.AddHours(timeZone),
                TicketId = ticketMessage.TicketId,
                UserId = ticketMessage.UserId,
                UserFirstName = ticketMessage.User == null ? string.Empty : ticketMessage.User.FirstName,
                UserLastName = ticketMessage.User == null ? string.Empty : ticketMessage.User.LastName
            };
        }

        public static FilterClientMessage MapToFilterClientMessage(this ApiFilterClientMessage filterClientMessage)
        {
            return new FilterClientMessage
            {
                Ids = filterClientMessage.Ids == null ? new FiltersOperation() : filterClientMessage.Ids.MapToFiltersOperation(),
                ClientIds = filterClientMessage.ClientIds == null ? new FiltersOperation() : filterClientMessage.ClientIds.MapToFiltersOperation(),
                PartnerIds = filterClientMessage.PartnerIds == null ? new FiltersOperation() : filterClientMessage.PartnerIds.MapToFiltersOperation(),
                Subjects = filterClientMessage.Subjects == null ? new FiltersOperation() : filterClientMessage.Subjects.MapToFiltersOperation(),
                UserNames = filterClientMessage.UserNames == null ? new FiltersOperation() : filterClientMessage.UserNames.MapToFiltersOperation(),
                Statuses = filterClientMessage.Statuses == null ? new FiltersOperation() : filterClientMessage.Statuses.MapToFiltersOperation(),
                MobileOrEmails = filterClientMessage.MobileOrEmails == null ? new FiltersOperation() : filterClientMessage.MobileOrEmails.MapToFiltersOperation(),
                CreatedBefore = filterClientMessage.CreatedBefore,
                CreatedFrom = filterClientMessage.CreatedFrom,
                TakeCount = filterClientMessage.TakeCount,
                SkipCount = filterClientMessage.SkipCount,
                OrderBy = filterClientMessage.OrderBy,
                FieldNameToOrderBy = filterClientMessage.FieldNameToOrderBy
            };
        }

        public static List<ClientMessageModel> MapToClientMessage(this IEnumerable<fnClientMessage> clientMessages, double timeZone)
        {
            return clientMessages.Select(x => x.MapToClientMessageModel(timeZone)).ToList();
        }

        public static ClientMessageModel MapToClientMessageModel(this fnClientMessage clientMessage, double timeZone)
        {
            return new ClientMessageModel
            {
                Id = clientMessage.Id,
                ClientId = clientMessage.ClientId ?? 0,
                PartnerId = clientMessage.PartnerId,
                UserName = clientMessage.UserName,
                Subject = clientMessage.Subject,
                Message = Constants.ClientInfoSecuredTypes.Contains(clientMessage.MessageType) ? string.Empty : clientMessage.Message,
                Type = clientMessage.MessageType,
                Status = clientMessage.Status,
                MobileOrEmail = clientMessage.MobileOrEmail,
                CreationTime = clientMessage.CreationTime.AddHours(timeZone)
            };
        }

        public static List<ApiClientLog> MapToApiClientLogs(this IEnumerable<fnClientLog> objectTypeModels, double timeZone)
        {
            return objectTypeModels.Select(x => x.MapToApiClientLog(timeZone)).ToList();
        }

        public static ApiClientLog MapToApiClientLog(this fnClientLog clientMessage, double timeZone)
        {
            return new ApiClientLog
            {
                Id = clientMessage.Id,
                Action = clientMessage.Action,
                ClientId = clientMessage.ClientId,
                UserId = clientMessage.UserId,
                UserFirstName = clientMessage.UserFirstName,
                UserLastName = clientMessage.UserLastName,
                Ip = clientMessage.Ip,
                Page = clientMessage.Page,
                CreationTime = clientMessage.CreationTime.GetGMTDateFromUTC(timeZone),
                SessionId = clientMessage.ClientSessionId
            };
        }

        #endregion

        #region CLIENT DOCUMENT DEBIT OR CREDIT
        public static ApiDocument MapToApiDocumentModel(this fnDocument document, double timeZone, string languageId)
        {
            var operationTypes = CacheManager.GetEnumerations(Constants.EnumerationTypes.OperationTypes, languageId).ToDictionary(x => x.Value, x => x.Text);
            return new ApiDocument
            {
                Id = document.Id,
                ClientId = document.ClientId ?? 0,
                Amount = Math.Floor(document.Amount * 100) / 100,
                ConvertedAmount = Math.Floor(document.ConvertedAmount * 100) / 100,
                CurrencyId = document.CurrencyId,
                State = document.State,
                OperationTypeId = document.OperationTypeId,
                OperationType = operationTypes.ContainsKey(document.OperationTypeId) ? operationTypes[document.OperationTypeId] : string.Empty,
                ExternalTransactionId = document.ExternalTransactionId,
                TypeId = document.TypeId,
                TypeName = document.TypeId.HasValue && operationTypes.ContainsKey(document.TypeId.Value) ? operationTypes[document.TypeId.Value] : string.Empty,
                PaymentRequestId = document.PaymentRequestId,
                PaymentSystemId = document.PaymentSystemId,
                PaymentSystemName = document.PaymentSystemName,
                RoundId = document.RoundId,
                GameProviderId = document.GameProviderId,
                GameProviderName = document.GameProviderName,
                ProductId = document.ProductId,
                ProductName = document.ProductName,
                CreationTime = document.CreationTime.GetGMTDateFromUTC(timeZone),
                LastUpdateTime = document.LastUpdateTime.GetGMTDateFromUTC(timeZone)
            };
        }

        public static List<DocumentModel> MapToDocumentModels(this IEnumerable<Document> documents, double timeZone)
        {
            return documents.Select(x => x.MapToDocumentModel(timeZone)).ToList();
        }

        public static DocumentModel MapToDocumentModel(this Document document, double timeZone)
        {
            return new DocumentModel
            {
                Id = document.Id,
                Amount = Math.Floor(document.Amount * 100) / 100,
                CurrencyId = document.CurrencyId,
                State = document.State,
                OperationTypeId = document.OperationTypeId,
                Info = document.Info,
                ClientId = document.ClientId,
                UserId = document.UserId,
                CreationTime = document.CreationTime.GetGMTDateFromUTC(timeZone),
                LastUpdateTime = document.LastUpdateTime.GetGMTDateFromUTC(timeZone),
                Creator = document.Creator,
            };
        }

        #endregion

        #region Accounts

        public static FnAccountModel ToFnAccountModel(this fnAccount account)
        {
            return new FnAccountModel
            {
                Id = account.Id,
                ObjectTypeId = account.ObjectTypeId,
                TypeId = account.TypeId,
                Balance = account.Balance,
                CurrencyId = account.CurrencyId,
                AccountTypeName = account.PaymentSystemId != null ? account.PaymentSystemName + " Wallet - " + account.BetShopName :
                    account.BetShopId != null ? "Shop Wallet - " + account.BetShopName : account.AccountTypeName, //make translatable later
                BetShopId = account.BetShopId,
                PaymentSystemId = account.PaymentSystemId,
                PaymentSystemName = account.PaymentSystemName,
                Status = account.Status == null ? (int)BaseStates.Active : account.Status.Value
            };
        }

        public static FnAccountModel ToFnAccountModel(this Account account)
        {
            return new FnAccountModel
            {
                Id = account.Id,
                ObjectTypeId = account.ObjectTypeId,
                TypeId = account.TypeId,
                Balance = account.Balance,
                CurrencyId = account.CurrencyId,
                BetShopId = account.BetShopId,
                PaymentSystemId = account.PaymentSystemId,
                Status = account.Status == null ? (int)BaseStates.Active : account.Status.Value
            };
        }

        public static Account ToAccount(this FnAccountModel account)
        {
            return new Account
            {
                Id = account.Id,
                ObjectTypeId = account.ObjectTypeId,
                TypeId = account.TypeId,
                Balance = account.Balance,
                CurrencyId = account.CurrencyId,
                BetShopId = account.BetShopId,
                PaymentSystemId = account.PaymentSystemId,
                Status = account.Status
            };
        }

        #endregion

        #region CRM
        public static List<ApiClient> MapToApiClientList(this IEnumerable<fnClient> clients, double timeZone, string language)
        {
            return clients.Select(x => x.MapTofApiClientItem(timeZone, language)).ToList();
        }

        public static ApiClient MapTofApiClientItem(this fnClient client, double timeZone, string language)
        {
            var region = CacheManager.GetRegionById(client.RegionId, language);
            var compBalance = CacheManager.GetClientCurrentBalance(client.Id).Balances
                .Where(x => x.TypeId == (int)AccountTypes.ClientCompBalance).FirstOrDefault();
            return new ApiClient
            {
                Id = client.Id,
                UserName = client.UserName,
                FirstName = client.FirstName,
                LastName = client.LastName,
                NickName = client.NickName,
                Email = client.Email,
                IsEmailVerified = client.IsEmailVerified,
                CurrencyId = client.CurrencyId,
                Gender = client.Gender,
                BirthDate = (long)client.BirthDate.GetGMTDateFromUTC(timeZone).Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds,
                Address = client.Address,
                MobileNumber = client.MobileNumber,
                IsMobileNumberVerified = client.IsMobileNumberVerified,
                AffiliatePlatformId = client.AffiliatePlatformId,
                AffiliateId = client.AffiliateId,
                AffiliateReferralId = client.AffiliateReferralId,
                IsBonusEligible = true, // ???
                CategoryId = client.CategoryId,
                State = client.State,
                IsBanned = client.State == (int)ClientStates.Active,
                ZipCode = client.ZipCode,
                CountryName = region.Name,
                CountryCode = region.IsoCode,
                CreationTime = (long)client.CreationTime.GetGMTDateFromUTC(timeZone).Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds,
                LastUpdateTime = (long)client.LastUpdateTime.GetGMTDateFromUTC(timeZone).Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds,
                RealBalance = Math.Floor((client.RealBalance ?? 0) * 100) / 100,
                BonusBalance = Math.Floor((client.BonusBalance ?? 0) * 100) / 100,
                CompBalance = compBalance != null ? Math.Truncate(compBalance.Balance) : 0,
                FirstDepositDate = client.FirstDepositDate.HasValue ?
                (long)client.FirstDepositDate.Value.GetGMTDateFromUTC(timeZone).Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds : (long?)null,
                LastDepositDate = client.LastDepositDate.HasValue ?
                (long)client.LastDepositDate.Value.GetGMTDateFromUTC(timeZone).Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds : (long?)null,
                LastLoginDate = client.LastSessionDate.HasValue ?
                (long)client.LastSessionDate.Value.GetGMTDateFromUTC(timeZone).Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds : (long?)null,
            };
        }
        #endregion

        #region fnClient

        public static fnClientModel MapTofnClientModelItem(this fnClient arg, double timeZone, bool hideClientContactInfo, string languageId)
        {
            var parentState = CacheManager.GetClientSettingByName(arg.Id, ClientSettings.ParentState);
            if (parentState.NumericValue.HasValue && CustomHelper.Greater((ClientStates)parentState.NumericValue.Value, (ClientStates)arg.State))
                arg.State = Convert.ToInt32(parentState.NumericValue.Value);
            var regionPath = CacheManager.GetRegionPathById(arg.RegionId);
            var cityId = regionPath.FirstOrDefault(x => x.TypeId == (int)RegionTypes.City)?.Id;
            var stateId = regionPath.FirstOrDefault(x => x.TypeId == (int)RegionTypes.State)?.Id;
            if (!stateId.HasValue)
                stateId = regionPath.FirstOrDefault(x => x.TypeId == (int)RegionTypes.Country)?.Id;
            return new fnClientModel
            {
                Id = arg.Id,
                Email = !hideClientContactInfo ? arg.Email : "*****",
                IsEmailVerified = arg.IsEmailVerified,
                CurrencyId = arg.CurrencyId,
                UserName = arg.UserName,
                PartnerId = arg.PartnerId,
                PartnerName = CacheManager.GetPartnerById(arg.PartnerId).Name,
                Gender = arg.Gender,
                GenderName = Enum.GetName(typeof(Gender), arg.Gender),
                BirthDate = arg.BirthDate != DateTime.MinValue ? arg.BirthDate.ToString() : string.Empty,
                Age = arg.BirthDate != DateTime.MinValue ? arg.Age ?? 0 : 0,
                SendMail = arg.SendMail,
                SendSms = arg.SendSms,
                FirstName = arg.FirstName,
                LastName = arg.LastName,
                NickName = arg.NickName,
                SecondSurname = arg.SecondSurname,
                SecondName = arg.SecondName,
                RegionId = arg.RegionId,
                CountryId = arg.CountryId,
                CountryName = arg.CountryId.HasValue ? CacheManager.GetRegionById(arg.CountryId.Value, languageId)?.Name : null,
                RegistrationIp = arg.RegistrationIp,
                DocumentNumber = arg.DocumentNumber,
                DocumentType = arg.DocumentType,
                DocumentIssuedBy = arg.DocumentIssuedBy,
                IsDocumentVerified = arg.IsDocumentVerified,
                Address = arg.Address,
                CountryState = stateId.HasValue ? CacheManager.GetRegionById(stateId.Value, languageId).Name : string.Empty,
                City = cityId.HasValue ? CacheManager.GetRegionById(cityId.Value, languageId).Name : arg.City,
                RegionIsoCode = arg.RegionIsoCode,
                MobileNumber = !hideClientContactInfo ? arg.MobileNumber : "*****",
                PhoneNumber = arg.PhoneNumber,
                IsMobileNumberVerified = arg.IsMobileNumberVerified,
                LanguageId = arg.LanguageId,
                CreationTime = arg.CreationTime.GetGMTDateFromUTC(timeZone),
                LastUpdateTime = arg.LastUpdateTime.GetGMTDateFromUTC(timeZone),
                LastSessionDate = arg.LastSessionDate?.GetGMTDateFromUTC(timeZone),
                CategoryId = arg.CategoryId,
                CategoryName = CacheManager.GetClientCategory(arg.CategoryId)?.Name,
                State = arg.State,
                StateName = Enum.GetName(typeof(ClientStates), arg.State),
                StateNickName = Enum.GetName(typeof(ClientStates), arg.State),
                CallToPhone = arg.CallToPhone,
                SendPromotions = arg.SendPromotions,
                ZipCode = arg.ZipCode?.Trim(),
                HasNote = arg.HasNote,
                RealBalance = Math.Floor((arg.RealBalance ?? 0) * 100) / 100,
                BonusBalance = Math.Floor((arg.BonusBalance ?? 0) * 100) / 100,
                Info = arg.Info,
                AffiliatePlatformId = arg.AffiliatePlatformId,
                AffiliateId = arg.AffiliateId,
                AffiliateReferralId = arg.AffiliateReferralId,
                UserId = arg.UserId,
                LastDepositDate = arg.LastDepositDate?.GetGMTDateFromUTC(timeZone),
                Title = arg.Title,
                UnderMonitoringTypes = !string.IsNullOrEmpty(arg.UnderMonitoringTypes) ? JsonConvert.DeserializeObject<List<int>>(arg.UnderMonitoringTypes) : null,
            };
        }

        public static List<ApiSegmentClient> MapTofnSegmentClientModelList(this IEnumerable<fnSegmentClient> arg, bool hideClientContactInfo, double timeZone)
        {
            return arg.Select(x => x.MapTofnSegmentClientModelItem(hideClientContactInfo, timeZone)).ToList();
        }

        public static ApiSegmentClient MapTofnSegmentClientModelItem(this fnSegmentClient arg, bool hideClientContactInfo, double timeZone)
        {
            return new ApiSegmentClient
            {
                Id = arg.Id,
                SegmentId = arg.SegmentId,
                Email = hideClientContactInfo ? "*****" : arg.Email,
                IsEmailVerified = arg.IsEmailVerified,
                CurrencyId = arg.CurrencyId,
                UserName = arg.UserName,
                PartnerId = arg.PartnerId,
                Gender = arg.Gender,
                BirthDate = arg.BirthDate.GetGMTDateFromUTC(timeZone),
                SendMail = arg.SendMail,
                SendSms = arg.SendSms,
                FirstName = arg.FirstName,
                LastName = arg.LastName,
                SecondSurname = arg.SecondSurname,
                SecondName = arg.SecondName,
                RegionId = arg.RegionId,
                RegistrationIp = arg.RegistrationIp,
                DocumentNumber = arg.DocumentNumber,
                DocumentType = arg.DocumentType,
                DocumentIssuedBy = arg.DocumentIssuedBy,
                IsDocumentVerified = arg.IsDocumentVerified,
                Address = arg.Address,
                MobileNumber = hideClientContactInfo ? "*****" : arg.MobileNumber,
                PhoneNumber = arg.PhoneNumber,
                IsMobileNumberVerified = arg.IsMobileNumberVerified,
                LanguageId = arg.LanguageId,
                CreationTime = arg.CreationTime.GetGMTDateFromUTC(timeZone),
                LastUpdateTime = arg.LastUpdateTime,
                CategoryId = arg.CategoryId,
                State = arg.State,
                CallToPhone = arg.CallToPhone,
                SendPromotions = arg.SendPromotions,
                ZipCode = arg.ZipCode,
                HasNote = arg.HasNote,
                Info = arg.Info,
                UserId = arg.UserId,
                AffiliatePlatformId = arg.AffiliatePlatformId,
                AffiliateId = arg.AffiliateId,
                AffiliateReferralId = arg.AffiliateReferralId,
                LastDepositDate = arg.LastDepositDate
            };
        }

        public static fnClientModel MapTofnClientModel(this Client client)
        {
            return new fnClientModel
            {
                Id = client.Id,
                Email = client.Email,
                IsEmailVerified = client.IsEmailVerified,
                CurrencyId = client.CurrencyId,
                UserName = client.UserName,
                PartnerId = client.PartnerId,
                Gender = client.Gender,
                BirthDate = client.BirthDate != DateTime.MinValue ? client.BirthDate.ToString() : string.Empty,
                SendMail = client.SendMail,
                SendSms = client.SendSms,
                FirstName = client.FirstName,
                LastName = client.LastName,
                NickName = client.NickName,
                RegionId = client.RegionId,
                CountryId = client.CountryId,
                RegistrationIp = client.RegistrationIp,
                DocumentNumber = client.DocumentNumber,
                DocumentType = client.DocumentType,
                DocumentIssuedBy = client.DocumentIssuedBy,
                IsDocumentVerified = client.IsDocumentVerified,
                Address = client.Address,
                MobileNumber = client.MobileNumber,
                PhoneNumber = client.PhoneNumber,
                IsMobileNumberVerified = client.IsMobileNumberVerified,
                LanguageId = client.LanguageId,
                CreationTime = client.CreationTime,
                LastUpdateTime = client.LastUpdateTime,
                CategoryId = client.CategoryId,
                State = client.State,
                CallToPhone = client.CallToPhone,
                SendPromotions = client.SendPromotions,
                ZipCode = client.ZipCode,
                HasNote = client.HasNote,
                Info = client.Info,
                RealBalance = 0,
                BonusBalance = 0,
                GGR = 0,
                NETGaming = 0
            };
        }


        public static ClientCategory MapToClientCategory(this ApiClientCategory apiClientCategory)
        {
            return new ClientCategory
            {
                Id = apiClientCategory.Id,
                NickName = apiClientCategory.NickName,
                Color = apiClientCategory.Color
            };
        }

        public static ApiClientCategory MapToApiClientCategory(this ClientCategory clientCategory)
        {
            return new ApiClientCategory
            {
                Id = clientCategory.Id,
                NickName = clientCategory.NickName,
                Color = clientCategory.Color,
                TranslationId = clientCategory.TranslationId
            };
        }
        #endregion

        #region Partner

        public static List<Partner> MapToPartners(this IEnumerable<PartnerModel> partners, double timeZone)
        {
            return partners.Select(x => x.MapToPartner(timeZone)).ToList();
        }

        public static Partner MapToPartner(this PartnerModel partner, double timeZone)
        {
            return new Partner
            {
                Id = partner.Id,
                Name = partner.Name,
                CurrencyId = partner.CurrencyId,
                SiteUrl = partner.SiteUrl,
                State = partner.State,
                CreationTime = partner.CreationTime.GetGMTDateFromUTC(timeZone),
                LastUpdateTime = partner.LastUpdateTime.GetGMTDateFromUTC(timeZone),
                AdminSiteUrl = partner.AdminSiteUrl
            };
        }

        public static List<PartnerModel> MapToPartnerModels(this IEnumerable<Partner> partners, double timeZone)
        {
            return partners.Select(x => x.MapToPartnerModel(timeZone)).ToList();
        }

        public static PartnerModel MapToPartnerModel(this Partner partner, double timeZone)
        {
            return new PartnerModel
            {
                Id = partner.Id,
                Name = partner.Name,
                CurrencyId = partner.CurrencyId,
                SiteUrl = partner.SiteUrl,
                State = partner.State,
                CreationTime = partner.CreationTime.GetGMTDateFromUTC(timeZone),
                LastUpdateTime = partner.LastUpdateTime.GetGMTDateFromUTC(timeZone),
                AdminSiteUrl = partner.AdminSiteUrl,
                AccountingDayStartTime = partner.AccountingDayStartTime,
                ClientMinAge = partner.ClientMinAge,
                PasswordRegExp = partner.PasswordRegExp,
                VerificationType = partner.VerificationType,
                EmailVerificationCodeLength = partner.EmailVerificationCodeLength,
                MobileVerificationCodeLength = partner.MobileVerificationCodeLength,
                UnusedAmountWithdrawPercent = partner.UnusedAmountWithdrawPercent,
                UserSessionExpireTime = partner.UserSessionExpireTime,
                UnpaidWinValidPeriod = partner.UnpaidWinValidPeriod,
                VerificationKeyActiveMinutes = partner.VerificationKeyActiveMinutes,
                AutoApproveBetShopDepositMaxAmount = partner.AutoApproveBetShopDepositMaxAmount,
                ClientSessionExpireTime = partner.ClientSessionExpireTime,
                AutoApproveWithdrawMaxAmount = partner.AutoApproveWithdrawMaxAmount,
                AutoConfirmWithdrawMaxAmount = partner.AutoConfirmWithdrawMaxAmount,
                PasswordRegExProperty = new Common.Models.RegExProperty(partner.PasswordRegExp)
            };
        }

        public static FilterEmail MapToFilterEmail(this ApiFilterEmail filterEmail)
        {
            return new FilterEmail
            {
                Ids = filterEmail.Ids == null ? new FiltersOperation() : filterEmail.Ids.MapToFiltersOperation(),
                PartnerIds = filterEmail.PartnerIds == null ? new FiltersOperation() : filterEmail.PartnerIds.MapToFiltersOperation(),
                Subjects = filterEmail.Subjects == null ? new FiltersOperation() : filterEmail.Subjects.MapToFiltersOperation(),
                Statuses = filterEmail.Statuses == null ? new FiltersOperation() : filterEmail.Statuses.MapToFiltersOperation(),
                Receiver = filterEmail.Receiver == null ? new FiltersOperation() : filterEmail.Receiver.MapToFiltersOperation(),
                CreatedBefore = filterEmail.CreatedBefore,
                CreatedFrom = filterEmail.CreatedFrom,
                TakeCount = filterEmail.TakeCount,
                SkipCount = filterEmail.SkipCount,
                OrderBy = filterEmail.OrderBy,
                FieldNameToOrderBy = filterEmail.FieldNameToOrderBy,
                ObjectId = filterEmail.ObjectId,
                ObjectTypeId = filterEmail.ObjectTypeId
            };
        }

        public static List<EmailModel> MapToEmail(this IEnumerable<Email> clientMessages, double timeZone)
        {
            return clientMessages.Select(x => x.MapToEmailModel(timeZone)).ToList();
        }

        public static EmailModel MapToEmailModel(this Email email, double timeZone)
        {
            return new EmailModel
            {
                Id = email.Id,
                PartnerId = email.PartnerId,
                Subject = email.Subject,
                Message = email.Body,
                Status = email.Status,
                Receiver = email.Receiver,
                CreationTime = email.CreationTime,
                ObjectId = email.ObjectId,
                ObjectTypeId = email.ObjectTypeId
            };
        }

        #endregion

        #region PaymentRequestHistory

        public static List<PaymentRequestHistoryModel> MapToPaymentRequestHistoryModels(this IEnumerable<PaymentRequestHistoryElement> elements, double timeZone)
        {
            return elements.Select(x => x.MapToPaymentRequestHistoryModel(timeZone)).ToList();
        }

        public static PaymentRequestHistoryModel MapToPaymentRequestHistoryModel(this PaymentRequestHistoryElement element, double timeZone)
        {
            return new PaymentRequestHistoryModel
            {
                Id = element.Id,
                RequestId = element.Id,
                Status = element.Status,
                Comment = element.Comment,
                CreationTime = element.CreationTime.GetGMTDateFromUTC(timeZone),
                FirstName = element.FirstName,
                LastName = element.LastName
            };
        }

        #endregion

        #region PaymentRequest

        public static ApiPaymentRequestsReport MapToApiPaymentRequestsReport(this PaymentRequestsReport request, double timeZone)
        {
            return new ApiPaymentRequestsReport
            {
                Entities = request.Entities.Select(x => x.MapToApiPaymentRequest(timeZone)).ToList(),
                Count = request.Count,
                TotalAmount = request.TotalAmount == null ? 0 : Math.Floor(request.TotalAmount.Value * 100) / 100,
                TotalUniquePlayers = request.TotalUniquePlayers
            };
        }

        public static ApiPaymentRequest MapToApiPaymentRequest(this fnPaymentRequest request, double timeZone)
        {
            var info = string.IsNullOrEmpty(request.Info) ? null : JsonConvert.DeserializeObject<Common.Models.PaymentInfo>(request.Info);
            var cardType = string.Empty;
            if (!string.IsNullOrEmpty(info?.CardNumber))
                cardType = info.CardNumber.StartsWith("4") ? "VISA" : info.CardNumber.StartsWith("5") ? "MC" :
                                              info.CardNumber.StartsWith("3") ? "AMEX" : "undefined";
            var resp = new ApiPaymentRequest
            {
                Id = request.Id,
                PartnerId = request.PartnerId ?? 0,
                ClientId = request.ClientId,
                Amount = request.Amount,
                CurrencyId = request.CurrencyId,
                State = request.Status,
                PaymentStatus = Enum.GetName(typeof(PaymentRequestStates), request.Status),
                Type = request.Type,
                BetShopId = request.BetShopId,
                Barcode = request.Barcode,
                BetShopName = request.BetShopName,
                BetShopAddress = request.BetShopAddress,
                PaymentSystemId = request.PaymentSystemId,
                PaymentSystemName = request.PaymentSystemName,
                Info = info == null ? "{}" : JsonConvert.SerializeObject(info, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Ignore
                }),
                CreationTime = request.CreationTime.GetGMTDateFromUTC(timeZone),
                LastUpdateTime = request.LastUpdateTime.GetGMTDateFromUTC(timeZone),
                CashDeskId = request.CashDeskId,
                UserName = request.UserName,
                FirstName = request.FirstName,
                LastName = request.LastName,
                ClientDocumentNumber = request.ClientDocumentNumber,
                ClientHasNote = request.ClientHasNote ?? false,
                GroupId = request.CategoryId ?? 0,
                CreatorId = request.UserId,
                CreatorFirstName = request.UserFirstName,
                CreatorLastName = request.UserLastName,
                HasNote = request.HasNote == 1,
                CashCode = request.CashCode,
                ExternalId = request.ExternalTransactionId,
                Parameters = request.Parameters,
                AffiliatePlatformId = request.AffiliatePlatformId,
                AffiliateId = request.AffiliateId,
                ActivatedBonusType = request.ActivatedBonusType,
                CommissionPercent = request.CommissionAmount.HasValue && request.Amount > 0 ?
                                    Math.Round(request.CommissionAmount.Value * 100 / request.Amount, 2) : 0,
                CommissionAmount = request.CommissionAmount,
                TransactionIp = info?.TransactionIp,
                CardType = cardType,
                CardNumber = request.CardNumber,
                CountryCode = request.CountryCode,
                BankName = info != null ? info.BankName : string.Empty,
                SegmentId = request.SegmentId,
                SegmentName = request.SegmentName,
                ClientEmail = request.Email,
                DepositCount = request.DepositCount ?? 0,
                ParentId = request.ParentId
            };
            if (!string.IsNullOrEmpty(request.Parameters))
            {
                var prm = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Parameters);
                if (prm != null && prm.ContainsKey("PaymentForm"))
                    resp.PaymentForm = "ClientPaymentForms/" + prm["PaymentForm"];
            }
            return resp;
        }

        #endregion

        #region PaymentSystem

        public static List<PaymentSystemModel> MapToPaymentSystemModels(this IEnumerable<PaymentSystem> paymentSystems, double timeZone)
        {
            return paymentSystems.Select(x => x.MapToPaymentSystemModel(timeZone)).ToList();
        }

        public static PaymentSystemModel MapToPaymentSystemModel(this PaymentSystem paymentSystem, double timeZone)
        {
            return new PaymentSystemModel
            {
                Id = paymentSystem.Id,
                Name = paymentSystem.Name,
                Type = paymentSystem.Type,
                ContentType = paymentSystem.ContentType,
                PeriodicityOfRequest = paymentSystem.PeriodicityOfRequest,
                PaymentRequestSendCount = paymentSystem.PaymentRequestSendCount,
                CreationTime = paymentSystem.CreationTime.GetGMTDateFromUTC(timeZone),
                LastUpdateTime = paymentSystem.LastUpdateTime.GetGMTDateFromUTC(timeZone)
            };
        }
        public static ApiPartnerPaymentCurrencyRate MapToApiPartnerPaymentCurrencyRate(this PartnerPaymentCurrencyRate paymentCurrencyRate)
        {
            var rate = paymentCurrencyRate.Rate != 0m ? 1 / paymentCurrencyRate.Rate : 0;
            return new ApiPartnerPaymentCurrencyRate
            {
                Id = paymentCurrencyRate.Id,
                PaymentSettingId = paymentCurrencyRate.PaymentSettingId,
                CurrencyId = paymentCurrencyRate.CurrencyId,
                Rate = rate > 10000 ? Math.Round(rate, 0) : Math.Round(rate, 2)
            };
        }

        public static PaymentSystem MapToPaymentSystem(this PaymentSystemModel paymentSystem)
        {
            return new PaymentSystem
            {
                Id = paymentSystem.Id,
                Name = paymentSystem.Name,
                Type = paymentSystem.Type,
                PeriodicityOfRequest = paymentSystem.PeriodicityOfRequest,
                PaymentRequestSendCount = paymentSystem.PaymentRequestSendCount,
                ContentType = paymentSystem.ContentType
            };
        }

        public static List<PaymentSystem> MapToPaymentSystems(this IEnumerable<PaymentSystemModel> paymentSystems)
        {
            return paymentSystems.Select(MapToPaymentSystem).ToList();
        }

        #endregion

        #region PERMISSIONS WITH ROLES

        public static RoleModel MapToRoleModel(this Role role)
        {
            return new RoleModel
            {
                Id = role.Id,
                Name = role.Name,
                Comment = role.Comment,
                IsAdmin = role.IsAdmin,
                PartnerId = role.PartnerId,
                RolePermissions = role.RolePermissions.MapToRolePermissionModels()
            };
        }

        public static List<RoleModel> MapToRoleModels(this IEnumerable<Role> roles)
        {
            return roles.Select(MapToRoleModel).ToList();
        }

        public static Role MapToRole(this RoleModel roleModel)
        {
            return new Role
            {
                Id = roleModel.Id,
                Name = roleModel.Name,
                Comment = roleModel.Comment,
                IsAdmin = roleModel.IsAdmin,
                PartnerId = roleModel.PartnerId,
                RolePermissions = roleModel.RolePermissions.MapToRolePermissions()
            };
        }

        public static List<Role> MapToRoles(this IEnumerable<RoleModel> roleModels)
        {
            return roleModels.Select(MapToRole).ToList();
        }



        public static List<ApiPermissionModel> MapToRolePermissionModels(this ICollection<RolePermission> rolePermissions)
        {
            return rolePermissions.Select(MapToRolePermissionModel).ToList();
        }

        public static ApiPermissionModel MapToRolePermissionModel(this RolePermission rolePermission)
        {
            return new ApiPermissionModel
            {
                Id = rolePermission.Id,
                IsForAll = rolePermission.IsForAll,
                Permissionid = rolePermission.PermissionId,
                RoleId = rolePermission.RoleId,
                AccessObjectsIds = rolePermission.Permission != null ? rolePermission.Permission.AccessObjects.Select(a => a.ObjectId).ToList() : null
            };
        }

        public static List<ApiPermissionModel> MapToRolePermissionModels(this IEnumerable<RolePermissionModel> rolePermissions)
        {
            return rolePermissions.Select(MapToRolePermissionModel).ToList();
        }

        public static ApiPermissionModel MapToRolePermissionModel(this RolePermissionModel rolePermission)
        {
            return new ApiPermissionModel
            {
                Id = rolePermission.Id,
                IsForAll = rolePermission.IsForAll,
                Permissionid = rolePermission.PermissionId,
                RoleId = rolePermission.RoleId,
                AccessObjectsIds = rolePermission.Permission != null ? rolePermission.Permission.AccessObjects.Select(a => a.ObjectId).ToList() : null
            };
        }

        public static List<RolePermission> MapToRolePermissions(this IEnumerable<RolePermissionModel> rolePermissions)
        {
            return rolePermissions.Select(MapToRolePermission).ToList();
        }

        public static RolePermission MapToRolePermission(this RolePermissionModel rolePermission)
        {
            return new RolePermission
            {
                Id = rolePermission.Id,
                IsForAll = rolePermission.IsForAll,
                RoleId = rolePermission.RoleId,
                PermissionId = rolePermission.PermissionId
            };
        }

        public static List<RolePermission> MapToRolePermissions(this List<ApiPermissionModel> rolePermissions)
        {
            return rolePermissions.Select(MapToRolePermission).ToList();
        }

        public static RolePermission MapToRolePermission(this ApiPermissionModel rolePermission)
        {
            return new RolePermission
            {
                Id = rolePermission.Id,
                IsForAll = rolePermission.IsForAll,
                RoleId = rolePermission.RoleId,
                PermissionId = rolePermission.Permissionid
            };
        }

        public static AccessObject MapToAccessObject(this AccessObjectModel accessObjectModel)
        {
            return new AccessObject
            {
                Id = accessObjectModel.Id,
                ObjectId = accessObjectModel.ObjectId,
                ObjectTypeId = accessObjectModel.ObjectTypeId,
                PermissionId = accessObjectModel.PermissionId,
                UserId = accessObjectModel.UserId
            };
        }

        public static List<AccessObject> MapToAccessObjects(this IEnumerable<AccessObjectModel> rolePermissions)
        {
            return rolePermissions.Select(MapToAccessObject).ToList();
        }

        public static AccessObjectModel MapToAccessObjectModel(this AccessObject accessObject)
        {
            return new AccessObjectModel
            {
                Id = accessObject.Id,
                ObjectId = accessObject.ObjectId,
                UserId = accessObject.UserId,
                ObjectTypeId = accessObject.ObjectTypeId,
                PermissionId = accessObject.PermissionId
            };
        }

        public static List<AccessObjectModel> MapToAccessObjectModels(this IEnumerable<AccessObject> accessObjects)
        {
            return accessObjects.Select(MapToAccessObjectModel).ToList();
        }

        public static List<PermissionModel> MapToPermissionModels(this List<BllPermission> permissions)
        {
            return permissions.Select(MapToPermissionModel).ToList();
        }

        public static PermissionModel MapToPermissionModel(this BllPermission permission)
        {
            return new PermissionModel
            {
                Id = permission.Id,
                Name = permission.Name,
                PermissionGroupId = permission.PermissionGroupId,
                ObjectTypeId = permission.ObjectTypeId
            };
        }

        #endregion

        #region fnProduct

        public static fnProduct MapTofnProduct(this ApiProduct product)
        {
            return new fnProduct
            {
                Id = product.Id,
                NewId = product.NewId,
                GameProviderId = product.GameProviderId,
                NickName = product.Description,
                ParentId = product.ParentId,
                Name = product.Name,
                ExternalId = product.ExternalId,
                State = product.State,
                IsForDesktop = product.IsForDesktop,
                IsForMobile = product.IsForMobile,
                SubproviderId = product.SubproviderId,
                WebImageUrl = product.WebImageUrl,
                MobileImageUrl = product.MobileImageUrl,
                BackgroundImageUrl = product.BackgroundImageUrl,
                HasDemo = product.HasDemo,
                FreeSpinSupport = product.FreeSpinSupport,
                CategoryId = product.CategoryId,
                RTP = product.RTP,
                ProductCountrySettings = product.Countries?.Ids?.Select(x => new ProductCountrySetting
                {
                    CountryId = x,
                    Type = product.Countries.Type ?? (int)ProductCountrySettingTypes.Restricted
                }).ToList(),
                BackgroundImage = product.BackgroundImage,
                MobileImage = product.MobileImage,
                WebImage = product.WebImage
            };
        }

        public static FnProductModel MapTofnProductModel(this fnProduct product, double timeZone)
        {
            return new FnProductModel
            {
                Id = product.Id,
                GameProviderId = product.GameProviderId,
                GameProviderName = product.GameProviderName,
                PaymentSystemId = product.PaymentSystemId,
                Description = product.NickName,
                ParentId = product.ParentId,
                Name = product.Name,
                Level = product.Level,
                IsLeaf = product.IsLeaf,
                IsLastProductGroup = product.IsLastProductGroup,
                TranslationId = product.TranslationId,
                ExternalId = product.ExternalId,
                State = product.State,
                IsForDesktop = product.IsForDesktop,
                IsForMobile = product.IsForMobile,
                SubproviderId = product.SubproviderId,
                WebImageUrl = product.WebImageUrl,
                MobileImageUrl = product.MobileImageUrl,
                BackgroundImageUrl = product.BackgroundImageUrl,
                HasDemo = product.HasDemo,
                Jackpot = product.Jackpot,
                FreeSpinSupport = product.FreeSpinSupport,
                SubproviderName = product.SubproviderName,
                CategoryId = product.CategoryId,
                RTP = product.RTP,
                Lines = product.Lines,
                BetValues = product.BetValues,
                CreationTime = product.CreationTime.GetGMTDateFromUTC(timeZone),
                LastUpdateTime = product.LastUpdateTime.GetGMTDateFromUTC(timeZone),
                Countries = product.ProductCountrySettings != null && product.ProductCountrySettings.Any() ? new ApiSetting
                {
                    Type = product.ProductCountrySettings.First().Type,
                    Ids = product.ProductCountrySettings.Select(x => x.CountryId).ToList()
                } : new ApiSetting { Type = (int)ProductCountrySettingTypes.Restricted, Ids = new List<int>() },
            };
        }

        #endregion

        #region fnOnlineClient

        public static fnOnlineClientModel MapToFnOnlineClientModel(this fnOnlineClientModel clientModel)
        {
            return new fnOnlineClientModel
            {
                Id = clientModel.Id,
                FirstName = clientModel.FirstName,
                LastName = clientModel.LastName,
                UserName = clientModel.UserName,
                RegionId = clientModel.RegionId,
                CategoryId = clientModel.CategoryId,
                LoginIp = clientModel.LoginIp,
                SessionTime = clientModel.SessionTime,
                Balance = clientModel.Balance
            };
        }

        public static List<fnOnlineClientModel> MapToFnOnlineClientModels(this IEnumerable<fnOnlineClientModel> clientModels)
        {
            return clientModels.Select(MapToFnOnlineClientModel).ToList();
        }
        #endregion

        #region BetShopReconingModel

        public static BetShopReconingModel MapToBetShopReconingModel(this fnBetShopReconing reconing, double timeZone)
        {
            return new BetShopReconingModel
            {
                Id = reconing.Id,
                CurrencyId = reconing.CurrencyId,
                Amount = reconing.Amount,
                BetShopId = reconing.BetShopId,
                UserId = reconing.UserId,
                BetShopAvailiableBalance = reconing.BetShopAvailiableBalance,
                CreationTime = reconing.CreationTime.GetGMTDateFromUTC(timeZone)
            };
        }

        public static BetShopReconingModel MapToBetShopReconingModel(this BetShopReconing reconing, double timeZone)
        {
            return new BetShopReconingModel
            {
                Id = reconing.Id,
                CurrencyId = reconing.CurrencyId,
                Amount = reconing.Amount,
                BetShopId = reconing.BetShopId,
                UserId = reconing.UserId,
                BetShopAvailiableBalance = reconing.BetShopAvailiableBalance,
                CreationTime = reconing.CreationTime.GetGMTDateFromUTC(timeZone)
            };
        }

        public static List<BetShopReconingModel> MapToBetShopReconingModels(this IEnumerable<fnBetShopReconing> reconings, double timeZone)
        {
            return reconings.Select(x => x.MapToBetShopReconingModel(timeZone)).ToList();
        }
        #endregion

        #endregion

        #region PartnerPaymentSetting

        public static List<ApiPartnerBankInfo> MapToApiPartnerBankInfo(this IEnumerable<fnPartnerBankInfo> partnerBankInfos, double timeZone)
        {
            return partnerBankInfos.Select(x => x.MapToApiPartnerBankInfo(timeZone)).ToList();
        }

        public static ApiPartnerBankInfo MapToApiPartnerBankInfo(this fnPartnerBankInfo partnerBankInfo, double timeZone)
        {
            return new ApiPartnerBankInfo
            {
                Id = partnerBankInfo.Id,
                PartnerId = partnerBankInfo.PartnerId,
                PaymentSystemId = partnerBankInfo.PaymentSystemId,
                BankName = partnerBankInfo.BankName,
                NickName = partnerBankInfo.NickName,
                BankCode = partnerBankInfo.BankCode,
                Accounts = partnerBankInfo.AccountNumber.Split(',').ToList(),
                OwnerName = partnerBankInfo.OwnerName,
                BranchName = partnerBankInfo.BranchName,
                IBAN = partnerBankInfo.IBAN,
                CurrencyId = partnerBankInfo.CurrencyId,
                Active = partnerBankInfo.Active,
                Type = partnerBankInfo.Type,
                Order = partnerBankInfo.Order,
                CreationTime = partnerBankInfo.CreationTime.GetGMTDateFromUTC(timeZone),
                LastUpdateTime = partnerBankInfo.LastUpdateTime.GetGMTDateFromUTC(timeZone)
            };
        }

        public static PartnerBankInfo MapToPartnerBankInfo(this ApiPartnerBankInfo partnerBankInfo)
        {
            return new PartnerBankInfo
            {
                Id = partnerBankInfo.Id ?? 0,
                PartnerId = partnerBankInfo.PartnerId,
                PaymentSystemId = partnerBankInfo.PaymentSystemId,
                BankName = partnerBankInfo.BankName,
                BankCode = partnerBankInfo.BankCode,
                AccountNumber = (partnerBankInfo.Accounts != null && partnerBankInfo.Accounts.Any()) ? string.Join(",", partnerBankInfo.Accounts) : string.Empty,
                OwnerName = partnerBankInfo.OwnerName ?? string.Empty,
                BranchName = partnerBankInfo.BranchName ?? string.Empty,
                IBAN = partnerBankInfo.IBAN ?? string.Empty,
                CurrencyId = partnerBankInfo.CurrencyId,
                Active = partnerBankInfo.Active,
                Type = partnerBankInfo.Type,
                Order = partnerBankInfo.Order,
                CreationTime = partnerBankInfo.CreationTime,
                LastUpdateTime = partnerBankInfo.LastUpdateTime
            };
        }

        public static ApiPartnerPaymentSetting MapToApiPartnerPaymentSetting(this PartnerPaymentSetting partnerPaymentSetting, double timeZone)
        {
            return new ApiPartnerPaymentSetting
            {
                Id = partnerPaymentSetting.Id,
                CurrencyId = partnerPaymentSetting.CurrencyId,
                PaymentSystemId = partnerPaymentSetting.PaymentSystemId,
                PartnerId = partnerPaymentSetting.PartnerId,
                LastUpdateTime = partnerPaymentSetting.LastUpdateTime.GetGMTDateFromUTC(timeZone),
                CreationTime = partnerPaymentSetting.CreationTime.GetGMTDateFromUTC(timeZone),
                State = partnerPaymentSetting.State,
                Type = partnerPaymentSetting.Type,
                Commission = partnerPaymentSetting.Commission,
                FixedFee = partnerPaymentSetting.FixedFee,
                Info = partnerPaymentSetting.Info,
                MinAmount = partnerPaymentSetting.MinAmount,
                MaxAmount = partnerPaymentSetting.MaxAmount,
                AllowMultipleClientsPerPaymentInfo = partnerPaymentSetting.AllowMultipleClientsPerPaymentInfo,
                AllowMultiplePaymentInfoes = partnerPaymentSetting.AllowMultiplePaymentInfoes,
                UserName = string.Empty,
                Password = string.Empty,
                Priority = partnerPaymentSetting.PaymentSystemPriority,
                Countries = partnerPaymentSetting.PartnerPaymentCountrySettings?.Select(x => x.CountryId).ToList(),
                OSTypes = partnerPaymentSetting.OSTypes?.Split(',').Select(Int32.Parse).ToList(),
                OpenMode = partnerPaymentSetting.OpenMode,
                PaymentSystemName = CacheManager.GetPaymentSystemById(partnerPaymentSetting.PaymentSystemId).Name
            };
        }

        public static PartnerPaymentSetting MapToPartnerPaymentSetting(this ApiPartnerPaymentSetting input)
        {
            return new PartnerPaymentSetting
            {
                Id = input.Id,
                PartnerId = input.PartnerId,
                PaymentSystemId = input.PaymentSystemId,
                State = input.State,
                CurrencyId = input.CurrencyId,
                Commission = input.Commission ?? 0,
                FixedFee = input.FixedFee ?? 0,
                Type = input.Type,
                Info = input.Info,
                MinAmount = input.MinAmount,
                MaxAmount = input.MaxAmount,
                AllowMultipleClientsPerPaymentInfo = input.AllowMultipleClientsPerPaymentInfo,
                AllowMultiplePaymentInfoes = input.AllowMultiplePaymentInfoes,
                UserName = input.UserName,
                Password = input.Password,
                PaymentSystemPriority = input.Priority,
                OpenMode = input.OpenMode,
                OSTypes = input.OSTypes != null ? string.Join(",", input.OSTypes) : null,
                PartnerPaymentCountrySettings = input.Countries?.Select(x => new PartnerPaymentCountrySetting { CountryId = x }).ToList()
            };
        }


        #endregion

        #region fnPartnerPaymentSetting

        public static FnPartnerPaymentSettingModel MapTofnPartnerPaymentSettingModel(
            this fnPartnerPaymentSetting model, double timeZone)
        {
            return new FnPartnerPaymentSettingModel
            {
                Id = model.Id,
                CurrencyId = model.CurrencyId,
                PaymentSystemId = model.PaymentSystemId,
                PaymentSystemName = model.PaymentSystemName,
                PartnerId = model.PartnerId,
                LastUpdateTime = model.LastUpdateTime.GetGMTDateFromUTC(timeZone),
                CreationTime = model.CreationTime.GetGMTDateFromUTC(timeZone),
                Commission = model.Commission,
                State = model.State,
                Type = model.Type,
                MinAmount = model.MinAmount,
                MaxAmount = model.MaxAmount,
                Info = model.Info,
                UserName = string.Empty,
                Password = string.Empty,
                Priority = model.PaymentSystemPriority,

            };
        }

        #endregion

        #region fnPartnerProductSettings

        public static FilterfnPartnerProductSetting MapTofnPartnerProductSettings(this ApiFilterPartnerProductSetting setting)
        {
            return new FilterfnPartnerProductSetting
            {
                SkipCount = setting.SkipCount,
                TakeCount = setting.TakeCount,
                PartnerId = setting.PartnerId,
                ProviderId = setting.ProviderId,
                CategoryIds = setting.CategoryIds?.ToString(),
                Ids = setting.Ids == null ? new FiltersOperation() : setting.Ids.MapToFiltersOperation(),
                ProductIds = setting.ProductIds == null ? new FiltersOperation() : setting.ProductIds.MapToFiltersOperation(),
                ProductDescriptions = setting.ProductDescriptions == null ? new FiltersOperation() : setting.ProductDescriptions.MapToFiltersOperation(),
                ProductNames = setting.ProductNames == null ? new FiltersOperation() : setting.ProductNames.MapToFiltersOperation(),
                ProductExternalIds = setting.ProductExternalIds == null ? new FiltersOperation() : setting.ProductExternalIds.MapToFiltersOperation(),
                ProductGameProviders = setting.GameProviderIds == null ? new FiltersOperation() : setting.GameProviderIds.MapToFiltersOperation(),
                SubProviderIds = setting.SubProviderIds == null ? new FiltersOperation() : setting.SubProviderIds.MapToFiltersOperation(),
                Jackpots = setting.Jackpots == null ? new FiltersOperation() : setting.Jackpots.MapToFiltersOperation(),
                States = setting.States == null ? new FiltersOperation() : setting.States.MapToFiltersOperation(),
                Percents = setting.Percents == null ? new FiltersOperation() : setting.Percents.MapToFiltersOperation(),
                OpenModes = setting.OpenModes == null ? new FiltersOperation() : setting.OpenModes.MapToFiltersOperation(),
                RTPs = setting.RTPs == null ? new FiltersOperation() : setting.RTPs.MapToFiltersOperation(),
                ExternalIds = setting.ExternalIds == null ? new FiltersOperation() : setting.ExternalIds.MapToFiltersOperation(),
                Volatilities = setting.Volatilities == null ? new FiltersOperation() : setting.Volatilities.MapToFiltersOperation(),
                Ratings = setting.Ratings == null ? new FiltersOperation() : setting.Ratings.MapToFiltersOperation(),
                IsForMobile = setting.IsForMobile == null ? new FiltersOperation() : setting.IsForMobile.MapToFiltersOperation(),
                IsForDesktop = setting.IsForDesktop == null ? new FiltersOperation() : setting.IsForDesktop.MapToFiltersOperation(),
                HasDemo = setting.HasDemo == null ? new FiltersOperation() : setting.HasDemo.MapToFiltersOperation(),
                ProductIsLeaf = setting.ProductIsLeaf == null ? new FiltersOperation() : setting.ProductIsLeaf.MapToFiltersOperation(),
                OrderBy = setting.OrderBy,
                FieldNameToOrderBy = setting.FieldNameToOrderBy
            };
        }

        public static FilterfnProduct MapTofnProductSettings(this ApiFilterPartnerProductSetting setting)
        {
            return new FilterfnProduct
            {
                SkipCount = setting.SkipCount,
                TakeCount = setting.TakeCount,
                Ids = setting.Ids == null ? new FiltersOperation() : setting.Ids.MapToFiltersOperation(),
                Descriptions = setting.ProductDescriptions == null ? new FiltersOperation() : setting.ProductDescriptions.MapToFiltersOperation(),
                Names = setting.ProductNames == null ? new FiltersOperation() : setting.ProductNames.MapToFiltersOperation(),
                ExternalIds = setting.ProductExternalIds == null ? new FiltersOperation() : setting.ProductExternalIds.MapToFiltersOperation(),
                GameProviderIds = setting.GameProviderIds == null ? new FiltersOperation() : setting.GameProviderIds.MapToFiltersOperation(),
                SubProviderIds = setting.SubProviderIds == null ? new FiltersOperation() : setting.SubProviderIds.MapToFiltersOperation(),
                States = setting.States == null ? new FiltersOperation() : setting.States.MapToFiltersOperation(),
                Jackpots = setting.Jackpots == null ? new FiltersOperation() : setting.Jackpots.MapToFiltersOperation(),
                RTPs = setting.RTPs == null ? new FiltersOperation() : setting.RTPs.MapToFiltersOperation(),
                IsForMobiles = setting.IsForMobile == null ? new FiltersOperation() : setting.IsForMobile.MapToFiltersOperation(),
                IsForDesktops = setting.IsForDesktop == null ? new FiltersOperation() : setting.IsForDesktop.MapToFiltersOperation(),
                HasDemo = setting.HasDemo == null ? new FiltersOperation() : setting.HasDemo.MapToFiltersOperation()
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

        public static FnPartnerProductSettingModel MapTofnPartnerProductSettingModel(this fnPartnerProductSetting setting, double timeZone)
        {
            return new FnPartnerProductSettingModel
            {
                Id = setting.Id,
                ProductId = setting.ProductId,
                ProductName = setting.ProductName,
                ProductDescription = setting.ProductNickName,
                ProductGameProviderId = setting.ProductGameProviderId,
                GameProviderName = setting.GameProviderName,
                PartnerId = setting.PartnerId,
                Percent = setting.Percent,
                State = setting.State,
                Rating = setting.Rating,
                RTP = setting.RTP,
                ExternalId = setting.ProductExternalId,
                Volatility = setting.Volatility,
                CategoryIds = !string.IsNullOrEmpty(setting.CategoryIds) ? JsonConvert.DeserializeObject<List<int>>(setting.CategoryIds) : null,
                SubproviderId = setting.SubproviderId,
                OpenMode = setting.OpenMode,
                IsForMobile = setting.IsForMobile,
                IsForDesktop = setting.IsForDesktop,
                Jackpot = setting.Jackpot,
                MobileImageUrl = setting.MobileImageUrl,
                WebImageUrl = setting.WebImageUrl,
                HasDemo = setting.HasDemo,
                CreationTime = setting.CreationTime.GetGMTDateFromUTC(timeZone),
                LastUpdateTime = setting.LastUpdateTime.GetGMTDateFromUTC(timeZone)
            };
        }

        #endregion

        #region Dashboard

        public static ApiBetsInfo MapToApiBetsInfo(this BetsInfo info)
        {
            return new ApiBetsInfo
            {
                TotalBetsAmount = Math.Round(info.TotalBetsAmount, 2),
                TotalBetsCount = info.TotalBetsCount,
                TotalPlayersCount = info.TotalPlayersCount,
                TotalGGR = Math.Round(info.TotalGGR, 2),
                TotalNGR = Math.Round(info.TotalGGR, 2),
                TotalBetsFromWebSite = Math.Round(info.TotalBetsFromWebSite, 2),
                TotalBetsCountFromWebSite = info.TotalBetsCountFromWebSite,
                TotalPlayersCountFromWebSite = info.TotalPlayersCountFromWebSite,
                TotalGGRFromWebSite = Math.Round(info.TotalGGRFromWebSite, 2),
                TotalNGRFromWebSite = Math.Round(info.TotalNGRFromWebSite, 2),
                TotalBetsFromMobile = Math.Round(info.TotalBetsFromMobile, 2),
                TotalBetsCountFromMobile = info.TotalBetsCountFromMobile,
                TotalPlayersCountFromMobile = info.TotalPlayersCountFromMobile,
                TotalGGRFromMobile = Math.Round(info.TotalGGRFromMobile, 2),
                TotalNGRFromMobile = Math.Round(info.TotalNGRFromMobile, 2),
                TotalBetsCountFromWap = info.TotalBetsCountFromWap,
                TotalBetsFromWap = Math.Round(info.TotalBetsFromWap, 2),
                TotalGGRFromWap = Math.Round(info.TotalGGRFromWap, 2),
                TotalNGRFromWap = Math.Round(info.TotalNGRFromWap, 2),
                TotalPlayersCountFromWap = info.TotalPlayersCountFromWap,
            };
        }

        public static ApiDepositsInfo MapToApiPaymentRequestsInfo(this PaymentRequestsInfo info)
        {
            return new ApiDepositsInfo
            {
                Status = info.Status,
                TotalPlayersCount = info.TotalPlayersCount,
                Deposits = info.PaymentRequests.Select(x => new ApiDepositInfo
                {
                    PaymentSystemId = x.PaymentSystemId,
                    PaymentSystemName = x.PaymentSystemName,
                    TotalAmount = Math.Round(x.TotalAmount, 2),
                    TotalDepositsCount = x.TotalRequestsCount,
                    TotalPlayersCount = x.TotalPlayersCount
                }).ToList()
            };
        }

        public static ApiWithdrawalsInfo MapToApiWithdrawalsInfo(this PaymentRequestsInfo info)
        {
            return new ApiWithdrawalsInfo
            {
                Status = info.Status,
                TotalPlayersCount = info.TotalPlayersCount,
                Withdrawals = info.PaymentRequests.Select(x => new ApiWithdrawalInfo
                {
                    PaymentSystemId = x.PaymentSystemId,
                    PaymentSystemName = x.PaymentSystemName,
                    TotalAmount = Math.Round(x.TotalAmount, 2),
                    TotalWithdrawalsCount = x.TotalRequestsCount,
                    TotalPlayersCount = x.TotalPlayersCount
                }).ToList()
            };
        }

        public static ApiPlayersInfo MapToApiPlayersInfo(this PlayersInfo info)
        {
            return new ApiPlayersInfo
            {
                VisitorsCount = info.VisitorsCount,
                SignUpsCount = info.SignUpsCount,
                AverageBet = Math.Round(info.AverageBet, 2),
                MaxBet = Math.Round(info.MaxBet, 2),
                MaxWin = Math.Round(info.MaxWin, 2),
                DepositsCount = info.DepositsCount,
                MaxWinBet = Math.Round(info.MaxWinBet, 2),
                ReturnsCount = info.ReturnsCount,
                TotalBetAmount = Math.Round(info.TotalBetAmount, 2),
                TotalBonusAmount = Math.Round(info.TotalBonusAmount, 2),
                TotalCashoutAmount = Math.Round(info.TotalCashoutAmount, 2),
                TotalPlayersCount = info.TotalPlayersCount
            };
        }

        public static ApiProvidersBetsInfo MapToApiProvidersBetsInfo(this ProvidersBetsInfo info)
        {
            return new ApiProvidersBetsInfo
            {
                TotalPlayersCount = info.TotalPlayersCount,
                TotalBetsAmount = Math.Round(info.TotalBetsAmount, 2),
                TotalBonusBetsAmount = Math.Round(info.TotalBonusBetsAmount, 2),
                TotalWinsAmount = Math.Round(info.TotalWinsAmount, 2),
                TotalBonusWinsAmount = Math.Round(info.TotalBonusWinsAmount, 2),
                TotalGGR = Math.Round(info.TotalGGR, 2),
                TotalNGR = Math.Round(info.TotalNGR, 2),
                Bets = info.Bets.Select(x => new ApiProviderBetsInfo
                {
                    ProviderId = x.ProviderId,
                    TotalBetsAmount = Math.Round(x.TotalBetsAmount, 2),
                    TotalWinsAmount = Math.Round(x.TotalWinsAmount, 2),
                    TotalBetsCount = x.TotalBetsCount,
                    TotalGGR = Math.Round(x.TotalGGR, 2),
                    TotalNGR = Math.Round(x.TotalNGR, 2),
                    TotalPlayersCount = x.TotalPlayersCount,
                    TotalBetsAmountFromInternet = Math.Round(x.TotalBetsAmountFromInternet, 2),
                    TotalBetsAmountFromBetShop = Math.Round(x.TotalBetsAmountFromBetShop, 2)
                }).OrderByDescending(x => x.TotalBetsAmount).ToList()
            };
        }

        #endregion

        #region Region

        public static fnRegion MapTofnRegion(this FnRegionModel region)
        {
            return new fnRegion
            {
                Id = region.Id,
                ParentId = region.ParentId,
                TypeId = region.TypeId,
                IsoCode = region.IsoCode,
                IsoCode3 = region.IsoCode3,
                State = region.State,
                Name = region.Name,
                CurrencyId = region.CurrencyId,
                LanguageId = region.LanguageId,
                Info = region.Info
            };
        }

        public static Region MapToRegion(this FnRegionModel region)
        {
            return new Region
            {
                Id = region.Id,
                ParentId = region.ParentId,
                TypeId = region.TypeId,
                IsoCode = region.IsoCode,
                IsoCode3 = region.IsoCode3,
                State = region.State,
                NickName = region.Name,
                CurrencyId = region.CurrencyId,
                LanguageId = region.LanguageId,
                Info = region.Info
            };
        }

        public static FnRegionModel MapTofnRegionModel(this fnRegion region)
        {
            return new FnRegionModel
            {
                Id = region.Id,
                ParentId = region.ParentId,
                TypeId = region.TypeId,
                IsoCode = region.IsoCode,
                IsoCode3 = region.IsoCode3,
                Name = region.Name,
                NickName = region.NickName,
                State = region.State,
                CurrencyId = region.CurrencyId,
                LanguageId = region.LanguageId,
                Info = region.Info
            };
        }

        public static List<FnRegionModel> MapTofnRegionModels(this IEnumerable<fnRegion> regions)
        {
            return regions.Select(MapTofnRegionModel).ToList();
        }

        #endregion

        #region Currency

        public static Currency MapToCurrency(this CurrencyModel model)
        {
            return new Currency
            {
                Id = model.Id,
                CurrentRate = model.CurrentRate != 0m ? 1 / model.CurrentRate : 0,
                Symbol = model.Symbol,
                Code = model.Code,
                Name = model.Name
            };
        }

        public static CurrencyModel MapToCurrency(this Currency currency)
        {
            var rate = (currency.CurrentRate != 0m ? 1 / currency.CurrentRate : 0);
            return new CurrencyModel
            {
                Id = currency.Id,
                CurrentRate = rate > 10000 ? Math.Round(rate, 2) : Math.Round(rate, 0),
                Symbol = currency.Symbol,
                Code = currency.Code,
                Name = currency.Name
            };
        }

        public static CurrencyRateModel MapToCurrencyRateModel(this CurrencyRate currencyRate, double timeZone)
        {
            var rb = currencyRate.RateBefore != 0m ? 1 / currencyRate.RateBefore : 0;
            var ra = currencyRate.RateAfter != 0m ? 1 / currencyRate.RateAfter : 0;
            return new CurrencyRateModel
            {
                Id = currencyRate.Id,
                CurrencyId = currencyRate.CurrencyId,
                RateBefore = rb > 10000 ? Math.Round(rb, 0) : Math.Round(rb, 2),
                RateAfter = ra > 10000 ? Math.Round(ra, 0) : Math.Round(ra, 2),
                UserId = currencyRate.UserSession.UserId.Value,
                UserName = currencyRate.UserSession.User.UserName,
                CreationTime = currencyRate.CreationTime.GetGMTDateFromUTC(timeZone),
                LastUpdateTime = currencyRate.LastUpdateTime.GetGMTDateFromUTC(timeZone)
            };
        }

        public static PartnerCurrencySettingModel MapToPartnerCurrencySettingModel(this PartnerCurrencySetting partnerCurrency, double timeZone)
        {
            return new PartnerCurrencySettingModel
            {
                Id = partnerCurrency.Id,
                PartnerId = partnerCurrency.PartnerId,
                CurrencyId = partnerCurrency.CurrencyId,
                State = partnerCurrency.State,
                UserMinLimit = partnerCurrency.UserMinLimit,
                UserMaxLimit = partnerCurrency.UserMaxLimit,
                ClientMinBet = partnerCurrency.ClientMinBet,
                CreationTime = partnerCurrency.CreationTime.GetGMTDateFromUTC(timeZone),
                LastUpdateTime = partnerCurrency.LastUpdateTime.GetGMTDateFromUTC(timeZone),
                Priority = partnerCurrency.Priority
            };
        }

        public static PartnerCurrencySetting ToPartnerCurrencySetting(this ApiPartnerCurrencySetting partnerCurrency)
        {
            return new PartnerCurrencySetting
            {
                Id = partnerCurrency.Id,
                PartnerId = partnerCurrency.PartnerId,
                CurrencyId = partnerCurrency.CurrencyId,
                State = partnerCurrency.State,
                CreationTime = partnerCurrency.CreationTime,
                LastUpdateTime = partnerCurrency.LastUpdateTime,
                Priority = partnerCurrency.Priority,
                UserMinLimit = partnerCurrency.UserMinLimit,
                UserMaxLimit = partnerCurrency.UserMaxLimit,
                ClientMinBet = partnerCurrency.ClientMinBet
            };
        }

        public static ApiPartnerCurrencySetting ToApiPartnerCurrencySetting(this PartnerCurrencySetting partnerCurrency)
        {
            return new ApiPartnerCurrencySetting
            {
                Id = partnerCurrency.Id,
                PartnerId = partnerCurrency.PartnerId,
                CurrencyId = partnerCurrency.CurrencyId,
                State = partnerCurrency.State,
                CreationTime = partnerCurrency.CreationTime,
                LastUpdateTime = partnerCurrency.LastUpdateTime,
                Priority = partnerCurrency.Priority,
                UserMinLimit = partnerCurrency.UserMinLimit,
                UserMaxLimit = partnerCurrency.UserMaxLimit,
                ClientMinBet = partnerCurrency.ClientMinBet
            };
        }

        #endregion

        #region Note

        public static List<NoteModel> MapToNoteModels(this IEnumerable<fnNote> models, double timeZone)
        {
            return models.Select(x => x.MapToNoteModel(timeZone)).ToList();
        }

        public static NoteModel MapToNoteModel(this fnNote note, double timeZone)
        {
            return new NoteModel
            {
                Id = note.Id,
                State = note.State,
                CreationTime = note.CreationTime.GetGMTDateFromUTC(timeZone),
                LastUpdateTime = note.LastUpdateTime.GetGMTDateFromUTC(timeZone),
                Message = note.Message,
                ObjectId = note.ObjectId,
                Type = note.Type,
                ObjectTypeId = note.ObjectTypeId,
                CommentTemplateId = note.CommentTemplateId,
                CreatorFirstName = note.CreatorFirstName,
                CreatorLastName = note.CreatorLastName
            };
        }

        #endregion

        #region Bonuses

        public static Bonu MapToBonus(this ApiBonus bonus)
        {
            return new Bonu
            {
                Id = bonus.Id ?? 0,
                Name = bonus.Name,
                Description = bonus.Description,
                PartnerId = bonus.PartnerId,
                FinalAccountTypeId = bonus.FinalAccountTypeId,
                Status = bonus.Status,
                StartTime = bonus.StartTime,
                FinishTime = bonus.FinishTime,
                LastExecutionTime = bonus.StartTime,
                Period = bonus.Period ?? 0,
                BonusProducts = bonus.Products?.Select(
                    x => new BonusProduct
                    {
                        Id = x.Id ?? 0,
                        ProductId = x.ProductId,
                        Percent = x.Percent,
                        Count = x.Count,
                        Lines = x.Lines,
                        Coins = x.Coins,
                        CoinValue = x.CoinValue,
                        BetValueLevel = x.BetValueLevel
                    }).ToList(),
                Type = bonus.BonusTypeId,
                TurnoverCount = bonus.TurnoverCount,
                Info = bonus.LinkedCampaign == true ? "1" : (bonus.LinkedCampaign == false ? "" : bonus.Info),
                MinAmount = bonus.MinAmount,
                MaxAmount = bonus.MaxAmount,
                Sequence = bonus.Sequence,
                Priority = bonus.Priority,
                WinAccountTypeId = bonus.WinAccountTypeId,
                ValidForAwarding = bonus.ValidForAwarding,
                ValidForSpending = bonus.ValidForSpending,
                ReusingMaxCount = bonus.ReusingMaxCount,
                ResetOnWithdraw = bonus.ResetOnWithdraw,
                AllowSplit = bonus.AllowSplit,
                RefundRollbacked = bonus.RefundRollbacked,
                MaxGranted = bonus.MaxGranted,
                MaxReceiversCount = bonus.MaxReceiversCount,
                LinkedBonusId = bonus.LinkedBonusId,
                AutoApproveMaxAmount = bonus.AutoApproveMaxAmount,
                BonusCountrySettings = bonus.Countries?.Ids?.Select(x => new BonusCountrySetting { CountryId = x,
                    Type = bonus.Countries.Type ?? (int)BonusSettingConditionTypes.InSet }).ToList(),
                BonusLanguageSettings = bonus.Languages?.Names?.Select(x => new BonusLanguageSetting { LanguageId = x,
                    Type = bonus.Languages.Type ?? (int)BonusSettingConditionTypes.InSet }).ToList(),
                BonusCurrencySettings = bonus.Currencies?.Names?.Select(x => new BonusCurrencySetting { CurrencyId = x,
                    Type = bonus.Currencies.Type ?? (int)BonusSettingConditionTypes.InSet }).ToList(),
                BonusSegmentSettings = bonus.SegmentIds?.Ids?.Select(x => new BonusSegmentSetting { SegmentId = x,
                    Type = bonus.SegmentIds.Type ?? (int)BonusSettingConditionTypes.InSet }).ToList(),
                BonusPaymentSystemSettings = bonus.PaymentSystemIds?.Ids?.Select(x => new BonusPaymentSystemSetting { PaymentSystemId = x, BonusId = bonus.Id,
                    Type = bonus.PaymentSystemIds.Type ?? (int)BonusSettingConditionTypes.InSet }).ToList(),
                Percent = bonus.Percent,
                FreezeBonusBalance = bonus.FreezeBonusBalance,
                Regularity = bonus.Regularity,
                DayOfWeek = bonus.DayOfWeek,
                ReusingMaxCountInPeriod = bonus.ReusingMaxCountInPeriod,
                AmountCurrencySettings = bonus.AmountSettings?.Select(x => new AmountCurrencySetting {
                    CurrencyId = x.CurrencyId,
                    BonusId = bonus.Id ?? 0,
                    MinAmount = x.MinAmount,
                    MaxAmount = x.MaxAmount,
                    UpToAmount = x.UpToAmount
                }).ToList()
            };
        }

        public static ApiClientBonuses MapToApiClientBonuses(this PagedModel<fnClientBonus> input, double timeZone)
        {
            return new ApiClientBonuses
            {
                Count = input.Count,
                Entities = input.Entities.Select(x => x.MapToApiClientBonus(timeZone)).ToList()
            };
        }

        public static ApiClientBonus MapToApiClientBonus(this fnClientBonus input, double timeZone)
        {
            return new ApiClientBonus
            {
                Id = input.Id,
                ClientId = input.ClientId,
                UserName = input.UserName,
                FirstName = input.FirstName,
                LastName = input.LastName,
                CurrencyId = input.CurrencyId,
                Email = input.Email,
                MobileNumber = input.MobileNumber,
                WageringTarget = input.WageringTarget,
                PartnerId = input.PartnerId,
                BonusId = input.BonusId,
                Status = input.Status,
                BonusName = input.Name,
                BonusPrize = input.BonusPrize,
                BonusType = input.Type,
                TurnoverAmountLeft = input.Type == (int)BonusTypes.CampaignFreeBet ? null : input.TurnoverAmountLeft,
                CreationTime = input.CreationTime.GetGMTDateFromUTC(timeZone),
                AwardingTime = input.AwardingTime.GetGMTDateFromUTC(timeZone),
                FinalAmount = input.FinalAmount,
                CalculationTime = input.CalculationTime?.GetGMTDateFromUTC(timeZone),
                ValidUntil = input.ValidUntil?.GetGMTDateFromUTC(timeZone),
                TriggerId = input.TriggerId,
                RemainingCredit = input.RemainingCredit,
                ReuseNumber = input.ReuseNumber,
                TriggerSettings = input.TriggerSettingItems?.Select(x => x.MapToApiTriggerSetting(timeZone, input.ClientId)).ToList()
            };
        }

        public static ApiClientExclusion MapToApiClientExclusion(this fnReportByClientExclusion input)
        {
            return new ApiClientExclusion
            {
                PartnerId = input.PartnerId.Value,
                ClientId = input.ClientId,
                Username = input.Username,
                DepositLimitDaily = input.DepositLimitDaily,
                DepositLimitWeekly = input.DepositLimitWeekly,
                DepositLimitMonthly = input.DepositLimitMonthly,
                TotalBetAmountLimitDaily = input.TotalBetAmountLimitDaily,
                TotalBetAmountLimitWeekly = input.TotalBetAmountLimitWeekly,
                TotalBetAmountLimitMonthly = input.TotalBetAmountLimitMonthly,
                TotalLossLimitDaily = input.TotalLossLimitDaily,
                TotalLossLimitWeekly = input.TotalLossLimitWeekly,
                TotalLossLimitMonthly = input.TotalLossLimitMonthly,
                SystemDepositLimitDaily = input.SystemDepositLimitDaily,
                SystemDepositLimitWeekly = input.SystemDepositLimitWeekly,
                SystemDepositLimitMonthly = input.SystemDepositLimitMonthly,
                SystemTotalBetAmountLimitDaily = input.SystemTotalBetAmountLimitDaily,
                SystemTotalBetAmountLimitWeekly = input.SystemTotalBetAmountLimitWeekly,
                SystemTotalBetAmountLimitMonthly = input.SystemTotalBetAmountLimitMonthly,
                SystemTotalLossLimitDaily = input.SystemTotalLossLimitDaily,
                SystemTotalLossLimitWeekly = input.SystemTotalLossLimitWeekly,
                SystemTotalLossLimitMonthly = input.SystemTotalLossLimitMonthly,
                SessionLimit = input.SessionLimit,
                SystemSessionLimit = input.SystemSessionLimit
            };
        }

        public static ApiClientTrigger MapToApiClientTrigger(this ClientBonusTrigger clientBonusTrigger)
        {
            return new ApiClientTrigger
            {
                ClientId = clientBonusTrigger.ClientId.Value,
                TriggerId = clientBonusTrigger.TriggerId,
                SourceAmount = clientBonusTrigger.SourceAmount
            };
        }

        public static ApiBonus MapToApiBonus(this Bonu bonus, double timeZone)
        {
            return new ApiBonus
            {
                Id = bonus.Id,
                Name = bonus.Name,
                Description = bonus.Description,
                PartnerId = bonus.PartnerId,
                FinalAccountTypeId = bonus.FinalAccountTypeId ?? 0,
                Status = bonus.Status,
                StartTime = bonus.StartTime.GetGMTDateFromUTC(timeZone),
                FinishTime = bonus.FinishTime.GetGMTDateFromUTC(timeZone),
                LastExecutionTime = bonus.LastExecutionTime.GetGMTDateFromUTC(timeZone),
                CreationTime = bonus.CreationTime?.GetGMTDateFromUTC(timeZone),
                UpdateTime = bonus.LastUpdateTime?.GetGMTDateFromUTC(timeZone),
                Period = bonus.Period,
                Percent = bonus.Percent,
                Products = bonus.BonusProducts == null ? new List<ApiBonusProducts>() :
                bonus.BonusProducts.Select(x => new ApiBonusProducts
                {
                    Id = x.Id,
                    ProductId = x.ProductId,
                    Percent = x.Percent,
                    Count = x.Count,
                    Lines = x.Lines,
                    Coins = x.Coins,
                    CoinValue = x.CoinValue,
                    BetValueLevel = x.BetValueLevel
                }).ToList(),
                BonusTypeId = bonus.Type,
                Info = bonus.Info,
                LinkedCampaign = (bonus.Info == "1"),
                TurnoverCount = bonus.TurnoverCount,
                MinAmount = bonus.MinAmount,
                MaxAmount = bonus.MaxAmount,
                Sequence = bonus.Sequence,
                Priority = bonus.Priority,
                WinAccountTypeId = bonus.WinAccountTypeId,
                ValidForAwarding = bonus.ValidForAwarding,
                ValidForSpending = bonus.ValidForSpending,
                ReusingMaxCount = bonus.ReusingMaxCount,
                ResetOnWithdraw = bonus.ResetOnWithdraw,
                AllowSplit = bonus.AllowSplit,
                RefundRollbacked = bonus.RefundRollbacked,
                MaxGranted = bonus.MaxGranted,
                TotalGranted = bonus.TotalGranted ?? 0,
                MaxReceiversCount = bonus.MaxReceiversCount,
                TotalReceiversCount = bonus.TotalReceiversCount ?? 0,
                LinkedBonusId = bonus.LinkedBonusId,
                AutoApproveMaxAmount = bonus.AutoApproveMaxAmount,
                Countries = bonus.BonusCountrySettings != null && bonus.BonusCountrySettings.Any() ? new ApiSetting
                {
                    Type = bonus.BonusCountrySettings.First().Type,
                    Ids = bonus.BonusCountrySettings.Select(x => x.CountryId).ToList()
                } : new ApiSetting { Type = (int)BonusSettingConditionTypes.InSet, Ids = new List<int>() },
                Languages = bonus.BonusLanguageSettings != null && bonus.BonusLanguageSettings.Any() ? new ApiSetting
                {
                    Type = bonus.BonusLanguageSettings.First().Type,
                    Names = bonus.BonusLanguageSettings.Select(x => x.LanguageId).ToList()
                } : new ApiSetting { Type = (int)BonusSettingConditionTypes.InSet, Names = new List<string>() },
                Currencies = bonus.BonusCurrencySettings != null && bonus.BonusCurrencySettings.Any() ? new ApiSetting
                {
                    Type = bonus.BonusCurrencySettings.First().Type,
                    Names = bonus.BonusCurrencySettings.Select(x => x.CurrencyId).ToList()
                } : new ApiSetting { Type = (int)BonusSettingConditionTypes.InSet, Names = new List<string>() },
                SegmentIds = bonus.BonusSegmentSettings != null && bonus.BonusSegmentSettings.Any() ? new ApiSetting
                {
                    Type = bonus.BonusSegmentSettings.First().Type,
                    Ids = bonus.BonusSegmentSettings.Select(x => x.SegmentId).ToList()
                } : new ApiSetting { Type = (int)BonusSettingConditionTypes.InSet, Ids = new List<int>() },
                PaymentSystemIds = bonus.BonusPaymentSystemSettings != null && bonus.BonusPaymentSystemSettings.Any() ? new ApiSetting
                {
                    Type = bonus.BonusPaymentSystemSettings.First().Type,
                    Ids = bonus.BonusPaymentSystemSettings.Select(x => x.PaymentSystemId).ToList()
                } : new ApiSetting { Type = (int)BonusSettingConditionTypes.InSet, Ids = new List<int>() },
                Conditions = string.IsNullOrEmpty(bonus.Condition) ? null : JsonConvert.DeserializeObject<Common.Models.Bonus.BonusCondition>(bonus.Condition),
                FreezeBonusBalance = bonus.FreezeBonusBalance,
                Regularity = bonus.Regularity,
                DayOfWeek = bonus.DayOfWeek,
                ReusingMaxCountInPeriod = bonus.ReusingMaxCountInPeriod,
                AmountSettings = bonus.AmountCurrencySettings == null ? null : bonus.AmountCurrencySettings.Select(x => new ApiAmountSetting
                {
                    CurrencyId = x.CurrencyId,
                    MinAmount = x.MinAmount,
                    MaxAmount = x.MaxAmount
                }).ToList()
            };
        }

        public static ApiBonus MapToApiBonus(this fnBonus bonus, double timeZone)
        {
            return new ApiBonus
            {
                Id = bonus.Id,
                Name = bonus.NickName,
                Description = bonus.Description,
                PartnerId = bonus.PartnerId,
                FinalAccountTypeId = bonus.FinalAccountTypeId ?? 0,
                Status = bonus.Status,
                StartTime = bonus.StartTime.GetGMTDateFromUTC(timeZone),
                FinishTime = bonus.FinishTime.GetGMTDateFromUTC(timeZone),
                LastExecutionTime = bonus.LastExecutionTime.GetGMTDateFromUTC(timeZone),
                CreationTime = bonus.CreationTime?.GetGMTDateFromUTC(timeZone),
                UpdateTime = bonus.LastUpdateTime?.GetGMTDateFromUTC(timeZone),
                Period = bonus.Period,
                Percent = 0,
                Products = new List<ApiBonusProducts>(),
                BonusTypeId = bonus.Type,
                Info = bonus.Info,
                TurnoverCount = bonus.TurnoverCount,
                MinAmount = bonus.MinAmount,
                MaxAmount = bonus.MaxAmount,
                Sequence = bonus.Sequence,
                Priority = bonus.Priority,
                WinAccountTypeId = bonus.WinAccountTypeId,
                ValidForAwarding = bonus.ValidForAwarding,
                ValidForSpending = bonus.ValidForSpending,
                ReusingMaxCount = bonus.ReusingMaxCount,
                ResetOnWithdraw = bonus.ResetOnWithdraw,
                AllowSplit = bonus.AllowSplit,
                RefundRollbacked = bonus.RefundRollbacked,
                MaxGranted = bonus.MaxGranted,
                TotalGranted = bonus.TotalGranted ?? 0,
                MaxReceiversCount = bonus.MaxReceiversCount,
                TotalReceiversCount = bonus.TotalReceiversCount ?? 0,
                LinkedBonusId = bonus.LinkedBonusId,
                AutoApproveMaxAmount = bonus.AutoApproveMaxAmount,
                Conditions = string.IsNullOrEmpty(bonus.Condition) ? null : JsonConvert.DeserializeObject<Common.Models.Bonus.BonusCondition>(bonus.Condition)
            };
        }

        public static Bonu MapToBonus(this ApiFreeSpin freeSpin)
        {
            return new Bonu
            {
                Id = freeSpin.Id ?? 0,
                Name = freeSpin.Name,
                PartnerId = freeSpin.PartnerId,
                Status = freeSpin.Status,
                Sequence = freeSpin.SpinsCount,
                StartTime = freeSpin.StartTime,
                FinishTime = freeSpin.FinishTime,
            };
        }

        public static TriggerSetting MapToTriggerSetting(this ApiTriggerSetting apiTriggerSetting, double timeZone)
        {
            return new TriggerSetting
            {
                Id = apiTriggerSetting.Id ?? 0,
                Name = apiTriggerSetting.Name,
                Description = apiTriggerSetting.Description,
                Type = apiTriggerSetting.Type,
                PartnerId = apiTriggerSetting.PartnerId,
                StartTime = apiTriggerSetting.StartTime,
                FinishTime = apiTriggerSetting.FinishTime,
                Percent = apiTriggerSetting.Percent ?? 0,
                BonusSettingCodes = !string.IsNullOrEmpty(apiTriggerSetting.BonusSettingCodes) ? apiTriggerSetting.BonusSettingCodes : apiTriggerSetting.PromoCode,
                MinAmount = apiTriggerSetting.MinAmount,
                MaxAmount = apiTriggerSetting.MaxAmount,
                MinBetCount = apiTriggerSetting.MinBetCount,
                SegmentId = apiTriggerSetting.SegmentId,
                DayOfWeek = apiTriggerSetting.DayOfWeek,
                UpToAmount = apiTriggerSetting.UpToAmount,
                Status = apiTriggerSetting.Status,
                BonusPaymentSystemSettings = apiTriggerSetting.PaymentSystemIds?.Ids?.Select(x => new BonusPaymentSystemSetting
                {
                    PaymentSystemId = x,
                    TriggerId = apiTriggerSetting.Id,
                    Type = apiTriggerSetting.PaymentSystemIds.Type ?? (int)BonusSettingConditionTypes.InSet
                }).ToList(),
                TriggerProductSettings = apiTriggerSetting.Products == null ? null : apiTriggerSetting.Products.Select(x => new TriggerProductSetting
                {
                    Id = x.Id ?? 0,
                    ProductId = x.ProductId,
                    Percent = x.Percent ?? -1
                }).ToList(),
                AmountCurrencySettings = apiTriggerSetting.AmountSettings == null ? null : apiTriggerSetting.AmountSettings.Select(x => new AmountCurrencySetting
                {
                    CurrencyId = x.CurrencyId,
                    BonusId = null,
                    TriggerId = apiTriggerSetting.Id ?? 0,
                    MinAmount = x.MinAmount,
                    MaxAmount = x.MaxAmount,
                    UpToAmount = x.UpToAmount
                }).ToList()
            };
        }

        public static ApiTriggerSetting MapToApiTriggerSetting(this TriggerSetting triggerSetting, double timeZone)
        {
            var triggerSettingTypeNames = CacheManager.GetEnumerations(Constants.EnumerationTypes.TriggerTypes, Constants.DefaultLanguageId);
            return new ApiTriggerSetting
            {
                Id = triggerSetting.Id,
                Name = triggerSetting.Name,
                Description = triggerSetting.Description,
                TranslationId = triggerSetting.TranslationId,
                Type = triggerSetting.Type,
                TypeName = triggerSettingTypeNames.FirstOrDefault(x => x.Value == triggerSetting.Type)?.NickName,
                PartnerId = triggerSetting.PartnerId,
                StartTime = triggerSetting.StartTime.GetGMTDateFromUTC(timeZone),
                FinishTime = triggerSetting.FinishTime.GetGMTDateFromUTC(timeZone),
                Percent = triggerSetting.Percent,
                BonusSettingCodes = triggerSetting.BonusSettingCodes,
                MinAmount = triggerSetting.MinAmount,
                MaxAmount = triggerSetting.MaxAmount,
                MinBetCount = triggerSetting.MinBetCount,
                SegmentId = triggerSetting.SegmentId,
                DayOfWeek = triggerSetting.DayOfWeek,
                CreationTime = triggerSetting.CreationTime.GetGMTDateFromUTC(timeZone),
                UpdateTime = triggerSetting.LastUpdateTime.GetGMTDateFromUTC(timeZone),
                Order = triggerSetting.Order,
                Conditions = string.IsNullOrEmpty(triggerSetting.Condition) || triggerSetting.Type == (int)TriggerTypes.NthDeposit ||
                    triggerSetting.Type == (int)TriggerTypes.ManualEvent ?
                    null : JsonConvert.DeserializeObject<Common.Models.Bonus.BonusCondition>(triggerSetting.Condition),
                Status = triggerSetting.Status ?? (int)TriggerStatuses.Active,
                ManualEventStatus = (triggerSetting.Type != (int)TriggerTypes.ManualEvent || string.IsNullOrEmpty(triggerSetting.Condition)) ? 0 :
                    Convert.ToInt32(triggerSetting.Condition),
                Sequence = triggerSetting.Type == (int)TriggerTypes.NthDeposit ? triggerSetting.Condition : string.Empty,
                Products = triggerSetting.TriggerProductSettings == null ? new List<ApiBonusProducts>() :
                    triggerSetting.TriggerProductSettings.Select(x => new ApiBonusProducts
                    {
                        Id = x.Id,
                        ProductId = x.ProductId,
                        Percent = x.Percent
                    }).ToList(),
                PaymentSystemIds = triggerSetting.BonusPaymentSystemSettings.Any() ? new ApiSetting
                {
                    Type = triggerSetting.BonusPaymentSystemSettings.First().Type,
                    Ids = triggerSetting.BonusPaymentSystemSettings.Select(x => x.PaymentSystemId).ToList()
                } : null,
                UpToAmount = triggerSetting.UpToAmount,
                AmountSettings = triggerSetting.AmountCurrencySettings.Select(x => new ApiAmountSetting {
                    CurrencyId = x.CurrencyId,
                    MinAmount = x.MinAmount,
                    MaxAmount = x.MaxAmount,
                    UpToAmount = x.UpToAmount
                }).ToList()
            };
        }

        public static ApiTriggerSetting MapToApiTriggerSetting(this TriggerSettingItem triggerSetting, double timeZone, int clientId)
        {
            var triggerSettingTypeNames = CacheManager.GetEnumerations(Constants.EnumerationTypes.TriggerTypes, Constants.DefaultLanguageId);
            return new ApiTriggerSetting
            {
                Id = triggerSetting.Id,
                Name = triggerSetting.Name,
                Description = triggerSetting.Description,
                TranslationId = triggerSetting.TranslationId,
                Type = triggerSetting.Type,
                TypeName = triggerSettingTypeNames.FirstOrDefault(x => x.Value == triggerSetting.Type)?.NickName,
                PartnerId = triggerSetting.PartnerId,
                StartTime = triggerSetting.StartTime.GetGMTDateFromUTC(timeZone),
                FinishTime = triggerSetting.FinishTime.GetGMTDateFromUTC(timeZone),
                Percent = triggerSetting.Percent,
                BonusSettingCodes = triggerSetting.BonusSettingCodes,
                MinAmount = triggerSetting.MinAmount,
                MaxAmount = triggerSetting.MaxAmount,
                CreationTime = triggerSetting.CreationTime.GetGMTDateFromUTC(timeZone),
                UpdateTime = triggerSetting.LastUpdateTime.GetGMTDateFromUTC(timeZone),
                Order = triggerSetting.Order,
                Status = triggerSetting.Status,
                ClientId = clientId,
                SourceAmount = triggerSetting.SourceAmount,
                MinBetCount = triggerSetting.MinBetCount,
                BetCount = triggerSetting.BetCount,
                WageringAmount = triggerSetting.WageringAmount
            };
        }

        public static TriggerGroup MapToTriggerGroup(this ApiTriggerGroup apiTriggerGroup)
        {
            return new TriggerGroup
            {
                Id = apiTriggerGroup.Id,
                Name = apiTriggerGroup.Name,
                BonusId = apiTriggerGroup.BonusId,
                Type = apiTriggerGroup.Type,
                Priority = apiTriggerGroup.Priority
            };
        }

        public static ComplimentaryPointRate MapToComplimentaryRate(this ApiComplimentaryPointRate complimentaryCoinRate)
        {
            return new ComplimentaryPointRate
            {
                Id = complimentaryCoinRate.Id,
                PartnerId = complimentaryCoinRate.PartnerId,
                ProductId = complimentaryCoinRate.ProductId,
                CurrencyId = complimentaryCoinRate.CurrencyId,
                Rate = complimentaryCoinRate.Rate ?? -1
            };
        }

        public static ApiComplimentaryPointRate MapToApiComplimentaryRate(this ComplimentaryPointRate complimentaryCoinRate, double timeZone)
        {
            return new ApiComplimentaryPointRate
            {
                Id = complimentaryCoinRate.Id,
                PartnerId = complimentaryCoinRate.PartnerId,
                ProductId = complimentaryCoinRate.ProductId,
                CurrencyId = complimentaryCoinRate.CurrencyId,
                Rate = complimentaryCoinRate.Rate,
                CreationDate = complimentaryCoinRate.CreationDate.GetGMTDateFromUTC(timeZone),
                LastUpdateDate = complimentaryCoinRate.LastUpdateDate.GetGMTDateFromUTC(timeZone),
            };
        }

        public static ApiTriggerGroup MapToApiTriggerGroup(this TriggerGroup triggerGroup)
        {
            return new ApiTriggerGroup
            {
                Id = triggerGroup.Id,
                Name = triggerGroup.Name,
                BonusId = triggerGroup.BonusId,
                Type = triggerGroup.Type,
                Priority = triggerGroup.Priority
            };
        }

        public static Jackpot MapToJackpot(this ApiJackpot apiJackpot)
        {
            var currentDate = DateTime.UtcNow;
            return new Jackpot
            {
                Id = apiJackpot.Id,
                Name = apiJackpot.Name,
                PartnerId = apiJackpot.PartnerId,
                Type = apiJackpot.Type,
                Amount = apiJackpot.Amount,
                FinishTime = apiJackpot.FinishTime,
                JackpotSettings = apiJackpot.Products?.Select(
                     x => new JackpotSetting
                     {
                         Id = x.Id ?? 0,
                         ProductId = x.ProductId,
                         Percent = x.Percent ?? -1,
                         CreationDate = currentDate,
                         LastUpdateDate = currentDate
                     }).ToList(),
            };
        }

        public static ApiJackpot MapToApiJackpot(this Jackpot jackpot, double timeZone)
        {
            return new ApiJackpot
            {
                Id = jackpot.Id,
                Name = jackpot.Name,
                PartnerId = jackpot.PartnerId,
                Type = jackpot.Type,
                Amount = jackpot.Amount,
                FinishTime = jackpot.FinishTime,
                WinnedClient = jackpot.WinnerId,
                Products = jackpot.JackpotSettings.Select(x => new ApiBonusProducts
                {
                    Id = x.Id,
                    ProductId = x.ProductId,
                    Percent = x.Percent,
                    CreationDate = x.CreationDate.GetGMTDateFromUTC(timeZone),
                    LastUpdateDate = x.LastUpdateDate.GetGMTDateFromUTC(timeZone)
                }).ToList(),
                CreationDate = jackpot.CreationDate.GetGMTDateFromUTC(timeZone),
                LastUpdateDate = jackpot.LastUpdateDate.GetGMTDateFromUTC(timeZone),
            };
        }

        #endregion

        #region Filters

        #region Reporting

        #region Internet Reports
        public static FilterfnDocument MapToFilterfnDocument(this ApiFilterfnDocument apiFilterClientDocument)
        {
            return new FilterfnDocument
            {
                ClientId = apiFilterClientDocument.ClientId,
                FromDate = apiFilterClientDocument.FromDate,
                ToDate = apiFilterClientDocument.ToDate,
                AccountId = apiFilterClientDocument.AccountId,
                Ids = apiFilterClientDocument.Ids == null ? new FiltersOperation() : apiFilterClientDocument.Ids.MapToFiltersOperation(),
                ExternalTransactionIds = apiFilterClientDocument.ExternalTransactionIds == null ? new FiltersOperation() : apiFilterClientDocument.ExternalTransactionIds.MapToFiltersOperation(),
                Amounts = apiFilterClientDocument.Amounts == null ? new FiltersOperation() : apiFilterClientDocument.Amounts.MapToFiltersOperation(),
                States = apiFilterClientDocument.States == null ? new FiltersOperation() : apiFilterClientDocument.States.MapToFiltersOperation(),
                OperationTypeIds = apiFilterClientDocument.OperationTypeIds == null ? new FiltersOperation() : apiFilterClientDocument.OperationTypeIds.MapToFiltersOperation(),
                PaymentRequestIds = apiFilterClientDocument.PaymentRequestIds == null ? new FiltersOperation() : apiFilterClientDocument.PaymentRequestIds.MapToFiltersOperation(),
                PaymentSystemIds = apiFilterClientDocument.PaymentSystemIds == null ? new FiltersOperation() : apiFilterClientDocument.PaymentSystemIds.MapToFiltersOperation(),
                PaymentSystemNames = apiFilterClientDocument.PaymentSystemNames == null ? new FiltersOperation() : apiFilterClientDocument.PaymentSystemNames.MapToFiltersOperation(),
                RoundIds = apiFilterClientDocument.RoundIds == null ? new FiltersOperation() : apiFilterClientDocument.RoundIds.MapToFiltersOperation(),
                ProductIds = apiFilterClientDocument.ProductIds == null ? new FiltersOperation() : apiFilterClientDocument.ProductIds.MapToFiltersOperation(),
                ProductNames = apiFilterClientDocument.ProductNames == null ? new FiltersOperation() : apiFilterClientDocument.ProductNames.MapToFiltersOperation(),
                GameProviderIds = apiFilterClientDocument.GameProviderIds == null ? new FiltersOperation() : apiFilterClientDocument.GameProviderIds.MapToFiltersOperation(),
                GameProviderNames = apiFilterClientDocument.GameProviderNames == null ? new FiltersOperation() : apiFilterClientDocument.GameProviderNames.MapToFiltersOperation(),
                LastUpdateTimes = apiFilterClientDocument.LastUpdateTimes == null ? new FiltersOperation() : apiFilterClientDocument.LastUpdateTimes.MapToFiltersOperation(),
                SkipCount = apiFilterClientDocument.SkipCount,
                TakeCount = apiFilterClientDocument.TakeCount,
                OrderBy = apiFilterClientDocument.OrderBy,
                FieldNameToOrderBy = apiFilterClientDocument.FieldNameToOrderBy,

            };
        }


        public static List<FilterInternetBet> MaptoFilterInternetBets(this IEnumerable<ApiFilterInternetBet> apiFilterInternetBets)
        {
            return apiFilterInternetBets.Select(MapToFilterInternetBet).ToList();
        }

        public static FilterInternetBet MapToFilterInternetBet(this ApiFilterInternetBet apiFilterInternetBet)
        {
            return new FilterInternetBet
            {
                PartnerId = apiFilterInternetBet.PartnerId,
                AccountId = apiFilterInternetBet.AccountId,
                FromDate = apiFilterInternetBet.BetDateFrom,
                ToDate = apiFilterInternetBet.BetDateBefore,
                Ids = apiFilterInternetBet.BetDocumentIds == null ? new FiltersOperation() : apiFilterInternetBet.BetDocumentIds.MapToFiltersOperation(),
                ClientIds = apiFilterInternetBet.ClientIds == null ? new FiltersOperation() : apiFilterInternetBet.ClientIds.MapToFiltersOperation(),
                UserIds = apiFilterInternetBet.UserIds == null ? new FiltersOperation() : apiFilterInternetBet.UserIds.MapToFiltersOperation(),
                Names = apiFilterInternetBet.Names == null ? new FiltersOperation() : apiFilterInternetBet.Names.MapToFiltersOperation(),
                UserNames = apiFilterInternetBet.UserNames == null ? new FiltersOperation() : apiFilterInternetBet.UserNames.MapToFiltersOperation(),
                Categories = apiFilterInternetBet.Categories == null ? new FiltersOperation() : apiFilterInternetBet.Categories.MapToFiltersOperation(),
                ProductIds = apiFilterInternetBet.ProductIds == null ? new FiltersOperation() : apiFilterInternetBet.ProductIds.MapToFiltersOperation(),
                ProductNames = apiFilterInternetBet.ProductNames == null ? new FiltersOperation() : apiFilterInternetBet.ProductNames.MapToFiltersOperation(),
                ProviderNames = apiFilterInternetBet.ProviderNames == null ? new FiltersOperation() : apiFilterInternetBet.ProviderNames.MapToFiltersOperation(),
                SubproviderIds = apiFilterInternetBet.SubproviderIds == null ? new FiltersOperation() : apiFilterInternetBet.SubproviderIds.MapToFiltersOperation(),
                SubproviderNames = apiFilterInternetBet.SubproviderNames == null ? new FiltersOperation() : apiFilterInternetBet.SubproviderNames.MapToFiltersOperation(),
                Currencies = apiFilterInternetBet.CurrencyIds == null ? new FiltersOperation() : apiFilterInternetBet.CurrencyIds.MapToFiltersOperation(),
                RoundIds = apiFilterInternetBet.RoundIds == null ? new FiltersOperation() : apiFilterInternetBet.RoundIds.MapToFiltersOperation(),
                DeviceTypes = apiFilterInternetBet.DeviceTypes == null ? new FiltersOperation() : apiFilterInternetBet.DeviceTypes.MapToFiltersOperation(),
                ClientIps = apiFilterInternetBet.ClientIps == null ? new FiltersOperation() : apiFilterInternetBet.ClientIps.MapToFiltersOperation(),
                Countries = apiFilterInternetBet.Countries == null ? new FiltersOperation() : apiFilterInternetBet.Countries.MapToFiltersOperation(),
                States = apiFilterInternetBet.States == null ? new FiltersOperation() : apiFilterInternetBet.States.MapToFiltersOperation(),
                BetTypes = apiFilterInternetBet.BetTypes == null ? new FiltersOperation() : apiFilterInternetBet.BetTypes.MapToFiltersOperation(),
                PossibleWins = apiFilterInternetBet.PossibleWins == null ? new FiltersOperation() : apiFilterInternetBet.PossibleWins.MapToFiltersOperation(),
                BetAmounts = apiFilterInternetBet.BetAmounts == null ? new FiltersOperation() : apiFilterInternetBet.BetAmounts.MapToFiltersOperation(),
                OriginalBetAmounts = apiFilterInternetBet.OriginalBetAmounts == null ? new FiltersOperation() : apiFilterInternetBet.OriginalBetAmounts.MapToFiltersOperation(),
                Coefficients = apiFilterInternetBet.Coefficients == null ? new FiltersOperation() : apiFilterInternetBet.Coefficients.MapToFiltersOperation(),
                WinAmounts = apiFilterInternetBet.WinAmounts == null ? new FiltersOperation() : apiFilterInternetBet.WinAmounts.MapToFiltersOperation(),
                OriginalWinAmounts = apiFilterInternetBet.OriginalWinAmounts == null ? new FiltersOperation() : apiFilterInternetBet.OriginalWinAmounts.MapToFiltersOperation(),
                BetDates = apiFilterInternetBet.BetDates == null ? new FiltersOperation() : apiFilterInternetBet.BetDates.MapToFiltersOperation(),
                WinDates = apiFilterInternetBet.CalculationDates == null ? new FiltersOperation() : apiFilterInternetBet.CalculationDates.MapToFiltersOperation(),
                LastUpdateTimes = apiFilterInternetBet.LastUpdateTimes == null ? new FiltersOperation() : apiFilterInternetBet.LastUpdateTimes.MapToFiltersOperation(),
                BonusIds = apiFilterInternetBet.BonusIds == null ? new FiltersOperation() : apiFilterInternetBet.BonusIds.MapToFiltersOperation(),
                GGRs = apiFilterInternetBet.GGRs == null ? new FiltersOperation() : apiFilterInternetBet.GGRs.MapToFiltersOperation(),
                Rakes = apiFilterInternetBet.Rakes == null ? new FiltersOperation() : apiFilterInternetBet.Rakes.MapToFiltersOperation(),
                BonusAmounts = apiFilterInternetBet.BonusAmounts == null ? new FiltersOperation() : apiFilterInternetBet.BonusAmounts.MapToFiltersOperation(),
                OriginalBonusAmounts = apiFilterInternetBet.OriginalBonusAmounts == null ? new FiltersOperation() : apiFilterInternetBet.OriginalBonusAmounts.MapToFiltersOperation(),
                Balances = apiFilterInternetBet.Balances == null ? new FiltersOperation() : apiFilterInternetBet.Balances.MapToFiltersOperation(),
                TotalBetsCounts = apiFilterInternetBet.TotalBetsCounts == null ? new FiltersOperation() : apiFilterInternetBet.TotalBetsCounts.MapToFiltersOperation(),
                TotalBetsAmounts = apiFilterInternetBet.TotalBetsAmounts == null ? new FiltersOperation() : apiFilterInternetBet.TotalBetsAmounts.MapToFiltersOperation(),
                TotalWinsAmounts = apiFilterInternetBet.TotalWinsAmounts == null ? new FiltersOperation() : apiFilterInternetBet.TotalWinsAmounts.MapToFiltersOperation(),
                MaxBetAmounts = apiFilterInternetBet.MaxBetAmounts == null ? new FiltersOperation() : apiFilterInternetBet.MaxBetAmounts.MapToFiltersOperation(),
                TotalDepositsCounts = apiFilterInternetBet.TotalDepositsCounts == null ? new FiltersOperation() : apiFilterInternetBet.TotalDepositsCounts.MapToFiltersOperation(),
                TotalDepositsAmounts = apiFilterInternetBet.TotalDepositsAmounts == null ? new FiltersOperation() : apiFilterInternetBet.TotalDepositsAmounts.MapToFiltersOperation(),
                TotalWithdrawalsCounts = apiFilterInternetBet.TotalWithdrawalsCounts == null ? new FiltersOperation() : apiFilterInternetBet.TotalWithdrawalsCounts.MapToFiltersOperation(),
                TotalWithdrawalsAmounts = apiFilterInternetBet.TotalWithdrawalsAmounts == null ? new FiltersOperation() : apiFilterInternetBet.TotalWithdrawalsAmounts.MapToFiltersOperation(),
                SkipCount = apiFilterInternetBet.SkipCount,
                TakeCount = Math.Min(apiFilterInternetBet.TakeCount, 5000),
                OrderBy = apiFilterInternetBet.OrderBy,
                FieldNameToOrderBy = apiFilterInternetBet.FieldNameToOrderBy,
                AgentId = apiFilterInternetBet.AgentId
            };
        }

        public static FilterInternetGame MapToFilterInternetGame(this ApiFilterInternetBet apiFilterInternetBet)
        {
            return new FilterInternetGame
            {
                PartnerId = apiFilterInternetBet.PartnerId,
                FromDate = apiFilterInternetBet.BetDateFrom,
                ToDate = apiFilterInternetBet.BetDateBefore,
                ProductIds = apiFilterInternetBet.ProductIds == null ? new FiltersOperation() : apiFilterInternetBet.ProductIds.MapToFiltersOperation(),
                ProductNames = apiFilterInternetBet.ProductNames == null ? new FiltersOperation() : apiFilterInternetBet.ProductNames.MapToFiltersOperation(),
                Currencies = apiFilterInternetBet.CurrencyIds == null ? new FiltersOperation() : apiFilterInternetBet.CurrencyIds.MapToFiltersOperation(),
                BetAmounts = apiFilterInternetBet.BetAmounts == null ? new FiltersOperation() : apiFilterInternetBet.BetAmounts.MapToFiltersOperation(),
                WinAmounts = apiFilterInternetBet.WinAmounts == null ? new FiltersOperation() : apiFilterInternetBet.WinAmounts.MapToFiltersOperation(),
                OriginalBetAmounts = apiFilterInternetBet.OriginalBetAmounts == null ? new FiltersOperation() : apiFilterInternetBet.OriginalBetAmounts.MapToFiltersOperation(),
                OriginalWinAmounts = apiFilterInternetBet.OriginalWinAmounts == null ? new FiltersOperation() : apiFilterInternetBet.OriginalWinAmounts.MapToFiltersOperation(),
                GGRs = apiFilterInternetBet.GGRs == null ? new FiltersOperation() : apiFilterInternetBet.GGRs.MapToFiltersOperation(),
                SkipCount = apiFilterInternetBet.SkipCount,
                TakeCount = Math.Min(apiFilterInternetBet.TakeCount, 5000),
                OrderBy = apiFilterInternetBet.OrderBy,
                FieldNameToOrderBy = apiFilterInternetBet.FieldNameToOrderBy
            };
        }

        #endregion

        #region BetShop Reports
        public static FilterReportByClientIdentity MapToFilterClientIdentity(this ApiFilterReportByClientIdentity filter)
        {
            return new FilterReportByClientIdentity
            {
                FromDate = filter.FromDate,
                ToDate = filter.ToDate,
                SkipCount = filter.SkipCount,
                TakeCount = filter.TakeCount,
                OrderBy = filter.OrderBy,
                FieldNameToOrderBy = filter.FieldNameToOrderBy,
                HasNote = filter.HasNote,
                Ids = filter.Ids == null ? new FiltersOperation() : filter.Ids.MapToFiltersOperation(),
                ClientIds = filter.ClientIds == null ? new FiltersOperation() : filter.ClientIds.MapToFiltersOperation(),
                UserNames = filter.UserNames == null ? new FiltersOperation() : filter.UserNames.MapToFiltersOperation(),
                PartnerIds = filter.PartnerIds == null ? new FiltersOperation() : filter.PartnerIds.MapToFiltersOperation(),
                UserIds = filter.UserIds == null ? new FiltersOperation() : filter.UserIds.MapToFiltersOperation(),
                DocumentTypeIds = filter.DocumentTypeIds == null ? new FiltersOperation() : filter.DocumentTypeIds.MapToFiltersOperation(),
                Statuses = filter.States == null ? new FiltersOperation() : filter.States.MapToFiltersOperation(),
                ExpirationTimes = filter.ExpirationTimes == null ? new FiltersOperation() : filter.ExpirationTimes.MapToFiltersOperation(),
                CreationTimes = filter.CreationTimes == null ? new FiltersOperation() : filter.CreationTimes.MapToFiltersOperation(),
                LastUpdateTimes = filter.LastUpdateTimes == null ? new FiltersOperation() : filter.LastUpdateTimes.MapToFiltersOperation()
            };
        }

        public static FilterBetShopBet MapToFilterBetShopBet(this ApiFilterBetShopBet filter)
        {
            return new FilterBetShopBet
            {
                SkipCount = filter.SkipCount,
                TakeCount = Math.Min(filter.TakeCount, 5000),
                PartnerId = filter.PartnerId,
                FromDate = filter.BetDateFrom,
                ToDate = filter.BetDateBefore,
                BetShopGroupIds = !filter.BetShopGroupId.HasValue ? new FiltersOperation() :
                                  new ApiFiltersOperation
                                  {
                                      IsAnd = true,
                                      ApiOperationTypeList = new List<ApiFiltersOperationType>
                                      {new ApiFiltersOperationType{OperationTypeId =1, IntValue = (int)filter.BetShopGroupId } }
                                  }.MapToFiltersOperation(),
                Ids = filter.Ids == null ? new FiltersOperation() : filter.Ids.MapToFiltersOperation(),
                CashierIds = filter.CashierIds == null ? new FiltersOperation() : filter.CashierIds.MapToFiltersOperation(),
                CashDeskIds = filter.CashDeskIds == null ? new FiltersOperation() : filter.CashDeskIds.MapToFiltersOperation(),
                BetShopIds = filter.BetShopIds == null ? new FiltersOperation() : filter.BetShopIds.MapToFiltersOperation(),
                BetShopNames = filter.BetShopNames == null ? new FiltersOperation() : filter.BetShopNames.MapToFiltersOperation(),
                BetShopGroupNames = filter.BetShopGroupNames == null ? new FiltersOperation() : filter.BetShopGroupNames.MapToFiltersOperation(),
                ProductIds = filter.ProductIds == null ? new FiltersOperation() : filter.ProductIds.MapToFiltersOperation(),
                ProductNames = filter.ProductNames == null ? new FiltersOperation() : filter.ProductNames.MapToFiltersOperation(),
                ProviderIds = filter.ProviderIds == null ? new FiltersOperation() : filter.ProviderIds.MapToFiltersOperation(),
                ProviderNames = filter.ProviderNames == null ? new FiltersOperation() : filter.ProviderNames.MapToFiltersOperation(),
                Currencies = filter.Currencies == null ? new FiltersOperation() : filter.Currencies.MapToFiltersOperation(),
                RoundIds = filter.RoundIds == null ? new FiltersOperation() : filter.RoundIds.MapToFiltersOperation(),
                States = filter.States == null ? new FiltersOperation() : filter.States.MapToFiltersOperation(),
                BetTypes = filter.BetTypes == null ? new FiltersOperation() : filter.BetTypes.MapToFiltersOperation(),
                PossibleWins = filter.PossibleWins == null ? new FiltersOperation() : filter.PossibleWins.MapToFiltersOperation(),
                BetAmounts = filter.BetAmounts == null ? new FiltersOperation() : filter.BetAmounts.MapToFiltersOperation(),
                WinAmounts = filter.WinAmounts == null ? new FiltersOperation() : filter.WinAmounts.MapToFiltersOperation(),
                OriginalBetAmounts = filter.OriginalBetAmounts == null ? new FiltersOperation() : filter.OriginalBetAmounts.MapToFiltersOperation(),
                OriginalWinAmounts = filter.OriginalWinAmounts == null ? new FiltersOperation() : filter.OriginalWinAmounts.MapToFiltersOperation(),
                Barcodes = filter.Barcodes == null ? new FiltersOperation() : filter.Barcodes.MapToFiltersOperation(),
                TicketNumbers = filter.TicketNumbers == null ? new FiltersOperation() : filter.TicketNumbers.MapToFiltersOperation(),
                OrderBy = filter.OrderBy,
                FieldNameToOrderBy = filter.FieldNameToOrderBy
            };
        }

        public static FilterBetShopBet MapToFilterBetShopBet(this ApiFilterReportByBetShop filter)
        {
            return new FilterBetShopBet
            {
                PartnerId = filter.PartnerId,
                FromDate = filter.BetDateFrom,
                ToDate = filter.BetDateBefore,
                ProductIds = filter.ProductIds == null ? new FiltersOperation() : filter.ProductIds.MapToFiltersOperation(),
                BetShopIds = filter.BetShopIds == null ? new FiltersOperation() : filter.BetShopIds.MapToFiltersOperation(),
                BetShopGroupIds = filter.BetShopGroupIds == null ? new FiltersOperation() : filter.BetShopGroupIds.MapToFiltersOperation(),
                BetShopNames = filter.BetShopNames == null ? new FiltersOperation() : filter.BetShopNames.MapToFiltersOperation(),
                Currencies = filter.Currencies == null ? new FiltersOperation() : filter.Currencies.MapToFiltersOperation()
            };
        }

        public static FilterReportByBetShopPayment MapToFilterReportByBetShopPayment(this ApiFilterReportByBetShopPayment filter)
        {
            return new FilterReportByBetShopPayment
            {
                SkipCount = filter.SkipCount,
                TakeCount = filter.TakeCount,
                FromDate = filter.FromDate,
                ToDate = filter.ToDate,
                BetShopIds = filter.BetShopIds == null ? new FiltersOperation() : filter.BetShopIds.MapToFiltersOperation(),
                GroupIds = filter.GroupIds == null ? new FiltersOperation() : filter.GroupIds.MapToFiltersOperation(),
                BetShopNames = filter.BetShopNames == null ? new FiltersOperation() : filter.BetShopNames.MapToFiltersOperation(),
                PendingDepositCounts = filter.TotalPendingDepositsCounts == null ? new FiltersOperation() : filter.TotalPendingDepositsCounts.MapToFiltersOperation(),
                PendingDepositAmounts = filter.TotalPendingDepositsAmounts == null ? new FiltersOperation() : filter.TotalPendingDepositsAmounts.MapToFiltersOperation(),
                PayedDepositCounts = filter.TotalPayedDepositsCounts == null ? new FiltersOperation() : filter.TotalPayedDepositsCounts.MapToFiltersOperation(),
                PayedDepositAmounts = filter.TotalPayedDepositsAmounts == null ? new FiltersOperation() : filter.TotalPayedDepositsAmounts.MapToFiltersOperation(),
                CanceledDepositCounts = filter.TotalCanceledDepositsCounts == null ? new FiltersOperation() : filter.TotalCanceledDepositsCounts.MapToFiltersOperation(),
                CanceledDepositAmounts = filter.TotalCanceledDepositsAmounts == null ? new FiltersOperation() : filter.TotalCanceledDepositsAmounts.MapToFiltersOperation(),
                PendingWithdrawalCounts = filter.TotalPendingWithdrawalsCounts == null ? new FiltersOperation() : filter.TotalPendingWithdrawalsCounts.MapToFiltersOperation(),
                PendingWithdrawalAmounts = filter.TotalPendingWithdrawalsAmounts == null ? new FiltersOperation() : filter.TotalPendingWithdrawalsAmounts.MapToFiltersOperation(),
                PayedWithdrawalCounts = filter.TotalPayedWithdrawalsCounts == null ? new FiltersOperation() : filter.TotalPayedWithdrawalsCounts.MapToFiltersOperation(),
                PayedWithdrawalAmounts = filter.TotalPayedWithdrawalsAmounts == null ? new FiltersOperation() : filter.TotalPayedWithdrawalsAmounts.MapToFiltersOperation(),
                CanceledWithdrawalCounts = filter.TotalCanceledWithdrawalsCounts == null ? new FiltersOperation() : filter.TotalCanceledWithdrawalsCounts.MapToFiltersOperation(),
                CanceledWithdrawalAmounts = filter.TotalCanceledWithdrawalsAmounts == null ? new FiltersOperation() : filter.TotalCanceledWithdrawalsAmounts.MapToFiltersOperation()
            };
        }

        public static FilterReportByBetShopLimitChanges MapToFilterReportByBetShopLimitChanges(this ApiFilterBetShopLimitChanges filter)
        {
            return new FilterReportByBetShopLimitChanges
            {
                SkipCount = filter.SkipCount,
                TakeCount = filter.TakeCount,
                OrderBy = filter.OrderBy,
                FromDate = filter.FromDate,
                ToDate = filter.ToDate,
                BetShopIds = filter.BetShopIds == null ? new FiltersOperation() : filter.BetShopIds.MapToFiltersOperation(),
                UserIds = filter.UserIds == null ? new FiltersOperation() : filter.UserIds.MapToFiltersOperation()
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

        public static FilterReportByBonus MapToFilterReportByBonus(this ApiFilterReportByBonus filter)
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
                FromDate = filter.FromDate,
                ToDate = filter.ToDate,
                SkipCount = filter.SkipCount,
                TakeCount = filter.TakeCount,
                OrderBy = filter.OrderBy,
                FieldNameToOrderBy = filter.FieldNameToOrderBy,
                Ids = filter.Ids == null ? new FiltersOperation() : filter.Ids.MapToFiltersOperation(),
                PartnerIds = filter.PartnerIds == null ? new FiltersOperation() : filter.PartnerIds.MapToFiltersOperation(),
                BonusIds = filter.BonusIds == null ? new FiltersOperation() : filter.BonusIds.MapToFiltersOperation(),
                BonusNames = filter.BonusNames == null ? new FiltersOperation() : filter.BonusNames.MapToFiltersOperation(),
                BonusTypes = filter.BonusTypes == null ? new FiltersOperation() : filter.BonusTypes.MapToFiltersOperation(),
                BonusStatuses = filter.BonusStatuses == null ? new FiltersOperation() : filter.BonusStatuses.MapToFiltersOperation(),
                ClientIds = filter.ClientIds == null ? new FiltersOperation() : filter.ClientIds.MapToFiltersOperation(),
                UserNames = filter.UserNames == null ? new FiltersOperation() : filter.UserNames.MapToFiltersOperation(),
                FirstNames = filter.FirstNames == null ? new FiltersOperation() : filter.FirstNames.MapToFiltersOperation(),
                LastNames = filter.LastNames == null ? new FiltersOperation() : filter.LastNames.MapToFiltersOperation(),
                CurrencyIds = filter.CurrencyIds == null ? new FiltersOperation() : filter.CurrencyIds.MapToFiltersOperation(),
                Emails = filter.Emails == null ? new FiltersOperation() : filter.Emails.MapToFiltersOperation(),
                MobileNumbers = filter.MobileNumbers == null ? new FiltersOperation() : filter.MobileNumbers.MapToFiltersOperation(),
                CategoryIds = filter.CategoryIds == null ? new FiltersOperation() : filter.CategoryIds.MapToFiltersOperation(),
                BonusPrizes = filter.BonusPrizes == null ? new FiltersOperation() : filter.BonusPrizes.MapToFiltersOperation(),
                TurnoverAmountLefts = filter.TurnoverAmountLefts == null ? new FiltersOperation() : filter.TurnoverAmountLefts.MapToFiltersOperation(),
                RemainingCredits = filter.RemainingCredits == null ? new FiltersOperation() : filter.RemainingCredits.MapToFiltersOperation(),
                WageringTargets = filter.WageringTargets == null ? new FiltersOperation() : filter.WageringTargets.MapToFiltersOperation(),
                FinalAmounts = filter.FinalAmounts == null ? new FiltersOperation() : filter.FinalAmounts.MapToFiltersOperation(),
                ClientBonusStatuses = filter.Statuses == null ? new FiltersOperation() : filter.Statuses.MapToFiltersOperation(),
                AwardingTimes = filter.AwardingTimes == null ? new FiltersOperation() : filter.AwardingTimes.MapToFiltersOperation(),
                CalculationTimes = filter.CalculationTimes == null ? new FiltersOperation() : filter.CalculationTimes.MapToFiltersOperation(),
                CreationTimes = filter.CreationTimes == null ? new FiltersOperation() : filter.CreationTimes.MapToFiltersOperation(),
                ValidUntils = filter.ValidUntils == null ? new FiltersOperation() : filter.ValidUntils.MapToFiltersOperation()
            };
        }

        public static FilterReportByClientSession MapToFilterReportByClientSession(this ApiFilterReportByClientSession filter)
        {
            return new FilterReportByClientSession
            {
                FromDate = filter.FromDate,
                ToDate = filter.ToDate,
                SkipCount = filter.SkipCount,
                TakeCount = filter.TakeCount,
                OrderBy = filter.OrderBy,
                FieldNameToOrderBy = filter.FieldNameToOrderBy,
                Ids = filter.Ids == null ? new FiltersOperation() : filter.Ids.MapToFiltersOperation(),
                PartnerIds = filter.PartnerIds == null ? new FiltersOperation() : filter.PartnerIds.MapToFiltersOperation(),
                ClientIds = filter.ClientIds == null ? new FiltersOperation() : filter.ClientIds.MapToFiltersOperation(),
                FirstNames = filter.FirstNames == null ? new FiltersOperation() : filter.FirstNames.MapToFiltersOperation(),
                LastNames = filter.LastNames == null ? new FiltersOperation() : filter.LastNames.MapToFiltersOperation(),
                UserNames = filter.UserNames == null ? new FiltersOperation() : filter.UserNames.MapToFiltersOperation(),
                LanguageIds = filter.LanguageIds == null ? new FiltersOperation() : filter.LanguageIds.MapToFiltersOperation(),
                Ips = filter.Ips == null ? new FiltersOperation() : filter.Ips.MapToFiltersOperation(),
                Countries = filter.Countries == null ? new FiltersOperation() : filter.Countries.MapToFiltersOperation(),
                States = filter.States == null ? new FiltersOperation() : filter.States.MapToFiltersOperation(),
                LogoutTypes = filter.LogoutTypes == null ? new FiltersOperation() : filter.LogoutTypes.MapToFiltersOperation()
            };
        }
        public static FilterReportByUserSession MapToFilterReportByUserSession(this ApiFilterReportByUserSession filter)
        {
            return new FilterReportByUserSession
            {
                FromDate = filter.FromDate,
                ToDate = filter.ToDate,
                SkipCount = filter.SkipCount,
                TakeCount = filter.TakeCount,
                OrderBy = filter.OrderBy,
                FieldNameToOrderBy = filter.FieldNameToOrderBy,
                PartnerId = filter.PartnerId,
                UserId = filter.UserId,
                Type = filter.Type,
                Ids = filter.Ids == null ? new FiltersOperation() : filter.Ids.MapToFiltersOperation(),
                PartnerIds = filter.PartnerIds == null ? new FiltersOperation() : filter.PartnerIds.MapToFiltersOperation(),
                UserIds = filter.UserIds == null ? new FiltersOperation() : filter.UserIds.MapToFiltersOperation(),
                FirstNames = filter.FirstNames == null ? new FiltersOperation() : filter.FirstNames.MapToFiltersOperation(),
                LastNames = filter.LastNames == null ? new FiltersOperation() : filter.LastNames.MapToFiltersOperation(),
                UserNames = filter.UserNames == null ? new FiltersOperation() : filter.UserNames.MapToFiltersOperation(),
                Emails = filter.Emails == null ? new FiltersOperation() : filter.Emails.MapToFiltersOperation(),
                LanguageIds = filter.LanguageIds == null ? new FiltersOperation() : filter.LanguageIds.MapToFiltersOperation(),
                Ips = filter.Ips == null ? new FiltersOperation() : filter.Ips.MapToFiltersOperation(),
                Types = filter.Types == null ? new FiltersOperation() : filter.Types.MapToFiltersOperation(),
                States = filter.States == null ? new FiltersOperation() : filter.States.MapToFiltersOperation(),
                LogoutTypes = filter.LogoutTypes == null ? new FiltersOperation() : filter.LogoutTypes.MapToFiltersOperation(),
                EndTimes = filter.EndTimes == null ? new FiltersOperation() : filter.EndTimes.MapToFiltersOperation()
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
                PartnerIds = filter.PartnerIds == null ? new FiltersOperation() : filter.PartnerIds.MapToFiltersOperation(),
                ClientIds = filter.ClientIds == null ? new FiltersOperation() : filter.ClientIds.MapToFiltersOperation(),
                Usernames = filter.Usernames == null ? new FiltersOperation() : filter.Usernames.MapToFiltersOperation(),
                DepositLimitDailys = filter.DepositLimitDailys == null ? new FiltersOperation() : filter.DepositLimitDailys.MapToFiltersOperation(),
                DepositLimitWeeklys = filter.DepositLimitWeeklys == null ? new FiltersOperation() : filter.DepositLimitWeeklys.MapToFiltersOperation(),
                DepositLimitMonthlys = filter.DepositLimitMonthlys == null ? new FiltersOperation() : filter.DepositLimitMonthlys.MapToFiltersOperation(),
                TotalBetAmountLimitDailys = filter.TotalBetAmountLimitDailys == null ? new FiltersOperation() : filter.TotalBetAmountLimitDailys.MapToFiltersOperation(),
                TotalBetAmountLimitWeeklys = filter.TotalBetAmountLimitWeeklys == null ? new FiltersOperation() : filter.TotalBetAmountLimitWeeklys.MapToFiltersOperation(),
                TotalBetAmountLimitMonthlys = filter.TotalBetAmountLimitMonthlys == null ? new FiltersOperation() : filter.TotalBetAmountLimitMonthlys.MapToFiltersOperation(),
                TotalLossLimitDailys = filter.TotalLossLimitDailys == null ? new FiltersOperation() : filter.TotalLossLimitDailys.MapToFiltersOperation(),
                TotalLossLimitWeeklys = filter.TotalLossLimitWeeklys == null ? new FiltersOperation() : filter.TotalLossLimitWeeklys.MapToFiltersOperation(),
                TotalLossLimitMonthlys = filter.TotalLossLimitMonthlys == null ? new FiltersOperation() : filter.TotalLossLimitMonthlys.MapToFiltersOperation(),
                SystemDepositLimitDailys = filter.SystemDepositLimitDailys == null ? new FiltersOperation() : filter.SystemDepositLimitDailys.MapToFiltersOperation(),
                SystemDepositLimitWeeklys = filter.SystemDepositLimitWeeklys == null ? new FiltersOperation() : filter.SystemDepositLimitWeeklys.MapToFiltersOperation(),
                SystemDepositLimitMonthlys = filter.SystemDepositLimitMonthlys == null ? new FiltersOperation() : filter.SystemDepositLimitMonthlys.MapToFiltersOperation(),
                SystemTotalBetAmountLimitDailys = filter.SystemTotalBetAmountLimitDailys == null ? new FiltersOperation() : filter.SystemTotalBetAmountLimitDailys.MapToFiltersOperation(),
                SystemTotalBetAmountLimitWeeklys = filter.SystemTotalBetAmountLimitWeeklys == null ? new FiltersOperation() : filter.SystemTotalBetAmountLimitWeeklys.MapToFiltersOperation(),
                SystemTotalBetAmountLimitMonthlys = filter.SystemTotalBetAmountLimitMonthlys == null ? new FiltersOperation() : filter.SystemTotalBetAmountLimitMonthlys.MapToFiltersOperation(),
                SystemTotalLossLimitDailys = filter.SystemTotalLossLimitDailys == null ? new FiltersOperation() : filter.SystemTotalLossLimitDailys.MapToFiltersOperation(),
                SystemTotalLossLimitWeeklys = filter.SystemTotalLossLimitWeeklys == null ? new FiltersOperation() : filter.SystemTotalLossLimitWeeklys.MapToFiltersOperation(),
                SystemTotalLossLimitMonthlys = filter.SystemTotalLossLimitMonthlys == null ? new FiltersOperation() : filter.SystemTotalLossLimitMonthlys.MapToFiltersOperation(),
                SessionLimits = filter.SessionLimits == null ? new FiltersOperation() : filter.SessionLimits.MapToFiltersOperation(),
                SystemSessionLimits = filter.SystemSessionLimits == null ? new FiltersOperation() : filter.SystemSessionLimits.MapToFiltersOperation()
            };
        }

        public static FilterReportByObjectChangeHistory MapToFilterObjectChangeHistory(this ApiFilterReportByObjectChangeHistory filter)
        {
            return new FilterReportByObjectChangeHistory
            {
                FromDate = filter.FromDate,
                ToDate = filter.ToDate,
                SkipCount = filter.SkipCount,
                TakeCount = filter.TakeCount,
                OrderBy = filter.OrderBy,
                FieldNameToOrderBy = filter.FieldNameToOrderBy,
                PartnerId = filter.PartnerId,
                ObjectId = filter.ObjectId,
                ObjectTypeId = filter.ObjectTypeId,
                Ids = filter.Ids == null ? new FiltersOperation() : filter.Ids.MapToFiltersOperation(),
                ObjectIds = filter.ObjectIds == null ? new FiltersOperation() : filter.ObjectIds.MapToFiltersOperation(),
                UserIds = filter.UserIds == null ? new FiltersOperation() : filter.UserIds.MapToFiltersOperation()
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

        public static FilterReportByProvider MapToFilterReportByProvider(this ApiFilterReportByProvider filter)
        {
            return new FilterReportByProvider
            {
                FromDate = filter.FromDate,
                ToDate = filter.ToDate,
                ProviderNames = filter.ProviderNames == null ? new FiltersOperation() : filter.ProviderNames.MapToFiltersOperation(),
                Currencies = filter.Currencies == null ? new FiltersOperation() : filter.Currencies.MapToFiltersOperation(),
                TotalBetsCounts = filter.TotalBetsCounts == null ? new FiltersOperation() : filter.TotalBetsCounts.MapToFiltersOperation(),
                TotalBetsAmounts = filter.TotalBetsAmounts == null ? new FiltersOperation() : filter.TotalBetsAmounts.MapToFiltersOperation(),
                TotalWinsAmounts = filter.TotalWinsAmounts == null ? new FiltersOperation() : filter.TotalWinsAmounts.MapToFiltersOperation(),
                TotalUncalculatedBetsCounts = filter.TotalUncalculatedBetsCounts == null ? new FiltersOperation() : filter.TotalUncalculatedBetsCounts.MapToFiltersOperation(),
                TotalUncalculatedBetsAmounts = filter.TotalUncalculatedBetsAmounts == null ? new FiltersOperation() : filter.TotalUncalculatedBetsAmounts.MapToFiltersOperation(),
                GGRs = filter.GGRs == null ? new FiltersOperation() : filter.GGRs.MapToFiltersOperation()
            };
        }

        public static FilterReportByPaymentSystem MapToFilterReportByPaymentSystem(this ApiFilterReportByPaymentSystem filter)
        {
            return new FilterReportByPaymentSystem
            {
                PartnerId = filter.PartnerId,
                FromDate = filter.FromDate,
                ToDate = filter.ToDate,
                PartnerIds = filter.PartnerIds == null ? new FiltersOperation() : filter.PartnerIds.MapToFiltersOperation(),
                PaymentSystemIds = filter.PaymentSystemIds == null ? new FiltersOperation() : filter.PaymentSystemIds.MapToFiltersOperation(),
                PaymentSystemNames = filter.PaymentSystemNames == null ? new FiltersOperation() : filter.PaymentSystemNames.MapToFiltersOperation(),
                Statuses = filter.Statuses == null ? new FiltersOperation() : filter.Statuses.MapToFiltersOperation(),
                Counts = filter.Counts == null ? new FiltersOperation() : filter.Counts.MapToFiltersOperation(),
                TotalAmounts = filter.TotalAmounts == null ? new FiltersOperation() : filter.TotalAmounts.MapToFiltersOperation()
            };
        }
        public static FilterReportByPartner MapToFilterReportByPartner(this ApiFilterReportByPartner filter)
        {
            return new FilterReportByPartner
            {
                FromDate = filter.FromDate,
                ToDate = filter.ToDate,
                PartnerIds = filter.PartnerIds == null ? new FiltersOperation() : filter.PartnerIds.MapToFiltersOperation(),
                PartnerNames = filter.PartnerNames == null ? new FiltersOperation() : filter.PartnerNames.MapToFiltersOperation(),
                TotalBetAmounts = filter.TotalBetAmounts == null ? new FiltersOperation() : filter.TotalBetAmounts.MapToFiltersOperation(),
                TotalBetsCounts = filter.TotalBetsCounts == null ? new FiltersOperation() : filter.TotalBetsCounts.MapToFiltersOperation(),
                TotalWinAmounts = filter.TotalWinAmounts == null ? new FiltersOperation() : filter.TotalWinAmounts.MapToFiltersOperation(),
                TotalGGRs = filter.TotalGGRs == null ? new FiltersOperation() : filter.TotalGGRs.MapToFiltersOperation()
            };
        }

        public static FilterReportByAgentTranfer MapToFilterReportByAgentTranfer(this ApiFilterReportByAgentTranfer filter)
        {
            return new FilterReportByAgentTranfer
            {
                PartnerId = filter.PartnerId,
                FromDate = filter.FromDate,
                ToDate = filter.ToDate,
                PartnerIds = filter.PartnerIds == null ? new FiltersOperation() : filter.PartnerIds.MapToFiltersOperation(),
                UserIds = filter.UserIds == null ? new FiltersOperation() : filter.UserIds.MapToFiltersOperation(),
                UserNames = filter.UserNames == null ? new FiltersOperation() : filter.UserNames.MapToFiltersOperation(),
                NickNames = filter.NickNames == null ? new FiltersOperation() : filter.NickNames.MapToFiltersOperation(),
                TotoalProfits = filter.TotoalProfits == null ? new FiltersOperation() : filter.TotoalProfits.MapToFiltersOperation(),
                TotalDebits = filter.TotalDebits == null ? new FiltersOperation() : filter.TotalDebits.MapToFiltersOperation(),
                Balances = filter.Balances == null ? new FiltersOperation() : filter.Balances.MapToFiltersOperation()
            };
        }

        public static FilterReportByUserTransaction MapToFilterReportByUserTransaction(this ApiFilterReportByUserTransaction filter)
        {
            return new FilterReportByUserTransaction
            {
                PartnerId = filter.PartnerId,
                FromDate = filter.FromDate,
                ToDate = filter.ToDate,
                PartnerIds = filter.PartnerIds == null ? new FiltersOperation() : filter.PartnerIds.MapToFiltersOperation(),
                UserIds = filter.UserIds == null ? new FiltersOperation() : filter.UserIds.MapToFiltersOperation(),
                Usernames = filter.Usernames == null ? new FiltersOperation() : filter.Usernames.MapToFiltersOperation(),
                NickNames = filter.NickNames == null ? new FiltersOperation() : filter.NickNames.MapToFiltersOperation(),
                UserFirstNames = filter.UserFirstNames == null ? new FiltersOperation() : filter.UserFirstNames.MapToFiltersOperation(),
                UserLastNames = filter.UserLastNames == null ? new FiltersOperation() : filter.UserLastNames.MapToFiltersOperation(),
                FromUserIds = filter.FromUserIds == null ? new FiltersOperation() : filter.FromUserIds.MapToFiltersOperation(),
                FromUsernames = filter.FromUsernames == null ? new FiltersOperation() : filter.FromUsernames.MapToFiltersOperation(),
                ClientIds = filter.ClientIds == null ? new FiltersOperation() : filter.ClientIds.MapToFiltersOperation(),
                ClientUsernames = filter.ClientUsernames == null ? new FiltersOperation() : filter.ClientUsernames.MapToFiltersOperation(),
                OperationTypeIds = filter.OperationTypeIds == null ? new FiltersOperation() : filter.OperationTypeIds.MapToFiltersOperation(),
                Amounts = filter.Amounts == null ? new FiltersOperation() : filter.Amounts.MapToFiltersOperation(),
                CurrencyIds = filter.CurrencyIds == null ? new FiltersOperation() : filter.CurrencyIds.MapToFiltersOperation()
            };
        }

        public static FilterReportByProduct MapToFilterReportByProduct(this ApiFilterReportByProduct filter)
        {
            return new FilterReportByProduct
            {
                FromDate = filter.FromDate,
                ToDate = filter.ToDate,
                ClientIds = filter.ClientIds == null ? new FiltersOperation() : filter.ClientIds.MapToFiltersOperation(),
                ClientNames = filter.ClientNames == null ? new FiltersOperation() : filter.ClientNames.MapToFiltersOperation(),
                Currencies = filter.Currencies == null ? new FiltersOperation() : filter.Currencies.MapToFiltersOperation(),
                ProductIds = filter.ProductIds == null ? new FiltersOperation() : filter.ProductIds.MapToFiltersOperation(),
                ProductNames = filter.ProductNames == null ? new FiltersOperation() : filter.ProductNames.MapToFiltersOperation(),
                DeviceTypeIds = filter.DeviceTypeIds == null ? new FiltersOperation() : filter.DeviceTypeIds.MapToFiltersOperation(),
                ProviderNames = filter.ProviderNames == null ? new FiltersOperation() : filter.ProviderNames.MapToFiltersOperation(),
                TotalBetsAmounts = filter.TotalBetsAmounts == null ? new FiltersOperation() : filter.TotalBetsAmounts.MapToFiltersOperation(),
                TotalWinsAmounts = filter.TotalWinsAmounts == null ? new FiltersOperation() : filter.TotalWinsAmounts.MapToFiltersOperation(),
                TotalBetsCounts = filter.TotalBetsCounts == null ? new FiltersOperation() : filter.TotalBetsCounts.MapToFiltersOperation(),
                TotalUncalculatedBetsCounts = filter.TotalUncalculatedBetsCounts == null ? new FiltersOperation() : filter.TotalUncalculatedBetsCounts.MapToFiltersOperation(),
                TotalUncalculatedBetsAmounts = filter.TotalUncalculatedBetsAmounts == null ? new FiltersOperation() : filter.TotalUncalculatedBetsAmounts.MapToFiltersOperation(),
                GGRs = filter.GGRs == null ? new FiltersOperation() : filter.GGRs.MapToFiltersOperation()
            };
        }

        #endregion

        #region BusinessAudit Reports

        public static FilterReportByActionLog MapToFilterReportByActionLog(this ApiFilterReportByActionLog filter)
        {
            return new FilterReportByActionLog
            {
                FromDate = filter.FromDate,
                ToDate = filter.ToDate,
                Ids = filter.Ids == null ? new FiltersOperation() : filter.Ids.MapToFiltersOperation(),
                ActionNames = filter.ActionNames == null ? new FiltersOperation() : filter.ActionNames.MapToFiltersOperation(),
                ActionGroups = filter.ActionGroups == null ? new FiltersOperation() : filter.ActionGroups.MapToFiltersOperation(),
                UserIds = filter.UserIds == null ? new FiltersOperation() : filter.UserIds.MapToFiltersOperation(),
                Domains = filter.Domains == null ? new FiltersOperation() : filter.Domains.MapToFiltersOperation(),
                Ips = filter.Ips == null ? new FiltersOperation() : filter.Ips.MapToFiltersOperation(),
                Sources = filter.Sources == null ? new FiltersOperation() : filter.Sources.MapToFiltersOperation(),
                Countries = filter.Countries == null ? new FiltersOperation() : filter.Countries.MapToFiltersOperation(),
                SessionIds = filter.SessionIds == null ? new FiltersOperation() : filter.SessionIds.MapToFiltersOperation(),
                Languages = filter.Languages == null ? new FiltersOperation() : filter.Languages.MapToFiltersOperation(),
                ResultCodes = filter.ResultCodes == null ? new FiltersOperation() : filter.ResultCodes.MapToFiltersOperation(),
                Pages = filter.Pages == null ? new FiltersOperation() : filter.Pages.MapToFiltersOperation(),
                Descriptions = filter.Descriptions == null ? new FiltersOperation() : filter.Descriptions.MapToFiltersOperation(),
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

        #endregion

        #region Accounting Reports

        public static FilterPartnerPaymentsSummary MapToFilterPartnerPaymentsSummary(this ApiFilterPartnerPaymentsSummary filter)
        {
            return new FilterPartnerPaymentsSummary
            {
                PartnerId = filter.PartnerId,
                Type = filter.Type,
                FromDate = filter.FromDate,
                ToDate = filter.ToDate
            };
        }

        #endregion

        #endregion

        #region Clients

        public static FilterAccountsBalanceHistory MapToFilterAccountsBalanceHistory(this ApiFilterAccountsBalanceHistory filter, double timeZone)
        {
            return new FilterAccountsBalanceHistory
            {
                FromDate = filter.FromDate.AddHours(timeZone),
                ToDate = filter.ToDate.AddHours(timeZone),
                ClientId = filter.ClientId,
                UserId = filter.UserId,
                AccountId = filter.AccountId
            };
        }

        #endregion

        #region FilterBetshop

        public static FilterfnBetShop MaptToFilterfnBetShop(this ApiFilterBetShop filter)
        {
            return new FilterfnBetShop
            {
                SkipCount = filter.SkipCount,
                TakeCount = filter.TakeCount,
                CreatedBefore = filter.CreatedBefore,
                CreatedFrom = filter.CreatedFrom,
                PartnerId = filter.PartnerId,
                Ids = filter.Ids == null ? new FiltersOperation() : filter.Ids.MapToFiltersOperation(),
                GroupIds = filter.GroupIds == null ? new FiltersOperation() : filter.GroupIds.MapToFiltersOperation(),
                States = filter.States == null ? new FiltersOperation() : filter.States.MapToFiltersOperation(),
                CurrencyIds = filter.CurrencyIds == null ? new FiltersOperation() : filter.CurrencyIds.MapToFiltersOperation(),
                Names = filter.Names == null ? new FiltersOperation() : filter.Names.MapToFiltersOperation(),
                Addresses = filter.Addresses == null ? new FiltersOperation() : filter.Addresses.MapToFiltersOperation(),
                Balances = filter.Balances == null ? new FiltersOperation() : filter.Balances.MapToFiltersOperation(),
                CurrentLimits = filter.CurrentLimits == null ? new FiltersOperation() : filter.CurrentLimits.MapToFiltersOperation(),
                AgentIds = filter.AgentIds == null ? new FiltersOperation() : filter.AgentIds.MapToFiltersOperation(),
                MaxCopyCounts = filter.MaxCopyCounts == null ? new FiltersOperation() : filter.MaxCopyCounts.MapToFiltersOperation(),
                MaxWinAmounts = filter.MaxWinAmounts == null ? new FiltersOperation() : filter.MaxWinAmounts.MapToFiltersOperation(),
                MinBetAmounts = filter.MinBetAmounts == null ? new FiltersOperation() : filter.MinBetAmounts.MapToFiltersOperation(),
                MaxEventCountPerTickets = filter.MaxEventCountPerTickets == null ? new FiltersOperation() : filter.MaxEventCountPerTickets.MapToFiltersOperation(),
                CommissionTypes = filter.CommissionTypes == null ? new FiltersOperation() : filter.CommissionTypes.MapToFiltersOperation(),
                CommissionRates = filter.CommissionRates == null ? new FiltersOperation() : filter.CommissionRates.MapToFiltersOperation(),
                AnonymousBets = filter.AnonymousBets == null ? new FiltersOperation() : filter.AnonymousBets.MapToFiltersOperation(),
                AllowCashouts = filter.AllowCashouts == null ? new FiltersOperation() : filter.AllowCashouts.MapToFiltersOperation(),
                AllowLives = filter.AllowLives == null ? new FiltersOperation() : filter.AllowLives.MapToFiltersOperation(),
                UsePins = filter.UsePins == null ? new FiltersOperation() : filter.UsePins.MapToFiltersOperation(),
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
        public static FilterUser MaptToFilterUser(this ApiFilterUser filterUser)
        {
            return new FilterUser
            {
                PartnerId = filterUser.PartnerId,
                Ids = filterUser.Ids == null ? new FiltersOperation() : filterUser.Ids.MapToFiltersOperation(),
                FirstNames = filterUser.FirstNames == null ? new FiltersOperation() : filterUser.FirstNames.MapToFiltersOperation(),
                LastNames = filterUser.LastNames == null ? new FiltersOperation() : filterUser.LastNames.MapToFiltersOperation(),
                UserNames = filterUser.UserNames == null ? new FiltersOperation() : filterUser.UserNames.MapToFiltersOperation(),
                Emails = filterUser.Emails == null ? new FiltersOperation() : filterUser.Emails.MapToFiltersOperation(),
                Genders = filterUser.Genders == null ? new FiltersOperation() : filterUser.Genders.MapToFiltersOperation(),
                Currencies = filterUser.Currencies == null ? new FiltersOperation() : filterUser.Currencies.MapToFiltersOperation(),
                LanguageIds = filterUser.LanguageIds == null ? new FiltersOperation() : filterUser.LanguageIds.MapToFiltersOperation(),
                UserStates = filterUser.UserStates == null ? new FiltersOperation() : filterUser.UserStates.MapToFiltersOperation(),
                UserTypes = filterUser.UserTypes == null ? new FiltersOperation() : filterUser.UserTypes.MapToFiltersOperation(),
                SkipCount = filterUser.SkipCount,
                TakeCount = filterUser.TakeCount,
                OrderBy = filterUser.OrderBy,
                FieldNameToOrderBy = filterUser.FieldNameToOrderBy
            };
        }
        public static FilterfnUser MaptToFilterfnUser(this ApiFilterUser filterUser)
        {
            if (!string.IsNullOrEmpty(filterUser.FieldNameToOrderBy))
            {
                var orderBy = filterUser.FieldNameToOrderBy;
                switch (orderBy)
                {
                    case "UserType":
                        filterUser.FieldNameToOrderBy = "Type";
                        break;
                    default:
                        break;
                }
            }

            return new FilterfnUser
            {
                PartnerId = filterUser.PartnerId,
                Ids = filterUser.Ids == null ? new FiltersOperation() : filterUser.Ids.MapToFiltersOperation(),
                FirstNames = filterUser.FirstNames == null ? new FiltersOperation() : filterUser.FirstNames.MapToFiltersOperation(),
                LastNames = filterUser.LastNames == null ? new FiltersOperation() : filterUser.LastNames.MapToFiltersOperation(),
                UserNames = filterUser.UserNames == null ? new FiltersOperation() : filterUser.UserNames.MapToFiltersOperation(),
                Emails = filterUser.Emails == null ? new FiltersOperation() : filterUser.Emails.MapToFiltersOperation(),
                Genders = filterUser.Genders == null ? new FiltersOperation() : filterUser.Genders.MapToFiltersOperation(),
                Currencies = filterUser.Currencies == null ? new FiltersOperation() : filterUser.Currencies.MapToFiltersOperation(),
                LanguageIds = filterUser.LanguageIds == null ? new FiltersOperation() : filterUser.LanguageIds.MapToFiltersOperation(),
                UserStates = filterUser.UserStates == null ? new FiltersOperation() : filterUser.UserStates.MapToFiltersOperation(),
                UserTypes = filterUser.UserTypes == null ? new FiltersOperation() : filterUser.UserTypes.MapToFiltersOperation(),
                UserRoles = filterUser.UserRoles == null ? new FiltersOperation() : filterUser.UserRoles.MapToFiltersOperation(),
                SkipCount = filterUser.SkipCount,
                TakeCount = filterUser.TakeCount,
                OrderBy = filterUser.OrderBy,
                FieldNameToOrderBy = filterUser.FieldNameToOrderBy
            };
        }

        public static List<FilterUser> MapToFilterUsers(this IEnumerable<ApiFilterUser> filterUsers)
        {
            return filterUsers.Select(MaptToFilterUser).ToList();
        }

        public static FilterCorrection MapToFilterCorrection(this ApiFilterUserCorrection filter)
        {
            return new FilterCorrection
            {
                UserId = filter.UserId,
                TakeCount = filter.TakeCount,
                SkipCount = filter.SkipCount,
                FromDate = filter.FromDate,
                ToDate = filter.ToDate,
                OrderBy = filter.OrderBy,
                FieldNameToOrderBy = filter.FieldNameToOrderBy,
                Ids = filter.Ids == null ? new FiltersOperation() : filter.Ids.MapToFiltersOperation(),
                Creators = filter.FromUserIds == null ? new FiltersOperation() : filter.FromUserIds.MapToFiltersOperation(),
                Amounts = filter.Amounts == null ? new FiltersOperation() : filter.Amounts.MapToFiltersOperation(),
                States = filter.States == null ? new FiltersOperation() : filter.States.MapToFiltersOperation(),
                CurrencyIds = filter.CurrencyIds == null ? new FiltersOperation() : filter.CurrencyIds.MapToFiltersOperation(),
                UserIds = filter.UserIds == null ? new FiltersOperation() : filter.UserIds.MapToFiltersOperation(),
                OperationTypeNames = filter.OperationTypeNames == null ? new FiltersOperation() : filter.OperationTypeNames.MapToFiltersOperation(),
                FirstNames = filter.FirstNames == null ? new FiltersOperation() : filter.FirstNames.MapToFiltersOperation(),
                LastNames = filter.LastNames == null ? new FiltersOperation() : filter.LastNames.MapToFiltersOperation()
            };
        }
        public static FilterUserCorrection MapToFilterUserCorrection(this ApiFilterUserCorrection filter)
        {
            return new FilterUserCorrection
            {
                UserId = filter.UserId,
                TakeCount = filter.TakeCount,
                SkipCount = filter.SkipCount,
                FromDate = filter.FromDate,
                ToDate = filter.ToDate,
                OrderBy = filter.OrderBy,
                FieldNameToOrderBy = filter.FieldNameToOrderBy,
                Ids = filter.Ids == null ? new FiltersOperation() : filter.Ids.MapToFiltersOperation(),
                Creators = filter.FromUserIds == null ? new FiltersOperation() : filter.FromUserIds.MapToFiltersOperation(),
                Amounts = filter.Amounts == null ? new FiltersOperation() : filter.Amounts.MapToFiltersOperation(),
                States = filter.States == null ? new FiltersOperation() : filter.States.MapToFiltersOperation(),
                CurrencyIds = filter.CurrencyIds == null ? new FiltersOperation() : filter.CurrencyIds.MapToFiltersOperation(),
                UserIds = filter.UserIds == null ? new FiltersOperation() : filter.UserIds.MapToFiltersOperation(),
                OperationTypeNames = filter.OperationTypeNames == null ? new FiltersOperation() : filter.OperationTypeNames.MapToFiltersOperation(),
                CreatorFirstNames = filter.FirstNames == null ? new FiltersOperation() : filter.FirstNames.MapToFiltersOperation(),
                CreatorLastNames = filter.LastNames == null ? new FiltersOperation() : filter.LastNames.MapToFiltersOperation()
            };
        }
        public static ApiUserCorrections MapToApiCorrections(this PagedModel<fnCorrection> input, double timeZone)
        {
            return new ApiUserCorrections
            {
                Count = input.Count,
                Entities = input.Entities.Select(x => x.MapToApiCorrection(timeZone)).ToList()
            };
        }
        public static ApiUserCorrection MapToApiCorrection(this fnCorrection input, double timeZone)
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
                UserFirstName = input.FirstName,
                UserLastName = input.LastName,
                HasNote = input.HasNote ?? false
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

        public static FilterfnClient MapToFilterfnClient(this ApiFilterfnClient filterClient)
        {
            return new FilterfnClient
            {
                PartnerId = filterClient.PartnerId,
                AgentId = filterClient.AgentId,
                CreatedFrom = filterClient.CreatedFrom,
                CreatedBefore = filterClient.CreatedBefore,
                UnderMonitoringTypes = filterClient.UnderMonitoringTypes?.ToString(),
                Ids = filterClient.Ids == null ? new FiltersOperation() : filterClient.Ids.MapToFiltersOperation(),
                Emails = filterClient.Emails == null ? new FiltersOperation() : filterClient.Emails.MapToFiltersOperation(),
                UserNames = filterClient.UserNames == null ? new FiltersOperation() : filterClient.UserNames.MapToFiltersOperation(),
                Currencies = filterClient.Currencies == null ? new FiltersOperation() : filterClient.Currencies.MapToFiltersOperation(),
                Genders = filterClient.Genders == null ? new FiltersOperation() : filterClient.Genders.MapToFiltersOperation(),
                FirstNames = filterClient.FirstNames == null ? new FiltersOperation() : filterClient.FirstNames.MapToFiltersOperation(),
                LastNames = filterClient.LastNames == null ? new FiltersOperation() : filterClient.LastNames.MapToFiltersOperation(),
                NickNames = filterClient.NickNames == null ? new FiltersOperation() : filterClient.NickNames.MapToFiltersOperation(),
                SecondNames = filterClient.SecondNames == null ? new FiltersOperation() : filterClient.SecondNames.MapToFiltersOperation(),
                SecondSurnames = filterClient.SecondSurnames == null ? new FiltersOperation() : filterClient.SecondSurnames.MapToFiltersOperation(),
                DocumentNumbers = filterClient.DocumentNumbers == null ? new FiltersOperation() : filterClient.DocumentNumbers.MapToFiltersOperation(),
                DocumentIssuedBys = filterClient.DocumentIssuedBys == null ? new FiltersOperation() : filterClient.DocumentIssuedBys.MapToFiltersOperation(),
                LanguageIds = filterClient.LanguageIds == null ? new FiltersOperation() : filterClient.LanguageIds.MapToFiltersOperation(),
                Categories = filterClient.Categories == null ? new FiltersOperation() : filterClient.Categories.MapToFiltersOperation(),
                MobileNumbers = filterClient.MobileNumbers == null ? new FiltersOperation() : filterClient.MobileNumbers.MapToFiltersOperation(),
                ZipCodes = filterClient.ZipCodes == null ? new FiltersOperation() : filterClient.ZipCodes.MapToFiltersOperation(),
                Cities = filterClient.Cities == null ? new FiltersOperation() : filterClient.Cities.MapToFiltersOperation(),
                IsDocumentVerifieds = filterClient.IsDocumentVerifieds == null ? new FiltersOperation() : filterClient.IsDocumentVerifieds.MapToFiltersOperation(),
                PhoneNumbers = filterClient.PhoneNumbers == null ? new FiltersOperation() : filterClient.PhoneNumbers.MapToFiltersOperation(),
                RegionIds = filterClient.RegionIds == null ? new FiltersOperation() : filterClient.RegionIds.MapToFiltersOperation(),
                CountryIds = filterClient.CountryIds == null ? new FiltersOperation() : filterClient.CountryIds.MapToFiltersOperation(),
                BirthDates = filterClient.BirthDates == null ? new FiltersOperation() : filterClient.BirthDates.MapToFiltersOperation(),
                Ages = filterClient.Ages == null ? new FiltersOperation() : filterClient.Ages.MapToFiltersOperation(),
                RegionIsoCodes = filterClient.RegionIsoCodes == null ? new FiltersOperation() : filterClient.RegionIsoCodes.MapToFiltersOperation(),
                States = filterClient.States == null ? new FiltersOperation() : filterClient.States.MapToFiltersOperation(),
                CreationTimes = filterClient.CreationTimes == null ? new FiltersOperation() : filterClient.CreationTimes.MapToFiltersOperation(),
                Balances = filterClient.Balances == null ? new FiltersOperation() : filterClient.Balances.MapToFiltersOperation(),
                GGRs = filterClient.GGRs == null ? new FiltersOperation() : filterClient.GGRs.MapToFiltersOperation(),
                NETGamings = filterClient.NETGamings == null ? new FiltersOperation() : filterClient.NETGamings.MapToFiltersOperation(),
                AffiliatePlatformIds = filterClient.AffiliatePlatformIds == null ? new FiltersOperation() : filterClient.AffiliatePlatformIds.MapToFiltersOperation(),
                AffiliateIds = filterClient.AffiliateIds == null ? new FiltersOperation() : filterClient.AffiliateIds.MapToFiltersOperation(),
                AffiliateReferralIds = filterClient.AffiliateReferralIds == null ? new FiltersOperation() : filterClient.AffiliateReferralIds.MapToFiltersOperation(),
                UserIds = filterClient.UserIds == null ? new FiltersOperation() : filterClient.UserIds.MapToFiltersOperation(),
                LastDepositDates = filterClient.LastDepositDates == null ? new FiltersOperation() : filterClient.LastDepositDates.MapToFiltersOperation(),
                LastUpdateTimes = filterClient.LastUpdateTimes == null ? new FiltersOperation() : filterClient.LastUpdateTimes.MapToFiltersOperation(),
                LastSessionDates = filterClient.LastSessionDates == null ? new FiltersOperation() : filterClient.LastSessionDates.MapToFiltersOperation(),
                SkipCount = filterClient.SkipCount,
                TakeCount = filterClient.TakeCount,
                OrderBy = filterClient.OrderBy,
                FieldNameToOrderBy = filterClient.FieldNameToOrderBy,
                Message = filterClient.Message,
                Subject = filterClient.Subject

            };
        }
        public static FilterfnSegmentClient MapToFilterfnSegmentClient(this ApiFilterfnSegmentClient filterClient)
        {
            return new FilterfnSegmentClient
            {
                PartnerId = filterClient.PartnerId,
                SegmentId = filterClient.SegmentId,
                CreatedFrom = filterClient.CreatedFrom,
                CreatedBefore = filterClient.CreatedBefore,
                Ids = filterClient.Ids == null ? new FiltersOperation() : filterClient.Ids.MapToFiltersOperation(),
                Emails = filterClient.Emails == null ? new FiltersOperation() : filterClient.Emails.MapToFiltersOperation(),
                UserNames = filterClient.UserNames == null ? new FiltersOperation() : filterClient.UserNames.MapToFiltersOperation(),
                Currencies = filterClient.Currencies == null ? new FiltersOperation() : filterClient.Currencies.MapToFiltersOperation(),
                Genders = filterClient.Genders == null ? new FiltersOperation() : filterClient.Genders.MapToFiltersOperation(),
                FirstNames = filterClient.FirstNames == null ? new FiltersOperation() : filterClient.FirstNames.MapToFiltersOperation(),
                LastNames = filterClient.LastNames == null ? new FiltersOperation() : filterClient.LastNames.MapToFiltersOperation(),
                SecondNames = filterClient.SecondNames == null ? new FiltersOperation() : filterClient.SecondNames.MapToFiltersOperation(),
                SecondSurnames = filterClient.SecondSurnames == null ? new FiltersOperation() : filterClient.SecondSurnames.MapToFiltersOperation(),
                DocumentNumbers = filterClient.DocumentNumbers == null ? new FiltersOperation() : filterClient.DocumentNumbers.MapToFiltersOperation(),
                DocumentIssuedBys = filterClient.DocumentIssuedBys == null ? new FiltersOperation() : filterClient.DocumentIssuedBys.MapToFiltersOperation(),
                LanguageIds = filterClient.LanguageIds == null ? new FiltersOperation() : filterClient.LanguageIds.MapToFiltersOperation(),
                Categories = filterClient.Categories == null ? new FiltersOperation() : filterClient.Categories.MapToFiltersOperation(),
                MobileNumbers = filterClient.MobileNumbers == null ? new FiltersOperation() : filterClient.MobileNumbers.MapToFiltersOperation(),
                ZipCodes = filterClient.ZipCodes == null ? new FiltersOperation() : filterClient.ZipCodes.MapToFiltersOperation(),
                IsDocumentVerifieds = filterClient.IsDocumentVerifieds == null ? new FiltersOperation() : filterClient.IsDocumentVerifieds.MapToFiltersOperation(),
                PhoneNumbers = filterClient.PhoneNumbers == null ? new FiltersOperation() : filterClient.PhoneNumbers.MapToFiltersOperation(),
                RegionIds = filterClient.RegionIds == null ? new FiltersOperation() : filterClient.RegionIds.MapToFiltersOperation(),
                BirthDates = filterClient.BirthDates == null ? new FiltersOperation() : filterClient.BirthDates.MapToFiltersOperation(),
                States = filterClient.States == null ? new FiltersOperation() : filterClient.States.MapToFiltersOperation(),
                CreationTimes = filterClient.CreationTimes == null ? new FiltersOperation() : filterClient.CreationTimes.MapToFiltersOperation(),
                UserIds = filterClient.UserIds == null ? new FiltersOperation() : filterClient.UserIds.MapToFiltersOperation(),
                AffiliatePlatformIds = filterClient.AffiliatePlatformIds == null ? new FiltersOperation() : filterClient.AffiliatePlatformIds.MapToFiltersOperation(),
                AffiliateIds = filterClient.AffiliateIds == null ? new FiltersOperation() : filterClient.AffiliateIds.MapToFiltersOperation(),
                AffiliateReferralIds = filterClient.AffiliateReferralIds == null ? new FiltersOperation() : filterClient.AffiliateReferralIds.MapToFiltersOperation(),
                SegmentIds = filterClient.SegmentIds == null ? new FiltersOperation() : filterClient.SegmentIds.MapToFiltersOperation(),
                SkipCount = filterClient.SkipCount,
                TakeCount = filterClient.TakeCount,
                OrderBy = filterClient.OrderBy,
                FieldNameToOrderBy = filterClient.FieldNameToOrderBy
            };
        }

        #endregion

        #region FilterAffiliates

        public static FilterfnAffiliate MapToFilterfnAffiliate(this ApiFilterfnAffiliate filterAffiliate)
        {
            return new FilterfnAffiliate
            {
                PartnerId = filterAffiliate.PartnerId,
                CreatedFrom = filterAffiliate.CreatedFrom,
                CreatedBefore = filterAffiliate.CreatedBefore,
                Ids = filterAffiliate.Ids == null ? new FiltersOperation() : filterAffiliate.Ids.MapToFiltersOperation(),
                Emails = filterAffiliate.Emails == null ? new FiltersOperation() : filterAffiliate.Emails.MapToFiltersOperation(),
                UserNames = filterAffiliate.UserNames == null ? new FiltersOperation() : filterAffiliate.UserNames.MapToFiltersOperation(),
                FirstNames = filterAffiliate.FirstNames == null ? new FiltersOperation() : filterAffiliate.FirstNames.MapToFiltersOperation(),
                LastNames = filterAffiliate.LastNames == null ? new FiltersOperation() : filterAffiliate.LastNames.MapToFiltersOperation(),
                MobileNumbers = filterAffiliate.MobileNumbers == null ? new FiltersOperation() : filterAffiliate.MobileNumbers.MapToFiltersOperation(),
                RegionIds = filterAffiliate.RegionIds == null ? new FiltersOperation() : filterAffiliate.RegionIds.MapToFiltersOperation(),
                States = filterAffiliate.States == null ? new FiltersOperation() : filterAffiliate.States.MapToFiltersOperation(),
                CreationTimes = filterAffiliate.CreationTimes == null ? new FiltersOperation() : filterAffiliate.CreationTimes.MapToFiltersOperation()
            };
        }

        #endregion

        #region Filter Client Correction

        public static FilterCorrection MapToFilterCorrection(this ApiFilterClientCorrection filter)
        {
            return new FilterCorrection
            {
                ClientId = filter.ClientId,
                AccountId = filter.AccountId,
                TakeCount = filter.TakeCount,
                SkipCount = filter.SkipCount,
                FromDate = filter.FromDate,
                ToDate = filter.ToDate,
                OrderBy = filter.OrderBy,
                FieldNameToOrderBy = filter.FieldNameToOrderBy,
                Ids = filter.Ids == null ? new FiltersOperation() : filter.Ids.MapToFiltersOperation(),
                ClientIds = filter.ClientIds == null ? new FiltersOperation() : filter.ClientIds.MapToFiltersOperation(),
                ClientUserNames = filter.ClientUserNames == null ? new FiltersOperation() : filter.ClientUserNames.MapToFiltersOperation(),
                Amounts = filter.Amounts == null ? new FiltersOperation() : filter.Amounts.MapToFiltersOperation(),
                States = filter.States == null ? new FiltersOperation() : filter.States.MapToFiltersOperation(),
                CurrencyIds = filter.CurrencyIds == null ? new FiltersOperation() : filter.CurrencyIds.MapToFiltersOperation(),
                Creators = filter.UserIds == null ? new FiltersOperation() : filter.UserIds.MapToFiltersOperation(),
                OperationTypeNames = filter.OperationTypeNames == null ? new FiltersOperation() : filter.OperationTypeNames.MapToFiltersOperation(),
                OperationTypeIds = filter.OperationTypeIds == null ? new FiltersOperation() : filter.OperationTypeIds.MapToFiltersOperation(),
                FirstNames = filter.FirstNames == null ? new FiltersOperation() : filter.FirstNames.MapToFiltersOperation(),
                LastNames = filter.LastNames == null ? new FiltersOperation() : filter.LastNames.MapToFiltersOperation(),
                ProductNames = filter.ProductNames == null ? new FiltersOperation() : filter.ProductNames.MapToFiltersOperation()
            };
        }


        #endregion

        #region FilterClientMessage

        public static List<FilterfnClientLog> MapToFilterClientLogs(this IEnumerable<ApiFilterClientLog> filterClientLogs)
        {
            return filterClientLogs.Select(MapToFilterClientLog).ToList();
        }

        public static FilterfnClientLog MapToFilterClientLog(this ApiFilterClientLog request)
        {
            return new FilterfnClientLog
            {
                SkipCount = request.SkipCount,
                TakeCount = request.TakeCount,
                Ids = request.Ids == null ? new FiltersOperation() : request.Ids.MapToFiltersOperation(),
                ClientIds = request.ClientIds == null ? new FiltersOperation() : request.ClientIds.MapToFiltersOperation(),
                Actions = request.Actions == null ? new FiltersOperation() : request.Actions.MapToFiltersOperation(),
                UserIds = request.UserIds == null ? new FiltersOperation() : request.UserIds.MapToFiltersOperation(),
                Ips = request.Ips == null ? new FiltersOperation() : request.Ips.MapToFiltersOperation(),
                Pages = request.Pages == null ? new FiltersOperation() : request.Pages.MapToFiltersOperation(),
                SessionIds = request.SessionIds == null ? new FiltersOperation() : request.SessionIds.MapToFiltersOperation(),
                CreatedFrom = request.CreatedFrom,
                CreatedBefore = request.CreatedBefore,
                OrderBy = request.OrderBy,
                FieldNameToOrderBy = request.FieldNameToOrderBy
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

        public static FilterPartner MapToFilterPartner(this ApiFilterPartner apiFilterPartner)
        {
            return new FilterPartner
            {
                Id = apiFilterPartner.Id,
                Name = apiFilterPartner.Name,
                CurrencyId = apiFilterPartner.CurrencyId,
                State = apiFilterPartner.State,
                AdminSiteUrl = apiFilterPartner.AdminSiteUrl,
                CreatedFrom = apiFilterPartner.CreatedFrom,
                CreatedBefore = apiFilterPartner.CreatedBefore,
                SkipCount = apiFilterPartner.SkipCount,
                TakeCount = apiFilterPartner.TakeCount,
                OrderBy = apiFilterPartner.OrderBy,
                FieldNameToOrderBy = apiFilterPartner.FieldNameToOrderBy
            };
        }

        public static List<FilterPartner> MapToFilterPartners(this IEnumerable<ApiFilterPartner> apiFilterPartners)
        {
            return apiFilterPartners.Select(MapToFilterPartner).ToList();
        }

        #endregion

        #region FiltersOperation

        public static FiltersOperation MapToFiltersOperation(this ApiFiltersOperation apiFiltersOperation)
        {
            if (apiFiltersOperation.ApiOperationTypeList.Count == 0)
            {
                apiFiltersOperation.ApiOperationTypeList.Add(new ApiFiltersOperationType { OperationTypeId = (int)FilterOperations.IsNull });
            }
            return new FiltersOperation
            {
                IsAnd = apiFiltersOperation.IsAnd,
                OperationTypeList = apiFiltersOperation.ApiOperationTypeList.MapToFiltersOperationTypes()
            };
        }

        #endregion

        #region OperationTypes

        public static List<FiltersOperationType> MapToFiltersOperationTypes(this List<ApiFiltersOperationType> apiFiltersOperationTypes)
        {
            return apiFiltersOperationTypes.Select(MapToFiltersOperationType).ToList();
        }

        public static FiltersOperationType MapToFiltersOperationType(this ApiFiltersOperationType apiFiltersOperationType)
        {
            return new FiltersOperationType
            {
                OperationTypeId = apiFiltersOperationType.OperationTypeId,
                StringValue = apiFiltersOperationType.ArrayValue != null && apiFiltersOperationType.ArrayValue.Any() ?
                              string.Join(",", apiFiltersOperationType.ArrayValue) : apiFiltersOperationType.StringValue,
                IntValue = apiFiltersOperationType.IntValue,
                DecimalValue = apiFiltersOperationType.DecimalValue,
                DateTimeValue = apiFiltersOperationType.DateTimeValue
            };
        }

        #endregion

        #region FilterPaymentRequest

        public static List<FilterfnPaymentRequest> MapToFilterPaymentRequests(this IEnumerable<ApiFilterfnPaymentRequest> requests)
        {
            return requests.Select(MapToFilterfnPaymentRequest).ToList();
        }

        public static FilterfnPaymentRequest MapToFilterfnPaymentRequest(this ApiFilterfnPaymentRequest request)
        {
            if (!string.IsNullOrEmpty(request.FieldNameToOrderBy))
            {
                var orderBy = request.FieldNameToOrderBy;
                switch (orderBy)
                {
                    case "ExternalId":
                        request.FieldNameToOrderBy = "ExternalTransactionId";
                        break;
                    case "State":
                        request.FieldNameToOrderBy = "Status";
                        break;
                    default:
                        break;
                }
            }
            return new FilterfnPaymentRequest
            {
                PartnerId = request.PartnerId,
                FromDate = request.FromDate == null ? 0 : (long)request.FromDate.Value.Year * 100000000 + (long)request.FromDate.Value.Month * 1000000 +
                    (long)request.FromDate.Value.Day * 10000 + (long)request.FromDate.Value.Hour * 100 + request.FromDate.Value.Minute,
                ToDate = request.ToDate == null ? 0 : (long)request.ToDate.Value.Year * 100000000 + (long)request.ToDate.Value.Month * 1000000 +
                    (long)request.ToDate.Value.Day * 10000 + (long)request.ToDate.Value.Hour * 100 + request.ToDate.Value.Minute,
                Type = request.Type,
                HasNote = request.HasNote,
                AgentId = request.AgentId,
                AccountIds = request.AccountId == null ? null : new List<long> { request.AccountId.Value },
                Ids = request.Ids == null ? new FiltersOperation() : request.Ids.MapToFiltersOperation(),
                UserNames = request.UserNames == null ? new FiltersOperation() : request.UserNames.MapToFiltersOperation(),
                Names = request.Names == null ? new FiltersOperation() : request.Names.MapToFiltersOperation(),
                CreatorNames = request.CreatorNames == null ? new FiltersOperation() : request.CreatorNames.MapToFiltersOperation(),
                ClientIds = request.ClientIds == null ? new FiltersOperation() : request.ClientIds.MapToFiltersOperation(),
                ClientEmails = request.Emails == null ? new FiltersOperation() : request.Emails.MapToFiltersOperation(),
                UserIds = request.UserIds == null ? new FiltersOperation() : request.UserIds.MapToFiltersOperation(),
                PartnerPaymentSettingIds = request.PartnerPaymentSettingIds == null ? new FiltersOperation() : request.PartnerPaymentSettingIds.MapToFiltersOperation(),
                PaymentSystemIds = request.PaymentSystemIds == null ? new FiltersOperation() : request.PaymentSystemIds.MapToFiltersOperation(),
                Currencies = request.CurrencyIds == null ? new FiltersOperation() : request.CurrencyIds.MapToFiltersOperation(),
                States = request.States == null ? new FiltersOperation() : request.States.MapToFiltersOperation(),
                Types = request.Types == null ? new FiltersOperation() : request.Types.MapToFiltersOperation(),
                BetShopIds = request.BetShopIds == null ? new FiltersOperation() : request.BetShopIds.MapToFiltersOperation(),
                BetShopNames = request.BetShopNames == null ? new FiltersOperation() : request.BetShopNames.MapToFiltersOperation(),
                Amounts = request.Amounts == null ? new FiltersOperation() : request.Amounts.MapToFiltersOperation(),
                CreationTimes = request.CreationTimes == null ? new FiltersOperation() : request.CreationTimes.MapToFiltersOperation(),
                AffiliatePlatformIds = request.AffiliatePlatformIds == null ? new FiltersOperation() : request.AffiliatePlatformIds.MapToFiltersOperation(),
                AffiliateIds = request.AffiliateIds == null ? new FiltersOperation() : request.AffiliateIds.MapToFiltersOperation(),
                ActivatedBonusTypes = request.ActivatedBonusTypes == null ? new FiltersOperation() : request.ActivatedBonusTypes.MapToFiltersOperation(),
                CommissionAmounts = request.CommissionAmounts == null ? new FiltersOperation() : request.CommissionAmounts.MapToFiltersOperation(),
                CardNumbers = request.CardNumbers == null ? new FiltersOperation() : request.CardNumbers.MapToFiltersOperation(),
                CountryCodes = request.CountryCodes == null ? new FiltersOperation() : request.CountryCodes.MapToFiltersOperation(),
                SegmentNames = request.SegmentNames == null ? new FiltersOperation() : request.SegmentNames.MapToFiltersOperation(),
                SegmentIds = request.SegmentIds == null ? new FiltersOperation() : request.SegmentIds.MapToFiltersOperation(),
                LastUpdateTimes = request.LastUpdateTimes == null ? new FiltersOperation() : request.LastUpdateTimes.MapToFiltersOperation(),
                ExternalTransactionIds = request.ExternalIds == null ? new FiltersOperation() : request.ExternalIds.MapToFiltersOperation(),
                TakeCount = request.TakeCount,
                SkipCount = request.SkipCount,
                OrderBy = request.OrderBy,
                FieldNameToOrderBy = request.FieldNameToOrderBy
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

        public static List<FilterRole> MapToFilterFilterRoles(this IEnumerable<ApiFilterRole> filterRoles)
        {
            return filterRoles.Select(MapToFilterFilterRole).ToList();
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
                Ids = filter.Ids == null ? new FiltersOperation() : filter.Ids.MapToFiltersOperation(),
                Names = filter.Names == null ? new FiltersOperation() : filter.Names.MapToFiltersOperation(),
                Descriptions = filter.Descriptions == null ? new FiltersOperation() : filter.Descriptions.MapToFiltersOperation(),
                ExternalIds = filter.ExternalIds == null ? new FiltersOperation() : filter.ExternalIds.MapToFiltersOperation(),
                States = filter.States == null ? new FiltersOperation() : filter.States.MapToFiltersOperation(),
                GameProviderIds = filter.GameProviderIds == null ? new FiltersOperation() : filter.GameProviderIds.MapToFiltersOperation(),
                SubProviderIds = filter.SubProviderIds == null ? new FiltersOperation() : filter.SubProviderIds.MapToFiltersOperation(),
                IsForDesktops = filter.IsForDesktops == null ? new FiltersOperation() : filter.IsForDesktops.MapToFiltersOperation(),
                IsForMobiles = filter.IsForMobiles == null ? new FiltersOperation() : filter.IsForMobiles.MapToFiltersOperation(),
                FreeSpinSupports = filter.FreeSpinSupports == null ? new FiltersOperation() : filter.FreeSpinSupports.MapToFiltersOperation(),
                Jackpots = filter.Jackpots == null ? new FiltersOperation() : filter.Jackpots.MapToFiltersOperation(),
                RTPs = filter.RTPs == null ? new FiltersOperation() : filter.RTPs.MapToFiltersOperation(),
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
                Name = filter.Name
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
                Ids = filter.Ids == null ? new List<FiltersOperationType>() : filter.Ids.MapToFiltersOperationTypes(),
                UserIds = filter.UserIds == null ? new List<FiltersOperationType>() : filter.UserIds.MapToFiltersOperationTypes(),
                Currencies = filter.Currencies == null ? new List<FiltersOperationType>() : filter.Currencies.MapToFiltersOperationTypes(),
                BetShopIds = filter.BetShopIds == null ? new List<FiltersOperationType>() : filter.BetShopIds.MapToFiltersOperationTypes(),
                BetShopNames = filter.BetShopNames == null ? new List<FiltersOperationType>() : filter.BetShopNames.MapToFiltersOperationTypes(),
                BetShopAvailiableBalances = filter.BetShopAvailiableBalances == null ? new List<FiltersOperationType>() : filter.BetShopAvailiableBalances.MapToFiltersOperationTypes(),
                Amounts = filter.Amounts == null ? new List<FiltersOperationType>() : filter.Amounts.MapToFiltersOperationTypes(),
                CreationTimes = filter.CreationTimes == null ? new List<FiltersOperationType>() : filter.CreationTimes.MapToFiltersOperationTypes()
            };
        }

        public static List<FilterfnBetShopReconing> MapToFilterBetShopReconings(this IEnumerable<ApiFilterBetShopReconing> filters)
        {
            return filters.Select(MapToFilterBetShopReconing).ToList();
        }
        #endregion

        #region FilterCashDeskTransaction

        public static FilterCashDeskTransaction MapToFilterCashDeskTransaction(
            this ApiFilterCashDeskTransaction transaction)
        {
            return new FilterCashDeskTransaction
            {
                FromDate = transaction.CreatedFrom,
                ToDate = transaction.CreatedBefore,
                Ids = transaction.Ids == null ? new FiltersOperation() : transaction.Ids.MapToFiltersOperation(),
                BetShopNames = transaction.BetShopNames == null ? new FiltersOperation() : transaction.BetShopNames.MapToFiltersOperation(),
                CashDeskIds = transaction.CashDeskIds == null ? new FiltersOperation() : transaction.CashDeskIds.MapToFiltersOperation(),
                CashierIds = transaction.CashierIds == null ? new FiltersOperation() : transaction.CashierIds.MapToFiltersOperation(),
                BetShopIds = transaction.BetShopIds == null ? new FiltersOperation() : transaction.BetShopIds.MapToFiltersOperation(),
                OperationTypeNames = transaction.OperationTypeNames == null ? new FiltersOperation() : transaction.OperationTypeNames.MapToFiltersOperation(),
                Amounts = transaction.Amounts == null ? new FiltersOperation() : transaction.Amounts.MapToFiltersOperation(),
                Currencies = transaction.Currencies == null ? new FiltersOperation() : transaction.Currencies.MapToFiltersOperation(),
                CreationTimes = transaction.CreationTimes == null ? new FiltersOperation() : transaction.CreationTimes.MapToFiltersOperation(),
                TakeCount = transaction.TakeCount,
                SkipCount = transaction.SkipCount
            };
        }
        #endregion

        #region FilterfnPartnerPaymentSetting

        public static FilterfnPartnerPaymentSetting MapToFilterfnPartnerPaymentSetting(
            this ApiFilterfnPartnerPaymentSetting filter)
        {
            return new FilterfnPartnerPaymentSetting
            {
                Id = filter.Id,
                Status = filter.Status,
                Type = filter.Type,
                PaymentSystemId = filter.PaymentSystemId,
                PartnerId = filter.PartnerId,
                CurrencyId = filter.CurrencyId,
                CreatedFrom = filter.CreatedFrom,
                CreatedBefore = filter.CreatedBefore
            };
        }
        #endregion

        #region FilterDashboard

        public static FilterDashboard MapToFilterDashboard(this ApiFilterDashboard filter)
        {
            return new FilterDashboard
            {
                PartnerId = filter.PartnerId,
                FromDate = filter.FromDate ?? DateTime.UtcNow,
                ToDate = filter.ToDate ?? DateTime.UtcNow
            };
        }

        public static FilterRealTime MapToFilterRealTime(this ApiFilterRealTime filter)
        {
            return new FilterRealTime
            {
                PartnerId = filter.PartnerId,
                ClientIds = filter.ClientIds == null ? new FiltersOperation() : filter.ClientIds.MapToFiltersOperation(),
                LanguageIds = filter.LanguageIds == null ? new FiltersOperation() : filter.LanguageIds.MapToFiltersOperation(),
                Names = filter.Names == null ? new FiltersOperation() : filter.Names.MapToFiltersOperation(),
                UserNames = filter.UserNames == null ? new FiltersOperation() : filter.UserNames.MapToFiltersOperation(),
                Categories = filter.Categories == null ? new FiltersOperation() : filter.Categories.MapToFiltersOperation(),
                RegionIds = filter.RegionIds == null ? new FiltersOperation() : filter.RegionIds.MapToFiltersOperation(),
                Currencies = filter.Currencies == null ? new FiltersOperation() : filter.Currencies.MapToFiltersOperation(),
                LoginIps = filter.LoginIps == null ? new FiltersOperation() : filter.LoginIps.MapToFiltersOperation(),
                Balances = filter.Balances == null ? new FiltersOperation() : filter.Balances.MapToFiltersOperation(),
                TotalDepositsCounts = filter.TotalDepositsCounts == null ? new FiltersOperation() : filter.TotalDepositsCounts.MapToFiltersOperation(),
                TotalDepositsAmounts = filter.TotalDepositsAmounts == null ? new FiltersOperation() : filter.TotalDepositsAmounts.MapToFiltersOperation(),
                TotalWithdrawalsCounts = filter.TotalWithdrawalsCounts == null ? new FiltersOperation() : filter.TotalWithdrawalsCounts.MapToFiltersOperation(),
                TotalWithdrawalsAmounts = filter.TotalWithdrawalsAmounts == null ? new FiltersOperation() : filter.TotalWithdrawalsAmounts.MapToFiltersOperation(),
                TotalBetsCounts = filter.TotalBetsCounts == null ? new FiltersOperation() : filter.TotalBetsCounts.MapToFiltersOperation(),
                GGRs = filter.GGRs == null ? new FiltersOperation() : filter.GGRs.MapToFiltersOperation(),
                TakeCount = filter.TakeCount,
                SkipCount = filter.SkipCount,
                OrderBy = filter.OrderBy,
                FieldNameToOrderBy = filter.FieldNameToOrderBy
            };
        }

        #endregion

        #region fnClientDashboard

        public static FilterfnClientDashboard MapToFilterfnClientDashboard(this ApiFilterfnClientDashboard filter)
        {
            return new FilterfnClientDashboard
            {
                PartnerId = filter.PartnerId,
                FromDate = filter.FromDate,
                ToDate = filter.ToDate,
                ClientIds = filter.ClientIds == null ? new FiltersOperation() : filter.ClientIds.MapToFiltersOperation(),
                UserNames = filter.UserNames == null ? new FiltersOperation() : filter.UserNames.MapToFiltersOperation(),
                PartnerIds = filter.PartnerIds == null ? new FiltersOperation() : filter.PartnerIds.MapToFiltersOperation(),
                CurrencyIds = filter.CurrencyIds == null ? new FiltersOperation() : filter.CurrencyIds.MapToFiltersOperation(),
                FirstNames = filter.FirstNames == null ? new FiltersOperation() : filter.FirstNames.MapToFiltersOperation(),
                LastNames = filter.LastNames == null ? new FiltersOperation() : filter.LastNames.MapToFiltersOperation(),
                Emails = filter.Emails == null ? new FiltersOperation() : filter.Emails.MapToFiltersOperation(),
                AffiliatePlatformIds = filter.AffiliatePlatformIds == null ? new FiltersOperation() : filter.AffiliatePlatformIds.MapToFiltersOperation(),
                AffiliateIds = filter.AffiliateIds == null ? new FiltersOperation() : filter.AffiliateIds.MapToFiltersOperation(),
                AffiliateReferralIds = filter.AffiliateReferralIds == null ? new FiltersOperation() : filter.AffiliateReferralIds.MapToFiltersOperation(),
                TotalWithdrawalAmounts = filter.TotalWithdrawalAmounts == null ? new FiltersOperation() : filter.TotalWithdrawalAmounts.MapToFiltersOperation(),
                WithdrawalsCounts = filter.WithdrawalsCounts == null ? new FiltersOperation() : filter.WithdrawalsCounts.MapToFiltersOperation(),
                TotalDepositAmounts = filter.TotalDepositAmounts == null ? new FiltersOperation() : filter.TotalDepositAmounts.MapToFiltersOperation(),
                DepositsCounts = filter.DepositsCounts == null ? new FiltersOperation() : filter.DepositsCounts.MapToFiltersOperation(),
                TotalBetAmounts = filter.TotalBetAmounts == null ? new FiltersOperation() : filter.TotalBetAmounts.MapToFiltersOperation(),
                BetsCounts = filter.BetsCounts == null ? new FiltersOperation() : filter.BetsCounts.MapToFiltersOperation(),
                TotalWinAmounts = filter.TotalWinAmounts == null ? new FiltersOperation() : filter.TotalWinAmounts.MapToFiltersOperation(),
                WinsCounts = filter.WinsCounts == null ? new FiltersOperation() : filter.WinsCounts.MapToFiltersOperation(),
                GGRs = filter.GGRs == null ? new FiltersOperation() : filter.GGRs.MapToFiltersOperation(),
                TotalDebitCorrections = filter.TotalDebitCorrections == null ? new FiltersOperation() : filter.TotalDebitCorrections.MapToFiltersOperation(),
                DebitCorrectionsCounts = filter.DebitCorrectionsCounts == null ? new FiltersOperation() : filter.DebitCorrectionsCounts.MapToFiltersOperation(),
                TotalCreditCorrections = filter.TotalCreditCorrections == null ? new FiltersOperation() : filter.TotalCreditCorrections.MapToFiltersOperation(),
                CreditCorrectionsCounts = filter.CreditCorrectionsCounts == null ? new FiltersOperation() : filter.CreditCorrectionsCounts.MapToFiltersOperation(),
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

        public static FilterAdminShift MapToFilterfnAdminShiftReport(this ApiFilterShiftReport apiFilterShift)
        {
            return new FilterAdminShift
            {
                FromDate = apiFilterShift.FromDate,
                ToDate = apiFilterShift.ToDate,
                Ids = apiFilterShift.Ids == null ? new FiltersOperation() : apiFilterShift.Ids.MapToFiltersOperation(),
                BetShopIds = apiFilterShift.BetShopIds == null ? new FiltersOperation() : apiFilterShift.BetShopIds.MapToFiltersOperation(),
                BetShopGroupIds = apiFilterShift.BetShopGroupIds == null ? new FiltersOperation() : apiFilterShift.BetShopGroupIds.MapToFiltersOperation(),
                BetShopNames = apiFilterShift.BetShopNames == null ? new FiltersOperation() : apiFilterShift.BetShopNames.MapToFiltersOperation(),
                BetShopGroupNames = apiFilterShift.BetShopGroupNames == null ? new FiltersOperation() : apiFilterShift.BetShopGroupNames.MapToFiltersOperation(),
                CashierIds = apiFilterShift.CashierIds == null ? new FiltersOperation() : apiFilterShift.CashierIds.MapToFiltersOperation(),
                CashdeskIds = apiFilterShift.CashdeskIds == null ? new FiltersOperation() : apiFilterShift.CashdeskIds.MapToFiltersOperation(),
                FirstNames = apiFilterShift.FirstNames == null ? new FiltersOperation() : apiFilterShift.FirstNames.MapToFiltersOperation(),
                EndAmounts = apiFilterShift.EndAmounts == null ? new FiltersOperation() : apiFilterShift.EndAmounts.MapToFiltersOperation(),
                BetAmounts = apiFilterShift.BetAmounts == null ? new FiltersOperation() : apiFilterShift.BetAmounts.MapToFiltersOperation(),
                PayedWinAmounts = apiFilterShift.PayedWinAmounts == null ? new FiltersOperation() : apiFilterShift.PayedWinAmounts.MapToFiltersOperation(),
                DepositAmounts = apiFilterShift.DepositAmounts == null ? new FiltersOperation() : apiFilterShift.DepositAmounts.MapToFiltersOperation(),
                WithdrawAmounts = apiFilterShift.WithdrawAmounts == null ? new FiltersOperation() : apiFilterShift.WithdrawAmounts.MapToFiltersOperation(),
                DebitCorrectionAmounts = apiFilterShift.DebitCorrectionAmounts == null ? new FiltersOperation() : apiFilterShift.DebitCorrectionAmounts.MapToFiltersOperation(),
                CreditCorrectionAmounts = apiFilterShift.CreditCorrectionAmounts == null ? new FiltersOperation() : apiFilterShift.CreditCorrectionAmounts.MapToFiltersOperation(),
                StartDates = apiFilterShift.StartDates == null ? new FiltersOperation() : apiFilterShift.StartDates.MapToFiltersOperation(),
                EndDates = apiFilterShift.EndDates == null ? new FiltersOperation() : apiFilterShift.EndDates.MapToFiltersOperation(),
                ShiftNumbers = apiFilterShift.ShiftNumbers == null ? new FiltersOperation() : apiFilterShift.ShiftNumbers.MapToFiltersOperation(),
                PartnerIds = apiFilterShift.PartnerIds == null ? new FiltersOperation() : apiFilterShift.PartnerIds.MapToFiltersOperation(),
                BonusAmounts = apiFilterShift.BonusAmounts == null ? new FiltersOperation() : apiFilterShift.BonusAmounts.MapToFiltersOperation(),

                SkipCount = apiFilterShift.SkipCount,
                TakeCount = apiFilterShift.TakeCount,

                OrderBy = apiFilterShift.OrderBy,
                FieldNameToOrderBy = apiFilterShift.FieldNameToOrderBy
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

        public static CRMSetting ToCRMSetting(this ApiCRMSetting apiCRMSetting)
        {
            return new CRMSetting
            {
                Id = apiCRMSetting.Id,
                PartnerId = apiCRMSetting.PartnerId,
                NickeName = apiCRMSetting.NickeName,
                State = apiCRMSetting.State,
                Type = apiCRMSetting.Type,
                Condition = apiCRMSetting.Condition,
                StartTime = apiCRMSetting.StartTime,
                FinishTime = apiCRMSetting.FinishTime,
                Sequence = apiCRMSetting.Sequence
            };
        }

        public static ApiCRMSetting ToApiCRMSetting(this CRMSetting setting)
        {
            return new ApiCRMSetting
            {
                Id = setting.Id,
                PartnerId = setting.PartnerId,
                NickeName = setting.NickeName,
                State = setting.State,
                Type = setting.Type,
                Condition = setting.Condition,
                StartTime = setting.StartTime,
                FinishTime = setting.FinishTime,
                Sequence = setting.Sequence
            };
        }

        public static DAL.Banner MapToBanner(this ApiBanner input)
        {
            return new DAL.Banner
            {
                Id = input.Id,
                PartnerId = input.PartnerId,
                NickName = input.NickName,
                Type = input.Type,
                Visibility = input.Visibility == null ? null : JsonConvert.SerializeObject(input.Visibility),
                Head = input.Head,
                Body = input.Body,
                Link = input.Link,
                ShowDescription = input.ShowDescription,
                ButtonType = Convert.ToInt32(string.Format("1{0}{1}", Convert.ToInt32(input.ShowRegistration), Convert.ToInt32(input.ShowLogin))),
                Order = input.Order,
                IsEnabled = input.IsEnabled,
                StartDate = input.StartDate,
                EndDate = input.EndDate,
                Image = string.IsNullOrEmpty(input.Image) ? string.Empty : input.Image,
                ImageSize = input.ImageSize,
                BannerSegmentSettings = (input.Segments == null || input.Segments.Ids == null) ? new List<BannerSegmentSetting>() :
                                          input.Segments?.Ids?.Select(x => new BannerSegmentSetting
                                          {
                                              BannerId = input.Id,
                                              SegmentId = x,
                                              Type = input.Segments.Type ?? (int)BonusSettingConditionTypes.InSet
                                          }).ToList(),
                BannerLanguageSettings = (input.Languages == null || input.Languages.Names == null) ? new List<BannerLanguageSetting>() :
                                          input.Languages?.Names?.Select(x => new BannerLanguageSetting
                                          {
                                              BannerId = input.Id,
                                              LanguageId = x,
                                              Type = input.Languages.Type ?? (int)BonusSettingConditionTypes.InSet
                                          }).ToList()
            };
        }

        public static ApiBanner ToApiBanner(this DAL.Banner banner, double timeZone)
        {
            var buttonTypes = Enumerable.Repeat(false, 3).ToArray();
            if (banner.ButtonType.HasValue)
                buttonTypes = banner.ButtonType.ToString().Select(x => x.Equals('1')).ToArray();
            var imageNames = banner.Image.Split(',');
            return new ApiBanner
            {
                Id = banner.Id,
                PartnerId = banner.PartnerId,
                NickName = banner.NickName,
                Type = banner.Type,
                Head = banner.Head,
                Body = banner.Body,
                Link = banner.Link,
                Order = banner.Order,
                Image = imageNames[0],
                ImageSizes = imageNames.Skip(1).ToList(),
                IsEnabled = banner.IsEnabled,
                ShowDescription = banner.ShowDescription,
                ShowRegistration = banner.ButtonType.HasValue && Convert.ToBoolean(banner.ButtonType.ToString().Select(y => y.Equals('1')).ToArray().ElementAtOrDefault(1)),
                ShowLogin = banner.ButtonType.HasValue && Convert.ToBoolean(banner.ButtonType.ToString().Select(y => y.Equals('1')).ToArray().ElementAtOrDefault(2)),
                StartDate = banner.StartDate.GetGMTDateFromUTC(timeZone),
                EndDate = banner.EndDate.GetGMTDateFromUTC(timeZone),
                Visibility = string.IsNullOrEmpty(banner.Visibility) ? new List<int>() : JsonConvert.DeserializeObject<List<int>>(banner.Visibility),
                Segments = banner.BannerSegmentSettings != null && banner.BannerSegmentSettings.Any() ? new ApiSetting
                {
                    Type = banner.BannerSegmentSettings.First().Type,
                    Ids = banner.BannerSegmentSettings.Select(x => x.SegmentId).ToList()
                } : new ApiSetting { Type = (int)BonusSettingConditionTypes.InSet, Ids = new List<int>() },
                Languages = banner.BannerLanguageSettings != null && banner.BannerLanguageSettings.Any() ? new ApiSetting
                {
                    Type = banner.BannerLanguageSettings.First().Type,
                    Names = banner.BannerLanguageSettings.Select(x => x.LanguageId).ToList()
                } : new ApiSetting { Type = (int)BonusSettingConditionTypes.InSet, Names = new List<string>() }
            };
        }

        public static Promotion MapToPromotion(this ApiPromotion input)
        {
            return new Promotion
            {
                Id = input.Id,
                PartnerId = input.PartnerId,
                NickName = input.NickName,
                Type = input.Type,
                Title = input.Title,
                Content = input.Title,
                Description = input.Description,
                ImageName = input.ImageName,
                State = input.State,
                StartDate = input.StartDate,
                FinishDate = input.FinishDate,
                PromotionSegmentSettings = (input.Segments == null || input.Segments.Ids == null) ? new List<PromotionSegmentSetting>() :
                                          input.Segments?.Ids?.Select(x => new PromotionSegmentSetting { PromotionId = input.Id, SegmentId = x,
                                              Type = input.Segments.Type ?? (int)BonusSettingConditionTypes.InSet }).ToList(),
                PromotionLanguageSettings = (input.Languages == null || input.Languages.Names == null) ? new List<PromotionLanguageSetting>() :
                                           input.Languages?.Names?.Select(x => new PromotionLanguageSetting { PromotionId = input.Id, LanguageId = x,
                                               Type = input.Languages.Type ?? (int)BonusSettingConditionTypes.InSet }).ToList(),
                Order = input.Order,
                ParentId = input.ParentId,
                StyleType = input.StyleType
            };
        }

        public static ApiPromotion ToApiPromotion(this Promotion input, double timeZone)
        {
            return new ApiPromotion
            {
                Id = input.Id,
                PartnerId = input.PartnerId,
                NickName = input.NickName,
                Type = input.Type,
                State = input.State,
                Title = input.Title,
                Description = input.Description,
                ImageName = input.ImageName,
                StartDate = input.StartDate.GetGMTDateFromUTC(timeZone),
                FinishDate = input.FinishDate.GetGMTDateFromUTC(timeZone),
                CreationTime = input.CreationTime.GetGMTDateFromUTC(timeZone),
                LastUpdateTime = input.LastUpdateTime.GetGMTDateFromUTC(timeZone),
                Order = input.Order,
                Segments = input.PromotionSegmentSettings != null && input.PromotionSegmentSettings.Any() ? new ApiSetting
                {
                    Type = input.PromotionSegmentSettings.First().Type,
                    Ids = input.PromotionSegmentSettings.Select(x => x.SegmentId).ToList()
                } : new ApiSetting { Type = (int)BonusSettingConditionTypes.InSet, Ids = new List<int>() },
                Languages = input.PromotionLanguageSettings != null && input.PromotionLanguageSettings.Any() ? new ApiSetting
                {
                    Type = input.PromotionLanguageSettings.First().Type,
                    Names = input.PromotionLanguageSettings.Select(x => x.LanguageId).ToList()
                } : new ApiSetting { Type = (int)BonusSettingConditionTypes.InSet, Names = new List<string>() },
                StyleType = input.StyleType,
                ParentId = input.ParentId
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

        #endregion

        #region Clients
        public static ApiClientLimit MapToApiClientLimit(this ClientCustomSettings clientSettings, double timeZone)
        {
            return new ApiClientLimit
            {
                DepositLimitDaily = clientSettings.DepositLimitDaily,
                DepositLimitWeekly = clientSettings.DepositLimitWeekly,
                DepositLimitMonthly = clientSettings.DepositLimitMonthly,
                TotalBetAmountLimitDaily = clientSettings.TotalBetAmountLimitDaily,
                TotalBetAmountLimitWeekly = clientSettings.TotalBetAmountLimitWeekly,
                TotalBetAmountLimitMonthly = clientSettings.TotalBetAmountLimitMonthly,
                TotalLossLimitDaily = clientSettings.TotalLossLimitDaily,
                TotalLossLimitWeekly = clientSettings.TotalLossLimitWeekly,
                TotalLossLimitMonthly = clientSettings.TotalLossLimitMonthly,
                SessionLimit = clientSettings.SessionLimit,
                SessionLimitDaily = clientSettings.SessionLimitDaily,
                SessionLimitWeekly = clientSettings.SessionLimitWeekly,
                SessionLimitMonthly = clientSettings.SessionLimitMonthly,
                SelfExcludedUntil = clientSettings.SelfExcludedUntil?.GetGMTDateFromUTC(timeZone),
                SystemDepositLimitDaily = clientSettings.SystemDepositLimitDaily,
                SystemDepositLimitWeekly = clientSettings.SystemDepositLimitWeekly,
                SystemDepositLimitMonthly = clientSettings.SystemDepositLimitMonthly,
                SystemTotalBetAmountLimitDaily = clientSettings.SystemTotalBetAmountLimitDaily,
                SystemTotalBetAmountLimitWeekly = clientSettings.SystemTotalBetAmountLimitWeekly,
                SystemTotalBetAmountLimitMonthly = clientSettings.SystemTotalBetAmountLimitMonthly,
                SystemTotalLossLimitDaily = clientSettings.SystemTotalLossLimitDaily,
                SystemTotalLossLimitWeekly = clientSettings.SystemTotalLossLimitWeekly,
                SystemTotalLossLimitMonthly = clientSettings.SystemTotalLossLimitMonthly,
                SystemSessionLimit = clientSettings.SystemSessionLimit,
                SystemSessionLimitDaily = clientSettings.SystemSessionLimitDaily,
                SystemSessionLimitWeekly = clientSettings.SystemSessionLimitWeekly,
                SystemSessionLimitMonthly = clientSettings.SystemSessionLimitMonthly,
                SystemExcludedUntil = clientSettings.SystemExcludedUntil?.GetGMTDateFromUTC(timeZone),
                SelfExclusionPeriod = clientSettings.SelfExclusionPeriod
            };
        }
        public static ApiClientPaymentInfo MapToApiClientPaymentInfo(this ClientPaymentInfo info, double timeZone)
        {
            return new ApiClientPaymentInfo
            {
                Id = info.Id,
                ClientId = info.ClientId,
                CardholderName = info.ClientFullName,
                CardNumber = info.CardNumber,
                CardExpireDate = info.CardExpireDate,
                BankName = info.BankName,
                BankAccountNumber = info.BankAccountNumber,
                Iban = info.BankIBAN,
                NickName = info.AccountNickName,
                Type = info.Type,
                State = info.State,
                WalletNumber = info.WalletNumber,
                PaymentSystem = info.PartnerPaymentSystemId,
                CreationTime = info.CreationTime.GetGMTDateFromUTC(timeZone),
                LastUpdateTime = info.LastUpdateTime.GetGMTDateFromUTC(timeZone)
            };
        }

        public static ClientPaymentInfo MapToClientPaymentInfo(this ApiClientPaymentInfo info)
        {
            return new ClientPaymentInfo
            {
                Id = info.Id,
                ClientId = info.ClientId,
                ClientFullName = info.CardholderName,
                CardNumber = info.CardNumber,
                CardExpireDate = info.CardExpireDate,
                BankName = info.BankName,
                BankIBAN = info.Iban,
                BankAccountNumber = info.BankAccountNumber,
                AccountNickName = info.NickName,
                Type = info.Type,
                State = info.State,
                WalletNumber = info.WalletNumber,
                PartnerPaymentSystemId = info.PaymentSystem,
                CreationTime = info.CreationTime,
                LastUpdateTime = info.LastUpdateTime
            };
        }
        #endregion

        #region Affiliates

        public static ApiFnAffiliateModel ToApifnAffiliateModel(this fnAffiliate arg, double timeZone)
        {
            return new ApiFnAffiliateModel
            {
                Id = arg.Id,
                Email = arg.Email,
                UserName = arg.UserName,
                PartnerId = arg.PartnerId,
                Gender = arg.Gender,
                FirstName = arg.FirstName,
                LastName = arg.LastName,
                NickName = arg.NickName,
                RegionId = arg.RegionId,
                MobileNumber = arg.MobileNumber,
                LanguageId = arg.LanguageId,
                CreationTime = arg.CreationTime.GetGMTDateFromUTC(timeZone),
                LastUpdateTime = arg.LastUpdateTime.GetGMTDateFromUTC(timeZone),
                State = arg.State,
                CommunicationType = arg.CommunicationType,
                CommunicationTypeValue = arg.CommunicationTypeValue
            };
        }

        public static ApiFnAffiliateModel ToApifnAffiliateModel(this BllAffiliate arg, double timeZone)
        {
            return new ApiFnAffiliateModel
            {
                Id = arg.AffiliateId,
                Email = arg.Email,
                UserName = arg.UserName,
                PartnerId = arg.PartnerId,
                Gender = arg.Gender,
                FirstName = arg.FirstName,
                LastName = arg.LastName,
                NickName = arg.NickName,
                RegionId = arg.RegionId,
                MobileNumber = arg.MobileNumber,
                LanguageId = arg.LanguageId,
                CreationTime = arg.CreationTime.GetGMTDateFromUTC(timeZone),
                LastUpdateTime = arg.LastUpdateTime.GetGMTDateFromUTC(timeZone),
                State = arg.State,
                FixedFeeCommission = arg.FixedFeeCommission,
                DepositCommission = arg.DepositCommission,
                TurnoverCommission = arg.TurnoverCommission,
                GGRCommission = arg.GGRCommission,
                CommunicationType = arg.CommunicationType,
                CommunicationTypeValue = arg.CommunicationTypeValue
            };
        }

        public static fnAffiliate ToFnAffiliate(this ApiFnAffiliateModel arg)
        {
            return new fnAffiliate
            {
                Id = arg.Id,
                Email = arg.Email,
                UserName = arg.UserName,
                PartnerId = arg.PartnerId,
                Gender = arg.Gender,
                FirstName = arg.FirstName,
                LastName = arg.LastName,
                NickName = arg.NickName,
                RegionId = arg.RegionId,
                MobileNumber = arg.MobileNumber,
                LanguageId = arg.LanguageId,
                State = arg.State
            };
        }

        #endregion

        #region WebSiteMenu

        public static List<ApiWebSiteMenu> MapToApiWebSiteMenus(this IEnumerable<WebSiteMenu> webSiteMenus)
        {
            return webSiteMenus.Select(x => x.MapToApiWebSiteMenu()).OrderBy(x => x.Type).ToList();
        }

        public static ApiWebSiteMenu MapToApiWebSiteMenu(this WebSiteMenu webSiteMenu)
        {
            return new ApiWebSiteMenu
            {
                Id = webSiteMenu.Id,
                Type = webSiteMenu.Type,
                StyleType = webSiteMenu.StyleType
            };
        }

        public static List<ApiWebSiteMenuItem> MapToApiWebSiteMenuItems(this IEnumerable<WebSiteMenuItem> webSiteMenuItems)
        {
            return webSiteMenuItems.Select(x => x.MapToApiWebSiteMenuItem()).ToList();
        }

        public static ApiWebSiteMenuItem MapToApiWebSiteMenuItem(this WebSiteMenuItem webSiteMenuItem)
        {
            return new ApiWebSiteMenuItem
            {
                Id = webSiteMenuItem.Id,
                MenuId = webSiteMenuItem.MenuId,
                Icon = webSiteMenuItem.Icon,
                Title = webSiteMenuItem.Title,
                Type = webSiteMenuItem.Type,
                StyleType = webSiteMenuItem.StyleType,
                Href = webSiteMenuItem.Href,
                OpenInRouting = webSiteMenuItem.OpenInRouting,
                Orientation = webSiteMenuItem.Orientation,
                Order = webSiteMenuItem.Order
            };
        }

        public static WebSiteMenu MapToWebSiteMenu(this ApiWebSiteMenu apiWebSiteMenu)
        {
            return new WebSiteMenu
            {
                Id = apiWebSiteMenu.Id,
                Type = apiWebSiteMenu.Type,
                PartnerId = apiWebSiteMenu.PartnerId,
                StyleType = apiWebSiteMenu.StyleType
            };
        }

        public static WebSiteMenuItem MapToWebSiteMenuItem(this ApiWebSiteMenuItem apiWebSiteMenuItem)
        {
            return new WebSiteMenuItem
            {
                Id = apiWebSiteMenuItem.Id ?? 0,
                MenuId = apiWebSiteMenuItem.MenuId,
                Icon = string.IsNullOrEmpty(apiWebSiteMenuItem.Icon) ? string.Empty : apiWebSiteMenuItem.Icon,
                Title = apiWebSiteMenuItem.Title,
                Type = string.IsNullOrEmpty(apiWebSiteMenuItem.Type) ? string.Empty : apiWebSiteMenuItem.Type,
                StyleType = string.IsNullOrEmpty(apiWebSiteMenuItem.StyleType) ? string.Empty : apiWebSiteMenuItem.StyleType,
                Href = string.IsNullOrEmpty(apiWebSiteMenuItem.Href) ? string.Empty : apiWebSiteMenuItem.Href,
                OpenInRouting = apiWebSiteMenuItem.OpenInRouting,
                Orientation = apiWebSiteMenuItem.Orientation,
                Order = apiWebSiteMenuItem.Order,
                Image = apiWebSiteMenuItem.Image
            };
        }

        public static List<ApiWebSiteSubMenuItem> MapToApiWebSiteSubMenuItems(this IEnumerable<WebSiteSubMenuItem> webSiteSubMenuItems)
        {
            return webSiteSubMenuItems.Select(x => x.MapToApiWebSiteSubMenuItem()).ToList();
        }

        public static ApiWebSiteSubMenuItem MapToApiWebSiteSubMenuItem(this WebSiteSubMenuItem webSitSubeMenuItem)
        {
            return new ApiWebSiteSubMenuItem
            {
                Id = webSitSubeMenuItem.Id,
                MenuItemId = webSitSubeMenuItem.MenuItemId,
                Icon = string.IsNullOrEmpty(webSitSubeMenuItem.Icon) ? string.Empty : webSitSubeMenuItem.Icon,
                Title = webSitSubeMenuItem.Title,
                Type = string.IsNullOrEmpty(webSitSubeMenuItem.Type) ? string.Empty : webSitSubeMenuItem.Type,
                StyleType = string.IsNullOrEmpty(webSitSubeMenuItem.StyleType) ? string.Empty : webSitSubeMenuItem.StyleType,
                Href = string.IsNullOrEmpty(webSitSubeMenuItem.Href) ? string.Empty : webSitSubeMenuItem.Href,
                OpenInRouting = webSitSubeMenuItem.OpenInRouting,
                Order = webSitSubeMenuItem.Order
            };
        }

        public static WebSiteSubMenuItem MapToWebSiteSubMenuItem(this ApiWebSiteSubMenuItem apiWebSitSubeMenuItem)
        {
            return new WebSiteSubMenuItem
            {
                Id = apiWebSitSubeMenuItem.Id ?? 0,
                MenuItemId = apiWebSitSubeMenuItem.MenuItemId,
                Icon = string.IsNullOrEmpty(apiWebSitSubeMenuItem.Icon) ? string.Empty : apiWebSitSubeMenuItem.Icon,
                Title = apiWebSitSubeMenuItem.Title,
                Type = string.IsNullOrEmpty(apiWebSitSubeMenuItem.Type) ? string.Empty : apiWebSitSubeMenuItem.Type,
                StyleType = string.IsNullOrEmpty(apiWebSitSubeMenuItem.StyleType) ? string.Empty : apiWebSitSubeMenuItem.StyleType,
                Href = string.IsNullOrEmpty(apiWebSitSubeMenuItem.Href) ? string.Empty : apiWebSitSubeMenuItem.Href,
                OpenInRouting = apiWebSitSubeMenuItem.OpenInRouting,
                Order = apiWebSitSubeMenuItem.Order,
                Image = apiWebSitSubeMenuItem.Image
            };
        }

        #endregion

        #region Provider

        public static AffiliatePlatformModel MapToAffiliatePlatformModel(this AffiliatePlatform affiliatePlatform)
        {
            return new AffiliatePlatformModel
            {
                Id = affiliatePlatform.Id,
                PartnerId = affiliatePlatform.PartnerId,
                Name = affiliatePlatform.Name,
                Status = affiliatePlatform.Status,
                LastExecutionTime = affiliatePlatform.LastExecutionTime,
                PeriodInHours = affiliatePlatform.PeriodInHours
            };
        }

        public static NotificationServiceModel MapToNotificationServiceModel(this NotificationService notificationService)
        {
            return new NotificationServiceModel
            {
                Id = notificationService.Id,
                Name = notificationService.Name,
                CreationTime = notificationService.CreationTime
            };
        }

        #endregion

        public static FilterAnnouncement MapToFilterAnnouncement(this ApiFilterAnnouncement filter)
        {
            return new FilterAnnouncement
            {
                PartnerId = filter.PartnerId,
                Type = filter.Type,
                TakeCount = filter.TakeCount,
                SkipCount = filter.SkipCount,
                FromDate = filter.FromDate,
                ToDate = filter.ToDate,
                OrderBy = filter.OrderBy,
                FieldNameToOrderBy = filter.FieldNameToOrderBy,

                Ids = filter.Ids == null ? new FiltersOperation() : filter.Ids.MapToFiltersOperation(),
                PartnerIds = filter.PartnerIds == null ? new FiltersOperation() : filter.PartnerIds.MapToFiltersOperation(),
                UserIds = filter.UserIds == null ? new FiltersOperation() : filter.UserIds.MapToFiltersOperation(),
                Types = filter.Types == null ? new FiltersOperation() : filter.Types.MapToFiltersOperation(),
                ReceiverTypes = filter.ReceiverTypes == null ? new FiltersOperation() : filter.ReceiverTypes.MapToFiltersOperation(),
                States = filter.States == null ? new FiltersOperation() : filter.States.MapToFiltersOperation()
            };
        }

        public static ApiAnnouncement MapToApiAnnouncement(this fnAnnouncement input, double timeZone)
        {
            return new ApiAnnouncement
            {
                Id = input.Id,
                PartnerId = input.PartnerId,
                UserId = input.UserId ?? 0,
                Type = input.Type,
                ReceiverType = input.ReceiverType,
                State = input.State,
                NickName = input.NickName,
                CreationDate = input.CreationDate.GetGMTDateFromUTC(timeZone),
                LastUpdateDate = input.LastUpdateDate.GetGMTDateFromUTC(timeZone)
            };
        }

        public static DAL.Character MapToCharacter(this ApiCharacter input)
        {
            return new DAL.Character
            {
                Id = input.Id,
                PartnerId = input.PartnerId,
                ParentId = input.ParentId,
                NickName = input.NickName,
                Title = input.Title,
                Description = input.Description,
                Status = input.Status,
                Order = input.Order,
                CompPoints = input.CompPoints,
                ImageData = input.ImageData,
                BackgroundImageData = input.BackgroundImageData
            };
        }

        public static ApiCharacter MapToApiCharacter(this Character input)
        {
            return new ApiCharacter
            {
                Id = input.Id,
                PartnerId = input.PartnerId,
                ParentId = input.ParentId,
                NickName = input.NickName,
                Title = input.Title,
                Description = input.Description,
                Status = input.Status,
                Order = input.Order,
                ImageData = input.ImageUrl,
                BackgroundImageData = input.BackgroundImageUrl
            };
        }

        public static ApiCharacter MapToApiCharacter(this fnCharacter character)
        {
            return new ApiCharacter
            {
				Id = character.Id,
				ParentId = character.ParentId,
				PartnerId = character.PartnerId,
				NickName = character.NickName,
				Title = character.Title,
				Description = character.Description,
				Status = character.Status,
				Order = character.Order,
				ImageData = character.ImageUrl,
				BackgroundImageData = character.BackgroundImageUrl,
				CompPoints = character.CompPoints
			};
        }

        public static FilterClientGame MapToFilterClientGame(this ApiFilterClientGame apiFilterClientGame)
        {
            return new FilterClientGame
			{
                PartnerId = apiFilterClientGame.PartnerId,
                FromDate = apiFilterClientGame.FromDate,
                ToDate = apiFilterClientGame.ToDate,
                ClientIds = apiFilterClientGame.ClientIds == null ? new FiltersOperation() : apiFilterClientGame.ClientIds.MapToFiltersOperation(),
                FirstNames = apiFilterClientGame.FirstNames == null ? new FiltersOperation() : apiFilterClientGame.FirstNames.MapToFiltersOperation(),
                LastNames = apiFilterClientGame.LastNames == null ? new FiltersOperation() : apiFilterClientGame.LastNames.MapToFiltersOperation(),
                ProductIds = apiFilterClientGame.ProductIds == null ? new FiltersOperation() : apiFilterClientGame.ProductIds.MapToFiltersOperation(),
                ProductNames = apiFilterClientGame.ProductNames == null ? new FiltersOperation() : apiFilterClientGame.ProductNames.MapToFiltersOperation(),
                ProviderNames = apiFilterClientGame.ProviderNames == null ? new FiltersOperation() : apiFilterClientGame.ProviderNames.MapToFiltersOperation(),
                Currencies = apiFilterClientGame.CurrencyIds == null ? new FiltersOperation() : apiFilterClientGame.CurrencyIds.MapToFiltersOperation(),
            };
        }
    }
}