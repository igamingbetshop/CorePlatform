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
using IqSoft.CP.AdminWebApi.Models.UserModels;
using IqSoft.CP.AdminWebApi.Models.CRM;
using IqSoft.CP.AdminWebApi.Models.ProductModels;
using IqSoft.CP.DAL.Filters;
using IqSoft.CP.Integration.Platforms.Models;
using IqSoft.CP.AdminWebApi.Models.ContentModels;
using IqSoft.CP.Common.Models.Partner;
using System.Collections.Generic;

namespace IqSoft.CP.AdminWebApi.ControllerClasses
{
    public static class PartnerController
    {
        public static ApiResponseBase CallFunction(RequestBase request, SessionIdentity identity, ILog log)
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
                    return ExportPartnersList(JsonConvert.DeserializeObject<ApiFilterPartner>(request.RequestData),
                        identity, log);
                case "ExportPartnersModel":
                    return ExportPartnersModel(JsonConvert.DeserializeObject<ApiFilterPartner>(request.RequestData),
                        identity, log);
                case "SetPaymentLimit":
                    return SetPaymentLimit(JsonConvert.DeserializeObject<PartnerPaymentLimit>(request.RequestData),
                        identity, log);
                case "GetPaymentLimit":
                    return GetPaymentLimit(Convert.ToInt32(request.RequestData), identity, log);
                case "PurgeContentCache":
                    return PurgeCloudflareCache(Convert.ToInt32(request.RequestData), identity, log);
                case "GetDnsRecords":
                    return GetDnsRecords(Convert.ToInt32(request.RequestData), identity, log);
                case "AddDnsRecord":
                    return AddDnsRecord(JsonConvert.DeserializeObject<DnsItem>(request.RequestData), identity, log);
                case "DeleteDnsRecord":
                    return DeleteDnsRecord(JsonConvert.DeserializeObject<DnsItem>(request.RequestData), identity, log);
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
                case "CopyPartnerKeys":
                    return CopyPartnerKeys(JsonConvert.DeserializeObject<ApiCloneObject>(request.RequestData), identity, log);
                case "GetEmails":
                    if (!string.IsNullOrEmpty(request.RequestData))
                        return GetEmails(JsonConvert.DeserializeObject<ApiFilterEmail>(request.RequestData), identity, log);
                    return GetEmails(new ApiFilterEmail(), identity, log);
                case "GetPartnerCountrySettings":
                    return GetPartnerCountrySettings(JsonConvert.DeserializeObject<PartnerSetting>(request.RequestData), identity, log);
                case "SavePartnerCountrySetting":
                    return SavePartnerCountrySetting(JsonConvert.DeserializeObject<PartnerCountrySetting>(request.RequestData), identity, log);
                case "RemovePartnerCountrySetting":
                    return RemovePartnerCountrySetting(JsonConvert.DeserializeObject<PartnerCountrySetting>(request.RequestData), identity, log);
                case "GetCharacters":
                    return GetCharacters(JsonConvert.DeserializeObject<ApiFilterCharacter>(request.RequestData), identity, log);
                case "GetCharacterHierarchy":
                    return GetCharacterHierarchy(Convert.ToInt32(request.RequestData), identity, log);
                case "GetCharacterById":
                    return GetCharacterById(Convert.ToInt32(request.RequestData), identity, log);
                case "SaveCharacter":
                    return SaveCharacter(JsonConvert.DeserializeObject<ApiCharacter>(request.RequestData), identity, log);
                case "DeleteCharacterById":
                    return DeleteCharacterById(JsonConvert.DeserializeObject<ApiCharacter>(request.RequestData), identity, log);
            }
            throw BaseBll.CreateException(string.Empty, Constants.Errors.MethodNotFound);
        }

        private static ApiResponseBase IsPartnerIdExists(int partnerId, SessionIdentity identity, ILog log)
        {
            using (var partnerBl = new PartnerBll(identity, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = partnerBl.IsPartnerIdExists(partnerId)
                };
            }
        }

        private static ApiResponseBase GetPartners(ApiFilterPartner request, SessionIdentity identity, ILog log)
        {
            using (var partnerBl = new PartnerBll(identity, log))
            {
                var resp = partnerBl.GetPartnersPagedModel(request.MapToFilterPartner());
                return new ApiResponseBase
                {
                    ResponseObject = new { resp.Count, Entities = resp.Entities.MapToPartnerModels(identity.TimeZone) }
                };
            }
        }

        private static ApiResponseBase GetPartnersList(ApiFilterPartner request, SessionIdentity identity, ILog log)
        {
            using (var partnerBl = new PartnerBll(identity, log))
            {
                var partnerEnum = partnerBl.GetPartners(request.MapToFilterPartner(), true).Select(x => new EnumerationModel<int>
                {
                    Id = x.Id,
                    Name = x.Name
                });

                return new ApiResponseBase
                {
                    ResponseObject = partnerEnum
                };
            }
        }

        private static ApiResponseBase SavePartner(Partner request, SessionIdentity identity, ILog log)
        {
            using (var partnerBl = new PartnerBll(identity, log))
            {
                var partner = partnerBl.SavePartner(request).MapToPartnerModel(identity.TimeZone);
                CacheManager.RemovePartner(partner.Id);
                Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.Partners, partner.Id));
                return new ApiResponseBase
                {
                    ResponseObject = partner
                };
            }
        }

        private static ApiResponseBase AddPartner(Partner request, SessionIdentity identity, ILog log)
        {
            using (var partnerBl = new PartnerBll(identity, log))
            {
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
        }

        private static ApiResponseBase PayBetShopDebt(PayBetShopDebtModel debtModel, long? externalOperationId, SessionIdentity identity, ILog log)
        {
            using (var partnerBl = new PartnerBll(identity, log))
            {
                var recoing = partnerBl.PayBetShopDebt(debtModel.BetshopId, debtModel.Amount, debtModel.CurrencyId,
                    externalOperationId);

                return new ApiResponseBase
                {
                    ResponseObject = recoing.MapToBetShopReconingModel(identity.TimeZone)
                };
            }
        }

        private static ApiResponseBase ExportPartnersList(ApiFilterPartner request, SessionIdentity identity, ILog log)
        {
            using (var partnerBl = new PartnerBll(identity, log))
            {
                var partnerEnum = partnerBl.ExportPartners(request.MapToFilterPartner(), true).Select(x => new EnumerationModel<int>
                {
                    Id = x.Id,
                    Name = x.Name
                }).ToList();

                string fileName = "ExportPartners.csv";
                string fileAbsPath = partnerBl.ExportToCSV<EnumerationModel<int>>(fileName, partnerEnum, request.CreatedFrom, request.CreatedBefore,
                                                                                  identity.TimeZone, request.AdminMenuId);

                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        ExportedFilePath = fileAbsPath
                    }
                };
            }
        }

        private static ApiResponseBase ExportPartnersModel(ApiFilterPartner request, SessionIdentity identity, ILog log)
        {
            using (var partnerBl = new PartnerBll(identity, log))
            {
                var filteredList = partnerBl.ExportPartnersModel(request.MapToFilterPartner()).MapToPartnerModels(identity.TimeZone);
                string fileName = "ExportPartnersModel.csv";
                string fileAbsPath = partnerBl.ExportToCSV<PartnerModel>(fileName, filteredList, request.CreatedFrom, request.CreatedBefore,
                    identity.TimeZone, request.AdminMenuId);

                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        ExportedFilePath = fileAbsPath
                    }
                };
            }
        }

        private static ApiResponseBase SetPaymentLimit(PartnerPaymentLimit partnerPaymentLimit, SessionIdentity identity, ILog log)
        {
            using (var partnerBl = new PartnerBll(identity, log))
            {
                partnerBl.SetPaymentLimit(partnerPaymentLimit, true);
                CacheManager.RemovePartnerSettingKeyFromCache(partnerPaymentLimit.PartnerId, Constants.PartnerKeys.WithdrawMaxCountPerDayPerCustomer);
                CacheManager.RemovePartnerSettingKeyFromCache(partnerPaymentLimit.PartnerId, Constants.PartnerKeys.CashWithdrawMaxCountPerDayPerCustomer);
            }
            return new ApiResponseBase();
        }

        private static ApiResponseBase GetPaymentLimit(int partnerId, SessionIdentity identity, ILog log)
        {
            using (var partnerBl = new PartnerBll(identity, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = partnerBl.GetPaymentLimit(partnerId, true)
                };
            }
        }

        private static ApiResponseBase PurgeCloudflareCache(int partnerId, SessionIdentity identity, ILog log)
        {
            using (var partnerBll = new PartnerBll(identity, log))
            {
                partnerBll.CheckPermission(Constants.Permissions.EditCloudflare);
                var partnerAccess = partnerBll.GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewPartner,
                    ObjectTypeId = ObjectTypes.Partner
                });
                if (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleIntegerObjects.All(x => x != partnerId))
                    throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.DontHavePermission);
                return new ApiResponseBase
                {
                    ResponseObject = Integration.Platforms.Helpers.CloudflareHelpers.PurgeCache(partnerId)
                };
            }
        }

        private static ApiResponseBase GetDnsRecords(int rowId, SessionIdentity identity, ILog log)
        {
            using (var partnerBll = new PartnerBll(identity, log))
            {
                using (var contentBll = new ContentBll(identity, log))
                {
                    partnerBll.CheckPermission(Constants.Permissions.EditCloudflare);
                    var item = contentBll.GetWebSiteSubMenuItem(rowId);

                    var partnerAccess = partnerBll.GetPermissionsToObject(new CheckPermissionInput
                    {
                        Permission = Constants.Permissions.ViewPartner,
                        ObjectTypeId = ObjectTypes.Partner
                    });
                    if (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleIntegerObjects.All(x => x != item.WebSiteMenuItem.WebSiteMenu.PartnerId))
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.DontHavePermission);

                    return new ApiResponseBase
                    {
                        ResponseObject = Integration.Platforms.Helpers.CloudflareHelpers.GetDnsRecords(item.WebSiteMenuItem.WebSiteMenu.PartnerId, item.Href)
                    };
                }
            }
        }

        private static ApiResponseBase AddDnsRecord(DnsItem input, SessionIdentity identity, ILog log)
        {
            using (var partnerBll = new PartnerBll(identity, log))
            {
                using (var contentBll = new ContentBll(identity, log))
                {
                    partnerBll.CheckPermission(Constants.Permissions.EditCloudflare);
                    var item = contentBll.GetWebSiteSubMenuItem(Convert.ToInt32(input.RowId));

                    var partnerAccess = partnerBll.GetPermissionsToObject(new CheckPermissionInput
                    {
                        Permission = Constants.Permissions.ViewPartner,
                        ObjectTypeId = ObjectTypes.Partner
                    });
                    if (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleIntegerObjects.All(x => x != item.WebSiteMenuItem.WebSiteMenu.PartnerId))
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.DontHavePermission);

                    return new ApiResponseBase
                    {
                        ResponseObject = Integration.Platforms.Helpers.CloudflareHelpers.AddDnsRecord(input, item.WebSiteMenuItem.WebSiteMenu.PartnerId, item.Href)
                    };
                }
            }
        }

        private static ApiResponseBase DeleteDnsRecord(DnsItem input, SessionIdentity identity, ILog log)
        {
            using (var partnerBll = new PartnerBll(identity, log))
            {
                using (var contentBll = new ContentBll(identity, log))
                {
                    partnerBll.CheckPermission(Constants.Permissions.EditCloudflare);
                    var item = contentBll.GetWebSiteSubMenuItem(Convert.ToInt32(input.RowId));

                    var partnerAccess = partnerBll.GetPermissionsToObject(new CheckPermissionInput
                    {
                        Permission = Constants.Permissions.ViewPartner,
                        ObjectTypeId = ObjectTypes.Partner
                    });
                    if (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleIntegerObjects.All(x => x != item.WebSiteMenuItem.WebSiteMenu.PartnerId))
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.DontHavePermission);

                    return new ApiResponseBase
                    {
                        ResponseObject = Integration.Platforms.Helpers.CloudflareHelpers.DeleteDnsRecord(input.Id, item.WebSiteMenuItem.WebSiteMenu.PartnerId, item.Href)
                    };
                }
            }
        }

        private static ApiResponseBase GetPartnerBanks(int partnerId, SessionIdentity identity, ILog log)
        {
            using (var paymentSystemBll = new PaymentSystemBll(identity, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = paymentSystemBll.GetPartnerBanks(partnerId, null, true, null).MapToApiPartnerBankInfo(identity.TimeZone)
                };
            }
        }

        private static ApiResponseBase UpdatePartnerBankInfo(ApiPartnerBankInfo apiPartnerBankInfo, SessionIdentity identity, ILog log)
        {
            using (var paymentSystemBll = new PaymentSystemBll(identity, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = paymentSystemBll.UpdatePartnerBankInfo(apiPartnerBankInfo.MapToPartnerBankInfo()).MapToApiPartnerBankInfo(identity.TimeZone)
                };
            }
        }

        private static ApiResponseBase GetPartnerCustomerBanks(ClientIdentifierInfo identifierInfo, SessionIdentity session, ILog log)
        {
            using (var paymentBl = new PaymentSystemBll(session, log))
            {
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
                return new ApiResponseBase
                {
                    ResponseObject = paymentBl.GetPartnerBanks(identifierInfo.PartnerId, null, false,
                                                              (int)BankInfoTypes.BankForCustomer, null).MapToApiPartnerBankInfo(session.TimeZone)
                };
            }
        }

        private static ApiResponseBase GetPartnerEnvironments(int partnerId, SessionIdentity identity, ILog log)
        {
            using (var partnerBl = new PartnerBll(identity, log))
            {
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
        }

        private static ApiResponseBase SavePasswordRegEx(RegExProperty regExProperty, SessionIdentity identity, ILog log)
        {
            using (var partnerBl = new PartnerBll(identity, log))
            {
                if (regExProperty.MinLength > regExProperty.MaxLength ||
                    regExProperty.MinLength <= 0 || regExProperty.MaxLength <= 0 ||
                   (!regExProperty.Symbol && !regExProperty.IsSymbolRequired && !regExProperty.Numeric && !regExProperty.IsDigitRequired &&
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
        }

        private static ApiResponseBase GetPartnerKeys(ApiFilterPartnerKey filter, SessionIdentity identity, ILog log)
        {
            using (var partnerBl = new PartnerBll(identity, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = partnerBl.GetPartnerKeys(filter.PartnerId).Select(x => new ApiPartnerKey
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
                        Status = x.Status,
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
                    ResponseObject = new ApiSecurityQuestion
                    {
                        Id = partnerSecurityQuestion.Id,
                        PartnerId = partnerSecurityQuestion.PartnerId,
                        NickName = partnerSecurityQuestion.NickName,
                        TranslationId = partnerSecurityQuestion.TranslationId,
                        Status = partnerSecurityQuestion.Status,
                        CreationTime = partnerSecurityQuestion.CreationTime,
                        LastUpdateTime = partnerSecurityQuestion.LastUpdateTime
                    }
                };
            }
        }

        private static ApiResponseBase SavePartnerKey(ApiPartnerKey apiPartnerKey, SessionIdentity identity, ILog log)
        {
            using (var partnerBl = new PartnerBll(identity, log))
            {
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
        }

        private static ApiResponseBase GetUserPasswordReqex(UserTypeInput userTypeInput, SessionIdentity identity, ILog log)
        {
            using (var userBl = new UserBll(identity, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = userBl.GetUserPasswordRegex(userTypeInput.PartnerId, 0, userTypeInput.Type)
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
                    var partnerAccess = partnerBll.GetPermissionsToObject(new CheckPermissionInput
                    {
                        Permission = Constants.Permissions.ViewPartner,
                        ObjectTypeId = ObjectTypes.Partner
                    });
                    if (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleIntegerObjects.All(x => x != partner.Id))
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
            var partner = CacheManager.GetPartnerById(partnerId) ??
                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.ClientNotFound);
            using (var productBll = new ProductBll(identity, log))
            {
                var partnerAccess = productBll.GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewPartner,
                    ObjectTypeId = ObjectTypes.Partner
                });
                if (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleIntegerObjects.All(x => x != partnerId))
                    throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.DontHavePermission);
                var partnerProviderSettings = productBll.GetGameProviderSettings((int)ObjectTypes.Partner, partnerId).Select(x => new ApiGameProviderSetting
                {
                    ObjectId = Convert.ToInt32(x.ObjectId),
                    GameProviderId = x.GameProviderId,
                    GameProviderName = x.GameProvider.Name,
                    State = x.State,
                    Order = x.Order ?? 1
                }).ToList();

                partnerProviderSettings.AddRange(productBll.GetGameProviders(new FilterGameProvider { IsActive = true })
                                                           .Where(x => !partnerProviderSettings.Any(y => y.GameProviderId == x.Id))
                                                           .Select(x => new ApiGameProviderSetting
                                                           {
                                                               ObjectId = partnerId,
                                                               GameProviderId = x.Id,
                                                               GameProviderName = x.Name,
                                                               State = (int)BaseStates.Active, 
                                                               Order = 10000 
                                                           }));
                return new ApiResponseBase
                {
                    ResponseObject = partnerProviderSettings
                };
            }
        }

        public static ApiResponseBase CopyPartnerKeys(ApiCloneObject input, SessionIdentity identity, ILog log)
        {

            using (var partnerBl = new PartnerBll(identity, log))
            {
                var partnerKeys = partnerBl.CopyPartnerKeys(input.FromPartnerId, input.ToPartnerId);

                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        Entities = partnerKeys.Select(x => new ApiPartnerKey
                        {
                            Id = x.Id,
                            Name = x.Name,
                            PartnerId = x.PartnerId,
                            GameProviderId = x.GameProviderId,
                            PaymentSystemId = x.PaymentSystemId
                        }),
                        partnerKeys.Count
                    }
                };
            }
        }

        private static ApiResponseBase GetEmails(ApiFilterEmail apiFilterEmail, SessionIdentity identity, ILog log)
        {
            using (var partnerBl = new PartnerBll(identity, log))
            {
                var filter = apiFilterEmail.MapToFilterEmail();
                var resp = partnerBl.GetEmails(filter, true);
                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        Entities = resp.Entities.MapToEmail(identity.TimeZone),
                        resp.Count
                    }
                };
            }
        }

        private static ApiResponseBase GetPartnerCountrySettings(PartnerSetting partnerSettingInput, SessionIdentity identity, ILog log)
        {
            using (var partnerBl = new PartnerBll(identity, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = partnerBl.GetPartnerCountrySettings(partnerSettingInput.PartnerId, partnerSettingInput.Type)
                    .Select(x => new
                    {
                        x.Id,
                        x.PartnerId,
                        x.Type,
                        x.CountryNickName,
                        x.CountryName,
                        x.IsoCode,
                        x.IsoCode3
                    }).ToList()
                };
            }
        }

        private static ApiResponseBase SavePartnerCountrySetting(PartnerCountrySetting input, SessionIdentity identity, ILog log)
        {
            using (var partnerBl = new PartnerBll(identity, log))
            {
                partnerBl.SavePartnerCountrySetting(input, out List<int> clientIds);
                Helpers.Helpers.InvokeMessage("RemoveKeysFromCache", string.Format("{0}_{1}_{2}_", Constants.CacheItems.PartnerCountrySetting, input.PartnerId, input.Type));
                var partnerCountrySetting = CacheManager.GetPartnerCountrySettings(input.PartnerId, input.Type, identity.LanguageId)
                                                        .FirstOrDefault(x => x.RegionId == input.RegionId);
                foreach (var c in clientIds)
                {
                    CacheManager.RemoveClientSetting(c, Constants.ClientSettings.UnderMonitoring);
                    Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}_{2}", Constants.CacheItems.ClientSettings, c, Constants.ClientSettings.UnderMonitoring));
                }
                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        partnerCountrySetting.Id,
                        partnerCountrySetting.PartnerId,
                        partnerCountrySetting.Type,
                        partnerCountrySetting.CountryNickName,
                        partnerCountrySetting.CountryName,
                        partnerCountrySetting.IsoCode,
                        partnerCountrySetting.IsoCode3
                    }
                };
            }
        }

        private static ApiResponseBase RemovePartnerCountrySetting(PartnerCountrySetting input, SessionIdentity identity, ILog log)
        {
            using (var partnerBl = new PartnerBll(identity, log))
            {
                partnerBl.RemovePartnerCountrySetting(input, out List<int> clientIds);
                Helpers.Helpers.InvokeMessage("RemoveKeysFromCache", string.Format("{0}_{1}_{2}_", Constants.CacheItems.PartnerCountrySetting, input.PartnerId, input.Type));
                foreach (var c in clientIds)
                {
                    CacheManager.RemoveClientSetting(c, Constants.ClientSettings.UnderMonitoring);
                    Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}_{2}", Constants.CacheItems.ClientSettings, c, Constants.ClientSettings.UnderMonitoring));
                }
                return new ApiResponseBase();
            }
        }


        private static ApiResponseBase GetCharacters(ApiFilterCharacter input, SessionIdentity identity, ILog log)
        {
            using (var partnerBll = new PartnerBll(identity, log))
            {
                var characters = partnerBll.GetCharacters(input.PartnerId, input.Id);
                return new ApiResponseBase
                {
                    ResponseObject = characters.Select(x => new ApiCharacter
                    {
                        Id = x.Id,
                        PartnerId = x.PartnerId,
                        ParentId = x.ParentId,
                        NickName = x.NickName,
                        Title = x.Title,
                        Description = x.Description,
                        Order = x.Order,
                        Status = x.Status,
                        ImageData = x.ImageUrl,
                        BackgroundImageData = x.BackgroundImageUrl
                    }).ToList()
                };
            }
        }

        private static ApiResponseBase GetCharacterHierarchy(int partnerId, SessionIdentity identity, ILog log)
        {
            using (var partnerBll = new PartnerBll(identity, log))
            {
                var characters = partnerBll.GetCharacters(partnerId, null).Select(x => x.MapToApiCharacter());
                var parentIds = characters.Where(x => x.ParentId == null).Select(x => x.Id);
				var relations = new List<ApiCharacterRelations>();
				foreach (var pId in parentIds)
				{
					var relation = new ApiCharacterRelations
					{
						Parent = characters.FirstOrDefault(x => x.Id == pId),
						Children = characters.Where(x => x.ParentId == pId).ToList()
					};
					relations.Add(relation);
				}
				return new ApiResponseBase
                {
                    ResponseObject = relations
				};
            }
        }

        private static ApiResponseBase GetCharacterById(int id, SessionIdentity identity, ILog log)
        {
            using (var partnerBll = new PartnerBll(identity, log))
            {
                var character = partnerBll.GetCharacterById(id).MapToApiCharacter() ?? throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.CharacterNotFound);
                var partner = CacheManager.GetPartnerById(character.PartnerId);
                var siteurl = partner.SiteUrl.Split(',')[0];
                character.SiteUrl = siteurl;

				return new ApiResponseBase
                {
                    ResponseObject = character
                };
            }
        }

        private static ApiResponseBase SaveCharacter(ApiCharacter input, SessionIdentity identity, ILog log)
        {
            using (var partnerBll = new PartnerBll(identity, log))
            {
                var character = input.MapToCharacter();
                var ext = input.ImageExtension?.Split('.', '?');
                var extension = ext?.Length > 1 ? "." + ext[1] : ".png";
                character = partnerBll.SaveCharacter(character, input.EnvironmentTypeId, extension);
                return new ApiResponseBase() { ResponseObject = character.MapToApiCharacter() };
            }
        }

        private static ApiResponseBase DeleteCharacterById(ApiCharacter input, SessionIdentity identity, ILog log)
        {
            using (var partnerBll = new PartnerBll(identity, log))
			{
				var character = input.MapToCharacter();
				partnerBll.DeleteCharacterById(character);
                return new ApiResponseBase();
            }
        }
    }
}