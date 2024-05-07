using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.Common.Models.WebSiteModels.Clients;
using IqSoft.CP.Common.Models.WebSiteModels.ComplimentaryPoint;
using IqSoft.CP.Common.Models.WebSiteModels.Filters;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Filters;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.DAL.Models.Clients;
using IqSoft.CP.DAL.Models.Notification;
using IqSoft.CP.Integration.Payments.Helpers;
using IqSoft.CP.Integration.Platforms.Helpers;
using IqSoft.CP.MasterCacheWebApi.Helpers;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static IqSoft.CP.Common.Constants;

namespace IqSoft.CP.MasterCacheWebApi.ControllerClasses
{
    public static class ClientController
    {
        public static ApiResponseBase CallFunction(RequestBase request, SessionIdentity session, ILog log)
        {
            switch (request.Method)
            {
                case "ChangeClientDetails":
                    return ChangeClientDetails(JsonConvert.DeserializeObject<ChangeClientFieldsInput>(request.RequestData), request.ClientId, session, log);
                case "ChangeClientPassword":
                    return ChangeClientPassword(JsonConvert.DeserializeObject<ChangeClientPasswordInput>(request.RequestData), request.ClientId, session, log);
                case "ChangeNickName":
                    return ChangeNickName(request.RequestData, request.ClientId, session, log);
                case "GetClientAccounts":
                    return GetClientAccounts(request.ClientId, session, log);
                case "GetClientStates":
                    return GetClientStates(request.ClientId, session, log);
                case "GetBetShopsByClientId": //-
                    return GetBetShopsByClientId(request.ClientId, session, log);
                case "ChangeClientUSSDPin": //-
                    return ChangeClientUSSDPin(JsonConvert.DeserializeObject<ChangeClientPasswordInput>(request.RequestData), request.ClientId, session, log);
                case "LogoutClient":
                    return LogoutClient(request.Token, session, log);
                case "SendVerificationCodeToMobileNumber":
                    return SendVerificationCodeToMobileNumber(JsonConvert.DeserializeObject<SendVerificationCodeInput>(request.RequestData),
                                                              request.ClientId, session, log);
                case "SendVerificationCodeToEmail":
                    return SendVerificationCodeToEmail(JsonConvert.DeserializeObject<SendVerificationCodeInput>(request.RequestData), request.ClientId, session, log);
                case "VerifyClientEmail":
                    return VerifyClientEmail(JsonConvert.DeserializeObject<VerifyClientInput>(request.RequestData), request.ClientId, session, log);
                case "VerifyClientMobileNumber":
                    return VerifyClientMobileNumber(JsonConvert.DeserializeObject<VerifyClientInput>(request.RequestData), request.ClientId, session, log);
                case "OpenTicket":
                    return OpenTicket(request.ClientId, request.PartnerId, JsonConvert.DeserializeObject<ApiOpenTicketInput>(request.RequestData), session, log);
                case "CreateMessage":
                    return CreateMessage(JsonConvert.DeserializeObject<ApiCreateMessageInput>(request.RequestData), session, log);
                case "GetClientTickets":
                    return GetClientTickets(request.ClientId, request.PartnerId, JsonConvert.DeserializeObject<GetTicketsInput>(request.RequestData), session, log);
                case "GetTicketMessages":
                    return GetTicketMessages(Convert.ToInt32(request.RequestData), session, log);
                case "DeleteTicket":
                    return DeleteTicket(Convert.ToInt32(request.RequestData), session, log);
                case "CloseTicket":
                    return CloseTicket(Convert.ToInt32(request.RequestData), session, log);
                case "GetAnnouncements":
                    return GetAnnouncements(JsonConvert.DeserializeObject<ApiFilterClientAnnouncement>(request.RequestData), request.ClientId, session, log);
                case "CreateProductLimitByClient"://-
                    return CreateProductLimitByClient(JsonConvert.DeserializeObject<DAL.Models.ProductLimit>(request.RequestData), session, log);
                case "SetPaymentLimit"://-
                    return SetPaymentLimit(JsonConvert.DeserializeObject<ApiPaymentLimit>(request.RequestData), request.ClientId, session, log);
                case "GetPaymentLimits"://??
                    return GetPaymentLimits(request.PartnerId, session, log);
                case "GetDepositLimits"://-
                    return GetDepositLimits(request.ClientId, session, log);
                case "GetPaymentLimitExclusion"://-
                    return GetPaymentLimitExclusion(request.ClientId, session, log);
                case "SetProductLimit"://-
                    return SetProductLimit(JsonConvert.DeserializeObject<DAL.Models.ProductLimit>(request.RequestData), request.ClientId, session, log);
                case "GetLimits":
                    return GetLimits(request.ClientId, session, log);
                case "SetLimits":
                    return SetLimits(JsonConvert.DeserializeObject<ClientCustomSettings>(request.RequestData), request.ClientId, session, log);
                case "ApplySelfExclusion":
                    return ApplySelfExclusion(JsonConvert.DeserializeObject<ApiExclusionInput>(request.RequestData), request.ClientId, true, session, log);
                case "ExcludeFor24Hours":
                    return ApplySelfExclusion(new ApiExclusionInput { Date = DateTime.UtcNow.AddHours(24) }, request.ClientId, false, session, log);
                case "GetPartnerCustomerBanks":
                    return GetPartnerCustomerBanks(request.ClientId, Convert.ToInt32(request.RequestData), session, log);
                case "SavePayoutLimit":
                    return SavePayoutLimit(request.ClientId, Convert.ToDecimal(request.RequestData), session, log);
                case "GetPayoutLimit":
                    var payoutLimit = CacheManager.GetClientSettingByName(request.ClientId, ClientSettings.PayoutLimit);
                    return new ApiResponseBase
                    {
                        ResponseObject = new { Limit = payoutLimit?.NumericValue, CreationDate = payoutLimit?.DateValue?.GetGMTDateFromUTC(session.TimeZone) }
                    };
                case "GetClientWallets":
                    return GetClientPaymentAccounts(request.ClientId, Int32.TryParse(request.RequestData, out int paymentSystemId) ? paymentSystemId : (int?)null,
                                                    (int)ClientPaymentInfoTypes.Wallet, session, log);
                case "GetClientCards":
                    return GetClientPaymentAccounts(request.ClientId, Int32.TryParse(request.RequestData, out paymentSystemId) ? paymentSystemId : (int?)null,
                                                    (int)ClientPaymentInfoTypes.CreditCard, session, log);
                case "GetClientBankAccounts":
                    return GetClientPaymentAccounts(request.ClientId, Int32.TryParse(request.RequestData, out paymentSystemId) ? paymentSystemId : (int?)null,
                                (int)ClientPaymentInfoTypes.BankAccount, session, log);
                case "AddBankAccount":
                    return AddBankAccount(JsonConvert.DeserializeObject<ApiClientPaymentInfo>(request.RequestData), request.ClientId, session, log);
                case "AddCardData":
                    return AddCardData(JsonConvert.DeserializeObject<ApiClientPaymentInfo>(request.RequestData), request.ClientId, session, log);
                case "AddWalletNumber":
                    return AddWalletNumber(JsonConvert.DeserializeObject<ApiClientPaymentInfo>(request.RequestData), request.ClientId, session, log);
                case "RemovePaymentAccount":
                    return RemovePaymentAccount(JsonConvert.DeserializeObject<ApiClientPaymentInfo>(request.RequestData), session, log);
                case "SendAccountDetailsVerificationCode": //--
                    return SendAccountDetailsVerificationCode(request.ClientId, session, log);
                case "GetClientPaymentInfoStates":
                    return GetClientPaymentInfoStatesEnum(nameof(ClientPaymentInfoStates), session);
                case "InviteFriend":
                    return InviteFriend(JsonConvert.DeserializeObject<ApiInviteFriendInput>(request.RequestData), session, log);
                case "GetVerificationPageUrl":
                    if (Int32.TryParse(request.RequestData, out int verificationPlatformId))
                        return GetVerificationPageUrl(verificationPlatformId, session, log);
                    return GetVerificationPageUrl(null, session, log);
                case "UploadImage":
                    return UploadImage(JsonConvert.DeserializeObject<AddClientIdentityModel>(request.RequestData), session, log);
                case "GetKYCDocumentTypesEnum":
                    return GetKYCDocumentTypesEnum(session, log);
                case "GetKYCDocumentStatesEnum":
                    return GetKYCDocumentStatesEnum(session, log);
                case "GetClientIdentityModels":
                    return GetClientIdentityModels(session, log);
                case "GetProductBalance": //??
                    return GetProductBalance(Convert.ToInt32(request.RequestData), request.ClientId, session, log);
                case "ActivatePromoCode":
                    return ActivatePromoCode(request.RequestData, request.ClientId, session, log);
                case "ClaimToCompainBonus":
                    return ClaimToCompainBonus(Convert.ToInt32(request.RequestData), request.ClientId, session, log);
                case "SpendComplimentaryPoints":
                    return SpendComplimentaryPoints(JsonConvert.DeserializeObject<ApiComplimentaryPoint>(request.RequestData), session, log);
                case "GetClientBonusTriggers":
                    return GetClientBonusTriggers(JsonConvert.DeserializeObject<ApiClientBonusItem>(request.RequestData), request.ClientId, session, log);
                case "AcceptTermsConditions":
                    return AcceptTermsConditions(request.ClientId, session, log);
                case "GetClientStatuses":
                    return GetClientStatuses(request.ClientId, session, log);
                case "GetSessionInfo": //-
                    return GetSessionInfo(session, log);
                case "UpdateSecurityAnswers":
                    return UpdateSecurityAnswers(request.ClientId, JsonConvert.DeserializeObject<ApiClientSecurityModel>(request.RequestData), session, log);
                case "GenerateQRCode":
                    return GenerateQRCode(session, log);
                case "EnableClientTwoFactor":
                    return EnableClientTwoFactor(JsonConvert.DeserializeObject<ApiQRCodeInput>(request.RequestData), session, log);
                case "DisableClientTwoFactor":
                    return DisableClientTwoFactor(request.RequestData, session, log);
                case "DisableClientAccount":
                    return DisableClientAccount(JsonConvert.DeserializeObject<ApiExclusionInput>(request.RequestData), session, log);
                case "SelectSessionAccount":
                    return SelectSessionAccount(Convert.ToInt64(request.RequestData), session, log);
                case "AddCharacterToClient":
                    return AddCharacterToClient(request.ClientId, Convert.ToInt32(request.RequestData), session, log);
                case "GetCharacterCurrentState":
                    return GetCharacterCurrentState(request.ClientId, request.PartnerId, session, log);
                case "ConfirmLimit":
                    return ConfirmLimit(request.ClientId, JsonConvert.DeserializeObject<ConfirmLimitInput>(request.RequestData), session, log);
                case "ViewPopup":
                    return ViewPopup(request.ClientId, JsonConvert.DeserializeObject<ApiPopupWeSiteModel>(request.RequestData), session, log);
                default:
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.MethodNotFound);
            }
        }

        private static ApiResponseBase ChangeClientDetails(ChangeClientFieldsInput input, int clientId, SessionIdentity session, ILog log)
        {
            using (var clientBl = new ClientBll(session, log))
            {
                var dbClient = clientBl.GetClientById(clientId, false);
                var changeClientFields = new ChangeClientFieldsInput
                {
                    ClientId = clientId,
                    CurrencyId = dbClient.CurrencyId,
                    BirthDate = input.BirthDate ?? dbClient.BirthDate,
                    Email = input.Email ?? dbClient.Email,
                    FirstName = input.FirstName ?? dbClient.FirstName,
                    LastName = input.LastName ?? dbClient.LastName,
                    NickName = input.NickName ?? dbClient.NickName,
                    SecondName = input.SecondName ?? dbClient.SecondName,
                    SecondSurname = input.SecondSurname ?? dbClient.SecondSurname,
                    DocumentNumber = input.DocumentNumber ?? dbClient.DocumentNumber,
                    DocumentIssuedBy = input.DocumentIssuedBy ?? dbClient.DocumentIssuedBy,
                    Address = input.Address ?? dbClient.Address,
                    MobileNumber = input.MobileNumber ?? dbClient.MobileNumber,
                    PhoneNumber = input.PhoneNumber ?? dbClient.PhoneNumber,
                    ZipCode = input.ZipCode ?? dbClient.ZipCode.Trim(),
                    LanguageId = input.LanguageId ?? dbClient.LanguageId,
                    Gender = input.Gender ?? dbClient.Gender,
                    SendPromotions = input.SendPromotions ?? dbClient.SendPromotions,
                    CallToPhone = input.CallToPhone ?? dbClient.CallToPhone,
                    SendMail = input.SendMail ?? dbClient.SendMail,
                    SendSms = input.SendSms ?? dbClient.SendSms,
                    RegionId = input.RegionId ?? dbClient.RegionId,
                    CountryId = input.CountryId ?? dbClient.CountryId,
                    CityId = input.CityId,
                    City = input.City ?? dbClient.City,
                    Info = input.Info ?? dbClient.Info,
                    TownId = input.TownId,
                    Citizenship = input.Citizenship ?? dbClient.Citizenship,
                    SecurityQuestions = input.SecurityQuestions
                };
                var client = clientBl.ChangeClientDataFromWebSite(changeClientFields);
                var clientLoginOut = new ClientLoginOut();
                clientBl.GetClientRegionInfo(client.RegionId, ref clientLoginOut);
                CacheManager.RemoveClientFromCache(clientId);
                Helpers.Helpers.InvokeMessage("RemoveClient", clientId);
                return new ApiResponseBase
                {
                    ResponseObject = client.MapToApiLoginClientOutput(session.TimeZone, clientLoginOut)
                };
            }
        }

        private static ApiResponseBase ChangeClientPassword(ChangeClientPasswordInput input, int clientId, SessionIdentity session, ILog log)
        {
            input.ClientId = clientId;
            using (var clientBl = new ClientBll(session, log))
            {
                clientBl.ChangeClientPassword(input);
                CacheManager.RemoveClientFromCache(clientId);
                Helpers.Helpers.InvokeMessage("RemoveClient", clientId);
                Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.ClientSettings,
                                              clientId, nameof(Constants.ClientSettings.PasswordChangedDate)));
                return new ApiResponseBase();
            }
        }

        private static ApiResponseBase ChangeClientUSSDPin(ChangeClientPasswordInput input, int clientId, SessionIdentity session, ILog log)
        {
            input.ClientId = clientId;
            using (var clientBl = new ClientBll(session, log))
            {
                clientBl.ChangeClientUSSDPin(input);
                CacheManager.RemoveClientFromCache(clientId);
                Helpers.Helpers.InvokeMessage("RemoveClient", clientId);
                Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.ClientSettings,
                                              clientId, nameof(Constants.ClientSettings.PasswordChangedDate)));
                return new ApiResponseBase();
            }
        }

        private static ApiResponseBase AcceptTermsConditions(int clientId, SessionIdentity session, ILog log)
        {
            using (var clientBl = new ClientBll(session, log))
            {
                var currentTime = DateTime.UtcNow;
                var client = CacheManager.GetClientById(clientId);
                var clientSettings = new ClientCustomSettings
                {
                    ClientId = clientId,
                    TermsConditionsAcceptanceVersion = CacheManager.GetConfigKey(client.PartnerId, Constants.PartnerKeys.TermsConditionVersion)
                };
                clientBl.SaveClientSetting(clientSettings);
                Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.ClientSettings,
                    clientId, nameof(clientSettings.TermsConditionsAcceptanceVersion)));
                return new ApiResponseBase();
            }
        }

        private static ApiResponseBase LogoutClient(string token, SessionIdentity session, ILog log)
        {
            using (var clientBl = new ClientBll(session, log))
            {
                var clientId = clientBl.LogoutClient(token);
                Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.ClientSessions, clientId));
                Helpers.Helpers.InvokeMessage("RemoveClient", clientId);
                var verificationPlatform = CacheManager.GetConfigKey(session.PartnerId, Constants.PartnerKeys.VerificationPlatform);
                if (!string.IsNullOrEmpty(verificationPlatform) && int.TryParse(verificationPlatform, out int verificationPatformId))
                {
                    switch (verificationPatformId)
                    {
                        case (int)VerificationPlatforms.Insic:
                            var thread = new Thread(() => InsicHelpers.PlayerLogout(session.PartnerId, clientId, WebApiApplication.DbLogger));
                            thread.Start();
                            break;
                        default:
                            break;
                    }
                }
                return new ApiResponseBase();
            }
        }

        public static ApiResponseBase GetClientAccounts(int clientId, SessionIdentity session, ILog log)
        {
            using (var baseBl = new ClientBll(session, log))
            {
                var accounts =
                    baseBl.GetfnAccounts(new FilterfnAccount
                    {
                        ObjectId = clientId,
                        ObjectTypeId = (int)ObjectTypes.Client
                    });
                var client = CacheManager.GetClientById(clientId);
                var partner = CacheManager.GetPartnerById(client.PartnerId);
                var clientSetting = CacheManager.GetClientSettingByName(clientId, Constants.ClientSettings.UnusedAmountWithdrawPercent);
                var uawp = partner.UnusedAmountWithdrawPercent;
                if (clientSetting != null && clientSetting.Id > 0 && clientSetting.NumericValue != null)
                    uawp = clientSetting.NumericValue.Value;
                var isExternalPlatformClient = ExternalPlatformHelpers.IsExternalPlatformClient(client, out DAL.Models.Cache.PartnerKey externalPlatformType);
                if (isExternalPlatformClient)
                {
                    var externalBalance = Math.Floor(ExternalPlatformHelpers.GetClientBalance(Convert.ToInt32(externalPlatformType.StringValue), clientId) * 100) / 100;
                    return new ApiResponseBase
                    {
                        ResponseObject = new GetClientAccountsOutput
                        {
                            Accounts = accounts.Where(x => x.TypeId == (int)AccountTypes.ClientUnusedBalance).Select(x => { x.Balance = externalBalance; return x.MapToAccountModel(uawp); }).ToList()
                        }
                    };
                }

                var result = accounts.Where(x => x.Status != (int)BaseStates.Inactive).Select(x => x.MapToAccountModel(uawp)).ToList();
                try
                {
                    result.AddRange(ExternalPlatformHelpers.GetExternalAccounts(client, session, log));
                }
                catch (Exception e)
                {
                    log.Error(e);
                }
                return new ApiResponseBase
                {
                    ResponseObject = new GetClientAccountsOutput
                    {
                        Accounts = result
                    }
                };
            }
        }

        private static ApiResponseBase SendVerificationCodeToMobileNumber(SendVerificationCodeInput input, int clientId, SessionIdentity session, ILog log)
        {
            using (var notificationBl = new NotificationBll(session, log))
            {
                var clientMobile = CacheManager.GetClientById(clientId).MobileNumber;
                if (string.IsNullOrEmpty(input.MobileNumber))
                    input.MobileNumber = clientMobile;
                var activePeriodInMinutes = notificationBl.SendVerificationCodeToMobileNumber(clientId, input.MobileNumber);
                if (clientMobile != input.MobileNumber)
                    Helpers.Helpers.InvokeMessage("RemoveClient", clientId);
                return new ApiResponseBase
                {
                    ResponseObject = new { ActivePeriodInMinutes = activePeriodInMinutes }
                };
            }
        }

        private static ApiResponseBase SendVerificationCodeToEmail(SendVerificationCodeInput input, int clientId, SessionIdentity session, ILog log)
        {
            using (var notificationBll = new NotificationBll(session, log))
            {
                var clientEmail = CacheManager.GetClientById(clientId).Email;
                if (string.IsNullOrEmpty(input.Email))
                    input.Email = clientEmail;
                var activePeriodInMinutes = notificationBll.SendVerificationCodeToEmail(clientId, input.Email);
                if (clientEmail != input.Email)
                    Helpers.Helpers.InvokeMessage("RemoveClient", clientId);
                return new ApiResponseBase
                {
                    ResponseObject = new { ActivePeriodInMinutes = activePeriodInMinutes }
                };
            }
        }

        private static ApiResponseBase VerifyClientEmail(VerifyClientInput input, int clientId, SessionIdentity session, ILog log)
        {
            using (var clientBl = new ClientBll(session, log))
            {
                var client = CacheManager.GetClientById(clientId);
                clientBl.VerifyClientEmail(input.Key, string.Empty, clientId, client.PartnerId, false, input.SecurityQuestions, (int)VerificationCodeTypes.EmailVerification);
                CacheManager.RemoveClientFromCache(clientId);
                Helpers.Helpers.InvokeMessage("RemoveClient", clientId);
                return new ApiResponseBase();
            }
        }

        private static ApiResponseBase VerifyClientMobileNumber(VerifyClientInput input, int clientId, SessionIdentity session, ILog log)
        {
            using (var clientBl = new ClientBll(session, log))
            {
                var client = CacheManager.GetClientById(clientId);
                clientBl.VerifyClientMobileNumber(input.Key, client.MobileNumber, clientId, client.PartnerId, false, input.SecurityQuestions, (int)VerificationCodeTypes.MobileNumberVerification);
                CacheManager.RemoveClientFromCache(clientId);
                Helpers.Helpers.InvokeMessage("RemoveClient", clientId);
                return new ApiResponseBase();
            }
        }

        public static ApiResponseBase GetClientStates(int clientId, SessionIdentity session, ILog log)
        {
            var client = CacheManager.GetClientById(clientId);
            if (client == null)
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.ClientNotFound);
            var messagesCount = CacheManager.GetClientUnreadTicketsCount(clientId).Count;
            return new ApiResponseBase
            {
                ResponseObject= new GetClientStatesOutput
                {
                    UnreadMessagesCount = messagesCount
                }
            };
        }

        public static ApiResponseBase GetBetShopsByClientId(int clientId, SessionIdentity session, ILog log)
        {
            using (var clientBl = new BetShopBll(session, log))
            {
                var betShops = clientBl.GetBetShopsByClientId(clientId);
                return new ApiResponseBase
                {
                    ResponseObject = new GetPartnerBetShopsOutput
                    {
                        BetShops = betShops.MapToBetShopModels()
                    }
                };
            }
        }

        private static ApiResponseBase CreateProductLimitByClient(DAL.Models.ProductLimit limit, SessionIdentity session, ILog log)
        {
            using (var clientBl = new ClientBll(session, log))
            {
                var result = clientBl.CreateProductLimitByClient(limit);
                var response = new ApiResponseBase
                {
                    ResponseObject = result
                };
                return response;
            }
        }

        private static ApiResponseBase SendAccountDetailsVerificationCode(int clientId, SessionIdentity session, ILog log)
        {
            using (var notificationBl = new NotificationBll(session, log))
            {
                var client = CacheManager.GetClientById(clientId);
                var partner = CacheManager.GetPartnerById(client.PartnerId);
                var partnerSetting = CacheManager.GetConfigKey(client.PartnerId, Constants.PartnerKeys.VerificationKeyNumberOnly);
                var verificationKey = (!string.IsNullOrEmpty(partnerSetting) && partnerSetting == "1") ? CommonFunctions.GetRandomNumber(partner.MobileVerificationCodeLength) :
                                                                                                         CommonFunctions.GetRandomString(partner.MobileVerificationCodeLength);
                notificationBl.SendNotificationMessage(new NotificationModel
                {
                    PartnerId = client.PartnerId,
                    ObjectId = client.Id,
                    ObjectTypeId = (int)ObjectTypes.Client,
                    MobileOrEmail = client.MobileNumber,
                    ClientInfoType = (int)ClientInfoTypes.AccountDetailsMobileKey,
                    VerificationCode = verificationKey
                }, out int responseCode);
                return new ApiResponseBase();
            }
        }

        private static ApiResponseBase GetClientPaymentAccounts(int clientId, int? paymentSystemId, int paymentAccountType, SessionIdentity session, ILog log)
        {
            using (var clientBl = new ClientBll(session, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = clientBl.GetClientPaymentAccountDetails(clientId, paymentSystemId, new List<int>(paymentAccountType), false)
                                             .Select(x => x.ToApiClientPaymentInfo()).ToList()
                };
            }
        }

        private static ApiResponseBase AddBankAccount(ApiClientPaymentInfo input, int clientId, SessionIdentity session, ILog log)
        {
            using (var clientBl = new ClientBll(session, log))
            {
                if (string.IsNullOrEmpty(input.BankAccountNumber) || string.IsNullOrEmpty(input.BankName))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongInputParameters);
                if (input.PaymentSystemId == null)
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PaymentSystemNotFound);

                input.Type = ((int)ClientPaymentInfoTypes.BankAccount).ToString();
                var pps = CacheManager.GetPartnerPaymentSettings(session.PartnerId, input.PaymentSystemId.Value, session.CurrencyId, (int)PaymentRequestTypes.Withdraw);
                if (pps == null || pps.Id == 0)
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PaymentSystemNotFound);
                var clientPaymentInfo = input.MapToClientPaymentInfo();
                clientPaymentInfo.ClientId = clientId;
                clientPaymentInfo.PartnerPaymentSystemId = pps.Id;
                var resp = clientBl.RegisterClientPaymentAccountDetails(clientPaymentInfo, input.Code, false);
                if (resp == null)
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.GeneralException);
                return new ApiResponseBase
                {
                    ResponseObject = resp.ToApiClientPaymentInfo()
                };
            }
        }

        private static ApiResponseBase AddCardData(ApiClientPaymentInfo input, int clientId, SessionIdentity session, ILog log)
        {
            using (var clientBl = new ClientBll(session, log))
            {
                var type = (int)Enum.Parse(typeof(ClientPaymentInfoTypes), input.Type);
                if (type != (int)ClientPaymentInfoTypes.CreditCard ||
                    string.IsNullOrEmpty(input.ClientName) || string.IsNullOrEmpty(input.CardNumber) || /*string.IsNullOrEmpty(input.Code) ||*/
                    !input.CardExpireDate.HasValue)
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongInputParameters);
                var clientPaymentInfo = input.MapToClientPaymentInfo();
                clientPaymentInfo.ClientId = clientId;
                var info = clientBl.RegisterClientPaymentAccountDetails(clientPaymentInfo, input.Code, false);
                if (info == null)
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.GeneralException);
                return new ApiResponseBase
                {
                    ResponseObject = info.ToApiClientPaymentInfo()
                };
            }
        }

        private static ApiResponseBase AddWalletNumber(ApiClientPaymentInfo input, int clientId, SessionIdentity session, ILog log)
        {
            if (!input.PaymentSystemId.HasValue)
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PaymentSystemNotFound);
            var paymentSystem = CacheManager.GetPaymentSystemById(input.PaymentSystemId.Value) ??
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PaymentSystemNotFound);
            var client = CacheManager.GetClientById(clientId);
            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentSystem.Id, client.CurrencyId, (int)PaymentRequestTypes.Withdraw);
            if (partnerPaymentSetting == null || partnerPaymentSetting.Id == 0)
            {
                partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentSystem.Id, client.CurrencyId, (int)PaymentRequestTypes.Deposit);
                if (partnerPaymentSetting == null || partnerPaymentSetting.Id == 0)
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.PaymentSystemNotFound);
            }
            using (var clientBl = new ClientBll(session, log))
            {
                var clientPaymentInfo = input.MapToClientPaymentInfo();
                clientPaymentInfo.ClientId = clientId;
                clientPaymentInfo.Type = (int)ClientPaymentInfoTypes.Wallet;
                ClientPaymentInfo info;
                switch (paymentSystem.Name)
                {
                    case PaymentSystems.OktoPay:
                        info = OktoPayHelpers.RegisterConsent(clientPaymentInfo, client.PartnerId, session, log);
                        break;
                    default:
                        info = clientBl.RegisterClientPaymentAccountDetails(clientPaymentInfo, input.Code, false);
                        break;
                }
                if (info == null)
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.GeneralException);
                return new ApiResponseBase
                {
                    ResponseObject = info.ToApiClientPaymentInfo()
                };
            }
        }

        private static ApiResponseBase RemovePaymentAccount(int accountId, SessionIdentity session, ILog log)
        {
            using (var clientBl = new ClientBll(session, log))
            {
                clientBl.DeleteClientPaymentInfo(session.Id, accountId);
                return new ApiResponseBase();
            }
        }

        private static ApiResponseBase RemovePaymentAccount(ApiClientPaymentInfo input, SessionIdentity session, ILog log)
        {
            using (var scope = CommonFunctions.CreateTransactionScope())
            {
                using (var clientBl = new ClientBll(session, log))
                {
                    using (var paymentSystemBl = new PaymentSystemBll(clientBl))
                    {
                        var clientPaymentInfo = clientBl.GetClientPaymentAccountDetailsById(input.Id);
                        if (clientPaymentInfo == null)
                            throw BaseBll.CreateException(session.LanguageId, Constants.Errors.AccountNotFound);
                        clientBl.DeleteClientPaymentInfo(session.Id, input.Id);
                        var partnerPaymentSettins = paymentSystemBl.GetPartnerPaymentSettingById(clientPaymentInfo.PartnerPaymentSystemId.Value, false);
                        var paymentSystem = CacheManager.GetPaymentSystemById(partnerPaymentSettins.PaymentSystemId);
                        switch (paymentSystem.Name)
                        {
                            case PaymentSystems.OktoPay:
                                OktoPayHelpers.RevokeConsent(partnerPaymentSettins, clientPaymentInfo.WalletNumber);
                                break;
                            default:
                                break;
                        }
                        scope.Complete();
                        return new ApiResponseBase();
                    }
                }
            }
        }

        private static ApiResponseBase OpenTicket(int clientId, int partnerId, ApiOpenTicketInput openTicketInput, SessionIdentity session, ILog log)
        {
            using (var clientBl = new ClientBll(session, log))
            {
                if (string.IsNullOrWhiteSpace(openTicketInput.Message) || string.IsNullOrWhiteSpace(openTicketInput.Subject))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongInputParameters);
                var ticket = new Ticket
                {
                    ClientId = clientId,
                    PartnerId = partnerId,
                    Type = (int)TicketTypes.Discussion,
                    Subject = openTicketInput.Subject,
                    Status = (int)MessageTicketState.Active
                };
                var message = new TicketMessage
                {
                    Message = openTicketInput.Message,
                    Type = (int)ClientMessageTypes.MessageFromClient
                };

                var resp = clientBl.OpenTickets(ticket, message, new List<int> { clientId }, null, false);
                return new ApiResponseBase
                {
                    ResponseObject = resp[0].ToApiTicket(session.TimeZone)
                };
            }
        }

        private static ApiResponseBase CreateMessage(ApiCreateMessageInput createMessageInput, SessionIdentity session, ILog log)
        {
            using (var clientBl = new ClientBll(session, log))
            {
                if (string.IsNullOrWhiteSpace(createMessageInput.Message))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongInputParameters);
                var message = new TicketMessage
                {
                    Message = createMessageInput.Message,
                    Type = (int)ClientMessageTypes.MessageFromClient,
                    TicketId = createMessageInput.TicketId,
                    CreationTime = clientBl.GetServerDate()
                };

                var resp = clientBl.AddMessageToTicket(message, out int clientId, out int unreadMessageCount);
                Helpers.Helpers.InvokeMessage("UpdateCacheItem", string.Format("{0}_{1}", Constants.CacheItems.ClientUnreadTicketsCount, clientId),
                                                                               new BllUnreadTicketsCount { Count = unreadMessageCount }, TimeSpan.FromHours(6));
                return new ApiResponseBase
                {
                    ResponseObject = resp.ToApiTicketMessage(session.TimeZone)
                };
            }
        }

        private static ApiResponseBase GetClientTickets(int clientId, int partnerId, GetTicketsInput ticketsInput, SessionIdentity session, ILog log)
        {
            using (var clientBl = new ClientBll(session, log))
            {
                var resp = clientBl.GetClientTickets(clientId, partnerId, ticketsInput.SkipCount, ticketsInput.TakeCount, ticketsInput.Statuses);
                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        Tickets = resp.Entities.Select(x => new
                        {
                            x.Id,
                            x.ClientId,
                            x.PartnerId,
                            x.Status,
                            x.Subject,
                            x.Type,
                            CreationTime = x.CreationTime.GetGMTDateFromUTC(session.TimeZone),
                            LastMessageTime = x.LastMessageTime.GetGMTDateFromUTC(session.TimeZone),
                            x.UnreadMessagesCount,
                            x.LastMessage
                        }),
                        resp.Count,
                        TotalUnreadMessagesCount = resp.Entities.Sum(x => x.UnreadMessagesCount)
                    }
                };
            }
        }

        private static ApiResponseBase GetTicketMessages(int ticketId, SessionIdentity session, ILog log)
        {
            using (var clientBl = new ClientBll(session, log))
            {
                var resp = clientBl.GetMessagesByTicket(ticketId, true);
                return new ApiResponseBase
                {
                    ResponseObject = resp.MapToTicketMessages(session.TimeZone)
                };
            }
        }

        private static ApiResponseBase DeleteTicket(int ticketId, SessionIdentity session, ILog log)
        {
            using (var clientBl = new ClientBll(session, log))
            {
                clientBl.ChangeTicketStatus(ticketId, session.Id, MessageTicketState.Deleted);
                return new ApiResponseBase
                {
                    ResponseObject = ticketId
                };
            }
        }

        private static ApiResponseBase CloseTicket(int ticketId, SessionIdentity session, ILog log)
        {
            using (var clientBl = new ClientBll(session, log))
            {
                clientBl.ChangeTicketStatus(ticketId, session.Id, MessageTicketState.Closed);
                return new ApiResponseBase();
            }
        }

        private static ApiResponseBase InviteFriend(ApiInviteFriendInput input, SessionIdentity session, ILog log)
        {
            using (var notificationBl = new NotificationBll(session, log))
            {
                if (!BaseBll.IsValidEmail(input.Email))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.InvalidEmail);
                notificationBl.SendInvitationToAffiliateClient(session.Id, input.Email);
                return new ApiResponseBase();
            }
        }

        private static ApiResponseBase GetVerificationPageUrl(int? verificationPatformId, SessionIdentity session, ILog log)
        {
            var client = CacheManager.GetClientById(session.Id);
            var url = string.Empty;
            if (!verificationPatformId.HasValue)
            {
                var verificationPlatform = CacheManager.GetConfigKey(client.PartnerId, Constants.PartnerKeys.VerificationPlatform);
                if (!string.IsNullOrEmpty(verificationPlatform) && int.TryParse(verificationPlatform, out int platformId))
                    verificationPatformId = platformId;
            }
            if (verificationPatformId.HasValue)

                switch (verificationPatformId)
                {
                    case (int)VerificationPlatforms.Insic:
                    case (int)VerificationPlatforms.SOW:
                        url = InsicHelpers.GetVerificationWidget(verificationPatformId.Value, client.PartnerId, client.Email, session.Domain, session.LanguageId, log);
                        break;
                    default:
                        break;
                }
            return new ApiResponseBase
            {
                ResponseObject = url
            };
        }

        private static ApiResponseBase UploadImage(AddClientIdentityModel input, SessionIdentity session, ILog log)
        {
            if (string.IsNullOrEmpty(input.ImageData))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongInputParameters);
            input.ClientId = session.Id;
            using (var clientBl = new ClientBll(session, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        ClientIdentityData = clientBl.SaveKYCDocument(input.ToClientIdentity(), input.Name, Convert.FromBase64String(input.ImageData), false)
                                                     .ToClientIdentityModel(session.TimeZone)
                    }
                };
            }
        }

        private static ApiResponseBase GetKYCDocumentTypesEnum(SessionIdentity identity, ILog log)
        {
            using (var clientBl = new ClientBll(identity, log))
            {
                var client = CacheManager.GetClientById(identity.Id); List<int> partnerKYCDocumentTypes = null;
                var partnerKYCTypesKey = CacheManager.GetConfigKey(identity.PartnerId, Constants.PartnerKeys.PartnerKYCTypes);
                if (!string.IsNullOrEmpty(partnerKYCTypesKey))
                    partnerKYCDocumentTypes = JsonConvert.DeserializeObject<List<int>>(partnerKYCTypesKey);

                var registrationKYCTypes = new List<int>();
                if (client.DocumentType.HasValue)
                {
                    registrationKYCTypes = CacheManager.GetConfigParameters(identity.PartnerId, Constants.PartnerKeys.RegistrationKYCTypes).
                        Where(x => x.Key != client.DocumentType.Value.ToString()).Select(x => Convert.ToInt32(x.Key)).ToList();
                }

                var clientDocuments = clientBl.GetClientIdentities(identity.Id).Where(x => (x.Status == (int)KYCDocumentStates.Approved || x.Status == (int)KYCDocumentStates.InProcess) &&
                    (registrationKYCTypes.Contains(x.DocumentTypeId) || x.DocumentTypeId == client.DocumentType)).Select(x => x.DocumentTypeId).ToList();
                var documentTypes = BaseBll.GetEnumerations(Constants.EnumerationTypes.KYCDocumentTypes, identity.LanguageId)
                    .Where(x => !clientDocuments.Contains(x.Value) && (partnerKYCDocumentTypes == null || partnerKYCDocumentTypes.Contains(x.Value)))
                    .Select(x => new
                    {
                        Id = x.Value,
                        Name = x.Text
                    }).ToList();

                foreach (var dt in registrationKYCTypes)
                    documentTypes.RemoveAll(x => x.Id == dt);

                return new ApiResponseBase
                {
                    ResponseObject = documentTypes
                };
            }
        }

        private static ApiResponseBase GetKYCDocumentStatesEnum(SessionIdentity identity, ILog log)
        {
            var documentTypes = BaseBll.GetEnumerations(Constants.EnumerationTypes.KYCDocumentStates, identity.LanguageId).Select(x => new
            {
                Id = x.Value,
                Name = x.Text
            }).ToList();

            return new ApiResponseBase
            {
                ResponseObject = documentTypes
            };
        }

        private static ApiResponseBase GetClientIdentityModels(SessionIdentity identity, ILog log)
        {
            using (var clientBl = new ClientBll(identity, log))
            {
                var clientIdentity = clientBl.GetClientIdentityInfo(identity.Id, false);
                var documentTypes = BaseBll.GetEnumerations(Constants.EnumerationTypes.KYCDocumentTypes, identity.LanguageId);
                var clientIdentityModelList = clientIdentity.Select(x => x.ToClientIdentityModel(identity.TimeZone, documentTypes)).ToList();

                var response = new ApiResponseBase
                {
                    ResponseObject = clientIdentityModelList
                };
                return response;
            }
        }

        private static ApiResponseBase GetProductBalance(int productId, int clientId, SessionIdentity identity, ILog log)
        {
            var product = CacheManager.GetProductById(productId);
            var response = new ApiResponseBase();
            if (product == null || product.GameProviderId == null)
            {
                response.ResponseCode = Constants.Errors.ProductNotFound;
                return response;
            }
            var provider = CacheManager.GetGameProviderById(product.GameProviderId.Value);
            if (provider == null)
                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongProviderId);

            var client = CacheManager.GetClientById(clientId);
            if (client == null)
                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.ClientNotFound);

            switch (provider.Name)
            {
                default:
                    response.ResponseCode = Constants.Errors.WrongProductId;
                    break;
            }
            return response;
        }

        private static ApiResponseBase ActivatePromoCode(string code, int clientId, SessionIdentity identity, ILog log)
        {
            using (var clientBl = new ClientBll(identity, log))
            {
                using (var bonusBl = new BonusService(identity, log))
                {
                    var currentTime = DateTime.UtcNow;
                    var client = CacheManager.GetClientById(clientId);
                    if (client == null)
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.ClientNotFound);

                    var promoCode = clientBl.GetPromoCode(client.PartnerId, code);
                    switch (promoCode.Type)
                    {
                        case (int)PromoCodeType.CampainActivationCode:
                            var potencialBonuses = clientBl.AutoClaim(bonusBl, client.Id, (int)TriggerTypes.PromotionalCode, promoCode.Code, null, out int awardedStatus, 0, null);
                            if (awardedStatus == 3)
                                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.BlockedForBonuses);
                            bool alreadyActivated = false;

                            foreach (var b in potencialBonuses)
                            {
                                foreach (var s in b.TriggerGroups)
                                {
                                    foreach (var ts in s.TriggerGroupSettings)
                                    {
                                        var setting = CacheManager.GetTriggerSettingById(ts.SettingId);
                                        if (setting.Type == (int)TriggerTypes.PromotionalCode && setting.BonusSettingCodes == promoCode.Code &&
                                            setting.StartTime <= currentTime && setting.FinishTime > currentTime)
                                        {
                                            var notAwardedCampaigns = CacheManager.GetClientNotAwardedCampaigns(client.Id);
                                            var cBonuses = notAwardedCampaigns.Select(x => new ClientBonusInfo { BonusId = b.Id, ReuseNumber = x.ReuseNumber }).ToList();
                                            if (cBonuses.Any())
                                            {
                                                CacheManager.RemoveClientNotAwardedCampaigns(client.Id);
                                                clientBl.FairClientBonusTrigger(new ClientTriggerInput
                                                {
                                                    ClientId = client.Id,
                                                    ClientBonuses = cBonuses,
                                                    TriggerType = (int)TriggerTypes.PromotionalCode,
                                                    PromoCode = promoCode.Code
                                                }, out bool alreadyAdded);
                                                if (alreadyAdded)
                                                    alreadyActivated = true;
                                                else
                                                    awardedStatus = 1;
                                            }
                                            else
                                                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.BonusNotFound);
                                        }
                                    }
                                }
                            }
                            if (awardedStatus == 2)
                                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.PromoCodeAlreadyActivated);
                            else if (awardedStatus == 0)
                            {
                                if (!alreadyActivated)
                                    throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.BonusNotFound);
                                else
                                    throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.PromoCodeAlreadyActivated);
                            }

                            CacheManager.RemoveClientNotAwardedCampaigns(client.Id);
                            Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.NotAwardedCampaigns, client.Id));
                            break;
                        default:
                            throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.PromoCodeNotExists);
                    }
                    return new ApiResponseBase();
                }
            }
        }

        private static ApiResponseBase GetPartnerCustomerBanks(int clientId, int paymentSystemId, SessionIdentity session, ILog log)
        {
            using (var paymentBl = new PaymentSystemBll(session, log))
            {
                var client = CacheManager.GetClientById(clientId);
                if (client == null)
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.ClientNotFound);
                var resp = paymentBl.GetPartnerBanks(client.PartnerId, paymentSystemId, false, (int)BankInfoTypes.BankForCustomer, client).GroupBy(x => new
                {
                    x.BankName
                }).Select(x => new
                {
                    BankName = x.Key.BankName,
                    Branches = x.Select(y => new
                    {
                        y.Id,
                        y.BranchName,
                        Accounts = y.ClientPaymentInfos.Select(z => new ApiClientPaymentInfo
                        {
                            BankAccountNumber = z.BankAccountNumber,
                            Type = z.Type.ToString(),
                            OwnerName = y.OwnerName
                        }).ToList()
                    })
                }).ToList();
                return new ApiResponseBase
                {
                    ResponseObject = resp
                };
            }
        }

        private static ApiResponseBase GetPaymentLimits(int partnerId, SessionIdentity session, ILog log)
        {
            using (var partnerBl = new PartnerBll(session, log))
            {
                var response = new ApiResponseBase
                {
                    ResponseObject = partnerBl.GetPaymentLimit(partnerId, false)
                };
                return response;
            }
        }
        private static ApiResponseBase SetPaymentLimit(ApiPaymentLimit input, int clientId, SessionIdentity identity, ILog log)
        {
            using (var clientBl = new ClientBll(identity, log))
            {
                var paymentLimit = input.MapToPaymentLimit();
                paymentLimit.ClientId = clientId;
                paymentLimit.LimitTypeId = (int)LimitTypes.SelfExclusionLimit;
                paymentLimit.RowState = (int)LimitRowStates.Active;
                clientBl.SetPaymentLimit(paymentLimit, false);
                return new ApiResponseBase();
            }
        }

        private static ApiResponseBase GetPaymentLimitExclusion(int clientId, SessionIdentity identity, ILog log)
        {
            using (var clientBl = new ClientBll(identity, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = clientBl.GetPaymentLimitExclusion(clientId, false).MapToApiPaymentLimit(clientId)
                };
            }
        }

        private static ApiResponseBase SetProductLimit(DAL.Models.ProductLimit limit, int clientId, SessionIdentity identity, ILog log)
        {
            using (var productBl = new ProductBll(identity, log))
            {
                limit.ObjectId = clientId;
                limit.ObjectTypeId = (int)ObjectTypes.Client;
                var result = productBl.SaveProductLimit(limit, false);
                Helpers.Helpers.InvokeMessage("UpdateProductLimit", (int)ObjectTypes.Client, clientId, (int)LimitTypes.FixedClientMaxLimit, limit.ProductId);
                var response = new ApiResponseBase
                {
                    ResponseObject = result
                };
                return response;
            }
        }

        private static ApiResponseBase ApplySelfExclusion(ApiExclusionInput input, int clientId, bool checkPassword, SessionIdentity identity, ILog log)
        {
            var ct = DateTime.UtcNow;
            var currentTime = new DateTime(ct.Year, ct.Month, ct.Day, ct.Hour, ct.Minute, ct.Second);
            if (currentTime >= input.Date)//Add new partner key for credentials
                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongInputParameters);
            input.Date = new DateTime(input.Date.Year, input.Date.Month, input.Date.Day, currentTime.Hour, currentTime.Minute, currentTime.Second);

            var partnerSetting = CacheManager.GetConfigKey(identity.PartnerId, Constants.PartnerKeys.SelfExclusionPeriod);
            if (int.TryParse(partnerSetting, out int selfExclusionPeriod) && (input.Date - currentTime).TotalDays < selfExclusionPeriod)
                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongInputParameters);
            var client = CacheManager.GetClientById(clientId);

            if (checkPassword)
            {
                var pass = CommonFunctions.RSADecrypt(input.Credentials);
                var passwordHash = CommonFunctions.ComputeClientPasswordHash(pass, client.Salt);
                if (client.PasswordHash != passwordHash)
                    throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongPassword);
            }
            using (var clientBl = new ClientBll(identity, log))
            {
                clientBl.AddOrUpdateClientSetting(clientId, ClientSettings.SelfExcluded, 1, input.Reason.ToString(), input.Date, null, string.Empty);
                clientBl.LogoutClient(identity.Token);
                CacheManager.RemoveClientSetting(clientId, ClientSettings.SelfExcluded);
                Helpers.Helpers.InvokeMessage("RemoveClientSetting", clientId, ClientSettings.SelfExcluded);

                var verificationPlatform = CacheManager.GetConfigKey(client.PartnerId, Constants.PartnerKeys.VerificationPlatform);
                if (!string.IsNullOrEmpty(verificationPlatform) && int.TryParse(verificationPlatform, out int verificationPatformId))
                {
                    switch (verificationPatformId)
                    {
                        case (int)VerificationPlatforms.Insic:
                            var reason = CacheManager.GetPartnerCommentTemplates(client.PartnerId, (int)CommentTemplateTypes.ExclusionReason, identity.LanguageId)
                                                     .FirstOrDefault(x => x.Id == input.Reason)?.Text;
                            InsicHelpers.PlayerExcluded(client.PartnerId, client.Id, reason, input.Date, WebApiApplication.DbLogger);
                            if (!checkPassword)
                                OASISHelpers.ShortTermLock(client, null, WebApiApplication.DbLogger);
                            else
                                OASISHelpers.LongTermLock(client, null, input.Date, reason, WebApiApplication.DbLogger);
                            break;
                        default:
                            break;
                    }
                }
                return new ApiResponseBase();
            }
        }

        public static ApiResponseBase SavePayoutLimit(int clientId, decimal payoutLimit, SessionIdentity identity, ILog log)
        {
            if (payoutLimit <= 0 && payoutLimit != -1)
                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongInputParameters);
            using (var clientBl = new ClientBll(identity, log))
            {
                if (payoutLimit == -1)
                    clientBl.AddOrUpdateClientSetting(clientId, ClientSettings.PayoutLimit, null, null, null, null, string.Empty);
                else
                    clientBl.AddOrUpdateClientSetting(clientId, ClientSettings.PayoutLimit, payoutLimit, payoutLimit.ToString(), DateTime.UtcNow, null, string.Empty);
                Helpers.Helpers.InvokeMessage("RemoveClientSetting", clientId, ClientSettings.PayoutLimit);

                return new ApiResponseBase { ResponseObject = CacheManager.GetClientSettingByName(clientId, ClientSettings.PayoutLimit)?.NumericValue };
            }
        }

        public static ApiResponseBase GetAnnouncements(ApiFilterClientAnnouncement apiAnnouncement, int clientId, SessionIdentity identity, ILog log)
        {
            using (var contentBl = new ContentBll(identity, log))
            {
                var client = CacheManager.GetClientById(clientId);
                var filter = new FilterAnnouncement
                {
                    PartnerId = client.PartnerId,
                    ReceiverType = (int)ObjectTypes.Client,
                    Type = apiAnnouncement.Type,
                    TakeCount = apiAnnouncement.TakeCount,
                    SkipCount = apiAnnouncement.SkipCount,
                    FromDate = apiAnnouncement.FromDate,
                    ToDate = apiAnnouncement.ToDate,
                    AgentId = null
                };

                //if (apiAnnouncement.Type == (int)AnnouncementTypes.Personal)
                //    filter.ReceiverId = clientId;
                if (client.UserId != null)
                    filter.AgentId = client.UserId.Value;

                var announcements = contentBl.GetAnnouncements(filter, false);
                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        announcements.Count,
                        Entities = announcements.Entities.Select(x => new
                        {
                            x.Id,
                            CreationDate = x.CreationDate.GetGMTDateFromUTC(identity.TimeZone),
                            x.Message
                        }).ToList()
                    }
                };
            }
        }

        public static ApiResponseBase ClaimToCompainBonus(int bonusId, int clientId, SessionIdentity identity, ILog log)
        {
            using (var bonusService = new BonusService(identity, log))
            {
                var bonus = bonusService.GetAvailableBonus(bonusId, false);
                if (!Constants.ClaimingBonusTypes.Contains(bonus.Type))
                    throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.BonusNotFound);
                var client = CacheManager.GetClientById(clientId);
                var clientSegmentsIds = new List<int>();
                if (bonus.BonusSegmentSettings.Any())
                {
                    var clientClassifications = CacheManager.GetClientClassifications(client.Id);
                    if (clientClassifications.Any())
                        clientSegmentsIds = clientClassifications.Where(x => x.SegmentId.HasValue && x.ProductId == (int)Constants.PlatformProductId)
                                                                .Select(x => x.SegmentId.Value).ToList();
                }
                if ((bonus.BonusSegmentSettings.Any() &&
                    (bonus.BonusSegmentSettings.Any(x => x.Type == (int)BonusSettingConditionTypes.InSet && !clientSegmentsIds.Contains(x.SegmentId)) ||
                     bonus.BonusSegmentSettings.Any(x => x.Type == (int)BonusSettingConditionTypes.OutOfSet && clientSegmentsIds.Contains(x.SegmentId)))) ||
                    (bonus.BonusCountrySettings.Any() &&
                    (bonus.BonusCountrySettings.Any(y => y.Type == (int)BonusSettingConditionTypes.InSet && y.CountryId != (client.CountryId ?? client.RegionId)) ||
                     bonus.BonusCountrySettings.Any(y => y.Type == (int)BonusSettingConditionTypes.OutOfSet && y.CountryId == (client.CountryId ?? client.RegionId)))) ||
                    (bonus.BonusCurrencySettings.Any() &&
                    (bonus.BonusCurrencySettings.Any(y => y.Type == (int)BonusSettingConditionTypes.InSet && y.CurrencyId != client.CurrencyId) ||
                     bonus.BonusCurrencySettings.Any(y => y.Type == (int)BonusSettingConditionTypes.OutOfSet && y.CurrencyId == client.CurrencyId))) ||
                    (bonus.BonusLanguageSettings.Any() &&
                     bonus.BonusLanguageSettings.Any(y => y.Type == (int)BonusSettingConditionTypes.InSet && y.LanguageId != client.LanguageId) &&
                     bonus.BonusLanguageSettings.Any(y => y.Type == (int)BonusSettingConditionTypes.OutOfSet && y.LanguageId == client.LanguageId)))
                    throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.NotAllowed);
                var input = new Common.Models.Bonus.ClientBonusItem
                {
                    PartnerId = client.PartnerId,
                    BonusId = bonus.Id,
                    Type = bonus.Type,
                    ClientId = client.Id,
                    ClientUserName = client.UserName,
                    ClientCurrencyId = client.CurrencyId,
                    FinalAccountTypeId = bonus.FinalAccountTypeId ?? (int)AccountTypes.BonusWin,
                    ReusingMaxCount = bonus.ReusingMaxCount,
                    WinAccountTypeId = bonus.WinAccountTypeId,
                    ValidForAwarding = bonus.ValidForAwarding == null ? (DateTime?)null : DateTime.Now.AddHours(bonus.ValidForAwarding.Value),
                    ValidForSpending = bonus.ValidForSpending == null ? (DateTime?)null : DateTime.Now.AddHours(bonus.ValidForSpending.Value)
                };
                bonusService.GiveCompainToClient(input, out int awardedStatus);
                if (awardedStatus == 0)
                {
                    CacheManager.RemoveClientNotAwardedCampaigns(client.Id);
                    Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.NotAwardedCampaigns, client.Id));
                }
            }
            return new ApiResponseBase();
        }

        public static ApiResponseBase SpendComplimentaryPoints(ApiComplimentaryPoint apiComplimentaryPoint, SessionIdentity identity, ILog log)
        {
            using (var clientBl = new ClientBll(identity, log))
            {
                clientBl.SpendComplimentaryPoints(identity.Id, apiComplimentaryPoint.CompPoints);
            }
            return new ApiResponseBase();
        }

        public static ApiResponseBase GetClientBonusTriggers(ApiClientBonusItem input, int clientId, SessionIdentity identity, ILog log)
        {
            using (var clientBl = new ClientBll(identity, log))
            {
                using (var documentBl = new DocumentBll(identity, log))
                {
                    var resp = clientBl.GetClientBonusTriggers(clientId, input.BonusId, input.ReuseNumber ?? 1, false).
                    Select(x => new
                    {
                        x.Name,
                        x.Description,
                        x.Type,
                        TypeName = Enum.GetName(typeof(TriggerTypes), x.Type),
                        StartTime = x.StartTime.GetGMTDateFromUTC(identity.TimeZone),
                        FinishTime = x.FinishTime.GetGMTDateFromUTC(identity.TimeZone),
                        x.Percent,
                        x.Status,
                        StatusName = Enum.GetName(typeof(ClientBonusTriggerStatuses), x.Status),
                        x.MinBetCount,
                        x.BetCount,
                        x.WageringAmount,
                        x.MinAmount
                    }).ToList();
                    var bonus = documentBl.GetClientBonusById(input.BonusId, input.ReuseNumber);
                    return new ApiResponseBase
                    {
                        ResponseObject = new { Bonus = bonus.ToApiClientBonusItem(identity.TimeZone), Triggers = resp }
                    };
                }
            }
        }

        private static ApiResponseBase GetClientStatuses(int clientId, SessionIdentity session, ILog log)
        {
            using (var clientBl = new ClientBll(session, log))
            {
                var client = CacheManager.GetClientById(clientId);
                var selfExcluded = CacheManager.GetClientSettingByName(client.Id, ClientSettings.SelfExcluded);
                var systemExcluded = CacheManager.GetClientSettingByName(client.Id, ClientSettings.SystemExcluded);
                var amlProhibited = CacheManager.GetClientSettingByName(client.Id, ClientSettings.AMLProhibited);
                var amlVerified = CacheManager.GetClientSettingByName(client.Id, ClientSettings.AMLVerified);
                var cautionSuspension = CacheManager.GetClientSettingByName(client.Id, ClientSettings.CautionSuspension);
                var blockedForInactivity = CacheManager.GetClientSettingByName(client.Id, ClientSettings.BlockedForInactivity);
                var partner = CacheManager.GetPartnerById(client.PartnerId);
                var termsConditionExpired = false;
                var termsConditionVersion = CacheManager.GetConfigKey(client.PartnerId, Constants.PartnerKeys.TermsConditionVersion);
                if (!string.IsNullOrEmpty(termsConditionVersion))
                {
                    var termsConditionsAcceptanceVersion = CacheManager.GetClientSettingByName(client.Id, ClientSettings.TermsConditionsAcceptanceVersion);
                    if (termsConditionsAcceptanceVersion != null && termsConditionsAcceptanceVersion.Id > 0 && termsConditionsAcceptanceVersion.StringValue == termsConditionVersion)
                        termsConditionExpired = true;
                }
                var documents = clientBl.GetClientIdentities(clientId);
                var currentDate = DateTime.UtcNow;
                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        Excluded = (client.State == (int)ClientStates.ForceBlock || client.State == (int)ClientStates.FullBlocked),
                        SelfExcluded = (selfExcluded != null && selfExcluded.Id != 0 && selfExcluded.NumericValue == 1),
                        SystemExcluded = (systemExcluded != null && systemExcluded.Id != 0 && systemExcluded.NumericValue == 1),
                        DocumentVerified = client.IsDocumentVerified,
                        DocumentExpired = (!documents.Any(x => x.Status == (int)KYCDocumentStates.Approved) && documents.Any(x => x.Status == (int)KYCDocumentStates.Expired)),
                        AMLProhibited = (amlProhibited != null && amlProhibited.Id != 0 && amlProhibited.NumericValue == 1),
                        AMLVerified = (amlVerified != null && amlVerified.Id != 0 && amlVerified.NumericValue == 1),
                        CautionSuspension = (cautionSuspension != null && cautionSuspension.Id != 0 && cautionSuspension.NumericValue == 1),
                        Younger = (client.BirthDate != DateTime.MinValue &&
                        (currentDate.Year - client.BirthDate.Year < partner.ClientMinAge ||
                        (currentDate.Year - client.BirthDate.Year == partner.ClientMinAge && currentDate.Month < client.BirthDate.Month) ||
                        (currentDate.Year - client.BirthDate.Year == partner.ClientMinAge && currentDate.Month == client.BirthDate.Month && currentDate.Day < client.BirthDate.Day))),
                        Active = !(blockedForInactivity != null && blockedForInactivity.Id > 0 && blockedForInactivity.NumericValue == 1),
                        TermsConditionsAccepted = termsConditionExpired
                    }
                };
            }
        }

        private static ApiResponseBase GetLimits(int clientId, SessionIdentity session, ILog log)
        {
            using (var clientBl = new ClientBll(session, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = clientBl.GetClientLimitSettings(clientId, false)
                };
            }
        }

        private static ApiResponseBase SetLimits(ClientCustomSettings limits, int clientId, SessionIdentity session, ILog log)
        {
            using (var scope = CommonFunctions.CreateTransactionScope())
            {
                using (var clientBl = new ClientBll(session, log))
                {
                    limits.ClientId = clientId;
                    clientBl.SaveClientLimitSettings(limits, null, out Dictionary<string, decimal?> editingSettings); // check 7 days
                    var verificationPlatform = CacheManager.GetConfigKey(session.PartnerId, Constants.PartnerKeys.VerificationPlatform);
                    if (!string.IsNullOrEmpty(verificationPlatform) && int.TryParse(verificationPlatform, out int verificationPatformId))
                    {
                        switch (verificationPatformId)
                        {
                            case (int)VerificationPlatforms.Insic:
                                foreach (var limit in editingSettings)
                                    InsicHelpers.UpdatePlayerLimit(session.PartnerId, clientId, limit.Key, limit.Value, WebApiApplication.DbLogger);
                                break;
                            default:
                                break;
                        }
                    }
                    scope.Complete();
                }
                return new ApiResponseBase();
            }
        }

        private static ApiResponseBase GetSessionInfo(SessionIdentity session, ILog log)
        {
            using (var clientBl = new ClientBll(session, log))
            {
                var result = clientBl.GetSessionInfo(string.Empty);
                return new ApiResponseBase
                {
                    ResponseCode = result.ResponseCode,
                    Description = result.Description,
                    ResponseObject = new
                    {
                        GivenCredit = Math.Floor(result.GivenCredit * 100) / 100,
                        AvailableCredit = Math.Floor(result.AvailableCredit * 100) / 100,
                        CashBalance = Math.Floor(result.CashBalance * 100) / 100,
                        OutstandingBalance = Math.Floor(result.OutstandingBalance * 100) / 100,
                        LastLoginDate = result.LastLoginDate.GetGMTDateFromUTC(session.TimeZone),
                        PasswordExpiryDate = result.PasswordExpiryDate.GetGMTDateFromUTC(session.TimeZone),
                        LastTransactionDate = result.LastTransactionDate.GetGMTDateFromUTC(session.TimeZone)
                    }
                };
            }
        }

        private static ApiResponseBase ChangeNickName(string nickName, int clientId, SessionIdentity session, ILog log)
        {
            using (var clientBl = new ClientBll(session, log))
            {
                clientBl.ChangeClientNickName(clientId, nickName);
                Helpers.Helpers.InvokeMessage("RemoveClient", clientId);
            }
            return new ApiResponseBase();
        }

        private static ApiResponseBase UpdateSecurityAnswers(int clientId, ApiClientSecurityModel apiClientSecurityModel, SessionIdentity session, ILog log)
        {
            using (var clientBl = new ClientBll(session, log))
            {
                clientBl.UpdateClientSecurityAnswers(clientId, apiClientSecurityModel);
            }
            return new ApiResponseBase();
        }


        private static ApiResponseBase GetDepositLimits(int clientId, SessionIdentity session, ILog log)
        {
            using (var clientBl = new ClientBll(session, log))
            {
                var info = clientBl.CheckClientDepositLimits(clientId, 0);
                return new ApiResponseBase { ResponseObject = info };
            }
        }
        private static ApiResponseBase GenerateQRCode(SessionIdentity session, ILog log)
        {
            var client = CacheManager.GetClientById(session.Id);
            var partner = CacheManager.GetPartnerById(client.PartnerId);
            var key = CommonFunctions.GenerateQRCode();
            return new ApiResponseBase
            {
                ResponseObject = new
                {
                    Key = key,
                    Data = $"otpauth://totp/{partner.Name}:{Uri.EscapeDataString(client.Email??client.UserName)}?secret={key}&issuer={partner.Name}"
                }
            };
        }

        private static ApiResponseBase EnableClientTwoFactor(ApiQRCodeInput apiQRCodeInput, SessionIdentity session, ILog log)
        {
            using (var clientBl = new ClientBll(session, log))
            {
                clientBl.EnableClientTwoFactor(apiQRCodeInput.QRCode, apiQRCodeInput.Pin);
                Helpers.Helpers.InvokeMessage("RemoveClient", session.Id);
                return new ApiResponseBase();
            }
        }

        private static ApiResponseBase DisableClientTwoFactor(string pin, SessionIdentity session, ILog log)
        {
            using (var clientBl = new ClientBll(session, log))
            {
                clientBl.DisableClientTwoFactor(pin);
                Helpers.Helpers.InvokeMessage("RemoveClient", session.Id);
                return new ApiResponseBase();
            }
        }

        private static ApiResponseBase DisableClientAccount(ApiExclusionInput input, SessionIdentity session, ILog log)
        {
            var pass = CommonFunctions.RSADecrypt(input.Credentials);
            var client = CacheManager.GetClientById(session.Id);
            var passwordHash = CommonFunctions.ComputeClientPasswordHash(pass, client.Salt);
            if (client.PasswordHash != passwordHash)
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongPassword);

            using (var clientBl = new ClientBll(session, log))
            {
                clientBl.ChangeClientState(client.Id, (int)ClientStates.Disabled, null);
                clientBl.LogoutClient(session.Token);
                Helpers.Helpers.InvokeMessage("RemoveClient", session.Id);
                var verificationPlatform = CacheManager.GetConfigKey(client.PartnerId, Constants.PartnerKeys.VerificationPlatform);
                if (!string.IsNullOrEmpty(verificationPlatform) && int.TryParse(verificationPlatform, out int verificationPatformId))
                {
                    switch (verificationPatformId)
                    {
                        case (int)VerificationPlatforms.Insic:
                            var reason = CacheManager.GetPartnerCommentTemplates(client.PartnerId, (int)CommentTemplateTypes.ExclusionReason, session.LanguageId)
                                                     .FirstOrDefault(x => x.Id == input.Reason)?.Text;
                            InsicHelpers.CloseAccount(client.PartnerId, client.Id, reason, WebApiApplication.DbLogger);
                            break;
                        default:
                            break;
                    }
                }
                return new ApiResponseBase();
            }
        }

        private static ApiResponseBase SelectSessionAccount(long accountId, SessionIdentity session, ILog log)
        {
            using (var clientBl = new ClientBll(session, log))
            {
                clientBl.SelectSessionAccount(accountId);
                Helpers.Helpers.InvokeMessage("RemoveClient", session.Id);
                return new ApiResponseBase();
            }
        }

        private static ApiResponseBase GetClientPaymentInfoStatesEnum(string enumName, SessionIdentity identity)
        {
            return new ApiResponseBase
            {
                ResponseObject = BaseBll.GetEnumerations(enumName, identity.LanguageId).Select(x => x.MapToApiEnumeration()).ToList()
            };
        }

        private static ApiResponseBase AddCharacterToClient(int clientId, int characterId, SessionIdentity session, ILog log)
        {
            using (var clientBl = new ClientBll(session, log))
            {
                var newCharacterId = clientBl.AddCharacterToClient(clientId, characterId);
                return new ApiResponseBase()
                {
                    ResponseObject = newCharacterId
                };
            }
        }

        private static ApiResponseBase GetCharacterCurrentState(int clientId, int partnerId, SessionIdentity session, ILog log)
        {
            var client = CacheManager.GetClientById(clientId) ?? throw BaseBll.CreateException(session.LanguageId, Constants.Errors.ClientNotFound);
            if (client.CharacterId == null)
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.CharacterNotFound);
            var balances = CacheManager.GetClientCurrentBalance(clientId).Balances;
            var balance = balances.Select(x => x.TypeId).Contains((int)AccountTypes.ClientCompBalance)
                                      ? balances.FirstOrDefault(x => x.TypeId == (int)AccountTypes.ClientCompBalance).Balance : 0;
            var characters = CacheManager.GetCharacters(partnerId, session.LanguageId);
            var currentCharacter = characters.FirstOrDefault(x => x.Id == client.CharacterId);
            var parent = characters.FirstOrDefault(x => x.Id == currentCharacter.ParentId);
            var state = new ClientCharacterState()
            {
                Parent = parent?.MapToApiCharacter(),
                Previous = currentCharacter?.MapToApiCharacter(),
                Current = balance,
                Next = characters.FirstOrDefault(x => x.ParentId == parent.Id && x.Order == currentCharacter.Order + 1)?.MapToApiCharacter()
            };
            return new ApiResponseBase()
            {
                ResponseObject = state
            };
        }

        private static ApiResponseBase ConfirmLimit(int clientId, ConfirmLimitInput input, SessionIdentity session, ILog log)
        {
            using (var scope = CommonFunctions.CreateTransactionScope())
            {
                using (var clientBl = new ClientBll(session, log))
                {
                    clientBl.SaveClientSetting(clientId, Constants.ClientSettings.LimitConfirmed, true.ToString(), 1, DateTime.UtcNow);
                    if (input.DefaultLimit)
                    {
                        var clientSettings = clientBl.GetClientLimitSettings(clientId, false);
                        var clientSettingsDictionary = clientSettings.GetType().GetProperties()
                                                    .Where(y => y != null && !string.IsNullOrWhiteSpace(y.ToString()) && y.Name.Contains("Limit") &&
                                                               !y.Name.Contains("System") && y.GetValue(clientSettings, null) != null  &&
                                                               y.PropertyType == typeof(LimitItem) )
                                                    .OrderByDescending(x => x.Name.Contains(PeriodsOfTime.Monthly.ToString()))
                                                    .ThenByDescending(x => x.Name.Contains(PeriodsOfTime.Weekly.ToString()))
                                                    .ThenByDescending(x => x.Name.Contains(PeriodsOfTime.Daily.ToString()))
                                                    .ToDictionary(x => x.Name, x => x.GetValue(clientSettings, null));
                        log.Debug("limits: " +JsonConvert.SerializeObject(clientSettingsDictionary));
                        var verificationPlatform = CacheManager.GetConfigKey(session.PartnerId, Constants.PartnerKeys.VerificationPlatform);
                        if (!string.IsNullOrEmpty(verificationPlatform) && int.TryParse(verificationPlatform, out int verificationPatformId))
                        {
                            switch (verificationPatformId)
                            {
                                case (int)VerificationPlatforms.Insic:
                                    foreach (var limit in clientSettingsDictionary)
                                    {
                                        var limitItem = (LimitItem)limit.Value;
                                        InsicHelpers.UpdatePlayerLimit(session.PartnerId, clientId, limit.Key, limitItem.Limit, WebApiApplication.DbLogger);
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
                scope.Complete();
                return new ApiResponseBase();
            }
        }

        private static ApiResponseBase ViewPopup(int clientId, ApiPopupWeSiteModel input, SessionIdentity session, ILog log)
        {
            using (var clientBl = new ClientBll(session, log))
            {
                clientBl.ViewPopup(clientId, input.Id);
                return new ApiResponseBase();
            }
        }
    }
}