using System.Collections.Generic;
using System.Linq;
using IqSoft.CP.Common;
using IqSoft.CP.DAL.Filters;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.DAL.Models;
using log4net;
using IqSoft.CP.AgentWebApi.Models;
using IqSoft.CP.Common.Enums;

namespace IqSoft.CP.AgentWebApi.ControllerClasses
{
    public static class EnumerationModelController
    {
        public static ApiResponseBase CallFunction(RequestBase request, SessionIdentity identity, ILog log)
        {
            switch (request.Method)
            {
                case "GetGendersEnum":
                    return GetTypesEnumByType(Constants.EnumerationTypes.Gender, identity, log);
                case "GetCommonEnumModels":
                    return GetCommonEnumModels(identity, log);
                case "GetDocumenStatesEnum":
                    return GetTypesEnumByType(Constants.EnumerationTypes.DocumentStates, identity, log);
                case "GetProductsEnum":
                    return GetProductsEnum(identity, log);
                case "GetRegionsEnum":
                    return GetRegionsEnum(identity, log);
                case "GetClientCategoriesEnum":
                    return GetClientCategoriesEnum(identity, log);
                case "GetPaymentSystemTypesEnum":
                    return GetTypesEnumByType(Constants.EnumerationTypes.PaymentSystemTypes, identity, log);
                case "GetRegionTypesEnum":
                    return GetTypesEnumByType(Constants.EnumerationTypes.RegionTypes, identity, log);
                case "GetPaymentRequestStatesEnum":
                    return GetTypesEnumByType(Constants.EnumerationTypes.PaymentRequestStates, identity, log);
                case "GetUserStatesEnum":
                    return GetTypesEnumByType(Constants.EnumerationTypes.UserStates, identity, log);
                case "GetUserTypesEnum":
                    return GetTypesEnumByType(Constants.EnumerationTypes.UserTypes, identity, log);
                case "GetPaymentRequestTypesEnum":
                    return GetTypesEnumByType(Constants.EnumerationTypes.PaymentRequestTypes, identity, log);
                case "GetFilterOperationsEnum":
                    return GetFilterOperations(identity, log);
                case "GetProductStatesEnum":
                    return GetTypesEnumByType(Constants.EnumerationTypes.ProductStates, identity, log);
                case "GetClientStatesEnum":
                    return GetTypesEnumByType(Constants.EnumerationTypes.ClientStates, identity, log);
                case "GetDeviceTypesEnum":
                    return GetTypesEnumByType(Constants.EnumerationTypes.DeviceTypes, identity, log);
                case "GetClientDocumentTypesEnum":
                    return GetTypesEnumByType(Constants.EnumerationTypes.ClientDocumentTypes, identity, log);
                case "GetKYCDocumentTypesEnum":
                    return GetTypesEnumByType(Constants.EnumerationTypes.KYCDocumentTypes, identity, log);
                case "GetKYCDocumentStatesEnum":
                    return GetTypesEnumByType(Constants.EnumerationTypes.KYCDocumentStates, identity, log);
                case "GetClientAccountTypesEnum":
                    return GetClientAccountTypesEnum(identity, log);
                case "GetClientLogActionsEnum":
                    return GetTypesEnumByType(Constants.EnumerationTypes.ClientLogActions, identity, log);
                case "GetCreditDocumentTypesEnum":
                    return GetTypesEnumByType(Constants.EnumerationTypes.CreditDocumentTypes, identity, log);
                case "GetPartnersVerificationTypeEnum":
                    return GetTypesEnumByType(Constants.EnumerationTypes.ClientInfoTypes, identity, log);
                case "GetClientPaymentInfoTypesEnum":
                    return GetTypesEnumByType(Constants.EnumerationTypes.ClientPaymentInfoTypes, identity, log);
                case "GetAgentLevelsEnum":
                    return GetTypesEnumByType(Constants.EnumerationTypes.AgentLevels, identity, log);
                case "GetAnnouncementTypesEnum":
                    return GetTypesEnumByType(Constants.EnumerationTypes.AnnouncementTypes, identity, log);
                case "GetActionGroupsEnum":
                    return GetTypesEnumByType(Constants.EnumerationTypes.ActionGroups, identity, log);
                case "GetAgentEmployeePermissions":
                    return GetTypesEnumByType(nameof(AgentEmployeePermissions), identity, log);
                case "GetBetShopGroupsEnum":
                    return GetBetShopGroupsEnum(identity, log);
            }

            throw BaseBll.CreateException(string.Empty, Constants.Errors.MethodNotFound);
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
                currencies = currencyBl.GetCurrencies(false).Select(x => new EnumerationModel<string>
                {
                    Id = x.Id,
                    Name = x.Symbol
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

        private static ApiResponseBase GetFilterOperations(SessionIdentity identity, ILog log)
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
                    x.Id == (int)AccountTypes.ClientUnusedBalance || x.Id == (int)AccountTypes.BonusWin ||
                    x.Id == (int)AccountTypes.ClientUsedBalance || x.Id == (int)AccountTypes.AffiliateManagerBalance).
                    Select(x => new EnumerationModel<int>
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

        private static ApiResponseBase GetTypesEnumByType(string enumType, SessionIdentity identity, ILog log)
        {
            return new ApiResponseBase
            {
                ResponseObject = BaseBll.GetEnumerations(enumType, identity.LanguageId).Select(x => new EnumerationModel<int>
                {
                    Id = x.Value,
                    Name = x.Text
                })
            };
        }

        private static ApiResponseBase GetBetShopGroupsEnum(SessionIdentity identity, ILog log)
        {
            using (var betShopBl = new BetShopBll(identity, log))
            {
                var betShopGroup = betShopBl.GetBetShopGroups(new FilterBetShopGroup(), false).Select(x => new EnumerationModel<int>
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
    }
}