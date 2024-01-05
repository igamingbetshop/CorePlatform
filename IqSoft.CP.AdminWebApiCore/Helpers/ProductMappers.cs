using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Models.AdminModels;
using IqSoft.CP.DAL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IqSoft.CP.AdminWebApi.Helpers
{
    public static class ProductMappers
    {
        public static fnProduct ToFnProduct(this Integration.Products.Models.IqSoft.PartnerProductModel input, int gameProviderId,
                                List<GameProvider> providers, List<KeyValuePair<int, int?>> dbCategories, Dictionary<string, int> categoryList)
        {
            var category = categoryList.FirstOrDefault(x => x.Key == (input.CategoryName ?? "slots"));
            string providerName = null;
            if (input.SubproviderId.HasValue)
                providerName = CacheManager.GetGameProviderById(input.SubproviderId.Value)?.Name;
            if(string.IsNullOrEmpty( providerName))
                providerName = input.GameProviderName;
            var subProvider = providers.FirstOrDefault(x => x.Name ==providerName);
            var parent = dbCategories.FirstOrDefault(y => y.Value == category.Value);
            return new fnProduct
            {
                GameProviderId = gameProviderId,
                ParentId = parent.Equals(new KeyValuePair<int, int?>()) ? (int?)null : parent.Key,
                NickName = input.ProductName,
                Name = input.ProductName,
                ExternalId = input.ProductId.ToString(),
                State = (int)ProductStates.Active,
                IsForDesktop = input.IsForDesktop,
                IsForMobile = input.IsForMobile,
                HasDemo = input.HasDemo ?? false,
                FreeSpinSupport = false,
                SubproviderId = subProvider?.Id,
                WebImageUrl = input.WebImageUrl,
                MobileImageUrl = input.MobileImageUrl
            };
        }

        public static fnProduct ToFnProduct(this Integration.Products.Models.BlueOcean.GameItem input, List<GameProvider> providers,
            int gameProviderId, List<KeyValuePair<int, int?>> blueOceanDbCategories, Dictionary<string, int> categoryList)
        {
            var parent = blueOceanDbCategories.FirstOrDefault(y => categoryList.ContainsKey(input.type) && y.Value == categoryList[input.type]);
            int? subProviderId = providers.FirstOrDefault(p => p.Name.ToLower() == input.subcategory.Replace("_", string.Empty)
                                                                                        .Replace(" ", string.Empty)
                                                                                        .Replace("Gameartpremiumgold", "gameart")
                                                                                        .Replace("pragmaticplaylive", "pragmaticplay")
                                                                                        .ToLower()
            || p.Name.ToLower().Contains(input.subcategory.Replace("_", string.Empty).Replace(" ", string.Empty).ToLower())
            || p.Name.ToLower() == input.subcategory.Split('_').First(x => !string.IsNullOrEmpty(x)).ToLower()
            || p.Name.ToLower() == input.subcategory.Replace("_", string.Empty).Replace(" ", string.Empty).ToLower().Replace("gaming", string.Empty)
            )?.Id;
            var nickName = input.name.Replace("\n", string.Empty);
            if (nickName.Length > 50)
                nickName = nickName.Substring(0, 48);

            return new fnProduct
            {
                GameProviderId = gameProviderId,
                ParentId = parent.Equals(new KeyValuePair<int, int?>()) ? (int?)null : parent.Key,
                NickName = nickName,
                Name = input.name.Replace("\n", string.Empty),
                ExternalId = input.id.ToString(),
                State = (int)ProductStates.Active,
                IsForDesktop = !input.mobile,
                IsForMobile = input.mobile,
                SubproviderId = subProviderId,
                HasDemo = input.play_for_fun_supported,
                WebImageUrl = input.image_filled,
                MobileImageUrl = input.image,
                BackgroundImageUrl = input.image_background,
                Jackpot = input.jackpotfeed != null ? JsonConvert.SerializeObject(input.jackpotfeed) : null,
                FreeSpinSupport = input.freerounds_supported
            };
        }

        public static fnProduct ToFnProduct(this Integration.Products.Models.SoftGaming.GameItem input, List<GameProvider> providers,
              int gameProviderId, List<KeyValuePair<int, int?>> dbCategories, Dictionary<int, int> categoryList, List<Integration.Products.Models.SoftGaming.MerchantItem> providerSubCategories)
        {
            var categ = categoryList.FirstOrDefault(x => input.CategoryID.Contains(x.Key));
            var parent = dbCategories.FirstOrDefault(y => y.Value == categ.Value);
            var subProvider = providerSubCategories.FirstOrDefault(x => x.ID == input.MerchantID);
            int? subProviderId = subProvider != null ? providers.FirstOrDefault(p => p.Name.ToLower() == subProvider.Name.ToLower() ||
                                                                                     p.Name.ToLower().Replace(".", string.Empty) == subProvider.Name.ToLower().Replace("gaming", string.Empty) ||
                                                                                     p.Name.ToLower().Replace("gaming", string.Empty) == subProvider.Name.ToLower()
                                                                                     )?.Id : null;
            var nickName = input.Name.en.Replace("\n", string.Empty);
            if (nickName.Length > 50)
                nickName = nickName.Substring(0, 48);

            return new fnProduct
            {
                GameProviderId = gameProviderId,
                ParentId = parent.Equals(new KeyValuePair<int, int?>()) ? (int?)null : parent.Key,
                NickName = nickName,
                Name = input.Name.en,
                ExternalId = string.Format("{0},{1},{2}", input.MerchantID, input.PageCode, !string.IsNullOrEmpty(input.MobilePageCode) ?
                                                          input.MobilePageCode : input.PageCode),
                State = (int)ProductStates.Active,
                IsForDesktop = true,
                IsForMobile = !string.IsNullOrEmpty(input.MobilePageCode),
                SubproviderId = subProviderId,
                HasDemo = input.HasDemo == "1",
                FreeSpinSupport = input.Freeround == "1",
                WebImageUrl = input.ImageFullPath,
                MobileImageUrl = input.ImageFullPath,
                ExternalCategory = JsonConvert.SerializeObject(input.CategoryID)
            };
        }

        public static fnProduct ToFnProduct(this Integration.Products.Models.OutcomeBet.GameItem input, List<GameProvider> providers,
            int gameProviderId, List<KeyValuePair<int, int?>> outcomeBetDbCategories, Dictionary<string, int> categoryList)
        {
            var parent = outcomeBetDbCategories.FirstOrDefault(y => categoryList.ContainsKey(input.Type) && y.Value == categoryList[input.Type]);
            int? subProviderId = providers.FirstOrDefault(p => p.Name.ToLower() == input.SectionId.ToLower())?.Id;
            var nickName = input.Name.Replace("\n", string.Empty);
            if (nickName.Length > 50)
                nickName = nickName.Substring(0, 48);

            return new fnProduct
            {
                GameProviderId = gameProviderId,
                ParentId = parent.Equals(new KeyValuePair<int, int?>()) ? (int?)null : parent.Key,
                NickName = nickName,
                Name = input.Name.Replace("\n", string.Empty),
                ExternalId = input.Id.ToString(),
                State = (int)ProductStates.Active,
                IsForDesktop = true,
                IsForMobile = true,
                SubproviderId = subProviderId.HasValue && subProviderId.Value == gameProviderId ? null : subProviderId,
                HasDemo = true,
                FreeSpinSupport = true
            };
        }

        public static fnProduct ToFnProduct(this Integration.Products.Models.PragmaticPlay.GameItem input,
            int gameProviderId, List<KeyValuePair<int, int?>> pragmaticPlayDbCategories, Dictionary<string, int> categoryList)
        {
            var parent = pragmaticPlayDbCategories.FirstOrDefault(y => categoryList.ContainsKey(input.TypeDescription) && y.Value == categoryList[input.TypeDescription]);
            var nickName = input.GameName.Replace("\n", string.Empty);
            if (nickName.Length > 50)
                nickName = nickName.Substring(0, 48);
            return new fnProduct
            {
                GameProviderId = gameProviderId,
                ParentId = parent.Equals(new KeyValuePair<int, int?>()) ? (int?)null : parent.Key,
                NickName = nickName,
                Name = input.GameName.Replace("\n", string.Empty),
                ExternalId = input.GameId.ToString(),
                State = (int)ProductStates.Active,
                IsForDesktop = input.Platform.Contains("WEB"),
                IsForMobile = input.Platform.Contains("MOBILE"),
                HasDemo = input.DemoGameAvailable,
                FreeSpinSupport = input.FreeRoundAvailable
            };
        }
        public static fnProduct ToFnProduct(this Integration.Products.Models.Habanero.Game input,
           int gameProviderId, List<KeyValuePair<int, int?>> habaneroDbCategories, Dictionary<string, int> categoryList)
        {
            var parent = habaneroDbCategories.FirstOrDefault(y => categoryList.ContainsKey(input.GameTypeName) && y.Value == categoryList[input.GameTypeName]);
            var nickName = input.Name.Replace("\n", string.Empty);
            if (nickName.Length > 50)
                nickName = nickName.Substring(0, 48);
            return new fnProduct
            {
                GameProviderId = gameProviderId,
                ParentId = parent.Equals(new KeyValuePair<int, int?>()) ? (int?)null : parent.Key,
                NickName = nickName,
                Name = input.Name.Replace("\n", string.Empty),
                ExternalId = input.KeyName.ToString(),
                State = (int)ProductStates.Active,
                IsForDesktop = true,
                IsForMobile = input.MobileCapable,
                HasDemo = true,
                FreeSpinSupport = input.SupportBonusFS
            };
        }

        public static fnProduct ToFnProduct(this Integration.Products.Models.Betsoft.GAMESSUITESSUITEGAME input,
          int gameProviderId, List<KeyValuePair<int, int?>> habaneroDbCategories, Dictionary<string, int> categoryList)
        {
            var parent = habaneroDbCategories.FirstOrDefault(y => categoryList.ContainsKey(input.CATEGORYID) && y.Value == categoryList[input.CATEGORYID]);
            var nickName = input.NAME.Replace("\n", string.Empty);
            if (nickName.Length > 50)
                nickName = nickName.Substring(0, 48);
            return new fnProduct
            {
                GameProviderId = gameProviderId,
                ParentId = parent.Equals(new KeyValuePair<int, int?>()) ? (int?)null : parent.Key,
                NickName = nickName,
                Name = input.NAME.Replace("\n", string.Empty),
                ExternalId = input.ID.ToString(),
                State = (int)ProductStates.Active,
                IsForDesktop = true,
                IsForMobile = true,
                HasDemo = true,
                FreeSpinSupport = true
            };
        }

        public static fnProduct ToFnProduct(this KeyValuePair<int, Integration.Products.Models.Evoplay.GameItem> input,
          int gameProviderId, List<KeyValuePair<int, int?>> evoplayDbCategories, Dictionary<string, int> categoryList)
        {
            var parent = evoplayDbCategories.FirstOrDefault(x => categoryList.ContainsKey(input.Value.Type.ToLower()) &&
                                                                 x.Value == categoryList[input.Value.Type.ToLower()]);

            var nickName = input.Value.Name.Replace("\n", string.Empty);
            if (nickName.Length > 50)
                nickName = nickName.Substring(0, 48);
            return new fnProduct
            {
                GameProviderId = gameProviderId,
                ParentId = parent.Equals(new KeyValuePair<int, int?>()) ? (int?)null : parent.Key,
                NickName = nickName,
                Name = input.Value.Name.Replace("\n", string.Empty),
                ExternalId = input.Key.ToString(),
                State = (int)ProductStates.Active,
                IsForDesktop = input.Value.Desktop == 1,
                IsForMobile = input.Value.Mobile == 1,
                HasDemo = true,
                FreeSpinSupport = input.Value.BonusTypes.Contains("freespin_on_start")
            };
        }

        public static fnProduct ToFnProduct(this Integration.Products.Models.BetSolutions.GameItem input,
              int gameProviderId, List<KeyValuePair<int, int?>> dbCategories, Dictionary<int, int> categoryList)
        {
            var categ = categoryList.FirstOrDefault(x =>x.Key == input.ProductId);
            var parent = dbCategories.FirstOrDefault(y => y.Value == categ.Value);

            return new fnProduct
            {
                GameProviderId = gameProviderId,
                ParentId = parent.Equals(new KeyValuePair<int, int?>()) ? (int?)null : parent.Key,
                NickName = input.Name,
                Name = input.Name,
                ExternalId = string.Format("{0},{1}", input.ProductId, input.GameId),
                State = (int)ProductStates.Active,
                IsForDesktop = true,
                IsForMobile =input.HasMobileDeviceSupport,
                HasDemo = true,
                FreeSpinSupport = input.HasFreeplay,
                WebImageUrl = input.Thumbnails.FirstOrDefault(x=>x.Lang== "en-US" && x.Width == 400)?.Url,
                MobileImageUrl = input.Thumbnails.FirstOrDefault(x => x.Lang == "en-US" && x.Width == 400)?.Url
            };
        }

        public static fnProduct ToFnProduct(this Integration.Products.Models.Groove.GameItem input, List<GameProvider> providers,
            int gameProviderId, List<KeyValuePair<int, int?>> dbCategories, Dictionary<string, int> categoryList)
        {
            var categ = categoryList.FirstOrDefault(x => x.Key == input.Category);
            int? subProviderId = providers.FirstOrDefault(p => p.Name.ToLower() == input.SubProvider.ToLower() ||
             p.Name.ToLower() == input.SubProvider.Replace(" ", string.Empty).ToLower() ||
              p.Name.ToLower() == input.SubProvider.ToLower().Replace(" ", string.Empty).Replace("gaming", string.Empty) ||
              p.Name.ToLower() == input.SubProvider.ToLower().Replace(" ", string.Empty).Replace("entertainment", string.Empty) ||
              p.Name.ToLower().Replace("gaming", string.Empty) == input.SubProvider.ToLower() ||
              p.Name.ToLower().Replace("direct", string.Empty) == input.SubProvider.ToLower() ||
              p.Name.ToLower() == input.SubProvider.ToLower().Replace(".", string.Empty)
            )?.Id;
            var parent = dbCategories.FirstOrDefault(y => y.Value == categ.Value);
            var nickName = input.Name.Replace("\n", string.Empty);
            if (nickName.Length > 50)
                nickName = nickName.Substring(0, 48);
            return new fnProduct
            {
                GameProviderId = gameProviderId,

                ParentId = parent.Equals(new KeyValuePair<int, int?>()) ? (int?)null : parent.Key,
                NickName = nickName,
                Name = input.Name,
                ExternalId =input.Id,
                State = (int)ProductStates.Active,
                IsForDesktop = input.Platforms.Any(x=>x.ToLower()=="desktop"),
                IsForMobile = input.Platforms.Any(x => x.ToLower()=="mobile"),
                HasDemo = true,
                FreeSpinSupport = input.SupportFreeBet.ToLower()=="yes",
                WebImageUrl = input.ImageUrl,
                MobileImageUrl = input.ImageUrl,
                SubproviderId = subProviderId
            };
        }

        public static fnProduct ToFnProduct(this Integration.Products.Models.EveryMatrix.GameItem input, List<GameProvider> providers,
          int gameProviderId, List<KeyValuePair<int, int?>> dbCategories, Dictionary<string, int> categoryList, Dictionary<int, string> countryCodes)
        {
            var category = categoryList.FirstOrDefault(x => x.Key == input.data.categories[0]);
            var subProvider1 = input.data.contentProvider;
            var subProvider2 = input.data.vendor;
            int? subProviderId = providers.FirstOrDefault(p => p.Name.ToLower() == subProvider1.ToLower() ||
            p.Name.ToLower().Replace("gaming", string.Empty) == subProvider1.ToLower().Replace("gaming", string.Empty) ||
            p.Name.ToLower().Replace("gambling", string.Empty) == subProvider1.ToLower() ||
            p.Name.ToLower().Replace("direct", string.Empty) == subProvider1.ToLower().Replace("direct", string.Empty) ||
            p.Name.ToLower() == subProvider1.ToLower().Replace("_", string.Empty) ||
            p.Name.ToLower() == subProvider1.ToLower().Replace("games", string.Empty) ||
            p.Name.ToLower() == subProvider1.ToLower().Replace("studio", string.Empty) ||
            p.Name.ToLower() == subProvider1.ToLower().Replace("onextwogaming", "1x2gaming") ||
            p.Name.ToLower().Replace("studios", string.Empty) == subProvider1.ToLower().Replace("studios", string.Empty) ||
            p.Name.ToLower().Replace("new", string.Empty) == subProvider1.ToLower())?.Id;
            if (!subProviderId.HasValue)
            {
                subProviderId = providers.FirstOrDefault(p => p.Name.ToLower() == subProvider2.ToLower() ||
                p.Name.ToLower().Replace("gaming", string.Empty) == subProvider2.ToLower().Replace("gaming", string.Empty) ||
                p.Name.ToLower().Replace("gambling", string.Empty) == subProvider2.ToLower() ||
                p.Name.ToLower().Replace("direct", string.Empty) == subProvider2.ToLower().Replace("direct", string.Empty) ||
                p.Name.ToLower() == subProvider2.ToLower().Replace("_", string.Empty) ||
                p.Name.ToLower() == subProvider2.ToLower().Replace("games", string.Empty) ||
                p.Name.ToLower() == subProvider2.ToLower().Replace("studio", string.Empty) ||
                p.Name.ToLower() == subProvider2.ToLower().Replace("onextwogaming", "1x2gaming") ||
                p.Name.ToLower().Replace("studios", string.Empty) == subProvider2.ToLower().Replace("studios", string.Empty) ||
                p.Name.ToLower().Replace("new", string.Empty) == subProvider2.ToLower())?.Id;
            }
            var parent = dbCategories.FirstOrDefault(y => y.Value == category.Value);

            var name = !string.IsNullOrEmpty(input.data.presentation.gameName?.itemKey) ?
                input.data.presentation.gameName.itemKey.Replace("\n", string.Empty) : input.data.presentation.tableName.itemKey.Replace("\n", string.Empty);
            if (name.Length > 50)
                name = name.Substring(0, 48);
            var nickname = string.Join(",", input.data.categories);
            if (nickname.Length > 50)
                nickname = nickname.Substring(0, 50);

            var externalId = input.data.slug;
            if (!string.IsNullOrEmpty(input.data.presentation.tableName?.itemKey))
                externalId = string.Format("{0},{1}", input.data.slug, input.data.id);
            List<ProductCountrySetting> productCountrySettings = null; 
            if (input.data.restrictedTerritories != null && input.data.restrictedTerritories.Any())
            {
                var countryIds = countryCodes.Where(x => input.data.restrictedTerritories.Contains(x.Value)).Select(x => x.Key).ToList();
                if (countryIds.Any())
                {
                    productCountrySettings = countryCodes.Where(x => input.data.restrictedTerritories.Contains(x.Value))?.Select(x => new ProductCountrySetting
                    {
                        CountryId = x.Key,
                        Type = (int)ProductCountrySettingTypes.Restricted
                    }).ToList();
                }
            }
            return new fnProduct
            {
                GameProviderId = gameProviderId,
                ParentId = parent.Equals(new KeyValuePair<int, int?>()) ? (int?)null : parent.Key,
                NickName = string.Join(",", input.data.categories),
                Name = name,
                ExternalId = externalId,
                State = (int)ProductStates.Active,
                IsForDesktop = input.data.property.terminal.Contains("PC"),
                IsForMobile = input.data.property.terminal.Contains("iPhone")  || input.data.property.terminal.Contains("Android"),
                HasDemo = externalId.Contains("sport") ? true : input.data.playMode.fun,
                FreeSpinSupport = input.data.property.freeSpin?.support,
                WebImageUrl = "https:" + input.data.presentation.thumbnail.itemKey,
                MobileImageUrl = "https:" + input.data.presentation.thumbnail.itemKey,
                BackgroundImageUrl = "https:" + input.data.presentation.backgroundImage.itemKey,
                SubproviderId = subProviderId,
                RTP = input.data.theoreticalPayOut,
                ProductCountrySettings = productCountrySettings
            };
        }

        public static fnProduct ToFnProduct(this Integration.Products.Models.Mancala.GameItem input, int gameProviderId,  int categoryId)
        {
            var nickName = input.Title.Replace("\n", string.Empty);
            if (nickName.Length > 50)
                nickName = nickName.Substring(0, 48);
            var imageUrl = input.Images!= null && input.Images.Any() ? input.Images[0] : string.Empty;
            return new fnProduct
            {
                GameProviderId = gameProviderId,
                ParentId = categoryId,
                NickName = nickName,
                Name = nickName,
                ExternalId = input.Id.ToString(),
                State = (int)ProductStates.Active,
                IsForDesktop = true,
                IsForMobile = true,
                HasDemo = true,
                FreeSpinSupport = true,
                WebImageUrl = imageUrl,
                MobileImageUrl = imageUrl,
                BackgroundImageUrl = string.Empty
            };
        }

        public static fnProduct ToFnProduct(this Integration.Products.Models.Nucleus.GAMESSUITESSUITEGAME input, int gameProviderId,
                                            List<KeyValuePair<int, int?>> dbCategories, Dictionary<string, int> categoryList)
        {
            var category = categoryList.FirstOrDefault(x => x.Key == input.CATEGORYNAME);
            var parent = dbCategories.FirstOrDefault(y => y.Value == category.Value);
            var nickName = input.NAME.Replace("\n", string.Empty);
            if (nickName.Length > 50)
                nickName = nickName.Substring(0, 48);
            return new fnProduct
            {
                GameProviderId = gameProviderId,

                ParentId = parent.Equals(new KeyValuePair<int, int?>()) ? (int?)null : parent.Key,
                NickName = nickName,
                Name = nickName,
                ExternalId = input.ID,
                State = (int)ProductStates.Active,
                IsForDesktop = true,
                IsForMobile = true,
                HasDemo = true,
                FreeSpinSupport = true,
                WebImageUrl = string.Empty,
                MobileImageUrl = string.Empty,
                BackgroundImageUrl = string.Empty
            };
        }

        public static fnProduct ToFnProduct(this Integration.Products.Models.GoldenRace.GameItem input, int gameProviderId,
                                           List<KeyValuePair<int, int?>> dbCategories, Dictionary<string, int> categoryList, List<GameProvider> providers)
        {
            var category = categoryList.FirstOrDefault(x => x.Key == input.Type);
            var parent = dbCategories.FirstOrDefault(y => y.Value == category.Value);
            int? subProviderId = providers.FirstOrDefault(p => p.Name.ToLower() == input.Provider.ToLower() ||
                                                               p.Name.ToLower().Replace("gaming", string.Empty) == input.Provider.ToLower()
                                                                                                                   .Replace("gaming", string.Empty))?.Id;


           var nickName = input.GameTitle.Replace("\n", string.Empty);
            if (nickName.Length > 50)
                nickName = nickName.Substring(0, 48);
            return new fnProduct
            {
                GameProviderId = gameProviderId,
                ParentId = parent.Equals(new KeyValuePair<int, int?>()) ? (int?)null : parent.Key,
                NickName = nickName,
                SubproviderId = subProviderId,
                Name = nickName,
                ExternalId = input.GameId,
                State = (int)ProductStates.Active,
                IsForDesktop = input.Platforms.Contains("desktop"),
                IsForMobile = input.Platforms.Contains("mobile"),
                HasDemo = true, 
                FreeSpinSupport = true,
                WebImageUrl = input.Thumbnail,
                MobileImageUrl = input.Thumbnail,
                BackgroundImageUrl = input.Thumbnail
            };
        }

        public static fnProduct ToFnProduct(this Integration.Products.Models.DragonGaming.GameProperty input, int gameProviderId,
                                         List<KeyValuePair<int, int?>> dbCategories, Dictionary<string, int> categoryList, List<GameProvider> providers)
        {
            var category = categoryList.FirstOrDefault(x => x.Key == input.Category);
            var parent = dbCategories.FirstOrDefault(y => y.Value == category.Value);
            int? subProviderId = providers.FirstOrDefault(p => p.Name.ToLower() == input.Supplier.ToLower() ||
                                                               p.Name.ToLower().Replace("gaming", string.Empty) == input.Supplier.ToLower()
                                                                                                                   .Replace("gaming", string.Empty))?.Id;
            var nickName = input.GameTitle.Replace("\n", string.Empty);
            if (nickName.Length > 50)
                nickName = nickName.Substring(0, 48);
            return new fnProduct
            {
                GameProviderId = gameProviderId,
                ParentId = parent.Equals(new KeyValuePair<int, int?>()) ? (int?)null : parent.Key,
                NickName = nickName,
                SubproviderId = subProviderId,
                Name = nickName,
                ExternalId = string.Format("{0},{1}", input.GameId, input.Category),
                State = (int)ProductStates.Active,
                IsForDesktop = true,
                IsForMobile = true,
                HasDemo = true,
                FreeSpinSupport = false,
                WebImageUrl = input.Logos.FirstOrDefault()?.Url,
                MobileImageUrl = input.Logos.FirstOrDefault()?.Url,
                BackgroundImageUrl = input.Logos.FirstOrDefault()?.Url
            };
        }

        public static fnProduct ToFnProduct(this Integration.Products.Models.Tomhorn.GameModule input, int gameProviderId,
                                            List<KeyValuePair<int, int?>> dbCategories, Dictionary<string, int> categoryList)
        {
            var category = categoryList.FirstOrDefault(x => x.Key == input.Type.Replace(" ", string.Empty).ToLower());
            var subProvider = input.Provider.ToLower().Replace("gaming", string.Empty).Replace(" ", string.Empty);
            var parent = dbCategories.FirstOrDefault(y => y.Value == category.Value);
            var nickName = input.Name.Replace("\n", string.Empty);
            if (nickName.Length > 50)
                nickName = nickName.Substring(0, 48);
            return new fnProduct
            {
                GameProviderId = gameProviderId,
                ParentId = parent.Equals(new KeyValuePair<int, int?>()) ? (int?)null : parent.Key,
                NickName = nickName,
                Name = nickName,
                ExternalId =input.Key,
                State = (int)ProductStates.Active,
                IsForDesktop = true,
                IsForMobile = true,
                HasDemo = true,
                FreeSpinSupport = false,
                SubproviderId = Convert.ToInt32(input.Provider)
            };
        }

        public static fnProduct ToFnProduct(this Integration.Products.Models.JackpotGaming.GameItem input, int gameProviderId,
                                            List<GameProvider> providers, List<KeyValuePair<int, int?>> dbCategories, Dictionary<string, int> categoryList)
        {
            var category = categoryList.FirstOrDefault(x => x.Key == input.Categories[0].Name.Replace(" ", string.Empty).ToLower());
            var parent = dbCategories.FirstOrDefault(y => y.Value == category.Value);

            var nickName = input.GameName;
            if (nickName.Length > 50)
                nickName = nickName.Substring(0, 48);
            int? subProviderId = providers.FirstOrDefault(p => p.Name.ToLower() == input.BrandName.ToLower() ||
                                                               p.Name.ToLower().Replace("gaming", string.Empty) == input.BrandName.ToLower()
                                                                                                       .Replace("gaming", string.Empty))?.Id;

            return new fnProduct
            {
                GameProviderId = gameProviderId,
                ParentId = parent.Equals(new KeyValuePair<int, int?>()) ? (int?)null : parent.Key,
                NickName = nickName,
                Name = nickName,
                SubproviderId = subProviderId,
                ExternalId = input.GameId,
                State = (int)ProductStates.Active,
                IsForDesktop = input.IsDesktop,
                IsForMobile = input.IsMobile,
                HasDemo = false,
                FreeSpinSupport = false,
                WebImageUrl = input.ImageUrl,
                MobileImageUrl = input.ImageUrl
            };
        }

        public static fnProduct ToFnProduct(this Integration.Products.Models.AleaPlay.Game input, int gameProviderId,
                                        List<KeyValuePair<int, int?>> dbCategories, Dictionary<string, int> categoryList, List<GameProvider> providers)
        {
            var category = categoryList.FirstOrDefault(x => x.Key == input.Categories[0].Name.Replace(" ", string.Empty).ToLower());
            var parent = dbCategories.FirstOrDefault(y => y.Value == category.Value);
            var subProvider = input.Software.Name.Replace(" ", string.Empty).ToLower().Replace("salsatechnology", "salsa").Replace("play'ngo", "playngo");
            int? subProviderId = providers.FirstOrDefault(p => p.Name.Replace(" ", string.Empty).ToLower() == subProvider ||
                                                               p.Name.Replace(" ", string.Empty).ToLower().Replace("gaming", string.Empty).Replace("new", string.Empty)
                                                               .Replace("direct", string.Empty) == subProvider.Replace("gaming", string.Empty))?.Id;
            var nickName = input.Name;
            if (nickName.Length > 50)
                nickName = nickName.Substring(0, 48);
            return new fnProduct
            {
                GameProviderId = gameProviderId,
                ParentId = parent.Equals(new KeyValuePair<int, int?>()) ? (int?)null : parent.Key,
                NickName = nickName,
                SubproviderId = subProviderId,
                Name = nickName,
                ExternalId = input.Id,
                State = (int)ProductStates.Active,
                IsForDesktop = input.Configurations != null && input.Configurations.Any(x => x.Device == "ALL" || x.Device == "DESKTOP"),
                IsForMobile = input.Configurations != null && input.Configurations.Any(x => x.Device == "ALL" || x.Device == "MOBILE"),
                HasDemo = true,
                FreeSpinSupport = !(input.Configurations == null) || input.Configurations[0].Attributes.Any(x => x.Name=="freeSpinAllowed"),
                //WebImageUrl = input.Logos.FirstOrDefault()?.Url,
                //MobileImageUrl = input.Logos.FirstOrDefault()?.Url,
                //BackgroundImageUrl = input.Logos.FirstOrDefault()?.Url
            };
        }
    }
}