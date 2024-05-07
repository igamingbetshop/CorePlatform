using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.AffiliateModels;
using IqSoft.CP.Common.Models.Filters;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Agents;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.DAL.Models.Products;
using IqSoft.CP.DAL.Models.Segment;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using static IqSoft.CP.Common.Constants;

namespace IqSoft.CP.BLL.Helpers
{
	public static class CustomMapper
	{
		public static BetShopFinOperationDocument MapToBetShopFinOperationDocument(this DAL.Document document)
		{
			return new BetShopFinOperationDocument
			{
				Id = document.Id,
				ExternalTransactionId = document.ExternalTransactionId,
				Amount = document.Amount,
				CurrencyId = document.CurrencyId,
				State = document.State,
				OperationTypeId = document.OperationTypeId,
				TypeId = document.TypeId,
				ParentId = document.ParentId,
				PaymentRequestId = document.PaymentRequestId,
				Info = document.Info,
				Creator = document.Creator,
				CashDeskId = document.CashDeskId,
				PartnerPaymentSettingId = document.PartnerPaymentSettingId,
				PartnerProductId = document.PartnerProductId,
				GameProviderId = document.GameProviderId,
				ClientId = document.ClientId,
				ExternalOperationId = document.ExternalOperationId,
				TicketNumber = document.TicketNumber,
				TicketInfo = document.TicketInfo,
				UserId = document.UserId,
				DeviceTypeId = document.DeviceTypeId,
				PossibleWin = document.PossibleWin,
				SessionId = document.SessionId,
				CreationTime = document.CreationTime,
				LastUpdateTime = document.LastUpdateTime,
				RoundId = document.RoundId,
				ProductId = document.ProductId
			};
		}

		public static BllAffiliate ToBllAffiliate(this Affiliate input, List<AffiliateCommission> affiliateCommissions)
		{
			return new BllAffiliate
			{
                AffiliateId = input.Id,
				PartnerId = input.PartnerId,
				UserName = input.UserName,
				FirstName = input.FirstName,
				LastName = input.LastName,
				NickName = input.NickName,
				Gender = input.Gender,
				RegionId = input.RegionId,
				LanguageId = input.LanguageId,
				PasswordHash = input.PasswordHash,
				Salt = input.Salt,
				State = input.State,
				Email = input.Email,
				MobileNumber = input.MobileNumber,
				CommunicationType = input.CommunicationType,
				CommunicationTypeValue = input.CommunicationTypeValue,
				CreationTime = input.CreationTime,
				LastUpdateTime = input.LastUpdateTime,
				FixedFeeCommission = affiliateCommissions?.Where(x => x.CommissionType == (int)AffiliateCommissionTypes.FixedFee)
														 .Select(x => new FixedFeeCommission
														 {
															 CurrencyId = x.CurrencyId,
															 Amount = x.Amount ?? 0,
															 TotalDepositAmount = x.TotalDepositAmount,
															 RequireVerification = x.RequireVerification
														 }).FirstOrDefault(),
				DepositCommission = affiliateCommissions?.Where(x => x.CommissionType == (int)AffiliateCommissionTypes.Deposit)
                                                         .Select(x => new DepositCommission
                                                         {
                                                             Percent = x.Percent ?? 0,
                                                             CurrencyId = x.CurrencyId,
                                                             UpToAmount = x.UpToAmount,
															 DepositCount = x.DepositCount
                                                         }).FirstOrDefault(),
                TurnoverCommission = affiliateCommissions?.Where(x => x.CommissionType == (int)AffiliateCommissionTypes.Turnover)
                                                         .Select(x => new BetCommission
                                                         {
                                                             ProductId = x.ProductId ?? 0,
                                                             Percent = x.Percent ?? 0
                                                         }).ToList(),
                GGRCommission = affiliateCommissions?.Where(x => x.CommissionType == (int)AffiliateCommissionTypes.GGR)
                                                         .Select(x => new BetCommission
                                                         {
                                                             ProductId = x.ProductId ?? 0,
                                                             Percent = x.Percent ?? 0
                                                         }).ToList(),
                NGRCommission = affiliateCommissions?.Where(x => x.CommissionType == (int)AffiliateCommissionTypes.NGR)
                                                         .Select(x => new BetCommission
                                                         {
                                                             ProductId = x.ProductId ?? 0,
                                                             Percent = x.Percent ?? 0
                                                         }).ToList()
            };
		}

        public static AgentReportItem ToAgentReportItem(this fnUser input)
        {
            return new AgentReportItem
            {
                AgentId = input.Id,
                AgentFirstName = input.FirstName,
				AgentLastName = input.LastName,
                AgentUserName = input.UserName,
                TotalDepositCount = 0,
                TotalWithdrawCount = 0,
                TotalDepositAmount = 0,
                TotalWithdrawAmount = 0,
                TotalBetsCount = 0,
                TotalUnsettledBetsCount = 0,
                TotalDeletedBetsCount = 0,
                TotalBetAmount = 0,
                TotalWinAmount = 0,
                TotalProfit = 0,
                TotalProfitPercent = 0,
                TotalGGRCommission = 0,
                TotalTurnoverCommission = 0
            };
        }

        public static AgentReportItem ToAgentReportItem(this fnAgent input)
        {
            return new AgentReportItem
            {
                AgentId = input.Id,
                AgentFirstName = input.FirstName,
                AgentLastName = input.LastName,
                AgentUserName = input.UserName,
                TotalDepositCount = 0,
                TotalWithdrawCount = 0,
                TotalDepositAmount = 0,
                TotalWithdrawAmount = 0,
                TotalBetsCount = 0,
                TotalUnsettledBetsCount = 0,
                TotalDeletedBetsCount = 0,
                TotalBetAmount = 0,
                TotalWinAmount = 0,
                TotalProfit = 0,
                TotalProfitPercent = 0,
                TotalGGRCommission = 0,
                TotalTurnoverCommission = 0
            };
        }

        public static object ToApiMenu(this BllMenu input)
		{
			return new
			{
				Type = input.Type,
				StyleType = input.StyleType,
				Items = input.Items.Select(x => x.ToApiMenuItem()).ToList()
			};
		}

		private static object ToApiMenuItem(this BllMenuItem input)
		{
			return new
			{
				Id = input.Id,
				Icon = input.Icon,
				Title = input.Title,
				Type = input.Type,
				StyleType = input.StyleType,
				Href = input.Href,
				OpenInRouting = input.OpenInRouting,
				Orientation = input.Orientation,
				Order = input.Order,
				SubMenu = input.SubMenu.Select(x => x.ToApiSubMenuItem()).ToList()
			};
		}

		private static object ToApiSubMenuItem(this BllSubMenuItem input)
		{
			return new
			{
				Icon = input.Icon,
				Title = input.Title,
				Type = input.Type,
				Href = input.Href,
				OpenInRouting = input.OpenInRouting,
				Order = input.Order,
				StyleType = input.StyleType
			};
		}

		public static object ToApiProductCategory(this BllProductCategory input)
		{
			return new
			{
				Id = input.Id == 0 ? (int?)null : input.Id,
				Type = input.Name,
				Products = input.Products
			};
		}

		public static object ToBonusInfo(this Bonu input)
		{
			return new
			{
				Id = input.Id,
				Name = input.Name,
				Description = input.Description,
				PartnerId = input.PartnerId,
				FinalAccountTypeId = input.FinalAccountTypeId,
				Status = input.Status,
				StartTime = input.StartTime,
				FinishTime = input.FinishTime,
				LastExecutionTime = input.LastExecutionTime,
				Period = input.Period,
				BonusType = input.Type,
				Info = input.Info,
				TurnoverCount = input.TurnoverCount,
				MinAmount = input.MinAmount,
				MaxAmount = input.MaxAmount,
				Sequence = input.Sequence,
				TranslationId = input.TranslationId,
				Priority = input.Priority,
				WinAccountTypeId = input.WinAccountTypeId,
				ValidForAwarding = input.ValidForAwarding,
				ValidForSpending = input.ValidForSpending,
				ReusingMaxCount = input.ReusingMaxCount,
				ResetOnWithdraw = input.ResetOnWithdraw,
				CreationTime = input.CreationTime,
				LastUpdateTime = input.LastUpdateTime,
				AllowSplit = input.AllowSplit,
				RefundRollbacked = input.RefundRollbacked,
				Condition = input.Condition,
				MaxGranted = input.MaxGranted,
				MaxReceiversCount = input.MaxReceiversCount,
				LinkedBonusId = input.LinkedBonusId,
				AutoApproveMaxAmount = input.AutoApproveMaxAmount,
				BonusCategorySettings = input.BonusSegmentSettings?.Select(x => new
				{ x.Id, x.SegmentId, x.Type }).ToList(),
				BonusCountrySettings = input.BonusCountrySettings?.Select(x => new
				{ x.Id, x.CountryId, x.Type }).ToList(),
				BonusCurrencySettings = input.BonusCurrencySettings?.Select(x => new
				{ x.Id, x.CurrencyId, x.Type }).ToList(),
				BonusLanguageSettings = input.BonusLanguageSettings?.Select(x => new
				{ x.Id, x.LanguageId, x.Type }).ToList(),
				BonusPaymentSystemSettings = input.BonusPaymentSystemSettings?.Select(x => new
				{ x.Id, x.PaymentSystemId, x.Type }).ToList(),
				FreezeBonusBalance = input.FreezeBonusBalance,
				Regularity = input.Regularity,
				DayOfWeek = input.DayOfWeek,
				ReusingMaxCountInPeriod = input.ReusingMaxCountInPeriod
			};
		}

		public static object ToUserInfo(this User input)
		{
			return new
			{
				Id = input.Id,
				PartnerId = input.PartnerId,
				FirstName = input.FirstName,
				LastName = input.LastName,
				Gender = input.Gender,
				LanguageId = input.LanguageId,
				UserName = input.UserName,
				NickName = input.NickName,
				PasswordHash = input.PasswordHash,
				Salt = input.Salt,
				State = input.State,
				CurrencyId = input.CurrencyId,
				Email = input.Email,
				MobileNumber = input.MobileNumber,
				Type = input.Type,
				AdminState = input.AdminState,
				ParentId = input.ParentId,
				Path = input.Path,
				SecurityCode = input.SecurityCode,
				QRCode = input.QRCode,
				IsTwoFactorEnabled = input.IsTwoFactorEnabled,
				Level = input.Level,
				CreationTime = input.CreationTime,
				LastUpdateTime = input.LastUpdateTime,
				SessionId = input.SessionId,
				ImageData = input.ImageData,
				PasswordChangedDate = input.PasswordChangedDate,
				LoginByNickName = input.LoginByNickName,
				OddsTypes = input.OddsType,
				input.CorrectionMaxAmount,
				input.CorrectionMaxAmountCurrency
			};
		}

		public static object ToTriggerInfo(this TriggerSetting input)
		{
			return new
			{
				Id = input.Id,
				Name = input.Name,
				Description = input.Description,
				TranslationId = input.TranslationId,
				Type = input.Type,
				StartTime = input.StartTime,
				FinishTime = input.FinishTime,
				Percent = input.Percent,
				BonusSettingCodes = input.BonusSettingCodes,
				PartnerId = input.PartnerId,
				CreationTime = input.CreationTime,
				LastUpdateTime = input.LastUpdateTime,
				MinAmount = input.MinAmount,
				MaxAmount = input.MaxAmount,
				Condition = input.Condition,
                ConsiderBonusBets = input.ConsiderBonusBets,
                UpToAmount = input.UpToAmount,
                Status = input.Status,
                BonusPaymentSystemSettings = input.BonusPaymentSystemSettings?.Select(x => new
				{ x.Id, x.PaymentSystemId, x.Type }).ToList()
			};
		}

		public static object ToClientIdentity(this ClientIdentity input)
		{
			return new
			{
				Id = input.Id,
				ClientId = input.ClientId,
				UserId = input.UserId,
				DocumentTypeId = input.DocumentTypeId,
				ImagePath = input.ImagePath,
				Status = input.Status,
				CreationTime = input.CreationTime,
				LastUpdateTime = input.LastUpdateTime,
				ExpirationTime = input.ExpirationTime,
				ExpirationDate = input.ExpirationDate
			};
		}

		public static object ToClientInfo(this Client input, AffiliateReferral affiliate, int? referralType)
		{
			return new
			{
				Id = input.Id,
				Email = input.Email,
				IsEmailVerified = input.IsEmailVerified,
				CurrencyId = input.CurrencyId,
				UserName = input.UserName,
				PasswordHash = input.PasswordHash,
				Salt = input.Salt,
				PartnerId = input.PartnerId,
				Gender = input.Gender,
				BirthDate = input.BirthDate,
				SendMail = input.SendMail,
				SendSms = input.SendSms,
				CallToPhone = input.CallToPhone,
				SendPromotions = input.SendPromotions,
				State = input.State,
				CategoryId = input.CategoryId,
				FirstName = input.FirstName,
				LastName = input.LastName,
				RegionId = input.RegionId,
				Info = input.Info,
				ZipCode = input.ZipCode,
				RegistrationIp = input.RegistrationIp,
				DocumentType = input.DocumentType,
				DocumentNumber = input.DocumentNumber,
				DocumentIssuedBy = input.DocumentIssuedBy,
				IsDocumentVerified = input.IsDocumentVerified,
				Address = input.Address,
				MobileNumber = input.MobileNumber,
				PhoneNumber = input.PhoneNumber,
				IsMobileNumberVerified = input.IsMobileNumberVerified,
				HasNote = input.HasNote,
				LanguageId = input.LanguageId,
				CreationTime = input.CreationTime,
				LastUpdateTime = input.LastUpdateTime,
				FirstDepositDate = input.FirstDepositDate,
				LastDepositDate = input.LastDepositDate,
				LastDepositAmount = input.LastDepositAmount,
				BetShopId = input.BetShopId,
				UserId = input.UserId,
				AffiliateReferralId = input.AffiliateReferralId,
				LastSessionId = input.LastSessionId,
				Citizenship = input.Citizenship,
				JobArea = input.JobArea,
				SecondName = input.SecondName,
				SecondSurname = input.SecondSurname,
				BuildingNumber = input.BuildingNumber,
				Apartment = input.Apartment,
				USSDPin = input.USSDPin,
				Title = input.Title,
				AffiliatePlatformId = affiliate?.AffiliatePlatformId,
				AffiliateId = affiliate?.AffiliateId,
				AffiliateRefId = affiliate?.RefId,
				ReferralType = referralType
			};
		}

		public static object ToClientInfo(this Client client, double timeZone)
		{
			var region = CacheManager.GetRegionById(client.RegionId, Constants.DefaultLanguageId);
			return new
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
				AffiliateReferralId = client.AffiliateReferralId,
				IsBonusEligible = true, // ???
				CategoryId = client.CategoryId,
				State = client.State,
				IsBanned = client.State == (int)ClientStates.Active,
				CountryName = region.Name,
				CountryCode = region.IsoCode
			};
		}

		public static object ToPartnerCurrencyInfo(this PartnerCurrencySetting input)
		{
			return new
			{
				Id = input.Id,
				PartnerId = input.PartnerId,
				CurrencyId = input.CurrencyId,
				State = input.State,
				CreationTime = input.CreationTime,
				LastUpdateTime = input.LastUpdateTime,
				Priority = input.Priority,
				UserMinLimit = input.UserMinLimit,
				UserMaxLimit = input.UserMaxLimit
			};
		}

		public static object ToProductInfo(this Product input, string languageId)
		{
			return new
			{
				input.Id,
				input.GameProviderId,
				input.PaymentSystemId,
				input.Level,
				Description = input.NickName,
				Name = input.Translation?.TranslationEntries?.FirstOrDefault(x => x.LanguageId == languageId)?.Text,
				input.ParentId,
				input.ExternalId,
				input.State,
				input.IsForDesktop,
				input.IsForMobile,
				input.WebImageUrl,
				input.MobileImageUrl,
				input.BackgroundImageUrl,
				input.SubproviderId,
				input.HasDemo,
				input.CategoryId,
				input.RTP,
				ProductCountrySettings = input.ProductCountrySettings?.Select(x => new
				{ x.Id, x.CountryId, x.Type }).ToList()
			};
		}

		public static object ToClientSettingInfo(this ClientSetting input)
		{
			return new
			{
				Id = input.Id,
				ClientId = input.ClientId,
				Name = input.Name,
				NumericValue = input.NumericValue,
				StringValue = string.IsNullOrEmpty(input.StringValue) ?
											   (input.NumericValue.HasValue ? input.NumericValue.Value.ToString() : String.Empty) : input.StringValue,
				DateValue = input.DateValue ?? input.CreationTime,
				UserId = input.UserId,
				CreationTime = input.CreationTime,
				LastUpdateTime = input.LastUpdateTime
			};
		}

		public static object ToBetShopInfo(this BetShop input)
		{
			return new
			{
				Id = input.Id,
				GroupId = input.GroupId,
				Type = input.Type,
				Name = input.Name,
				CurrencyId = input.CurrencyId,
				Address = input.Address,
				RegionId = input.RegionId,
				PartnerId = input.PartnerId,
				State = input.State,
				DailyTicketNumber = input.DailyTicketNumber,
				DefaultLimit = input.DefaultLimit,
				CurrentLimit = input.CurrentLimit,
				SessionId = input.SessionId,
				CreationTime = input.CreationTime,
				LastUpdateTime = input.LastUpdateTime,
				BonusPercent = input.BonusPercent,
				PrintLogo = input.PrintLogo,
				Ips = input.Ips,
				AgentId = input.UserId,
				PaymentSystems = input.PaymentSystems
			};
		}

		public static Segment MapToSegment(this SegmentModel model)
		{
			var currentTime = DateTime.UtcNow;
			return new Segment
			{
				Id = model.Id ?? 0,
				Name = model.Name,
				PartnerId = model.PartnerId,
				State = model.State ?? (int)SegmentStates.Active,
				Mode = model.Mode,
				Gender = model.Gender,
				IsKYCVerified = model.IsKYCVerified,
				IsTermsConditionAccepted = model.IsTermsConditionAccepted,
				ClientStatus = model.ClientStatus?.ToString(),
				SegmentId = model.SegmentId?.ToString(),
				ClientId = model.ClientId?.ToString(),
				Email = model.Email?.ToString(),
				FirstName = model.FirstName?.ToString(),
				LastName = model.LastName?.ToString(),
				Region = model.Region?.ToString(),
				AffiliateId = model.AffiliateId?.ToString(),
				MobileCode = model.MobileCode?.ToString(),
				SessionPeriod = model.SessionPeriod?.ToString(),
				SignUpPeriod = model.SignUpPeriod?.ToString(),
				TotalDepositsCount = model.TotalDepositsCount?.ToString(),
				TotalDepositsAmount = model.TotalDepositsAmount?.ToString(),
				TotalWithdrawalsCount = model.TotalWithdrawalsCount?.ToString(),
				TotalWithdrawalsAmount = model.TotalWithdrawalsAmount?.ToString(),
				TotalBetsCount = model.TotalBetsCount?.ToString(),
				TotalBetsAmount = model.TotalBetsAmount?.ToString(),
				Profit = model.Profit?.ToString(),
				SuccessDepositPaymentSystem = model.SuccessDepositPaymentSystem?.ToString(),
				SuccessWithdrawalPaymentSystem = model.SuccessWithdrawalPaymentSystem?.ToString(),
				SportBetsCount = model.SportBetsCount?.ToString(),
				CasinoBetsCount = model.CasinoBetsCount?.ToString(),
				ComplimentaryPoint = model.ComplimentaryPoint?.ToString(),
				CreationTime = model.CreationTime ?? currentTime,
				LastUpdateTime = model.LastUpdateTime ?? currentTime
			};
		}

		public static SegmentOutput MapToSegmentModel(this Segment segment, double timeZone)
		{
			var segmentSettingItems = CacheManager.GetSegmentSetting(segment.Id);
			var priority = segmentSettingItems.FirstOrDefault(y => y.Name == Constants.SegmentSettings.Priority);
			var depositMinAmount = segmentSettingItems.FirstOrDefault(y => y.Name == Constants.SegmentSettings.DepositMinAmount);
			var depositMaxAmount = segmentSettingItems.FirstOrDefault(y => y.Name == Constants.SegmentSettings.DepositMaxAmount);
			var withdrawMinAmount = segmentSettingItems.FirstOrDefault(y => y.Name == Constants.SegmentSettings.WithdrawMinAmount);
			var withdrawMaxAmount = segmentSettingItems.FirstOrDefault(y => y.Name == Constants.SegmentSettings.WithdrawMaxAmount);
			var segmentSetting = new SegementSettingModel
			{
				Priority = priority != null && priority.NumericValue.HasValue ? Convert.ToInt32(priority.NumericValue) : 0,
				DepositMinAmount = depositMinAmount != null && depositMinAmount.NumericValue.HasValue ? Convert.ToInt32(depositMinAmount.NumericValue) : 0,
				DepositMaxAmount = depositMaxAmount != null && depositMaxAmount.NumericValue.HasValue ? Convert.ToInt32(depositMaxAmount.NumericValue) : 0,
				WithdrawMinAmount = withdrawMinAmount != null && withdrawMinAmount.NumericValue.HasValue ? Convert.ToInt32(withdrawMinAmount.NumericValue) : 0,
				WithdrawMaxAmount = withdrawMaxAmount != null && withdrawMaxAmount.NumericValue.HasValue ? Convert.ToInt32(withdrawMaxAmount.NumericValue) : 0,
				ApiUrl = segmentSettingItems.FirstOrDefault(y => y.Name == Constants.SegmentSettings.ApiUrl)?.StringValue,
				ApiKey = segmentSettingItems.FirstOrDefault(y => y.Name == Constants.SegmentSettings.ApiKey)?.StringValue
			};
			return new SegmentOutput
			{
				Id = segment.Id,
				Name = segment.Name,
				PartnerId = segment.PartnerId,
                CurrencyId = segment.Partner?.CurrencyId,
                State = segment.State,
				Mode = segment.Mode,
				Gender = segment.Gender,
				IsKYCVerified = segment.IsKYCVerified,
				IsTermsConditionAccepted = segment.IsTermsConditionAccepted,
				ClientStatus = !string.IsNullOrEmpty(segment.ClientStatus) ?
					String.Join(",", JsonConvert.DeserializeObject<List<ConditionItem>>(segment.ClientStatus).Select(x => x.StringValue)) : null,
				ClientStatusObject = !string.IsNullOrEmpty(segment.ClientStatus) ? new Condition
				{
					ConditionItems = JsonConvert.DeserializeObject<List<ConditionItem>>(segment.ClientStatus)
				} : null,
				SegmentId = !string.IsNullOrEmpty(segment.SegmentId) ?
					String.Join(",", JsonConvert.DeserializeObject<List<ConditionItem>>(segment.SegmentId).Select(x => x.StringValue)) : null,
				SegmentIdObject = !string.IsNullOrEmpty(segment.SegmentId) ? new Condition
				{
					ConditionItems = JsonConvert.DeserializeObject<List<ConditionItem>>(segment.SegmentId)
				} : null,
				ClientId = !string.IsNullOrEmpty(segment.ClientId) ?
					String.Join(",", JsonConvert.DeserializeObject<List<ConditionItem>>(segment.ClientId).Select(x => x.StringValue)) : null,
				ClientIdObject = !string.IsNullOrEmpty(segment.ClientId) ? new Condition
				{
					ConditionItems = JsonConvert.DeserializeObject<List<ConditionItem>>(segment.ClientId)
				} : null,
				Email = !string.IsNullOrEmpty(segment.Email) ?
					String.Join(",", JsonConvert.DeserializeObject<List<ConditionItem>>(segment.Email).Select(x => x.StringValue)) : null,
				EmailObject = !string.IsNullOrEmpty(segment.Email) ? new Condition
				{
					ConditionItems = JsonConvert.DeserializeObject<List<ConditionItem>>(segment.Email)
				} : null,
				FirstName = !string.IsNullOrEmpty(segment.FirstName) ?
					String.Join(",", JsonConvert.DeserializeObject<List<ConditionItem>>(segment.FirstName).Select(x => x.StringValue)) : null,
				FirstNameObject = !string.IsNullOrEmpty(segment.FirstName) ? new Condition
				{
					ConditionItems = JsonConvert.DeserializeObject<List<ConditionItem>>(segment.FirstName)
				} : null,
				LastName = !string.IsNullOrEmpty(segment.LastName) ?
					String.Join(",", JsonConvert.DeserializeObject<List<ConditionItem>>(segment.LastName).Select(x => x.StringValue)) : null,
				LastNameObject = !string.IsNullOrEmpty(segment.LastName) ? new Condition
				{
					ConditionItems = JsonConvert.DeserializeObject<List<ConditionItem>>(segment.LastName)
				} : null,
				Region = !string.IsNullOrEmpty(segment.Region) ?
					String.Join(",", JsonConvert.DeserializeObject<List<ConditionItem>>(segment.Region).Select(x => x.StringValue)) : null,
				RegionObject = !string.IsNullOrEmpty(segment.Region) ? new Condition
				{
					ConditionItems = JsonConvert.DeserializeObject<List<ConditionItem>>(segment.Region)
				} : null,
				AffiliateId = !string.IsNullOrEmpty(segment.AffiliateId) ?
					String.Join(",", JsonConvert.DeserializeObject<List<ConditionItem>>(segment.AffiliateId).Select(x => x.StringValue)) : null,
				AffiliateIdObject = !string.IsNullOrEmpty(segment.AffiliateId) ? new Condition
				{
					ConditionItems = JsonConvert.DeserializeObject<List<ConditionItem>>(segment.AffiliateId)
				} : null,
				MobileCode = !string.IsNullOrEmpty(segment.MobileCode) ?
					String.Join(",", JsonConvert.DeserializeObject<List<ConditionItem>>(segment.MobileCode).Select(x => x.StringValue)) : null,
				MobileCodeObject = !string.IsNullOrEmpty(segment.MobileCode) ? new Condition
				{
					ConditionItems = JsonConvert.DeserializeObject<List<ConditionItem>>(segment.MobileCode)
				} : null,
				SessionPeriod = !string.IsNullOrEmpty(segment.SessionPeriod) ?
					String.Join("&", JsonConvert.DeserializeObject<List<ConditionItem>>(segment.SessionPeriod).Select(x => GetOperationByTypeId(x.OperationTypeId) + " " + x.StringValue)) : null,
				SessionPeriodObject = !string.IsNullOrEmpty(segment.SessionPeriod) ? new Condition
				{
					ConditionItems = JsonConvert.DeserializeObject<List<ConditionItem>>(segment.SessionPeriod)
				} : null,
				SignUpPeriod = !string.IsNullOrEmpty(segment.SignUpPeriod) ?
					String.Join("&", JsonConvert.DeserializeObject<List<ConditionItem>>(segment.SignUpPeriod).Select(x => GetOperationByTypeId(x.OperationTypeId) + " " + x.StringValue)) : null,
				SignUpPeriodObject = !string.IsNullOrEmpty(segment.SignUpPeriod) ? new Condition
				{
					ConditionItems = JsonConvert.DeserializeObject<List<ConditionItem>>(segment.SignUpPeriod)
				} : null,
				TotalDepositsCount = !string.IsNullOrEmpty(segment.TotalDepositsCount) ?
					String.Join("&", JsonConvert.DeserializeObject<List<ConditionItem>>(segment.TotalDepositsCount).Select(x => GetOperationByTypeId(x.OperationTypeId) + " " + x.StringValue)) : null,
				TotalDepositsCountObject = !string.IsNullOrEmpty(segment.TotalDepositsCount) ? new Condition
				{
					ConditionItems = JsonConvert.DeserializeObject<List<ConditionItem>>(segment.TotalDepositsCount)
				} : null,
				TotalDepositsAmount = !string.IsNullOrEmpty(segment.TotalDepositsAmount) ?
					String.Join("&", JsonConvert.DeserializeObject<List<ConditionItem>>(segment.TotalDepositsAmount).Select(x => GetOperationByTypeId(x.OperationTypeId) + " " + x.StringValue)) : null,
				TotalDepositsAmountObject = !string.IsNullOrEmpty(segment.TotalDepositsAmount) ? new Condition
				{
					ConditionItems = JsonConvert.DeserializeObject<List<ConditionItem>>(segment.TotalDepositsAmount)
				} : null,
				TotalWithdrawalsCount = !string.IsNullOrEmpty(segment.TotalWithdrawalsCount) ?
					String.Join("&", JsonConvert.DeserializeObject<List<ConditionItem>>(segment.TotalWithdrawalsCount).Select(x => GetOperationByTypeId(x.OperationTypeId) + " " + x.StringValue)) : null,
				TotalWithdrawalsCountObject = !string.IsNullOrEmpty(segment.TotalWithdrawalsCount) ? new Condition
				{
					ConditionItems = JsonConvert.DeserializeObject<List<ConditionItem>>(segment.TotalWithdrawalsCount)
				} : null,
				TotalWithdrawalsAmount = !string.IsNullOrEmpty(segment.TotalWithdrawalsAmount) ?
					String.Join("&", JsonConvert.DeserializeObject<List<ConditionItem>>(segment.TotalWithdrawalsAmount).Select(x => GetOperationByTypeId(x.OperationTypeId) + " " + x.StringValue)) : null,
				TotalWithdrawalsAmountObject = !string.IsNullOrEmpty(segment.TotalWithdrawalsAmount) ? new Condition
				{
					ConditionItems = JsonConvert.DeserializeObject<List<ConditionItem>>(segment.TotalWithdrawalsAmount)
				} : null,
				TotalBetsCount = !string.IsNullOrEmpty(segment.TotalBetsCount) ?
					String.Join("&", JsonConvert.DeserializeObject<List<ConditionItem>>(segment.TotalBetsCount).Select(x => GetOperationByTypeId(x.OperationTypeId) + " " + x.StringValue)) : null,
				TotalBetsCountObject = !string.IsNullOrEmpty(segment.TotalBetsCount) ? new Condition
				{
					ConditionItems = JsonConvert.DeserializeObject<List<ConditionItem>>(segment.TotalBetsCount)
				} : null,
				TotalBetsAmount = !string.IsNullOrEmpty(segment.TotalBetsAmount) ?
					String.Join("&", JsonConvert.DeserializeObject<List<ConditionItem>>(segment.TotalBetsAmount).Select(x => GetOperationByTypeId(x.OperationTypeId) + " " + x.StringValue)) : null,
				TotalBetsAmountObject = !string.IsNullOrEmpty(segment.TotalBetsAmount) ? new Condition
				{
					ConditionItems = JsonConvert.DeserializeObject<List<ConditionItem>>(segment.TotalBetsAmount)
				} : null,
				SuccessDepositPaymentSystem = !string.IsNullOrEmpty(segment.SuccessDepositPaymentSystem) ?
					String.Join(",", JsonConvert.DeserializeObject<List<ConditionItem>>(segment.SuccessDepositPaymentSystem).Select(x => x.StringValue)) : null,
				SuccessDepositPaymentSystemObject = !string.IsNullOrEmpty(segment.SuccessDepositPaymentSystem) ? new Condition
				{
					ConditionItems = JsonConvert.DeserializeObject<List<ConditionItem>>(segment.SuccessDepositPaymentSystem)
				} : null,
				SuccessWithdrawalPaymentSystem = !string.IsNullOrEmpty(segment.SuccessWithdrawalPaymentSystem) ?
					String.Join(",", JsonConvert.DeserializeObject<List<ConditionItem>>(segment.SuccessWithdrawalPaymentSystem).Select(x => x.StringValue)) : null,
				SuccessWithdrawalPaymentSystemObject = !string.IsNullOrEmpty(segment.SuccessWithdrawalPaymentSystem) ? new Condition
				{
					ConditionItems = JsonConvert.DeserializeObject<List<ConditionItem>>(segment.SuccessWithdrawalPaymentSystem)
				} : null,
				ComplimentaryPoint = !string.IsNullOrEmpty(segment.ComplimentaryPoint) ?
					String.Join("&", JsonConvert.DeserializeObject<List<ConditionItem>>(segment.ComplimentaryPoint).Select(x => GetOperationByTypeId(x.OperationTypeId) + " " + x.StringValue)) : null,
				ComplimentaryPointObject = !string.IsNullOrEmpty(segment.ComplimentaryPoint) ? new Condition
				{
					ConditionItems = JsonConvert.DeserializeObject<List<ConditionItem>>(segment.ComplimentaryPoint)
				} : null,
				SportBetsCount = !string.IsNullOrEmpty(segment.SportBetsCount) ?
					String.Join("&", JsonConvert.DeserializeObject<List<ConditionItem>>(segment.SportBetsCount).Select(x => GetOperationByTypeId(x.OperationTypeId) + " " + x.StringValue)) : null,
				SportBetsCountObject = !string.IsNullOrEmpty(segment.SportBetsCount) ? new Condition
				{
					ConditionItems = JsonConvert.DeserializeObject<List<ConditionItem>>(segment.SportBetsCount)
				} : null,
				CasinoBetsCount = !string.IsNullOrEmpty(segment.CasinoBetsCount) ?
					String.Join("&", JsonConvert.DeserializeObject<List<ConditionItem>>(segment.CasinoBetsCount).Select(x => GetOperationByTypeId(x.OperationTypeId) + " " + x.StringValue)) : null,
				CasinoBetsCountObject = !string.IsNullOrEmpty(segment.CasinoBetsCount) ? new Condition
				{
					ConditionItems = JsonConvert.DeserializeObject<List<ConditionItem>>(segment.CasinoBetsCount)
				} : null,
				SegementSetting = segmentSetting,
				CreationTime = segment.CreationTime.GetGMTDateFromUTC(timeZone),
				LastUpdateTime = segment.LastUpdateTime.GetGMTDateFromUTC(timeZone)
            };
		}

		private static string GetOperationByTypeId(int typeId)
		{
			switch (typeId)
			{
				case 1:
					return "=";
				case 2:
					return ">=";
				case 3:
					return ">";
				case 4:
					return "<=";
				case 5:
					return "<";
				case 6:
					return "!=";
				case 7:
					return "Contains";
				case 8:
					return "StartsWith";
				case 9:
					return "EndsWith";
				case 10:
					return "DoesNotContain";
				case 11:
					return "InSet";
				case 12:
					return "OutOfSet";
				case 13:
					return "AtLeastOneInSet";
				case 14:
					return "IsNull";
				default:
					return string.Empty;
			}
		}

		public static ClientInfoTypes MapToClientInfoType(this VerificationCodeTypes verificationCodeType)
		{
			switch (verificationCodeType)
			{
				case VerificationCodeTypes.MobileNumberVerification:
				case VerificationCodeTypes.PasswordChangeByMobile://to be checked
				case VerificationCodeTypes.SecurityQuestionChangeByMobile://to be checked
				case VerificationCodeTypes.USSDPinChangeByMobile://to be checked
					return ClientInfoTypes.MobileVerificationKey;
				case VerificationCodeTypes.EmailVerification:
				case VerificationCodeTypes.PasswordChangeByEmail: //to be checked
				case VerificationCodeTypes.SecurityQuestionChangeByEmail: //to be checked
				case VerificationCodeTypes.USSDPinChangeByEmail: //to be checked
					return ClientInfoTypes.EmailVerificationKey;
				case VerificationCodeTypes.PasswordRecoveryByEmail:
					return ClientInfoTypes.PasswordRecoveryEmailKey;
				case VerificationCodeTypes.PasswordRecoveryByMobile:
					return ClientInfoTypes.PasswordRecoveryMobileKey;
				case VerificationCodeTypes.WithdrawByEmail:
					return ClientInfoTypes.WithdrawVerificationEmail;
				case VerificationCodeTypes.WithdrawByMobile:
					return ClientInfoTypes.WithdrawVerificationSMS;
				case VerificationCodeTypes.AddBankAccountByEmail:
				case VerificationCodeTypes.AddBankAccountByMobile:
					return ClientInfoTypes.AccountDetailsMobileKey; //to be checked
			}
			throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.WrongInputParameters);
		}
	}
}
