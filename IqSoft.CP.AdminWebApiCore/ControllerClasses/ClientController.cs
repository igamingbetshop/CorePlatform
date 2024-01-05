using System;
using System.Linq;
using Newtonsoft.Json;
using log4net;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Filters;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.AdminWebApi.Filters;
using IqSoft.CP.AdminWebApi.Helpers;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.AdminWebApi.Models.ClientModels;
using IqSoft.CP.AdminWebApi.ClientModels.Models;
using IqSoft.CP.AdminWebApi.Models.CommonModels;
using IqSoft.CP.AdminWebApi.Models.NotificationModels;
using IqSoft.CP.AdminWebApi.Models.PaymentModels;
using IqSoft.CP.DAL.Models.Clients;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.AdminWebApi.Models;
using System.Collections.Generic;
using IqSoft.CP.AdminWebApi.Models.BonusModels;
using System.IO;
using static IqSoft.CP.Common.Constants;
using IqSoft.CP.Integration.Platforms.Helpers;
using IqSoft.CP.AdminWebApi.Filters.Clients;
using IqSoft.CP.AdminWebApi.Models.ProductModels;
using System.ServiceModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.DAL.Models.Notification;

namespace IqSoft.CP.AdminWebApi.ControllerClasses
{
    public static class ClientController
    {
        public static ApiResponseBase CallFunction(RequestBase request, SessionIdentity identity, ILog log, IWebHostEnvironment env, HttpRequest httpRequest)
        {
            switch (request.Method)
            {
                case "GetClients":
                    return GetfnClientsPagedModel(
                        JsonConvert.DeserializeObject<ApiFilterfnClient>(request.RequestData), identity, log);
                case "GetClientById":
                    {
                        if (int.TryParse(request.RequestObject.ToString(), out int clientId))
                            return GetClientById(clientId, identity, log);
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongClientId);
                    }
                case "GetClientConatctInfo":
                    {
                        if (int.TryParse(request.RequestObject.ToString(), out int clientId))
                            return GetClientConatctInfo(clientId, identity, log);
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongClientId);
                    }
                case "ChangeClientPassword":
                    return ChangeClientPassword(JsonConvert.DeserializeObject<NewPasswordInput>(request.RequestData),
                        identity, log);
                case "ChangeClientDetails":
                    return
                        ChangeClientDetails(
                            JsonConvert.DeserializeObject<ChangeClientDetailsInput>(request.RequestData), identity, log);
                case "ResetClientPassword":
                    return
                        ResetClientPassword(
                            JsonConvert.DeserializeObject<NewPasswordInput>(request.RequestData), identity, log);
                case "GetClientLogs":
                    return GetClientLogs(JsonConvert.DeserializeObject<ApiFilterClientLog>(request.RequestData),
                        identity, log);
                case "CreateDebitCorrection":
                    return
                        CreateDebitCorrection(JsonConvert.DeserializeObject<ClientCorrectionInput>(request.RequestData), identity, log);
                case "CreateCreditCorrection":
                    return
                        CreateCreditCorrection(JsonConvert.DeserializeObject<ClientCorrectionInput>(request.RequestData), identity, log);
                case "SendEmailToClient":
                    return
                        SendEmailToClient(
                            JsonConvert.DeserializeObject<ApiSendEmailToClientInput>(request.RequestData), identity, log);
                case "GetClientCorrections":
                    return
                        GetClientCorrections(
                            JsonConvert.DeserializeObject<ApiFilterClientCorrection>(request.RequestData), identity, log);
                case "GetClientAccounts":
                    {
                        if (int.TryParse(request.RequestObject.ToString(), out int clientId))
                            return GetClientAccounts(clientId, identity, log);
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongClientId);
                    }
                case "GetClientLoginsPagedModel":
                    return
                        GetClientLoginsPagedModel(
                            JsonConvert.DeserializeObject<FilterClientSession>(request.RequestData), identity, log);
                case "GetClientInfo":
                    {
                        if (int.TryParse(request.RequestObject.ToString(), out int clientId))
                            return GetClientInfo(clientId, identity, log);
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongClientId);
                    }
                case "GetClientAccountsBalanceHistoryPaging":
                    return
                        GetClientAccountsBalanceHistoryPaging(
                            JsonConvert.DeserializeObject<ApiFilterAccountsBalanceHistory>(request.RequestData),
                            identity, log);
                case "UploadImage":
                    return UploadImage(JsonConvert.DeserializeObject<AddClientIdentityModel>(request.RequestData),
                                       identity, log, env);
                case "GetClientIdentityModel":
                    {
                        if (int.TryParse(request.RequestObject.ToString(), out int clientId))
                            return GetClientIdentityModels(clientId, identity, log);
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongClientId);
                    }
                case "UpdateClientIdentityModel":
                    return
                        UpdateClientIdentityModel(
                            JsonConvert.DeserializeObject<AddClientIdentityModel>(request.RequestData), identity, log, env);
                case "RemoveClientIdentity":
                    return RemoveClientIdentity(Convert.ToInt32(request.RequestData), identity, log, env);
                case "SetPaymentLimit":
                    return SetPaymentLimit(JsonConvert.DeserializeObject<ApiPaymentLimit>(request.RequestData),
                        identity, log);
                case "GetPaymentLimit":
                    {
                        if (int.TryParse(request.RequestObject.ToString(), out int clientId))
                            return GetPaymentLimit(clientId, identity, log);
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongClientId);
                    }
                case "ExportClients":
                    return ExportClients(JsonConvert.DeserializeObject<ApiFilterfnClient>(request.RequestData), identity, log, env);
                case "ExportClientCorrections":
                    return
                        ExportClientCorrections(
                            JsonConvert.DeserializeObject<ApiFilterClientCorrection>(request.RequestData), identity, log, env);
                case "ExportClientAccounts":
                    {
                        if (int.TryParse(request.RequestObject.ToString(), out int clientId))
                            return ExportClientAccounts(clientId, identity, log, env);
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongClientId);
                    }
                case "ExportClientIdentity":
                    {
                        if (int.TryParse(request.RequestObject.ToString(), out int clientId))
                            return ExportClientIdentity(clientId, identity, log, env);
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongClientId);
                    }
                case "GetPaymentLimitExclusion":
                    {
                        if (int.TryParse(request.RequestObject.ToString(), out int clientId))
                            return GetPaymentLimitExclusion(clientId, identity, log);
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongClientId);
                    }
                case "SetPaymentLimitExclusion":
                    return SetPaymentLimitExclusion(
                        JsonConvert.DeserializeObject<ApiPaymentLimit>(request.RequestData), identity, log);
                case "GetClientPaymentAccountDetails":
                    return GetClientPaymentAccountDetails(JsonConvert.DeserializeObject<ClientIdentifierInfo>(request.RequestData), identity, log);
                case "UpdateClientPaymentAccount":
                    return UpdateClientPaymentAccount(JsonConvert.DeserializeObject<ApiClientPaymentInfo>(request.RequestData), identity, log);
                case "RegisterClient":
                    return RegisterClient(JsonConvert.DeserializeObject<NewClientModel>(request.RequestData), identity, log, httpRequest, env);
                case "GetEmails":
                    if (!string.IsNullOrEmpty(request.RequestData))
                        return GetEmails(JsonConvert.DeserializeObject<ApiFilterClientMessage>(request.RequestData), identity, log);
                    return GetEmails(new ApiFilterClientMessage(), identity, log);
                case "GetSmses":
                    if (!string.IsNullOrEmpty(request.RequestData))
                        return GetSmses(JsonConvert.DeserializeObject<ApiFilterClientMessage>(request.RequestData), identity, log);
                    return GetSmses(new ApiFilterClientMessage(), identity, log);
                case "GetAffiliateClientsOfManager":
                    return GetAffiliateClientsOfManager(JsonConvert.DeserializeObject<AffiliateClientsOfManagerInput>(request.RequestData), identity, log);
                case "ResetClientBankInfo":
                    {
                        if (int.TryParse(request.RequestObject.ToString(), out int clientId))
                            return ResetClientBankInfo(clientId, identity, log);
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongClientId);
                    }
                case "BlockClientPaymentSystem":
                    return BlockClientPaymentSystem(JsonConvert.DeserializeObject<ApiClientPaymentItem>(request.RequestData), identity, log);
                case "ActivateClientPaymentSystem":
                    return ActivateClientPaymentSystem(JsonConvert.DeserializeObject<ApiClientPaymentItem>(request.RequestData), identity, log);
                case "GetClientBlockedPayments":
                    {
                        if (int.TryParse(request.RequestObject.ToString(), out int clientId))
                            return GetClientBlockedPayments(clientId, identity, log);
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongClientId);
                    }
                case "ChangeClientPaymentSettingState":
                    return ChangeClientPaymentSettingState(JsonConvert.DeserializeObject<ApiClientPaymentItem>(request.RequestData), identity, log);
                case "GetClientPaymentSettings":
                    {
                        if (int.TryParse(request.RequestObject.ToString(), out int clientId))
                            return GetClientPaymentSettings(clientId, identity, log);
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongClientId);
                    }
                case "GiveBonusToClient":
                    return GiveBonusToClient(JsonConvert.DeserializeObject<ApiClientBonusInput>(request.RequestData), identity, log);
                case "ChangeClientBonusTrigger":
                    return ChangeClientBonusTrigger(JsonConvert.DeserializeObject<ApiClientTrigger>(request.RequestData), identity, log);
                case "GetClientTriggers":
                    return GetClientTriggers(JsonConvert.DeserializeObject<ApiClientBonusInput>(request.RequestData), identity, log);
                case "GetClientBonuses":
                    return GetClientBonuses(JsonConvert.DeserializeObject<ApiClientBonusInput>(request.RequestData), identity, log);
                case "CancelClientBonus":
                    {
                        if (int.TryParse(request.RequestObject.ToString(), out int clientId))
                            return CancelClientBonus(clientId, identity, log);
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongClientId);
                    }
                case "ApproveClientCashbackBonus":
                    {
                        if (int.TryParse(request.RequestObject.ToString(), out int clientBonusId))
                            return ApproveClientCashbackBonus(clientBonusId, identity, log);
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.BonusNotFound);
                    }
                case "CancelFreeSpin":
                    return CancelFreeSpin(Convert.ToInt32(request.RequestData), identity, log);
                case "SendMessageToClients":
                    return SendMessageToClients(JsonConvert.DeserializeObject<ApiSendMessageToClientsInput>(request.RequestData), identity, log);
                case "GetClientSettings":
                    {
                        if (int.TryParse(request.RequestObject.ToString(), out int clientId))
                            return GetClientSettings(clientId, identity, log);
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongClientId);
                    }
                case "UpdateClientSettings":
                    return UpdateClientSettings(request.ClientId.Value, JsonConvert.DeserializeObject<List<ApiClientSetting>>(request.RequestData), identity, log);
                case "GetClientLimitSettings":
                    return GetClientLimitSettings(Convert.ToInt32(request.RequestData), identity, log);
                case "SaveClientLimitSettings":
                    return SaveClientLimitSettings(JsonConvert.DeserializeObject<ClientCustomSettings>(request.RequestData), identity, log);
                case "ApplySystemExclusion":
                    return ApplySystemExclusion(JsonConvert.DeserializeObject<ApiExlusionModel>(request.RequestData), identity, log);
                case "GetSegmentClients":
                    return GetSegmentClients(JsonConvert.DeserializeObject<ApiFilterfnSegmentClient>(request.RequestData), identity, log);
                case "SaveClientGameProviderSetting":
                    return SaveClientGameProviderSetting(JsonConvert.DeserializeObject<ApiGameProviderSetting>(request.RequestData), identity, log);
                case "GetClientGameProviderSettings":
                    return GetClientGameProviderSettings(Convert.ToInt32(request.RequestData), identity, log);
                case "SaveClientCategory":
                    return SaveClientCategory(JsonConvert.DeserializeObject<ApiClientCategory>(request.RequestData), identity, log);
                case "GetClientCategories":
                    return GetClientCategories(identity, log);
                case "DeleteClientCategory":
                    return DeleteClientCategory(Convert.ToInt32(request.RequestData), identity, log);
            }
            throw BaseBll.CreateException(string.Empty, Constants.Errors.MethodNotFound);
        }

        private static ApiResponseBase GetfnClientsPagedModel(ApiFilterfnClient filter, SessionIdentity identity, ILog log)
        {
            using var clientBl = new ClientBll(identity, log);
            var hideClientContactInfo = false;
            try
            {
                clientBl.CheckPermission(Constants.Permissions.ViewClientContactInfoList);
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                if (fex.Detail.Id == Constants.Errors.DontHavePermission)
                    hideClientContactInfo = true;
            }
            var input = filter.MapToFilterfnClient();
            var resp = clientBl.GetfnClientsPagedModel(input, true);
            return new ApiResponseBase
            {
                ResponseObject = new { resp.Count, Entities = resp.Entities.MapTofnClientModelList(identity.TimeZone, hideClientContactInfo) }
            };
        }

        private static ApiResponseBase GetClientConatctInfo(int clientId, SessionIdentity identity, ILog log)
        {
            var client = CacheManager.GetClientById(clientId);
            if (client == null)
               throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.ClientNotFound);
            using var clientBl = new ClientBll(identity, log);
            try
            {
                clientBl.CheckPermission(Constants.Permissions.ViewClientContactInfo);
                var checkClientPermission = clientBl.GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewClient,
                    ObjectTypeId = ObjectTypes.Client
                });
                if (!checkClientPermission.HaveAccessForAllObjects && checkClientPermission.AccessibleObjects.All(x => x != clientId))
                    throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.DontHavePermission);
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                if (fex.Detail.Id == Constants.Errors.DontHavePermission)
                    return new ApiResponseBase
                    {
                        ResponseObject = new { Email = "*****", MobileNumber = "*****" }
                    };
            }

            return new ApiResponseBase
            {
                ResponseObject = new { client.Email, client.MobileNumber }
            };
        }

        private static ApiResponseBase GetClientById(int id, SessionIdentity identity, ILog log)
        {
            using var clientBl = new ClientBll(identity, log);
            using var regionBl = new RegionBll(identity, log);
            var hideClientContactInfo = false;
            try
            {
                clientBl.CheckPermission(Constants.Permissions.ViewClientContactInfo);
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                if (fex.Detail.Id == Constants.Errors.DontHavePermission)
                    hideClientContactInfo = true;
            }

            var resp = clientBl.GetClientById(id, true).MapToClientModel(hideClientContactInfo, identity.TimeZone);
            var regionPath = regionBl.GetRegionPath(resp.RegionId);
            resp.CountryId = regionPath.FirstOrDefault(x => x.TypeId == (int)RegionTypes.Country)?.Id;
            resp.DistrictId = regionPath.FirstOrDefault(x => x.TypeId == (int)RegionTypes.District)?.Id;
            resp.CityId = regionPath.FirstOrDefault(x => x.TypeId == (int)RegionTypes.City)?.Id;
            resp.TownId =regionPath.FirstOrDefault(x => x.TypeId == (int)RegionTypes.Town)?.Id;
            return new ApiResponseBase
            {
                ResponseObject = new { Client = resp }
            };
        }

        private static ApiResponseBase ChangeClientDetails(ChangeClientDetailsInput request, SessionIdentity identity, ILog log)
        {
            bool isBonusAccepted = false;
            bool isSessionExpired = false;
            using (var clientBl = new ClientBll(identity, log))
            {
                var dbClient = clientBl.GetClientById(request.Id);
                if (dbClient == null)
                    throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.ClientNotFound);

                clientBl.ChangeClientDetails(request.MapToClient(dbClient), out isBonusAccepted, out isSessionExpired);
            }
            CacheManager.RemoveClientFromCache(request.Id);
            Helpers.Helpers.InvokeMessage("RemoveClient", request.Id);

            var client = CacheManager.GetClientById(request.Id);
            if (client == null)
                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.ClientNotFound);

            if (isBonusAccepted)
            {
                CacheManager.RemoveClientBalance(request.Id);
                Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.ClientBalance, request.Id));
            }
            if (isSessionExpired)
                Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.ClientSessions, request.Id));

            return new ApiResponseBase();
        }

        private static ApiResponseBase ChangeClientPassword(NewPasswordInput input, SessionIdentity identity, ILog log)
        {
            var response = new ApiResponseBase { ResponseObject = new object() };
            var client = CacheManager.GetClientById(input.ClientId);
            var partner = CacheManager.GetPartnerById(client.PartnerId);
            identity.Domain = partner.SiteUrl.Split(',')[0];
            using var clientBl = new ClientBll(identity, log);
            clientBl.ChangeClientPassword(input);
            CacheManager.RemoveClientFromCache(input.ClientId);
            Helpers.Helpers.InvokeMessage("RemoveClient", input.ClientId);
            Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}_{2}", Constants.CacheItems.ClientSettings, input.ClientId, "PasswordChangedDate"));
            return response;
        }

        private static ApiResponseBase ResetClientPassword(NewPasswordInput request, SessionIdentity identity, ILog log)
        {
            var response = new ApiResponseBase { ResponseObject = new object() };
            var client = CacheManager.GetClientById(request.ClientId);
            var partner = CacheManager.GetPartnerById(client.PartnerId);
            identity.Domain = partner.SiteUrl.Split(',')[0];
            using var clientBl = new ClientBll(identity, log);
            clientBl.ChangeClientPassword(request);
            CacheManager.RemoveClientFromCache(request.ClientId);
            Helpers.Helpers.InvokeMessage("RemoveClient", request.ClientId);
            Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}_{2}", Constants.CacheItems.ClientSettings, request.ClientId, "PasswordChangedDate"));
            Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}_{2}", ClientSettings.BlockedForInactivity, request.ClientId, "BlockedForInactivity"));
            return response;
        }

        private static ApiResponseBase GetClientLogs(ApiFilterClientLog request, SessionIdentity identity, ILog log)
        {
            using var clientBl = new ClientBll(identity, log);
            var resp = clientBl.GetClientLogs(request.MapToFilterClientLog());
            return new ApiResponseBase
            {
                ResponseObject = new { resp.Count, Entities = resp.Entities.MapToApiClientLogs(clientBl.GetUserIdentity().TimeZone) }
            };
        }

        private static ApiResponseBase CreateDebitCorrection(ClientCorrectionInput input, SessionIdentity identity, ILog log)
        {
            using var clientBl = new ClientBll(identity, log);
            using var documentBl = new DocumentBll(clientBl);
            using var userBl = new UserBll(clientBl);
            var result = clientBl.CreateDebitCorrectionOnClient(input, documentBl, true);
            Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.ClientBalance, result.ClientId));
            return new ApiResponseBase
            {
                ResponseObject = result.MapToDocumentModel(clientBl.GetUserIdentity().TimeZone)
            };
        }

        private static ApiResponseBase CreateCreditCorrection(ClientCorrectionInput input, SessionIdentity identity, ILog log)
        {
            using var clientBl = new ClientBll(identity, log);
            using var documentBl = new DocumentBll(clientBl);
            using var userBl = new UserBll(clientBl);
            var result = clientBl.CreateCreditCorrectionOnClient(input, documentBl, true);
            Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.ClientBalance, result.ClientId));
            return new ApiResponseBase
            {
                ResponseObject = result.MapToDocumentModel(clientBl.GetUserIdentity().TimeZone)
            };
        }

        public static ApiResponseBase GetClientCorrections(ApiFilterClientCorrection filter, SessionIdentity identity, ILog log)
        {
            using var reportBl = new ReportBll(identity, log);
            using var userBl = new UserBll(reportBl);
            var user = userBl.GetUserById(identity.Id);
            if (user == null)
                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.UserNotFound);
            var corrections = reportBl.GetClientCorrections(filter.MapToFilterCorrection(), true);
            return new ApiResponseBase
            {
                ResponseObject = corrections.MapToApiClientCorrections(identity.TimeZone)
            };
        }

        public static ApiResponseBase GetClientAccounts(int clientId, SessionIdentity identity, ILog log)
        {
            using var clientBl = new ClientBll(identity, log);
            var accounts = clientBl.GetClientAccounts(clientId, true).MapToFnAccountModels();
            return new ApiResponseBase
            {
                ResponseObject = accounts
            };
        }

        private static ApiResponseBase GetClientLoginsPagedModel(FilterClientSession filter, SessionIdentity identity, ILog log)
        {
            using var clientBl = new ClientBll(identity, log);
            filter.ProductId = Constants.PlatformProductId;
            var resp = clientBl.GetClientLoginsPagedModel(filter);
            return new ApiResponseBase
            {
                ResponseObject = new { resp.Count, Entities = resp.Entities.MapToClientSessionModels(clientBl.GetUserIdentity().TimeZone) }
            };
        }

        public static ApiResponseBase GetClientInfo(int clientId, SessionIdentity identity, ILog log)
        {
            using var clientBl = new ClientBll(identity, log);
            var hideClientContactInfo = false;
            try
            {
                clientBl.CheckPermission(Constants.Permissions.ViewClientContactInfo);
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                if (fex.Detail.Id == Constants.Errors.DontHavePermission)
                    hideClientContactInfo = true;
            }
            return new ApiResponseBase
            {
                ResponseObject = clientBl.GetClientInfo(clientId, true).MapToClientInfoModel(hideClientContactInfo)
            };
        }

        private static ApiResponseBase SendEmailToClient(ApiSendEmailToClientInput input, SessionIdentity identity, ILog log)
        {
            using var notificationBll = new NotificationBll(identity, log);
            var client = CacheManager.GetClientById(input.ClientId);
            if (client == null || client.Id == 0)
                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.ClientNotFound);
            notificationBll.SaveEmailMessage(client.PartnerId, input.ClientId, client.Email, input.Subject, input.Message, null);
            return new ApiResponseBase
            {
                ResponseObject = true
            };
        }

        private static ApiResponseBase GetClientAccountsBalanceHistoryPaging(
            ApiFilterAccountsBalanceHistory input, SessionIdentity identity, ILog log)
        {
            using var clientBl = new ClientBll(identity, log);
            var filter = input.MapToFilterAccountsBalanceHistory(identity.TimeZone);
            return new ApiResponseBase
            {
                ResponseObject =
                    clientBl.GetClientAccountsBalanceHistoryPaging(filter)
                        .Select(x => x.MapToApiAccountsBalanceHistoryElement(identity.TimeZone))
                        .ToList()
            };
        }

        private static ApiResponseBase RemoveClientIdentity(int clientIdentityId, SessionIdentity identity, ILog log, IWebHostEnvironment env)
        {
            using var clientBl = new ClientBll(identity, log);
            var clientIdentity = clientBl.RemoveClientIdentity(clientIdentityId, true);
            var path = string.Format("{0}/{1}/", env.ContentRootPath, clientIdentity.ImagePath);

            if (File.Exists(path))
                File.Delete(path);
            return new ApiResponseBase();
        }

        private static ApiResponseBase UploadImage(AddClientIdentityModel input, SessionIdentity identity, ILog log, IWebHostEnvironment env)
        {
            if (string.IsNullOrEmpty(input.ImageData))
                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongInputParameters);
            using var clientBl = new ClientBll(identity, log);
            var resp = clientBl.SaveKYCDocument(input.ToClientIdentity(identity.TimeZone), input.Name, Convert.FromBase64String(input.ImageData), true, env)
                                  .ToClientIdentityModel(identity.TimeZone);
            Helpers.Helpers.InvokeMessage("RemoveClient", input.ClientId);
            return new ApiResponseBase
            {
                ResponseObject = new
                {
                    ClientIdentityData = resp
                }
            };
        }

        private static ApiResponseBase GetClientIdentityModels(int clientId, SessionIdentity identity, ILog log)
        {
            using var clientBl = new ClientBll(identity, log);
            var clientIdentity = clientBl.GetClientIdentityInfo(clientId, true);
            var clientIdentityModelList = clientIdentity.Select(x => x.ToClientIdentityModel(identity.TimeZone)).ToList();

            return new ApiResponseBase
            {
                ResponseObject = clientIdentityModelList
            };
        }

        private static ApiResponseBase UpdateClientIdentityModel(AddClientIdentityModel input, SessionIdentity identity, ILog log, IWebHostEnvironment env)
        {
            using var clientBl = new ClientBll(identity, log);
            var resp = clientBl.SaveKYCDocument(input.ToClientIdentity(identity.TimeZone), input.Name, null, true, env)
                                    .ToClientIdentityModel(identity.TimeZone);
            Helpers.Helpers.InvokeMessage("RemoveClient", input.ClientId);
            return new ApiResponseBase
            {
                ResponseObject = new
                {
                    ClientIdentityData = resp
                }
            };
        }

        private static ApiResponseBase SetPaymentLimit(ApiPaymentLimit paymentLimit, SessionIdentity identity, ILog log)
        {
            using var clientBl = new ClientBll(identity, log);
            return clientBl.SetPaymentLimit(paymentLimit.MapToPaymentLimit(), true).MapToApiResponseBase();
        }

        private static ApiResponseBase GetPaymentLimit(int clientId, SessionIdentity identity, ILog log)
        {
            using var clientBl = new ClientBll(identity, log);
            return new ApiResponseBase
            {
                ResponseObject = clientBl.GetPaymentLimit(clientId).MapToApiPaymentLimit(clientId)
            };
        }
        private static ApiResponseBase ExportClients(ApiFilterfnClient filter, SessionIdentity identity, ILog log, IWebHostEnvironment env)
        {
            using var clientBl = new ClientBll(identity, log);
            var hideClientContactInfo = false;
            try
            {
                clientBl.CheckPermission(Constants.Permissions.ViewClientContactInfoList);
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                if (fex.Detail.Id == Constants.Errors.DontHavePermission)
                    hideClientContactInfo = true;
            }
            var input = filter.MapToFilterfnClient();
            var result = clientBl.ExportClients(input).MapTofnClientModelList(identity.TimeZone, hideClientContactInfo);
            string fileName = "ExportClients.csv";
            string fileAbsPath = clientBl.ExportToCSV<fnClientModel>(fileName, result, filter.CreatedFrom, filter.CreatedBefore, identity.TimeZone, env);

            return new ApiResponseBase
            {
                ResponseObject = new
                {
                    ExportedFilePath = fileAbsPath
                }
            };
        }

        private static ApiResponseBase ExportClientCorrections(ApiFilterClientCorrection filter, SessionIdentity identity, ILog log, IWebHostEnvironment env)
        {
            using var reportBl = new ReportBll(identity, log);
            var corrections = reportBl.ExportClientCorrections(filter.MapToFilterCorrection());
            var fileName = "ExportClientCorrections.csv";
            var fileAbsPath = reportBl.ExportToCSV<fnCorrection>(fileName, corrections, null, null, reportBl.GetUserIdentity().TimeZone, env);

            return new ApiResponseBase
            {
                ResponseObject = new
                {
                    ExportedFilePath = fileAbsPath
                }
            };
        }

        private static ApiResponseBase ExportClientAccounts(int clientId, SessionIdentity identity, ILog log, IWebHostEnvironment env)
        {
            using var reportBl = new ReportBll(identity, log);
            var accounts = reportBl.ExportClientAccounts(new FilterfnAccount
            {
                ObjectId = clientId
            }).MapToFnAccountModels();

            var fileName = "ExportClientAccounts.csv";
            var fileAbsPath = reportBl.ExportToCSV<FnAccountModel>(fileName, accounts, null, null, 0, env);
            return new ApiResponseBase
            {
                ResponseObject = new
                {
                    ExportedFilePath = fileAbsPath
                }
            };
        }

        private static ApiResponseBase ExportClientIdentity(int clientId, SessionIdentity identity, ILog log, IWebHostEnvironment env)
        {
            using var clientBl = new ClientBll(identity, log);
            var clientIdentity = clientBl.ExportClientIdentity(clientId);
            var clientIdentityModelList = clientIdentity.Select(x => x.ToClientIdentityModel(identity.TimeZone)).ToList();
            var fileName = "ExportClientIdentity.csv";
            var fileAbsPath = clientBl.ExportToCSV<ClientIdentityModel>(fileName, clientIdentityModelList, null, null, 0, env);

            return new ApiResponseBase
            {
                ResponseObject = new
                {
                    ExportedFilePath = fileAbsPath
                }
            };
        }

        private static ApiResponseBase GetPaymentLimitExclusion(int clientId, SessionIdentity identity, ILog log)
        {
            using var clientBl = new ClientBll(identity, log);
            return new ApiResponseBase
            {
                ResponseObject = clientBl.GetPaymentLimitExclusion(clientId, true).MapToApiPaymentLimit(clientId)
            };
        }

        private static ApiResponseBase SetPaymentLimitExclusion(ApiPaymentLimit paymentLimit, SessionIdentity identity, ILog log)
        {
            using var clientBl = new ClientBll(identity, log);
            return clientBl.SetPaymentLimitExclusion(paymentLimit.MapToPaymentLimit()).MapToApiResponseBase();
        }

        private static ApiResponseBase GetClientPaymentAccountDetails(ClientIdentifierInfo clientInput, SessionIdentity identity, ILog log)
        {
            using var clientBl = new ClientBll(identity, log);
            BllClient client = null;
            if (Int32.TryParse(clientInput.ClientIdentifier, out int clientId))
                client = CacheManager.GetClientById(clientId);
            else
                client = CacheManager.GetClientByUserName(clientInput.PartnerId, clientInput.ClientIdentifier);
            if (client == null)
                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.ClientNotFound);
            return new ApiResponseBase
            {
                ResponseObject = clientBl.GetClientPaymentAccountDetails(client.Id, null,
                Enum.GetValues(typeof(ClientPaymentInfoTypes)).Cast<int>().ToList(),
                true).Select(x => x.MapToApiClientPaymentInfo(identity.TimeZone)).ToList()
            };
        }

        private static ApiResponseBase RegisterClient(NewClientModel input, SessionIdentity identity, ILog log, HttpRequest httpRequest, IWebHostEnvironment env)
        {
            using var clientBl = new ClientBll(identity, log);
            using var regionBl = new RegionBll(identity, log);
            var ip = httpRequest.Headers.ContainsKey("CF-Connecting-IP") ? httpRequest.Headers["CF-Connecting-IP"].ToString() : Constants.DefaultIp;
            var client = input.MapToClient();
            client.RegistrationIp = ip;
            clientBl.CheckPermission(Constants.Permissions.CreateClient);

            if (client.RegionId == 0 && httpRequest.Headers.ContainsKey("CF-IPCountry"))
            {
                var region = regionBl.GetRegionByCountryCode(httpRequest.Headers["CF-IPCountry"]);
                if (region != null)
                    client.RegionId = region.Id;
            }
            client = clientBl.RegisterClient(new ClientRegistrationInput { ClientData = client, IsQuickRegistration = false, IsFromAdmin = true }, env);

            return new ApiResponseBase { ResponseObject = client.MapTofnClientModel() };
        }

        private static ApiResponseBase GetEmails(ApiFilterClientMessage apiFilterClientMessage, SessionIdentity identity, ILog log)
        {
            using var clientBl = new ClientBll(identity, log);
            var filter = apiFilterClientMessage.MapToFilterClientMessage();
            filter.Types = new List<int> { (int)ClientMessageTypes.Email, (int)ClientMessageTypes.SecuredEmail };
            var resp = clientBl.GetClientMessages(filter, true);
            return new ApiResponseBase
            {
                ResponseObject = new
                {
                    Entities = resp.Entities.MapToClientMessage(identity.TimeZone),
                    resp.Count
                }
            };
        }

        private static ApiResponseBase GetSmses(ApiFilterClientMessage apiFilterClientMessage, SessionIdentity identity, ILog log)
        {
            using var clientBl = new ClientBll(identity, log);
            var filter = apiFilterClientMessage.MapToFilterClientMessage();
            filter.Types = new List<int> { (int)ClientMessageTypes.Sms, (int)ClientMessageTypes.SecuredSms };
            var resp = clientBl.GetClientMessages(filter, true);
            return new ApiResponseBase
            {
                ResponseObject = new
                {
                    Entities = resp.Entities.MapToClientMessage(identity.TimeZone),
                    resp.Count
                }
            };
        }

        private static ApiResponseBase GetAffiliateClientsOfManager(AffiliateClientsOfManagerInput input, SessionIdentity session, ILog log)
        {
            using var clientBl = new ClientBll(session, log);
            return new ApiResponseBase
            {
                ResponseObject = clientBl.GetClientsOfAffiliateManager(input.ManagerId, input.Hours)
            };
        }

        private static ApiResponseBase ResetClientBankInfo(int clientId, SessionIdentity session, ILog log)
        {
            using var clientBl = new ClientBll(session, log);
            clientBl.ResetClientBankInfo(clientId);
            return new ApiResponseBase();
        }

        private static ApiResponseBase ChangeClientPaymentSettingState(ApiClientPaymentItem apiClientPaymentItem, SessionIdentity session, ILog log)
        {
            using var clientBl = new ClientBll(session, log);
            clientBl.ChangeClientPaymentSettingState(apiClientPaymentItem.ClientId, apiClientPaymentItem.PartnerPaymentSettingId, apiClientPaymentItem.State);
            return new ApiResponseBase();
        }

        // change to ChangeClientPaymentSettingState
        private static ApiResponseBase BlockClientPaymentSystem(ApiClientPaymentItem apiChangeClientPaymentState, SessionIdentity session, ILog log)
        {
            using var clientBl = new ClientBll(session, log);
            var resp = clientBl.BlockClientPaymentSettingState(apiChangeClientPaymentState.ClientId, apiChangeClientPaymentState.PartnerPaymentSettingId);

            return new ApiResponseBase
            {
                ResponseObject = new ApiClientPaymentItem
                {
                    Id = resp.Id,
                    ClientId = apiChangeClientPaymentState.ClientId,
                    PartnerPaymentSettingId = resp.PartnerPaymentSettingId,
                    Type = resp.PartnerPaymentSetting.Type,
                    PaymentSystem = resp.PartnerPaymentSetting.PaymentSystem.Name,
                    CurrencyId = resp.PartnerPaymentSetting.CurrencyId
                }
            };
        }

        // change to ChangeClientPaymentSettingState
        private static ApiResponseBase ActivateClientPaymentSystem(ApiClientPaymentItem apiChangeClientPaymentState, SessionIdentity session, ILog log)
        {
            using var clientBl = new ClientBll(session, log);
            clientBl.ActivateClientPaymentSetting(apiChangeClientPaymentState.Id);
            return new ApiResponseBase { ResponseObject = apiChangeClientPaymentState.Id };
        }

        //changed to GetClientPaymentSettings
        private static ApiResponseBase GetClientBlockedPayments(int clientId, SessionIdentity session, ILog log)
        {
            using var clientBl = new ClientBll(session, log);
            var response = clientBl.GetClientPaymentSettings(clientId, (int)ClientPaymentStates.Blocked, true);
            return new ApiResponseBase
            {
                ResponseObject = response.Select(x => new ApiClientPaymentItem
                {
                    Id = x.Id,
                    ClientId = x.ClientId,
                    PartnerPaymentSettingId = x.PartnerPaymentSettingId,
                    Type = x.PartnerPaymentSetting.Type,
                    PaymentSystem = x.PartnerPaymentSetting.PaymentSystem.Name,
                    CurrencyId = x.PartnerPaymentSetting.CurrencyId
                }).ToList()
            };
        }

        private static ApiResponseBase GetClientPaymentSettings(int clientId, SessionIdentity session, ILog log)
        {
            using var clientBl = new ClientBll(session, log);
            return new ApiResponseBase
            {
                ResponseObject = clientBl.GetClientPaymentSettings(clientId, null, true)
                                         .Select(x => new ApiClientPaymentItem
                                         {
                                             Id = x.Id,
                                             ClientId = x.ClientId,
                                             PartnerPaymentSettingId = x.PartnerPaymentSettingId,
                                             Type = x.PartnerPaymentSetting.Type,
                                             PaymentSystem = x.PartnerPaymentSetting.PaymentSystem.Name,
                                             CurrencyId = x.PartnerPaymentSetting.CurrencyId,
                                             State = x.State
                                         }).ToList()
            };
        }

        private static ApiResponseBase GiveBonusToClient(ApiClientBonusInput apiClientBonus, SessionIdentity session, ILog log)
        {
            using (var bonusService = new BonusService(session, log))
            {
                var client = CacheManager.GetClientById(apiClientBonus.ClientId);
                if (client == null)
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.ClientNotFound);
                var bonus = bonusService.GetAvailableBonus(apiClientBonus.BonusSettingId, true);

                if (!Constants.ClaimingBonusTypes.Contains(bonus.BonusType))
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.BonusNotFound);

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
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.NotAllowed);

                var input = new Common.Models.Bonus.ClientBonusItem
                {
                    PartnerId = client.PartnerId,
                    BonusId = bonus.Id,
                    BonusType = bonus.BonusType,
                    ClientId = client.Id,
                    ClientUserName = client.UserName,
                    ClientCurrencyId = client.CurrencyId,
                    AccountTypeId = bonus.AccountTypeId ?? (int)AccountTypes.ClientUsedBalance,
                    ReusingMaxCount = bonus.ReusingMaxCount,
                    IgnoreEligibility = bonus.IgnoreEligibility,
                    ValidForAwarding = bonus.ValidForAwarding == null ? (DateTime?)null : DateTime.Now.AddHours(bonus.ValidForAwarding.Value),
                    ValidForSpending = bonus.ValidForSpending == null ? (DateTime?)null : DateTime.Now.AddHours(bonus.ValidForSpending.Value)
                };
                bonusService.GiveCompainToClient(input, out bool alreadyGiven);
                if (alreadyGiven)
                    throw BaseBll.CreateException(session.LanguageId, Constants.Errors.NotAllowed);

                CacheManager.RemoveClientNotAwardedCampaigns(client.Id);
                Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.NotAwardedCampaigns, client.Id));
            }
            return new ApiResponseBase();
        }

        private static ApiResponseBase ChangeClientBonusTrigger(ApiClientTrigger apiClientTrigger, SessionIdentity session, ILog log)
        {
            using var clientBl = new ClientBll(session, log);
            var clientBonus = clientBl.GetClientBonusById(apiClientTrigger.ClientBonusId);
            if (clientBonus == null)
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.BonusNotFound);

            var resp = clientBl.ChangeClientBonusTriggerManually(apiClientTrigger.ClientId, apiClientTrigger.TriggerId, apiClientTrigger.BonusId,
                clientBonus.ReuseNumber ?? 1, apiClientTrigger.SourceAmount, apiClientTrigger.Status);

            return new ApiResponseBase
            {
                ResponseObject = resp.MapToApiClientTrigger()
            };
        }

        private static ApiResponseBase GetClientTriggers(ApiClientBonusInput input, SessionIdentity session, ILog log)
        {
            using var clientBl = new ClientBll(session, log);
            var resp = clientBl.GetClientBonusTriggers(input.ClientId, input.BonusSettingId, input.ReuseNumber, true)
                               .Select(x => x.MapToApiTriggerSetting(session.TimeZone, input.ClientId)).ToList();
            return new ApiResponseBase
            {
                ResponseObject = new { Triggers = resp }
            };
        }

        private static ApiResponseBase GetClientBonuses(ApiClientBonusInput input, SessionIdentity session, ILog log)
        {
            using var clientBl = new ClientBll(session, log);
            var clientBonuses = clientBl.GetClientBonuses(input.ClientId, null);
            return new ApiResponseBase
            {
                ResponseObject = clientBonuses.MapToApiClientBonuses(session.TimeZone)
            };
        }

        private static ApiResponseBase CancelClientBonus(int clientBonusId, SessionIdentity session, ILog log)
        {
            using var clientBl = new ClientBll(session, log);
            var clientBonus = clientBl.CancelClientBonus(clientBonusId, true);
            CacheManager.RemoveClientBalance(clientBonus.ClientId);
            CacheManager.RemoveClientActiveBonus(clientBonus.ClientId);
            CacheManager.RemoveClientBonus(clientBonus.ClientId, clientBonus.BonusId);
            Helpers.Helpers.InvokeMessage("ClientBonus", clientBonus.ClientId);
            return new ApiResponseBase();
        }

        private static ApiResponseBase ApproveClientCashbackBonus(int clientBonusId, SessionIdentity session, ILog log)
        {
            using var clientBl = new ClientBll(session, log);
            var clientBonus = clientBl.ApproveClientCashbackBonus(clientBonusId);
            CacheManager.RemoveClientActiveBonus(clientBonus.ClientId);
            CacheManager.RemoveClientBonus(clientBonus.ClientId, clientBonus.BonusId);
            Helpers.Helpers.InvokeMessage("ClientBonus", clientBonus.ClientId);
            return new ApiResponseBase
            {
                ResponseObject = clientBonus.MapToApiClientBonus(session.TimeZone)
            };
        }

        private static ApiResponseBase CancelFreeSpin(int clientBonusId, SessionIdentity session, ILog log)
        {
            using var clientBl = new ClientBll(session, log);
            using var bonusBl = new BonusService(clientBl);
            var clientFreeSpin = clientBl.CancelClientFreespin(clientBonusId, true);
            var client = CacheManager.GetClientById(clientFreeSpin.ClientId);
            var bonus = bonusBl.GetBonusById(clientFreeSpin.BonusId);
            if (bonus == null)
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.BonusNotFound);
            var bonusProduct = bonus.BonusProducts.FirstOrDefault(x => x.BonusId == bonus.Id);
            if (bonusProduct == null)
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.BonusNotFound);
            var product = CacheManager.GetProductById(bonusProduct.ProductId);
            if (product == null)
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.ProductNotFound);

            var gameProvider = CacheManager.GetGameProviderById(product.GameProviderId.Value);
            switch (gameProvider.Name)
            {
                case Constants.GameProviders.TwoWinPower:
                    Integration.Products.Helpers.TwoWinPowerHelpers.CancelFreeSpin(client.PartnerId, clientBonusId);
                    break;
                case Constants.GameProviders.BlueOcean:
                    Integration.Products.Helpers.BlueOceanHelpers.CancelFreeRound(client.PartnerId, clientBonusId.ToString()/*must be bo id*/);
                    break;
                case Constants.GameProviders.PragmaticPlay:
                    Integration.Products.Helpers.PragmaticPlayHelpers.CanceFreeRound(client.PartnerId, clientFreeSpin.BonusId);
                    break;
                default:
                    break;
            }
            return new ApiResponseBase();
        }

        private static ApiResponseBase SendMessageToClients(ApiSendMessageToClientsInput input, SessionIdentity session, ILog log)
        {
            using var clientBl = new ClientBll(session, log);
            var filteredClients = input.MapToFilterfnClient();
            if (string.IsNullOrWhiteSpace(input.Message) || string.IsNullOrWhiteSpace(input.Subject))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongInputParameters);

            var clients = clientBl.GetfnClientsPagedModel(filteredClients, true).Entities.GroupBy(x => x.PartnerId).Select(x => new { x.Key, ClientIds = x.Select(y => y.Id) });
            foreach (var clientsByParter in clients)
            {
                var ticket = new Ticket
                {
                    PartnerId = clientsByParter.Key,
                    Type = (int)TicketTypes.Discussion,
                    Subject = input.Subject,
                    Status = (int)MessageTicketState.Active
                };
                var message = new TicketMessage
                {
                    Message = input.Message,
                    Type = (int)ClientMessageTypes.MessageFromUser,
                    UserId = session.Id
                };

                clientBl.OpenTickets(ticket, message, clientsByParter.ClientIds.ToList(), true);
            }
            return new ApiResponseBase();
        }

        private static ApiResponseBase UpdateClientPaymentAccount(ApiClientPaymentInfo input, SessionIdentity session, ILog log)
        {
            using var clientBl = new ClientBll(session, log);
            return new ApiResponseBase
            {
                ResponseObject = clientBl.UpdateClientPaymentAccount(input.MapToClientPaymentInfo()).MapToApiClientPaymentInfo(session.TimeZone)
            };
        }

        private static ApiResponseBase GetClientSettings(int clientId, SessionIdentity session, ILog log)
        {
            using var clientBl = new ClientBll(session, log);
            return new ApiResponseBase
            {
                ResponseObject = clientBl.GetClientsSettings(clientId, true).Select(x => new ApiClientSetting
                {
                    Name = x.Name,
                    StringValue = !string.IsNullOrEmpty(x.StringValue) ? x.StringValue :
                    (x.NumericValue.HasValue ? x.NumericValue.Value.ToString() : string.Empty),
                    DateValue = x.DateValue == null ? x.CreationTime : x.DateValue,
                    LastUpdateTime = x.LastUpdateTime
                }).ToList()
            };
        }

        private static ApiResponseBase UpdateClientSettings(int clientId, List<ApiClientSetting> input, SessionIdentity session, ILog log)
        {
            using var clientBl = new ClientBll(session, log);
            using var regionBl = new RegionBll(session, log);
            var editableSettings = new List<string>
                    {
                        ClientSettings.UnusedAmountWithdrawPercent,
                        ClientSettings.CasinoLayout,
                        ClientSettings.IsAffiliateManager,
                        ClientSettings.BlockedForBonuses,
                        ClientSettings.AMLVerified,
                        ClientSettings.AMLProhibited,
                        ClientSettings.BlockedForInactivity,
                        ClientSettings.CautionSuspension,
                        ClientSettings.AMLPercent
                    };
            var settingsNames = typeof(Constants.ClientSettings).GetFields().Select(x => x.Name).ToList();
            var result = clientBl.UpdateClientSettings(clientId, input.Where(x => (settingsNames.Contains(x.Name) && x.Name.Contains("Limit")) ||
                                                                 editableSettings.Contains(x.Name)).
                Select(x => new ClientSetting
                {
                    Name = x.Name,
                    StringValue = x.StringValue,
                    DateValue = x.DateValue
                }).ToList());
            var client = CacheManager.GetClientById(clientId);

            var excluded = input.FirstOrDefault(x => x.Name == "Excluded");
            var documentVerified = input.FirstOrDefault(x => x.Name == "DocumentVerified");
            var systemExcluded = input.FirstOrDefault(x => x.Name == ClientSettings.SystemExcluded);
            if (systemExcluded != null && systemExcluded.StringValue == "0")
            {
                result.AddRange(clientBl.UpdateClientSettings(clientId, new List<ClientSetting>{ new ClientSetting
                    {
                        Name = systemExcluded.Name,
                        NumericValue = 0,
                        StringValue = "",
                        DateValue = systemExcluded.DateValue
                    }}));
            }

            bool? isDocumentVerified = null;
            int? state = null;
            if (excluded != null && excluded.StringValue != (client.State == (int)ClientStates.ForceBlock ||
                client.State == (int)ClientStates.FullBlocked || client.State == (int)ClientStates.Disabled
                ? "1" : "0"))
            {
                state = excluded.StringValue == "0" ? (int)ClientStates.Active : (int)ClientStates.FullBlocked;
                result.Add(new ClientSetting { Name = excluded.Name, StringValue = excluded.StringValue });
            }
            if (documentVerified != null && documentVerified.StringValue != (client.IsDocumentVerified ? "1" : "0"))
            {
                isDocumentVerified = !client.IsDocumentVerified;
                result.Add(new ClientSetting { Name = documentVerified.Name, StringValue = documentVerified.StringValue });
            }
            clientBl.ChangeClientState(clientId, state, isDocumentVerified);
            if (isDocumentVerified.HasValue && isDocumentVerified.Value)
            {
                if (CacheManager.GetConfigKey(client.PartnerId, Constants.PartnerKeys.AMLVerification) == "1")
                {
                    try
                    {
                        var regionPath = regionBl.GetRegionPath(client.RegionId);
                        var country = regionPath.FirstOrDefault(x => x.TypeId == (int)RegionTypes.Country);
                        if (country != null)
                        {
                            var clientAmlStatus = AMLStatusHelpers.GetAMLStatus(client, country.Id ?? 0, log);
                            if (!string.IsNullOrEmpty(clientAmlStatus.Error))
                                throw new Exception(clientAmlStatus.Error);

                            clientBl.UpdateClientAMLStatus(client.Id, clientAmlStatus, "System");
                            if (clientAmlStatus.Status == AMLStatuses.BLOCK)
                                clientBl.LogoutClientById(client.Id, (int)LogoutTypes.System);
                        }
                    }
                    catch (Exception e)
                    {
                        log.Error(e);
                    }
                }
                if (CacheManager.GetConfigKey(client.PartnerId, Constants.PartnerKeys.JCJVerification) == "1")
                {
                    try
                    {
                        var res = DigitalCustomerHelpers.GetJCJStatus(client.PartnerId, client.DocumentType ?? 0, client.DocumentNumber, session.LanguageId, log);
                        clientBl.AddOrUpdateClientSetting(client.Id, ClientSettings.JCJProhibited, res, res.ToString(), null, null, "System");
                        if (res == 1)
                            clientBl.LogoutClientById(client.Id, (int)LogoutTypes.System);

                        CacheManager.RemoveClientSetting(clientId, ClientSettings.JCJProhibited);
                        Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}_{2}", Constants.CacheItems.ClientSettings, clientId, ClientSettings.JCJProhibited));
                    }
                    catch (Exception e)
                    {
                        log.Error(e);
                    }
                }
                if (CacheManager.GetConfigKey(client.PartnerId, Constants.PartnerKeys.GreenIdVerification) == "1")
                {
                    try
                    {
                        var res = GreenIDHelpers.IsDocumentVerified(client.Id, session, log) ? 1 : 0;
                        clientBl.AddOrUpdateClientSetting(client.Id, ClientSettings.DocumentVerified, res, res.ToString(), null, null, "System");
                        CacheManager.RemoveClientSetting(clientId, ClientSettings.DocumentVerified);
                        Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}_{2}", Constants.CacheItems.ClientSettings, clientId, ClientSettings.DocumentVerified));
                    }
                    catch (Exception e)
                    {
                        log.Error(e);
                    }
                }
            }
            if (result.FirstOrDefault(x => x.Name == ClientSettings.AMLVerified)?.StringValue == "1" &&
                result.FirstOrDefault(x => x.Name == ClientSettings.AMLProhibited)?.StringValue == "2")
                clientBl.LogoutClientById(client.Id, (int)LogoutTypes.System);

            CacheManager.RemoveClientFromCache(clientId);
            Helpers.Helpers.InvokeMessage("RemoveClient", clientId);

            foreach (var setting in result)
            {
                CacheManager.RemoveClientSetting(clientId, setting.Name);
                Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}_{2}", Constants.CacheItems.ClientSettings, clientId, setting.Name));
            }
            return new ApiResponseBase
            {
                ResponseObject = result.ToDictionary(x => x.Name, y => y.StringValue)
            };
        }

        private static ApiResponseBase GetClientLimitSettings(int clientId, SessionIdentity session, ILog log)
        {
            using var clientBl = new ClientBll(new SessionIdentity { Id = clientId }, log);
            var resp = clientBl.GetClientLimitSettings(session.Id);
            resp.ClientId = clientId;
            return new ApiResponseBase { ResponseObject = resp };
        }

        private static ApiResponseBase SaveClientLimitSettings(ClientCustomSettings limits, SessionIdentity session, ILog log)
        {
            using var clientBl = new ClientBll(session, log);
            clientBl.SaveClientLimitSettings(limits, session.Id);
            return new ApiResponseBase();
        }

        private static ApiResponseBase ApplySystemExclusion(ApiExlusionModel input, SessionIdentity identity, ILog log)
        {
            var ct = DateTime.UtcNow;
            var currentTime = new DateTime(ct.Year, ct.Month, ct.Day, ct.Hour, ct.Minute, ct.Second);
            if (!DateTime.TryParse(input.ToDate, out DateTime toDate))
                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongInputParameters);
            if (input.Type == 1)
                toDate = toDate.AddYears(100);
            if (currentTime >= toDate)
                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongInputParameters);

            toDate = new DateTime(toDate.Year, toDate.Month, toDate.Day, currentTime.Hour, currentTime.Minute, currentTime.Second);

            var client = CacheManager.GetClientById(input.ClientId);
            var partnerSetting = CacheManager.GetConfigKey(client.PartnerId, Constants.PartnerKeys.SelfExclusionPeriod);
            if (int.TryParse(partnerSetting, out int selfExclusionPeriod) && (toDate - currentTime).TotalDays < selfExclusionPeriod)
                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongInputParameters);

            using var clientBl = new ClientBll(identity, log);
            using var notificationBll = new NotificationBll(identity, log);
            clientBl.AddOrUpdateClientSetting(input.ClientId, ClientSettings.SystemExcluded, 1, string.Empty, toDate, null, string.Empty);
            clientBl.LogoutClientById(input.ClientId, (int)LogoutTypes.System);
            var activePeriodInMinutes = notificationBll.SendNotificationMessage(new NotificationModel
            {
                PartnerId = client.PartnerId,
                ClientId = client.Id,
                MobileOrEmail =client.Email,
                ClientInfoType = (int)ClientInfoTypes.SystemExclusionApplied,
                Parameters = String.Format("todate:{0}", toDate.ToString("yyyy-MM-dd")),
                LanguageId = client.LanguageId
            });
            CacheManager.RemoveClientSetting(input.ClientId, ClientSettings.SystemExcluded);
            Helpers.Helpers.InvokeMessage("RemoveClientSetting", input.ClientId, ClientSettings.SystemExcluded);
            return new ApiResponseBase();
        }

        private static ApiResponseBase GetSegmentClients(ApiFilterfnSegmentClient filter, SessionIdentity identity, ILog log)
        {
            using var clientBl = new ClientBll(identity, log);
            var res = clientBl.GetSegmentClients(filter.MapToFilterfnSegmentClient());
            var hideClientContactInfo = false;
            try
            {
                clientBl.CheckPermission(Constants.Permissions.ViewClientContactInfoList);
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                if (fex.Detail.Id == Constants.Errors.DontHavePermission)
                    hideClientContactInfo = true;
            }

            return new ApiResponseBase
            {
                ResponseObject = new
                {
                    res.Count,
                    Entities = res.Entities.MapTofnSegmentClientModelList(hideClientContactInfo, identity.TimeZone)
                }
            };
        }

        private static ApiResponseBase SaveClientGameProviderSetting(ApiGameProviderSetting input, SessionIdentity identity, ILog log)
        {
            var client = CacheManager.GetClientById(input.ObjectId);
            if (client == null)
                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.ClientNotFound);
            using var productBll = new ProductBll(identity, log);
            using var partnerBll = new PartnerBll(productBll);
            var checkPartnerPermission = partnerBll.GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            var checkClientPermission = partnerBll.GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewClient,
                ObjectTypeId = ObjectTypes.Client
            });
            var clientAccess = partnerBll.GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.EditClient,
                ObjectTypeId = ObjectTypes.Client
            });
            if (!checkPartnerPermission.HaveAccessForAllObjects && checkPartnerPermission.AccessibleObjects.All(x => x != client.PartnerId) ||
                !checkClientPermission.HaveAccessForAllObjects && checkClientPermission.AccessibleObjects.All(x => x != client.Id) ||
                !clientAccess.HaveAccessForAllObjects && clientAccess.AccessibleObjects.All(x => x != client.Id))
                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.DontHavePermission);
            var result = productBll.SaveGameProviderSetting(new GameProviderSetting
            {
                ObjectTypeId = (int)ObjectTypes.Client,
                ObjectId = input.ObjectId,
                GameProviderId = input.GameProviderId,
                State = input.State,
                Order = input.Order
            });
            var cacheKey = string.Format("{0}_{1}_{2}", Constants.CacheItems.GameProviderSettings, (int)ObjectTypes.Client, client.Id);
            CacheManager.RemoveFromCache(cacheKey);
            Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", cacheKey);
            return new ApiResponseBase
            {
                ResponseObject = new
                {
                    result.Id,
                    result.ObjectId,
                    result.GameProviderId,
                    GameProviderName = result.GameProvider.Name,
                    result.State,
                    result.Order
                }
            };
        }

        private static ApiResponseBase GetClientGameProviderSettings(int clientId, SessionIdentity identity, ILog log)
        {
            var client = CacheManager.GetClientById(clientId);
            if (client == null)
                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.ClientNotFound);
            using var productBll = new ProductBll(identity, log);
            using var partnerBll = new PartnerBll(productBll);
            var checkPartnerPermission = partnerBll.GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            var checkClientPermission = partnerBll.GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewClient,
                ObjectTypeId = ObjectTypes.Client
            });
            if (!checkPartnerPermission.HaveAccessForAllObjects && checkPartnerPermission.AccessibleObjects.All(x => x != client.PartnerId) ||
                !checkClientPermission.HaveAccessForAllObjects && checkClientPermission.AccessibleObjects.All(x => x != client.Id))
                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.DontHavePermission);
            var clientProviderSettings = productBll.GetGameProviderSettings((int)ObjectTypes.Client, clientId).Select(x => new ApiGameProviderSetting
            {
                ObjectId = Convert.ToInt32(x.ObjectId),
                GameProviderId = x.GameProviderId,
                GameProviderName = x.GameProvider.Name,
                State = x.State,
                Order = x.Order ?? 1
            }).ToList();

            clientProviderSettings.AddRange(productBll.GetGameProviders(new FilterGameProvider()).Where(x => !clientProviderSettings.Any(y => y.GameProviderId == x.Id))
                .Select(x => new ApiGameProviderSetting { ObjectId = clientId, GameProviderId = x.Id, GameProviderName = x.Name, State = (int)BaseStates.Active, Order = 10000 }));
            return new ApiResponseBase
            {
                ResponseObject = clientProviderSettings
            };
        }
        private static ApiResponseBase SaveClientCategory(ApiClientCategory input, SessionIdentity identity, ILog log)
        {
            using var clientBl = new ClientBll(identity, log);
            return new ApiResponseBase
            {
                ResponseObject = clientBl.SaveClientCategory(input.MapToClientCategory()).MapToApiClientCategory()
            };
        }

        private static ApiResponseBase DeleteClientCategory(int clientCategoryId, SessionIdentity identity, ILog log)
        {
            using var clientBl = new ClientBll(identity, log);
            clientBl.DeleteClientCategory(clientCategoryId);
            return new ApiResponseBase
            {
                ResponseObject = new { Id = clientCategoryId }
            };
        }

        private static ApiResponseBase GetClientCategories(SessionIdentity identity, ILog log)
        {
            using var clientBl = new ClientBll(identity, log);
            return new ApiResponseBase
            {
                ResponseObject = clientBl.GetClientCategories().Select(x => x.MapToApiClientCategory()).ToList()
            };
        }
    }
}