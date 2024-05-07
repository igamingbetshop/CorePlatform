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
            var subProvider = providerSubCategories.FirstOrDefault(x => x.ID == input.SubMerchantID || x.ID == input.MerchantID);
            int? subProviderId = subProvider != null ? providers.FirstOrDefault(p => p.Name.ToLower() == subProvider.Name.ToLower() ||
                                                                                     p.Name.ToLower().Replace(".", string.Empty).Replace(" ", string.Empty).Replace("gaming", string.Empty) ==
                                                                                     subProvider.Name.ToLower().Replace("gaming", string.Empty) ||
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
                RTP = input.RTP.HasValue && input.RTP.Value <= 100 ? input.RTP : null,
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
            if (parent.Equals(new KeyValuePair<int, int?>()))
                WebApiApplication.DbLogger.Info("MissingCategoryId_" + input.Category);

            var nickName = input.Name.Replace("\n", string.Empty);
            if (nickName.Length > 50)
                nickName = nickName.Substring(0, 48);
            decimal rtp = 0m;
            if (!string.IsNullOrEmpty(input.DefaultRtp))
                decimal.TryParse(input.DefaultRtp.EndsWith("%") ? input.DefaultRtp.Replace("%", string.Empty) : input.DefaultRtp , out rtp);
            if (rtp > 100)
                rtp = 0m;
            return new fnProduct
            {
                GameProviderId = gameProviderId,
                ParentId = parent.Equals(new KeyValuePair<int, int?>()) ? (int?)null : parent.Key,
                NickName = nickName,
                Name = input.Name,
                ExternalId = input.Id,
                State = (int)ProductStates.Active,
                IsForDesktop = input.Platforms.Any(x => x.ToLower()=="desktop"),
                IsForMobile = input.Platforms.Any(x => x.ToLower()=="mobile"),
                HasDemo = true,
                RTP = rtp != 0 ? rtp : (decimal?)null,
                FreeSpinSupport = input.SupportFreeBet.ToLower()=="yes",
                WebImageUrl = input.ImageUrl,
                MobileImageUrl = input.ImageUrl,
                SubproviderId = subProviderId
            };
        }

        public static fnProduct ToFnProduct(this Integration.Products.Models.EveryMatrix.GameItem input, List<GameProvider> providers,
          int gameProviderId, List<KeyValuePair<int, int?>> dbCategories, Dictionary<string, int> categoryList, Dictionary<int, string> countryCodes)
        {
            var category = categoryList.FirstOrDefault(x => x.Key == input.Data.Categories[0]);
            var subProvider1 = input.Data.ContentProvider;
            var subProvider2 = input.Data.Vendor;
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

            var name = !string.IsNullOrEmpty(input.Data.Presentation.GameName?.ItemKey) ?
                input.Data.Presentation.GameName.ItemKey.Replace("\n", string.Empty) : input.Data.Presentation.TableName.ItemKey.Replace("\n", string.Empty);
            if (name.Length > 50)
                name = name.Substring(0, 48);
            var nickname = string.Join(",", input.Data.Categories);
            if (nickname.Length > 50)
                nickname = nickname.Substring(0, 50);

            var externalId = input.Data.Slug;
            if (!string.IsNullOrEmpty(input.Data.Presentation.TableName?.ItemKey))
                externalId = string.Format("{0},{1},{2}", input.Data.Slug, input.Data.Id, input.Data.GameCode);
            else
                externalId = string.Format("{0},{1}", input.Data.Slug, input.Data.GameCode);
            List<ProductCountrySetting> productCountrySettings = null; 
            if (input.Data.RestrictedTerritories != null && input.Data.RestrictedTerritories.Any())
            {
                var countryIds = countryCodes.Where(x => input.Data.RestrictedTerritories.Contains(x.Value)).Select(x => x.Key).ToList();
                if (countryIds.Any())
                {
                    productCountrySettings = countryCodes.Where(x => input.Data.RestrictedTerritories.Contains(x.Value))?.Select(x => new ProductCountrySetting
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
                NickName = string.Join(",", input.Data.Categories),
                Name = name,
                ExternalId = externalId,
                State = (int)ProductStates.Active,
                IsForDesktop = input.Data.Property.Terminal.Contains("PC"),
                IsForMobile = input.Data.Property.Terminal.Contains("iPhone")  || input.Data.Property.Terminal.Contains("Android"),
                HasDemo = externalId.Contains("sport") || input.Data.PlayMode.Fun,
                FreeSpinSupport = input.Data.Property.FreeSpin?.Support,
                Lines = input.Data.Property.FreeSpin?.Lines?.Selections != null ?  
                        JsonConvert.SerializeObject(input.Data.Property.FreeSpin?.Lines?.Selections) : null,
                BetValues = input.Data.Property.FreeSpin?.BetValues?.Selections != null ?
                            JsonConvert.SerializeObject(input.Data.Property.FreeSpin?.BetValues?.Selections) : null,
                WebImageUrl = "https:" + input.Data.Presentation.Thumbnail.ItemKey,
                MobileImageUrl = "https:" + input.Data.Presentation.Thumbnail.ItemKey,
                BackgroundImageUrl = "https:" + input.Data.Presentation.BackgroundImage.ItemKey,
                SubproviderId = subProviderId,
                RTP = input.Data.TheoreticalPayOut,
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
            var category = categoryList.FirstOrDefault(x => x.Key == input.Genre.Replace(" ", string.Empty).ToLower());
            var parent = dbCategories.FirstOrDefault(y => y.Value == category.Value);
            var subProvider = input.Software.Name.Replace(" ", string.Empty).ToLower().Replace("salsatechnology", "salsa")
                                                                                      .Replace("play'ngo", "playngo")
                                                                                      .Replace("pragmaticplaylive", "pragmaticlive");
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
                NickName = input.Genre,
                SubproviderId = subProviderId,
                Name = nickName,
                ExternalId = input.Id,
                State = (int)ProductStates.Active,
                IsForDesktop = true,
                IsForMobile = true,
                HasDemo = true,
                FreeSpinSupport = input.FreeSpinsCurrencies != null && input.FreeSpinsCurrencies.Any(),
                RTP = input.RTP,
                Lines = input.Lines,
                MobileImageUrl = input.ThumbnailLinks?.RATIO_4_3,
                WebImageUrl = input.ThumbnailLinks?.RATIO_4_3
            };
        }

        public static fnProduct ToFnProduct(this Integration.Products.Models.PlaynGo.GameItem input, int gameProviderId,
                                        List<KeyValuePair<int, int?>> dbCategories, Dictionary<string, int> categoryList)
        {
            var category = categoryList.FirstOrDefault(x => x.Key == input.GameType);
            var parent = dbCategories.FirstOrDefault(y => y.Value == category.Value);
            var nickName = input.Name;
            if (nickName.Length > 50)
                nickName = nickName.Substring(0, 48);
            return new fnProduct
            {
                GameProviderId = gameProviderId,
                ParentId = parent.Equals(new KeyValuePair<int, int?>()) ? (int?)null : parent.Key,
                NickName = nickName,
                SubproviderId = gameProviderId,
                Name = nickName,
                ExternalId =$"{input.GameId}-{input.GId}",
                State = (int)ProductStates.Active,
                IsForDesktop = true,
                IsForMobile = true,
                HasDemo = true
                //FreeSpinSupport = true                
            };
        }

        public static fnProduct ToFnProduct(this Integration.Products.Models.SoftSwiss.YamlItem input, int gameProviderId,List<KeyValuePair<int, int?>> dbCategories,
                                            Dictionary<string, int> categoryList, List<GameProvider> gameProviders)
        {
            var category = categoryList.FirstOrDefault(x => x.Key == input.Category);
            var parent = dbCategories.FirstOrDefault(y => y.Value == category.Value);
            var subProvider = gameProviders.FirstOrDefault(y => y.Name.ToLower() == input.Producer || 
            y.Name.ToLower().Replace("gaming", string.Empty).Replace("direct", string.Empty).Replace("new", string.Empty)
                            .Replace(".", string.Empty).Replace("games", string.Empty).Replace("studios", string.Empty)
                            .Replace("atmosphera", "atmosfera").Replace("eagames", "eagaming")
                            .Replace("pragmaticplay", "pragmatic").Replace("absolutelivegaming", "alg")
                            .Replace("spearheadstudios", "spearhead").Replace("nolimitcity", "nolimit")
                            .Replace("rtcbingo", "bingo") == input.Producer.Replace("games", string.Empty).Replace("irondogstudio", "irondog")
                            .Replace("bsg", "betsoft").Replace("gaming", string.Empty).Replace(" ", string.Empty).Replace("pragmaticplaylive", "pragmatic")) ?? 
                            gameProviders.FirstOrDefault(y => y.Name.ToLower()==input.Provider ||  y.Name.ToLower().Replace("gaming", string.Empty)
                            .Replace("direct", string.Empty).Replace("new", string.Empty)
                            .Replace(".", string.Empty).Replace("games", string.Empty).Replace("studios", string.Empty)
                            .Replace("atmosphera", "atmosfera").Replace("eagames", "eagaming")
                            .Replace("pragmaticplay", "pragmatic") == input.Provider.Replace("games", string.Empty).Replace("pragmaticplaylive", "pragmatic"));
            var nickName = input.Title;
            if (nickName.Length > 50)
                nickName = nickName.Substring(0, 48);
            return new fnProduct
            {
                GameProviderId = gameProviderId,
                SubproviderId = subProvider?.Id,
                ParentId = parent.Equals(new KeyValuePair<int, int?>()) ? (int?)null : parent.Key,
                NickName = nickName,
                Name = nickName,
                ExternalId = input.Identifier,
                State = (int)ProductStates.Active,
                IsForDesktop = input.IsDesktop,
                IsForMobile = input.IsMobile,
                HasDemo = true,
                FreeSpinSupport = input.HasFreespins,
                RTP = input.Payout,
                Jackpot = input.FeatureGroup,
                WebImageUrl = $"https://cdn.softswiss.net/i/s3/{input.Identifier.Split(':')[0]}/{input.Identifier.Split(':')[1]}.png",
                MobileImageUrl = $"https://cdn.softswiss.net/i/s3/{input.Identifier.Split(':')[0]}/{input.Identifier.Split(':')[1]}.png"
            };
        }


        public static fnProduct ToFnProduct(this Integration.Products.Models.Elite.Game input, int gameProviderId,
                                         List<KeyValuePair<int, int?>> dbCategories, Dictionary<string, int> categoryList, List<GameProvider> providers)
        {
            var category = categoryList.FirstOrDefault(x => x.Key == input.Category);
            var parent = dbCategories.FirstOrDefault(y => y.Value == category.Value);          
            var subProvider = input.GameVendor.Replace("_RGS", string.Empty).ToLower();
            int? subProviderId = providers.FirstOrDefault(p => p.Name.Replace("Games", string.Empty).Replace("Play", string.Empty).Replace("gaming", string.Empty).Replace("Slots", string.Empty).ToLower() == subProvider.Replace("games", string.Empty))?.Id;
            var nickName = input.GameDescription;
            if (nickName.Length > 50)
                nickName = nickName.Substring(0, 48);
            return new fnProduct
            {
                GameProviderId = gameProviderId,
                ParentId = parent.Equals(new KeyValuePair<int, int?>()) ? (int?)null : parent.Key,
                NickName = nickName,
                SubproviderId = subProviderId,
                Name = nickName,
                ExternalId = input.GameID,
                State = (int)ProductStates.Active,
                WebImageUrl = input.ImageUrl,
                MobileImageUrl = input.ImageUrl,
                IsForDesktop = true,
                IsForMobile = true,
                HasDemo = true
            };
        }

        public static fnProduct ToFnProduct(this Integration.Products.Models.BGGames.Game input, int gameProviderId,
                                         List<KeyValuePair<int, int?>> dbCategories, Dictionary<string, int> categoryList, List<GameProvider> providers)
        {
            var category = categoryList.FirstOrDefault(x => x.Key == input.category);
            var parent = dbCategories.FirstOrDefault(y => y.Value == category.Value);
			var subProvider = providers.FirstOrDefault(p => p.Name.ToLower() == input.provider.ToLower().Replace(" ", "") ||
															p.Name.ToLower().Replace("playsondirect", "playson") == input.provider.ToLower() ||
															p.Name.ToLower().Replace("gambling", string.Empty).Replace("play", string.Empty)
                                                         == input.provider.ToLower().Replace(" ", "").Replace("gaming", string.Empty)
                                                                                                     .Replace("games", string.Empty)
                                                                                                     .Replace("studio", string.Empty)
                                                                                                     .Replace("pragmaticplay(livedealer)", "pragmatic")
                                                                                                     .Replace("ezugi(e)","ezugi"));
            var nickName = input.name;
            if (nickName.Length > 50)
                nickName = nickName.Substring(0, 48);
            return new fnProduct
            {
                GameProviderId = gameProviderId,
                ParentId = parent.Equals(new KeyValuePair<int, int?>()) ? (int?)null : parent.Key,
                NickName = nickName,
                SubproviderId = subProvider?.Id,
                Name = input.name,
                ExternalId = input.ID,
                State = (int)ProductStates.Active,
                WebImageUrl = input.thumbnail,
                MobileImageUrl = input.thumbnail,
                IsForDesktop = input.desktop,
                IsForMobile = input.mobile,
                HasDemo = input.has_demo == "Y"
            };
        }

        public static fnProduct ToFnProduct(this Integration.Products.Models.TimelessTech.Game input, int gameProviderId,
                                         List<KeyValuePair<int, int?>> dbCategories, Dictionary<string, int> categoryList, List<GameProvider> providers)
        {
            var category = categoryList.FirstOrDefault(x => x.Key == input.subtype);
            var parent = dbCategories.FirstOrDefault(y => y.Value == category.Value);
            var subProvider = providers.FirstOrDefault(p => p.Name.ToLower().Replace("livegaming", string.Empty).Replace("direc", string.Empty) == input.vendor.ToLower().Replace("-", string.Empty).Replace(" ", string.Empty) ||
                                                            p.Name.ToLower().Replace("gaming", string.Empty).Replace("games", string.Empty) == input.vendor.ToLower().Replace("gaming", string.Empty) ||
															p.Name.Length > 5 && input.vendor.Length > 5 && p.Name.ToLower().Substring(0, 6) == input.vendor.ToLower().Replace("-", string.Empty).Replace(" ", string.Empty).Substring(0, 6));
            var nickName = input.title;
            if (nickName.Length > 50)
                nickName = nickName.Substring(0, 48);
            return new fnProduct
            {
                GameProviderId = gameProviderId,
                ParentId = parent.Equals(new KeyValuePair<int, int?>()) ? (int?)null : parent.Key,
                NickName = nickName,
                SubproviderId = subProvider?.Id,
                Name = input.title,
                ExternalId = input.title.Contains("lobby") ? input.title : input.id.ToString(),
                State = (int)ProductStates.Active,
                WebImageUrl = input.details?.thumbnails._450x345jpg,
				MobileImageUrl = input.details?.thumbnails._450x345jpg,
				IsForDesktop = true,
                IsForMobile = true,
                HasDemo = input.fun_mode == 1,
                BetValues = input.betValue,
                RTP = input.details?.rtp,
                Volatility = input.details?.volatility,
                Lines = input.details?.tags != null ? string.Join(",", input.details?.tags) : null,
                FreeSpinSupport = (input.campaigns.HasValue && input.campaigns.Value == 1)
			};
        }

        public static fnProduct ToFnProduct(this Integration.Products.Models.RiseUp.Product input, int gameProviderId,
                                         List<KeyValuePair<int, int?>> dbCategories, Dictionary<string, int> categoryList, List<GameProvider> providers)
        {
            var category = categoryList.FirstOrDefault(x => x.Key == input.type);
            var parent = dbCategories.FirstOrDefault(y => y.Value == category.Value);
            var subProvider = providers.FirstOrDefault(p => p.Name.ToLower().Replace("gaming", string.Empty).Replace("games", string.Empty)
                                                                            .Replace("game", string.Empty).Replace("direct", string.Empty) == 
                                                    input.provider.ToLower().Replace(" ", "").Replace("gaming", string.Empty).Replace("games", string.Empty)
													                        .Replace("slots", string.Empty).Replace("casinotechnology", "technology")
                                                                            .Replace("pragmaticplaylive", "pragmaticlive").Replace("'", string.Empty));      
            var nickName = input.name;
            if (nickName.Length > 50)
                nickName = nickName.Substring(0, 48);
			return new fnProduct
            {
                GameProviderId = gameProviderId,
                ParentId = parent.Equals(new KeyValuePair<int, int?>()) ? (int?)null : parent.Key,
                NickName = nickName,
                SubproviderId = subProvider?.Id,
                Name = input.name,
                ExternalId = $"{input.id},{input.provider.Replace(" ", "/")}",
                State = (int)ProductStates.Active,
                WebImageUrl = input.webImageUrl,
				MobileImageUrl = input.mobileImgUrl,
				IsForDesktop = true,
                IsForMobile = true,
                HasDemo = input.type == "Premium Slots"
			};
        }

        public static fnProduct ToFnProduct(this Integration.Products.Models.SoftLand.Game input, int gameProviderId,
                                         List<KeyValuePair<int, int?>> dbCategories, Dictionary<string, int> categoryList, List<GameProvider> providers)
        {
            var category = categoryList.FirstOrDefault(x => x.Key == input.CategoryNames.First());
            var parent = dbCategories.FirstOrDefault(y => y.Value == category.Value);          
            var subProvider = input.Provider.Name.ToLower();
            int? subProviderId = providers.FirstOrDefault(p => p.Name.Replace("Games", string.Empty).Replace("Play", string.Empty).Replace("gaming", string.Empty).Replace("Slots", string.Empty).ToLower() == subProvider.Replace("games", string.Empty))?.Id;
            var nickName = input.Description;
            if (nickName.Length > 50)
                nickName = nickName.Substring(0, 48);
            return new fnProduct
            {
                GameProviderId = gameProviderId,
                ParentId = parent.Equals(new KeyValuePair<int, int?>()) ? (int?)null : parent.Key,
                NickName = nickName,
                SubproviderId = subProviderId,
                Name = input.Name,
                ExternalId = input.Id.ToString(),
                State = (int)ProductStates.Active,
                WebImageUrl = input.LogoPath,
                MobileImageUrl = input.BackgroundPath,
                IsForDesktop = true,
                IsForMobile = true,
                HasDemo = input.HasDemoMode
            };
        }


        public static fnProduct ToFnProduct(this Integration.Products.Models.LuckyStreak.Product input, int gameProviderId,
                                         List<KeyValuePair<int, int?>> dbCategories, Dictionary<string, int> categoryList, List<GameProvider> providers)
        {
            var category = categoryList.FirstOrDefault(x => x.Key == input.type);
            var parent = dbCategories.FirstOrDefault(y => y.Value == category.Value);
            
            var subProvider =  providers.FirstOrDefault(p => p.Name == input.provider);
            var nickName = input.name;
            if (nickName.Length > 50)
                nickName = nickName.Substring(0, 48);
			return new fnProduct
            {
                GameProviderId = gameProviderId,
                ParentId = parent.Equals(new KeyValuePair<int, int?>()) ? (int?)null : parent.Key,
                NickName = nickName,
                SubproviderId = subProvider?.Id,
                Name = input.name,
                ExternalId = input.externalId,
                State = (int)ProductStates.Active,
                WebImageUrl = input.imageUrl,
                MobileImageUrl = input.imageUrl,
                IsForDesktop = true,
                IsForMobile = true,
                HasDemo = input.demoUrl != null
            };
        }
    }
}