using System.Collections.Generic;
using System.Linq;
using IqSoft.CP.Common;
using IqSoft.CP.DAL.Filters;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.AdminWebApi.Models.CommonModels;
using log4net;
using IqSoft.CP.Common.Enums;
using Newtonsoft.Json;

namespace IqSoft.CP.AdminWebApi.ControllerClasses
{
    public static class EnumerationModelController
    {
        public static ApiResponseBase CallFunction(RequestBase request, SessionIdentity identity, ILog log)
        {
            switch (request.Method)
            {
                case "GetEnumerations":
                    return GetEnumerations(JsonConvert.DeserializeObject<string>(request.RequestData), identity, log);
                case "GetPartnersStateEnum":
                    return GetTypesEnumByType(Constants.EnumerationTypes.PartnerStates, identity);
                case "GetPartnersClientVerificationTypeEnum":
                    return GetTypesEnumByType(Constants.EnumerationTypes.PartnerClientVerificationTypes, identity);
                case "GetGendersEnum":
                    return GetTypesEnumByType(Constants.EnumerationTypes.Gender, identity);
                case "GetCommonEnumModels":
                    return GetCommonEnumModels(identity, log);
                case "GetCashDeskStatesEnum":
                    return GetTypesEnumByType(Constants.EnumerationTypes.CashDeskStates, identity);
                case "GetDocumentStatesEnum":
                    return GetTypesEnumByType(Constants.EnumerationTypes.DocumentStates, identity);
                case "GetProductsEnum":
                    return GetProductsEnum(identity, log);
                case "GetRegionsEnum":
                    return GetRegionsEnum(identity, log);
                case "GetBetShopStatesEnum":
                    return GetTypesEnumByType(Constants.EnumerationTypes.BetShopStates, identity);
                case "GetBetShopGroupsEnum":
                    return GetBetShopGroupsEnum(identity, log);
                case "GetClientCategoriesEnum":
                    return GetClientCategoriesEnum(identity, log);
                case "GetPartnerPaymentSettingStatesEnum":
                    return GetTypesEnumByType(Constants.EnumerationTypes.PartnerPaymentSettingStates, identity);
                case "GetPartnerProductSettingStatesEnum":
                    return GetTypesEnumByType(Constants.EnumerationTypes.PartnerProductSettingStates, identity);
                case "GetPaymentSystemTypesEnum":
                    return GetTypesEnumByType(Constants.EnumerationTypes.PaymentSystemTypes, identity);
                case "GetRegionTypesEnum":
                    return GetTypesEnumByType(Constants.EnumerationTypes.RegionTypes, identity);
                case "GetPaymentRequestStatesEnum":
                    return GetTypesEnumByType(Constants.EnumerationTypes.PaymentRequestStates, identity);
                case "GetClientMessageTypesEnum":
                    return GetTypesEnumByType(Constants.EnumerationTypes.ClientMessageTypes, identity);
                case "GetUserStatesEnum":
                    return GetTypesEnumByType(Constants.EnumerationTypes.UserStates, identity);
                case "GetUserTypesEnum":
                    return GetUserTypesEnum(identity);
                case "GetPaymentRequestTypesEnum":
                    return GetTypesEnumByType(Constants.EnumerationTypes.PaymentRequestTypes, identity);
                case "GetFilterOperationsEnum":
                    return GetFilterOperations(identity);
                case "GetProductStatesEnum":
                    return GetTypesEnumByType(Constants.EnumerationTypes.ProductStates, identity);
                case "GetClientStatesEnum":
                    return GetTypesEnumByType(Constants.EnumerationTypes.ClientStates, identity);
                case "GetDeviceTypesEnum":
                    return GetTypesEnumByType(Constants.EnumerationTypes.DeviceTypes, identity);
                case "GetClientDocumentTypesEnum":
                    return GetTypesEnumByType(Constants.EnumerationTypes.ClientDocumentTypes, identity);
                case "GetKYCDocumentTypesEnum":
                    return GetTypesEnumByType(Constants.EnumerationTypes.KYCDocumentTypes, identity);
                case "GetKYCDocumentStatesEnum":
                    return GetTypesEnumByType(Constants.EnumerationTypes.KYCDocumentStates, identity);
                case "GetClientAccountTypesEnum":
                    return GetClientAccountTypesEnum(identity, log);
                case "GetClientLogActionsEnum":
                    return GetTypesEnumByType(Constants.EnumerationTypes.ClientLogActions, identity);
                case "GetCreditDocumentTypesEnum":
                    return GetTypesEnumByType(Constants.EnumerationTypes.CreditDocumentTypes, identity);
                case "GetPartnersVerificationTypeEnum":
                    return GetTypesEnumByType(Constants.EnumerationTypes.ClientInfoTypes, identity);
                case "GetClientPaymentInfoTypesEnum":
                    return GetTypesEnumByType(Constants.EnumerationTypes.ClientPaymentInfoTypes, identity);
                case "GetProductGroupsTypesEnum":
                    return GetTypesEnumByType(Constants.EnumerationTypes.ProductGroupTypes, identity);
                case "GetBannerTypes":
                    return GetTypesEnumByType(Constants.EnumerationTypes.BannerTypes, identity);
                case "GetTicketsTypesEnum":
                    return GetTypesEnumByType(Constants.EnumerationTypes.TicketTypes, identity);
                case "GetCRMSettingTypesEnum":
                    return GetTypesEnumByType(Constants.EnumerationTypes.CRMSettingTypes, identity);
                case "GetAgentLevelsEnum":
                    return GetTypesEnumByType(Constants.EnumerationTypes.AgentLevels, identity);
                case "GetAnnouncementTypesEnum":
                    return GetTypesEnumByType(Constants.EnumerationTypes.AnnouncementTypes, identity);
                case "GetCasinoLayoutTypesEnum":
                    return GetCasinoLayoutTypesEnum();
                case "GetTriggerTypesEnum":
                    return GetTypesEnumByType(Constants.EnumerationTypes.TriggerTypes, identity);
                case "GetLogoutTypesEnum":
                    return GetTypesEnumByType(Constants.EnumerationTypes.LogoutTypes, identity);
                case "GetCommentTemplateTypesEnum":
                    return GetTypesEnumByType(nameof(CommentTemplateTypes), identity);
                case "GetEmailStatesEnum":
                    return GetTypesEnumByType(nameof(EmailStates), identity);
                case "GetClientPaymentStatesEnum":
                    return GetTypesEnumByType(nameof(ClientPaymentStates), identity);
                case "GetPromotionTypesEnum":
                    return GetTypesEnumByType(nameof(PromotionTypes), identity);
                case "GetReferralTypesEnum":
                    return GetTypesEnumByType(nameof(ReferralTypes), identity);
                case "GetMessageTemplateStatesEnum":
                    return GetTypesEnumByType(Constants.EnumerationTypes.MessageTemplateStates, identity);
                case "GetCommunicationTypesEnum":
                    return GetTypesEnumByType(Constants.EnumerationTypes.CommunicationTypes, identity);
                case "GetOperationTypesEnum":
                    return GetOperationTypesEnum(identity, log);
                case "GetPartnerCountrySettingTypesEnum":
                    return GetTypesEnumByType(nameof(PartnerCountrySettingTypes), identity);
                case "GetClientTitlesEnum":
                    return GetTypesEnumByType(nameof(ClientTitles), identity);
                case "GetClientPaymentInfoStates":
                    return GetTypesEnumByType(nameof(ClientPaymentInfoStates), identity);
                case "GetUnderMonitoringTypesEnum":
                    return GetTypesEnumByType(nameof(UnderMonitoringTypes), identity);
                case "GetSessionStatesEnum":
                    return GetTypesEnumByType(nameof(SessionStates), identity);
                case "GetBonusTypesEnum":
                    return GetTypesEnumByType(nameof(BonusTypes), identity);
                case "GetPopupTypesEnum":
                    return GetTypesEnumByType(nameof(PopupTypes), identity);
            }
            throw BaseBll.CreateException(string.Empty, Constants.Errors.MethodNotFound);
        }

        private static ApiResponseBase GetBetShopGroupsEnum(SessionIdentity identity, ILog log)
        {
            using (var betShopBl = new BetShopBll(identity, log))
            {
                var betShopGroup = betShopBl.GetBetShopGroups(new FilterBetShopGroup(), true).Select(x => new EnumerationModel<int>
                {
                    Id = x.Id,
                    Name = x.Name
                });

                var response = new ApiResponseBase
                {
                    ResponseObject = betShopGroup
                };
                return response;
            }
        }

        private static ApiResponseBase GetRegionsEnum(SessionIdentity identity, ILog log)
        {
            using (var regionBl = new RegionBll(identity, log))
            {
                var regions = regionBl.GetfnRegions(new FilterRegion { }, identity.LanguageId, true, null).Select(x => new EnumerationModel<int>
                {
                    Id = x.Id,
                    Name = x.Name
                }).ToList();
                var response = new ApiResponseBase
                {
                    ResponseObject = regions
                };
                return response;
            }
        }

        private static ApiResponseBase GetCommonEnumModels(SessionIdentity identity, ILog log)
        {
            List<EnumerationModel<int>> genders;
            List<EnumerationModel<string>> currencies;
            List<EnumerationModel<string>> languages;
            genders = BaseBll.GetEnumerations(Constants.EnumerationTypes.Gender, identity.LanguageId).Select(x => new EnumerationModel<int>
            {
                Id = x.Value,
                Name = x.Text
            }).ToList();

            using (var currencyBl = new CurrencyBll(identity, log))
            {
                currencies = currencyBl.GetCurrencies(true).Select(x => new EnumerationModel<string>
                {
                    Id = x.Id,
                    Name = x.Id
                }).ToList();
            }

            languages = CacheManager.GetAvailableLanguages().Select(x => new EnumerationModel<string>
            {
                Id = x.Id,
                Name = x.Name
            }).ToList();

            var response = new ApiResponseBase
            {
                ResponseObject = new
                {
                    genders,
                    currencies,
                    languages
                }
            };
            return response;
        }

        private static ApiResponseBase GetProductsEnum(SessionIdentity identity, ILog log)
        {
            using (var basebl = new ProductBll(identity, log))
            {
                var products = basebl.GetProducts(new FilterProduct()).Select(x => new EnumerationModel<int>
                {
                    Id = x.Id,
                    Name = x.NickName
                }).ToList();
                var response = new ApiResponseBase
                {
                    ResponseObject = products
                };
                return response;
            }
        }

        private static ApiResponseBase GetClientCategoriesEnum(SessionIdentity identity, ILog log)
        {
            using (var clientbl = new ClientBll(identity, log))
            {
                var clientCategories = clientbl.GetClientCategories().Select(x => new EnumerationModel<int>
                {
                    Id = x.Id,
                    Name = x.NickName
                }).ToList();

                var response = new ApiResponseBase
                {
                    ResponseObject = clientCategories
                };
                return response;
            }
        }

        private static ApiResponseBase GetFilterOperations(SessionIdentity identity)
        {
            var operationTypes = BaseBll.GetEnumerations(Constants.EnumerationTypes.FilterOperations, identity.LanguageId).Select(x => new EnumerationModel<int>
            {
                Id = x.Value,
                NickName = x.NickName,
                Name = x.Text
            }).ToList();
            return new ApiResponseBase
            {
                ResponseObject = operationTypes
            };
        }

        private static ApiResponseBase GetClientAccountTypesEnum(SessionIdentity identity, ILog log)
        {
            using (var clientBl = new ClientBll(identity, log))
            {
                var accountTypes = clientBl.GetAccountTypes(Constants.DefaultLanguageId).Where(x =>
                    x.Id == (int)AccountTypes.ClientUnusedBalance || x.Id == (int)AccountTypes.ClientUsedBalance || x.Id == (int)AccountTypes.BonusWin ||
                    x.Id == (int)AccountTypes.ClientCoinBalance || x.Id == (int)AccountTypes.ClientCompBalance ||
                    x.Id == (int)AccountTypes.ClientBonusBalance || x.Id == (int)AccountTypes.AffiliateManagerBalance)
                    .Select(x => new EnumerationModel<int>
                    {
                        Id = x.Id,
                        Name = x.Name
                    }).ToList();

                return new ApiResponseBase
                {
                    ResponseObject = accountTypes
                };
            }
        }

        private static ApiResponseBase GetUserTypesEnum(SessionIdentity identity)
        {
            var resp = BaseBll.GetEnumerations(Constants.EnumerationTypes.UserTypes, identity.LanguageId)
                              .Where(x => x.Value == (int)UserTypes.AdminUser || x.Value == (int)UserTypes.Cashier)
                              .Select(x => new EnumerationModel<int>
                              {
                                  Id = x.Value,
                                  Name = x.Text
                              });
            return new ApiResponseBase
            {
                ResponseObject = resp
            };
        }

        private static ApiResponseBase GetTypesEnumByType(string enumType, SessionIdentity identity)
        {
            return new ApiResponseBase
            {
                ResponseObject = BaseBll.GetEnumerations(enumType, identity.LanguageId).Select(x => new EnumerationModel<int>
                {
                    Id = x.Value,
                    Name = x.Text
                }).OrderBy(x => x.Name).ToList()
            };
        }        

        private static ApiResponseBase GetCasinoLayoutTypesEnum()
        {
            return new ApiResponseBase
            {
                ResponseObject = BaseBll.GetEnumerations(Constants.EnumerationTypes.CasinoLayoutTypes, Constants.DefaultLanguageId).Select(x => new EnumerationModel<int>
                {
                    Id = x.Value,
                    NickName = x.NickName
                }).ToList()
            };
        }

        private static ApiResponseBase GetEnumerations(string languageId, SessionIdentity identity, ILog log)
        {
            using (var contentBl = new ContentBll(identity, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = contentBl.GetEnumerations(!string.IsNullOrEmpty(languageId) ? languageId : Constants.DefaultLanguageId).Select(x => new
                    {
                        x.Id,
                        x.EnumType,
                        x.NickName,
                        x.Value,
                        x.TranslationId,
                        x.Text,
                        x.LanguageId
                    }).ToList()
                };
            }
        }

        private static ApiResponseBase GetOperationTypesEnum(SessionIdentity identity, ILog log)
        {
            using (var baseBl = new BaseBll(identity, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = baseBl.GetOperationTypes().Select(x => new EnumerationModel<int>
                    {
                        Id = x.Id,
                        NickName = x.NickName,
                        Name = x.Name
                    }).ToList()
                };
            }
        }
    }
}