﻿using System.Linq;
using System.Collections.Generic;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Filters;
using IqSoft.CP.AdminWebApi.Filters;
using IqSoft.CP.AdminWebApi.Helpers;
using Newtonsoft.Json;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.AdminWebApi.Models.CommonModels;
using IqSoft.CP.AdminWebApi.Models.ProductModels;
using log4net;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models.Products;
using IqSoft.CP.BLL.Caching;
using System;
using IqSoft.CP.Integration.Products.Models.SoftGaming;
using IqSoft.CP.AdminWebApi.Models.ContentModels;
using System.Threading.Tasks;
using IqSoft.CP.Common.Models.AdminModels;
using IqSoft.CP.Common.Models;
using IqSoft.CP.AdminWebApi.Models.PartnerModels;

namespace IqSoft.CP.AdminWebApi.ControllerClasses
{
    public static class ProductController
    {
        internal static ApiResponseBase CallFunction(RequestBase request, SessionIdentity identity, ILog log)
        {
            switch (request.Method)
            {
                case "GetProducts":
                    return GetProducts(JsonConvert.DeserializeObject<ApiFilterfnProduct>(request.RequestData), identity, log);
                case "GetProductById":
                    return GetProductById(Convert.ToInt32(request.RequestData), identity, log);
                case "GetGameProviders":
                    return GetGameProviders(JsonConvert.DeserializeObject<ApiFilterGameProvider>(request.RequestData), identity, log);
                case "GetGameProviderById":
                    return GetGameProviderById(Convert.ToInt32(request.RequestData), identity, log);
                case "SaveGameProvider":
                    return SaveGameProvider(JsonConvert.DeserializeObject<ApiGameProvider>(request.RequestData), identity, log);
                case "GetProductDetails":
                    return GetProductDetails(JsonConvert.DeserializeObject<int>(request.RequestData), identity, log);
                case "AddProduct":
                    return AddProduct(JsonConvert.DeserializeObject<ApiProduct>(request.RequestData), identity, log);
                case "EditProduct":
                    return EditProduct(JsonConvert.DeserializeObject<FnProductModel>(request.RequestData), identity, log);
                case "SaveProductsCountrySetting":
                    return SaveProductsCountrySetting(JsonConvert.DeserializeObject<ApiProduct>(request.RequestData), identity, log);
                case "GetPartnerProducts":
                    return
                        GetPartnerProducts(
                            JsonConvert.DeserializeObject<ApiFilterPartnerProductSetting>(request.RequestData), identity, log);
                case "ExportPartnerProducts":
                    return
                        ExportPartnerProducts(
                            JsonConvert.DeserializeObject<ApiFilterPartnerProductSetting>(request.RequestData), identity, log);
                case "GetPartnerProductSettings":
                    return
                        GetPartnerProductSettings(
                            JsonConvert.DeserializeObject<ApiFilterPartnerProductSetting>(request.RequestData), identity, log);
                case "ExportPartnerProductSettings":
                    return
                        ExportfnPartnerProductSettings(
                            JsonConvert.DeserializeObject<ApiFilterPartnerProductSetting>(request.RequestData), identity, log);
                case "SavePartnerProductSettings":
                    return
                        SavePartnerProductSettings(JsonConvert.DeserializeObject<ApiPartnerProductSettingInput>(request.RequestData), identity, log);
                case "RemovePartnerProductSettings":
                    return
                        RemovePartnerProductSettings(JsonConvert.DeserializeObject<ApiPartnerProductSettingInput>(request.RequestData), identity, log);
                case "CopyPartnerProductSettings":
                    return
                        CopyPartnerProductSettings(JsonConvert.DeserializeObject<ApiCloneObject>(request.RequestData), identity, log);
                case "ExportProducts":
                    return ExportProducts(JsonConvert.DeserializeObject<ApiFilterfnProduct>(request.RequestData),
                        identity, log);
                case "ChangePartnerProductState":
                    return ChangePartnerProductState(JsonConvert.DeserializeObject<PartnerProductSetting>(request.RequestData), identity, log);
                case "GetProductCategories":
                    return GetProductCategories(identity, log);
                case "SaveProductCategory":
                    return SaveProductCategory(JsonConvert.DeserializeObject<ApiProductCategory>(request.RequestData), identity, log);
                case "SynchronizeProviderProducts":
                    return SynchronizeProviderProducts(Convert.ToInt32(request.RequestData), identity, log);
            }
            throw BaseBll.CreateException(string.Empty, Constants.Errors.MethodNotFound);
        }

        public static ApiResponseBase GetProducts(ApiFilterfnProduct apiFilter, SessionIdentity identity, ILog log)
        {
            using (var productsBl = new ProductBll(identity, log))
            {
                using (var bonusService = new BonusService(productsBl))
                {
                    var user = CacheManager.GetUserById(identity.Id) ??
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.UserNotFound);

                    var result = new List<fnProduct>();
                    long totalCount = 0;

                    if (apiFilter.ParentId == null)
                    {
                        var products = productsBl.GetFnProducts(new FilterfnProduct { ProductId = (int)Constants.PlatformProductId }, !(user.Type == (int)UserTypes.CompanyAgent ||
                            user.Type == (int)UserTypes.DownlineAgent || user.Type == (int)UserTypes.AgentEmployee));
                        result = products.Entities.ToList();
                        totalCount = products.Count;
                    }
                    else
                    {
                        var productFilter = apiFilter.MapToFilterfnProduct();
                        productFilter.IsProviderActive = true;
                        productFilter.Pattern = null;
                        List<int> productIds = null;
                        List<int> groupIds = new List<int>();

                        if (apiFilter.BonusId.HasValue)
                        {
                            var bonus = CacheManager.GetBonusById(apiFilter.BonusId.Value);
                            if (bonus == null)
                                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.BonusNotFound);

                            var filter = apiFilter.MapTofnPartnerProductSettings();
                            filter.PartnerId = bonus.PartnerId;
                            filter.IsProviderActive = true;
                            if (bonus.Type == (int)BonusTypes.CampaignFreeSpin)
                                filter.FreeSpinSupport = true;
                            
                            if (apiFilter.Counts != null || apiFilter.Lines != null || apiFilter.Coins != null ||
                                apiFilter.CoinValues != null || apiFilter.BetValues != null)
                                productIds = bonusService.GetBonusProducts(new FilterBonusProduct
                                {
                                    BonusId = apiFilter.BonusId,
                                    Percents = apiFilter.Percents == null ? new FiltersOperation() : apiFilter.Percents.MapToFiltersOperation(identity.TimeZone),
                                    Counts = apiFilter.Counts == null ? new FiltersOperation() : apiFilter.Counts.MapToFiltersOperation(identity.TimeZone),
                                    Lines = apiFilter.Lines == null ? new FiltersOperation() : apiFilter.Lines.MapToFiltersOperation(identity.TimeZone),
                                    Coins = apiFilter.Coins == null ? new FiltersOperation() : apiFilter.Coins.MapToFiltersOperation(identity.TimeZone),
                                    CoinValues = apiFilter.CoinValues == null ? new FiltersOperation() : apiFilter.CoinValues.MapToFiltersOperation(identity.TimeZone),
                                    BetValues = apiFilter.BetValues == null ? new FiltersOperation() : apiFilter.BetValues.MapToFiltersOperation(identity.TimeZone)
                                }).Select(x => x.ProductId).ToList();

                            filter.ParentId = null;
                            filter.ProductId = null;
                            if (productIds != null && productIds.Count > 0)
                                filter.ProductIds = new FiltersOperation
                                {
                                    IsAnd = true,
                                    OperationTypeList = new List<FiltersOperationType> { new FiltersOperationType
                                    {
                                        OperationTypeId = (int)FilterOperations.InSet,
                                        StringValue = string.Join(",", productIds)
                                    } }
                                };
                            filter.Path = "/" + productFilter.ParentId.Value + "/";
                            if (productIds == null || productIds.Count > 0)
                            {
                                var games = productsBl.GetfnPartnerProductSettings(filter, !(user.Type == (int)UserTypes.CompanyAgent ||
                                    user.Type == (int)UserTypes.DownlineAgent || user.Type == (int)UserTypes.AgentEmployee)).Entities.ToList();

                                foreach (var g in games)
                                {
                                    var items = g.Path.Split('/').ToList();
                                    foreach (var item in items)
                                    {
                                        if (!string.IsNullOrEmpty(item) && Int32.TryParse(item, out int groupId) && groupId != g.ProductId)
                                            groupIds.Add(groupId);
                                    }
                                }
                                groupIds = groupIds.Distinct().ToList();
                                productFilter.GroupIds = groupIds;
                            }

                            productFilter.PartnerId = bonus.PartnerId;
                        }
                        else if (!string.IsNullOrEmpty(apiFilter.Pattern))
                        {
                            var filter = apiFilter.MapToFilterfnProduct();
                            filter.ParentId = null;
                            filter.ProductId = null;
                            filter.Path = "/" + productFilter.ParentId.Value + "/";

                            var games = productsBl.GetFnProducts(filter, !(user.Type == (int)UserTypes.CompanyAgent ||
                                user.Type == (int)UserTypes.DownlineAgent || user.Type == (int)UserTypes.AgentEmployee)).Entities.ToList();
                            productIds = games.Select(x => x.Id).ToList();

                            foreach (var g in games)
                            {
                                var items = g.Path.Split('/').ToList();
                                foreach (var item in items)
                                    if (!string.IsNullOrEmpty(item))
                                        productIds.Add(Convert.ToInt32(item));
                            }
                            productIds = productIds.Distinct().ToList();
                            if (productIds.Count == 0)
                                productIds.Add(Constants.PlatformProductId);
                        }
                        if (productIds != null)
                        {
                            var filtersOperationType = new FiltersOperationType
                            {
                                OperationTypeId = (int)FilterOperations.InSet,
                                StringValue = string.Join(",", productIds)
                            };
                            if (productFilter.Ids != null && productFilter.Ids.OperationTypeList != null)
                                productFilter.Ids.OperationTypeList.Add(filtersOperationType);
                            else
                                productFilter.Ids = new FiltersOperation
                                {
                                    IsAnd = true,
                                    OperationTypeList = new List<FiltersOperationType> { filtersOperationType }
                                };
                        }
                        
                        var products = productsBl.GetFnProducts(productFilter, !(user.Type == (int)UserTypes.CompanyAgent ||
                        user.Type == (int)UserTypes.DownlineAgent || user.Type == (int)UserTypes.AgentEmployee));

                        result = products.Entities.ToList();
                        totalCount = products.Count;
                    }

                    if (result.All(x => x.GameProviderId == null))
                        result = result.OrderBy(x => x.Name).ToList();
                    return new ApiResponseBase
                    {
                        ResponseObject = new
                        {
                            Count = totalCount,
                            Entities = result.Select(x=>x.MapTofnProductModel(identity.TimeZone, productsBl.HideAggregatorInfo()))
                        }
                    };
                }
            }
        }

        public static ApiResponseBase GetProductById(int productId, SessionIdentity identity, ILog log)
        {
            using (var productsBl = new ProductBll(identity, log))
            {
                var resp = productsBl.GetfnProductById(productId, true, identity.LanguageId);
                return new ApiResponseBase
                {
                    ResponseObject = resp.MapTofnProductModel(identity.TimeZone, productsBl.HideAggregatorInfo())
                };
            }
        }

        private static ApiResponseBase GetGameProviders(ApiFilterGameProvider filter, SessionIdentity identity, ILog log)
        {
            using (var productBl = new ProductBll(identity, log))
            {
                var providers = productBl.GetGameProviders(filter.MapToFilterGameProvider());
                return new ApiResponseBase
                {
                    ResponseObject = providers.Select(x => new ApiGameProvider
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Type = x.Type,
                        IsActive = x.IsActive,
                        SessionExpireTime = x.SessionExpireTime,
                        GameLaunchUrl = x.GameLaunchUrl
                    }).ToList()
                };
            }
        }

        private static ApiResponseBase GetGameProviderById(int gameProviderId, SessionIdentity identity, ILog log)
        {
            using (var productBl = new ProductBll(identity, log))
            {
                var provider = CacheManager.GetGameProviderById(gameProviderId);
                if (productBl.HideAggregatorInfo() && provider.Type == (int)ProviderTypes.Aggregator)
                    throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.DontHavePermission);
                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        provider.Id,
                        provider.Name,
                        provider.Type,
                        provider.IsActive,
                        provider.SessionExpireTime,
                        provider.GameLaunchUrl,
                        CurrencySetting = provider.CurrencySetting.Select(x => new { x.Id, x.Type, x.CurrencyIds }).ToList()
                    }
                };
            }
        }

        private static ApiResponseBase SaveGameProvider(ApiGameProvider input, SessionIdentity identity, ILog log)
        {
            using (var productBl = new ProductBll(identity, log))
            {
                var result = productBl.SaveGameProvider(new ApiGameProvider
                {
                    Id = input.Id,
                    Ids = input.Ids,
                    Type = input.Type,
                    IsActive = input.IsActive,
                    SessionExpireTime = input.SessionExpireTime,
                    Name = input.Name,
                    GameLaunchUrl = input.GameLaunchUrl,
                    GameProviderCurrencySettings = input.CurrencySetting?.Names?.Select(x => new ApiGameProviderCurrencySetting
                    {
                        CurrencyId = x,
                        Type = input.CurrencySetting.Type ?? (int)ProductCountrySettingTypes.Restricted
                    }).ToList(),
                });
                if (input.Id.HasValue && input.Id >= 0)
                {
                    Helpers.Helpers.InvokeMessage(string.Format("{0}_{1}", Constants.CacheItems.GameProviders, input.Id));
                    Helpers.Helpers.InvokeMessage(string.Format("{0}_{1}", Constants.CacheItems.GameProviders, input.Name));
                }
                else
                    result.ForEach(x =>
                    {
                        Helpers.Helpers.InvokeMessage(string.Format("{0}_{1}", Constants.CacheItems.GameProviders, x.Id));
                        Helpers.Helpers.InvokeMessage(string.Format("{0}_{1}", Constants.CacheItems.GameProviders, x.Name));
                    });
                return new ApiResponseBase
                {
                    ResponseObject = result.Select(x => new
                    {
                        x.Id,
                        x.Name,
                        x.Type,
                        x.SessionExpireTime,
                        x.GameLaunchUrl,
                        x.IsActive
                    })
                };
            }
        }

        private static ApiResponseBase GetProductDetails(int productId, SessionIdentity identity, ILog log)
        {
            using (var productBl = new ProductBll(identity, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = productBl.GetfnProductById(productId, true).MapTofnProductModel(identity.TimeZone, productBl.HideAggregatorInfo())
                };
            }
        }

        private static ApiResponseBase AddProduct(ApiProduct product, SessionIdentity identity, ILog log)
        {
            using (var partnerBl = new PartnerBll(identity, log))
            {
                using (var productBl = new ProductBll(identity, log))
                {
                    var input = product.MapTofnProduct();
                    input.IsNewObject = true;
                    var ftpModel = partnerBl.GetPartnerEnvironments(Constants.MainPartnerId).FirstOrDefault();
                    var res = productBl.SaveProduct(input, string.Empty, ftpModel.Value);
                    Helpers.Helpers.InvokeMessage("UpdateProduct", res.Id);
                    return new ApiResponseBase
                    {
                        ResponseObject = productBl.GetfnProductById(res.Id, true).MapTofnProductModel(identity.TimeZone, productBl.HideAggregatorInfo())
                    };
                }
            }
        }

        private static ApiResponseBase SaveProductsCountrySetting(ApiProduct product, SessionIdentity identity, ILog log)
        {
            if (product.Countries != null && product.Countries.Ids!= null && product.Countries.Ids.Any() &&
               (!product.Countries.Type.HasValue || !Enum.IsDefined(typeof(ProductCountrySettingTypes), product.Countries.Type)))
                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongInputParameters);
            using (var productsBl = new ProductBll(identity, log))
            {
                productsBl.SaveProductsCountrySetting(product.Ids, product.Countries?.Ids?.Select(x => new ProductCountrySetting
                {
                    CountryId = x,
                    Type = product.Countries.Type ?? (int)ProductCountrySettingTypes.Restricted
                }).ToList(), product.State, out List<int> partners);

                foreach (var id in product.Ids)
                    Helpers.Helpers.InvokeMessage("UpdateProduct", id);
            }
            return new ApiResponseBase();
        }

        private static ApiResponseBase EditProduct(FnProductModel product, SessionIdentity identity, ILog log)
        {
            if (product.Countries != null && !Enum.IsDefined(typeof(BonusSettingConditionTypes), product.Countries.Type))
                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongInputParameters);
            using (var partnerBl = new PartnerBll(identity, log))
            {
                using (var productBl = new ProductBll(identity, log))
                {
                    var input = product.MapTofnProduct();
                    input.IsNewObject = false;
                    var ftpModel = partnerBl.GetPartnerEnvironments(Constants.MainPartnerId).FirstOrDefault();
                    var res = productBl.SaveProduct(input, string.Empty, ftpModel.Value);
                    Helpers.Helpers.InvokeMessage("UpdateProduct", res.Id);
                    return new ApiResponseBase
                    {
                        ResponseObject = productBl.GetfnProductById(res.Id, true).MapTofnProductModel(identity.TimeZone, productBl.HideAggregatorInfo())
                    };
                }
            }
        }

        private static ApiResponseBase GetPartnerProducts(ApiFilterPartnerProductSetting input, SessionIdentity identity, ILog log)
        {
            using (var productBl = new ProductBll(identity, log))
            {
                var products = productBl.GetPartnerProducts(input.PartnerId, input.MapTofnProductSettings());

                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        Entities = products.Entities.Select(x=>x.MapTofnProductModel(identity.TimeZone, productBl.HideAggregatorInfo())).ToList(),
                        products.Count
                    }
                };
            }
        }

        private static ApiResponseBase GetPartnerProductSettings(ApiFilterPartnerProductSetting input, SessionIdentity identity, ILog log)
        {
            using (var productBl = new ProductBll(identity, log))
            {
                var partnerProducts = productBl.GetfnPartnerProductSettings(input.MapTofnPartnerProductSettings(identity.TimeZone), true);

                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        Entities = partnerProducts.Entities.Select(x => x.MapTofnPartnerProductSettingModel(identity.TimeZone, productBl.HideAggregatorInfo())).ToList(),
                        partnerProducts.Count
                    }
                };
            }
        }

        private static ApiResponseBase ExportfnPartnerProductSettings(ApiFilterPartnerProductSetting filter, SessionIdentity identity, ILog log)
        {
            using (var productBl = new ProductBll(identity, log))
            {
                var partnerSettings = productBl.ExportfnPartnerProductSettings(filter.MapTofnPartnerProductSettings(identity.TimeZone))
                                               .Entities.Select(x=>x.MapTofnPartnerProductSettingModel(identity.TimeZone, productBl.HideAggregatorInfo())).ToList();
                string fileName = "ExportPartnerProductSettings.csv";
                string fileAbsPath = productBl.ExportToCSV<FnPartnerProductSettingModel>(fileName, partnerSettings, null, null, identity.TimeZone, filter.AdminMenuId);

                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        ExportedFilePath = fileAbsPath
                    }
                };
            }
        }

        private static ApiResponseBase ExportPartnerProducts(ApiFilterPartnerProductSetting filter, SessionIdentity identity, ILog log)
        {
            using (var productBl = new ProductBll(identity, log))
            {
                var partnerProducts = productBl.ExportPartnerProducts(filter.PartnerId, filter.MapTofnProductSettings())
                                               .Entities.Select(x=>x.MapTofnProductModel(identity.TimeZone, productBl.HideAggregatorInfo())).ToList();
                string fileName = "ExportPartnerProducts.csv";
                string fileAbsPath = productBl.ExportToCSV<FnProductModel>(fileName, partnerProducts, null, null, identity.TimeZone, filter.AdminMenuId);

                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        ExportedFilePath = fileAbsPath
                    }
                };
            }
        }

        private static ApiResponseBase CopyPartnerProductSettings(ApiCloneObject input, SessionIdentity identity, ILog log)
        {
            using (var productBl = new ProductBll(identity, log))
            {
                var partnerProducs = productBl.CopyPartnerProductSetting(input.FromPartnerId, input.ToPartnerId);

                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        Entities = partnerProducs.Entities.Select(x => x.MapTofnPartnerProductSettingModel(identity.TimeZone, productBl.HideAggregatorInfo())).ToList(),
                        partnerProducs.Count
                    }
                };
            }
        }

        private static ApiResponseBase SavePartnerProductSettings(ApiPartnerProductSettingInput apiPartnerProductSetting, SessionIdentity identity, ILog log)
        {
            using (var productBl = new ProductBll(identity, log))
            {
                var result = productBl.SavePartnerProductSettings(apiPartnerProductSetting);
                var currencies = CacheManager.GetPartnerCurrencies(apiPartnerProductSetting.PartnerId);
                Parallel.ForEach(apiPartnerProductSetting.ProductIds, productId =>
                {
                    CacheManager.RemovePartnerProductSetting(apiPartnerProductSetting.PartnerId, productId);
                    BaseController.BroadcastCacheChanges(apiPartnerProductSetting.PartnerId, string.Format("{0}_{1}_{2}", Constants.CacheItems.PartnerProductSettings,
                     apiPartnerProductSetting.PartnerId, productId));
                    
                    Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}_{2}", Constants.CacheItems.PartnerProductSettings, 
                        apiPartnerProductSetting.PartnerId, productId));
                    Helpers.Helpers.InvokeMessage("RemovePartnerProductSettings", apiPartnerProductSetting.PartnerId, productId);
                });

                var key = string.Format("{0}_{1}", Constants.CacheItems.ClientProductCategories, apiPartnerProductSetting.PartnerId);
                CacheManager.RemoveFromCache(key);
                Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", key);

                var resp = CacheManager.RemovePartnerProductSettingPages(apiPartnerProductSetting.PartnerId);

                foreach (var r in resp)
                {
                    Helpers.Helpers.InvokeMessage("RemoveKeysFromCache", r);
                }

                var res = productBl.GetfnPartnerProductSettings(new FilterfnPartnerProductSetting
                {
                    ProductSettingIds = result.Select(x => x.Id).ToList()
                }, true);
                
                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        Entities = res.Entities.Select(x => x.MapTofnPartnerProductSettingModel(identity.TimeZone, productBl.HideAggregatorInfo())).ToList(),
                        res.Count
                    }
                };
            }
        }

        private static ApiResponseBase RemovePartnerProductSettings(ApiPartnerProductSettingInput apiPartnerProductSetting, SessionIdentity identity, ILog log)
        {
            using (var productBl = new ProductBll(identity, log))
            {
                productBl.RemovePartnerProductSettings(apiPartnerProductSetting);
                Parallel.ForEach(apiPartnerProductSetting.ProductIds, productId =>
                {
                    CacheManager.RemovePartnerProductSetting(apiPartnerProductSetting.PartnerId, productId);
                    BaseController.BroadcastCacheChanges(apiPartnerProductSetting.PartnerId, string.Format("{0}_{1}_{2}", Constants.CacheItems.PartnerProductSettings,
                     apiPartnerProductSetting.PartnerId, productId));
                    
                    Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}_{2}", Constants.CacheItems.PartnerProductSettings,
                        apiPartnerProductSetting.PartnerId, productId));
                    Helpers.Helpers.InvokeMessage("RemovePartnerProductSettings", apiPartnerProductSetting.PartnerId, productId);
                });
                var resp = CacheManager.RemovePartnerProductSettingPages(apiPartnerProductSetting.PartnerId);
                return new ApiResponseBase();
            }
        }

        private static ApiResponseBase ExportProducts(ApiFilterfnProduct filter, SessionIdentity identity, ILog log)
        {
            using (var productsBl = new ProductBll(identity, log))
            {
                var products = productsBl.ExportFnProducts(filter.MapToFilterfnProduct())
                                         .Select(x => x.MapTofnProductModel(identity.TimeZone, productsBl.HideAggregatorInfo())).ToList();
                string fileName = "ExportProducts.csv";
                string fileAbsPath = productsBl.ExportToCSV<FnProductModel>(fileName, products, null, null, 0, filter.AdminMenuId);

                var response = new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        ExportedFilePath = fileAbsPath
                    }
                };
                return response;
            }
        }

        private static ApiResponseBase ChangePartnerProductState(PartnerProductSetting partnerProductSetting, SessionIdentity identity, ILog log)
        {
            using (var productsBl = new ProductBll(identity, log))
            {
                var productIds = productsBl.ChangePartnerProductState(partnerProductSetting);

                foreach (var p in productIds)
                {
                    Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}_{2}", Constants.CacheItems.PartnerProductSettings,
                         partnerProductSetting.PartnerId, p));
                }
                return new ApiResponseBase();
            }
        }

        private static ApiResponseBase GetProductCategories(SessionIdentity identity, ILog log)
        {
            using (var productsBl = new ProductBll(identity, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = productsBl.GetProductCategories().Select(x => new ApiProductCategory
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Type = x.Type,
                        TranslationId = x.TranslationId
                    }).OrderBy(x => x.Type).ThenBy(X => X.Name).ToList()
                };
            }
        }

        private static ApiResponseBase SaveProductCategory(ApiProductCategory productCategory, SessionIdentity identity, ILog log)
        {
            using (var productsBl = new ProductBll(identity, log))
            {
                using (var partnerBll = new PartnerBll(productsBl))
                {
                    var result = productsBl.SaveProductCategory(new DAL.ProductCategory
                    {
                        Id = productCategory.Id,
                        Name = productCategory.Name,
                        Type = productCategory.Type
                    });
                    var partners = partnerBll.GetPartners(new FilterPartner(), false).Select(x => x.Id).ToList();
                    partners.ForEach(x =>
                    {
                        var key = string.Format("{0}_{1}", Constants.CacheItems.ClientProductCategories, x);
                        CacheManager.RemoveFromCache(key);
                        Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", key);
                    });

                    return new ApiResponseBase
                    {
                        ResponseObject = new ApiProductCategory
                        {
                            Id = result.Id,
                            Name = result.Name,
                            Type = result.Type,
                            TranslationId = result.TranslationId
                        }
                    };
                }
            }
        }

        public static ApiResponseBase SynchronizeProviderProducts(int gameProviderId, SessionIdentity identity, ILog log)
        {
            using (var productsBl = new ProductBll(identity, log))
            {
                using (var regionBl = new RegionBll(productsBl))
                {
                    using (var partnerBl = new PartnerBll(regionBl))
                    {
                        var gameProvider = CacheManager.GetGameProviderById(gameProviderId);
                        var filter = new FilterfnProduct
                        {
                            ParentId = Constants.PlatformProductId
                        };
                        var productCategories = productsBl.GetFnProducts(filter, true).Entities.Select(x => new { x.Id, x.NickName }).ToList();
                        var providers = productsBl.GetGameProviders(new FilterGameProvider());

                        filter = new FilterfnProduct
                        {
                            Descriptions = new FiltersOperation
                            {
                                IsAnd = true,
                                OperationTypeList = new List<FiltersOperationType>
                                {
                                    new FiltersOperationType
                                    {
                                        OperationTypeId = (int)FilterOperations.IsEqualTo,
                                        StringValue = gameProvider.Name
                                    }
                                }
                            }
                        };
                        var dbCategories = productsBl.GetFnProducts(filter, true).Entities.Select(x => new KeyValuePair<int, int?>(x.Id, x.ParentId)).ToList();
                        var countryCodes = regionBl.GetAllCountryCodes();
                        List<fnProduct> providerGames;
                        switch (gameProvider.Name)
                        {
                            case Constants.GameProviders.IqSoft:
                                var iqSoftCategoryList = new Dictionary<string, int>
                            {
                                { "classic-slots", productCategories.First(x => x.NickName.ToLower() == "slots").Id },
                                { "fish-games", productCategories.First(x => x.NickName.Replace(" ", string.Empty).ToLower() == "fishgames").Id },
                                { "live-casino", productCategories.First(x => x.NickName.Replace(" ", string.Empty).ToLower() == "livegames").Id },
                                { "skill-games", productCategories.First(x => x.NickName.Replace(" ", string.Empty).ToLower() == "skillgames").Id },
                                { "specials", productCategories.First(x => x.NickName.ToLower() == "specials").Id },
                                { "sportsbook", productCategories.First(x => x.NickName.ToLower() == "sports").Id },
                                { "table-games", productCategories.First(x => x.NickName.Replace(" ", string.Empty).ToLower() == "tablegames").Id },
                                { "video-poker", productCategories.First(x => x.NickName.Replace(" ", string.Empty).ToLower() == "videopoker").Id },
                                { "virtual-games", productCategories.First(x => x.NickName.Replace(" ", string.Empty).ToLower() == "virtualgames").Id },
                                { "virtual-sport", productCategories.First(x => x.NickName.Replace(" ", string.Empty).ToLower() == "virtualsports").Id },
                                { "lottery", productCategories.First(x => x.NickName.ToLower() == "lottery").Id },
                            };
                                providerGames = Integration.Products.Helpers.IqSoftHelpers.GetPartnerGames(Constants.MainPartnerId).AsParallel()
                                                .Select(x => x.ToFnProduct(gameProviderId, providers, dbCategories, iqSoftCategoryList)).ToList();

                                break;
                            case Constants.GameProviders.TomHorn:
                                var tomHornCategoryList = new Dictionary<string, int>
                            {
                                { "videoslot", productCategories.FirstOrDefault(x => x.NickName.ToLower() == "slots").Id },
                                { "tableGame", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "tablegames").Id },
                                { "livedealers", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "livegames").Id }
                            };
                                providerGames = Integration.Products.Helpers.TomHornHelpers.GetGamesList(Constants.MainPartnerId).AsParallel()
                                    .Select(x => x.ToFnProduct(gameProviderId, dbCategories, tomHornCategoryList)).ToList();
                                break;
                            case Constants.GameProviders.BlueOcean:
                                var blueOceanCategoryList = new Dictionary<string, int>
                            {
                                { "video-slots", productCategories.First(x => x.NickName.ToLower() == "slots").Id },
                                { "video-slot", productCategories.First(x => x.NickName.ToLower() == "slots").Id },
                                { "livecasino", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "livegames").Id },
                                { "table-games", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "tablegames").Id },
                                { "live-casino-table", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "tablegames").Id },
                                { "live-casino", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "livegames").Id  },
                                { "scratch-cards", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) ==  "scratchgames").Id },
                                { "video-poker", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "tablegames").Id },
                                { "virtual-sports", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "virtualgames").Id },
                                { "video-bingo", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) ==  "skillgames").Id },
                                { "video-slots-lobby", productCategories.First(x => x.NickName.ToLower() == "slots").Id  },
                                { "virtual-games", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "virtualgames").Id },
                                { "poker", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "skillgames").Id },
                                { "tournaments", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "slots").Id }
                            };
                                providerGames = Integration.Products.Helpers.BlueOceanHelpers.GetProductsList(Constants.MainPartnerId).AsParallel().Where(x => x.category.ToLower() != "sportsbook")
                                    .Select(x => x.ToFnProduct(providers, gameProviderId, dbCategories, blueOceanCategoryList)).ToList();
                                break;
                            case Constants.GameProviders.SoftGaming:
                                var softGamingCategoryList = new Dictionary<int, int>
                            {
                                { 16, productCategories.FirstOrDefault(x => x.NickName.ToLower() == "slots").Id },
                                { 37, productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "livegames").Id },
                                { 41, productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "tablegames").Id },
                                { 10, productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "tablegames").Id },
                                { 19, productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) ==  "scratchgames").Id },
                                { 84, productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "livegames").Id },
                                { 13, productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) ==  "tablegames").Id },
                                { 7, productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "tablegames").Id },
                                { 38, productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "tablegames").Id },
                                { 22, productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "slots").Id },
                                { 1366, productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "tablegames").Id },
                                { 3604, productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "virtualgames").Id }
                            };
                                providerGames = Integration.Products.Helpers.SoftGamingHelpers.GetGamePage(Constants.MainPartnerId, out List<MerchantItem> subProviders).
                                  Select(x => x.ToFnProduct(providers, gameProviderId, dbCategories, softGamingCategoryList, subProviders)).ToList();
                                break;
                            case Constants.GameProviders.OutcomeBet:
                            case Constants.GameProviders.Mascot:
                                var categoryList = new Dictionary<string, int>
                            {
                                { "slots", productCategories.First(x => x.NickName.ToLower() == "slots").Id }
                            };
                                providerGames = Integration.Products.Helpers.OutcomeBetHelpers.GetGamesList(Constants.MainPartnerId, gameProvider.Name).AsParallel()
                                    .Select(x => x.ToFnProduct(providers, gameProviderId, dbCategories, categoryList)).ToList();
                                break;
                            case Constants.GameProviders.PragmaticPlay:
                                var pragmaticPlayCategoryList = new Dictionary<string, int>
                                {
                                    { "Video Slots", productCategories.First(x => x.NickName.ToLower() == "slots").Id },
                                    { "Classic Slots", productCategories.First(x => x.NickName.ToLower() == "slots").Id },
                                    { "Blackjack", productCategories.First(x => x.NickName.Replace(" ", string.Empty).ToLower() == "livegames").Id },
                                    { "Baccarat New", productCategories.First(x => x.NickName.Replace(" ", string.Empty).ToLower() == "livegames").Id },
                                    { "Baccarat", productCategories.First(x => x.NickName.Replace(" ", string.Empty).ToLower() == "livegames").Id },
                                    { "Roulette", productCategories.First(x => x.NickName.Replace(" ", string.Empty).ToLower() == "livegames").Id },
                                    { "Live games", productCategories.First(x => x.NickName.Replace(" ", string.Empty).ToLower() == "livegames").Id },
                                    { "Video Poker", productCategories.First(x => x.NickName.Replace(" ", string.Empty).ToLower() == "tablegames").Id },
                                    { "Scratch card", productCategories.First(x => x.NickName.Replace(" ", string.Empty).ToLower() == "scratchgames").Id },
                                    { "RGS - VSB", productCategories.First(x => x.NickName.ToLower() == "slots").Id } // ??
                                };
                                providerGames = Integration.Products.Helpers.PragmaticPlayHelpers.GetProductsList(Constants.MainPartnerId).AsParallel()
                                   .Select(x => x.ToFnProduct(gameProviderId, dbCategories, pragmaticPlayCategoryList)).ToList();
                                break;
                            case Constants.GameProviders.Habanero:
                                var habaneroCategoryList = new Dictionary<string, int>
                            {
                                { "Video Slots", productCategories.First(x => x.NickName.ToLower() == "slots").Id },
                                { "BlackJack", productCategories.First(x => x.NickName.Replace(" ", string.Empty).ToLower() == "tablegames").Id },
                                { "TableGames", productCategories.First(x => x.NickName.Replace(" ", string.Empty).ToLower() == "tablegames").Id },
                                { "Video Poker", productCategories.First(x => x.NickName.Replace(" ", string.Empty).ToLower() == "videopoker").Id },
                                { "War", productCategories.First(x => x.NickName.Replace(" ", string.Empty).ToLower() == "slots").Id },
                                { "Roulette", productCategories.First(x => x.NickName.Replace(" ", string.Empty).ToLower() == "tablegames").Id },
                                { "Gamble", productCategories.First(x => x.NickName.Replace(" ", string.Empty).ToLower() == "tablegames").Id }
                            };
                                providerGames = Integration.Products.Helpers.HabaneroHelpers.GetGames(Constants.MainPartnerId).AsParallel()
                                                .Select(x => x.ToFnProduct(gameProviderId, dbCategories, habaneroCategoryList)).ToList();
                                break;
                            case Constants.GameProviders.BetSoft:
                                var betsoftCategoryList = new Dictionary<string, int>
                            {
                                { "Slots", productCategories.First(x => x.NickName.ToLower() == "slots").Id },
                                { "Table", productCategories.First(x => x.NickName.Replace(" ", string.Empty).ToLower() == "tablegames").Id },
                                { "Video Poker", productCategories.First(x => x.NickName.Replace(" ", string.Empty).ToLower() == "videopoker").Id }
                            };
                                providerGames = Integration.Products.Helpers.BetSoftHelpers.GetGames(Constants.MainPartnerId).AsParallel()
                                                .Select(x => x.ToFnProduct(gameProviderId, dbCategories, betsoftCategoryList)).ToList();

                                break;
                            case Constants.GameProviders.Evoplay:
                                var evoplayCategoryList = new Dictionary<string, int>
                            {
                                { "slots", productCategories.First(x => x.NickName.ToLower() == "slots").Id },
                                { "instant", productCategories.First(x => x.NickName.ToLower() == "slots").Id }, //?
                                { "blackjack", productCategories.First(x => x.NickName.Replace(" ", string.Empty).ToLower() == "tablegames").Id },
                                { "table", productCategories.First(x => x.NickName.Replace(" ", string.Empty).ToLower() == "tablegames").Id },
                                { "baccarat", productCategories.First(x => x.NickName.Replace(" ", string.Empty).ToLower() == "livegames").Id },
                                { "roulette", productCategories.First(x => x.NickName.Replace(" ", string.Empty).ToLower() == "tablegames").Id },
                                { "socketgames", productCategories.First(x => x.NickName.ToLower() == "slots").Id }, // ? 
                                { "poker", productCategories.First(x => x.NickName.Replace(" ", string.Empty).ToLower() == "skillgames").Id },
                            };
                                providerGames = Integration.Products.Helpers.EvoplayHelpers.GetGames(Constants.MainPartnerId).AsParallel()
                                    .Select(x => x.ToFnProduct(gameProviderId, dbCategories, evoplayCategoryList)).ToList();
                                break;

                            case Constants.GameProviders.BetSolutions:
                                var betSolutionsCategoryList = new Dictionary<int, int>
                            {
                                { 2, productCategories.FirstOrDefault(x => x.NickName.ToLower() == "slots").Id },
                                { 1, productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "tablegames").Id },
                                { 3, productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "virtualgames").Id }
                            };
                                providerGames = Integration.Products.Helpers.BetSolutionsHelpers.GetGames(Constants.MainPartnerId).AsParallel()
                                    .Select(x => x.ToFnProduct(gameProviderId, dbCategories, betSolutionsCategoryList)).ToList();
                                break;
                            case Constants.GameProviders.GrooveGaming:
                                var grooveCategoryList = new Dictionary<string, int>
                            {
                                { "Slots", productCategories.FirstOrDefault(x => x.NickName.ToLower() == "slots").Id },
                                { "Win or Crash", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "virtualgames").Id },
                                { "Table & Cards", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "tablegames").Id },
                                { "Video Poker", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "livegames").Id },
                                { "Live Dealer", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "livegames").Id },
                                { "Instant Win", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "slots").Id },
                                { "Video Bingo & Keno", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "virtualgames").Id },
                                { "Virtual Sports", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "virtualgames").Id },
                                { "Multiplayer", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "scratchgames").Id },
                                { "Lottery", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "lottery").Id },
                                { "Scratch Card", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "scratchgames").Id },
                                { "Action Games", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "specials").Id }
                            };
                                providerGames = Integration.Products.Helpers.GrooveHelpers.GetGames(Constants.MainPartnerId).AsParallel()
                                    .Select(x => x.ToFnProduct(providers, gameProviderId, dbCategories, grooveCategoryList)).ToList();
                                break;
                            case Constants.GameProviders.EveryMatrix:
                                var everyMatrixCategoryList = new Dictionary<string, int>
                            {
                                { "VIDEOSLOTS", productCategories.FirstOrDefault(x => x.NickName.ToLower() == "slots").Id },
                                { "TABLEGAMES", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "tablegames").Id },
                                { "SCRATCHCARDS", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ",string.Empty) == "scratchgames").Id },
                                { "VIDEOPOKERS", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "videopoker").Id },
                                { "OTHERGAMES", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "slots").Id },
                                { "JACKPOTGAMES", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "slots").Id },
                                { "BINGO", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "virtualgames").Id },
                                { "CLASSICSLOTS", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "slots").Id },
                                { "VIRTUALSPORTS", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "virtualsports").Id },
                                { "CRASHGAMES", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "specials").Id },
                                { "LIVEDEALER", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "livegames").Id },
                                { "3DSLOTS", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "slots").Id },
                                { "LOTTERY", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "lottery").Id },
                                { "MINIGAMES", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "tablegames").Id }
                            };
                                providerGames = Integration.Products.Helpers.EveryMatrixHelpers.GetGames(Constants.MainPartnerId).AsParallel()
                                    .Select(x => x.ToFnProduct(providers, gameProviderId, dbCategories, everyMatrixCategoryList, countryCodes)).ToList();
                                break;
                            case Constants.GameProviders.Mancala:
                                var mancalaCategory = productCategories.FirstOrDefault(x => x.NickName.ToLower() == "slots").Id;
                                providerGames = Integration.Products.Helpers.MancalaHelpers.GetGames(Constants.MainPartnerId).AsParallel()
                                    .Select(x => x.ToFnProduct(gameProviderId, mancalaCategory)).ToList();
                                break;
                            case Constants.GameProviders.Nucleus:
                                var NucleusCategoryList = new Dictionary<string, int>
                            {
                                { "Slots", productCategories.FirstOrDefault(x => x.NickName.ToLower() == "slots").Id },
                                { "Table", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "tablegames").Id }
                            };
                                providerGames = Integration.Products.Helpers.NucleusHelpers.GetGames(Constants.MainPartnerId).AsParallel()
                                    .Select(x => x.ToFnProduct(gameProviderId, dbCategories, NucleusCategoryList)).ToList();
                                break;
                            case Constants.GameProviders.GoldenRace:
                                var goldenRaceList = new Dictionary<string, int>
                            {
                                { "retail", productCategories.FirstOrDefault(x => x.NickName.ToLower() == "slots").Id },
                                { "virtual", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "virtualgames").Id },
                                { "slot", productCategories.FirstOrDefault(x => x.NickName.ToLower() == "slots").Id },
                                { "live", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "livegames").Id },
                            };
                                providerGames = Integration.Products.Helpers.GoldenRaceHelpers.GetGames(Constants.MainPartnerId).AsParallel()
                                    .Select(x => x.ToFnProduct(gameProviderId, dbCategories, goldenRaceList, providers)).ToList();
                                break;
                            case Constants.GameProviders.DragonGaming:
                                var dragonGamesList = new Dictionary<string, int>
                            {
                                { "slots", productCategories.FirstOrDefault(x => x.NickName.ToLower() == "slots").Id },
                                { "table_games", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "tablegames").Id },
                                { "scratch_cards", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "scratchgames").Id },
                            };
                                providerGames = Integration.Products.Helpers.DragonGamingHelpers.GetGames(Constants.MainPartnerId).AsParallel()
                                     .Select(x => x.ToFnProduct(gameProviderId, dbCategories, dragonGamesList, providers)).ToList();
                                break;
                            case Constants.GameProviders.JackpotGaming:
                                var jackpotGamingGamesList = new Dictionary<string, int>
                            {
                                { "videoslots", productCategories.FirstOrDefault(x => x.NickName.ToLower() == "slots").Id }
                            };
                                providerGames = Integration.Products.Helpers.JackpotGamingHelpers.GetGames(Constants.MainPartnerId).AsParallel()
                                    .Select(x => x.ToFnProduct(gameProviderId, providers, dbCategories, jackpotGamingGamesList)).ToList();
                                break;
                            case Constants.GameProviders.AleaPlay:
                                var aleaPlayGamesList = new Dictionary<string, int>
                            {
                                { "slots", productCategories.FirstOrDefault(x => x.NickName.ToLower() == "slots").Id },
                                { "roulette", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "livegames").Id },
                                { "baccarat", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "livegames").Id },
                                { "blackjack", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "livegames").Id },
                                { "poker", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "livegames").Id },
                                { "show", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "livegames").Id },
                                { "other", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "tablegames").Id },
                                { "dragontiger", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "tablegames").Id },
                                { "craps", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "tablegames").Id },
                                { "sicbo", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "livegames").Id },
                                { "bingo", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "livegames").Id },
                                { "scratchcards", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "scratchgames").Id },
                                { "hilo", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "virtualgames").Id },
                                { "andarbahar", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "livegames").Id },
                                { "keno", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "virtualgames").Id },
                                { "setteemezzo", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "tablegames").Id },
                                { "crash", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "crash").Id },
                                { "instantwin", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "virtualgames").Id },
                                { "wheel", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "virtualgames").Id },
                                { "heads&tails", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "virtualgames").Id },
                                { "ladders", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "slots").Id },
                                { "mine", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "virtualgames").Id },
                                { "hilo", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "virtualgames").Id },
                                { "dice", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "virtualgames").Id },
                                { "shooting", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "virtualgames").Id },
                                { "plinko", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "virtualgames").Id }
                            };
                                providerGames = Integration.Products.Helpers.AleaPlayHelpers.GetGames(Constants.MainPartnerId).AsParallel()
                                                .Select(x => x.ToFnProduct(gameProviderId, dbCategories, aleaPlayGamesList, providers)).ToList();
                                break;
                            case Constants.GameProviders.PlaynGo:
                                var gamesList = new Dictionary<string, int>
                            {
                                { "slots", productCategories.FirstOrDefault(x => x.NickName.ToLower() == "slots").Id },
                                { "table", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "tablegames").Id },
                                { "ro", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "livegames").Id },
                                { "videoslot", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "slots").Id },
                                { "bj", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "livegames").Id },
                                { "fixedodds", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "virtualgames").Id },
                                { "gridslot", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "slots").Id },
                                { "vp", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "videopoker").Id },
                                { "vb", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "virtualgames").Id },
                                { "mw", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "virtualgames").Id }
                            };
                                providerGames = Integration.Products.Helpers.PlaynGoHelpers.GetProductsList(Constants.MainPartnerId).AsParallel()
                                                .Select(x => x.ToFnProduct(gameProviderId, dbCategories, gamesList)).ToList();
                                break;
                            case Constants.GameProviders.SoftSwiss:
                                gamesList = new Dictionary<string, int>
                                {
                                    { "slots", productCategories.FirstOrDefault(x => x.NickName.ToLower() == "slots").Id },
                                    { "roulette", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "tablegames").Id },
                                    { "card", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "tablegames").Id },
                                    { "craps", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "tablegames").Id },
                                    { "poker", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "tablegames").Id },
                                    { "casual", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "virtualgames").Id },
                                    { "lottery", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "lottery").Id },
                                    { "video_poker", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "videopoker").Id },
                                    { "virtual_sports", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "virtualsports").Id },
                                    { "fishing", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "fishgames").Id },
                                    { "crash", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "virtualgames").Id }
                                };
                                providerGames = Integration.Products.Helpers.SoftSwissHelpers.GetGames(Constants.MainPartnerId, log).AsParallel()
                                                .Select(x => x.ToFnProduct(gameProviderId, dbCategories, gamesList, providers)).ToList();
                                break;
                            case Constants.GameProviders.Endorphina:
                                int categoryId = productCategories.FirstOrDefault(x => x.NickName.ToLower() == "slots").Id;
                                var parent = dbCategories.FirstOrDefault(y => y.Value == categoryId);
                                providerGames = Integration.Products.Helpers.EndorphinaHelpers.GetGames(Constants.MainPartnerId, log).AsParallel()
                                                .Select(x => x.ToFnProduct(gameProviderId, parent.Key)).ToList();
                                break;
                            case Constants.GameProviders.RelaxGaming:
                                gamesList = new Dictionary<string, int> // check list
                                {
                                    { "slots", productCategories.FirstOrDefault(x => x.NickName.ToLower() == "slots").Id },
                                };
                                providerGames = Integration.Products.Helpers.RelaxGamingHelpers.GetGames(Constants.MainPartnerId).AsParallel()
                                                .Select(x => x.ToFnProduct(gameProviderId, dbCategories, gamesList, providers)).ToList();
                                break;
                            case Constants.GameProviders.Pixmove:
                                gamesList = new Dictionary<string, int>
                                {
                                    { "slot", productCategories.FirstOrDefault(x => x.NickName.ToLower() == "slots").Id },
                                    { "instant", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "tablegames").Id },
                                    { "table", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "tablegames").Id }
                                };
                                providerGames = Integration.Products.Helpers.PixmoveHelpers.GetGames(Constants.MainPartnerId, identity, log).AsParallel()
                                                .Select(x => x.ToFnProduct(gameProviderId, dbCategories, gamesList)).ToList();
                                break;
                            case Constants.GameProviders.Elite:
                                var eliteGameList = new Dictionary<string, int>
                                {
                                    { "Bingo", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "virtualgames").Id },
                                    { "Table Games", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "tablegames").Id },
                                    { "Slots", productCategories.First(x => x.NickName.ToLower() == "slots").Id },
                                    { "Fishing Games", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "virtualgames").Id },
                                    { "Keno", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "virtualgames").Id },
                                    { "Instant Win", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "virtualgames").Id },
                                    { "Wheel", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "virtualgames").Id },
                                    { "Provably Fair", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "virtualgames").Id },
                                };
                                providerGames = new List<fnProduct>();
                                providerGames = Integration.Products.Helpers.EliteHelpers.GetGames(Constants.MainPartnerId).AsParallel()
                                    .Select(x => x.ToFnProduct(gameProviderId, dbCategories, eliteGameList, providers)).ToList();
                                break;
                            case Constants.GameProviders.SoftLand:
                                var softLandGameList = new Dictionary<string, int>
                            {
                                { "Slots", productCategories.First(x => x.NickName.ToLower() == "slots").Id }
                            };
                                providerGames = new List<fnProduct>();
                                providerGames = Integration.Products.Helpers.SoftLandHelpers.GetGames(Constants.MainPartnerId).AsParallel()
                                    .Select(x => x.ToFnProduct(gameProviderId, dbCategories, softLandGameList, providers)).ToList();
                                break;
                            case Constants.GameProviders.BGGames:
                                var betgamesList = new Dictionary<string, int>
                            {
                                { "Slots", productCategories.First(x => x.NickName.ToLower() == "slots").Id },
                                { "Live casino", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "livegames").Id },
                                { "Live Casino", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "livegames").Id },
                                { "Virtual Sports", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "virtualgames").Id },
                                { "Crash", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "virtualgames").Id },
                                { "", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "virtualgames").Id },
                                { "O", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "virtualgames").Id }
                            };
                                providerGames = new List<fnProduct>();
                                providerGames = Integration.Products.Helpers.BGGamesHelpers.GetGames(Constants.MainPartnerId, log).AsParallel()
                                    .Select(x => x.ToFnProduct(gameProviderId, dbCategories, betgamesList, providers)).ToList();
                                break;
                            case Constants.GameProviders.TimelessTech:
                            case Constants.GameProviders.BCWGames:
                                var tltgamesList = new Dictionary<string, int>
                            {
                                { "casino", productCategories.First(x => x.NickName.ToLower() == "slots").Id },
                                { "live-casino", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "livegames").Id },
                                { "virtual-games", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "virtualgames").Id }
                            };
                                providerGames = new List<fnProduct>();
                                providerGames = Integration.Products.Helpers.TimelessTechHelpers.GetGames(Constants.MainPartnerId, gameProvider.Id).AsParallel()
                                    .Select(x => x.ToFnProduct(gameProviderId, dbCategories, tltgamesList, providers)).ToList();
                                break;
                            case Constants.GameProviders.RiseUp:
                                var riseUpList = new Dictionary<string, int>
                                {
                                    { "videoslot", productCategories.First(x => x.NickName.ToLower() == "slots").Id },
                                    { "slot", productCategories.First(x => x.NickName.ToLower() == "slots").Id },
                                    { "live", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "livegames").Id },
                                    { "livetable", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "livegames").Id },
                                    { "livelobby", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "livegames").Id },
                                    { "lobby", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "livegames").Id },
                                    { "virtualgame", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "virtualgames").Id },
                                    { "virtuallobby", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "virtualgames").Id },
                                    { "poker", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "virtualgames").Id },
                                    { "holdempoker", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "virtualgames").Id },
                                    { "lottery", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "virtualgames").Id },
                                    { "bingo,", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "virtualgames").Id },
                                    { "scratchcard,", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "virtualgames").Id },
                                    { "crashgame", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "virtualgames").Id }
                                };
                                providerGames = new List<fnProduct>();
                                providerGames = Integration.Products.Helpers.RiseUpHelpers.GetGames(Constants.MainPartnerId, gameProvider.Id).AsParallel()
                                    .Select(x => x.ToFnProduct(gameProviderId, dbCategories, riseUpList, providers)).ToList();
                                break;
                            case Constants.GameProviders.LuckyStreak:
                                var luckyStreakList = new Dictionary<string, int>
                            {
                                { "Video Slots", productCategories.First(x => x.NickName.ToLower() == "slots").Id },
                                { "Video Slot", productCategories.First(x => x.NickName.ToLower() == "slots").Id },
                                { "Video slot", productCategories.First(x => x.NickName.ToLower() == "slots").Id },
                                { "video slot", productCategories.First(x => x.NickName.ToLower() == "slots").Id },
                                { "Vide Slot", productCategories.First(x => x.NickName.ToLower() == "slots").Id },
                                { "Slot", productCategories.First(x => x.NickName.ToLower() == "slots").Id },
                                { "slot", productCategories.First(x => x.NickName.ToLower() == "slots").Id },
                                { "Slots", productCategories.First(x => x.NickName.ToLower() == "slots").Id },
                                { "slots", productCategories.First(x => x.NickName.ToLower() == "slots").Id },
                                { "3Oaks", productCategories.First(x => x.NickName.ToLower() == "slots").Id },
                                { "betting", productCategories.First(x => x.NickName.ToLower() == "slots").Id },
                                { "Playson", productCategories.First(x => x.NickName.ToLower() == "slots").Id },
                                { "Ruby Play", productCategories.First(x => x.NickName.ToLower() == "slots").Id },
                                { "Evoplay Entertainment", productCategories.First(x => x.NickName.ToLower() == "slots").Id },
                                { "Classic Slots", productCategories.First(x => x.NickName.ToLower() == "slots").Id },
                                { "Table Games", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "tablegames").Id },
                                { "Table Game", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "tablegames").Id },
                                { "turbogames", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "tablegames").Id },
                                { "dice", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "tablegames").Id },
                                { "casual", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "tablegames").Id },
                                { "lottery", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "tablegames").Id },
                                { "egame", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "tablegames").Id },
                                { "vp", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "tablegames").Id },
                                { "vb", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "tablegames").Id },
                                { "rlt", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "tablegames").Id },
                                { "loki", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "tablegames").Id },
                                { "bj", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "tablegames").Id },
                                { "Video Poker", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "tablegames").Id },
                                { "poker", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "tablegames").Id },
                                { "Scratch card", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "virtualgames").Id },
                                { "card", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "virtualgames").Id },
                                { "Card", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "virtualgames").Id },
                                { "Scratch Card", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "virtualgames").Id },
                                { "Fugaso", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "virtualgames").Id },
                                { "Live games", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "livegames").Id },
                                { "Baccarat", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "livegames").Id },
                                { "Baccarat New", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "livegames").Id },
                                { "Blackjack", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "livegames").Id },
                                { "Roulette", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "livegames").Id },
                                { "roulette", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "livegames").Id },
                                { "AutoRoulette", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "livegames").Id },
                                { "ExternalRoulette", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "livegames").Id },
                                { "ExternalBaccarat", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "livegames").Id }
                            };
                                providerGames = new List<fnProduct>();
                                providerGames = Integration.Products.Helpers.LuckyStreakHelpers.GetGames(Constants.MainPartnerId, gameProvider.Id, log).AsParallel()
                                    .Select(x => x.ToFnProduct(gameProviderId, dbCategories, luckyStreakList, providers)).ToList();
                                break;
							case Constants.GameProviders.ImperiumGames:
								var imperiumGames = new Dictionary<string, int>
							{
								{ "slots", productCategories.First(x => x.NickName.ToLower() == "slots").Id },
								{ "fast_games", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "virtualgames").Id },
								{ "sport", productCategories.First(x => x.NickName.ToLower() == "sports").Id },
								{ "live_dealers", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "livegames").Id },
								{ "card",  productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "tablegames").Id },
								{ "roulette", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "livegames").Id },
								{ "video_poker",  productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "tablegames").Id },
								{ "lottery",  productCategories.First(x => x.NickName.ToLower() == "lottery").Id },
								{ "", productCategories.First(x => x.NickName.ToLower().Replace(" ", string.Empty) == "virtualgames").Id }
							};
								providerGames = new List<fnProduct>();
								providerGames = Integration.Products.Helpers.ImperiumGamesHelpers.GetGames(Constants.MainPartnerId, gameProvider.Id, log).AsParallel()
									.Select(x => x.ToFnProduct(gameProviderId, dbCategories, imperiumGames, providers)).ToList();
								break;
							default:
                                return new ApiResponseBase();
                        }
                        var ids = productsBl.SynchronizeProducts(gameProviderId, providerGames);
                        var partners = partnerBl.GetPartners(new FilterPartner(), false).Where(x => x.State == (int)PartnerStates.Active).Select(x => x.Id).ToList();
                    
                        foreach (var id in ids)
                        {
                            CacheManager.DeleteProductFromCache(id);
                            Helpers.Helpers.InvokeMessage("UpdateProduct", id);
                        }
                        foreach (var p in partners)
                        {
                            CacheManager.RemovePartnerProductSettingPages(p);
                        }
                        return new ApiResponseBase();
                    }
                }
            }
        }
    }
}