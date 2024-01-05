using System.Linq;
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
using Microsoft.AspNetCore.Hosting;

namespace IqSoft.CP.AdminWebApi.ControllerClasses
{
    public static class ProductController
    {
        internal static ApiResponseBase CallFunction(RequestBase request, SessionIdentity identity, ILog log, IWebHostEnvironment env)
        {
            switch (request.Method)
            {
                case "GetProducts":
                    return GetProducts(JsonConvert.DeserializeObject<ApiFilterfnProduct>(request.RequestData), identity, log);
                case "GetGameProviders":
                    return GetGameProviders(JsonConvert.DeserializeObject<ApiFilterGameProvider>(request.RequestData),
                        identity, log);
                case "SaveGameProvider":
                    return SaveGameProvider(JsonConvert.DeserializeObject<ApiGameProvider>(request.RequestData), identity, log);
                case "GetProductDetails":
                    return GetProductDetails(JsonConvert.DeserializeObject<int>(request.RequestData), identity, log);
                case "AddProduct":
                    return AddProduct(JsonConvert.DeserializeObject<ApiProduct>(request.RequestData), identity, log);
                case "EditProduct":
                    return EditProduct(JsonConvert.DeserializeObject<ApiProduct>(request.RequestData), identity, log);
                case "GetPartnerProducts":
                    return
                        GetPartnerProducts(
                            JsonConvert.DeserializeObject<ApiFilterPartnerProductSetting>(request.RequestData), identity, log);
                case "ExportPartnerProducts":
                    return
                        ExportPartnerProducts(
                            JsonConvert.DeserializeObject<ApiFilterPartnerProductSetting>(request.RequestData), identity, log, env);
                case "GetPartnerProductSettings":
                    return
                        GetPartnerProductSettings(
                            JsonConvert.DeserializeObject<ApiFilterPartnerProductSetting>(request.RequestData), identity, log);
                case "ExportPartnerProductSettings":
                    return
                        ExportfnPartnerProductSettings(
                            JsonConvert.DeserializeObject<ApiFilterPartnerProductSetting>(request.RequestData), identity, log, env);
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
                    return ExportProducts(JsonConvert.DeserializeObject<ApiFilterfnProduct>(request.RequestData), identity, log, env);
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

        public static ApiResponseBase GetProducts(ApiFilterfnProduct filter, SessionIdentity identity, ILog log)
        {
            using (var productsBl = new ProductBll(identity, log))
            {
                using (var userBl = new UserBll(productsBl))
                {
                    var user = userBl.GetUserById(identity.Id);
                    if (user == null)
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.UserNotFound);
                    var products = productsBl.GetFnProducts(filter.MapToFilterfnProduct(), !(user.Type == (int)UserTypes.MasterAgent ||
                        user.Type == (int)UserTypes.Agent || user.Type == (int)UserTypes.AgentEmployee));

                    var entities = products.Entities.Select(x => x.MapTofnProductModel(identity.TimeZone)).ToList();
                    if (entities.All(x => x.GameProviderId == null))
                        entities = entities.OrderBy(x => x.Name).ToList();

                    return new ApiResponseBase
                    {
                        ResponseObject = new
                        {
                            products.Count,
                            Entities = entities
                        }
                    };
                }
            }
        }

        private static ApiResponseBase GetGameProviders(ApiFilterGameProvider filter, SessionIdentity identity, ILog log)
        {
            using (var productsBl = new ProductBll(identity, log))
            {
                var providers = productsBl.GetGameProviders(filter.MapToFilterGameProvider());
                return new ApiResponseBase
                {
                    ResponseObject = providers.Select(x => new ApiGameProvider
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Type = x.Type,
                        SessionExpireTime = x.SessionExpireTime,
                        GameLaunchUrl = x.GameLaunchUrl
                    }).ToList()
                };
            }
        }

        private static ApiResponseBase SaveGameProvider(ApiGameProvider input, SessionIdentity identity, ILog log)
        {
            using (var productsBl = new ProductBll(identity, log))
            {
                var provider = productsBl.SaveGameProvider(new GameProvider
                {
                    Id = input.Id,
                    Type = input.Type ?? 0,
                    SessionExpireTime = input.SessionExpireTime,
                    Name = input.Name,
                    GameLaunchUrl = input.GameLaunchUrl
                });
                Helpers.Helpers.InvokeMessage(string.Format("{0}_{1}", Constants.CacheItems.GameProviders, input.Id));
                Helpers.Helpers.InvokeMessage(string.Format("{0}_{1}", Constants.CacheItems.GameProviders, input.Name));
                return new ApiResponseBase
                {
                    ResponseObject = new ApiGameProvider
                    {
                        Id = provider.Id,
                        Name = provider.Name,
                        Type = provider.Type,
                        SessionExpireTime = provider.SessionExpireTime,
                        GameLaunchUrl = provider.GameLaunchUrl
                    }
                };
            }
        }

        private static ApiResponseBase GetProductDetails(int productId, SessionIdentity identity, ILog log)
        {
            using var productsBl = new ProductBll(identity, log);
            return new ApiResponseBase
            {
                ResponseObject = productsBl.GetfnProductById(productId, true).MapTofnProductModel(identity.TimeZone)
            };
        }

        private static ApiResponseBase AddProduct(ApiProduct product, SessionIdentity identity, ILog log)
        {
            using (var productsBl = new ProductBll(identity, log))
            {
                var input = product.MapTofnProduct();
                input.IsNewObject = true;
                var res = productsBl.SaveProduct(input, string.Empty, out _);
                Helpers.Helpers.InvokeMessage("UpdateProduct", res.Id);
                return new ApiResponseBase
                {
                    ResponseObject = productsBl.GetfnProductById(res.Id, true).MapTofnProductModel(identity.TimeZone)
                };
            }
        }

        private static ApiResponseBase EditProduct(ApiProduct product, SessionIdentity identity, ILog log)
        {
            if (product.Countries != null && !Enum.IsDefined(typeof(BonusSettingConditionTypes), product.Countries.Type))
                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongInputParameters);
            using (var productsBl = new ProductBll(identity, log))
            {
                var input = product.MapTofnProduct();
                input.IsNewObject = false;
                var res = productsBl.SaveProduct(input, product.Comment, out List<int> partners);
                Helpers.Helpers.InvokeMessage("UpdateProduct", res.Id);
                foreach (var partnerId in partners)
                    Helpers.Helpers.InvokeMessage("RemovePartnerProductSettings", partnerId);
                return new ApiResponseBase
                {
                    ResponseObject = productsBl.GetfnProductById(res.Id, true).MapTofnProductModel(identity.TimeZone)
                };
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
                        Entities = products.Entities.Select(x => x.MapTofnProductModel(identity.TimeZone)).ToList(),
                        products.Count
                    }
                };
            }
        }

        private static ApiResponseBase GetPartnerProductSettings(ApiFilterPartnerProductSetting input, SessionIdentity identity, ILog log)
        {
            using (var productBl = new ProductBll(identity, log))
            {
                var partnerProducts = productBl.GetfnPartnerProductSettings(input.MapTofnPartnerProductSettings(), true);

                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        Entities = partnerProducts.Entities.Select(x => x.MapTofnPartnerProductSettingModel(identity.TimeZone)).ToList(),
                        partnerProducts.Count
                    }
                };
            }
        }

        private static ApiResponseBase ExportfnPartnerProductSettings(ApiFilterPartnerProductSetting filter, SessionIdentity identity, ILog log, IWebHostEnvironment env)
        {
            using (var productBl = new ProductBll(identity, log))
            {
                var partnerSettings = productBl.ExportfnPartnerProductSettings(filter.MapTofnPartnerProductSettings()).Entities.ToList();
                string fileName = "ExportPartnerProductSettings.csv";
                string fileAbsPath = productBl.ExportToCSV<fnPartnerProductSetting>(fileName, partnerSettings, null, null, identity.TimeZone, env);

                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        ExportedFilePath = fileAbsPath
                    }
                };
            }
        }

        private static ApiResponseBase ExportPartnerProducts(ApiFilterPartnerProductSetting filter, SessionIdentity identity, ILog log, IWebHostEnvironment env)
        {
            using (var productBl = new ProductBll(identity, log))
            {
                var partnerProducts = productBl.ExportPartnerProducts(filter.PartnerId, filter.MapTofnProductSettings()).Entities.ToList();
                string fileName = "ExportPartnerProducts.csv";
                string fileAbsPath = productBl.ExportToCSV<fnProduct>(fileName, partnerProducts, null, null, identity.TimeZone, env);

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
                        Entities = partnerProducs.Entities.Select(x => x.MapTofnPartnerProductSettingModel(identity.TimeZone)).ToList(),
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
                foreach (var productId in apiPartnerProductSetting.ProductIds)
                {
                    BaseController.BroadcastCacheChanges(apiPartnerProductSetting.PartnerId, string.Format("{0}_{1}_{2}", Constants.CacheItems.PartnerProductSettings,
                     apiPartnerProductSetting.PartnerId, productId));
                    Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}_{2}", Constants.CacheItems.PartnerProductSettings, apiPartnerProductSetting.PartnerId, productId));
                    Helpers.Helpers.InvokeMessage("RemovePartnerProductSettings", apiPartnerProductSetting.PartnerId, productId);
                }
                var key = string.Format("{0}_{1}", Constants.CacheItems.ClientProductCategories, apiPartnerProductSetting.PartnerId);
                CacheManager.RemoveFromCache(key);
                Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", key);

                var res = productBl.GetfnPartnerProductSettings(new FilterfnPartnerProductSetting
                {
                    ProductSettingIds = result.Select(x => x.Id).ToList()
                }, true);
                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        Entities = res.Entities.Select(x => x.MapTofnPartnerProductSettingModel(identity.TimeZone)).ToList(),
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
                return new ApiResponseBase();
            }
        }

        private static ApiResponseBase ExportProducts(ApiFilterfnProduct filter, SessionIdentity identity, ILog log, IWebHostEnvironment env)
        {
            using (var productsBl = new ProductBll(identity, log))
            {
                var products = productsBl.ExportFnProducts(filter.MapToFilterfnProduct()).Select(x => x.MapTofnProductModel(identity.TimeZone)).ToList();
                string fileName = "ExportProducts.csv";
                string fileAbsPath = productsBl.ExportToCSV<FnProductModel>(fileName, products, null, null, 0, env);

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
            using var productsBl = new ProductBll(identity, log);
            using var regionBl = new RegionBll(productsBl);
            var gameProvider = CacheManager.GetGameProviderById(gameProviderId);
            var filter = new FilterfnProduct
            {
                ParentId = Constants.PlatformProductId
            };
            var productCategories = productsBl.GetFnProducts(filter, true).Entities.Select(x => new { x.Id, x.NickName }).ToList();
            var countryCodes = regionBl.GetAllCountryCodes();
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
            List<fnProduct> providerGames;
            switch (gameProvider.Name)
            {
                case Constants.GameProviders.IqSoft:
                    var iqSoftCategoryList = new Dictionary<string, int>
                        {
                            { "classic-slots", productCategories.First(x => x.NickName.ToLower() == "slots").Id },
                            { "fish-games", productCategories.First(x => x.NickName.Replace(" ", "").ToLower() == "fishgames").Id },
                            { "live-casino", productCategories.First(x => x.NickName.Replace(" ", "").ToLower() == "livegames").Id },
                            { "skill-games", productCategories.First(x => x.NickName.Replace(" ", "").ToLower() == "skillgames").Id },
                            { "specials", productCategories.First(x => x.NickName.ToLower() == "specials").Id },
                            { "sportsbook", productCategories.First(x => x.NickName.ToLower() == "sports").Id },
                            { "table-games", productCategories.First(x => x.NickName.Replace(" ", "").ToLower() == "tablegames").Id },
                            { "video-poker", productCategories.First(x => x.NickName.Replace(" ", "").ToLower() == "videopoker").Id },
                            { "virtual-games", productCategories.First(x => x.NickName.Replace(" ", "").ToLower() == "virtualgames").Id },
                            { "virtual-sport", productCategories.First(x => x.NickName.Replace(" ", "").ToLower() == "virtualsports").Id },
                            { "lottery", productCategories.First(x => x.NickName.ToLower() == "lottery").Id },
                        };
                    providerGames = Integration.Products.Helpers.IqSoftHelpers.GetPartnerGames(Constants.MainPartnerId).AsParallel()
                                    .Select(x => x.ToFnProduct(gameProviderId, providers, dbCategories, iqSoftCategoryList)).ToList();

                    break;
                case Constants.GameProviders.BlueOcean:
                    var blueOceanCategoryList = new Dictionary<string, int>
                        {
                            { "video-slots", productCategories.First(x => x.NickName.ToLower() == "slots").Id },
                            { "video-slot", productCategories.First(x => x.NickName.ToLower() == "slots").Id },
                            { "livecasino", productCategories.First(x => x.NickName.ToLower().Replace(" ", "") == "livegames").Id },
                            { "table-games", productCategories.First(x => x.NickName.ToLower().Replace(" ", "") == "tablegames").Id },
                            { "live-casino-table", productCategories.First(x => x.NickName.ToLower().Replace(" ", "") == "tablegames").Id },
                            { "live-casino", productCategories.First(x => x.NickName.ToLower().Replace(" ", "") == "livegames").Id  },
                            { "scratch-cards", productCategories.First(x => x.NickName.ToLower().Replace(" ", "") ==  "scratchgames").Id },
                            { "video-poker", productCategories.First(x => x.NickName.ToLower().Replace(" ", "") == "tablegames").Id },
                            { "virtual-sports", productCategories.First(x => x.NickName.ToLower().Replace(" ", "") == "virtualgames").Id },
                            { "video-bingo", productCategories.First(x => x.NickName.ToLower().Replace(" ", "") ==  "skillgames").Id },
                            { "video-slots-lobby", productCategories.First(x => x.NickName.ToLower() == "slots").Id  },
                            { "virtual-games", productCategories.First(x => x.NickName.ToLower().Replace(" ", "") == "virtualgames").Id },
                            { "poker", productCategories.First(x => x.NickName.ToLower().Replace(" ", "") == "skillgames").Id },
                            { "tournaments", productCategories.First(x => x.NickName.ToLower().Replace(" ", "") == "slots").Id }
                        };
                    providerGames = Integration.Products.Helpers.BlueOceanHelpers.GetProductsList(Constants.MainPartnerId).AsParallel().Where(x => x.category.ToLower() != "sportsbook")
                        .Select(x => x.ToFnProduct(providers, gameProviderId, dbCategories, blueOceanCategoryList)).ToList();
                    break;
                case Constants.GameProviders.SoftGaming:
                    var softGamingCategoryList = new Dictionary<int, int>
                        {
                            { 16, productCategories.FirstOrDefault(x => x.NickName.ToLower() == "slots").Id },
                            { 37, productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", "") == "livegames").Id },
                            { 41, productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", "") == "tablegames").Id },
                            { 10, productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", "") == "tablegames").Id },
                            { 19, productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", "") ==  "scratchgames").Id },
                            { 84, productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", "") == "livegames").Id },
                            { 13, productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", "") ==  "tablegames").Id },
                            { 7, productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", "") == "tablegames").Id },
                            { 38, productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", "") == "tablegames").Id }
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
                            { "Blackjack", productCategories.First(x => x.NickName.Replace(" ", "").ToLower() == "livegames").Id },
                            { "Baccarat New", productCategories.First(x => x.NickName.Replace(" ", "").ToLower() == "livegames").Id },
                            { "Baccarat", productCategories.First(x => x.NickName.Replace(" ", "").ToLower() == "livegames").Id },
                            { "Roulette", productCategories.First(x => x.NickName.Replace(" ", "").ToLower() == "livegames").Id },
                            { "Video Poker", productCategories.First(x => x.NickName.Replace(" ", "").ToLower() == "tablegames").Id },
                            { "Scratch card", productCategories.First(x => x.NickName.Replace(" ", "").ToLower() == "scratchgames").Id },
                            { "RGS - VSB", productCategories.First(x => x.NickName.ToLower() == "slots").Id } // ??
                        };
                    providerGames = Integration.Products.Helpers.PragmaticPlayHelpers.GetProductsList(Constants.MainPartnerId).AsParallel()
                       .Select(x => x.ToFnProduct(gameProviderId, dbCategories, pragmaticPlayCategoryList)).ToList();
                    break;
                case Constants.GameProviders.Habanero:
                    var habaneroCategoryList = new Dictionary<string, int>
                        {
                            { "Video Slots", productCategories.First(x => x.NickName.ToLower() == "slots").Id },
                            { "BlackJack", productCategories.First(x => x.NickName.Replace(" ", "").ToLower() == "tablegames").Id },
                            { "TableGames", productCategories.First(x => x.NickName.Replace(" ", "").ToLower() == "tablegames").Id },
                            { "Video Poker", productCategories.First(x => x.NickName.Replace(" ", "").ToLower() == "videopoker").Id },
                            { "War", productCategories.First(x => x.NickName.Replace(" ", "").ToLower() == "slots").Id },
                            { "Roulette", productCategories.First(x => x.NickName.Replace(" ", "").ToLower() == "tablegames").Id },
                            { "Gamble", productCategories.First(x => x.NickName.Replace(" ", "").ToLower() == "tablegames").Id }
                        };
                    providerGames = Integration.Products.Helpers.HabaneroHelpers.GetGames(Constants.MainPartnerId).AsParallel()
                                    .Select(x => x.ToFnProduct(gameProviderId, dbCategories, habaneroCategoryList)).ToList();
                    break;
                case Constants.GameProviders.BetSoft:
                    var betsoftCategoryList = new Dictionary<string, int>
                        {
                            { "Slots", productCategories.First(x => x.NickName.ToLower() == "slots").Id },
                            { "Table", productCategories.First(x => x.NickName.Replace(" ", "").ToLower() == "tablegames").Id },
                            { "Video Poker", productCategories.First(x => x.NickName.Replace(" ", "").ToLower() == "videopoker").Id }
                        };
                    providerGames = Integration.Products.Helpers.BetSoftHelpers.GetGames(Constants.MainPartnerId).AsParallel()
                                    .Select(x => x.ToFnProduct(gameProviderId, dbCategories, betsoftCategoryList)).ToList();

                    break;
                case Constants.GameProviders.Evoplay:
                    var evoplayCategoryList = new Dictionary<string, int>
                        {
                            { "slots", productCategories.First(x => x.NickName.ToLower() == "slots").Id },
                            { "instant", productCategories.First(x => x.NickName.ToLower() == "slots").Id }, //?
                            { "blackjack", productCategories.First(x => x.NickName.Replace(" ", "").ToLower() == "tablegames").Id },
                            { "table", productCategories.First(x => x.NickName.Replace(" ", "").ToLower() == "tablegames").Id },
                            { "baccarat", productCategories.First(x => x.NickName.Replace(" ", "").ToLower() == "livegames").Id },
                            { "roulette", productCategories.First(x => x.NickName.Replace(" ", "").ToLower() == "tablegames").Id },
                            { "socketgames", productCategories.First(x => x.NickName.ToLower() == "slots").Id }, // ? 
                            { "poker", productCategories.First(x => x.NickName.Replace(" ", "").ToLower() == "skillgames").Id },
                        };
                    providerGames = Integration.Products.Helpers.EvoplayHelpers.GetGames(Constants.MainPartnerId).AsParallel()
                        .Select(x => x.ToFnProduct(gameProviderId, dbCategories, evoplayCategoryList)).ToList();
                    break;

                case Constants.GameProviders.BetSolutions:
                    var betSolutionsCategoryList = new Dictionary<int, int>
                        {
                            { 2, productCategories.FirstOrDefault(x => x.NickName.ToLower() == "slots").Id },
                            { 1, productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", "") == "tablegames").Id },
                            { 3, productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", "") == "virtualgames").Id }
                        };
                    providerGames = Integration.Products.Helpers.BetSolutionsHelpers.GetGames(Constants.MainPartnerId).AsParallel()
                        .Select(x => x.ToFnProduct(gameProviderId, dbCategories, betSolutionsCategoryList)).ToList();
                    break;
                case Constants.GameProviders.GrooveGaming:
                    var grooveCategoryList = new Dictionary<string, int>
                        {
                            { "Slots", productCategories.FirstOrDefault(x => x.NickName.ToLower() == "slots").Id },
                            { "Win or Crash", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", "") == "virtualgames").Id },
                            { "Table & Cards", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", "") == "tablegames").Id },
                            { "Video Poker", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", "") == "livegames").Id },
                            { "Live Dealer", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", "") == "livegames").Id },
                            { "Instant Win", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", "") == "slots").Id },
                            { "Video Bingo & Keno", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", "") == "virtualgames").Id },
                            { "Virtual Sports", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", "") == "virtualgames").Id },
                            { "Multiplayer", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", "") == "scratchgames").Id }
                        };
                    providerGames = Integration.Products.Helpers.GrooveHelpers.GetGames(Constants.MainPartnerId).AsParallel()
                                    .Select(x => x.ToFnProduct(providers, gameProviderId, dbCategories, grooveCategoryList)).ToList();
                    break;
                case Constants.GameProviders.EveryMatrix:
                    var everyMatrixCategoryList = new Dictionary<string, int>
                        {
                           { "VIDEOSLOTS", productCategories.FirstOrDefault(x => x.NickName.ToLower() == "slots").Id },
                            { "TABLEGAMES", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", "") == "tablegames").Id },
                            { "SCRATCHCARDS", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", "") == "scratchgames").Id },
                            { "VIDEOPOKERS", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", "") == "videopoker").Id },
                            { "OTHERGAMES", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", "") == "slots").Id },
                            { "JACKPOTGAMES", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", "") == "slots").Id },
                            { "BINGO", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", "") == "virtualgames").Id },
                            { "CLASSICSLOTS", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", "") == "slots").Id },
                            { "VIRTUALSPORTS", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", "") == "virtualsports").Id },
                            { "CRASHGAMES", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", "") == "minigames").Id },
                            { "LIVEDEALER", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", "") == "livegames").Id },
                            { "3DSLOTS", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", "") == "slots").Id },
                            { "LOTTERY", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", "") == "lottery").Id },
                            { "MINIGAMES", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", "") == "tablegames").Id }
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
                            { "Table", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", "") == "tablegames").Id }
                        };
                    providerGames = Integration.Products.Helpers.NucleusHelpers.GetGames(Constants.MainPartnerId).AsParallel()
                        .Select(x => x.ToFnProduct(gameProviderId, dbCategories, NucleusCategoryList)).ToList();
                    break;
                case Constants.GameProviders.GoldenRace:
                    var goldenRaceList = new Dictionary<string, int>
                        {
                            { "retail", productCategories.FirstOrDefault(x => x.NickName.ToLower() == "slots").Id },
                            { "virtual", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", "") == "virtualgames").Id },
                            { "slot", productCategories.FirstOrDefault(x => x.NickName.ToLower() == "slots").Id },
                            { "live", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", "") == "livegames").Id },
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
                case Constants.GameProviders.TomHorn:
                    var tomHornCategoryList = new Dictionary<string, int>
                        {
                            { "videoslot", productCategories.FirstOrDefault(x => x.NickName.ToLower() == "slots").Id },
                            { "tableGame", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", "") == "tablegames").Id },
                            { "livedealers", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", "") == "livegames").Id }
                        };
                    providerGames = Integration.Products.Helpers.TomHornHelpers.GetGamesList(Constants.MainPartnerId).AsParallel()
                        .Select(x => x.ToFnProduct(gameProviderId, dbCategories, tomHornCategoryList)).ToList();
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
                        { "setteemezzo", productCategories.FirstOrDefault(x => x.NickName.ToLower().Replace(" ", string.Empty) == "tablegames").Id }
                    };
                    providerGames = Integration.Products.Helpers.AleaPlayHelpers.GetGames(Constants.MainPartnerId).AsParallel()
                                    .Select(x => x.ToFnProduct(gameProviderId, dbCategories, aleaPlayGamesList, providers)).ToList();
                    break;
                default:
                    return new ApiResponseBase();
            }
            var ids = productsBl.SynchronizeProducts(gameProviderId, providerGames);
            foreach (var id in ids)
            {
                CacheManager.UpdateProductById(id);
                Helpers.Helpers.InvokeMessage("UpdateProduct", id);
            }
            return new ApiResponseBase();
        }
    }
}