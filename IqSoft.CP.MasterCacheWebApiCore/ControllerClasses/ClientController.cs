using System.Collections.Generic;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.DAL.Filters;
using IqSoft.CP.DAL.Models;
using Newtonsoft.Json;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using log4net;
using System;
using System.Linq;
using IqSoft.CP.DAL;
using IqSoft.CP.Common.Models.WebSiteModels.Clients;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.MasterCacheWebApi.Helpers;
using IqSoft.CP.Integration.Platforms.Helpers;
using IqSoft.CP.Common.Models.WebSiteModels.Filters;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.DAL.Models.Clients;
using static IqSoft.CP.Common.Constants;
using Microsoft.AspNetCore.Hosting;
using IqSoft.CP.DAL.Models.Notification;

namespace IqSoft.CP.MasterCacheWebApi.ControllerClasses
{
    public static class ClientController
    {
        public static ApiResponseBase CallFunction(RequestBase request, SessionIdentity session, ILog log, IWebHostEnvironment environment)
        {
            switch (request.Method)
            {
                case "ChangeClientDetails":
                    return
                        ChangeClientDetails(
                            JsonConvert.DeserializeObject<ChangeClientFieldsInput>(request.RequestData),
                            request.ClientId, session, log);
                case "ChangeClientPassword":
                    return
                        ChangeClientPassword(
                            JsonConvert.DeserializeObject<ChangeClientPasswordInput>(request.RequestData),
                            request.ClientId, session, log);
                case "LogoutClient":
                    return LogoutClient(request.Token, session, log);
                case "SendVerificationCodeToMobileNumber":
                    return
                        SendVerificationCodeToMobileNumber(
                            JsonConvert.DeserializeObject<SendVerificationCodeInput>(request.RequestData),
                            request.ClientId, session, log);
                case "SendVerificationCodeToEmail":
                    return
                        SendVerificationCodeToEmail(
                            JsonConvert.DeserializeObject<SendVerificationCodeInput>(request.RequestData),
                            request.ClientId, session, log);
                case "VerifyClientEmail":
                    return VerifyClientEmail(JsonConvert.DeserializeObject<VerifyClientInput>(request.RequestData),
                        request.ClientId, session, log);
                case "VerifyClientMobileNumber":
                    return
                        VerifyClientMobileNumber(JsonConvert.DeserializeObject<VerifyClientInput>(request.RequestData),
                            request.ClientId, session, log);
                case "CreateProductLimitByClient":
                    return
                        CreateProductLimitByClient(
                            JsonConvert.DeserializeObject<DAL.Models.ProductLimit>(request.RequestData), session, log);
                case "AddBankAccount":
                    return AddBankAccount(JsonConvert.DeserializeObject<ApiClientPaymentInfo>(request.RequestData), request.ClientId, session, log);
                case "AddCardData":
                    return AddCardData(JsonConvert.DeserializeObject<ApiClientPaymentInfo>(request.RequestData), request.ClientId, session, log);
                case "GetClientBankAccounts":
                    return GetClientBankAccounts(request.ClientId, session, log);
                case "GetClientPaymentMethods":
                    return GetClientPaymentMethods(Convert.ToInt32(request.RequestData), session, log);
                case "GetClientCards":
                    return GetClientCards(request.ClientId, session, log);
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
                case "SendAccountDetailsVerificationCode":
                    return SendAccountDetailsVerificationCode(request.ClientId, session, log);
                case "InviteFriend":
                    return InviteFriend(JsonConvert.DeserializeObject<ApiInviteFriendInput>(request.RequestData), session, log);
                case "GetPartnerCustomerBanks":
                    return GetPartnerCustomerBanks(request.ClientId, Convert.ToInt32(request.RequestData), session, log);
                case "UploadImage":
                    return UploadImage(JsonConvert.DeserializeObject<AddClientIdentityModel>(request.RequestData), session, log, environment);
                case "GetKYCDocumentTypesEnum":
                    return GetKYCDocumentTypesEnum(session, log);
                case "GetKYCDocumentStatesEnum":
                    return GetKYCDocumentStatesEnum(session, log);
                case "GetClientIdentityModels":
                    return GetClientIdentityModels(session, log);
                case "GetProductBalance":
                    return GetProductBalance(Convert.ToInt32(request.RequestData), request.ClientId, session, log);
                case "ActivatePromoCode":
                    return ActivatePromoCode(request.RequestData, request.ClientId, session, log);
                case "SetPaymentLimit":
                    return SetPaymentLimit(JsonConvert.DeserializeObject<ApiPaymentLimit>(request.RequestData), request.ClientId, session, log);
                case "GetPaymentLimits"://??
                    return GetPaymentLimits(request.PartnerId, session, log);
                case "GetPaymentLimitExclusion":
                    return GetPaymentLimitExclusion(request.ClientId, session, log);
                case "SetProductLimit":
                    return SetProductLimit(JsonConvert.DeserializeObject<DAL.Models.ProductLimit>(request.RequestData), request.ClientId, session, log);
                case "ApplySelfExclusion":
                    return ApplySelfExclusion(request.RequestData, request.ClientId, session, log);
                case "GetAnnouncements":
                    return GetAnnouncements(JsonConvert.DeserializeObject<ApiFilterClientAnnouncement>(request.RequestData), request.ClientId, session, log);
                case "ClaimToCompainBonus":
                    return ClaimToCompainBonus(Convert.ToInt32(request.RequestData), request.ClientId, session, log);
                case "GetClientBonusTriggers":
                    return GetClientBonusTriggers(JsonConvert.DeserializeObject<ApiClientBonusItem>(request.RequestData), request.ClientId, session, log);
                case "AcceptTermsConditions":
                    return AcceptTermsConditions(request.ClientId, session, log);
                case "GetClientStatuses":
                    return GetClientStatuses(request.ClientId, session, log);
                case "GetLimits":
                    return GetLimits(request.ClientId, session, log);
                case "SetLimits":
                    return SetLimits(JsonConvert.DeserializeObject<ClientCustomSettings>(request.RequestData), request.ClientId, session, log);
                case "GetSessionInfo":
                    return GetSessionInfo(session, log);
                case "ChangeNickName":
                    return ChangeNickName(request.RequestData, request.ClientId, session, log);
                case "UpdateSecurityAnswers":
                    return UpdateSecurityAnswers(request.ClientId, JsonConvert.DeserializeObject<ApiClientSecurityModel>(request.RequestData), session, log);
                case "GetClientExternalAccounts":
                    return GetClientExternalAccounts(Convert.ToInt32(request.RequestData), session, log);
                default:
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.MethodNotFound);
            }
        }
        private static ApiResponseBase ChangeClientDetails(ChangeClientFieldsInput input, int clientId, SessionIdentity session, ILog log)
        {
            using (var clientBl = new ClientBll(session, log))
            {
                input.ClientId = clientId;
                var client = clientBl.ChangeClientDataFromWebSite(input).MapToApiLoginClientOutput(session.TimeZone);
                CacheManager.RemoveClientFromCache(clientId);
                Helpers.Helpers.InvokeMessage("RemoveClient", clientId);
                return new ApiResponseBase
                {
                    ResponseObject = client
                };
            }
        }

        private static ApiResponseBase ChangeClientPassword(ChangeClientPasswordInput input, int clientId, SessionIdentity session, ILog log)
        {
            input.ClientId = clientId;
            using var clientBl = new ClientBll(session, log);
            clientBl.ChangeClientPassword(input);
            CacheManager.RemoveClientFromCache(clientId);
            Helpers.Helpers.InvokeMessage("RemoveClient", clientId);
            Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.ClientSettings,
                                          clientId, nameof(Constants.ClientSettings.PasswordChangedDate)));
            return new ApiResponseBase();
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
                return new ApiResponseBase();
            }
        }

        public static GetClientAccountsOutput GetClientAccounts(int clientId, SessionIdentity session, ILog log)
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
                    return new GetClientAccountsOutput
                    {
                        Accounts = accounts.Where(x => x.TypeId == (int)AccountTypes.ClientUnusedBalance).Select(x => { x.Balance = externalBalance; return x.MapToAccountModel(uawp); }).ToList()
                    };
                }
                return new GetClientAccountsOutput
                {
                    Accounts = accounts.Select(x => x.MapToAccountModel(uawp)).ToList()
                };
            }
        }

        public static ApiResponseBase GetClientExternalAccounts(int clientId, SessionIdentity session, ILog log)
        {
            var result = new List<AccountModel>();
            var client = CacheManager.GetClientById(clientId);
            var partnerKey = CacheManager.GetConfigKey(client.PartnerId, Constants.PartnerKeys.GetEMBonusBalance);
            if (!string.IsNullOrEmpty(partnerKey) && partnerKey == "1")
            {
                result.Add(new AccountModel
                {
                  //  TypeId = (int)AccountTypes.EMBonusBalance, //??
                    Balance = Integration.Products.Helpers.EveryMatrixHelpers.GetPlayerBonusBalance(client, log),
                    CurrencyId = client.CurrencyId,
                    //AccountTypeName = AccountTypes.EMBonusBalance.ToString()
                });
            }
            return new ApiResponseBase { ResponseObject = result };
        }



        private static ApiResponseBase SendVerificationCodeToMobileNumber(SendVerificationCodeInput input, int clientId, SessionIdentity session, ILog log)
        {
            using var notificationBl = new NotificationBll(session, log);
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

        private static ApiResponseBase SendVerificationCodeToEmail(SendVerificationCodeInput input, int clientId, SessionIdentity session, ILog log)
        {
            using var notificationBll = new NotificationBll(session, log);
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

        private static ApiResponseBase VerifyClientEmail(VerifyClientInput input, int clientId, SessionIdentity session, ILog log)
        {
            using var clientBl = new ClientBll(session, log);
            var client = CacheManager.GetClientById(clientId);
            clientBl.VerifyClientEmail(input.Key, string.Empty, clientId, client.PartnerId, false, input.SecurityQuestions);
            CacheManager.RemoveClientFromCache(clientId);
            Helpers.Helpers.InvokeMessage("RemoveClient", clientId);
            return new ApiResponseBase();
        }

        private static ApiResponseBase VerifyClientMobileNumber(VerifyClientInput input, int clientId, SessionIdentity session, ILog log)
        {
            using var clientBl = new ClientBll(session, log);
            var client = CacheManager.GetClientById(clientId);
            clientBl.VerifyClientMobileNumber(input.Key, client.MobileNumber, clientId, client.PartnerId, false, input.SecurityQuestions);
            CacheManager.RemoveClientFromCache(clientId);
            Helpers.Helpers.InvokeMessage("RemoveClient", clientId);
            return new ApiResponseBase();
        }

        public static GetClientStatesOutput GetClientStates(int clientId, SessionIdentity session, ILog log)
        {
            var client = CacheManager.GetClientById(clientId);
            if (client == null)
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.ClientNotFound);
            var messagesCount = CacheManager.GetClientUnreadTicketsCount(clientId).Count;
            var response = new GetClientStatesOutput
            {
                UnreadMessagesCount = messagesCount
            };
            return response;
        }

        public static GetPartnerBetShopsOutput GetBetShopsByClientId(int clientId, SessionIdentity session, ILog log)
        {
            using (var clientBl = new BetShopBll(session, log))
            {
                var betShops = clientBl.GetBetShopsByClientId(clientId);
                var response = new GetPartnerBetShopsOutput
                {
                    BetShops = betShops.MapToBetShopModels()
                };
                return response;
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
                    ClientId = client.Id,
                    MobileOrEmail = client.MobileNumber,
                    ClientInfoType = (int)ClientInfoTypes.AccountDetailsMobileKey,
                    VerificationCode = verificationKey
                });
                return new ApiResponseBase();
            }
        }

        private static ApiResponseBase AddCardData(ApiClientPaymentInfo input, int clientId, SessionIdentity session, ILog log)
        {
            using (var clientBl = new ClientBll(session, log))
            {
                var type = (int)Enum.Parse(typeof(ClientPaymentInfoTypes), input.Type);
                if (type != (int)ClientPaymentInfoTypes.CreditCard ||
                    string.IsNullOrEmpty(input.ClientName) || string.IsNullOrEmpty(input.CardNumber) || string.IsNullOrEmpty(input.Code) ||
                    !input.CardExpireDate.HasValue)
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongInputParameters);
                var clientPaymentInfo = input.MapToClientPaymentInfo();
                clientPaymentInfo.ClientId = clientId;
                var info = clientBl.RegisterClientPaymentAccountDetails(clientPaymentInfo, input.Code, false);
                return new ApiResponseBase
                {
                    ResponseCode = info == null ? Errors.GeneralException : Constants.SuccessResponseCode,
                    ResponseObject = info == null ? null : info.ToApiClientPaymentInfo()
                };
            }
        }

        private static ApiResponseBase AddBankAccount(ApiClientPaymentInfo input, int clientId, SessionIdentity session, ILog log)
        {
            using (var clientBl = new ClientBll(session, log))
            {
                if (string.IsNullOrEmpty(input.BankAccountNumber) || string.IsNullOrEmpty(input.BankName))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongInputParameters);

                input.Type = ((int)ClientPaymentInfoTypes.BankAccount).ToString();
                var clientPaymentInfo = input.MapToClientPaymentInfo();
                clientPaymentInfo.ClientId = clientId;
                var resp = clientBl.RegisterClientPaymentAccountDetails(clientPaymentInfo, input.Code, false);
                return new ApiResponseBase
                {
                    ResponseCode = resp == null ? Errors.GeneralException : Constants.SuccessResponseCode,
                    ResponseObject = resp == null ? null : resp.ToApiClientPaymentInfo()
                };
            }
        }

        private static ApiResponseBase GetClientBankAccounts(int clientId, SessionIdentity session, ILog log)
        {
            using (var clientBl = new ClientBll(session, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = clientBl.GetClientPaymentAccountDetails(clientId, null, new List<int> { (int)ClientPaymentInfoTypes.BankAccount }, false).Select(x => x.ToApiClientPaymentInfo()).ToList()
                };
            }
        }

        private static ApiResponseBase GetClientPaymentMethods(int paymentSystemId, SessionIdentity session, ILog log)
        {
            using (var clientBl = new ClientBll(session, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = clientBl.GetClientPaymentAccountDetails(session.Id, paymentSystemId, new List<int>(), false).Select(x => x.ToApiClientPaymentInfo()).ToList()
                };
            }
        }

        private static ApiResponseBase GetClientCards(int clientId, SessionIdentity session, ILog log)
        {
            using (var clientBl = new ClientBll(session, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = clientBl.GetClientPaymentAccountDetails(clientId, null,
                    new List<int> { (int)ClientPaymentInfoTypes.CreditCard },
                    false).Select(x => x.ToApiClientPaymentInfo()).ToList()
                };
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

                var resp = clientBl.OpenTickets(ticket, message, new List<int> { clientId }, false);
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
                var resp = clientBl.GetClientTickets(clientId, partnerId, ticketsInput.SkipCount, ticketsInput.TakeCount);
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

        private static ApiResponseBase UploadImage(AddClientIdentityModel input, SessionIdentity session, ILog log, IWebHostEnvironment env)
        {
            if (string.IsNullOrEmpty(input.ImageData))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongInputParameters);
            using (var clientBl = new ClientBll(session, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        ClientIdentityData = clientBl.SaveKYCDocument(input.ToClientIdentity(), input.Name, Convert.FromBase64String(input.ImageData), false, env)
                                                     .ToClientIdentityModel(session.TimeZone)
                    }
                };
            }
        }

        private static ApiResponseBase GetKYCDocumentTypesEnum(SessionIdentity identity, ILog log)
        {
            using (var clientBl = new ClientBll(identity, log))
            {
                var client = CacheManager.GetClientById(identity.Id);

                List<int> partnerKYCDocumentTypes = null;
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
                    .Where(x => !clientDocuments.Contains(x.Value) && (partnerKYCDocumentTypes == null || partnerKYCDocumentTypes.Contains(x.Value))).Select(x => new
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
                        case (int)PromoCodeType.CampainActivationCore:
                            var oldBonuses = clientBl.AutoClaim(bonusBl, client.Id, (int)TriggerTypes.PromotionalCode, promoCode.Code, null, out int awardedStatus, 0, null);
                            if (awardedStatus == 3)
                                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.BlockedForBonuses);
                            bool alreadyActivated = false;
                            foreach (var b in oldBonuses)
                            {
                                foreach (var s in b.TriggerGroups)
                                {
                                    foreach (var ts in s.TriggerGroupSettings)
                                    {
                                        var setting = CacheManager.GetTriggerSettingById(ts.SettingId);
                                        if (setting.Type == (int)TriggerTypes.PromotionalCode && setting.BonusSettingCodes == promoCode.Code &&
                                            setting.StartTime <= currentTime && setting.FinishTime > currentTime)
                                        {
                                            CacheManager.RemoveClientNotAwardedCampaigns(client.Id);
                                            clientBl.FairClientBonusTrigger(new ClientTriggerInput
                                            {
                                                ClientId= client.Id,
                                                ClientBonuses = new List<ClientBonusInfo> { new ClientBonusInfo { BonusId = b.Id, ReuseNumber = 1 } },
                                                TriggerType = (int)TriggerTypes.PromotionalCode,
                                                PromoCode = promoCode.Code
                                            }, out bool alreadyAdded);
                                            if (alreadyAdded)
                                                alreadyActivated = true;
                                            else
                                                awardedStatus = 1;
                                        }
                                    }
                                }
                            }
                            if (awardedStatus == 0)
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
                var resp = paymentBl.GetPartnerBanks(client.PartnerId, paymentSystemId, false, (int)BankInfoTypes.BankForCustomer, client)
                                    .Select(x => x.MapToApiPartnerBankInfo()).ToList();
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
                var response = new ApiResponseBase
                {
                    ResponseObject = clientBl.GetPaymentLimitExclusion(clientId, false).MapToApiPaymentLimit(clientId)
                };
                return response;
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

        private static ApiResponseBase ApplySelfExclusion(string input, int clientId, SessionIdentity identity, ILog log)
        {
            var ct = DateTime.UtcNow;
            var currentTime = new DateTime(ct.Year, ct.Month, ct.Day, ct.Hour, ct.Minute, ct.Second);
            if (!DateTime.TryParse(input, out DateTime toDate) || currentTime >= toDate)
                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongInputParameters);
            toDate = new DateTime(toDate.Year, toDate.Month, toDate.Day, currentTime.Hour, currentTime.Minute, currentTime.Second);

            var partnerSetting = CacheManager.GetConfigKey(identity.PartnerId, Constants.PartnerKeys.SelfExclusionPeriod);
            if (int.TryParse(partnerSetting, out int selfExclusionPeriod) && (toDate - currentTime).TotalDays < selfExclusionPeriod)
                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongInputParameters);

            using (var clientBl = new ClientBll(identity, log))
            {
                clientBl.AddOrUpdateClientSetting(clientId, ClientSettings.SelfExcluded, 1, string.Empty, toDate, null, string.Empty);
                clientBl.LogoutClient(identity.Token);
                CacheManager.RemoveClientSetting(clientId, ClientSettings.SelfExcluded);
                Helpers.Helpers.InvokeMessage("RemoveClientSetting", clientId, ClientSettings.SelfExcluded);
                return new ApiResponseBase();
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
                    ReceiverTypeId = (int)ObjectTypes.Client,
                    Type = apiAnnouncement.Type,
                    TakeCount = apiAnnouncement.TakeCount,
                    SkipCount = apiAnnouncement.SkipCount,
                    FromDate = apiAnnouncement.FromDate,
                    ToDate = apiAnnouncement.ToDate,
                    AgentId = null
                };

                if (apiAnnouncement.Type == (int)AnnouncementTypes.Personal)
                    filter.ReceiverId = clientId;
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
                if (!Constants.ClaimingBonusTypes.Contains(bonus.BonusType))
                    throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.BonusNotFound);
                var client = CacheManager.GetClientById(clientId);
                var clientSegmentsIds = new List<int>();
                if (bonus.BonusSegmentSettings.Any())
                {
                    var clientClasifications = CacheManager.GetClientClasifications(client.Id);
                    if (clientClasifications.Any())
                        clientSegmentsIds = clientClasifications.Where(x => x.SegmentId.HasValue && x.ProductId == (int)Constants.PlatformProductId)
                                                                .Select(x => x.SegmentId.Value).ToList();
                }
                if ((bonus.BonusSegmentSettings.Any() &&
                    (bonus.BonusSegmentSettings.Any(x => x.Type == (int)BonusSettingConditionTypes.InSet && !clientSegmentsIds.Contains(x.SegmentId)) ||
                     bonus.BonusSegmentSettings.Any(x => x.Type == (int)BonusSettingConditionTypes.OutOfSet && clientSegmentsIds.Contains(x.SegmentId)))) ||
                    (bonus.BonusCountrySettings.Any() &&
                    (bonus.BonusCountrySettings.Any(y => y.Type == (int)BonusSettingConditionTypes.InSet && y.CountryId != client.RegionId) ||
                     bonus.BonusCountrySettings.Any(y => y.Type == (int)BonusSettingConditionTypes.OutOfSet && y.CountryId == client.RegionId))) ||
                    (bonus.BonusCountrySettings.Any() &&
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
                    BonusType = bonus.BonusType,
                    ClientId = client.Id,
                    ClientUserName = client.UserName,
                    ClientCurrencyId = client.CurrencyId,
                    AccountTypeId = bonus.AccountTypeId.Value,
                    ReusingMaxCount = bonus.ReusingMaxCount,
                    IgnoreEligibility = bonus.IgnoreEligibility,
                    ValidForAwarding = bonus.ValidForAwarding == null ? (DateTime?)null : DateTime.Now.AddHours(bonus.ValidForAwarding.Value),
                    ValidForSpending = bonus.ValidForSpending == null ? (DateTime?)null : DateTime.Now.AddHours(bonus.ValidForSpending.Value)
                };
                bonusService.GiveCompainToClient(input, out bool alreadyGiven);
                CacheManager.RemoveClientNotAwardedCampaigns(client.Id);
                Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.NotAwardedCampaigns, client.Id));
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
                    var bonus = documentBl.GetClientBonusById(input.BonusId);
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
                var resp = clientBl.GetClientLimitSettings(null);
                return new ApiResponseBase { ResponseObject = resp };
            }
        }

        private static ApiResponseBase SetLimits(ClientCustomSettings limits, int clientId, SessionIdentity session, ILog log)
        {
            using (var clientBl = new ClientBll(session, log))
            {
                limits.ClientId = clientId;
                clientBl.SaveClientLimitSettings(limits, null);
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
    }
}