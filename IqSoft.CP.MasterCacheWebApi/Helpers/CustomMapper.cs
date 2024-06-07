using System;
using System.Collections.Generic;
using System.Linq;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Filters;
using IqSoft.CP.DAL.Filters.Documents;
using IqSoft.CP.DAL.Filters.PaymentRequests;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.Common.Models.WebSiteModels.Clients;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.WebSiteModels.Filters;
using IqSoft.CP.Common.Models.WebSiteModels.Menu;
using IqSoft.CP.DAL.Models.Clients;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.DAL.Models.Report;
using Newtonsoft.Json;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.Common.Models.WebSiteModels.Bets;
using static IqSoft.CP.Integration.Products.Helpers.InternalHelpers;
using IqSoft.CP.DataWarehouse.Filters;
using IqSoft.CP.DataWarehouse;
using Client = IqSoft.CP.DAL.Client;
using Document = IqSoft.CP.DAL.Document;
using PaymentRequest = IqSoft.CP.DAL.PaymentRequest;
using IqSoft.CP.BLL.Services;

namespace IqSoft.CP.MasterCacheWebApi.Helpers
{
    public static class CustomMapper
    {
        #region Models

        #region Client
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
        public static Client MapToClient(this ClientModel client)
        {
            int regionId = 0;
            if (client.Town != null)
                regionId = client.Town.Value;
            else if (client.City != null)
                regionId = client.City.Value;
            else if (client.Country != null)
                regionId = client.Country.Value;

            return new Client
            {
                Id = client.Id,
                Email = string.IsNullOrWhiteSpace(client.Email) ? string.Empty : client.Email.ToLower(),
                IsEmailVerified = false,
                CurrencyId = client.CurrencyId,
                UserName = string.IsNullOrWhiteSpace(client.UserName) ? string.Empty : client.UserName,
                Password = client.Password,
                PartnerId = client.PartnerId,
                RegionId = regionId,
                CountryId = client.Country,
                Gender = client.Gender,
                BirthDate = new DateTime(client.BirthYear ?? DateTime.MinValue.Year, client.BirthMonth ?? DateTime.MinValue.Month, client.BirthDay ?? DateTime.MinValue.Day),
                FirstName = client.FirstName,
                LastName = client.LastName,
                NickName = client.NickName,
                SecondName = client.SecondName,
                RegistrationIp = client.Ip,
                DocumentType = client.DocumentType,
                DocumentNumber = client.DocumentNumber,
                PhoneNumber = client.PhoneNumber,
                DocumentIssuedBy = client.DocumentIssuedBy,
                Address = client.Address,
                MobileNumber = string.IsNullOrWhiteSpace(client.MobileNumber) ? string.Empty : (client.MobileNumber.StartsWith("+") ? client.MobileNumber : "+" + client.MobileNumber),
                IsMobileNumberVerified = false,
                LanguageId = client.LanguageId,
                CreationTime = client.CreationTime,
                Token = client.Token,
                SendMail = client.SendMail ?? true,
                SendSms = client.SendSms,
                CallToPhone = client.CallToPhone,
                SendPromotions = client.SendPromotions,
                IsDocumentVerified = false,
                CategoryId = client.CategoryId == null ? 0 : client.CategoryId.Value,
                ZipCode = client.ZipCode?.Trim(),
                City = client.CityName,
                Info = client.PromoCode,
                Citizenship = client.Citizenship,
                JobArea = client.JobArea,
                Apartment = client.Apartment,
                BuildingNumber = client.BuildingNumber,
                Title = client.Title,
                PinCode = client.PinCode
            };
        }
   
        public static TerminalClientInput MapToTerminalClientInput(this LoginDetails loginDetails)
        {
            return new TerminalClientInput
            {
                TerminalId = loginDetails.TerminalId,
                BetShopId = loginDetails.BetShopId.Value,
                AuthToken = loginDetails.Token,
                PartnerId = loginDetails.PartnerId,
                Ip = loginDetails.Ip,
                LanguageId = loginDetails.LanguageId
            };
        }

        public static ApiLoginClientOutput MapToApiLoginClientOutput(this Client client, double timezone, ClientLoginOut clientLoginOut = null)
        {
            var currency = CacheManager.GetCurrencyById(client.CurrencyId);
            return new ApiLoginClientOutput
            {
                Id = client.Id,
                Email = client.Email,
                IsEmailVerified = client.IsEmailVerified,
                CurrencyId = currency.Id,
                CurrencyName = currency.Name,
                UserName = client.UserName,
                Password = client.Password,
                PartnerId = client.PartnerId,
                RegionId = clientLoginOut == null ? client.RegionId : clientLoginOut.RegionId,
                TownId = clientLoginOut == null ? client.RegionId : clientLoginOut.TownId,
                CityId = clientLoginOut == null ? client.RegionId : clientLoginOut.CityId,
                DistrictId = clientLoginOut == null ? client.RegionId : clientLoginOut.DistrictId,
                CountryId = clientLoginOut == null ? (client.CountryId ?? client.RegionId) : clientLoginOut.CountryId,
                City = client.City,
                Gender = client.Gender,
                BirthDate = client.BirthDate,
                FirstName = client.FirstName,
                LastName = client.LastName,
                NickName = client.NickName,
                RegistrationIp = client.RegistrationIp,
                DocumentNumber = client.DocumentNumber,
                PhoneNumber = client.PhoneNumber,
                DocumentIssuedBy = client.DocumentIssuedBy,
                Address = client.Address,
                MobileNumber = client.MobileNumber,
                IsMobileNumberVerified = client.IsMobileNumberVerified,
                LanguageId = client.LanguageId,
                CreationTime = client.CreationTime,
                EmailOrMobile = client.EmailOrMobile,
                Token = client.Token,
                SendMail = client.SendMail,
                SendSms = client.SendSms,
                CallToPhone = client.CallToPhone,
                SendPromotions = client.SendPromotions,
                IsDocumentVerified = client.IsDocumentVerified,
                ZipCode = client.ZipCode?.Trim(),
                Info = client.Info,
                CategoryId = client.CategoryId,
                WelcomeBonusActivationKey = client.WelcomeBonusActivationKey,
                CurrencySymbol = client.CurrencySymbol,
                Citizenship = client.Citizenship,
                JobArea = client.JobArea,
                LastLogin = clientLoginOut?.LastSession?.StartTime?.GetGMTDateFromUTC(timezone),
                LastLogout = clientLoginOut?.LastSession?.EndTime?.GetGMTDateFromUTC(timezone),
                LastLoginIp = clientLoginOut?.LastSession?.Ip,
                SessionTimeLeft = clientLoginOut?.SessionTimeLeft,
                SportBets = clientLoginOut?.SportBets,
                SportWins = clientLoginOut?.SportWins,
                SportProfit = clientLoginOut?.SportProfit,
                ResetPassword = clientLoginOut?.ResetPassword,
                AcceptTermsConditions = clientLoginOut?.AcceptTermsConditions,
                DocumentExpirationStatus = clientLoginOut?.DocumentExpirationStatus,
                IframeUrl = clientLoginOut?.IframeUrl,
                USSDPin = client.USSDPin,
                Title = client.Title, 
                CharacterId = client.CharacterId, 
                IsTwoFactorEnabled = client.IsTwoFactorEnabled ?? false
            };
        }

        public static ApiLoginClientOutput MapToApiAffiliateOutput(this Affiliate affiliate, double timezone, ClientLoginOut affiliateLoginOut = null)
        {
            return new ApiLoginClientOutput
            {
                Id = affiliate.Id,
                Email = affiliate.Email,
                PartnerId = affiliate.PartnerId,
                UserName = affiliate.UserName,
                RegionId = affiliateLoginOut?.RegionId ?? affiliate.RegionId,
                CityId = affiliateLoginOut?.CityId ?? affiliate.RegionId,
                DistrictId = affiliateLoginOut?.DistrictId ?? affiliate.RegionId,
                CountryId = affiliateLoginOut?.CountryId ?? affiliate.RegionId,
                Gender = affiliate.Gender,
                FirstName = affiliate.FirstName,
                LastName = affiliate.LastName,
                NickName = affiliate.NickName,
                MobileNumber = affiliate.MobileNumber,
                LanguageId = affiliate.LanguageId,
                CreationTime = affiliate.CreationTime,
                LastLogin = affiliateLoginOut?.LastSession?.StartTime?.GetGMTDateFromUTC(timezone),
                LastLogout = affiliateLoginOut?.LastSession?.EndTime?.GetGMTDateFromUTC(timezone),
                LastLoginIp = affiliateLoginOut?.LastSession?.Ip,
                ResetPassword = affiliateLoginOut?.ResetPassword,
                AcceptTermsConditions = affiliateLoginOut?.AcceptTermsConditions,
                DocumentExpirationStatus = affiliateLoginOut?.DocumentExpirationStatus
            };
        }

        public static ApiLoginClientOutput ToApiLoginClientOutput(this BllUser user, string newToken)
        {
            var currency = CacheManager.GetCurrencyById(user.CurrencyId);
            return new ApiLoginClientOutput
            {
                Id = user.Id,
                Email = user.Email,
                CurrencyId = currency.Id,
                CurrencyName = currency.Name,
                UserName = user.UserName,
                PartnerId = user.PartnerId,
                Gender = user.Gender,
                FirstName = user.FirstName,
                LastName = user.LastName,
                NickName = user.NickName,
                MobileNumber = user.MobileNumber,
                CreationTime = user.CreationTime,
                Token = newToken,
                CurrencySymbol = currency.Symbol,
                IsAgent = true
            };
        }

        public static ApiClientInfo ToApiClientInfo(this BllClient client, double timezone, ClientLoginOut clientLoginOut = null)
        {
            var currency = CacheManager.GetCurrencyById(client.CurrencyId);
            return new ApiClientInfo
            {
                Id = client.Id,
                Email = client.Email,
                IsEmailVerified = client.IsEmailVerified,
                CurrencyId = currency.Name,
                UserName = client.UserName,
                PartnerId = client.PartnerId,
                City = client.City,
                RegionId = clientLoginOut == null ? client.RegionId : clientLoginOut.RegionId,
                CityId = clientLoginOut == null ? client.RegionId : clientLoginOut.CityId,
                CountryId = clientLoginOut == null ? client.RegionId : clientLoginOut.CountryId,
                DistrictId = clientLoginOut == null ? client.RegionId : clientLoginOut.DistrictId,
                StateId = clientLoginOut == null ? client.RegionId : clientLoginOut.StateId,
                TownId = clientLoginOut == null ? client.RegionId : clientLoginOut.TownId,
                Gender = client.Gender,
                BirthDate = client.BirthDate,
                FirstName = client.FirstName,
                LastName = client.LastName,
                NickName = client.NickName,
                DocumentNumber = client.DocumentNumber,
                DocumentIssuedBy = client.DocumentIssuedBy,
                Address = client.Address,
                MobileNumber = client.MobileNumber,
                IsMobileNumberVerified = client.IsMobileNumberVerified,
                LanguageId = client.LanguageId,
                CreationTime = client.CreationTime,
                EmailOrMobile = client.EmailOrMobile,
                SendMail = client.SendMail,
                SendSms = client.SendSms,
                SendPromotions = client.SendPromotions,
                IsDocumentVerified = client.IsDocumentVerified,
                ZipCode = client.ZipCode?.Trim(),
                Info = client.Info,
                CategoryId = client.CategoryId,
                CurrencySymbol = currency.Symbol,
                Citizenship = client.Citizenship,
                JobArea = client.JobArea,
                LastLogin = clientLoginOut?.LastSession?.StartTime?.GetGMTDateFromUTC(timezone),
                LastLogout = clientLoginOut?.LastSession?.EndTime?.GetGMTDateFromUTC(timezone),
                LastLoginIp = clientLoginOut?.LastSession?.Ip,
                ResetPassword = clientLoginOut?.ResetPassword,
                AcceptTermsConditions = clientLoginOut?.AcceptTermsConditions,
                DocumentExpirationStatus = clientLoginOut?.DocumentExpirationStatus,
                USSDPin = client.USSDPin,
                Title = client.Title,
                IsTwoFactorEnabled = client.IsTwoFactorEnabled
            };
        }

        public static ApiClientInfo ToApiClientInfo(this Client client, double timezone, ClientLoginOut clientLoginOut = null)
        {
            var currency = CacheManager.GetCurrencyById(client.CurrencyId);
            return new ApiClientInfo
            {
                Id = client.Id,
                Email = client.Email,
                IsEmailVerified = client.IsEmailVerified,
                CurrencyId = currency.Name,
                UserName = client.UserName,
                PartnerId = client.PartnerId,
                RegionId = clientLoginOut == null ? client.RegionId : clientLoginOut.RegionId,
                CityId = clientLoginOut == null ? client.RegionId : clientLoginOut.CityId,
                CountryId = clientLoginOut == null ? client.RegionId : clientLoginOut.CountryId,
                Gender = client.Gender,
                BirthDate = client.BirthDate,
                FirstName = client.FirstName,
                LastName = client.LastName,
                NickName = client.NickName,
                RegistrationIp = client.RegistrationIp,
                DocumentNumber = client.DocumentNumber,
                PhoneNumber = client.PhoneNumber,
                DocumentIssuedBy = client.DocumentIssuedBy,
                Address = client.Address,
                MobileNumber = client.MobileNumber,
                IsMobileNumberVerified = client.IsMobileNumberVerified,
                LanguageId = client.LanguageId,
                CreationTime = client.CreationTime,
                EmailOrMobile = client.EmailOrMobile,
                Token = client.Token,
                SendMail = client.SendMail,
                SendSms = client.SendSms,
                CallToPhone = client.CallToPhone,
                SendPromotions = client.SendPromotions,
                IsDocumentVerified = client.IsDocumentVerified,
                ZipCode = client.ZipCode?.Trim(),
                Info = client.Info,
                CategoryId = client.CategoryId,
                WelcomeBonusActivationKey = client.WelcomeBonusActivationKey,
                CurrencySymbol = currency.Symbol,
                Citizenship = client.Citizenship,
                JobArea = client.JobArea,
                LastLogin = clientLoginOut?.LastSession?.StartTime?.GetGMTDateFromUTC(timezone),
                LastLogout = clientLoginOut?.LastSession?.EndTime?.GetGMTDateFromUTC(timezone),
                LastLoginIp = clientLoginOut?.LastSession?.Ip,
                ResetPassword = clientLoginOut?.ResetPassword,
                AcceptTermsConditions = clientLoginOut?.AcceptTermsConditions,
                DocumentExpirationStatus = clientLoginOut?.DocumentExpirationStatus
            };
        }

        public static ApiLoginClientOutput MapToApiLoginClientOutput(this BllClient client, string token, double timeZone)
        {
            var partnerSetting = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.ResetPasswordOnFirstLogin);
            var currency = CacheManager.GetCurrencyById(client.CurrencyId);
            return new ApiLoginClientOutput
            {
                Id = client.Id,
                Email = client.Email,
                IsEmailVerified = client.IsEmailVerified,
                CurrencyId = currency.Name,
                UserName = client.UserName,
                PartnerId = client.PartnerId,
                RegionId = client.RegionId,
                CityId = client.RegionId,
                CountryId = client.RegionId,
                Gender = client.Gender,
                BirthDate = client.BirthDate,
                FirstName = client.FirstName,
                LastName = client.LastName,
                NickName = client.NickName,
                DocumentNumber = client.DocumentNumber,
                DocumentIssuedBy = client.DocumentIssuedBy,
                Address = client.Address,
                MobileNumber = client.MobileNumber,
                IsMobileNumberVerified = client.IsMobileNumberVerified,
                LanguageId = client.LanguageId,
                CreationTime = client.CreationTime,
                EmailOrMobile = client.EmailOrMobile,
                Token = token,
                SendMail = client.SendMail,
                SendSms = client.SendSms,
                SendPromotions = client.SendPromotions,
                IsDocumentVerified = client.IsDocumentVerified,
                Info = client.Info,
                CategoryId = client.CategoryId,
                CurrencySymbol = client.CurrencySymbol,
                Citizenship = client.Citizenship,
                JobArea = client.JobArea,
                AD = client.AlternativeDomain,
                ADM = client.AlternativeDomainMessage,
                TimeZone = timeZone,
                ResetPassword = (partnerSetting != null && partnerSetting.NumericValue.HasValue && partnerSetting.NumericValue != 0 && !client.LastSessionId.HasValue),
                ResetNickName = (partnerSetting != null && partnerSetting.NumericValue.HasValue && partnerSetting.NumericValue != 0 && string.IsNullOrEmpty(client.NickName)),
                Title = client.Title
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
                EndTime = paymentLimit.EndTime
            };
        }

		public static ClientIdentity ToClientIdentity(this AddClientIdentityModel input)
		{
			return new ClientIdentity
			{
				Id = input.Id,
				ClientId = input.ClientId,
				DocumentTypeId = input.DocumentTypeId,
				Status = (int)KYCDocumentStates.InProcess,
				UserId = null
			};
		}

		public static ApiClientIdentityModel ToClientIdentityModel(this ClientIdentity clientIdentity, double timezone)
		{
			return new ApiClientIdentityModel
			{
				Id = clientIdentity.Id,
				ClientId = clientIdentity.ClientId,
                Status = clientIdentity.Status,
                CreationTime = clientIdentity.CreationTime.GetGMTDateFromUTC(timezone),
				LastUpdateTime = clientIdentity.LastUpdateTime.GetGMTDateFromUTC(timezone),
				DocumentTypeId = clientIdentity.DocumentTypeId
			};
		}

		public static ApiClientIdentityModel ToClientIdentityModel(this fnClientIdentity clientIdentity, double timezone, List<BllFnEnumeration> documentTypes)
		{
			return new ApiClientIdentityModel
			{
				Id = clientIdentity.Id,
				ClientId = clientIdentity.ClientId,
				CreationTime = clientIdentity.CreationTime.GetGMTDateFromUTC(timezone),
				LastUpdateTime = clientIdentity.LastUpdateTime.GetGMTDateFromUTC(timezone),
				DocumentTypeId = clientIdentity.DocumentTypeId,
                DocumentTypeName = documentTypes.FirstOrDefault(x => x.Value == clientIdentity.DocumentTypeId)?.Text,
                Status = clientIdentity.Status
			};
		}


        #endregion

        #region ClientMessage
        public static DAL.Models.Notification.PaymentNotificationInfo MapToPaymentNotificationInfo(this Common.Models.PaymentInfo paymentInfo)
        {
            return new DAL.Models.Notification.PaymentNotificationInfo
            {
                Amount = paymentInfo.Amount ?? 0,
                BankName = paymentInfo.BankName,
                BankBranchName = paymentInfo.BankBranchName,
                BankCode = paymentInfo.BankCode,
                BankAccountNumber = paymentInfo.BankAccountNumber,
                BankAccountHolder = paymentInfo.BankAccountHolder,
                WalletNumber = paymentInfo.WalletNumber
            };
        }

        public static List<TicketMessageModel> MapToTicketMessages(this IEnumerable<TicketMessage> ticketMessages, double timeZone)
        {
            return ticketMessages.Select(x => x.MapToTicketMessageModel(timeZone)).OrderBy(x => x.CreationTime).ToList();
        }

        public static TicketMessageModel MapToTicketMessageModel(this TicketMessage ticketMessage, double timeZone)
        {
            return new TicketMessageModel
            {
                Id = ticketMessage.Id,
                Message  = ticketMessage.Message,              
                Type = ticketMessage.Type,
                CreationTime = ticketMessage.CreationTime.AddHours(timeZone),
                TicketId = ticketMessage.TicketId,
				IsFromUser = (ticketMessage.UserId != null)
            };
        }

        public static List<TicketModel> MapToClientTickets(this IEnumerable<Ticket> tickets, double timeZone)
        {
            return tickets.Select(x => x.MapToTicketeModel(timeZone)).OrderByDescending(x => x.LastMessageTime).ToList();
        }

        public static TicketModel MapToTicketeModel(this Ticket ticket, double timeZone)
        {
            return new TicketModel
            {
                Id = ticket.Id,
                ClientId = ticket.ClientId ?? 0,
                PartnerId = ticket.PartnerId,
                Status = ticket.Status,
                Subject = ticket.Subject,
                Type = ticket.Type,
                CreationTime = ticket.CreationTime.AddHours(timeZone),
                LastMessageTime = ticket.LastMessageTime.AddHours(timeZone),
                UnreadMessagesCount = ticket.ClientUnreadMessagesCount ?? 0
            };
        }

        public static ApiTicket ToApiTicket(this fnTicket ticket, double timeZone )
        {
            return new ApiTicket
            {
               Id = ticket.Id,
               ClientId = ticket.ClientId,
               PartnerId = ticket.PartnerId,
               Status = ticket.Status,
               Subject = ticket.Subject,
               Type = ticket.Type,
               CreationTime = ticket.CreationTime.AddHours(timeZone)
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
                TicketId = ticketMessage.TicketId
            };
        }       

        #endregion

        #region fnTransaction

        public static TransactionModel MapToTransactionModel(this fnTransaction transaction, List<BllAccountType> accountTypes, double timeZone)
        {
            return new TransactionModel
            {
                Id = transaction.Id,
                AccountId = transaction.AccountId,
                AccountTypeName = accountTypes.FirstOrDefault(x => x.Id == transaction.AccountTypeId)?.Name,
                Amount = transaction.Amount,
                Type = transaction.Type,
                DocumentId = transaction.DocumentId,
                OperationTypeId = transaction.OperationTypeId,
                CreationTime = transaction.CreationTime.GetGMTDateFromUTC(timeZone),
                Info = transaction.Info,
                PartnerPaymentSettingId = transaction.PartnerPaymentSettingId,
                PaymentRequestId = transaction.PaymentRequestId,
                DocumentState = transaction.DocumentState,
                ClientId = transaction.ClientId,
                GameProviderId = transaction.GameProviderId,
                OperationTypeName = transaction.OperationTypeName,
                PaymentSystemName = transaction.PaymentSystemName,
                ProductName = transaction.ProductName,
                GameProviderName = transaction.GameProviderName,
                CurrencyId = transaction.CurrencyId,
                BalanceBefore = transaction.BalanceBefore,
                BalanceAfter = transaction.BalanceAfter
            };
        }

        public static BetModel MapToBetModel(this fnInternetBet bet, double timeZone, string language)
        {
            var product = CacheManager.GetProductById(bet.ProductId);
            
            return new BetModel
            {
                BetDocumentId = bet.BetDocumentId,
                State = bet.State,
                BetDate = bet.BetDate.GetGMTDateFromUTC(timeZone),
                WinDate = bet.WinDate.GetGMTDateFromUTC(timeZone),
                ClientId = bet.ClientId,
                BetAmount = bet.BetAmount,
                WinAmount = bet.WinAmount,
                BetTypeId = bet.BetTypeId,
                PossibleWin = bet.PossibleWin,
                ProductName = CacheManager.GetTranslation(product.TranslationId, language),
                ProductId = bet.ProductId,
                Profit = bet.BetAmount - bet.WinAmount,
                ProviderId = bet.GameProviderId ?? 0,
                SelectionsCount = bet.SelectionsCount
            };
        }

        #endregion

        #region Transaction

        public static Transaction MapToTransaction(this TransactionModel transactionModel)
        {
            return new Transaction
            {
                Id = transactionModel.Id,
                AccountId = transactionModel.AccountId,
                Amount = transactionModel.Amount,
                Type = transactionModel.Type,
                DocumentId = transactionModel.DocumentId,
                OperationTypeId = transactionModel.Type,
                CreationTime = transactionModel.CreationTime
            };
        }

        public static List<Transaction> MapToTransactions(this IEnumerable<TransactionModel> transactionModels)
        {
            return transactionModels.Select(MapToTransaction).ToList();
        }

        public static TransactionModel MapToTransactionModel(this Transaction transaction)
        {
            return new TransactionModel
            {
                Id = transaction.Id,
                AccountId = transaction.AccountId,
                Amount = transaction.Amount,
                Type = transaction.Type,
                DocumentId = transaction.DocumentId,
                OperationTypeId = transaction.Type,
                CreationTime = transaction.CreationTime
            };
        }

        public static List<TransactionModel> MapToTransactions(this IEnumerable<Transaction> transactions)
        {
            return transactions.Select(MapToTransactionModel).ToList();
        }

        public static ApiGetBetInfoOutput ToApiGetBetInfoOutput(this Document document)
        {
            var product = CacheManager.GetProductById(document.ProductId ?? 0);
            return new ApiGetBetInfoOutput
            {
                TransactionId = document.Id.ToString(),
                Barcode = document.Barcode,
                TicketNumber = document.TicketNumber?.ToString(),
                GameId = document.ProductId ?? 0,
                GameName = product.Name,
                BetAmount = document.Amount,
                BetDate = document.CreationTime,
                Status = document.State,
                PossibleWin = document.PossibleWin ?? 0,
                TypeId = document.TypeId ?? (int)BetTypes.Single
            };
        }

        #endregion

        #region PaymentRequest

        public static PaymentRequest MapToPaymentRequest(this PaymentRequestModel paymentRequestModel)
        {
            return new PaymentRequest
            {
                Id = paymentRequestModel.Id,
                ClientId = paymentRequestModel.ClientId,
                Amount = paymentRequestModel.Amount,
                CurrencyId = paymentRequestModel.CurrencyId,
                Status = paymentRequestModel.Status,
                BetShopId = paymentRequestModel.BetShopId,
                PaymentSystemId = paymentRequestModel.PaymentSystemId,
                Info = paymentRequestModel.Info,
                CreationTime = paymentRequestModel.CreationTime,
                LastUpdateTime = paymentRequestModel.LastUpdateTime,
                CashCode = paymentRequestModel.CashCode
            };
        }

        public static List<PaymentRequestModel> MapTofnPaymentRequestModels(this IEnumerable<fnPaymentRequest> paymentRequests, double timeZone,
            List<BllFnEnumeration> requestStates, List<PaymentRequestHistoryElement> historyItems)
        {
            return paymentRequests.Join(requestStates, pr => pr.Status, rs => rs.Value, 
                (pr, rs) => new { paymentRequest = pr, requestStateName = rs.Text }).
             Select(y => new PaymentRequestModel
             {
                 Id = y.paymentRequest.Id,
                 PartnerId = y.paymentRequest.PartnerId ?? 0,
                 ClientId = y.paymentRequest.ClientId.Value,
                 Amount = y.paymentRequest.Amount,
                 CurrencyId = y.paymentRequest.CurrencyId,
                 Status = y.paymentRequest.Status,
                 BetShopId = y.paymentRequest.BetShopId,
                 PaymentSystemId = y.paymentRequest.PaymentSystemId,
                 Info = string.IsNullOrEmpty(y.paymentRequest.Info) ? "{}" : 
                    JsonConvert.SerializeObject(JsonConvert.DeserializeObject<Common.Models.PaymentInfo>(y.paymentRequest.Info), new JsonSerializerSettings()
                    {
                         NullValueHandling = NullValueHandling.Ignore,
                         DefaultValueHandling = DefaultValueHandling.Ignore
                    }),
                 CreationTime = y.paymentRequest.CreationTime.GetGMTDateFromUTC(timeZone),
                 LastUpdateTime = y.paymentRequest.LastUpdateTime.GetGMTDateFromUTC(timeZone),
                 Type = y.paymentRequest.Type,
                 CashDeskId = y.paymentRequest.CashDeskId,
                 UserName = y.paymentRequest.UserName,
                 FirstName = y.paymentRequest.FirstName,
                 LastName = y.paymentRequest.LastName,
                 GroupId = y.paymentRequest.CategoryId ?? 0,
                 ParentId = y.paymentRequest.ParentId,
                 StatusName = y.requestStateName,
                 CommissionAmount = y.paymentRequest.CommissionAmount,
                 Comment = historyItems.FirstOrDefault(x => x.RequestId == y.paymentRequest.Id)?.Comment
             }).ToList();
        }

        public static PaymentRequestModel MapToPaymentRequestModel(this fnPaymentRequest paymentRequest, double timeZone)
        {
            return new PaymentRequestModel
            {
                Id = paymentRequest.Id,
                PartnerId = paymentRequest.PartnerId ?? 0,
                ClientId = paymentRequest.ClientId.Value,
                Amount = paymentRequest.Amount,
                CurrencyId = paymentRequest.CurrencyId,
                Status = paymentRequest.Status,
                BetShopId = paymentRequest.BetShopId,
                PaymentSystemId = paymentRequest.PaymentSystemId,
                Info = paymentRequest.Info,
                CreationTime = paymentRequest.CreationTime.GetGMTDateFromUTC(timeZone),
                LastUpdateTime = paymentRequest.LastUpdateTime.GetGMTDateFromUTC(timeZone),
                Type = paymentRequest.Type,
                CashDeskId = paymentRequest.CashDeskId,
                UserName = paymentRequest.UserName,
                FirstName = paymentRequest.FirstName,
                LastName = paymentRequest.LastName,
                GroupId = paymentRequest.CategoryId ?? 0
            };
        }

        public static PaymentRequestModel MapToPaymentRequestModel(this PaymentRequest paymentRequest)
        {
            return new PaymentRequestModel
            {
                Id = paymentRequest.Id,
                ClientId = paymentRequest.ClientId.Value,
                Amount = paymentRequest.Amount,
                CurrencyId = paymentRequest.CurrencyId,
                Status = paymentRequest.Status,
                BetShopId = paymentRequest.BetShopId,
                PaymentSystemId = paymentRequest.PaymentSystemId,
                Info = paymentRequest.Info,
                CreationTime = paymentRequest.CreationTime,
                LastUpdateTime = paymentRequest.LastUpdateTime,
                Type = paymentRequest.Type,
                CashDeskId = paymentRequest.CashDeskId
            };
        }

        public static ClientPaymentInfo MapToClientPaymentInfo(this ApiClientPaymentInfo info)
        {
            var type = (int)Enum.Parse(typeof(ClientPaymentInfoTypes), info.Type);
            return new ClientPaymentInfo
            {
                Id = info.Id,
                AccountNickName = info.NickName,
                ClientFullName = info.ClientName,
                CardNumber = info.CardNumber,
				WalletNumber = info.WalletNumber,
				CardExpireDate = info.CardExpireDate,
                BankName = info.BankName,
                BankIBAN = info.IBAN,
                BranchName = info.BranchName,
                BankAccountNumber = info.BankAccountNumber,
                BankAccountType = info.BankAccountType,
                Type = type
            };
        }

        public static ApiClientPaymentInfo ToApiClientPaymentInfo(this ClientPaymentInfo info)
        {
            return new ApiClientPaymentInfo
            {
                Id = info.Id,
                PaymentSystemId = info.PartnerPaymentSetting?.PaymentSystemId,
                NickName = info.AccountNickName,
                ClientName = info.ClientFullName,
                CardNumber = info.CardNumber,
                CardExpireDate = info.CardExpireDate,
                BankName = info.BankName,
                IBAN = info.BankIBAN,
                BankAccountNumber = info.BankAccountNumber,
                BankAccountType = info.BankAccountType,
                WalletNumber = info.WalletNumber,
                BranchName = info.BranchName,
                Type = ((ClientPaymentInfoTypes)info.Type).ToString(),
                State = info.State
            };
        }

        public static ApiPartnerBankInfo MapToApiPartnerBankInfo(this fnPartnerBankInfo partnerBankInfo)
        {
            return new ApiPartnerBankInfo
            {
                Id = partnerBankInfo.Id,
                BankName = partnerBankInfo.BankName,
                BankCode = partnerBankInfo.BankCode,
                OwnerName = partnerBankInfo.OwnerName,
                Accounts = partnerBankInfo.ClientPaymentInfos
                           .Select(x => new ApiClientPaymentInfo
                           {
                               BankAccountNumber = x.BankAccountNumber,
                               IBAN = x.BankIBAN,
                               Type = x.Type.ToString(),
                               OwnerName = partnerBankInfo.OwnerName
                           }).ToList()
            };
        }

        #endregion

        #region BetShop

        public static BetShop MapToBetShop(this BetShopModel betShopModel)
        {
            return new BetShop
            {
                Id = betShopModel.Id,
                CurrencyId = betShopModel.CurrencyId,
                Address = betShopModel.Address,
                PartnerId = betShopModel.PartnerId,
                State = betShopModel.State,
                Name = betShopModel.Name,
                Type = betShopModel.Type,
                RegionId = betShopModel.RegionId
            };
        }

        public static List<BetShop> MapToBetShops(this IEnumerable<BetShopModel> betShopModels)
        {
            return betShopModels.Select(MapToBetShop).ToList();
        }

        public static BetShopModel MapToBetShopModel(this BetShop betShop)
        {
            return new BetShopModel
            {
                Id = betShop.Id,
                CurrencyId = betShop.CurrencyId,
                Address = betShop.Address,
                PartnerId = betShop.PartnerId,
                State = betShop.State,
                Name = betShop.Name,
                Type = betShop.Type,
                RegionId = betShop.RegionId
            };
        }

        public static List<BetShopModel> MapToBetShopModels(this IEnumerable<BetShop> betShops)
        {
            return betShops.Select(MapToBetShopModel).ToList();
        }

        #endregion

        #region BetShop

        public static fnAccount MapToAccount(this AccountModel accountModel)
        {
            return new fnAccount
            {
                Id = accountModel.Id,
                TypeId = accountModel.TypeId,
                Balance = accountModel.Balance,
                CurrencyId = accountModel.CurrencyId,
                AccountTypeName = accountModel.AccountTypeName,
                CreationTime = accountModel.CreationTime
            };
        }

        public static AccountModel MapToAccountModel(this fnAccount account, decimal percent)
        {
            return new AccountModel
            {
                Id = account.Id,
                TypeId = account.TypeId,
                Balance = (account.TypeId == (int)AccountTypes.ClientCoinBalance || account.TypeId == (int)AccountTypes.ClientCompBalance) ?
                           Math.Truncate(account.Balance) : Math.Floor(account.Balance * 100) / 100,
                WithdrawableBalance = Math.Floor(account.TypeId == (int)AccountTypes.ClientUnusedBalance ? account.Balance * (100 - percent) :
                    ((account.TypeId == (int)AccountTypes.ClientBonusBalance || account.TypeId == (int)AccountTypes.ClientBooking ||
                      account.TypeId == (int)AccountTypes.ClientCompBalance || account.TypeId == (int)AccountTypes.ClientCoinBalance) ? 0 : account.Balance * 100)) / 100,
                CurrencyId = (account.TypeId == (int)AccountTypes.ClientCoinBalance || account.TypeId == (int)AccountTypes.ClientCompBalance) ?
                             string.Empty : account.CurrencyId,
                AccountTypeName = account.PaymentSystemId != null ? account.PaymentSystemName + " Wallet - " + account.BetShopName :
                    account.BetShopId != null ? "Shop Wallet - " + account.BetShopName : account.AccountTypeName, //make translatable later
                BetShopId = account.BetShopId,
                PaymentSystemId = account.PaymentSystemId,
                PaymentSystemName = account.PaymentSystemName,
                CreationTime = account.CreationTime
            };
        }

        #endregion

        #region Region

        public static fnRegion MapToRegion(this RegionModel regionModel)
        {
            return new fnRegion
            {
                Id = regionModel.Id,
                //ParentId = regionModel.ParentId,
                //TypeId = regionModel.TypeId,
                Name = regionModel.Name,
                //Path = regionModel.Path
            };
        }

        public static List<fnRegion> MapToRegions(this IEnumerable<RegionModel> regionModels)

        {
            return regionModels.Select(MapToRegion).ToList();
        }

        public static RegionModel MapToRegionModel(this fnRegion region)
        {
            return new RegionModel
            {
                Id = region.Id,
                //ParentId = region.ParentId,
                //TypeId = region.TypeId,
                Name = region.Name,
                NickName = region.NickName,
                IsoCode = region.IsoCode,
                IsoCode3 = region.IsoCode3,
                //Path = region.Path
            };
        }

        public static List<RegionModel> MapToRegionModels(this IEnumerable<fnRegion> regions)
        {
            return regions.Select(MapToRegionModel).ToList();
        }

        #endregion

        #region PartnerPaymentSystem

        public static fnPartnerPaymentSetting MapToPartnerPaymentSetting(this PartnerPaymentSettingModel partnerPaymentSettingModel)
        {
            return new fnPartnerPaymentSetting
            {
                Id = partnerPaymentSettingModel.Id,
                PartnerId = partnerPaymentSettingModel.PartnerId,
                PaymentSystemId = partnerPaymentSettingModel.PaymentSystemId,
                Commission = partnerPaymentSettingModel.Commission,
                State = partnerPaymentSettingModel.State,
                CurrencyId = partnerPaymentSettingModel.CurrencyId,
                CreationTime = partnerPaymentSettingModel.CreationTime,
                PaymentSystemName = partnerPaymentSettingModel.PaymentSystemName,
                Type = partnerPaymentSettingModel.Type,
                MinAmount= partnerPaymentSettingModel.MinAmount,
                MaxAmount = partnerPaymentSettingModel.MaxAmount
            };
        }

        public static List<fnPartnerPaymentSetting> MapToPartnerPaymentSettings(this IEnumerable<PartnerPaymentSettingModel> partnerPaymentSettingModels)
        {
            return partnerPaymentSettingModels.Select(MapToPartnerPaymentSetting).ToList();
        }

        public static PartnerPaymentSettingModel MapToPartnerPaymentSettingsModel(this fnPartnerPaymentSetting fnPartnerPaymentSetting)
        {
            return new PartnerPaymentSettingModel
            {
                Id = fnPartnerPaymentSetting.Id,
                PartnerId = fnPartnerPaymentSetting.PartnerId,
                PaymentSystemId = fnPartnerPaymentSetting.PaymentSystemId,
                Commission = fnPartnerPaymentSetting.Commission,
                FixedFee = fnPartnerPaymentSetting.FixedFee,
                Type = fnPartnerPaymentSetting.Type,
                State = fnPartnerPaymentSetting.State,
                CurrencyId = fnPartnerPaymentSetting.CurrencyId,
                CreationTime = fnPartnerPaymentSetting.CreationTime,
                PaymentSystemName = fnPartnerPaymentSetting.PaymentSystemName,
                PaymentSystemType = fnPartnerPaymentSetting.Type == (int)PaymentSettingTypes.Withdraw ? fnPartnerPaymentSetting.PaymenSystemType % 100 : (fnPartnerPaymentSetting.PaymenSystemType / 100) % 100,
                PaymentSystemPriority = fnPartnerPaymentSetting.PaymentSystemPriority,
                ContentType = fnPartnerPaymentSetting.Type == (int)PaymentRequestTypes.Deposit ?
                             (fnPartnerPaymentSetting.ContentType.Value / 10 == 0 ? (int)OpenModes.Iframe : fnPartnerPaymentSetting.ContentType.Value / 10 ) :
                             (fnPartnerPaymentSetting.ContentType.Value % 10 == 0 ? (int)OpenModes.StatusMessage : fnPartnerPaymentSetting.ContentType.Value % 10),
                Info = fnPartnerPaymentSetting.Info,
                ImageExtension = fnPartnerPaymentSetting.ImageExtension,
                MinAmount = fnPartnerPaymentSetting.MinAmount,
                MaxAmount = fnPartnerPaymentSetting.MaxAmount
            };
        }

        #endregion

        #region Region

        public static fnOperationType MapToOperationType(this OperationTypeModel operationTypeModel)
        {
            return new fnOperationType
            {
                Id = operationTypeModel.Id,
                NickName = operationTypeModel.NickName,
                TranslationId = operationTypeModel.NameId,
                Name = operationTypeModel.Name
            };
        }

        public static List<fnOperationType> MapToOperationTypes(this IEnumerable<OperationTypeModel> operationTypeModels)
        {
            return operationTypeModels.Select(MapToOperationType).ToList();
        }

        public static OperationTypeModel MapToOperationTypeModel(this fnOperationType operationType)
        {
            return new OperationTypeModel
            {
                Id = operationType.Id,
                NickName = operationType.NickName,
                NameId = operationType.TranslationId,
                Name = operationType.Name
            };
        }

        public static List<OperationTypeModel> MapToOperationTypeModels(this IEnumerable<fnOperationType> operationTypes)
        {
            return operationTypes.Select(MapToOperationTypeModel).ToList();
        }

        #endregion

        #region Partner

        public static ApiGetPartnerByIdOutput MapToApiGetPartnerByIdOutput(this BllPartner partner)
        {
            return new ApiGetPartnerByIdOutput
            {
                Id = partner.Id,
                Name = partner.Name,
                CurrencyId = partner.CurrencyId,
                SiteUrl = partner.SiteUrl,
                AdminSiteUrl = partner.AdminSiteUrl,
                State = partner.State,
                SessionId = partner.SessionId,
                CreationTime = partner.CreationTime,
                LastUpdateTime = partner.LastUpdateTime,
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
                ClientSessionExpireTime = partner.ClientSessionExpireTime
            };
        }

        #endregion

        #region Enumerations

        public static ApiEnumeration MapToApiEnumeration(this BllFnEnumeration enumeration)
        {
            return new ApiEnumeration
            {
                Name = enumeration.Text,
                Value = enumeration.Value
            };
        }

		#endregion

		#region Bonus

		public static ApiClientBonusItem ToApiClientBonusItem(this fnClientBonus bonus, double timeZone, string languageId)
		{
            var awardingTime = bonus.AwardingTime ?? bonus.CreationTime.AddHours(bonus.ValidForAwarding ?? 0);
            List<string> connectedBonuses = null;
            if(bonus.Type == (int)BonusTypes.SpinWheel)
            {
                var bonuses = bonus.Info.Split(',').Select(x => Convert.ToInt32(x)).ToList();
                connectedBonuses = new List<string>();
                foreach(var b in bonuses)
                {
                    var bs = CacheManager.GetBonusById(b);
                    var name = CacheManager.GetTranslation(bs.TranslationId, languageId);
                    connectedBonuses.Add(name);
                }
            }
            return new ApiClientBonusItem
            {
                Id = bonus.Id,
                BonusId = bonus.BonusId,
                Name = bonus.Name,
                Amount = bonus.BonusPrize,
                TypeId = bonus.Type,
                StatusId = bonus.Status,
                TurnoverCount = bonus.TurnoverCount,
                TurnoverAmountLeft = bonus.TurnoverAmountLeft,
                FinalAmount = bonus.FinalAmount,
                AwardingTime = awardingTime.GetGMTDateFromUTC(timeZone),
                CalculationTime = (bonus.CalculationTime ?? (bonus.AwardingTime == null ? (DateTime?)null : 
                    awardingTime.AddHours(bonus.ValidForSpending ?? 0))).GetGMTDateFromUTC(timeZone),
                ReuseNumber = bonus.ReuseNumber,
                ConnectedBonuses = connectedBonuses
            };
		}

        public static ApiBonus ToApiBonus(this BllBonus bonus, double timeZone)
        {
            return new ApiBonus
            {
                Id = bonus.Id,
                Name = bonus.Name,
                PartnerId = bonus.PartnerId,
                Status = bonus.Status,
                StartTime = bonus.StartTime.GetGMTDateFromUTC(timeZone),
                FinishTime = bonus.FinishTime.GetGMTDateFromUTC(timeZone),
                Type = bonus.Type,
                Info = bonus.Info,
                Sequence = bonus.Sequence
            };
        }

        public static ApiLeaderboardItem ToApiLeaderboardItem(this BllLeaderboardItem item, string currencyId)
        {
            var client = CacheManager.GetClientById(item.Id);
            return new ApiLeaderboardItem
            {
                Name = string.IsNullOrEmpty(client.FirstName) ? client.Id.ToString() : client.FirstName,
                Points = Math.Round(BaseBll.ConvertCurrency(item.CurrencyId, currencyId, item.Points))
            };
        }

        #endregion

        #region WebSite

        private static ApiMenuItem ToApiMenuItem(this BllMenuItem input)
		{
			return new ApiMenuItem
			{
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

		private static ApiSubMenuItem ToApiSubMenuItem(this BllSubMenuItem input)
		{
			return new ApiSubMenuItem
			{
				Icon = input.Icon,
				Title = input.Title,
				Type = input.Type,
				Href = input.Href,
				OpenInRouting = input.OpenInRouting,
				Order = input.Order
			};
		}

        #endregion

        public static ApiPartnerProductSetting ToApiPartnerProductSetting(this BllPartnerProductSetting input)
        {
            return new ApiPartnerProductSetting
            {
                Id = input.Id,
                PartnerId = input.PartnerId,
                ProductId = input.ProductId,
                Percent = input.Percent,
                State = input.State,
                Rating = input.Rating,
                HasDemo = input.HasDemo
            };
        }

        public static ApiProduct ToApiProduct(this fnProduct input)
        {
            return new ApiProduct
            {
                Id = input.Id,
                TranslationId = input.TranslationId,
                GameProviderId = input.GameProviderId,
                Level = input.Level,
                Name = input.Name,
                Description = input.NickName,
                ParentId = input.ParentId,
                ExternalId = input.ExternalId,
                State = input.State,
                MobileImageUrl = input.MobileImageUrl,
                WebImageUrl = input.WebImageUrl,
                BackgroundImageUrl = input.BackgroundImageUrl
            };
        }

        public static ApiGameProvider ToApiGameProvider(this BllGameProvider input)
        {
            return new ApiGameProvider
            {
                Id = input.Id,
                Name = input.Name,
                Type = input.Type,
                SessionExpireTime = input.SessionExpireTime,
                GameLaunchUrl = input.GameLaunchUrl
            };
        }

        public static ApiBalance ToApiBalance(this BllClientBalance balance)
        {
            return new ApiBalance
            {
                AvailableBalance = balance.AvailableBalance,
                Balances = balance.Balances.Select(x => new ApiAccount
                {
                    TypeId = x.TypeId,
                    CurrencyId = x.CurrencyId,
                    Balance = x.Balance
                }).ToList()
            };
        }

        #endregion

        #region Filters

        #region FilterClient

        public static FilterfnPartnerPaymentSetting MapToFilterPartnerPaymentSystem(this ApiFilterPartnerPaymentSetting filterPartnerPaymentSystem)
        {
            return new FilterfnPartnerPaymentSetting
            {
                Id = filterPartnerPaymentSystem.Id,
                PartnerId = filterPartnerPaymentSystem.PartnerId,
                PaymentSystemId = filterPartnerPaymentSystem.PaymentSystemId,
                Status = filterPartnerPaymentSystem.Status,
                CreatedFrom = filterPartnerPaymentSystem.CreatedFrom,
                CreatedBefore = filterPartnerPaymentSystem.CreatedBefore,
                TakeCount = filterPartnerPaymentSystem.TakeCount,
                SkipCount = filterPartnerPaymentSystem.SkipCount
            };
        }

        public static List<FilterfnPartnerPaymentSetting> MapToFilterPartnerPaymentSystems(this IEnumerable<ApiFilterPartnerPaymentSetting> filterPartnerPaymentSystems)
        {
            return filterPartnerPaymentSystems.Select(MapToFilterPartnerPaymentSystem).ToList();
        }

        #endregion

        #region FilterClient

        public static FilterClient MaptToFilterClient(this ApiFilterClient filterClient)
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
                CreatedFrom = filterClient.CreatedFrom,
                CreatedBefore = filterClient.CreatedBefore,
                TakeCount = filterClient.TakeCount,
                SkipCount = filterClient.SkipCount
            };
        }

        public static List<FilterClient> MapToFilterClients(this IEnumerable<ApiFilterClient> filterClients)
        {
            return filterClients.Select(MaptToFilterClient).ToList();
        }

        #endregion
        
        #region FilterPaymentRequest

        public static FilterfnPaymentRequest MapToFilterPaymentRequest(this ApiFilterPaymentRequest filterPaymentRequest)
        {
			var fromDate = filterPaymentRequest.CreatedFrom.GetUTCDateFromGmt(filterPaymentRequest.TimeZone);
			var toDate = filterPaymentRequest.CreatedBefore.GetUTCDateFromGmt(filterPaymentRequest.TimeZone);

			return new FilterfnPaymentRequest
            {
                FromDate = fromDate == null ? 0 : (long)fromDate.Value.Year * 100000000 + (long)fromDate.Value.Month * 1000000 + (long)fromDate.Value.Day * 10000 +
                                                  (long)fromDate.Value.Hour * 100 + fromDate.Value.Minute,
                ToDate = toDate == null ? 0 : (long)toDate.Value.Year * 100000000 + (long)toDate.Value.Month * 1000000 + (long)toDate.Value.Day * 10000 +
                                              (long)toDate.Value.Hour * 100 + toDate.Value.Minute,
                Type = filterPaymentRequest.Type,
                Ids = new FiltersOperation
                {
                    IsAnd = true,
                    OperationTypeList = filterPaymentRequest.Id == null ? new List<FiltersOperationType>() : 
                    new List<FiltersOperationType> { new FiltersOperationType { OperationTypeId = (int)FilterOperations.IsEqualTo, IntValue = filterPaymentRequest.Id.Value } }
                },
                ClientIds = new FiltersOperation
                {
                    IsAnd = true,
                    OperationTypeList = new List<FiltersOperationType> { new FiltersOperationType { OperationTypeId = (int)FilterOperations.IsEqualTo, IntValue = filterPaymentRequest.ClientId } }
                },
                PartnerPaymentSettingIds = new FiltersOperation
                {
                    IsAnd = true,
                    OperationTypeList = filterPaymentRequest.PartnerPaymentSettingId == null ? new List<FiltersOperationType>() :
                    new List<FiltersOperationType> { new FiltersOperationType { OperationTypeId = (int)FilterOperations.IsEqualTo, IntValue = filterPaymentRequest.PartnerPaymentSettingId.Value } }
                },
                PaymentSystemIds = new FiltersOperation
                {
                    IsAnd = true,
                    OperationTypeList = filterPaymentRequest.PaymentSystemId == null ? new List<FiltersOperationType>() :
                    new List<FiltersOperationType> { new FiltersOperationType { OperationTypeId = (int)FilterOperations.IsEqualTo, IntValue = filterPaymentRequest.PaymentSystemId.Value } }
                },
                Currencies = new FiltersOperation
                {
                    IsAnd = true,
                    OperationTypeList = string.IsNullOrEmpty(filterPaymentRequest.CurrencyId) ? new List<FiltersOperationType>() :
                    new List<FiltersOperationType> { new FiltersOperationType { OperationTypeId = (int)FilterOperations.IsEqualTo, StringValue = filterPaymentRequest.CurrencyId } }
                },
                States = new FiltersOperation
                {
                    IsAnd = false,
                    OperationTypeList = filterPaymentRequest.Statuses == null ? new List<FiltersOperationType>() :
                    filterPaymentRequest.Statuses.Select(x => new FiltersOperationType { OperationTypeId = (int)FilterOperations.IsEqualTo, IntValue = x } ).ToList()       
                },
                BetShopIds = new FiltersOperation
                {
                    IsAnd = true,
                    OperationTypeList = filterPaymentRequest.BetShopId == null ? new List<FiltersOperationType>() :
                    new List<FiltersOperationType> { new FiltersOperationType { OperationTypeId = (int)FilterOperations.IsEqualTo, IntValue = filterPaymentRequest.BetShopId.Value } }
                },
                SkipCount = filterPaymentRequest.SkipCount,
                TakeCount = filterPaymentRequest.TakeCount
            };
        }

        public static List<FilterfnPaymentRequest> MapToFilterPaymentRequests(this IEnumerable<ApiFilterPaymentRequest> filterPaymentRequests)
        {
            return filterPaymentRequests.Select(MapToFilterPaymentRequest).ToList();
        }

        #endregion

        #region FilterTransaction

        public static FilterTransaction MaptToFilterTransaction(this ApiFilterTransaction input)
        {
            return new FilterTransaction
            {
                Id = input.Id,
                DocumentId = input.DocumentId,
                CurrencyId = input.CurrencyId,
                AccountIds = input.AccountIds,
                OperationTypeId = input.OperationTypeId,
                OperationTypeIds = input.OperationTypeIds,
                FromDate = input.CreatedFrom,
                ToDate = input.CreatedBefore,
                SkipCount = input.SkipCount,
                TakeCount = input.TakeCount
            };
        }

        public static List<FilterTransaction> MaptToFilterTransactions(this IEnumerable<ApiFilterTransaction> filterTransactions)
        {
            return filterTransactions.Select(MaptToFilterTransaction).ToList();
        }

        #endregion

        #region FilterFnTransaction

        public static FilterFnTransaction MaptToFilterFnTransaction(this ApiFilterTransaction input)
        {
            return new FilterFnTransaction
            {
                Id = input.Id,
                ObjectTypeId = input.ObjectTypeId,
                ObjectId = input.ObjectId,
                DocumentId = input.DocumentId,
                CurrencyId = input.CurrencyId,
                AccountIds = input.AccountIds,
                OperationTypeId = input.OperationTypeId,
                OperationTypeIds = input.OperationTypeIds,
                FromDate = input.CreatedFrom.GetUTCDateFromGmt(input.TimeZone),
                ToDate = input.CreatedBefore.GetUTCDateFromGmt(input.TimeZone),
                SkipCount = input.SkipCount,
                TakeCount = input.TakeCount
            };
        }

        public static List<FilterFnTransaction> MaptToFilterFnTransactions(this IEnumerable<ApiFilterTransaction> input)
        {
            return input.Select(MaptToFilterFnTransaction).ToList();
        }

        public static FilterWebSiteBet MaptToFilterWebSiteBet(this ApiFilterInternetBet filterInternetBet)
        {
            return new FilterWebSiteBet
			{
                FromDate = filterInternetBet.CreatedFrom.GetUTCDateFromGmt(filterInternetBet.TimeZone),
                ToDate = filterInternetBet.CreatedBefore.GetUTCDateFromGmt(filterInternetBet.TimeZone),
                ClientId = filterInternetBet.ClientId,
                ProductIds = filterInternetBet.ProductIds,
                State = filterInternetBet.Status,
                GroupId = filterInternetBet.GroupId,
                SkipCount = filterInternetBet.SkipCount,
                TakeCount = (filterInternetBet.TakeCount == 0 ? 20 : Math.Min(20, filterInternetBet.TakeCount))
            };
        }

		#endregion

		#endregion

		#region Character

		public static ApiCharacter MapToApiCharacter(this BllCharacter character, bool IsForMobile = false)
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
				ImageData = character.ImageData,
				BackgroundImageData = IsForMobile ? character.BackgroundImageData?.Replace("/assets/images/characters/background/", "/assets/images/characters/background/mobile/") : character.BackgroundImageData,
				CompPoints = character.CompPoints
			};
		}

		#endregion
	}
}