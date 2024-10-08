using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Newtonsoft.Json;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Report;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.AdminWebApi.Models.DashboardModels;
using IqSoft.CP.DAL.Models.Dashboard;
using IqSoft.CP.AdminWebApi.Models.ClientModels;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.AdminWebApi.Models.ReportModels;
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
using IqSoft.CP.AdminWebApi.Models.NotificationModels;
using IqSoft.CP.AdminWebApi.Models.PartnerModels;
using IqSoft.CP.AdminWebApi.Models.PaymentModels;
using IqSoft.CP.AdminWebApi.Models.ProductModels;
using IqSoft.CP.AdminWebApi.Models.ReportModels.BetShop;
using IqSoft.CP.AdminWebApi.Models.ReportModels.Internet;
using IqSoft.CP.AdminWebApi.Models.AgentModels;
using IqSoft.CP.AdminWebApi.Models;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.DAL.Models.Bonuses;
using IqSoft.CP.Common.Models.AdminModels;
using static IqSoft.CP.Common.Constants;
using IqSoft.CP.AdminWebApi.Models.CRM;
using IqSoft.CP.Common.Models.AffiliateModels;
using IqSoft.CP.DAL.Models.Clients;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DataWarehouse;
using IqSoft.CP.DataWarehouse.Models;
using IqSoft.CP.Common.Models.Report;
using IqSoft.CP.Common.Models.AgentModels;
using IqSoft.CP.DAL.Models.Agents;

using Client = IqSoft.CP.DAL.Client;
using Document = IqSoft.CP.DAL.Document;
using User = IqSoft.CP.DAL.User;
using ClientSession = IqSoft.CP.DAL.ClientSession;
using AffiliatePlatform = IqSoft.CP.DAL.AffiliatePlatform;
using Partner = IqSoft.CP.DAL.Partner;
using Bonu = IqSoft.CP.DAL.Bonu;
using AgentCommission = IqSoft.CP.DAL.AgentCommission;
using PermissionModel = IqSoft.CP.AdminWebApi.Models.RoleModels.PermissionModel;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Integration.Products.Models.BGGames;

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

        #region RealTime

        public static ApiRealTimeInfo MapToApiRealTimeInfo(this RealTimeInfo info, double timeZone)
        {
            return new ApiRealTimeInfo
            {
                OnlineClients = info.OnlineClients.Select(x => x.MapToApiOnlineClient(timeZone)).ToList(),
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

        public static ApiOnlineClient MapToApiOnlineClient(this BllOnlineClient info, double timeZone)
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
                RegistrationDate = info.RegistrationDate.GetGMTDateFromUTC(timeZone),
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
                BonusWinAmount = fnInternetBet.BonusWinAmount,
                OriginalBonusWinAmount = fnInternetBet.OriginalBonusWinAmount,
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

        public static ApiReportByUserTransaction MapToApiReportByUserTransaction(this fnReportByUserTransaction input, double timeZone)
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
                CreationTime = input.CreationTime.GetGMTDateFromUTC(timeZone)
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

        public static ApiUser MapToUserModel(this User user, double timeZone)
        {
            return new ApiUser
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
                Path = user.Path
            };
        }

        public static List<ApiUser> MapToUserModels(this IEnumerable<fnUser> users, double timeZone)
        {
            return users.Select(x => x.MapToUserModel(timeZone)).ToList();
        }

        public static ApiUser MapToUserModel(this fnUser user, double timeZone)
        {
            var balance = BaseBll.GetObjectBalance((int)ObjectTypes.User, user.Id);
            return new ApiUser
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
                UserRoles = user.UserRoles,
                Balance = Math.Floor(balance.Balances.Sum(x => BaseBll.ConvertCurrency(x.CurrencyId, user.CurrencyId, x.Balance)) * 100) / 100,
                Path = user.Type == (int)UserTypes.CompanyAgent ? ("/" + user.Id + "/") : string.Empty
            };
        }

        public static ApiUserConfiguration ToApiUserConfiguration(this UserConfiguration input, double timeZone)
        {
            return new ApiUserConfiguration
            {
                Id = input.Id,
                UserId = input.UserId,
                CreatedBy = input.CreatedBy,
                Name = input.Name,
                BooleanValue = input.BooleanValue,
                NumericValue = input.NumericValue,
                StringValue = input.StringValue,
                CreationTime = input.CreationTime.GetGMTDateFromUTC(timeZone),
                LastUpdateTime = input.LastUpdateTime.GetGMTDateFromUTC(timeZone)
            };
        }

        public static User ToUser(this ApiUser user, double timeZone)
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
                UserConfigurations = user.Configurations == null ? new List<UserConfiguration>() : 
                    user.Configurations.Select(x => x.ToUserConfiguration(timeZone)).ToList()
            };
        }

        public static UserConfiguration ToUserConfiguration(this ApiUserConfiguration input, double timeZone)
        {
            return new UserConfiguration
            {
                Id = input.Id,
                UserId = input.UserId,
                CreatedBy = input.CreatedBy,
                Name = input.Name,
                BooleanValue = input.BooleanValue,
                NumericValue = input.NumericValue,
                StringValue = input.StringValue,
                CreationTime = input.CreationTime.GetUTCDateFromGMT(timeZone),
                LastUpdateTime = input.LastUpdateTime.GetUTCDateFromGMT(timeZone)
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

        public static Client MapToClient(this ChangeClientDetailsInput input, Client client, double timeZone)
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
                PhoneNumber = input.MobileCode ?? client.PhoneNumber,
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
                BirthDate = input.BirthDate?.Date ?? client.BirthDate,
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

        public static Client MapToClient(this NewClientModel input, double timeZone)
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
                PhoneNumber =  string.IsNullOrWhiteSpace(input.MobileCode) ? string.Empty : (input.MobileCode.StartsWith("+") ? input.MobileCode : "+" + input.MobileCode),
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
                BirthDate = input.BirthDate,//No need to consider the timezone
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
                ExpirationTime = clientIdentity.ExpirationTime.GetGMTDateFromUTC(timeZone)
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
                ExpirationTime = clientIdentity.ExpirationTime.GetGMTDateFromUTC(timeZone)
            };
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
                BirthDate = client.BirthDate.GetGMTDateFromUTC(timeZone),
                Age = (DateTime.UtcNow - client.BirthDate).Days / 365,
                FirstName = client.FirstName,
                LastName = client.LastName,
                NickName = client.NickName,
                RegistrationIp = client.RegistrationIp,
                DocumentNumber = client.DocumentNumber,
                DocumentIssuedBy = client.DocumentIssuedBy,
                Address = client.Address,
                MobileNumber = hideClientContactInfo ? "*****" : 
                (!string.IsNullOrEmpty(client.PhoneNumber) ? client.MobileNumber.TrimStart(client.PhoneNumber.ToCharArray()) :  client.MobileNumber),
                MobileCode = hideClientContactInfo ? "*****" : client.PhoneNumber,
                IsMobileNumberVerified = client.IsMobileNumberVerified,
                LanguageId = client.LanguageId,
                CreationTime = client.CreationTime.GetGMTDateFromUTC(timeZone),
                LastUpdateTime = client.LastUpdateTime.GetGMTDateFromUTC(timeZone),
                RegionId = client.RegionId,
                CountryId = client.CountryId,
                SendMail = client.SendMail,
                SendSms = client.SendSms,
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
                PinCode = client.PinCode,
				UnderMonitoringTypes = underMonitoringTypes?.StringValue != null ? JsonConvert.DeserializeObject<List<int>>(underMonitoringTypes.StringValue) : null,
                Duplicated = client.Duplicated
            };
        }

        public static ClientInfoModel MapToClientInfoModel(this DAL.Models.Clients.ClientInfo client, bool hideContactInfo, double timeZone)
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
                RegistrationDate = client.RegistrationDate.GetGMTDateFromUTC(timeZone),
                Status = client.Status,
                Balance = client.Balance,
                BonusBalance = client.BonusBalance,
                WithdrawableBalance = client.WithdrawableBalance,
                CompPointBalance = client.CompPointBalance,
                GGR = client.GGR,
                NGR = client.NGR,
                Rake = client.Rake,
                TotalDepositsCount = client.TotalDepositsCount,
                TotalDepositsAmount = client.TotalDepositsAmount,
                TotalDepositsPartnerConvertedAmount = client.TotalDepositsPartnerConvertedAmount,
                TotalWithdrawalsCount = client.TotalWithdrawalsCount,
                TotalWithdrawalsAmount = client.TotalWithdrawalsAmount,
                TotalWithdrawalsPartnerConvertedAmount = client.TotalWithdrawalsPartnerConvertedAmount,
                FailedDepositsCount = client.FailedDepositsCount,
                FailedDepositsAmount = client.FailedDepositsAmount,
                TotalBetsCount = client.TotalBetsCount,
                TotalPartnerConvertedBetsAmount = client.TotalBetsPartnerConvertedAmount,
                SportBetsCount = client.SportBetsCount,
                CasinoBetsCount = client.CasinoBetsCount,
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
                OperationTime = element.OperationTime.GetGMTDateFromUTC(timeZone),
                PaymentSystemName = element.PaymentSystemName
            };
        }

        public static PaymentLimit MapToPaymentLimit(this ApiPaymentLimit paymentLimit, double timeZone)
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
                StartTime = paymentLimit.StartTime.GetGMTDateFromUTC(timeZone),
                EndTime = paymentLimit.EndTime.GetGMTDateFromUTC(timeZone),
                RowState = paymentLimit.RowState
            };
        }

        public static ApiPaymentLimit MapToApiPaymentLimit(this PaymentLimit paymentLimit, int clientId, double timeZone)
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
                StartTime = paymentLimit == null ? null : paymentLimit.StartTime.GetGMTDateFromUTC(timeZone),
                EndTime = paymentLimit == null ? null : paymentLimit.EndTime.GetGMTDateFromUTC(timeZone)
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

        public static List<ClientSessionModel> MapToClientSessionModels(this IEnumerable<DAL.fnClientSession> sessions, double timeZone)
        {
            return sessions.Select(x => x.MapToClientSessionModel(timeZone)).ToList();
        }

        public static ClientSessionModel MapToClientSessionModel(this DAL.fnClientSession session, double timeZone)
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

        public static List<ClientSessionModel> MapToClientSessionModels(this IEnumerable<DataWarehouse.ClientSession> sessions, double timeZone)
        {
            return sessions.Select(x => x.MapToClientSessionModel(timeZone)).ToList();
        }
        public static ClientSessionModel MapToClientSessionModel(this DataWarehouse.ClientSession session, double timeZone)
        {
            var productName = CacheManager.GetProductById(session.ProductId);
            return new ClientSessionModel
            {
                Id = session.Id,
                ClientId = session.ClientId,
                Country = session.Country,
                ProductId = session.ProductId,
                ProductName = productName?.Name,
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
        public static ClientSessionModel MapToClientSessionModel(this DAL.ClientSession session, double timeZone)
        {
            var productName = CacheManager.GetProductById(session.ProductId);
            return new ClientSessionModel
            {
                Id = session.Id,
                ClientId = session.ClientId,
                Country = session.Country,
                ProductId = session.ProductId,
                ProductName = productName?.Name,
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

        public static BetShop MapToBetshop(this ApiGetBetShopByIdOutput input, double timeZone)
        {
            return new BetShop
            {
                Id = input.Id,
                Address = input.Address,
                CurrencyId = input.CurrencyId,
                GroupId = input.GroupId,
                SessionId = input.SessionId,
                CreationTime = input.CreationTime.GetGMTDateFromUTC(timeZone),
                LastUpdateTime = input.LastUpdateTime.GetGMTDateFromUTC(timeZone),
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
                CreationTime = ticketMessage.CreationTime.GetGMTDateFromUTC(timeZone),
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
                CreationTime = ticket.CreationTime.GetGMTDateFromUTC(timeZone),
                LastMessageTime = ticket.LastMessageTime.GetGMTDateFromUTC(timeZone),
                UnreadMessagesCount = ticket.UserUnreadMessagesCount ?? 0,
                UserName = ticket.UserName,
                StatusName = statuses.FirstOrDefault(x => x.Value == ticket.Status)?.Text,
                TypeName = types.FirstOrDefault(x => x.Value == ticket.Type)?.Text,
                UserId = ticket.UserId,
                UserFirstName = ticket.UserFirstName,
                UserLastName = ticket.UserLastName
            };
        }

        public static ApiTicketMessage ToApiTicketMessage(this TicketMessage ticketMessage, double timeZone)
        {
            return new ApiTicketMessage
            {
                Id = ticketMessage.Id,
                Message = ticketMessage.Message,
                Type = ticketMessage.Type,
                CreationTime = ticketMessage.CreationTime.GetGMTDateFromUTC(timeZone),
                TicketId = ticketMessage.TicketId,
                UserId = ticketMessage.UserId,
                UserFirstName = ticketMessage.User == null ? string.Empty : ticketMessage.User.FirstName,
                UserLastName = ticketMessage.User == null ? string.Empty : ticketMessage.User.LastName
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
                GenderName =arg.Gender.HasValue ? Enum.GetName(typeof(Gender), arg.Gender) : string.Empty,
                BirthDate = arg.BirthDate != DateTime.MinValue ? arg.BirthDate.ToString() : string.Empty,
                Age = arg.BirthDate != DateTime.MinValue ? arg.Age : 0,
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
                MobileCode = arg.PhoneNumber,
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
                Duplicated = bool.TryParse(arg.Duplicated, out bool idDuplicated) && idDuplicated,
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
                LastUpdateTime = arg.LastUpdateTime.GetGMTDateFromUTC(timeZone),
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
                LastDepositDate = arg.LastDepositDate.GetGMTDateFromUTC(timeZone)
            };
        }

        public static fnClientModel MapTofnClientModel(this Client client, double timeZone)
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
                MobileCode = client.PhoneNumber,
                IsMobileNumberVerified = client.IsMobileNumberVerified,
                LanguageId = client.LanguageId,
                CreationTime = client.CreationTime.GetGMTDateFromUTC(timeZone),
                LastUpdateTime = client.LastUpdateTime.GetGMTDateFromUTC(timeZone),
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
                PasswordRegExProperty = new Common.Models.RegExProperty(partner.PasswordRegExp),
                VipLevel = partner.VipLevel
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
                CreationTime = email.CreationTime.GetGMTDateFromUTC(timeZone),
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
                TotalAmount = Math.Round(request.TotalAmount, 2),
                TotalFinalAmount = Math.Round(request.TotalFinalAmount, 2),
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
                ConvertedAmount = Math.Round(request.ConvertedAmount, 2),
                FinalAmount = request.FinalAmount ?? request.Amount,
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
                    resp.PaymentForm = prm["PaymentForm"];
            }
            return resp;
        }

        #endregion

        #region PaymentSystem

        public static List<ApiPaymentSystemModel> MapToPaymentSystemModels(this IEnumerable<PaymentSystem> paymentSystems, double timeZone)
        {
            return paymentSystems.Select(x => x.MapToPaymentSystemModel(timeZone)).ToList();
        }

        public static ApiPaymentSystemModel MapToPaymentSystemModel(this PaymentSystem paymentSystem, double timeZone)
        {
            return new ApiPaymentSystemModel
            {
                Id = paymentSystem.Id,
                Name = paymentSystem.Name,
                Type = paymentSystem.Type,
                IsActive = paymentSystem.IsActive,
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
                RolePermissions = role.RolePermissions.Select(x => x.MapToRolePermissionModel()).ToList()
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
                RolePermissions = roleModel.RolePermissions.Select(x => x.MapToRolePermission()).ToList()
            };
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
                State = product.State ?? (int)ProductStates.Active,
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

        public static fnProduct MapTofnProduct(this FnProductModel product)
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
                BetValues = product.BetValues,
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
                TranslationId = product.TranslationId,
                ExternalId = product.ExternalId,
                State = product.State,
                StateName = Enum.GetName(typeof(ProductStates), product.State),
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

        public static PartnerBankInfo MapToPartnerBankInfo(this ApiPartnerBankInfo partnerBankInfo, double timeZone)
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
                CreationTime = partnerBankInfo.CreationTime.GetUTCDateFromGMT(timeZone),
                LastUpdateTime = partnerBankInfo.LastUpdateTime.GetUTCDateFromGMT(timeZone)
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
                ApplyPercentAmount = partnerPaymentSetting.ApplyPercentAmount,
                Info = partnerPaymentSetting.Info,
                MinAmount = partnerPaymentSetting.MinAmount,
                MaxAmount = partnerPaymentSetting.MaxAmount,
                AllowMultipleClientsPerPaymentInfo = partnerPaymentSetting.AllowMultipleClientsPerPaymentInfo,
                AllowMultiplePaymentInfoes = partnerPaymentSetting.AllowMultiplePaymentInfoes,
                UserName = string.Empty,
                Password = string.Empty,
                Priority = partnerPaymentSetting.PaymentSystemPriority,
                Countries = partnerPaymentSetting.PartnerPaymentCountrySettings != null && partnerPaymentSetting.PartnerPaymentCountrySettings.Any() ? new ApiSetting
                {
                    Type = partnerPaymentSetting.PartnerPaymentCountrySettings.First().Type,
                    Ids = partnerPaymentSetting.PartnerPaymentCountrySettings.Select(x => x.CountryId).ToList()
                } : new ApiSetting { Type = (int)BonusSettingConditionTypes.InSet, Ids = new List<int>() },
                Segments = partnerPaymentSetting.PartnerPaymentSegmentSettings != null && partnerPaymentSetting.PartnerPaymentSegmentSettings.Any() ? new ApiSetting
                {
                    Type = partnerPaymentSetting.PartnerPaymentSegmentSettings.First().Type,
                    Ids = partnerPaymentSetting.PartnerPaymentSegmentSettings.Select(x => x.SegmentId).ToList()
                } : new ApiSetting { Type = (int)BonusSettingConditionTypes.InSet, Ids = new List<int>() },
                OSTypes = partnerPaymentSetting.OSTypes?.Split(',').Select(Int32.Parse).ToList(),
                OpenMode = partnerPaymentSetting.OpenMode,
                ImageExtension = partnerPaymentSetting.ImageExtension,
                PaymentSystemName = CacheManager.GetPaymentSystemById(partnerPaymentSetting.PaymentSystemId).Name
            };
        }

        public static PartnerPaymentSetting MapToPartnerPaymentSetting(this ApiPartnerPaymentSetting input)
        {
            var dbSetting = CacheManager.GetPartnerPaymentSettings(input.PartnerId, input.PaymentSystemId, input.CurrencyId, input.Type);
            return new PartnerPaymentSetting
            {
                Id = input.Id,
                PartnerId = input.PartnerId,
                PaymentSystemId = input.PaymentSystemId,
                State = input.State ?? (dbSetting?.State ?? 0),
                CurrencyId = input.CurrencyId,
                Commission = input.Commission ?? (dbSetting?.Commission ?? 0),
                FixedFee = input.FixedFee ?? 0,
                ApplyPercentAmount = input.ApplyPercentAmount ?? (dbSetting?.ApplyPercentAmount ?? 0),
                Type = input.Type,
                Info = input.Info ?? dbSetting?.Info,
                MinAmount = input.MinAmount ?? (dbSetting?.MinAmount ?? 0),
                MaxAmount = input.MaxAmount ?? (dbSetting?.MaxAmount ?? 0),
                AllowMultipleClientsPerPaymentInfo = input.AllowMultipleClientsPerPaymentInfo ?? dbSetting?.AllowMultipleClientsPerPaymentInfo,
                AllowMultiplePaymentInfoes = input.AllowMultiplePaymentInfoes ?? dbSetting?.AllowMultiplePaymentInfoes,
                UserName = input.UserName,
                Password = input.Password,
                PaymentSystemPriority = input.Priority ?? (dbSetting?.Priority ?? 0),
                OpenMode = input.OpenMode ?? dbSetting?.OpenMode,
                OSTypes = input.OSTypes != null ? string.Join(",", input.OSTypes) : dbSetting?.OSTypesString,
                ImageExtension = input.ImageExtension ?? dbSetting?.ImageExtension,
                PartnerPaymentCountrySettings = input?.Countries != null ? input?.Countries.Ids.Select(x => new PartnerPaymentCountrySetting
                {
                    CountryId = x,
                    Type = input.Countries.Type ?? (int)BonusSettingConditionTypes.InSet
                }).ToList() : dbSetting?.Countries?.Ids?.Select(x => new PartnerPaymentCountrySetting { CountryId = x, Type = dbSetting.Countries.Type }).ToList(),
                PartnerPaymentSegmentSettings = input?.Segments != null ? input?.Segments.Ids.Select(x => new PartnerPaymentSegmentSetting
                {
                    SegmentId = x,
                    Type = input.Segments.Type ?? (int)BonusSettingConditionTypes.InSet
                }).ToList() : dbSetting?.Segments?.Ids?.Select(x => new PartnerPaymentSegmentSetting { SegmentId = x, Type = dbSetting.Segments.Type }).ToList(),
            };
        }

        public static FnPartnerPaymentSettingModel MapTofnPartnerPaymentSettingModel(this fnPartnerPaymentSetting model, double timeZone)
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
                ImageExtension = model.ImageExtension
            };
        }


        #endregion

        #region fnPartnerProductSettings

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
                HasImages = setting.HasImages ?? false,
                CreationTime = setting.CreationTime.GetGMTDateFromUTC(timeZone),
                LastUpdateTime = setting.LastUpdateTime.GetGMTDateFromUTC(timeZone)
            };
        }

        public static fnProduct ToFnProduct(this fnPartnerProductSetting input)
        {
            return new fnProduct
            {
                Id = input.ProductId,
                GameProviderId = input.ProductGameProviderId,
                PaymentSystemId = input.PaymentSystemId,
                Level = input.Level,
                NickName = input.ProductNickName,
                ParentId = input.ParentId,
                ExternalId = input.ProductExternalId,
                State = input.ProductState,
                IsForDesktop = input.IsForDesktop,
                IsForMobile = input.IsForMobile,
                HasDemo = input.HasDemo ?? false,
                SubproviderId = input.SubproviderId,
                WebImageUrl = input.WebImageUrl,
                MobileImageUrl = input.MobileImageUrl,
                BackgroundImageUrl = input.BackgroundImageUrl,
                FreeSpinSupport = input.FreeSpinSupport,
                Jackpot = input.Jackpot,
                CategoryId = input.CategoryId,
                RTP = input.RTP,
                Volatility = input.ProductVolatility,
                HasImages = input.HasImages,
                Path = input.Path,
                Name = input.ProductName,
                GameProviderName = input.GameProviderName,
                SubproviderName = input.SubproviderName,
                IsProviderActive = input.IsProviderActive
            };
        }

        #endregion

        #region Dashboard

        public static ApiPlayersInfo MapToApiPlayersInfo(this ClientsInfo info)
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
                TotalPlayersCount = info.TotalPlayersCount,
                FTDCount = info.FTDCount,
                DailyInfo = info.DailyInfo == null ? new List<ApiPlayersDailyInfo>() : info.DailyInfo.Select(x => x.ToApiPlayersDailyInfo()).ToList()
            };
        }

        public static ApiPlayersDailyInfo ToApiPlayersDailyInfo(this ClientsDailyInfo info)
        {
            return new ApiPlayersDailyInfo
            {
                Date = info.Date,
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
                TotalPlayersCount = info.TotalPlayersCount,
                FTDCount = info.FTDCount
            };
        }

        public static ApiBetsInfo MapToApiBetsInfo(this BetsInfo info)
        {
            return new ApiBetsInfo
            {
                TotalBetsAmount = Math.Round(info.TotalBetsAmount, 2),
                TotalBetsCount = info.TotalBetsCount,
                TotalPlayersCount = info.TotalPlayersCount,
                TotalGGR = Math.Round(info.TotalGGR, 2),
                TotalNGR = Math.Round(info.TotalNGR, 2),
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
                TotalBetsCountFromTablet = info.TotalBetsCountFromTablet,
                TotalBetsFromTablet = Math.Round(info.TotalBetsFromTablet, 2),
                TotalGGRFromTablet = Math.Round(info.TotalGGRFromTablet, 2),
                TotalNGRFromTablet = Math.Round(info.TotalNGRFromTablet, 2),
                TotalPlayersCountFromTablet = info.TotalPlayersCountFromTablet,
                DailyInfo = info.DailyInfo == null ? new List<ApiBetsDailyInfo>() : info.DailyInfo.Select(x => x.ToApiBetsDailyInfo()).ToList()
            };
        }

        public static ApiBetsDailyInfo ToApiBetsDailyInfo(this BetsDailyInfo info)
        {
            return new ApiBetsDailyInfo
            {
                Date = info.Date,
                TotalBetsAmount = Math.Round(info.TotalBetsAmount, 2),
                TotalBetsCount = info.TotalBetsCount,
                TotalPlayersCount = info.TotalPlayersCount,
                TotalGGR = Math.Round(info.TotalGGR, 2),
                TotalNGR = Math.Round(info.TotalNGR, 2),
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
                TotalBetsCountFromTablet = info.TotalBetsCountFromTablet,
                TotalBetsFromTablet = Math.Round(info.TotalBetsFromTablet, 2),
                TotalGGRFromTablet = Math.Round(info.TotalGGRFromTablet, 2),
                TotalNGRFromTablet = Math.Round(info.TotalNGRFromTablet, 2),
                TotalPlayersCountFromTablet = info.TotalPlayersCountFromTablet
            };
        }

        public static ApiDepositsInfo ToApiDepositsInfo(this PaymentRequestsInfo info)
        {
            return new ApiDepositsInfo
            {
                Status = info.Status,
                TotalAmount = info.TotalAmount,
                DailyInfo = info.DailyInfo == null ? new List<ApiDepositDailyInfo>() :
                    info.DailyInfo.Select(x => x.ToApiDepositDailyInfo()).ToList(),
                Deposits = info.PaymentRequests.Select(x => new ApiDepositInfo
                {
                    PaymentSystemId = x.PaymentSystemId,
                    PaymentSystemName = x.PaymentSystemName,
                    TotalAmount = Math.Round(x.TotalAmount, 2),
                    TotalDepositsCount = x.TotalRequestsCount,
                    TotalPlayersCount = x.TotalPlayersCount,
                    DailyInfo = x.DailyInfo == null ? new List<ApiDepositDailyInfo>() :
                        x.DailyInfo.Select(y => y.ToApiDepositDailyInfo()).ToList()
                }).ToList()
            };
        }

        public static ApiDepositDailyInfo ToApiDepositDailyInfo(this PaymentDailyInfo info)
        {
            return new ApiDepositDailyInfo
            {
                Date = info.Date,
                TotalAmount = info.TotalAmount,
                TotalRequestsCount = info.TotalRequestsCount
            };
        }

        public static ApiWithdrawalsInfo ToApiWithdrawalsInfo(this PaymentRequestsInfo info)
        {
            return new ApiWithdrawalsInfo
            {
                Status = info.Status,
                TotalAmount = info.TotalAmount,
                DailyInfo = info.DailyInfo == null ? new List<ApiWithdrawDailyInfo>() :
                    info.DailyInfo.Select(x => x.ToApiWithdrawDailyInfo()).ToList(),
                Withdrawals = info.PaymentRequests.Select(x => new ApiWithdrawalInfo
                {
                    PaymentSystemId = x.PaymentSystemId,
                    PaymentSystemName = x.PaymentSystemName,
                    TotalAmount = Math.Round(x.TotalAmount, 2),
                    TotalWithdrawalsCount = x.TotalRequestsCount,
                    TotalPlayersCount = x.TotalPlayersCount,
                    DailyInfo = x.DailyInfo == null ? new List<ApiWithdrawDailyInfo>() :
                        x.DailyInfo.Select(y => y.ToApiWithdrawDailyInfo()).ToList()
                }).ToList()
            };
        }

        public static ApiWithdrawDailyInfo ToApiWithdrawDailyInfo(this PaymentDailyInfo info)
        {
            return new ApiWithdrawDailyInfo
            {
                Date = info.Date,
                TotalAmount = info.TotalAmount,
                TotalRequestsCount = info.TotalRequestsCount
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
                    GameProviderName = x.GameProviderName,
                    SubProviderName = x.SubProviderName,
                    GameProviderId = x.GameProviderId,
                    SubProviderId = x.SubProviderId,
                    TotalBetsAmount = Math.Round(x.TotalBetsAmount, 2),
                    TotalWinsAmount = Math.Round(x.TotalWinsAmount, 2),
                    TotalBetsCount = x.TotalBetsCount,
                    TotalGGR = Math.Round(x.TotalGGR, 2),
                    TotalNGR = Math.Round(x.TotalNGR, 2),
                    TotalPlayersCount = x.TotalPlayersCount,
                    TotalBetsAmountFromInternet = Math.Round(x.TotalBetsAmountFromInternet, 2),
                    TotalBetsAmountFromBetShop = Math.Round(x.TotalBetsAmountFromBetShop, 2),
                    DailyInfo = x.DailyInfo == null ? new List<ApiProviderDailyInfo>() : x.DailyInfo.Select(y => y.ToApiProviderDailyInfo()).ToList()
                }).OrderByDescending(x => x.TotalBetsAmount).ToList(),
            };
        }

        public static ApiProviderDailyInfo ToApiProviderDailyInfo(this ProviderDailyInfo info)
        {
            return new ApiProviderDailyInfo
            {
                Date = info.Date,
                GameProviderName = info.GameProviderName,
                SubProviderName = info.SubProviderName,
                GameProviderId = info.GameProviderId,
                SubProviderId = info.SubProviderId,
                TotalBetsAmount = Math.Round(info.TotalBetsAmount, 2),
                TotalWinsAmount = Math.Round(info.TotalWinsAmount, 2),
                TotalBetsCount = info.TotalBetsCount,
                TotalGGR = Math.Round(info.TotalGGR, 2),
                TotalNGR = Math.Round(info.TotalNGR, 2),
                TotalPlayersCount = info.TotalPlayersCount,
                TotalBetsAmountFromInternet = Math.Round(info.TotalBetsAmountFromInternet, 2),
                TotalBetsAmountFromBetShop = Math.Round(info.TotalBetsAmountFromBetShop, 2)
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

        public static DAL.Currency MapToCurrency(this CurrencyModel model)
        {
            return new DAL.Currency
            {
                Id = model.Id,
                CurrentRate = model.CurrentRate != 0m ? 1 / model.CurrentRate : 0,
                Symbol = model.Symbol,
                Code = model.Code,
                Name = model.Name,
                Type = model.Type
            };
        }

        public static CurrencyModel MapToCurrency(this DAL.Currency currency)
        {
            var rate = (currency.CurrentRate != 0m ? 1 / currency.CurrentRate : 0);
            return new CurrencyModel
            {
                Id = currency.Id,
                CurrentRate = rate > 10000 ? Math.Round(rate, 2) : Math.Round(rate, 0),
                Symbol = currency.Symbol,
                Code = currency.Code,
                Name = currency.Name,
                Type = currency.Type
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

        public static PartnerCurrencySetting ToPartnerCurrencySetting(this ApiPartnerCurrencySetting partnerCurrency, double timeZone)
        {
            return new PartnerCurrencySetting
            {
                Id = partnerCurrency.Id,
                PartnerId = partnerCurrency.PartnerId,
                CurrencyId = partnerCurrency.CurrencyId,
                State = partnerCurrency.State,
                CreationTime = partnerCurrency.CreationTime.GetUTCDateFromGMT(timeZone),
                LastUpdateTime = partnerCurrency.LastUpdateTime.GetUTCDateFromGMT(timeZone),
                Priority = partnerCurrency.Priority,
                UserMinLimit = partnerCurrency.UserMinLimit,
                UserMaxLimit = partnerCurrency.UserMaxLimit,
                ClientMinBet = partnerCurrency.ClientMinBet
            };
        }

        public static ApiPartnerCurrencySetting ToApiPartnerCurrencySetting(this PartnerCurrencySetting partnerCurrency, double timeZone)
        {
            return new ApiPartnerCurrencySetting
            {
                Id = partnerCurrency.Id,
                PartnerId = partnerCurrency.PartnerId,
                CurrencyId = partnerCurrency.CurrencyId,
                State = partnerCurrency.State,
                CreationTime = partnerCurrency.CreationTime.GetGMTDateFromUTC(timeZone),
                LastUpdateTime = partnerCurrency.LastUpdateTime.GetGMTDateFromUTC(timeZone),
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

        public static Bonu MapToBonus(this ApiBonus bonus, double timeZone)
        {
            return new Bonu
            {
                Id = bonus.Id ?? 0,
                Name = bonus.Name,
                Description = bonus.Description,
                PartnerId = bonus.PartnerId,
                FinalAccountTypeId = bonus.FinalAccountTypeId,
                Status = bonus.Status,
                StartTime = bonus.StartTime.GetUTCDateFromGMT(timeZone),
                FinishTime = bonus.FinishTime.GetUTCDateFromGMT(timeZone),
                LastExecutionTime = bonus.StartTime.GetUTCDateFromGMT(timeZone),
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
                        BetValues = x.BetValues
                    }).ToList(),
                Type = bonus.BonusTypeId,
                TurnoverCount = bonus.TurnoverCount,
                Info = (bonus.LinkedCampaign == true ? "1" : bonus.Info),
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
                BonusCountrySettings = bonus.Countries?.Ids?.Select(x => new BonusCountrySetting
                {
                    CountryId = x,
                    Type = bonus.Countries.Type ?? (int)BonusSettingConditionTypes.InSet
                }).ToList(),
                BonusLanguageSettings = bonus.Languages?.Names?.Select(x => new BonusLanguageSetting
                {
                    LanguageId = x,
                    Type = bonus.Languages.Type ?? (int)BonusSettingConditionTypes.InSet
                }).ToList(),
                BonusCurrencySettings = bonus.Currencies?.Names?.Select(x => new BonusCurrencySetting
                {
                    CurrencyId = x,
                    Type = bonus.Currencies.Type ?? (int)BonusSettingConditionTypes.InSet
                }).ToList(),
                BonusSegmentSettings = bonus.SegmentIds?.Ids?.Select(x => new BonusSegmentSetting
                {
                    SegmentId = x,
                    Type = bonus.SegmentIds.Type ?? (int)BonusSettingConditionTypes.InSet
                }).ToList(),
                BonusPaymentSystemSettings = bonus.PaymentSystemIds?.Ids?.Select(x => new BonusPaymentSystemSetting
                {
                    PaymentSystemId = x,
                    BonusId = bonus.Id,
                    Type = bonus.PaymentSystemIds.Type ?? (int)BonusSettingConditionTypes.InSet
                }).ToList(),
                Percent = bonus.Percent,
                FreezeBonusBalance = bonus.FreezeBonusBalance,
                Regularity = bonus.Regularity,
                DayOfWeek = bonus.DayOfWeek,
                ReusingMaxCountInPeriod = bonus.ReusingMaxCountInPeriod,
                Color = bonus.Color,
                AmountCurrencySettings = bonus.AmountSettings?.Select(x => new AmountCurrencySetting
                {
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
                SpinsCount = input.SpinsCount,
                BonusType = input.Type,
                TurnoverAmountLeft = input.Type == (int)BonusTypes.CampaignFreeBet ? null : input.TurnoverAmountLeft,
                CreationTime = input.CreationTime.GetGMTDateFromUTC(timeZone),
                AwardingTime = input.AwardingTime.GetGMTDateFromUTC(timeZone),
                FinalAmount = input.FinalAmount,
                CalculationTime = input.CalculationTime?.GetGMTDateFromUTC(timeZone),
                ValidUntil = input.ValidUntil?.GetGMTDateFromUTC(timeZone),
                LinkedBonusId = input.LinkedBonusId,
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
                    BetValues = x.BetValues
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
                Color = bonus.Color,
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

        public static TriggerSetting MapToTriggerSetting(this ApiTriggerSetting apiTriggerSetting, double timeZone)
        {
            return new TriggerSetting
            {
                Id = apiTriggerSetting.Id ?? 0,
                Name = apiTriggerSetting.Name,
                Description = apiTriggerSetting.Description,
                Type = apiTriggerSetting.Type,
                PartnerId = apiTriggerSetting.PartnerId,
                StartTime = apiTriggerSetting.StartTime.GetUTCDateFromGMT(timeZone),
                FinishTime = apiTriggerSetting.FinishTime.GetUTCDateFromGMT(timeZone),
                Percent = apiTriggerSetting.Percent,
                BonusSettingCodes = !string.IsNullOrEmpty(apiTriggerSetting.BonusSettingCodes) ? apiTriggerSetting.BonusSettingCodes : apiTriggerSetting.PromoCode,
                MinAmount = apiTriggerSetting.MinAmount,
                MaxAmount = apiTriggerSetting.MaxAmount,
                MinBetCount = apiTriggerSetting.MinBetCount,
                SegmentId = apiTriggerSetting.SegmentId,
                DayOfWeek = apiTriggerSetting.DayOfWeek,
                UpToAmount = apiTriggerSetting.UpToAmount,
                Amount = apiTriggerSetting.Amount,
                Status = apiTriggerSetting.Status,
                ConsiderBonusBets = apiTriggerSetting.ConsiderBonusBets,
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
                    UpToAmount = x.UpToAmount,
                    Amount = x.Amount
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
                Amount = triggerSetting.Amount,
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
                ConsiderBonusBets = triggerSetting.ConsiderBonusBets,
                AmountSettings = triggerSetting.AmountCurrencySettings.Select(x => new ApiAmountSetting
                {
                    CurrencyId = x.CurrencyId,
                    MinAmount = x.MinAmount,
                    MaxAmount = x.MaxAmount,
                    UpToAmount = x.UpToAmount,
                    Amount = x.Amount
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

        public static Jackpot MapToJackpot(this ApiJackpot apiJackpot, double timeZone)
        {
            var currentDate = DateTime.UtcNow;
            return new Jackpot
            {
                Id = apiJackpot.Id ?? 0,
                Name = apiJackpot.Name,
                PartnerId = apiJackpot.PartnerId,
                Type = apiJackpot.Type,
                Amount = apiJackpot.Amount,
                RightBorder = apiJackpot.RightBorder,
                LeftBorder = apiJackpot.LeftBorder,
                FinishTime = apiJackpot.FinishTime.GetUTCDateFromGMT(timeZone),
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
                FinishTime = jackpot.FinishTime.GetGMTDateFromUTC(timeZone),
                WinnedClient = jackpot.WinnerId,
                Products = jackpot.JackpotSettings?.Select(x => new ApiBonusProducts
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

        public static Common.Models.WebSiteModels.ApiLeaderboardItem ToApiLeaderboardItem(this BllLeaderboardItem item, string currencyId)
        {
            var client = CacheManager.GetClientById(item.Id);
            return new Common.Models.WebSiteModels.ApiLeaderboardItem
            {
                Name = client == null ? string.Empty : (string.IsNullOrEmpty(client.FirstName) ? client.Id.ToString() : client.FirstName),
                Points = Math.Round(BaseBll.ConvertCurrency(item.CurrencyId, currencyId, item.Points))
            };
        }

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
                CardExpireDate = info.CardExpireDate.GetGMTDateFromUTC(timeZone),
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

        public static ClientPaymentInfo MapToClientPaymentInfo(this ApiClientPaymentInfo info, double timeZone)
        {
            return new ClientPaymentInfo
            {
                Id = info.Id,
                ClientId = info.ClientId,
                ClientFullName = info.CardholderName,
                CardNumber = info.CardNumber,
                CardExpireDate = info.CardExpireDate.GetUTCDateFromGMT(timeZone),
                BankName = info.BankName,
                BankIBAN = info.Iban,
                BankAccountNumber = info.BankAccountNumber,
                AccountNickName = info.NickName,
                Type = info.Type,
                State = info.State,
                WalletNumber = info.WalletNumber,
                PartnerPaymentSystemId = info.PaymentSystem,
                CreationTime = info.CreationTime.GetUTCDateFromGMT(timeZone),
                LastUpdateTime = info.LastUpdateTime.GetUTCDateFromGMT(timeZone)
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
                CommunicationTypeValue = arg.CommunicationTypeValue,
                ClientId = arg.ClientId
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
                State = arg.State,
                ClientId = arg.ClientId
            };
        }

        public static ApifnAgentTransaction ToApifnAffiliateTransaction(this fnAffiliateTransaction transactions, double timeZone)
        {
            return new ApifnAgentTransaction
            {
                Id = transactions.Id,
                ExternalTransactionId = transactions.ExternalTransactionId,
                Amount = transactions.Amount,
                CurrencyId = transactions.CurrencyId,
                State = transactions.State,
                OperationTypeId = transactions.OperationTypeId,
                ProductId = transactions.ProductId,
                ProductName = transactions.ProductName,
                TransactionType = transactions.TransactionType,
                CreationTime = transactions.CreationTime.GetGMTDateFromUTC(timeZone),
                LastUpdateTime = transactions.LastUpdateTime.GetGMTDateFromUTC(timeZone)
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
                Image = apiWebSiteMenuItem.Image,
                HoverImage = apiWebSiteMenuItem.HoverImage
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
                Image = apiWebSitSubeMenuItem.Image,
                HoverImage = apiWebSitSubeMenuItem.HoverImage
            };
        }

        #endregion

        #region Provider

        public static AffiliatePlatformModel MapToAffiliatePlatformModel(this AffiliatePlatform affiliatePlatform, double timeZone)
        {
            return new AffiliatePlatformModel
            {
                Id = affiliatePlatform.Id,
                PartnerId = affiliatePlatform.PartnerId,
                Name = affiliatePlatform.Name,
                Status = affiliatePlatform.Status,
                LastExecutionTime = affiliatePlatform.LastExecutionTime.GetGMTDateFromUTC(timeZone),
                KickOffTime = affiliatePlatform.KickOffTime.GetGMTDateFromUTC(timeZone),
                StepInHours = affiliatePlatform.StepInHours,
                PeriodInHours = affiliatePlatform.PeriodInHours
            };
        }

        public static NotificationServiceModel MapToNotificationServiceModel(this NotificationService notificationService, double timeZone)
        {
            return new NotificationServiceModel
            {
                Id = notificationService.Id,
                Name = notificationService.Name,
                CreationTime = notificationService.CreationTime.GetGMTDateFromUTC(timeZone)
            };
        }

        #endregion

        #region Agents

        public static ApiUser MapToUserModel(this fnAgent user, double timeZone, List<AgentCommission> commissions, int currentUserId, ILog log)
        {
            var commission = commissions.FirstOrDefault(x => x.AgentId == user.Id)?.TurnoverPercent;
            BllUserSetting parentSetting = null;
            BllUser parent = null;
            var parentState = user.State;
            if (user.ParentId.HasValue)
            {
                parent = CacheManager.GetUserById(user.ParentId.Value);
                parentSetting = CacheManager.GetUserSetting(user.ParentId.Value);
            }
            if (parentSetting != null && parentSetting.ParentState.HasValue && CustomHelper.Greater((UserStates)parentSetting.ParentState.Value, (UserStates)parent.State))
                parentState = parentSetting.ParentState.Value;
            else if (parent != null)
                parentState = parent.State;
            var resp = new ApiUser
            {
                Id = user.Id,
                NickName = user.NickName,
                CreationTime = user.CreationTime.GetGMTDateFromUTC(timeZone),
                CurrencyId = user.CurrencyId,
                FirstName = user.FirstName,
                Gender = user.Gender,
                LanguageId = user.LanguageId,
                LastName = user.LastName,
                UserName = user.UserName,
                Phone = !string.IsNullOrEmpty(user.Phone) ? user.Phone.Split('/')[0] : string.Empty,
                Fax = (!string.IsNullOrEmpty(user.Phone) && user.Phone.Split('/').Length > 1) ? user.Phone.Split('/')[1] : string.Empty,
                Type = user.Type,
                State = (user.ParentState.HasValue && CustomHelper.Greater((UserStates)user.ParentState.Value, (UserStates)user.State)) ? user.ParentState.Value : user.State,
                ParentState = parentState,
                Email = user.Email,
                MobileNumber = user.MobileNumber,
                ParentId = user.ParentId,
                ClientCount = user.ClientCount,
                DirectClientCount = user.DirectClientCount,
                Balance = user.Balance,
                Level = user.Level,
                LastLogin = user.LastLogin,
                LoginIp = user.LoginIp,
                AllowAutoPT = user.AllowAutoPT,
                AllowParentAutoPT = user.ParentId != currentUserId ? false : (parent != null && parentSetting != null && parent.Level != 0 ? parentSetting.AllowAutoPT : true),
                AllowOutright = user.AllowOutright,
                AllowParentOutright = user.ParentId != currentUserId ? false : (parent != null && parentSetting != null && parent.Level != 0 ? parentSetting.AllowOutright : true),
                AllowDoubleCommission = user.AllowDoubleCommission,
                AllowParentDoubleCommission = user.ParentId != currentUserId ? false : (parent != null && parentSetting != null && parent.Level != 0 ? parentSetting.AllowDoubleCommission : true),
                CalculationPeriod = string.IsNullOrEmpty(user.CalculationPeriod) ? new List<int>() : JsonConvert.DeserializeObject<List<int>>(user.CalculationPeriod),
                TotalBetAmount = user.TotalBetAmount,
                DirectBetAmount = user.DirectBetAmount,
                TotalWinAmount = user.TotalWinAmount,
                DirectWinAmount = user.DirectWinAmount,
                TotalGGR = user.TotalGGR,
                DirectGGR = user.DirectGGR,
                TotalTurnoverProfit = user.TotalTurnoverProfit,
                DirectTurnoverProfit = user.DirectTurnoverProfit,
                TotalGGRProfit = user.TotalGGRProfit,
                DirectGGRProfit = user.DirectGGRProfit,
                Path = user.Path
            };
            if (!string.IsNullOrEmpty(commission))
            {
                try
                {
                    var userSetting = CacheManager.GetUserSetting(user.Id);
                    var c = JsonConvert.DeserializeObject<AsianCommissionPlan>(commission);
                    resp.Commissions = c.Groups.Select(x => x.Value).ToList();
                    resp.PositionTakings = c.PositionTaking;
                    resp.CalculationPeriod = JsonConvert.DeserializeObject<List<int>>(userSetting.CalculationPeriod);
                }
                catch (Exception)
                {
                }
            }
            return resp;
        }

        public static ApifnAgentTransaction ToApifnAgentTransaction(this fnAgentTransaction transactions, double timeZone)
        {
            return new ApifnAgentTransaction
            {
                Id = transactions.Id,
                ExternalTransactionId = transactions.ExternalTransactionId,
                Amount = transactions.Amount,
                CurrencyId = transactions.CurrencyId,
                State = transactions.State,
                OperationTypeId = transactions.OperationTypeId,
                ProductId = transactions.ProductId,
                ProductName = transactions.ProductName,
                TransactionType = transactions.TransactionType.ToString(),
                FromUserId = transactions.FromUserId,
                UserId = transactions.UserId,
                UserName = transactions.UserName,
                FirstName = transactions.FirstName,
                LastName = transactions.LastName,
                CreationTime = transactions.CreationTime.GetGMTDateFromUTC(timeZone),
                LastUpdateTime = transactions.LastUpdateTime.GetGMTDateFromUTC(timeZone)
            };
        }

        public static ApiAgentReportItem ToApiAgentReportItem(this AgentReportItem input, double timeZone)
        {
            return new ApiAgentReportItem
            {
                AgentId = input.AgentId,
                AgentFirstName = input.AgentFirstName,
                AgentLastName = input.AgentLastName,
                AgentUserName = input.AgentUserName,
                TotalDepositCount = input.TotalDepositCount,
                TotalWithdrawCount = input.TotalWithdrawCount,
                TotalDepositAmount = input.TotalDepositAmount,
                TotalWithdrawAmount = input.TotalWithdrawAmount,
                TotalBetsCount = input.TotalBetsCount,
                TotalUnsettledBetsCount = input.TotalUnsettledBetsCount,
                TotalDeletedBetsCount = input.TotalDeletedBetsCount,
                TotalBetAmount = input.TotalBetAmount,
                TotalWinAmount = input.TotalWinAmount,
                TotalProfit = input.TotalProfit,
                TotalProfitPercent = input.TotalProfitPercent,
                TotalGGRCommission = input.TotalGGRCommission,
                TotalTurnoverCommission = input.TotalTurnoverCommission
            };
        }

        #endregion

        #region Characters

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
                BackgroundImageData = input.BackgroundImageData,
                MobileBackgroundImageData = input.MobileBackgroundImageData,
                ItemBackgroundImageData = input.ItemBackgroundImageData
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
                BackgroundImageData = input.BackgroundImageUrl,
                MobileBackgroundImageData = input.BackgroundImageUrl?.Replace("/assets/images/characters/background/", "/assets/images/characters/background/mobile/"),
                ItemBackgroundImageData = input.BackgroundImageUrl?.Replace("/assets/images/characters/background/", "/assets/images/characters/background/item/")
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
                MobileBackgroundImageData = character.BackgroundImageUrl?.Replace("/assets/images/characters/background/", "/assets/images/characters/background/mobile/"),
                ItemBackgroundImageData = character.BackgroundImageUrl?.Replace("/assets/images/characters/background/", "/assets/images/characters/background/item/"),
                CompPoints = character.CompPoints
            };
        }

        #endregion

        #region Announcements

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

        #endregion

        public static CRMSetting ToCRMSetting(this ApiCRMSetting apiCRMSetting, double timeZone)
        {
            return new CRMSetting
            {
                Id = apiCRMSetting.Id,
                PartnerId = apiCRMSetting.PartnerId,
                NickeName = apiCRMSetting.NickeName,
                State = apiCRMSetting.State,
                Type = apiCRMSetting.Type,
                Condition = apiCRMSetting.Condition,
                StartTime = apiCRMSetting.StartTime.GetUTCDateFromGMT(timeZone),
                FinishTime = apiCRMSetting.FinishTime.GetUTCDateFromGMT(timeZone),
                Sequence = apiCRMSetting.Sequence
            };
        }

        public static ApiCRMSetting ToApiCRMSetting(this CRMSetting setting, double timeZone)
        {
            return new ApiCRMSetting
            {
                Id = setting.Id,
                PartnerId = setting.PartnerId,
                NickeName = setting.NickeName,
                State = setting.State,
                Type = setting.Type,
                Condition = setting.Condition,
                StartTime = setting.StartTime.GetGMTDateFromUTC(timeZone),
                FinishTime = setting.FinishTime.GetGMTDateFromUTC(timeZone),
                Sequence = setting.Sequence
            };
        }

        public static DAL.Banner MapToBanner(this ApiBanner input, double timeZone)
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
                StartDate = input.StartDate.GetUTCDateFromGMT(timeZone),
                EndDate = input.EndDate.GetUTCDateFromGMT(timeZone),
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

        public static Promotion MapToPromotion(this ApiPromotion input, double timeZone)
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
                StartDate = input.StartDate.GetUTCDateFromGMT(timeZone),
                FinishDate = input.FinishDate.GetUTCDateFromGMT(timeZone),
                PromotionSegmentSettings = (input.Segments == null || input.Segments.Ids == null) ? new List<PromotionSegmentSetting>() :
                                          input.Segments?.Ids?.Select(x => new PromotionSegmentSetting
                                          {
                                              PromotionId = input.Id,
                                              SegmentId = x,
                                              Type = input.Segments.Type ?? (int)BonusSettingConditionTypes.InSet
                                          }).ToList(),
                PromotionLanguageSettings = (input.Languages == null || input.Languages.Names == null) ? new List<PromotionLanguageSetting>() :
                                           input.Languages?.Names?.Select(x => new PromotionLanguageSetting
                                           {
                                               PromotionId = input.Id,
                                               LanguageId = x,
                                               Type = input.Languages.Type ?? (int)BonusSettingConditionTypes.InSet
                                           }).ToList(),
                Order = input.Order,
                ParentId = input.ParentId,
                StyleType = input.StyleType,
                DeviceType = input.DeviceType,
                Visibility = input.Visibility == null ? null : JsonConvert.SerializeObject(input.Visibility),
            };
        }

        public static News ToNews(this ApiNews input, double timeZone)
        {
            return new News
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
                StartDate = input.StartDate.GetUTCDateFromGMT(timeZone),
                FinishDate = input.FinishDate.GetUTCDateFromGMT(timeZone),
                NewsSegmentSettings = (input.Segments == null || input.Segments.Ids == null) ? new List<NewsSegmentSetting>() :
                                          input.Segments?.Ids?.Select(x => new NewsSegmentSetting
                                          {
                                              NewsId = input.Id,
                                              SegmentId = x,
                                              Type = input.Segments.Type ?? (int)BonusSettingConditionTypes.InSet
                                          }).ToList(),
                NewsLanguageSettings = (input.Languages == null || input.Languages.Names == null) ? new List<NewsLanguageSetting>() :
                                           input.Languages?.Names?.Select(x => new NewsLanguageSetting
                                           {
                                               NewsId = input.Id,
                                               LanguageId = x,
                                               Type = input.Languages.Type ?? (int)BonusSettingConditionTypes.InSet
                                           }).ToList(),
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
                ParentId = input.ParentId,
                DeviceType = input.DeviceType,
                Visibility = string.IsNullOrEmpty(input.Visibility) ? new List<int>() : JsonConvert.DeserializeObject<List<int>>(input.Visibility),
            };
        }

        public static ApiNews ToApiNews(this News input, double timeZone)
        {
            return new ApiNews
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
                Segments = input.NewsSegmentSettings != null && input.NewsSegmentSettings.Any() ? new ApiSetting
                {
                    Type = input.NewsSegmentSettings.First().Type,
                    Ids = input.NewsSegmentSettings.Select(x => x.SegmentId).ToList()
                } : new ApiSetting { Type = (int)BonusSettingConditionTypes.InSet, Ids = new List<int>() },
                Languages = input.NewsLanguageSettings != null && input.NewsLanguageSettings.Any() ? new ApiSetting
                {
                    Type = input.NewsLanguageSettings.First().Type,
                    Names = input.NewsLanguageSettings.Select(x => x.LanguageId).ToList()
                } : new ApiSetting { Type = (int)BonusSettingConditionTypes.InSet, Names = new List<string>() },
                StyleType = input.StyleType,
                ParentId = input.ParentId
            };
        }

        public static ApiPopup MapToApiPopup(this Popup popup, double timeZone)
        {
            return new ApiPopup
            {
                Id = popup.Id,
                PartnerId = popup.PartnerId,
                NickName = popup.NickName,
                Type = popup.Type,
                State = popup.State,
                Order = popup.Order,
                Page = popup.Page,
                DeviceType = popup.DeviceType,
                StartDate = popup.StartDate.GetGMTDateFromUTC(timeZone),
                FinishDate = popup.FinishDate.GetGMTDateFromUTC(timeZone),
                CreationTime = popup.CreationTime.GetGMTDateFromUTC(timeZone),
                LastUpdateTime = popup.LastUpdateTime.GetGMTDateFromUTC(timeZone),
                TranslationId = popup.ContentTranslationId,
                ImageName = popup.ImageName,
                SegmentIds = popup.PopupSettings.Where(x => x.ObjectTypeId == (int)ObjectTypes.Segment).Select(x => x.ObjectId).ToList(),
                ClientIds = popup.PopupSettings.Where(x => x.ObjectTypeId == (int)ObjectTypes.Client).Select(x => x.ObjectId).ToList()
            };
        }

        public static ApiNotification ToApiNotification(this UserNotification input, double timeZone)
        {
            return new ApiNotification
            {
                Id = input.Id,
                UserId = input.UserId,
                TypeId = input.TypeId,
                ClientId = input.ClientId,
                PaymentRequestId = input.PaymentRequestId,
                BonusId = input.BonusId,
                Status = input.Status,
                CreationTime = input.CreationTime.GetGMTDateFromUTC(timeZone)
            };
        }
    }
}