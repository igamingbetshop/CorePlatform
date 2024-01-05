using System;
using System.Linq;
using IqSoft.CP.DAL;
using IqSoft.CP.AdminWebApi.Filters;
using IqSoft.CP.AdminWebApi.Helpers;
using Newtonsoft.Json;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.AdminWebApi.Models.BetShopModels;
using IqSoft.CP.AdminWebApi.Models.CommonModels;
using IqSoft.CP.AdminWebApi.Models.PartnerModels;
using log4net;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.AdminWebApi.Models.ClientModels;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common.Models;
using Microsoft.AspNetCore.Hosting;
using IqSoft.CP.AdminWebApi.Models.UserModels;
using IqSoft.CP.AdminWebApi.Models.CRM;
using IqSoft.CP.AdminWebApi.Models.ProductModels;
using IqSoft.CP.DAL.Filters;

namespace IqSoft.CP.AdminWebApi.ControllerClasses
{
    public static class PartnerController
    {
        public static ApiResponseBase CallFunction(RequestBase request, SessionIdentity identity, ILog log, IWebHostEnvironment env)
        {
            switch (request.Method)
            {
                case "GetPartnersList":
                    return GetPartnersList(JsonConvert.DeserializeObject<ApiFilterPartner>(request.RequestData), identity, log);
                case "IsPartnerIdExists":
                    return IsPartnerIdExists(JsonConvert.DeserializeObject<int>(request.RequestData), identity, log);
                case "GetPartners":
                    return GetPartners(JsonConvert.DeserializeObject<ApiFilterPartner>(request.RequestData), identity, log);
                case "SavePartner":
                    return SavePartner(JsonConvert.DeserializeObject<Partner>(request.RequestData), identity, log);
                case "AddPartner":
                    return AddPartner(JsonConvert.DeserializeObject<Partner>(request.RequestData), identity, log);
                case "PayBetShopDebt":
                    return PayBetShopDebt(JsonConvert.DeserializeObject<PayBetShopDebtModel>(request.RequestData),
                        request.ExternalOperationId, identity, log);
                case "ExportPartnersList":
                    return ExportPartnersList(JsonConvert.DeserializeObject<ApiFilterPartner>(request.RequestData),identity, log, env);
                case "ExportPartnersModel":
                    return ExportPartnersModel(JsonConvert.DeserializeObject<ApiFilterPartner>(request.RequestData),identity, log, env);
                case "SetPaymentLimit":
                    return SetPaymentLimit(JsonConvert.DeserializeObject<PartnerPaymentLimit>(request.RequestData),
                        identity, log);
                case "GetPaymentLimit":
                    return GetPaymentLimit(Convert.ToInt32(request.RequestData), identity, log);
                case "PurgeContentCache":
                    return PurgeCloudflareCache(Convert.ToInt32(request.RequestData), identity, log);
                case "GetPartnerBanks":
                    return GetPartnerBanks(Convert.ToInt32(request.RequestData), identity, log);
                case "UpdatePartnerBankInfo":
                    return UpdatePartnerBankInfo(JsonConvert.DeserializeObject<ApiPartnerBankInfo>(request.RequestData), identity, log);
                case "GetPartnerCustomerBanks":
                    return GetPartnerCustomerBanks(JsonConvert.DeserializeObject<ClientIdentifierInfo>(request.RequestData), identity, log);
                case "GetPartnerEnvironments":
                    return GetPartnerEnvironments(Convert.ToInt32(request.RequestData), identity, log);
                case "SavePasswordRegEx":
                    return SavePasswordRegEx(JsonConvert.DeserializeObject<RegExProperty>(request.RequestData), identity, log);
                case "GetPartnerKeys":
                    return GetPartnerKeys(JsonConvert.DeserializeObject<ApiFilterPartnerKey>(request.RequestData), identity, log);
                case "SavePartnerKey":
                    return SavePartnerKey(JsonConvert.DeserializeObject<ApiPartnerKey>(request.RequestData), identity, log);
                case "GetUserPasswordReqex":
                    return GetUserPasswordReqex(JsonConvert.DeserializeObject<UserTypeInput>(request.RequestData), identity, log);
                case "GetSecurityQuestions":
                    return GetSecurityQuestions(JsonConvert.DeserializeObject<ApiBaseFilter>(request.RequestData), identity, log);
                case "SaveSecurityQuestion":
                    return SaveSecurityQuestion(JsonConvert.DeserializeObject<ApiSecurityQuestion>(request.RequestData), identity, log);
                case "SavePartnerGameProviderSetting":
                    return SavePartnerGameProviderSetting(JsonConvert.DeserializeObject<ApiGameProviderSetting>(request.RequestData), identity, log);
                case "GetPartnerGameProviderSettings":
                    return GetPartnerGameProviderSettings(Convert.ToInt32(request.RequestData), identity, log);
            }
            throw BaseBll.CreateException(string.Empty, Constants.Errors.MethodNotFound);
        }

        private static ApiResponseBase IsPartnerIdExists(int partnerId, SessionIdentity identity, ILog log)
        {
            using (var partnerBl = new PartnerBll(identity, log))
            {
                var response = new ApiResponseBase
                {
                    ResponseObject = partnerBl.IsPartnerIdExists(partnerId)
                };
                return response;
            }
        }

        private static ApiResponseBase GetPartners(ApiFilterPartner request, SessionIdentity identity, ILog log)
        {
            using var partnerBl = new PartnerBll(identity, log);
            var resp = partnerBl.GetPartnersPagedModel(request.MapToFilterPartner());
            var response = new ApiResponseBase
            {
                ResponseObject = new { resp.Count, Entities = resp.Entities.MapToPartnerModels(partnerBl.GetUserIdentity().TimeZone) }
            };
            return response;
        }

        private static ApiResponseBase GetPartnersList(ApiFilterPartner request, SessionIdentity identity, ILog log)
        {
            using var partnerBl = new PartnerBll(identity, log);
            return new ApiResponseBase
            {
                ResponseObject = partnerBl.GetPartners(request.MapToFilterPartner(), true).Select(x => new EnumerationModel<int>
                {
                    Id = x.Id,
                    Name = x.Name
                })
            };
        }

        private static ApiResponseBase SavePartner(Partner request, SessionIdentity identity, ILog log)
        {
            using var partnerBl = new PartnerBll(identity, log);
            var partner = partnerBl.SavePartner(request).MapToPartnerModel(identity.TimeZone);
            CacheManager.RemovePartner(partner.Id);
            Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.Partners, partner.Id));
            return new ApiResponseBase
            {
                ResponseObject = partner
            };
        }

        private static ApiResponseBase AddPartner(Partner request, SessionIdentity identity, ILog log)
        {
            using var partnerBl = new PartnerBll(identity, log);
            if (CacheManager.GetPartnerById(request.Id) == null)
            {
                var partner = partnerBl.SavePartner(request).MapToPartnerModel(identity.TimeZone);
                CacheManager.RemovePartner(partner.Id);
                Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.Partners, partner.Id));
                return new ApiResponseBase
                {
                    ResponseObject = partner
                };
            }
            throw new InvalidOperationException();
        }

        private static ApiResponseBase PayBetShopDebt(PayBetShopDebtModel debtModel, long? externalOperationId, SessionIdentity identity, ILog log)
        {
            using var partnerBl = new PartnerBll(identity, log);
            var recoing = partnerBl.PayBetShopDebt(debtModel.BetshopId, debtModel.Amount, debtModel.CurrencyId,
                externalOperationId);
            return new ApiResponseBase
            {
                ResponseObject = recoing.MapToBetShopReconingModel(identity.TimeZone)
            };
        }

        private static ApiResponseBase ExportPartnersList(ApiFilterPartner request, SessionIdentity identity, ILog log, IWebHostEnvironment env)
        {
            using var partnerBl = new PartnerBll(identity, log);
            var partnerEnum = partnerBl.ExportPartners(request.MapToFilterPartner(), true).Select(x => new EnumerationModel<int>
            {
                Id = x.Id,
                Name = x.Name
            }).ToList();

            string fileName = "ExportPartners.csv";
            string fileAbsPath = partnerBl.ExportToCSV<EnumerationModel<int>>(fileName, partnerEnum, request.CreatedFrom, request.CreatedBefore,
                                                                              partnerBl.GetUserIdentity().TimeZone, env);

            return new ApiResponseBase
            {
                ResponseObject = new
                {
                    ExportedFilePath = fileAbsPath
                }
            };
        }

        private static ApiResponseBase ExportPartnersModel(ApiFilterPartner request, SessionIdentity identity, ILog log, IWebHostEnvironment env)
        {
            using var partnerBl = new PartnerBll(identity, log);
            var timeZone = partnerBl.GetUserIdentity().TimeZone;
            var filteredList = partnerBl.ExportPartnersModel(request.MapToFilterPartner()).MapToPartnerModels(timeZone);
            string fileName = "ExportPartnersModel.csv";
            string fileAbsPath = partnerBl.ExportToCSV<PartnerModel>(fileName, filteredList, request.CreatedFrom, request.CreatedBefore, timeZone, env);

            return new ApiResponseBase
            {
                ResponseObject = new
                {
                    ExportedFilePath = fileAbsPath
                }
            };
        }

        private static ApiResponseBase SetPaymentLimit(PartnerPaymentLimit partnerPaymentLimit, SessionIdentity identity, ILog log)
        {
            using var partnerBl = new PartnerBll(identity, log);
            partnerBl.SetPaymentLimit(partnerPaymentLimit, true);
            CacheManager.RemovePartnerSettingKeyFromCache(partnerPaymentLimit.PartnerId, Constants.PartnerKeys.WithdrawMaxCountPerDayPerCustomer);
            CacheManager.RemovePartnerSettingKeyFromCache(partnerPaymentLimit.PartnerId, Constants.PartnerKeys.CashWithdrawMaxCountPerDayPerCustomer);
            return new ApiResponseBase();
        }

        private static ApiResponseBase GetPaymentLimit(int partnerId, SessionIdentity identity, ILog log)
        {
            using var partnerBl = new PartnerBll(identity, log);
            return new ApiResponseBase
            {
                ResponseObject = partnerBl.GetPaymentLimit(partnerId, true)
            };
        }

        private static ApiResponseBase PurgeCloudflareCache(int partnerId, SessionIdentity identity, ILog log)
        {
            using var partnerBll = new PartnerBll(identity, log);
            partnerBll.CheckPermission(Constants.Permissions.EditCloudflare);
            var checkPartnerPermission = partnerBll.GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            if (!checkPartnerPermission.HaveAccessForAllObjects && checkPartnerPermission.AccessibleObjects.All(x => x != partnerId))
                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.DontHavePermission);
            Integration.Platforms.Helpers.CloudflareHelpers.PurgeCache(partnerId);
            return new ApiResponseBase();
        }

        private static ApiResponseBase GetPartnerBanks(int partnerId, SessionIdentity identity, ILog log)
        {
            using var paymentSystemBll = new PaymentSystemBll(identity, log);
            return new ApiResponseBase
            {
                ResponseObject = paymentSystemBll.GetPartnerBanks(partnerId, null, true, null).MapToApiPartnerBankInfo(identity.TimeZone)
            };
        }

        private static ApiResponseBase UpdatePartnerBankInfo(ApiPartnerBankInfo apiPartnerBankInfo, SessionIdentity identity, ILog log)
        {
            using var paymentSystemBll = new PaymentSystemBll(identity, log);
            return new ApiResponseBase
            {
                ResponseObject = paymentSystemBll.UpdatePartnerBankInfo(apiPartnerBankInfo.MapToPartnerBankInfo()).MapToApiPartnerBankInfo(identity.TimeZone)
            };
        }

        private static ApiResponseBase GetPartnerCustomerBanks(ClientIdentifierInfo identifierInfo, SessionIdentity session, ILog log)
        {
            using var paymentBl = new PaymentSystemBll(session, log);
            if (identifierInfo.PartnerId == 0 && !string.IsNullOrEmpty(identifierInfo.ClientIdentifier))
            {
                if (Int32.TryParse(identifierInfo.ClientIdentifier, out int clientId))
                {
                    var client = CacheManager.GetClientById(clientId);
                    if (client == null)
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.ClientNotFound);
                    identifierInfo.PartnerId = client.PartnerId;
                }
            }
            var response = paymentBl.GetPartnerBanks(identifierInfo.PartnerId, null, false,
                           (int)BankInfoTypes.BankForCustomer, null).MapToApiPartnerBankInfo(session.TimeZone);
            return new ApiResponseBase
            {
                ResponseObject = response
            };
        }

        private static ApiResponseBase GetPartnerEnvironments(int partnerId, SessionIdentity identity, ILog log)
        {
            using var partnerBl = new PartnerBll(identity, log);
            var partner = CacheManager.GetPartnerById(partnerId);
            if (partner == null)
                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.PartnerNotFound);
            var environments = partnerBl.GetPartnerEnvironments(partnerId).Keys.ToList();
            return new ApiResponseBase
            {
                ResponseObject = BaseBll.GetEnumerations(Constants.EnumerationTypes.EnvironmentTypes, identity.LanguageId).Where(x => environments.Contains(x.Value)).
                Select(x => new EnumerationModel<int>
                {
                    Id = x.Value,
                    Name = x.Text
                })
            };
        }

        private static ApiResponseBase SavePasswordRegEx(RegExProperty regExProperty, SessionIdentity identity, ILog log)
        {
            using var partnerBl = new PartnerBll(identity, log);
            if (regExProperty.MinLength > regExProperty.MaxLength ||
              regExProperty.MinLength <= 0 || regExProperty.MaxLength <= 0 ||
             (!regExProperty.Symbol && !regExProperty.IsSymbolRequired && !regExProperty.Numeric &&!regExProperty.IsDigitRequired &&
              !regExProperty.Lowercase && !regExProperty.IsLowercaseRequired && !regExProperty.Uppercase &&
              !regExProperty.IsUppercaseRequired))
                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongInputParameters);
            var res = partnerBl.SavePasswordRegex(regExProperty.PartnerId.Value, regExProperty.GetExpression());
            CacheManager.RemovePartner(regExProperty.PartnerId.Value);
            Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.Partners, regExProperty.PartnerId.Value));
            return new ApiResponseBase
            {
                ResponseObject = res
            };
        }

        private static ApiResponseBase GetPartnerKeys(ApiFilterPartnerKey filter, SessionIdentity identity, ILog log)
        {
            using var partnerBl = new PartnerBll(identity, log);
            var res = partnerBl.GetPartnerKeys(filter.PartnerId);
            return new ApiResponseBase
            {
                ResponseObject = res.Select(x => new ApiPartnerKey
                {
                    Id = x.Id,
                    Name = x.Name,
                    PartnerId = x.PartnerId,
                    GameProviderId = x.GameProviderId,
                    PaymentSystemId = x.PaymentSystemId,
                    NotificationServiceId = x.NotificationServiceId,
                    StringValue = x.StringValue,
                    DateValue = x.DateValue,
                    NumericValue = x.NumericValue
                }).ToList()
            };
        }

        private static ApiResponseBase GetSecurityQuestions(ApiBaseFilter input, SessionIdentity identity, ILog log)
        {
            using (var partnerBl = new PartnerBll(identity, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = partnerBl.GetPartnerSecurityQuestions(input.PartnerId).Select(x => new ApiSecurityQuestion
                    {
                        Id = x.Id,
                        PartnerId = x.PartnerId,
                        NickName = x.NickName,
                        TranslationId = x.TranslationId,
                        Status  =  x.Status,
                        CreationTime = x.CreationTime,
                        LastUpdateTime = x.LastUpdateTime
                    }).ToList()
                };
            }
        }

        private static ApiResponseBase SaveSecurityQuestion(ApiSecurityQuestion apiSecurityQuestion, SessionIdentity identity, ILog log)
        {
            using (var partnerBl = new PartnerBll(identity, log))
            {
                var partnerSecurityQuestion = partnerBl.SavePartnerSecurityQuestion(new SecurityQuestion
                {
                    Id = apiSecurityQuestion.Id,
                    PartnerId = apiSecurityQuestion.PartnerId,
                    NickName = apiSecurityQuestion.NickName,
                    QuestionText = apiSecurityQuestion.QuestionText,
                    Status = apiSecurityQuestion.Status,
                    CreationTime = apiSecurityQuestion.CreationTime,
                    LastUpdateTime = apiSecurityQuestion.LastUpdateTime
                });
                var languages = CacheManager.GetAvailableLanguages();
                foreach (var lan in languages)
                {
                    BaseController.BroadcastCacheChanges(apiSecurityQuestion.PartnerId, string.Format("{0}_{1}_{2}", Constants.CacheItems.SecurityQuestions, apiSecurityQuestion.PartnerId, lan.Id));
                }
                return new ApiResponseBase
                {
                    ResponseObject =  new ApiSecurityQuestion
                    {
                        Id = partnerSecurityQuestion.Id,
                        PartnerId = partnerSecurityQuestion.PartnerId,
                        NickName = partnerSecurityQuestion.NickName,
                        TranslationId = partnerSecurityQuestion.TranslationId,
                        Status  =  partnerSecurityQuestion.Status,
                        CreationTime = partnerSecurityQuestion.CreationTime,
                        LastUpdateTime = partnerSecurityQuestion.LastUpdateTime
                    }
                };
            }
        }

        private static ApiResponseBase SavePartnerKey(ApiPartnerKey apiPartnerKey, SessionIdentity identity, ILog log)
        {
            using var partnerBl = new PartnerBll(identity, log);
            var partnerKey = new PartnerKey
            {
                Id = apiPartnerKey.Id,
                Name = apiPartnerKey.Name,
                PartnerId = apiPartnerKey.PartnerId,
                GameProviderId = apiPartnerKey.GameProviderId,
                PaymentSystemId = apiPartnerKey.PaymentSystemId,
                NotificationServiceId = apiPartnerKey.NotificationServiceId,
                StringValue = apiPartnerKey.StringValue,
                DateValue = apiPartnerKey.DateValue,
                NumericValue = apiPartnerKey.NumericValue
            };
            var res = partnerBl.SavePartnerKey(partnerKey);
            Helpers.Helpers.InvokeMessage(partnerKey.Name);
            if (partnerKey.PartnerId.HasValue)
                Helpers.Helpers.InvokeMessage(string.Format("{0}_{1}", partnerKey.Name, partnerKey.PartnerId.Value));
            if (partnerKey.GameProviderId.HasValue)
                Helpers.Helpers.InvokeMessage(string.Format("{0}_{1}_{2}", partnerKey.PartnerId.Value, partnerKey.GameProviderId, partnerKey.Name));
            if (partnerKey.PaymentSystemId.HasValue)
                Helpers.Helpers.InvokeMessage(string.Format("{0}_{1}_{2}", partnerKey.PartnerId.Value, partnerKey.PaymentSystemId, partnerKey.Name));
            if (partnerKey.NotificationServiceId.HasValue)
                Helpers.Helpers.InvokeMessage(string.Format("NotificationService_{0}_{1}_{2}", partnerKey.Name, partnerKey.NotificationServiceId, partnerKey.PartnerId.Value));

            return new ApiResponseBase
            {
                ResponseObject = new ApiPartnerKey
                {
                    Id = res.Id,
                    Name = res.Name,
                    PartnerId = res.PartnerId,
                    GameProviderId = res.GameProviderId,
                    PaymentSystemId = res.PaymentSystemId,
                    NotificationServiceId = res.NotificationServiceId,
                    StringValue = res.StringValue,
                    DateValue = res.DateValue,
                    NumericValue = res.NumericValue
                }
            };
        }

        private static ApiResponseBase GetUserPasswordReqex(UserTypeInput userTypeInput, SessionIdentity identity, ILog log)
        {
            using (var userBl = new UserBll(identity, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = userBl.GetUserPasswordRegex(userTypeInput.PartnerId, userTypeInput.Type)
                };
            }
        }

        private static ApiResponseBase SavePartnerGameProviderSetting(ApiGameProviderSetting input, SessionIdentity identity, ILog log)
        {
            var partner = CacheManager.GetPartnerById(input.ObjectId);
            if (partner == null)
                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.ClientNotFound);
            using (var productBll = new ProductBll(identity, log))
            {
                using (var partnerBll = new PartnerBll(productBll))
                {
                    var checkPartnerPermission = partnerBll.GetPermissionsToObject(new CheckPermissionInput
                    {
                        Permission = Constants.Permissions.ViewPartner,
                        ObjectTypeId = ObjectTypes.Partner
                    });
                    if (!checkPartnerPermission.HaveAccessForAllObjects && checkPartnerPermission.AccessibleObjects.All(x => x != partner.Id))
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.DontHavePermission);
                    var result = productBll.SaveGameProviderSetting(new GameProviderSetting
                    {
                        ObjectTypeId = (int)ObjectTypes.Partner,
                        ObjectId = input.ObjectId,
                        GameProviderId = input.GameProviderId,
                        State = input.State,
                        Order = input.Order
                    });
                    var cacheKey = string.Format("{0}_{1}_{2}", Constants.CacheItems.GameProviderSettings, (int)ObjectTypes.Partner, partner.Id);
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
            }
        }

        private static ApiResponseBase GetPartnerGameProviderSettings(int partnerId, SessionIdentity identity, ILog log)
        {
            var partner = CacheManager.GetPartnerById(partnerId);
            if (partner == null)
                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.ClientNotFound);
            using (var productBll = new ProductBll(identity, log))
            {
                using (var partnerBll = new PartnerBll(productBll))
                {
                    var checkPartnerPermission = partnerBll.GetPermissionsToObject(new CheckPermissionInput
                    {
                        Permission = Constants.Permissions.ViewPartner,
                        ObjectTypeId = ObjectTypes.Partner
                    });
                    if (!checkPartnerPermission.HaveAccessForAllObjects && checkPartnerPermission.AccessibleObjects.All(x => x != partnerId))
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.DontHavePermission);
                    var partnerProviderSettings = productBll.GetGameProviderSettings((int)ObjectTypes.Partner, partnerId).Select(x => new ApiGameProviderSetting
                    {
                        ObjectId = Convert.ToInt32(x.ObjectId),
                        GameProviderId = x.GameProviderId,
                        GameProviderName = x.GameProvider.Name,
                        State = x.State,
                        Order = x.Order ?? 1
                    }).ToList();

                    partnerProviderSettings.AddRange(productBll.GetGameProviders(new FilterGameProvider()).Where(x => !partnerProviderSettings.Any(y => y.GameProviderId == x.Id))
                        .Select(x => new ApiGameProviderSetting { ObjectId = partnerId, GameProviderId = x.Id, GameProviderName = x.Name, State = (int)BaseStates.Active, Order = 10000 }));
                    return new ApiResponseBase
                    {
                        ResponseObject = partnerProviderSettings
                    };
                }
            }
        }
    }
}