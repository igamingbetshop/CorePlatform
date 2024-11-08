﻿using System;
using System.Collections.Generic;
using System.Linq;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Filters;
using IqSoft.CP.DAL.Filters.PaymentRequests;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Report;
using IqSoft.CP.Common.Helpers;
using Newtonsoft.Json;
using IqSoft.CP.AgentWebApi.Models;
using IqSoft.CP.AgentWebApi.Filters;
using IqSoft.CP.AgentWebApi.Models.User;
using IqSoft.CP.AgentWebApi.Models.Payment;
using IqSoft.CP.AgentWebApi.Models.ClientModels;
using IqSoft.CP.AgentWebApi.ClientModels;
using IqSoft.CP.AgentWebApi.Models.CommonModels;
using IqSoft.CP.DAL.Filters.Agent;
using IqSoft.CP.DAL.Filters.Bets;
using IqSoft.CP.DAL.Filters.Reporting;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.BLL.Caching;
using static IqSoft.CP.Common.Constants;
using IqSoft.CP.Common.Models.AgentModels;
using log4net;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.AgentWebApi.Filter;
using IqSoft.CP.DAL.Filters.Messages;
using IqSoft.CP.AgentWebApi.Filters.Messages;
using IqSoft.CP.Common;


namespace IqSoft.CP.AgentWebApi.Helpers
{

    public static class CustomMapper
    {
        #region Agent

        public static UserModel MapToUserModel(this fnAgent user, double timeZone, List<AgentCommission> commissions, int currentUserId, ILog log)
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
            var resp = new UserModel
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
                DirectGGRProfit = user.DirectGGRProfit
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

        public static UserModel MapToUserModel(this User user, double timeZone)
        {
            return new UserModel
            {
                Id = user.Id,
                CreationTime = user.CreationTime.GetGMTDateFromUTC(timeZone),
                CurrencyId = user.CurrencyId,
                FirstName = user.FirstName,
                Gender = user.Gender,
                LastName = user.LastName,
                UserName = user.UserName,
                NickName = user.NickName,
                MobileNumber = user.MobileNumber,
                Phone = !string.IsNullOrEmpty(user.Phone) ? user.Phone.Split('/')[0] : string.Empty,
                Fax = (!string.IsNullOrEmpty(user.Phone) && user.Phone.Split('/').Length > 1) ? user.Phone.Split('/')[1] : string.Empty,
                State = user.State,
                Type = user.Type,
                Email = user.Email,
                ParentId = user.ParentId,
                LanguageId = user.LanguageId,
                Level = user.Level
            };
        }

        public static User MapToUser(this UserModel user)
        {
            return new User
            {
                Id = user.Id,
                PartnerId = user.PartnerId,
                Password = user.Password,
                CurrencyId = user.CurrencyId,
                FirstName = user.FirstName,
                Gender = user.Gender ?? (int)Gender.Male,
                LanguageId = user.LanguageId,
                LastName = user.LastName,
                State = user.State,
                Type = user.Type,
                UserName = user.UserName,
                NickName = user.NickName,
                MobileNumber = user.MobileNumber,
                Phone = string.Format("{0}/{1}", user.Phone, user.Fax),
                Email = user.Email,
                ParentId = user.ParentId,
                Level = user.Level
            };
        }

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
                AgentId = agentCommission.AgentId,
                ProductId = agentCommission.ProductId,
                Percent = agentCommission.Percent,
                TurnoverPercent = isNumber ? agentCommission.TurnoverPercent :
                ((turnoverPercents == null || !turnoverPercents.Any()) ? string.Empty : String.Join(",", turnoverPercents.Select(x => string.Format("{0}-{1}|{2}", x.FromCount, x.ToCount, x.Percent)))),
                ClientId = agentCommission.ClientId
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
                JsonConvert.SerializeObject(agentCommission.TurnoverPercentsList) : agentCommission.TurnoverPercent,
                ClientId = agentCommission.ClientId
            };
        }
        public static ApiUserCorrections MapToApiUserCorrections(this PagedModel<fnUserCorrection> input, double timeZone)
        {
            return new ApiUserCorrections
            {
                Count = input.Count,
                Entities = input.Entities.Select(x => x.MapToApiUserCorrection(timeZone)).ToList()
            };
        }

        public static ApiUserCorrection MapToApiUserCorrection(this fnUserCorrection input, double timeZone)
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
                HasNote = input.HasNote ?? false
            };
        }

        #endregion

        #region Client
        public static Client MapToClient(this NewClientModel input)
        {
            var agentCategories = new List<int> { (int)ClientCategories.A, (int)ClientCategories.B, (int)ClientCategories.C, (int)ClientCategories.D };
            return new Client
            {
                UserName = input.UserName,
                Email = string.IsNullOrWhiteSpace(input.Email) ? string.Empty : input.Email,
                IsMobileNumberVerified = false,
                IsEmailVerified = false,
                Password = input.Password,
                FirstName = input.FirstName,
                LastName = input.LastName,
                DocumentNumber = input.DocumentNumber,
                DocumentIssuedBy = input.DocumentIssuedBy,
                Address = input.Address,
                MobileNumber = string.IsNullOrWhiteSpace(input.MobileNumber) ? string.Empty : (input.MobileNumber.StartsWith("+") ? input.MobileNumber : "+" + input.MobileNumber),
                PhoneNumber =string.Format("{0}/{1}", input.Phone, input.Fax),
                LanguageId = input.LanguageId,
                SendMail = input.SendMail,
                SendSms = input.SendSms,
                Gender = input.Gender ?? (int)Gender.Male,
                RegionId = input.RegionId,
                BirthDate = input.BirthDate,
                State = (input.Closed.HasValue && input.Closed.Value) ? (int)ClientStates.FullBlocked : (int)ClientStates.Active,
                CategoryId = agentCategories.Contains(input.Group) ? input.Group : (int)ClientCategories.A
            };
        }

        public static Common.Models.WebSiteModels.ChangeClientFieldsInput MapToClientFields(this NewClientModel input)
        {
            return new Common.Models.WebSiteModels.ChangeClientFieldsInput
            {
                ClientId = input.Id ?? 0,
                Email = string.IsNullOrWhiteSpace(input.Email) ? string.Empty : input.Email,
                FirstName = input.FirstName,
                LastName = input.LastName,
                Address = input.Address,
                MobileNumber = string.IsNullOrWhiteSpace(input.MobileNumber) ? string.Empty : (input.MobileNumber.StartsWith("+") ? input.MobileNumber : "+" + input.MobileNumber),
                PhoneNumber = string.Format("{0}/{1}", input.Phone, input.Fax),
                CategoryId = input.Group
            };
        }

        public static FilterfnClient MapToFilterfnClient(this ApiFilterfnClient filterClient)
        {
            return new FilterfnClient
            {
                PartnerId = filterClient.PartnerId,
                AgentId = filterClient.AgentId,
                CreatedFrom = filterClient.CreatedFrom,
                CreatedBefore = filterClient.CreatedBefore,
                Ids = filterClient.Ids == null ? new FiltersOperation() : filterClient.Ids.MapToFiltersOperation(),
                Emails = filterClient.Emails == null ? new FiltersOperation() : filterClient.Emails.MapToFiltersOperation(),
                UserNames = filterClient.UserNames == null ? new FiltersOperation() : filterClient.UserNames.MapToFiltersOperation(),
                Currencies = filterClient.CurrencyIds == null ? new FiltersOperation() : filterClient.CurrencyIds.MapToFiltersOperation(),
                Genders = filterClient.Genders == null ? new FiltersOperation() : filterClient.Genders.MapToFiltersOperation(),
                FirstNames = filterClient.FirstNames == null ? new FiltersOperation() : filterClient.FirstNames.MapToFiltersOperation(),
                LastNames = filterClient.LastNames == null ? new FiltersOperation() : filterClient.LastNames.MapToFiltersOperation(),
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
                Balances = filterClient.Balances == null ? new FiltersOperation() : filterClient.Balances.MapToFiltersOperation(),
                GGRs = filterClient.GGRs == null ? new FiltersOperation() : filterClient.GGRs.MapToFiltersOperation(),
                NETGamings = filterClient.NETGamings == null ? new FiltersOperation() : filterClient.NETGamings.MapToFiltersOperation(),
                AffiliatePlatformIds = filterClient.AffiliatePlatformIds == null ? new FiltersOperation() : filterClient.AffiliatePlatformIds.MapToFiltersOperation(),
                AffiliateIds = filterClient.AffiliateIds == null ? new FiltersOperation() : filterClient.AffiliateIds.MapToFiltersOperation(),
                UserIds = filterClient.UserIds == null ? new FiltersOperation() : filterClient.UserIds.MapToFiltersOperation(),
                SkipCount = filterClient.SkipCount,
                TakeCount = filterClient.TakeCount,
                OrderBy = filterClient.OrderBy,
                FieldNameToOrderBy = filterClient.FieldNameToOrderBy
            };
        }
        public static FilterClientModel MapToFilterClientModel(this ApiFilterfnClient filterClient)
        {
            return new FilterClientModel
            {
                PartnerId = filterClient.PartnerId,
                AgentId = filterClient.AgentId,
                CreatedFrom = filterClient.CreatedFrom,
                CreatedBefore = filterClient.CreatedBefore,
                Ids = filterClient.Ids == null ? new FiltersOperation() : filterClient.Ids.MapToFiltersOperation(),
                Emails = filterClient.Emails == null ? new FiltersOperation() : filterClient.Emails.MapToFiltersOperation(),
                UserNames = filterClient.UserNames == null ? new FiltersOperation() : filterClient.UserNames.MapToFiltersOperation(),
                Currencies = filterClient.CurrencyIds == null ? new FiltersOperation() : filterClient.CurrencyIds.MapToFiltersOperation(),
                Genders = filterClient.Genders == null ? new FiltersOperation() : filterClient.Genders.MapToFiltersOperation(),
                FirstNames = filterClient.FirstNames == null ? new FiltersOperation() : filterClient.FirstNames.MapToFiltersOperation(),
                LastNames = filterClient.LastNames == null ? new FiltersOperation() : filterClient.LastNames.MapToFiltersOperation(),
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
                Balances = filterClient.Balances == null ? new FiltersOperation() : filterClient.Balances.MapToFiltersOperation(),
                GGRs = filterClient.GGRs == null ? new FiltersOperation() : filterClient.GGRs.MapToFiltersOperation(),
                NETGamings = filterClient.NETGamings == null ? new FiltersOperation() : filterClient.NETGamings.MapToFiltersOperation(),
                AffiliatePlatformIds = filterClient.AffiliatePlatformIds == null ? new FiltersOperation() : filterClient.AffiliatePlatformIds.MapToFiltersOperation(),
                AffiliateIds = filterClient.AffiliateIds == null ? new FiltersOperation() : filterClient.AffiliateIds.MapToFiltersOperation(),
                UserIds = filterClient.UserIds == null ? new FiltersOperation() : filterClient.UserIds.MapToFiltersOperation(),
                SkipCount = filterClient.SkipCount,
                TakeCount = filterClient.TakeCount,
                OrderBy = filterClient.OrderBy,
                FieldNameToOrderBy = filterClient.FieldNameToOrderBy
            };
        }


        public static fnClientModel MapTofnClientModelItem(this Client arg, double timeZone, List<AgentCommission> agents, int callerLevel, int callerId, ILog log)
        {
            var commissions1 = new List<MemberCommission>();
            var commissions2 = new List<MemberCommission>();
            var commissions3 = new List<MemberCommission>();
            var adc = CacheManager.GetClientSettingByName(arg.Id, ClientSettings.AllowDoubleCommission);
            var ao = CacheManager.GetClientSettingByName(arg.Id, ClientSettings.AllowOutright);
            var ss = CacheManager.GetClientSettingByName(arg.Id, ClientSettings.ParentState);
            var state = arg.State;
            if (ss.NumericValue.HasValue && CustomHelper.Greater((ClientStates)ss.NumericValue, (ClientStates)state))
                state = Convert.ToInt32(ss.NumericValue.Value);
            var parentSetting = CacheManager.GetUserSetting(arg.UserId.Value);
            state = CustomHelper.MapUserStateToClient.First(x => x.Value == state).Key;
            var balance = CacheManager.GetClientCurrentBalance(arg.Id).AvailableBalance;
            var resp = new fnClientModel
            {
                Id = arg.Id,
                Level = (int)AgentLevels.Member,
                Email = arg.Email,
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
                RegionId = arg.RegionId,
                RegistrationIp = arg.RegistrationIp,
                DocumentNumber = arg.DocumentNumber,
                DocumentType = arg.DocumentType,
                DocumentIssuedBy = arg.DocumentIssuedBy,
                IsDocumentVerified = arg.IsDocumentVerified,
                Address = arg.Address,
                MobileNumber = arg.MobileNumber,
                Phone = !string.IsNullOrEmpty(arg.PhoneNumber) ? arg.PhoneNumber.Split('/')[0] : string.Empty,
                Fax = (!string.IsNullOrEmpty(arg.PhoneNumber) && arg.PhoneNumber.Split('/').Length > 1) ? arg.PhoneNumber.Split('/')[1] : string.Empty,
                IsMobileNumberVerified = arg.IsMobileNumberVerified,
                LanguageId = arg.LanguageId,
                CreationTime = arg.CreationTime.GetGMTDateFromUTC(timeZone),
                LastUpdateTime = arg.LastUpdateTime,
                Group = arg.CategoryId,
                State = state,
                Closed = state == (int)UserStates.Closed,
                CallToPhone = arg.CallToPhone,
                SendPromotions = arg.SendPromotions,
                ZipCode = arg.ZipCode,
                HasNote = arg.HasNote,
                Balance = balance,
                Info = arg.Info,
                UserId = arg.UserId,
                AllowDoubleCommission = Convert.ToBoolean(adc == null || adc.Id == 0 ? 0 : (adc.NumericValue ?? 0)),
                AllowParentDoubleCommission = callerId != arg.UserId || parentSetting  == null ? false : parentSetting.AllowDoubleCommission,
                AllowOutright = Convert.ToBoolean(ao == null || ao.Id == 0 ? 0 : (ao.NumericValue ?? 0)),
                AllowParentOutright = callerId != arg.UserId || parentSetting == null ? false : parentSetting.AllowOutright,
                NickName = arg.NickName,
                Commissions1 = new List<MemberCommission>(),
                Commissions2 = new List<MemberCommission>(),
                Commissions3 = new List<MemberCommission>()
            };
            for (int i = 0; i < arg.ParentsPath.Count; i++)
            {
                var parent = agents.FirstOrDefault(x => x.AgentId == arg.ParentsPath[i]);
                if (parent != null && !string.IsNullOrEmpty(parent.TurnoverPercent) && !Decimal.TryParse(parent.TurnoverPercent, out decimal uVal))
                {
                    var comm = JsonConvert.DeserializeObject<AsianCommissionPlan>(parent.TurnoverPercent).Groups;
                    commissions1.Add(new MemberCommission { Level = parent.Agent.Level.Value, Commission = comm[0].Value });
                    commissions2.Add(new MemberCommission { Level = parent.Agent.Level.Value, Commission = comm[1].Value });
                    commissions3.Add(new MemberCommission { Level = parent.Agent.Level.Value, Commission = comm[2].Value });
                }
            }
            var commInfo = arg.AgentCommissions.FirstOrDefault();
            if (commInfo != null && !string.IsNullOrEmpty(commInfo?.TurnoverPercent) && !Decimal.TryParse(commInfo?.TurnoverPercent, out decimal cVal))
            {
                var mComm = JsonConvert.DeserializeObject<AsianCommissionPlan>(commInfo.TurnoverPercent);
                if (mComm != null && mComm.Groups != null)
                {
                    commissions1.Add(new MemberCommission { Level = 7, Commission = mComm.Groups[0].Value });
                    commissions2.Add(new MemberCommission { Level = 7, Commission = mComm.Groups[1].Value });
                    commissions3.Add(new MemberCommission { Level = 7, Commission = mComm.Groups[2].Value });
                }
            }
            for (int i = 0; i < commissions1.Count - 1; i++)
            {
                commissions1[i].CommissionLeft = commissions1[i].Commission - commissions1[i + 1].Commission;
                commissions2[i].CommissionLeft = commissions2[i].Commission - commissions2[i + 1].Commission;
                commissions3[i].CommissionLeft = commissions3[i].Commission - commissions3[i + 1].Commission;
            }

            for (int i = callerLevel; i < 8; i++)
            {
                if (!commissions1.Any(x => x.Level == i))
                {
                    commissions1.Add(new MemberCommission { Level = i });
                    commissions2.Add(new MemberCommission { Level = i });
                    commissions3.Add(new MemberCommission { Level = i });
                }
            }
            resp.Commissions1 = commissions1.OrderBy(x => x.Level).ToList();
            resp.Commissions2 = commissions2.OrderBy(x => x.Level).ToList();
            resp.Commissions3 = commissions3.OrderBy(x => x.Level).ToList();

            return resp;
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
                BirthDate = client.BirthDate.GetGMTDateFromUTC(timeZone),
                SendMail = client.SendMail,
                SendSms = client.SendSms,
                FirstName = client.FirstName,
                LastName = client.LastName,
                RegionId = client.RegionId,
                RegistrationIp = client.RegistrationIp,
                DocumentNumber = client.DocumentNumber,
                DocumentType = client.DocumentType,
                DocumentIssuedBy = client.DocumentIssuedBy,
                IsDocumentVerified = client.IsDocumentVerified,
                Address = client.Address,
                MobileNumber = client.MobileNumber,
                Phone = !string.IsNullOrEmpty(client.PhoneNumber) ? client.PhoneNumber.Split('/')[0] : string.Empty,
                Fax = (!string.IsNullOrEmpty(client.PhoneNumber) && client.PhoneNumber.Split('/').Length > 1) ? client.PhoneNumber.Split('/')[1] : string.Empty,
                IsMobileNumberVerified = client.IsMobileNumberVerified,
                LanguageId = client.LanguageId,
                CreationTime = client.CreationTime,
                LastUpdateTime = client.LastUpdateTime,
                Group = client.CategoryId,
                State = CustomHelper.MapUserStateToClient.First(x => x.Value == client.State).Key,
                CallToPhone = client.CallToPhone,
                SendPromotions = client.SendPromotions,
                ZipCode = client.ZipCode,
                HasNote = client.HasNote,
                Info = client.Info,
                Level = (int)AgentLevels.Member,
                //Balance = 0,
                //GGR = 0,
                //NETGaming = 0
            };
        }

        public static ClientInfoModel MapToClientInfoModel(this DAL.Models.Clients.ClientInfo client)
        {
            var adc = CacheManager.GetClientSettingByName(client.Id, ClientSettings.AllowDoubleCommission);
            var ao = CacheManager.GetClientSettingByName(client.Id, ClientSettings.AllowOutright);
            return new ClientInfoModel
            {
                Id = client.Id,
                UserName = client.UserName,
                CategoryId = client.CategoryId,
                CurrencyId = client.CurrencyId,
                FirstName = client.FirstName,
                LastName = client.LastName,
                Email = client.Email,
                RegistrationDate = client.RegistrationDate,
                Status = client.Status,
                Balance = client.Balance,
                WithdrawableBalance = client.WithdrawableBalance,
                TotalBetsAmount = client.TotalBetsAmount,
                LastMonthBetsAmount = client.LastMonthBetsAmount,
                LastWeekBetsAmount = client.LastWeekBetsAmount,
                TodayBetsAmount = client.TodayBetsAmount,
                GGR = client.GGR,
                NGR = client.NGR,
                TotalDepositsCount = client.TotalDepositsCount,
                TotalDepositsAmount = client.TotalDepositsAmount,
                LastMonthDepositsAmount = client.LastMonthDepositsAmount,
                LastWeekDepositsAmount = client.LastWeekDepositsAmount,
                TodayDepositsAmount = client.TodayDepositsAmount,
                TotalWithdrawalsCount = client.TotalWithdrawalsCount,
                TotalWithdrawalsAmount = client.TotalWithdrawalsAmount,
                LastMonthWithdrawalsAmount = client.LastMonthWithdrawalsAmount,
                LastWeekWithdrawalsAmount = client.LastWeekWithdrawalsAmount,
                TodayWithdrawalsAmount = client.TodayWithdrawalsAmount,
                FailedDepositsCount = client.FailedDepositsCount,
                FailedDepositsAmount = client.FailedDepositsAmount,
                Risk = client.Risk,
                IsOnline = client.IsOnline,
                AllowDoubleCommission = Convert.ToBoolean(adc == null || adc.Id == 0 ? 0 : (adc.NumericValue ?? 0)),
                AllowOutright = Convert.ToBoolean(ao == null || ao.Id == 0 ? 0 : (ao.NumericValue ?? 0))
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
                UserId = input.Creator,
                CreationTime = input.CreationTime.GetGMTDateFromUTC(timeZone),
                LastUpdateTime = input.LastUpdateTime.GetGMTDateFromUTC(timeZone),
                OperationTypeName = input.OperationTypeName,
                FirstName = input.FirstName,
                LastName = input.LastName,
                AccoutTypeId = input.AccountTypeId,
                HasNote = input.HasNote ?? false
            };
        }
        public static FilterCorrection MapToFilterCorrection(this ApiFilterClientCorrection filter)
        {
            return new FilterCorrection
            {
                ClientId = filter.ClientId,
                TakeCount = filter.TakeCount,
                SkipCount = filter.SkipCount,
                FromDate = filter.FromDate,
                ToDate = filter.ToDate,
                OrderBy = filter.OrderBy,
                FieldNameToOrderBy = filter.FieldNameToOrderBy,

                Ids = filter.Ids == null ? new FiltersOperation() : filter.Ids.MapToFiltersOperation(),
                ClientIds = filter.ClientIds == null ? new FiltersOperation() : filter.ClientIds.MapToFiltersOperation(),
                Amounts = filter.Amounts == null ? new FiltersOperation() : filter.Amounts.MapToFiltersOperation(),
                States = filter.States == null ? new FiltersOperation() : filter.States.MapToFiltersOperation(),
                CurrencyIds = filter.CurrencyIds == null ? new FiltersOperation() : filter.CurrencyIds.MapToFiltersOperation(),
                Creators = filter.UserIds == null ? new FiltersOperation() : filter.UserIds.MapToFiltersOperation(),
                OperationTypeNames = filter.OperationTypeNames == null ? new FiltersOperation() : filter.OperationTypeNames.MapToFiltersOperation(),
                FirstNames = filter.FirstNames == null ? new FiltersOperation() : filter.FirstNames.MapToFiltersOperation(),
                LastNames = filter.LastNames == null ? new FiltersOperation() : filter.LastNames.MapToFiltersOperation()

            };
        }
        public static FnAccountModel MapToFnAccountModel(this fnAccount account)
        {
            return new FnAccountModel
            {
                Id = account.Id,
                ObjectTypeId = account.ObjectTypeId,
                TypeId = account.TypeId,
                Balance = account.Balance,
                CurrencyId = account.CurrencyId,
                AccountTypeName = account.AccountTypeName
            };
        }

        public static List<FnAccountModel> MapToFnAccountModels(this IEnumerable<fnAccount> accounts)
        {
            return accounts.Select(MapToFnAccountModel).ToList();
        }
        #endregion

        #region FilterPaymentRequest

        public static List<FilterfnPaymentRequest> MapToFilterPaymentRequests(this IEnumerable<ApiFilterfnPaymentRequest> requests)
        {
            return requests.Select(MapToFilterfnPaymentRequest).ToList();
        }

        public static FilterfnPaymentRequest MapToFilterfnPaymentRequest(this ApiFilterfnPaymentRequest request)
        {
            return new FilterfnPaymentRequest
            {
                PartnerId = request.PartnerId,
                FromDate = (request.FromDate == null ? 0 : (long)request.FromDate.Value.Year * 1000000 + (long)request.FromDate.Value.Month * 10000 + (long)request.FromDate.Value.Day * 100 + (long)request.FromDate.Value.Hour),
                ToDate = (request.ToDate == null ? 0 : (long)request.ToDate.Value.Year * 1000000 + (long)request.ToDate.Value.Month * 10000 + (long)request.ToDate.Value.Day * 100 + (long)request.ToDate.Value.Hour),
                Type = request.Type,
                AgentId = request.AgentId,
                Ids = request.Ids == null ? new FiltersOperation() : request.Ids.MapToFiltersOperation(),
                UserNames = request.UserNames == null ? new FiltersOperation() : request.UserNames.MapToFiltersOperation(),
                Names = request.Names == null ? new FiltersOperation() : request.Names.MapToFiltersOperation(),
                CreatorNames = request.CreatorNames == null ? new FiltersOperation() : request.CreatorNames.MapToFiltersOperation(),
                ClientIds = request.ClientIds == null ? new FiltersOperation() : request.ClientIds.MapToFiltersOperation(),
                UserIds = request.UserIds == null ? new FiltersOperation() : request.UserIds.MapToFiltersOperation(),
                PartnerPaymentSettingIds = request.PartnerPaymentSettingIds == null ? new FiltersOperation() : request.PartnerPaymentSettingIds.MapToFiltersOperation(),
                PaymentSystemIds = request.PaymentSystemIds == null ? new FiltersOperation() : request.PaymentSystemIds.MapToFiltersOperation(),
                Currencies = request.Currencies == null ? new FiltersOperation() : request.Currencies.MapToFiltersOperation(),
                States = request.States == null ? new FiltersOperation() : request.States.MapToFiltersOperation(),
                BetShopIds = request.BetShopIds == null ? new FiltersOperation() : request.BetShopIds.MapToFiltersOperation(),
                BetShopNames = request.BetShopNames == null ? new FiltersOperation() : request.BetShopNames.MapToFiltersOperation(),
                Amounts = request.Amounts == null ? new FiltersOperation() : request.Amounts.MapToFiltersOperation(),
                CreationDates = request.CreationDates == null ? new FiltersOperation() : request.CreationDates.MapToFiltersOperation(),
                AffiliatePlatformIds = request.AffiliatePlatformIds == null ? new FiltersOperation() : request.AffiliatePlatformIds.MapToFiltersOperation(),
                AffiliateIds = request.AffiliateIds == null ? new FiltersOperation() : request.AffiliateIds.MapToFiltersOperation(),
                ActivatedBonusTypes = request.ActivatedBonusTypes == null ? new FiltersOperation() : request.ActivatedBonusTypes.MapToFiltersOperation(),
                LastUpdateDates = new FiltersOperation(),
                ExternalTransactionIds = request.ExternalIds == null ? new FiltersOperation() : request.ExternalIds.MapToFiltersOperation(),
                TakeCount = request.TakeCount,
                SkipCount = request.SkipCount,
                OrderBy = request.OrderBy,
                FieldNameToOrderBy = request.FieldNameToOrderBy
            };
        }

        #endregion

        #region Transactions
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

        public static FilterUserCorrection MapToFilterCorrection(this ApiFilterUserCorrection filter)
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
        #endregion

        #region Payment
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
        public static PaymentSystemModel MapToPaymentSystemModel(this PaymentSystem paymentSystem, double timeZone)
        {
            return new PaymentSystemModel
            {
                Id = paymentSystem.Id,
                Name = paymentSystem.Name,
                Type = paymentSystem.Type,
                PeriodicityOfRequest = paymentSystem.PeriodicityOfRequest,
                PaymentRequestSendCount = paymentSystem.PaymentRequestSendCount,
                CreationTime = paymentSystem.CreationTime.GetGMTDateFromUTC(timeZone),
                LastUpdateTime = paymentSystem.LastUpdateTime.GetGMTDateFromUTC(timeZone)
            };
        }
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

        public static ApiPaymentRequest MapToApiPaymentRequest(this fnPaymentRequest request, double timeZone)
        {
            var resp = new ApiPaymentRequest
            {
                Id = request.Id,
                PartnerId = request.PartnerId,
                ClientId = request.ClientId,
                Amount = request.Amount,
                CurrencyId = request.CurrencyId,
                Status = request.Status,
                Type = request.Type,
                BetShopId = request.BetShopId,
                Barcode = request.Barcode,
                BetShopName = request.BetShopName,
                BetShopAddress = request.BetShopAddress,
                PaymentSystemId = request.PaymentSystemId,
                PaymentSystemName = request.PaymentSystemName,
                Info = request.Info,
                CreationTime = request.CreationTime.GetGMTDateFromUTC(timeZone),
                LastUpdateTime = request.LastUpdateTime.GetGMTDateFromUTC(timeZone),
                CashDeskId = request.CashDeskId,
                UserName = request.UserName,
                FirstName = request.FirstName,
                LastName = request.LastName,
                ClientDocumentNumber = request.ClientDocumentNumber,
                ClientHasNote = request.ClientHasNote,
                GroupId = request.CategoryId,
                CreatorId = request.UserId,
                CreatorFirstName = request.UserFirstName,
                CreatorLastName = request.UserLastName,
                HasNote = request.HasNote == 1,
                CashCode = request.CashCode,
                ExternalId = request.ExternalTransactionId,
                Parameters = request.Parameters,
                AffiliatePlatformId = request.AffiliatePlatformId,
                AffiliateId = request.AffiliateId,
                ActivatedBonusType = request.ActivatedBonusType
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


        #region FiltersOperation

        public static FiltersOperation MapToFiltersOperation(this ApiFiltersOperation apiFiltersOperation)
        {
            return new FiltersOperation
            {
                IsAnd = apiFiltersOperation.IsAnd,
                OperationTypeList = apiFiltersOperation.ApiOperationTypeList.MapToFiltersOperationTypes()
            };
        }

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
                StringValue = apiFiltersOperationType.StringValue,
                IntValue = apiFiltersOperationType.IntValue,
                DecimalValue = apiFiltersOperationType.DecimalValue,
                DateTimeValue = apiFiltersOperationType.DateTimeValue
            };
        }

        #endregion

        #region Report
        public static FilterReportByActionLog MapToFilterReportByActionLog(this ApiFilterReportByActionLog filter)
        {
            return new FilterReportByActionLog
            {
                FromDate = filter.FromDate,
                ToDate = filter.ToDate,
                ActionGroupId = filter.ActionGroupId,
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

        public static InternetBetsReportModel MapToInternetBetsReportModel(this InternetBetsReport internetBetsReport, double timeZone)
        {
            return new InternetBetsReportModel
            {
                Count = internetBetsReport.Count,
                Entities = internetBetsReport.Entities.MapToInternetBetModels(timeZone)
            };
        }

        public static List<InternetBetModel> MapToInternetBetModels(this IEnumerable<fnInternetBet> fnInternetBets, double timeZone)
        {
            return fnInternetBets.Select(x => x.MapToInternetBetModel(timeZone)).ToList();
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
                TicketInfo = string.Empty,
                BetDate = fnInternetBet.BetDate.GetGMTDateFromUTC(timeZone),
                CalculationDate = fnInternetBet.WinDate.GetGMTDateFromUTC(timeZone),
                ClientId = fnInternetBet.ClientId,
                ClientUserName = fnInternetBet.ClientUserName,
                ClientFirstName = fnInternetBet.ClientFirstName,
                ClientLastName = fnInternetBet.ClientLastName,
                BetAmount = fnInternetBet.BetAmount,
                WinAmount = fnInternetBet.WinAmount,
                CurrencyId = fnInternetBet.CurrencyId,
                TicketNumber = fnInternetBet.TicketNumber,
                DeviceTypeId = fnInternetBet.DeviceTypeId,
                BetTypeId = fnInternetBet.BetTypeId,
                PossibleWin = fnInternetBet.PossibleWin,
                PartnerId = fnInternetBet.PartnerId,
                ProductName = fnInternetBet.ProductName,
                ProviderName = fnInternetBet.ProviderName,
                ClientIp = string.Empty,
                Country = string.Empty,
                ClientCategoryId = fnInternetBet.ClientCategoryId,
                HasNote = fnInternetBet.HasNote,
                RoundId = string.Empty,
                ClientHasNote = fnInternetBet.ClientHasNote,
                Profit = fnInternetBet.BetAmount - fnInternetBet.WinAmount,
                BonusId = fnInternetBet.BonusId
            };
        }
        public static FilterfnAgent MaptToFilterfnAgent(this ApiFilterfnAgent filterAgent)
        {
            return new FilterfnAgent
            {
                CreatedFrom = filterAgent.CreatedFrom,
                CreatedBefore = filterAgent.CreatedBefore,
                Ids = filterAgent.Ids == null ? new FiltersOperation() : filterAgent.Ids.MapToFiltersOperation(),
                FirstNames = filterAgent.FirstNames == null ? new FiltersOperation() : filterAgent.FirstNames.MapToFiltersOperation(),
                LastNames = filterAgent.LastNames == null ? new FiltersOperation() : filterAgent.LastNames.MapToFiltersOperation(),
                UserNames = filterAgent.UserNames == null ? new FiltersOperation() : filterAgent.UserNames.MapToFiltersOperation(),
                Emails = filterAgent.Emails == null ? new FiltersOperation() : filterAgent.Emails.MapToFiltersOperation(),
                Genders = filterAgent.Genders == null ? new FiltersOperation() : filterAgent.Genders.MapToFiltersOperation(),
                UserStates = filterAgent.UserStates == null ? new FiltersOperation() : filterAgent.UserStates.MapToFiltersOperation(),
                ClientCounts = filterAgent.ClientCounts == null ? new FiltersOperation() : filterAgent.ClientCounts.MapToFiltersOperation(),
                Balances = filterAgent.Balances == null ? new FiltersOperation() : filterAgent.Balances.MapToFiltersOperation(),
                SkipCount = filterAgent.SkipCount,
                TakeCount = filterAgent.TakeCount,
                OrderBy = filterAgent.OrderBy,
                FieldNameToOrderBy = filterAgent.FieldNameToOrderBy
            };
        }
        public static FilterfnAgentTransaction MapToFilterfnAgentTransaction(this ApiFilterfnAgentTransaction apiFilterfnAgentTransaction)
        {
            var currentDate = DateTime.UtcNow;
            return new FilterfnAgentTransaction
            {
                FromDate = apiFilterfnAgentTransaction.FromDate ?? 
                currentDate.AddDays((apiFilterfnAgentTransaction.IsYesterday.HasValue && apiFilterfnAgentTransaction.IsYesterday.Value) ? - 2 : -1) ,
                ToDate = apiFilterfnAgentTransaction.ToDate ?? currentDate.AddDays(1),
                UserState =apiFilterfnAgentTransaction.UserState,
                Ids = apiFilterfnAgentTransaction.Ids == null ? new FiltersOperation() : apiFilterfnAgentTransaction.Ids.MapToFiltersOperation(),
                FromUserIds = apiFilterfnAgentTransaction.FromUserIds == null ? new FiltersOperation() : apiFilterfnAgentTransaction.FromUserIds.MapToFiltersOperation(),
                UserIds = apiFilterfnAgentTransaction.UserIds == null ? new FiltersOperation() : apiFilterfnAgentTransaction.UserIds.MapToFiltersOperation(),
                ExternalTransactionIds = apiFilterfnAgentTransaction.ExternalTransactionIds == null ? new FiltersOperation() : apiFilterfnAgentTransaction.ExternalTransactionIds.MapToFiltersOperation(),
                Amounts = apiFilterfnAgentTransaction.Amounts == null ? new FiltersOperation() : apiFilterfnAgentTransaction.Amounts.MapToFiltersOperation(),
                CurrencyIds = apiFilterfnAgentTransaction.CurrencyIds == null ? new FiltersOperation() : apiFilterfnAgentTransaction.CurrencyIds.MapToFiltersOperation(),
                States = apiFilterfnAgentTransaction.States == null ? new FiltersOperation() : apiFilterfnAgentTransaction.States.MapToFiltersOperation(),
                OperationTypeIds = apiFilterfnAgentTransaction.OperationTypeIds == null ? new FiltersOperation() : apiFilterfnAgentTransaction.OperationTypeIds.MapToFiltersOperation(),
                ProductIds = apiFilterfnAgentTransaction.ProductIds == null ? new FiltersOperation() : apiFilterfnAgentTransaction.ProductIds.MapToFiltersOperation(),
                ProductNames = apiFilterfnAgentTransaction.ProductNames == null ? new FiltersOperation() : apiFilterfnAgentTransaction.ProductNames.MapToFiltersOperation(),
                TransactionTypes = apiFilterfnAgentTransaction.TransactionTypes == null ? new FiltersOperation() : apiFilterfnAgentTransaction.TransactionTypes.MapToFiltersOperation(),
                SkipCount = apiFilterfnAgentTransaction.SkipCount,
                TakeCount = Math.Min(apiFilterfnAgentTransaction.TakeCount, 5000),
                OrderBy = apiFilterfnAgentTransaction.OrderBy,
                FieldNameToOrderBy = apiFilterfnAgentTransaction.FieldNameToOrderBy
            };
        }

        public static ApifnAgentTransaction MapToApifnAgentTransaction(this fnAgentTransaction transactions, double timeZone)
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
                FromUserId = transactions.FromUserId,
                UserId = transactions.UserId,
                UserName = transactions.UserName,
                FirstName = transactions.FirstName,
                LastName = transactions.LastName,
                CreationTime = transactions.CreationTime.GetGMTDateFromUTC(timeZone),
                LastUpdateTime = transactions.LastUpdateTime.GetGMTDateFromUTC(timeZone)
            };
        }

        public static List<FilterInternetBet> MaptoFilterInternetBets(this IEnumerable<ApiFilterInternetBet> apiFilterInternetBets)
        {
            return apiFilterInternetBets.Select(MapToFilterInternetBet).ToList();
        }
        public static FilterfnProduct MapToFilterfnProduct(this ApiFilterfnProduct filter)
        {
            return new FilterfnProduct
            {
                ParentId = filter.ParentId,
                Ids = filter.Ids == null ? new FiltersOperation() : filter.Ids.MapToFiltersOperation(),
                Names = filter.Names == null ? new FiltersOperation() : filter.Names.MapToFiltersOperation(),
                Descriptions = filter.Descriptions == null ? new FiltersOperation() : filter.Descriptions.MapToFiltersOperation(),
                ExternalIds = filter.ExternalIds == null ? new FiltersOperation() : filter.ExternalIds.MapToFiltersOperation(),
                States = filter.States == null ? new FiltersOperation() : filter.States.MapToFiltersOperation(),
                GameProviderIds = filter.GameProviderIds == null ? new FiltersOperation() : filter.GameProviderIds.MapToFiltersOperation(),
                SkipCount = filter.SkipCount,
                TakeCount = filter.TakeCount,
                OrderBy = filter.OrderBy,
                FieldNameToOrderBy = filter.FieldNameToOrderBy
            };
        }

        public static FilterGameProvider MapToFilterGameProvider(this ApiFilterGameProvider filter)
        {
            return new FilterGameProvider
            {
                Id = filter.Id,
                Name = filter.Name
            };
        }
        public static FnProductModel MapTofnProductModel(this fnProduct product)
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
                BackgroundImageUrl = product.BackgroundImageUrl
            };
        }
        public static FilterInternetBet MapToFilterInternetBet(this ApiFilterInternetBet apiFilterInternetBet)
        {
            return new FilterInternetBet
            {
                PartnerId = apiFilterInternetBet.PartnerId,
                FromDate = apiFilterInternetBet.BetDateFrom,
                ToDate = apiFilterInternetBet.BetDateBefore,
                Ids = apiFilterInternetBet.Ids == null ? new FiltersOperation() : apiFilterInternetBet.Ids.MapToFiltersOperation(),
                ClientIds = apiFilterInternetBet.ClientIds == null ? new FiltersOperation() : apiFilterInternetBet.ClientIds.MapToFiltersOperation(),
                Names = apiFilterInternetBet.Names == null ? new FiltersOperation() : apiFilterInternetBet.Names.MapToFiltersOperation(),
                UserNames = apiFilterInternetBet.UserNames == null ? new FiltersOperation() : apiFilterInternetBet.UserNames.MapToFiltersOperation(),
                Categories = apiFilterInternetBet.Categories == null ? new FiltersOperation() : apiFilterInternetBet.Categories.MapToFiltersOperation(),
                ProductIds = apiFilterInternetBet.ProductIds == null ? new FiltersOperation() : apiFilterInternetBet.ProductIds.MapToFiltersOperation(),
                ProductNames = apiFilterInternetBet.ProductNames == null ? new FiltersOperation() : apiFilterInternetBet.ProductNames.MapToFiltersOperation(),
                ProviderNames = apiFilterInternetBet.ProviderNames == null ? new FiltersOperation() : apiFilterInternetBet.ProviderNames.MapToFiltersOperation(),
                Currencies = apiFilterInternetBet.Currencies == null ? new FiltersOperation() : apiFilterInternetBet.Currencies.MapToFiltersOperation(),
                RoundIds = apiFilterInternetBet.RoundIds == null ? new FiltersOperation() : apiFilterInternetBet.RoundIds.MapToFiltersOperation(),
                DeviceTypes = apiFilterInternetBet.DeviceTypes == null ? new FiltersOperation() : apiFilterInternetBet.DeviceTypes.MapToFiltersOperation(),
                ClientIps = apiFilterInternetBet.ClientIps == null ? new FiltersOperation() : apiFilterInternetBet.ClientIps.MapToFiltersOperation(),
                Countries = apiFilterInternetBet.Countries == null ? new FiltersOperation() : apiFilterInternetBet.Countries.MapToFiltersOperation(),
                States = apiFilterInternetBet.States == null ? new FiltersOperation() : apiFilterInternetBet.States.MapToFiltersOperation(),
                BetTypes = apiFilterInternetBet.BetTypes == null ? new FiltersOperation() : apiFilterInternetBet.BetTypes.MapToFiltersOperation(),
                PossibleWins = apiFilterInternetBet.PossibleWins == null ? new FiltersOperation() : apiFilterInternetBet.PossibleWins.MapToFiltersOperation(),
                BetAmounts = apiFilterInternetBet.BetAmounts == null ? new FiltersOperation() : apiFilterInternetBet.BetAmounts.MapToFiltersOperation(),
                WinAmounts = apiFilterInternetBet.WinAmounts == null ? new FiltersOperation() : apiFilterInternetBet.WinAmounts.MapToFiltersOperation(),
                BetDates = apiFilterInternetBet.BetDates == null ? new FiltersOperation() : apiFilterInternetBet.BetDates.MapToFiltersOperation(),
                BonusIds = apiFilterInternetBet.BonusIds == null ? new FiltersOperation() : apiFilterInternetBet.BonusIds.MapToFiltersOperation(),
                GGRs = apiFilterInternetBet.GGRs == null ? new FiltersOperation() : apiFilterInternetBet.GGRs.MapToFiltersOperation(),
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
        #endregion

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
                CreatorFirstName = note.CreatorFirstName,
                CreatorLastName = note.CreatorLastName
            };
        }

        public static FilterAnnouncement MapToFilterAnnouncement(this ApiFilterAnnouncement filter)
        {
            return new FilterAnnouncement
            {
                PartnerId = filter.PartnerId,
                ReceiverId = filter.ReceiverId,
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
                ReceiverIds = filter.ReceiverIds == null ? new FiltersOperation() : filter.ReceiverIds.MapToFiltersOperation(),
                Types = filter.Types == null ? new FiltersOperation() : filter.Types.MapToFiltersOperation(),
                States = filter.States == null ? new FiltersOperation() : filter.States.MapToFiltersOperation()
            };
        }

        public static ApiAnnouncement MapToApiAnnouncement(this fnAnnouncement input, double timeZone)
        {
            return new ApiAnnouncement
            {
                Id = input.Id,
                PartnerId = input.PartnerId,
                ReceiverId = input.ReceiverId,
                UserId = input.UserId,
                Type = input.Type,
                State = input.State,
                Message = input.Message,
                CreationDate = input.CreationDate.GetGMTDateFromUTC(timeZone),
                LastUpdateDate = input.LastUpdateDate.GetGMTDateFromUTC(timeZone)
            };
        }

        public static ApiOnlineUser MapToApiOnlineUser(this fnOnlineUser input, double timeZone)
        {
            return new ApiOnlineUser
            {
                Id = input.Id,
                Ip = input.Ip,
                StartTime = input.StartTime.GetGMTDateFromUTC(timeZone)
            };
        }

        public static FilterDashboard MapToFilterDashboard(this ApiFilterDashboard filter)
        {
            return new FilterDashboard
            {
                PartnerId = filter.PartnerId,
                FromDate = filter.FromDate ?? DateTime.UtcNow,
                ToDate = filter.ToDate ?? DateTime.UtcNow
            };
        }


        public static FilterTicket MapToFilterTicket(this ApiFilterTicket filterTicket)
        {
            return new FilterTicket
            {
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
                FieldNameToOrderBy = filterTicket.FieldNameToOrderBy,
                PartnerId = filterTicket.PartnerId,
                UserId = filterTicket.UserId
            };
        }

        public static List<TicketModel> MapToTickets(this IEnumerable<fnTicket> tickets, double timeZone, string languageId)
        {
            var statuses = CacheManager.GetEnumerations(Constants.EnumerationTypes.MessageTicketState, languageId);
            var types = CacheManager.GetEnumerations(Constants.EnumerationTypes.TicketTypes, languageId);
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
    }
}