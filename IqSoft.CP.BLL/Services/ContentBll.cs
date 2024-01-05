using System.Data.Entity;
using System.Collections.Generic;
using System.Linq;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Interfaces;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using log4net;
using IqSoft.CP.DAL.Models.WebSite;
using System;
using Newtonsoft.Json;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.BLL.Helpers;
using IqSoft.CP.DAL.Filters;
using IqSoft.CP.Common.Models;
using System.Data.Entity.Validation;
using IqSoft.CP.BLL.Models;
using IqSoft.CP.DAL.Models.Segment;
using System.Threading.Tasks;
using IqSoft.CP.Common.Models.AdminModels;
using IqSoft.CP.Common.Helpers;

namespace IqSoft.CP.BLL.Services
{
    public class ContentBll : PermissionBll, IContentBll
    {
        #region Constructors

        public ContentBll(SessionIdentity identity, ILog log)
            : base(identity, log)
        {

        }

        public ContentBll(BaseBll baseBl)
            : base(baseBl)
        {

        }

        #endregion

        #region Helpers 

        public DAL.Banner SaveWebSiteBanner(DAL.Banner input)
        {
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.SaveBanner,
                ObjectTypeId = ObjectTypes.Banner
            });
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            if (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleObjects.All(x => x != input.PartnerId))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);

            var dbBanner = Db.Banners.FirstOrDefault(x => x.Id == input.Id);
            if (dbBanner != null)
            {
                var imageSizes = dbBanner.Image.Split(',').ToList();
                var version = imageSizes[0].Split('?');
                dbBanner.IsEnabled = input.IsEnabled;
                dbBanner.Image = version.Length > 0 ? version[0] + "?ver=" + ((version.Length > 1 ? Convert.ToInt32(version[1].Replace("ver=", string.Empty)) : 0) + 1) : string.Empty;
                imageSizes[0] = dbBanner.Image;
                if (!string.IsNullOrEmpty(input.ImageSize))
                    imageSizes.Add(input.ImageSize);
                dbBanner.Image = string.Join(",", imageSizes);
                dbBanner.Order = input.Order;
                dbBanner.StartDate = input.StartDate;
                dbBanner.EndDate = input.EndDate;
                dbBanner.Type = input.Type;
                dbBanner.Link = input.Link;
                dbBanner.Visibility = input.Visibility;
                dbBanner.ShowDescription = input.ShowDescription;
                dbBanner.ButtonType = input.ButtonType;
                
                if (input.BannerSegmentSettings == null || !input.BannerSegmentSettings.Any())
                    Db.BannerSegmentSettings.Where(x => x.BannerId == dbBanner.Id).DeleteFromQuery();
                else
                {
                    var type = input.BannerSegmentSettings.First().Type;
                    var segments = input.BannerSegmentSettings.Select(x => x.SegmentId).ToList();
                    Db.BannerSegmentSettings.Where(x => x.BannerId == dbBanner.Id && (x.Type != type || !segments.Contains(x.SegmentId))).DeleteFromQuery();
                    var dbSegments = Db.BannerSegmentSettings.Where(x => x.BannerId == dbBanner.Id).Select(x => x.SegmentId).ToList();
                    segments.RemoveAll(x => dbSegments.Contains(x));
                    foreach (var s in segments)
                        Db.BannerSegmentSettings.Add(new BannerSegmentSetting { BannerId = dbBanner.Id, SegmentId = s, Type = type });
                }
                if (input.BannerLanguageSettings == null || !input.BannerLanguageSettings.Any())
                    Db.BannerLanguageSettings.Where(x => x.BannerId == dbBanner.Id).DeleteFromQuery();
                else
                {
                    var type = input.BannerLanguageSettings.First().Type;
                    var languages = input.BannerLanguageSettings.Select(x => x.LanguageId).ToList();
                    Db.BannerLanguageSettings.Where(x => x.BannerId == dbBanner.Id && (x.Type != type || !languages.Contains(x.LanguageId))).DeleteFromQuery();
                    var dbLanguages = Db.BannerLanguageSettings.Where(x => x.BannerId == dbBanner.Id).Select(x => x.LanguageId).ToList();
                    languages.RemoveAll(x => dbLanguages.Contains(x));
                    foreach (var l in languages)
                        Db.BannerLanguageSettings.Add(new BannerLanguageSetting { BannerId = dbBanner.Id, LanguageId = l, Type = type });
                }

                Db.SaveChanges();
            }
            else
            {
                dbBanner = input.Copy();
                if (!Enum.GetNames(typeof(BannerExtensions)).Select(x => { return x.ToLower(); }).Contains(input.Image.ToLower()))
                    throw CreateException(LanguageId, Constants.Errors.WrongFileExtension);

                dbBanner.Translation = CreateTranslation(new fnTranslation
                {
                    ObjectTypeId = (int)ObjectTypes.Banner,
                    Text = string.IsNullOrEmpty(dbBanner.Body) ? string.Empty : dbBanner.Body,
                    LanguageId = Constants.DefaultLanguageId
                });
                dbBanner.Translation1 = CreateTranslation(new fnTranslation
                {
                    ObjectTypeId = (int)ObjectTypes.Banner,
                    Text = string.IsNullOrEmpty(dbBanner.Head) ? string.Empty : dbBanner.Head,
                    LanguageId = Constants.DefaultLanguageId
                });
                dbBanner.Head = string.Empty;
                dbBanner.Body = string.Empty;
                if (string.IsNullOrEmpty(dbBanner.Link))
                    dbBanner.Link = string.Empty;
                Db.Banners.Add(dbBanner);
                Db.SaveChanges();
                foreach (var bss in input.BannerSegmentSettings)
                {
                    bss.BannerId = dbBanner.Id;
                    Db.BannerSegmentSettings.Add(bss);
                }
                foreach (var bls in input.BannerLanguageSettings)
                {
                    bls.BannerId = dbBanner.Id;
                    Db.BannerLanguageSettings.Add(bls);
                }
                dbBanner.Head = $"Banner_{dbBanner.Id}_Head";
                dbBanner.Body = $"Banner_{dbBanner.Id}_Body";
                if (!string.IsNullOrEmpty(dbBanner.Image))
                    dbBanner.Image = dbBanner.Id.ToString() + "." + dbBanner.Image.ToLower() + "?ver=1";
                Db.SaveChanges();
            }

            return dbBanner;
        }

        public void RemoveWebSiteBanner(int bannerId, out int partnerId, out int type)
        {
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.SaveBanner,
                ObjectTypeId = ObjectTypes.Banner
            });
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            var dbBanner = Db.Banners.FirstOrDefault(x => x.Id == bannerId);
            if (dbBanner == null)
                throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);
            partnerId = dbBanner.PartnerId;
            type = dbBanner.Type;
            if (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleObjects.All(x => x != dbBanner.PartnerId))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            Db.BannerSegmentSettings.Where(x => x.BannerId == bannerId).DeleteFromQuery();
            Db.BannerLanguageSettings.Where(x => x.BannerId == bannerId).DeleteFromQuery();
            Db.Banners.Where(x => x.Id == bannerId).DeleteFromQuery();
            Db.SaveChanges();
        }

        public PagedModel<fnBanner> GetBanners(int? partnerId, int? id, int skipCount, int takeCount)
        {
            var result = Db.fn_Banner(Identity.LanguageId).AsQueryable();
            if (partnerId != null)
                result = result.Where(x => x.PartnerId == partnerId);
            if (id != null)
                result = result.Where(x => x.Id == id);

            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewBanner,
                ObjectTypeId = ObjectTypes.Banner
            });

            var partners = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });

            if (!partners.HaveAccessForAllObjects)
                result = result.Where(x => partners.AccessibleObjects.Contains(x.PartnerId));

            var entities = result.OrderByDescending(x => x.Id).Skip(skipCount * takeCount).Take(takeCount).ToList();
            if (entities.Any())
            {
                var smiFragments = Db.WebSiteSubMenuItems.Include(x => x.WebSiteMenuItem)
                                  .Where(x => x.WebSiteMenuItem.WebSiteMenu.Type == Constants.WebSiteConfiguration.Config &&
                                             (x.WebSiteMenuItem.Title == Constants.WebSiteConfiguration.WebFragments ||
                                              x.WebSiteMenuItem.Title == Constants.WebSiteConfiguration.MobileFragments) && x.Title.StartsWith("Banners_")).ToList();
                var miFragments = Db.WebSiteMenuItems
                                  .Where(x => (x.WebSiteMenu.Type == Constants.WebSiteConfiguration.HomeMenu ||
                                              x.WebSiteMenu.Type == Constants.WebSiteConfiguration.MobileHomeMenu) && x.Type == "banner").ToList();

                entities.ForEach(b =>
                {
                    var smiFragment = smiFragments.FirstOrDefault(x => x.Id == b.Type);
                    if (smiFragment != null)
                        b.FragmentName = smiFragment.Type + "-" + smiFragment.Title + "-" + smiFragment.WebSiteMenuItem.Title;
                    else
                    {
                        var miFragment = miFragments.FirstOrDefault(x => x.Id == b.Type);
                        if (miFragment != null)
                            b.FragmentName = "Home-" + miFragment.Title;
                    }
                });
            }
            return new PagedModel<fnBanner>
            {
                Entities = entities,
                Count = result.Count()
            };
        }

        public DAL.Banner GetBannerById(int? partnerId, int bannerId)
        {
            var bannerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewBanner,
                ObjectTypeId = ObjectTypes.Banner
            });
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });

            if (!bannerAccess.HaveAccessForAllObjects ||
                (partnerId.HasValue && !partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleObjects.All(x => x != partnerId.Value)))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);

            return Db.Banners.Include(x => x.BannerSegmentSettings).FirstOrDefault(x => x.Id == bannerId);
        }

        public object GetWebSiteFragments(int partnerId)
        {
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewBanner,
                ObjectTypeId = ObjectTypes.Banner
            });

            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            if (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleObjects.All(x => x != partnerId))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);

            var resp = Db.WebSiteSubMenuItems.Where(x => x.WebSiteMenuItem.WebSiteMenu.PartnerId == partnerId &&
                                                    (x.WebSiteMenuItem.Title == Constants.WebSiteConfiguration.WebFragments ||
                                                     x.WebSiteMenuItem.Title == Constants.WebSiteConfiguration.MobileFragments)
                                                    && x.Title.StartsWith("Banners_")).Select(x => new
                                                    {
                                                        Id = x.Id,
                                                        Name = x.Type + "-" + x.Title + "-" + x.WebSiteMenuItem.Title
                                                    }).ToList();

            resp.AddRange(Db.WebSiteMenuItems.Where(x => x.WebSiteMenu.PartnerId == partnerId &&
                                                    (x.WebSiteMenu.Type == Constants.WebSiteConfiguration.HomeMenu ||
                                                     x.WebSiteMenu.Type == Constants.WebSiteConfiguration.MobileHomeMenu)
                                                    && x.Type == "banner").Select(x => new
                                                    {
                                                        Id = x.Id,
                                                        Name = x.WebSiteMenu.Type + "-" + x.Title
                                                    }).ToList());
            return resp;
        }

        public Promotion SavePromotion(Promotion promotion)
        {
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.EditPromotions,
                ObjectTypeId = ObjectTypes.Promotion
            });
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            if (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleObjects.All(x => x != promotion.PartnerId))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            if (!Enum.IsDefined(typeof(PromotionTypes), promotion.Type) ||
                promotion.StartDate == DateTime.MinValue || promotion.FinishDate == DateTime.MinValue ||
                promotion.StartDate >= promotion.FinishDate)
                throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);
            var currentDate = DateTime.Now;
            var dbPromotion = Db.Promotions.Include(x => x.PromotionSegmentSettings).FirstOrDefault(x => x.Id == promotion.Id);
            if (dbPromotion != null)
            {
                var version = dbPromotion.ImageName.Split('?');
                dbPromotion.ImageName = version.Length > 0 ? version[0] + "?ver=" + ((version.Length > 1 ? Convert.ToInt32(version[1].Replace("ver=", string.Empty)) : 0) + 1) : string.Empty;
                dbPromotion.NickName = promotion.NickName;
                dbPromotion.Type = promotion.Type;
                dbPromotion.StartDate = promotion.StartDate;
                dbPromotion.State = promotion.State;
                dbPromotion.StartDate = promotion.StartDate;
                dbPromotion.FinishDate = promotion.FinishDate;
                dbPromotion.LastUpdateTime = currentDate;
                dbPromotion.Order = promotion.Order;
                dbPromotion.ParentId = promotion.ParentId;
                dbPromotion.StyleType = promotion.StyleType;

                if (promotion.PromotionSegmentSettings == null || !promotion.PromotionSegmentSettings.Any())
                    Db.PromotionSegmentSettings.Where(x => x.PromotionId == dbPromotion.Id).DeleteFromQuery();
                else
                {
                    var type = promotion.PromotionSegmentSettings.First().Type;
                    var segments = promotion.PromotionSegmentSettings.Select(x => x.SegmentId).ToList();
                    Db.PromotionSegmentSettings.Where(x => x.PromotionId == dbPromotion.Id &&
                                                          (x.Type != type || !segments.Contains(x.SegmentId))).DeleteFromQuery();
                    var dbSegments = Db.PromotionSegmentSettings.Where(x => x.PromotionId == dbPromotion.Id).Select(x => x.SegmentId).ToList();
                    segments.RemoveAll(x => dbSegments.Contains(x));
                    foreach (var s in segments)
                        Db.PromotionSegmentSettings.Add(new PromotionSegmentSetting { PromotionId = dbPromotion.Id, SegmentId = s, Type = type });
                }

                if (promotion.PromotionLanguageSettings == null || !promotion.PromotionLanguageSettings.Any())
                    Db.PromotionLanguageSettings.Where(x => x.PromotionId == dbPromotion.Id).DeleteFromQuery();
                else
                {
                    var type = promotion.PromotionLanguageSettings.First().Type;
                    var languages = promotion.PromotionLanguageSettings.Select(x => x.LanguageId).ToList();
                    Db.PromotionLanguageSettings.Where(x => x.PromotionId == dbPromotion.Id && (x.Type != type || !languages.Contains(x.LanguageId))).DeleteFromQuery();
                    var dbLang = Db.PromotionLanguageSettings.Where(x => x.PromotionId == dbPromotion.Id).Select(x => x.LanguageId).ToList();
                    languages.RemoveAll(x => dbLang.Contains(x));
                    foreach (var l in languages)
                    {
                        Db.PromotionLanguageSettings.Add(new PromotionLanguageSetting { PromotionId = dbPromotion.Id, LanguageId = l, Type = type });
                    }
                }

                Db.SaveChanges();
                return dbPromotion;
            }
            else
            {
                promotion.Translation = CreateTranslation(new fnTranslation
                {
                    ObjectTypeId = (int)ObjectTypes.PromotionContent,
                    Text = string.IsNullOrEmpty(promotion.Content) ? string.Empty : promotion.Content,
                    LanguageId = Constants.DefaultLanguageId
                });
                promotion.Translation1 = CreateTranslation(new fnTranslation
                {
                    ObjectTypeId = (int)ObjectTypes.PromotionDescription,
                    Text = string.IsNullOrEmpty(promotion.Description) ? string.Empty : promotion.Description,
                    LanguageId = Constants.DefaultLanguageId
                });
                promotion.Translation2 = CreateTranslation(new fnTranslation
                {
                    ObjectTypeId = (int)ObjectTypes.Promotion,
                    Text = string.IsNullOrEmpty(promotion.Title) ? string.Empty : promotion.Title,
                    LanguageId = Constants.DefaultLanguageId
                });
                promotion.CreationTime = currentDate;
                promotion.LastUpdateTime = currentDate;
                Db.Promotions.Add(promotion);
                Db.SaveChanges();
                if (!string.IsNullOrEmpty(promotion.ImageName))
                    promotion.ImageName = promotion.Id.ToString() + "." + promotion.ImageName.ToLower() + "?ver=1";
                Db.SaveChanges();
                return promotion;
            }
        }

        public Promotion RemovePromotion(int promotionId)
        {
            var dbPromotion = Db.Promotions.FirstOrDefault(x => x.Id == promotionId);
            if (dbPromotion == null)
                throw CreateException(LanguageId, Constants.Errors.PromotionNotFound);

            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.RemovePromotions,
                ObjectTypeId = ObjectTypes.Promotion
            });
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            if (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleObjects.All(x => x != dbPromotion.PartnerId))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            var contentTranslationId = dbPromotion.ContentTranslationId;
            var titleTranslationId = dbPromotion.TitleTranslationId;
            var descriptionTranslationId = dbPromotion.DescriptionTranslationId;
            Db.PromotionSegmentSettings.Where(x => x.PromotionId == promotionId).DeleteFromQuery();
            Db.PromotionLanguageSettings.Where(x => x.PromotionId == promotionId).DeleteFromQuery();
            Db.Promotions.Remove(dbPromotion);
            Db.SaveChanges();
            Db.TranslationEntries.Where(x => x.TranslationId == contentTranslationId ||
                                             x.TranslationId == titleTranslationId ||
                                             x.TranslationId == descriptionTranslationId).DeleteFromQuery();
            Db.Translations.Where(x => x.Id == contentTranslationId ||
                                       x.Id == titleTranslationId ||
                                       x.Id == descriptionTranslationId).DeleteFromQuery();
            return dbPromotion;
        }

        public List<fnPromotion> GetPromotions(int? promotionId, int? partnerId, int? parentId, int skipCount, int takeCount)
        {
            var result = Db.fn_Promotion(Identity.LanguageId, false).AsQueryable();
            if (partnerId != null)
                result = result.Where(x => x.PartnerId == partnerId);
            if (promotionId != null)
                result = result.Where(x => x.Id == promotionId);
            if(parentId == null)
                result = result.Where(x => x.ParentId == null);
            else
                result = result.Where(x => x.ParentId == parentId.Value);

            var partners = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPromotion,
                ObjectTypeId = ObjectTypes.Promotion
            });
            if (partners.HaveAccessForAllObjects)
                return result.OrderByDescending(x => x.Id).Skip(skipCount * takeCount).Take(takeCount).ToList();
            return result.Where(x => partners.AccessibleObjects.Contains(x.PartnerId)).OrderByDescending(x => x.Id).Skip(skipCount * takeCount).Take(takeCount).ToList();
        }

        public Promotion GetPromotionById(int id)
        {
            var dbPromotion = Db.Promotions.Include(x => x.PromotionSegmentSettings).Include(x => x.PromotionLanguageSettings).FirstOrDefault(x => x.Id == id);
            if (dbPromotion == null)
                throw CreateException(LanguageId, Constants.Errors.PromotionNotFound);

            var promotionAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPromotion
            });
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            if (!promotionAccess.HaveAccessForAllObjects || (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleObjects.All(x => x != dbPromotion.PartnerId)))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);

            return dbPromotion;
        }

        #endregion

        #region WebSiteMenu

        public List<WebSiteMenu> GetWebSiteMenus(int partnerId, int? deviceType)
        {
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewWebSiteMenu
            });
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            if (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleObjects.All(x => x != partnerId))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            return Db.WebSiteMenus.Where(x => x.PartnerId == partnerId && (x.DeviceType == null ||
                x.DeviceType == deviceType)).OrderBy(x => x.Id).ToList();
        }

        public List<ApiAdminMenu> GetAdminMenus(List<string> permissionIds, bool isAdmin)
        {
            var dbAdminMenus = Db.AdminMenus.ToList();
            var nodes = dbAdminMenus.Select(x => new ApiAdminMenu
            {
                Id = x.Id,
                Name = x.Name,
                Icon = x.Icon,
                Color = x.Color,
                ApiRequest = x.ApiRequest,
                Route = x.Route,
                ParentId = x.ParentId,
                Path = x.Path,
                Level = x.Path.Split('/').Count() - 1,
                PermissionId = x.PermissionId,
                Pages = new List<ApiAdminMenu>()
            }).ToList();
            var maxLevel = nodes.Max(x => x.Level);

            for (int i = 1; i < maxLevel; i++)
            {
                var parents = nodes.Where(x => x.Level == i && (isAdmin || permissionIds.Contains(x.PermissionId))).ToList();
                var children = nodes.Where(x => x.Level == i + 1 && (isAdmin || permissionIds.Contains(x.PermissionId))).ToList();
                foreach(var childNode in children)
                {
                    var responseItem = parents.FirstOrDefault(x => x.Id == childNode.ParentId);
                    if(responseItem != null)
                        responseItem.Pages.Add(childNode);
                }
            }

            return nodes.Where(x => x.Level == 1 && (isAdmin || permissionIds.Contains(x.PermissionId))).ToList();
		}

        public WebSiteSubMenuItem FindSubMenuItemByTitle(int menuId, string title)
        {
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewWebSiteMenu
            });
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            var menuItem = Db.WebSiteMenus.FirstOrDefault(x => x.Id == menuId);
            if (menuItem == null)
                throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);
            if (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleObjects.All(x => x != menuItem.PartnerId))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            return Db.WebSiteSubMenuItems.Include(x => x.WebSiteMenuItem)
                                     .FirstOrDefault(x => x.WebSiteMenuItem.MenuId == menuItem.Id && x.Title == title);
        }

        public List<WebSiteMenuItem> GetWebSiteMenuItems(int menuId)
        {
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewWebSiteMenu
            });
            var menu = Db.WebSiteMenus.Where(x => x.Id == menuId).FirstOrDefault();
            if (menu == null)
                throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);

            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            if (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleObjects.All(x => x != menu.PartnerId))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            return Db.WebSiteMenuItems.Where(x => x.MenuId == menuId).OrderBy(x => x.Order).ToList();
        }

        public List<WebSiteMenuItem> GetAdminTranslations()
        {
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewAdminTranslations
            });

            return Db.WebSiteMenuItems.Where(x => x.WebSiteMenu.PartnerId == (int)Constants.MainPartnerId && 
                x.WebSiteMenu.Type == Constants.WebSiteConfiguration.Translations).OrderBy(x => x.Order).ToList();
        }

        public List<WebSiteSubMenuItem> GetWebSiteSubMenuItems(int menuItemId)
        {
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewWebSiteMenu
            });
            var menuItem = Db.WebSiteMenuItems.Include(x => x.WebSiteMenu).Where(x => x.Id == menuItemId).FirstOrDefault();
            if (menuItem == null)
                throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);

            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            if (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleObjects.All(x => x != menuItem.WebSiteMenu.PartnerId))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            return Db.WebSiteSubMenuItems.Where(x => x.MenuItemId == menuItemId).OrderBy(x => x.Order).ToList();
        }

        public WebSiteSubMenuItem GetWebSiteSubMenuItem(int rowId)
        {
            return Db.WebSiteSubMenuItems.Include(x => x.WebSiteMenuItem.WebSiteMenu).Where(x => x.Id == rowId).FirstOrDefault();
        }

        public List<WebSiteSubMenuItem> GetAdminTranslationItems(int menuItemId)
        {
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewAdminTranslations
            });
            var menuItem = Db.WebSiteMenuItems.Include(x => x.WebSiteMenu).Where(x => x.Id == menuItemId).FirstOrDefault();
            if (menuItem == null)
                throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);

            return Db.WebSiteSubMenuItems.Where(x => x.MenuItemId == menuItemId).OrderBy(x => x.Order).ToList();
        }

        public WebSiteMenu SaveWebSiteMenu(WebSiteMenu webSiteMenu)
        {
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.EditWebSiteMenu
            });
            var dbWebSiteMenu = Db.WebSiteMenus.FirstOrDefault(x => x.Id == webSiteMenu.Id);
            if (dbWebSiteMenu == null || dbWebSiteMenu.PartnerId != webSiteMenu.PartnerId)
                throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);
            var oldValue = JsonConvert.SerializeObject(new
            {
                dbWebSiteMenu.Id,
                dbWebSiteMenu.PartnerId,
                dbWebSiteMenu.Type,
                dbWebSiteMenu.StyleType
            });
            dbWebSiteMenu.StyleType = webSiteMenu.StyleType;
            Db.SaveChanges();
            SaveChangesWithHistory((int)ObjectTypes.WebSiteMenu, dbWebSiteMenu.Id, oldValue, string.Empty);
            return dbWebSiteMenu;
        }

        public WebSiteMenuItem SaveWebSiteMenuItem(WebSiteMenuItem webSiteMenuItem, out bool broadcastChanges, out int partnerId)
        {
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.EditWebSiteMenu
            });

            broadcastChanges = false;
            var dbWebSiteMenuItem = Db.WebSiteMenuItems.Include(x => x.WebSiteMenu).FirstOrDefault(x => x.Id == webSiteMenuItem.Id);
            var menuId = dbWebSiteMenuItem != null ? dbWebSiteMenuItem.MenuId : webSiteMenuItem.MenuId;
            var generalMenu = Db.WebSiteMenus.First(x => x.Id == menuId);
            webSiteMenuItem.PartnerId = generalMenu.PartnerId;

            var oldValue = JsonConvert.SerializeObject(new
            {
                webSiteMenuItem.Id,
                webSiteMenuItem.MenuId,
                webSiteMenuItem.Icon,
                webSiteMenuItem.Title,
                webSiteMenuItem.Type,
                webSiteMenuItem.StyleType,
                webSiteMenuItem.Href,
                webSiteMenuItem.OpenInRouting,
                webSiteMenuItem.Orientation,
                webSiteMenuItem.Order
            });
            if (dbWebSiteMenuItem != null)
            {
                oldValue = JsonConvert.SerializeObject(new
                {
                    dbWebSiteMenuItem.Id,
                    dbWebSiteMenuItem.MenuId,
                    dbWebSiteMenuItem.Icon,
                    dbWebSiteMenuItem.Title,
                    dbWebSiteMenuItem.Type,
                    dbWebSiteMenuItem.StyleType,
                    dbWebSiteMenuItem.Href,
                    dbWebSiteMenuItem.OpenInRouting,
                    dbWebSiteMenuItem.Orientation,
                    dbWebSiteMenuItem.Order
                });
                dbWebSiteMenuItem.Icon = webSiteMenuItem.Icon;
                dbWebSiteMenuItem.Title = webSiteMenuItem.Title;
                dbWebSiteMenuItem.Href = webSiteMenuItem.Href ?? string.Empty;
                dbWebSiteMenuItem.Type = webSiteMenuItem.Type;
                dbWebSiteMenuItem.StyleType = webSiteMenuItem.StyleType;
                dbWebSiteMenuItem.OpenInRouting = webSiteMenuItem.OpenInRouting;
                dbWebSiteMenuItem.Orientation = webSiteMenuItem.Orientation;
                dbWebSiteMenuItem.Order = webSiteMenuItem.Order;
                dbWebSiteMenuItem.Type = webSiteMenuItem.Type;
            }
            else
                Db.WebSiteMenuItems.Add(webSiteMenuItem);
            Db.SaveChanges();
            partnerId = generalMenu.PartnerId;
            if (generalMenu.Type == Constants.WebSiteConfiguration.Config)
            {
                SaveChangesWithHistory(webSiteMenuItem.Title == Constants.PartnerKeys.TermsConditionVersion ?
                    (int)ObjectTypes.TermsConditions : (int)ObjectTypes.WebSiteMenuItem, webSiteMenuItem.Id, oldValue, string.Empty);

                if (webSiteMenuItem.Title == Constants.PartnerKeys.WhitelistedCountries ||
                    webSiteMenuItem.Title == Constants.PartnerKeys.BlockedCountries ||
                    webSiteMenuItem.Title == Constants.PartnerKeys.WhitelistedIps ||
                    webSiteMenuItem.Title == Constants.PartnerKeys.BlockedIps ||
                    webSiteMenuItem.Title == Constants.PartnerKeys.RegistrationLimitPerDay)
                    broadcastChanges = true;

                if (webSiteMenuItem.Title == Constants.PartnerKeys.TermsConditionVersion)
                {
                    var items = Db.WebSiteSubMenuItems.Include(x => x.WebSiteMenuItem).Where(x => x.WebSiteMenuItem.WebSiteMenu.Type == Constants.WebSiteConfiguration.Translations &&
                        x.WebSiteMenuItem.WebSiteMenu.PartnerId == generalMenu.PartnerId && x.Title == webSiteMenuItem.Href).ToList();
                    var item = items.FirstOrDefault(x => x.WebSiteMenuItem.Title.Replace("_", "").ToLower() == Constants.WebSiteConfiguration.TermsConditions.ToLower());

                    if (item != null)
                    {
                        item.Href = DateTime.UtcNow.ToString("yyyy/MM/dd HH:mm:ss");
                        Db.SaveChanges();
                    }
                }

                CacheManager.RemoveConfigKey(generalMenu.PartnerId, webSiteMenuItem.Title);
                CacheManager.RemoveFromCache(string.Format("{0}_{1}_{2}", Constants.CacheItems.ConfigParameters, partnerId, webSiteMenuItem.Title));
            }
            else if (generalMenu.Type == Constants.WebSiteConfiguration.CasinoMenu)
            {
                CacheManager.RemoveCasinoMenues(generalMenu.PartnerId);
            }
            if (!string.IsNullOrEmpty(webSiteMenuItem.Image))
                UploadMenuImage(webSiteMenuItem.PartnerId, webSiteMenuItem.Icon, webSiteMenuItem.Image, webSiteMenuItem.Title, string.Empty, generalMenu.Type);

            return webSiteMenuItem;
        }

        public WebSiteMenuItem SaveAdminTramslation(WebSiteMenuItem webSiteMenuItem)
        {
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.EditAdminTranslation
            });
            var menu = Db.WebSiteMenus.Where(x => x.PartnerId == (int)Constants.MainPartnerId && x.Type == Constants.WebSiteConfiguration.Translations).First();
            webSiteMenuItem.MenuId = menu.Id;

            var dbWebSiteMenuItem = Db.WebSiteMenuItems.Include(x => x.WebSiteMenu).FirstOrDefault(x => x.Id == webSiteMenuItem.Id);
            
            if (dbWebSiteMenuItem != null)
            {
                dbWebSiteMenuItem.Icon = webSiteMenuItem.Icon;
                dbWebSiteMenuItem.Title = webSiteMenuItem.Title;
                dbWebSiteMenuItem.Href = webSiteMenuItem.Href ?? string.Empty;
                dbWebSiteMenuItem.Type = webSiteMenuItem.Type;
                dbWebSiteMenuItem.StyleType = webSiteMenuItem.StyleType;
                dbWebSiteMenuItem.OpenInRouting = webSiteMenuItem.OpenInRouting;
                dbWebSiteMenuItem.Orientation = webSiteMenuItem.Orientation;
                dbWebSiteMenuItem.Order = webSiteMenuItem.Order;
                dbWebSiteMenuItem.Type = webSiteMenuItem.Type;
            }
            else
                Db.WebSiteMenuItems.Add(webSiteMenuItem);
            Db.SaveChanges();

            return webSiteMenuItem;
        }

        public WebSiteSubMenuItem SaveWebSiteSubMenuItem(WebSiteSubMenuItem webSiteSubMenuItem, out bool broadcastChanges)
        {
            try
            {
                GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.EditWebSiteMenu
                });
                broadcastChanges = false;
                var menuItem = Db.WebSiteMenuItems.Include(x => x.WebSiteMenu).First(x => x.Id == webSiteSubMenuItem.MenuItemId);
                var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewPartner,
                    ObjectTypeId = ObjectTypes.Partner
                });
                if (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleObjects.All(x => x != menuItem.WebSiteMenu.PartnerId))
                    throw CreateException(LanguageId, Constants.Errors.DontHavePermission);

                var dbWebSiteSubMenuItem = Db.WebSiteSubMenuItems.Include(x => x.WebSiteMenuItem.WebSiteMenu).FirstOrDefault(x => x.Id == webSiteSubMenuItem.Id);
                if (dbWebSiteSubMenuItem != null)
                {
                    if (dbWebSiteSubMenuItem.WebSiteMenuItem.Title == Constants.SubMenuConfiguration.TermsAndConditions)
                        throw CreateException(LanguageId, Constants.Errors.DontHavePermission);

                    dbWebSiteSubMenuItem.Icon = webSiteSubMenuItem.Icon;
                    dbWebSiteSubMenuItem.Title = webSiteSubMenuItem.Title;
                    dbWebSiteSubMenuItem.Href = webSiteSubMenuItem.Href;
                    dbWebSiteSubMenuItem.Type = webSiteSubMenuItem.Type;
                    dbWebSiteSubMenuItem.StyleType = webSiteSubMenuItem.StyleType;
                    dbWebSiteSubMenuItem.OpenInRouting = webSiteSubMenuItem.OpenInRouting;
                    dbWebSiteSubMenuItem.Order = webSiteSubMenuItem.Order;
                    dbWebSiteSubMenuItem.Type = webSiteSubMenuItem.Type;
                    Db.SaveChanges();
                }
                else
                {
                    if (menuItem.WebSiteMenu.Type == Constants.WebSiteConfiguration.Translations)
                    {
                        webSiteSubMenuItem.Translation = CreateTranslation(new fnTranslation
                        {
                            ObjectTypeId = (int)ObjectTypes.WebSiteTranslation,
                            Text = menuItem.Title == Constants.SubMenuConfiguration.TermsAndConditions ? string.Empty : webSiteSubMenuItem.Title,
                            LanguageId = Constants.DefaultLanguageId
                        });
                    }
                    Db.WebSiteSubMenuItems.Add(webSiteSubMenuItem);
                    Db.SaveChanges();
                }
                if (menuItem.WebSiteMenu.Type == Constants.WebSiteConfiguration.Config)
                {
                    CacheManager.RemoveFromCache(string.Format("{0}_{1}_{2}", Constants.CacheItems.ConfigParameters, menuItem.WebSiteMenu.PartnerId, menuItem.Title));
                    if (menuItem.Title == Constants.PartnerKeys.WhitelistedCountries ||
                    menuItem.Title == Constants.PartnerKeys.BlockedCountries ||
                    menuItem.Title == Constants.PartnerKeys.WhitelistedIps ||
                    menuItem.Title == Constants.PartnerKeys.BlockedIps ||
                    (menuItem.WebSiteMenu.PartnerId == Constants.MainPartnerId && menuItem.Title.Contains(Constants.CacheItems.WhitelistedIps)))
                    {
                        CacheManager.RemoveFromCache(string.Format("{0}_{1}", Constants.CacheItems.WhitelistedIps,
                            menuItem.Title.Replace(Constants.CacheItems.WhitelistedIps, string.Empty)));
                        broadcastChanges = true;
                    }
                }
                webSiteSubMenuItem.PartnerId = menuItem.WebSiteMenu.PartnerId;
                webSiteSubMenuItem.MenuItemName = menuItem.Title;
                if (!string.IsNullOrEmpty(webSiteSubMenuItem.Image))
                    UploadMenuImage(webSiteSubMenuItem.PartnerId, webSiteSubMenuItem.Icon, webSiteSubMenuItem.Image, webSiteSubMenuItem.MenuItemName, webSiteSubMenuItem.Title, menuItem.WebSiteMenu.Type);

                return webSiteSubMenuItem;
            }
            catch (DbEntityValidationException e)
            {
                var m = string.Empty;
                foreach (var eve in e.EntityValidationErrors)
                {
                    m += string.Format("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:", eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        m += string.Format("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                Log.Error(m);
                throw;
            }
        }

        public WebSiteSubMenuItem SaveAdminTranslationItem(WebSiteSubMenuItem webSiteSubMenuItem)
        {
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.EditAdminTranslation
            });
            var dbWebSiteSubMenuItem = Db.WebSiteSubMenuItems.Include(x => x.WebSiteMenuItem.WebSiteMenu).FirstOrDefault(x => x.Id == webSiteSubMenuItem.Id);

            if (dbWebSiteSubMenuItem != null)
            {
                dbWebSiteSubMenuItem.Icon = webSiteSubMenuItem.Icon;
                dbWebSiteSubMenuItem.Title = webSiteSubMenuItem.Title;
                dbWebSiteSubMenuItem.Href = webSiteSubMenuItem.Href;
                dbWebSiteSubMenuItem.Type = webSiteSubMenuItem.Type;
                dbWebSiteSubMenuItem.OpenInRouting = webSiteSubMenuItem.OpenInRouting;
                dbWebSiteSubMenuItem.Order = webSiteSubMenuItem.Order;
                dbWebSiteSubMenuItem.Type = webSiteSubMenuItem.Type;
                Db.SaveChanges();
            }
            else
            {
                webSiteSubMenuItem.Translation = CreateTranslation(new fnTranslation
                {
                    ObjectTypeId = (int)ObjectTypes.WebSiteTranslation,
                    Text = webSiteSubMenuItem.Title,
                    LanguageId = Constants.DefaultLanguageId
                });
                Db.WebSiteSubMenuItems.Add(webSiteSubMenuItem);
                Db.SaveChanges();
            }

            return webSiteSubMenuItem;
        }

        public KeyValuePair<int, string> RemoveWebSiteMenuItem(int menuItemId, out bool broadcastChanges)
        {
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.EditWebSiteMenu
            });
            broadcastChanges = false;
            var dbWebSiteMenuItem = Db.WebSiteMenuItems.Include(x => x.WebSiteMenu).FirstOrDefault(x => x.Id == menuItemId);
            if (dbWebSiteMenuItem != null)
            {
                var partnerId = dbWebSiteMenuItem.WebSiteMenu.PartnerId;
                var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewPartner,
                    ObjectTypeId = ObjectTypes.Partner
                });
                if (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleObjects.All(x => x != partnerId))
                    throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
                if (dbWebSiteMenuItem.WebSiteMenu.Type == Constants.WebSiteConfiguration.Config &&
                    dbWebSiteMenuItem.Title == Constants.PartnerKeys.RegistrationLimitPerDay)
                    broadcastChanges = true;
                Db.WebSiteMenuItems.Remove(dbWebSiteMenuItem);
                Db.SaveChanges();
                return new KeyValuePair<int, string>(partnerId, dbWebSiteMenuItem.Title);
            }
            return new KeyValuePair<int, string>(0, string.Empty);
        }

        public void RemoveAdminTranslation(int menuItemId)
        {
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.EditAdminTranslation
            });
            var dbWebSiteMenuItem = Db.WebSiteMenuItems.Include(x => x.WebSiteMenu).FirstOrDefault(x => x.Id == menuItemId);
            if (dbWebSiteMenuItem != null)
            {
                Db.WebSiteMenuItems.Remove(dbWebSiteMenuItem);
                Db.SaveChanges();
            }
        }

        public KeyValuePair<int, string> RemoveWebSiteSubMenuItem(int subMenuItemId, out bool broadcastChanges)
        {
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.EditWebSiteMenu
            });
            broadcastChanges = false;
            var dbWebSiteSubMenuItem = Db.WebSiteSubMenuItems.Include(x => x.WebSiteMenuItem.WebSiteMenu)
                                                            .FirstOrDefault(x => x.Id == subMenuItemId);
            if (dbWebSiteSubMenuItem != null)
            {
                var partnerId = dbWebSiteSubMenuItem.WebSiteMenuItem.WebSiteMenu.PartnerId;
                var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewPartner,
                    ObjectTypeId = ObjectTypes.Partner
                });
                if (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleObjects.All(x => x != partnerId))
                    throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
                var res = new KeyValuePair<int, string>(partnerId, dbWebSiteSubMenuItem.WebSiteMenuItem.Title);
                if (dbWebSiteSubMenuItem.WebSiteMenuItem.WebSiteMenu.Type == Constants.WebSiteConfiguration.Config &&
                   (dbWebSiteSubMenuItem.WebSiteMenuItem.Title == Constants.PartnerKeys.WhitelistedCountries ||
                    dbWebSiteSubMenuItem.WebSiteMenuItem.Title == Constants.PartnerKeys.BlockedCountries ||
                    dbWebSiteSubMenuItem.WebSiteMenuItem.Title == Constants.PartnerKeys.WhitelistedIps ||
                    dbWebSiteSubMenuItem.WebSiteMenuItem.Title == Constants.PartnerKeys.BlockedIps))
                    broadcastChanges = true;

                Db.WebSiteSubMenuItems.Remove(dbWebSiteSubMenuItem);
                Db.SaveChanges();
                return res;
            }
            return new KeyValuePair<int, string>(0, string.Empty);
        }

        public void RemoveAdminTranslationItem(int subMenuItemId)
        {
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.EditAdminTranslation
            });

            var dbWebSiteSubMenuItem = Db.WebSiteSubMenuItems.Include(x => x.WebSiteMenuItem.WebSiteMenu)
                                                            .FirstOrDefault(x => x.Id == subMenuItemId);
            if (dbWebSiteSubMenuItem != null)
            {
                Db.WebSiteSubMenuItems.Remove(dbWebSiteSubMenuItem);
                Db.SaveChanges();
            }
        }

        public WebSiteTranslation GetWebSiteTranslations(int subMenuItemId)
        {
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.EditWebSiteMenu
            });
            var webSiteSubMenuItem = Db.WebSiteSubMenuItems.Include(x => x.WebSiteMenuItem.WebSiteMenu).First(x => x.Id == subMenuItemId);
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            if (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleObjects.All(x => x != webSiteSubMenuItem.WebSiteMenuItem.WebSiteMenu.PartnerId))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);

            var languages = Db.PartnerLanguageSettings.Include(x => x.Language).
                Where(x => x.PartnerId == webSiteSubMenuItem.WebSiteMenuItem.WebSiteMenu.PartnerId && x.State == (int)PartnerLanguageStates.Active).ToList();

            var response = new WebSiteTranslation
            {
                ItemId = subMenuItemId,
                NickName = webSiteSubMenuItem.Title,
                TranslationId = webSiteSubMenuItem.TranslationId ?? 0,
                Translations = Db.TranslationEntries.Where(x => x.TranslationId == webSiteSubMenuItem.TranslationId.Value)
                .Select(x => new WebSiteTranslationEntry
                {
                    Id = x.Id,
                    LanguageId = x.LanguageId,
                    Text = x.Text
                }).ToList()
            };
            foreach (var l in languages)
            {
                var lg = response.Translations.FirstOrDefault(x => x.LanguageId == l.LanguageId);
                if (lg != null)
                    lg.Language = l.Language.Name;
                else
                {
                    response.Translations.Add(new WebSiteTranslationEntry { Language = l.Language.Name, LanguageId = l.LanguageId, Text = string.Empty });
                }
            }
            return response;
        }

        public WebSiteTranslation GetAdminTranslations(int subMenuItemId)
        {
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.EditAdminTranslation
            });
            var webSiteSubMenuItem = Db.WebSiteSubMenuItems.Include(x => x.WebSiteMenuItem.WebSiteMenu).First(x => x.Id == subMenuItemId);
            var languages = Db.PartnerLanguageSettings.Include(x => x.Language).
                Where(x => x.PartnerId == webSiteSubMenuItem.WebSiteMenuItem.WebSiteMenu.PartnerId && x.State == (int)PartnerLanguageStates.Active).ToList();

            var response = new WebSiteTranslation
            {
                ItemId = subMenuItemId,
                NickName = webSiteSubMenuItem.Title,
                TranslationId = webSiteSubMenuItem.TranslationId ?? 0,
                Translations = Db.TranslationEntries.Where(x => x.TranslationId == webSiteSubMenuItem.TranslationId.Value)
                .Select(x => new WebSiteTranslationEntry
                {
                    Id = x.Id,
                    LanguageId = x.LanguageId,
                    Text = x.Text
                }).ToList()
            };
            foreach (var l in languages)
            {
                var lg = response.Translations.FirstOrDefault(x => x.LanguageId == l.LanguageId);
                if (lg != null)
                    lg.Language = l.Language.Name;
                else
                {
                    response.Translations.Add(new WebSiteTranslationEntry { Language = l.Language.Name, LanguageId = l.LanguageId, Text = string.Empty });
                }
            }
            return response;
        }

        public ObjectTranslations GetObjectTranslations(int objectTypeId, int objectId, string languageId)
        {
            var partnerId = 0;
            var translationObject = new ObjectTranslations();
            switch (objectTypeId)
            {
                case (int)ObjectTypes.MessageTemplate:
                    GetPermissionsToObject(new CheckPermissionInput
                    {
                        Permission = Constants.Permissions.EditPartnerMessageTemplate
                    });

                    var messageTemplate = Db.MessageTemplates.FirstOrDefault(x => x.Id == objectId);
                    if (messageTemplate == null)
                        throw CreateException(LanguageId, Constants.Errors.MessageNotFound);
                    partnerId = messageTemplate.PartnerId;
                    translationObject = new ObjectTranslations
                    {
                        Id = messageTemplate.Id,
                        NickName = messageTemplate.NickName,
                        TranslationId = messageTemplate.TranslationId
                    };
                    break;
                case (int)ObjectTypes.BannerBody:
                case (int)ObjectTypes.BannerHead:
                    var banner = Db.Banners.FirstOrDefault(x => x.Id == objectId);
                    if (banner == null)
                        throw CreateException(LanguageId, Constants.Errors.TranslationNotFound);
                    partnerId = banner.PartnerId;
                    translationObject = new ObjectTranslations
                    {
                        Id = banner.Id,
                        NickName = banner.Body,
                        TranslationId = objectTypeId == (int)ObjectTypes.BannerBody ? banner.BodyTranslationId ?? 0 : banner.HeadTranslationId ?? 0
                    };
                    break;
                case (int)ObjectTypes.CommentTemplate:
                    GetPermissionsToObject(new CheckPermissionInput
                    {
                        Permission = Constants.Permissions.EditPartnerCommentTemplate
                    });

                    var commentTemplate = Db.CommentTemplates.FirstOrDefault(x => x.Id == objectId);
                    if (commentTemplate == null)
                        throw CreateException(LanguageId, Constants.Errors.CommentTemplateNotFound);
                    partnerId = commentTemplate.PartnerId;
                    translationObject = new ObjectTranslations
                    {
                        Id = commentTemplate.Id,
                        NickName = commentTemplate.NickName,
                        TranslationId = commentTemplate.TranslationId
                    };
                    break;
                case (int)ObjectTypes.Bonus:
                    GetPermissionsToObject(new CheckPermissionInput
                    {
                        Permission = Constants.Permissions.EditBonuses
                    });
                    var bonus = Db.Bonus.FirstOrDefault(x => x.Id == objectId);
                    if (bonus == null)
                        throw CreateException(LanguageId, Constants.Errors.BonusNotFound);
                    partnerId = bonus.PartnerId;
                    if (!bonus.TranslationId.HasValue)
                    {
                        bonus.Translation = CreateTranslation(new fnTranslation
                        {
                            ObjectTypeId = (int)ObjectTypes.Bonus,
                            Text = bonus.Name,
                            LanguageId = Constants.DefaultLanguageId
                        });
                        Db.SaveChanges();
                    }
                    translationObject = new ObjectTranslations
                    {
                        Id = bonus.Id,
                        NickName = bonus.Name,
                        TranslationId = bonus.Translation.Id
                    };
                    break;
                case (int)ObjectTypes.Trigger:
                    GetPermissionsToObject(new CheckPermissionInput
                    {
                        Permission = Constants.Permissions.EditBonuses
                    });
                    var trigger = Db.TriggerSettings.FirstOrDefault(x => x.Id == objectId);
                    if (trigger == null)
                        throw CreateException(LanguageId, Constants.Errors.BonusNotFound);
                    partnerId = trigger.PartnerId;
                    translationObject = new ObjectTranslations
                    {
                        Id = trigger.Id,
                        NickName = trigger.Name,
                        TranslationId = trigger.TranslationId
                    };
                    break;
                case (int)ObjectTypes.Promotion:
                case (int)ObjectTypes.PromotionContent:
                case (int)ObjectTypes.PromotionDescription:
                    GetPermissionsToObject(new CheckPermissionInput
                    {
                        Permission = Constants.Permissions.EditPromotions
                    });

                    var promotion = Db.Promotions.FirstOrDefault(x => x.Id == objectId);
                    if (promotion == null)
                        throw CreateException(LanguageId, Constants.Errors.PromotionNotFound);
                    partnerId = promotion.PartnerId;
                    translationObject = new ObjectTranslations
                    {
                        Id = promotion.Id,
                        NickName = promotion.NickName,
                        TranslationId = objectTypeId == (int)ObjectTypes.Promotion ? promotion.TitleTranslationId :
                        (objectTypeId == (int)ObjectTypes.PromotionContent ? promotion.ContentTranslationId : promotion.DescriptionTranslationId)
                    };
                    break;
                case (int)ObjectTypes.SecurityQuestion:
                    var securityQuestion = Db.SecurityQuestions.FirstOrDefault(x => x.Id == objectId);
                    if (securityQuestion == null)
                        throw CreateException(LanguageId, Constants.Errors.WrongSecurityQuestionAnswer);
                    partnerId = securityQuestion.PartnerId;
                    translationObject = new ObjectTranslations
                    {
                        Id = securityQuestion.Id,
                        NickName = securityQuestion.NickName,
                        TranslationId = securityQuestion.TranslationId
                    };
                    break;
                case (int)ObjectTypes.Announcement:
                    var announcement = Db.Announcements.FirstOrDefault(x => x.Id == objectId);
                    if (announcement == null)
                        throw CreateException(LanguageId, Constants.Errors.ObjectTypeNotFound);
                    partnerId = announcement.PartnerId;
                    translationObject = new ObjectTranslations
                    {
                        Id = (int)announcement.Id,
                        NickName = announcement.NickName,
                        TranslationId = announcement.TranslationId
                    };
                    break;
                case (int)ObjectTypes.CharacterTitle:
                case (int)ObjectTypes.CharacterDescription:
                    var character = Db.Characters.FirstOrDefault(x => x.Id == objectId);
                    if (character == null)
                        throw CreateException(LanguageId, Constants.Errors.ObjectTypeNotFound);
                    partnerId = character.PartnerId;
                    translationObject = new ObjectTranslations
                    {
                        Id = (int)character.Id,
                        NickName = character.NickName,
                        TranslationId = objectTypeId == (int)ObjectTypes.CharacterTitle ? character.TitleTranslationId : character.DescriptionTranslationId
                    };
                    break;
                default: break;
            }
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            if (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleObjects.All(x => x != partnerId))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);

            var languages = Db.PartnerLanguageSettings.Include(x => x.Language).
                Where(x => x.PartnerId == partnerId && x.State == (int)PartnerLanguageStates.Active).ToList();
            translationObject.Translations = Db.TranslationEntries.Where(x => x.TranslationId == translationObject.TranslationId &&
                (string.IsNullOrEmpty(languageId) || x.LanguageId == languageId))
                .Select(x => new ObjectTranslationEntry
                {
                    Id = x.Id,
                    LanguageId = x.LanguageId,
                    Text = x.Text
                }).ToList();
            if (string.IsNullOrEmpty(languageId))
                foreach (var l in languages)
                {
                    var lg = translationObject.Translations.FirstOrDefault(x => x.LanguageId == l.LanguageId);
                    if (lg != null)
                        lg.Language = l.Language.Name;
                    else
                    {
                        translationObject.Translations.Add(new ObjectTranslationEntry { Language = l.Language.Name, LanguageId = l.LanguageId, Text = string.Empty });
                    }
                }
            return translationObject;
        }

        public void CloneWebSiteMenu(int fromPartnerId, int toPartnerId, int? menuItemId)
        {
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.EditWebSiteMenu
            });
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            if (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleObjects.All(x => x != fromPartnerId))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            var partner = CacheManager.GetPartnerById(fromPartnerId);
            var toPartner = CacheManager.GetPartnerById(toPartnerId);

            if (partner == null || toPartner == null)
                throw CreateException(LanguageId, Constants.Errors.PartnerNotFound);

            Db.sp_CreateWebSiteMenuCopy(fromPartnerId, toPartnerId, menuItemId);
        }

        public void GenerateWebSiteStylesFile(int partnerId, FtpModel ftpInput)
        {
            CheckPermission(Constants.Permissions.EditStyles);
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            if (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleObjects.All(x => x != partnerId))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);

            var stylesQuery = Db.WebSiteSubMenuItems.Where(x => x.WebSiteMenuItem.WebSiteMenu.PartnerId == partnerId &&
            x.WebSiteMenuItem.WebSiteMenu.Type == Constants.WebSiteConfiguration.Styles).Select(x => new
            {
                x.Title,
                x.Type,
                ParentType = x.WebSiteMenuItem.Type,
                ParentStyleType = x.WebSiteMenuItem.StyleType
            }).AsEnumerable();
            var partner = CacheManager.GetPartnerById(partnerId);
            if (partner == null)
                throw CreateException(LanguageId, Constants.Errors.PartnerNotFound);

            var styles = stylesQuery.ToDictionary(y => (string.IsNullOrEmpty(y.ParentType) ? string.Empty : (y.ParentType + "-" + y.ParentStyleType + "-")) + y.Title, y => y.Type);
            if (styles != null)
            {
                var text = styles.Aggregate(":root{" + Environment.NewLine, (current, par) => current + par.Key + ":" + par.Value + ";" + Environment.NewLine) + "}";


                UploadFile(text, "/coreplatform/website/" + partner.Name.ToLower() + "/assets/css/skin.css", ftpInput);
                UploadFile(string.Format("window.VERSION = {0};", new Random().Next(1, 1000)), "/coreplatform/website/" + partner.Name.ToLower() + "/assets/js/version.js", ftpInput);
            }
            var fonts = Db.WebSiteSubMenuItems.Where(x => x.WebSiteMenuItem.WebSiteMenu.PartnerId == partnerId && x.WebSiteMenuItem.WebSiteMenu.Type == Constants.WebSiteConfiguration.Assets &&
                                                          x.WebSiteMenuItem.Title == Constants.WebSiteConfiguration.Fonts)
                                              .Select(x => new
                                              {
                                                  FontFamily = x.Title,
                                                  x.Type,
                                                  Weight = x.Href,
                                                  Src = x.Icon,
                                                  x.Order
                                              }).OrderBy(x => x.Order).ToList();
            UploadFile(JsonConvert.SerializeObject(fonts), "/coreplatform/website/" + partner.Name.ToLower() + "/assets/json/fonts.json", ftpInput);
        }

        public void GenerateConfigFile(int partnerId, FtpModel ftpInput)
        {
            CheckPermission(Constants.Permissions.EditConfig);
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            if (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleObjects.All(x => x != partnerId))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            var partner = CacheManager.GetPartnerById(partnerId);
            var dl = CacheManager.GetPartnerSettingByKey(partnerId, "DefaultLanguage");
            var sip = CacheManager.GetPartnerSettingByKey(partnerId, "ShowInfoPopup");
            var ea = CacheManager.GetPartnerSettingByKey(partnerId, "EmailAddress");
            var partnerSetting = CacheManager.GetConfigKey(partnerId, Constants.PartnerKeys.VerificationKeyNumberOnly);
            var partnerDomains = partner.SiteUrl.Split(',');
            var domain = partnerDomains.Count() == 1 ? partner.SiteUrl : "{ws}";
            var webSiteConfig = Db.WebSiteMenus.Where(x => x.PartnerId == partnerId && x.Type == Constants.WebSiteConfiguration.Config)
                .Select(x => new BllMenu
                {
                    Id = x.Id,
                    Type = x.Type,
                    StyleType = x.StyleType,
                    Items = x.WebSiteMenuItems.Select(y => new BllMenuItem
                    {
                        Id = y.Id,
                        Icon = y.Icon,
                        Title = y.Title,
                        Type = y.Type,
                        Href = y.Href,
                        OpenInRouting = y.OpenInRouting,
                        Orientation = y.Orientation,
                        Order = y.Order,
                        SubMenu = y.WebSiteSubMenuItems.Select(z => new BllSubMenuItem
                        {
                            Id = z.Id,
                            Icon = z.Icon,
                            Title = z.Title,
                            Type = z.Type,
                            Href = z.Href,
                            StyleType = z.StyleType,
                            OpenInRouting = z.OpenInRouting,
                            Order = z.Order
                        }).OrderBy(z => z.Order).ToList()
                    }).OrderBy(y => y.Order).ToList()
                }).FirstOrDefault();

            var mc = webSiteConfig?.Items.FirstOrDefault(x => x.Title == "MobileCodes");
            var rt = webSiteConfig?.Items.FirstOrDefault(x => x.Title == "RegisterType");
            var qrt = webSiteConfig?.Items.FirstOrDefault(x => x.Title == "QuickRegisterType");
            var snp = webSiteConfig?.Items.FirstOrDefault(x => x.Title == "SocialNetworkProviders");
            var eb = webSiteConfig?.Items.FirstOrDefault(x => x.Title == "ExternalBalance");
            var license = webSiteConfig?.Items.FirstOrDefault(x => x.Title == "LicenseUrl");
            var hp = webSiteConfig?.Items.FirstOrDefault(x => x.Title == "HomePage");
            var csk = webSiteConfig?.Items.FirstOrDefault(x => x.Title == "CaptchaSiteKey");
            var ce = webSiteConfig?.Items.FirstOrDefault(x => x.Title == "CaptchaEnabled");
            var cpu = webSiteConfig?.Items.FirstOrDefault(x => x.Title == "CashierPageUrl");
            var smnp = webSiteConfig?.Items.FirstOrDefault(x => x.Title == "ShowMobileNavPanel");
            var fv = webSiteConfig?.Items.FirstOrDefault(x => x.Title == "FooterVisibility");
            var hbg = webSiteConfig?.Items.FirstOrDefault(x => x.Title == "HomeBGImage");
            var gl = webSiteConfig?.Items.FirstOrDefault(x => x.Title == "GameLayouts");
            var tz = webSiteConfig?.Items.FirstOrDefault(x => x.Title == "TimeZone");
            var sep = webSiteConfig?.Items.FirstOrDefault(x => x.Title == "SelfExclusionPeriod");
            var edp = webSiteConfig?.Items.FirstOrDefault(x => x.Title == "ErrorDisplayTime");
            var att = webSiteConfig?.Items.FirstOrDefault(x => x.Title == "AccountTemplateType");
            var som = webSiteConfig?.Items.FirstOrDefault(x => x.Title == "SportOpenMode");
            var slp = webSiteConfig?.Items.FirstOrDefault(x => x.Title == "ShowLogoutInfoPopup");
            var sllp = webSiteConfig?.Items.FirstOrDefault(x => x.Title == "LastLoginInfo");
            var cp = webSiteConfig?.Items.FirstOrDefault(x => x.Title == "CheckPortrait");
            var ww = webSiteConfig?.Items.FirstOrDefault(x => x.Title == "WinnersWidget");
            var sgn = webSiteConfig?.Items.FirstOrDefault(x => x.Title == "ShowGameNames");
            var waURL = webSiteConfig?.Items.FirstOrDefault(x => x.Title == "WebApiUrl");
            var wsm = webSiteConfig?.Items.FirstOrDefault(x => x.Title == "WebSlideMode");
            var fvt = webSiteConfig?.Items.FirstOrDefault(x => x.Title == "FormValidationType");
            var dm = webSiteConfig?.Items.FirstOrDefault(x => x.Title == "DefaultMode");
            var vcfw = webSiteConfig?.Items.FirstOrDefault(x => x.Title == "VerificationCodeForWithdraw");
            var che = webSiteConfig?.Items.FirstOrDefault(x => x.Title == "CharactersEnabled");
            var accw = webSiteConfig?.Items.FirstOrDefault(x => x.Title == "AllowCancelConfirmedWithdraw");

            var wf = webSiteConfig?.Items.FirstOrDefault(x => x.Title == Constants.WebSiteConfiguration.WebFragments)?.SubMenu?.ToList();
            var mf = webSiteConfig?.Items.FirstOrDefault(x => x.Title == Constants.WebSiteConfiguration.MobileFragments)?.SubMenu?.ToList();

            var config = new
            {
                PartnerId = partnerId,
                PartnerName = partner.Name,
                WebApiUrl = (waURL != null && !string.IsNullOrEmpty(waURL.Href)) ? waURL.Href : string.Format("https://websitewebapi.{0}", domain),
                DefaultCurrency = partner.CurrencyId,
                Domain = domain,
                AllowedAge = partner.ClientMinAge,
                VerificationCodeForWithdraw = (vcfw != null && !string.IsNullOrEmpty(vcfw.Href) ? Convert.ToInt32(vcfw.Href) : 0),
                DefaultLanguage = (dl == null || dl.Id == 0) ? Constants.DefaultLanguageId : dl.StringValue,
                ShowGameNames = sgn != null && sgn.Href == "1",
                ShowInfoPopup = !(sip == null || sip.Id == 0 || sip.NumericValue == 0),
                ReCaptchaKey = csk == null ? string.Empty : csk.Href,
                IsReCaptcha = ce != null && ce.Href != "0",
                HomePageType = hp == null ? string.Empty : hp.Href,
                CashierPageUrl = cpu == null ? string.Empty : cpu.Href,
                RedirectUrl = hp == null ? string.Empty : hp.Href,
                ShowMobileNavPanel = smnp == null ? "0" : smnp.Href,
                EmailAddress = (ea == null || ea.Id == 0) ? string.Empty : ea.StringValue,
                Languages = Db.PartnerLanguageSettings.Where(x => x.PartnerId == partnerId &&
                    x.State == (int)PartnerLanguageStates.Active).OrderBy(x => x.Order).Select(x => new { key = x.LanguageId, value = x.Language.Name }).ToList(),
                Currencies = Db.PartnerCurrencySettings.Where(x => x.PartnerId == partnerId &&
                    x.State == (int)PartnerCurrencyStates.Active).OrderBy(x => x.Priority).Select(x => x.Currency.Name).ToList(),
                ProductsWithTransfer = new List<object>(),
                MobileCodes = mc == null ? new List<object>().Select(x => new { Title = "", Type = "", Mask = "", StyleType = "" }).ToList() : mc.SubMenu.Select(x => new { x.Title,x.Type, Mask = x.Href, x.StyleType }).ToList(),
                RegisterType = rt == null ? new List<object>().Select(x => new { Title = "", Order = 0, Settings = "{}" }).ToList() :
                    rt.SubMenu.Select(x => new { Title = x.Title, Order = x.Order, Settings = x.Href }).ToList(),
                QuickRegisterType = (qrt == null ? new List<object>().Select(x => new { Title = "", Order = 0, Type = "" }).ToList() : qrt.SubMenu.Select(x => new { Title = x.Title, Order = x.Order, Type = x.Type }).ToList()),
                SocialNetworkProviders = (snp == null ? new List<object>().Select(x => new { Title = "", ProviderId = "", Order = 0 }).ToList() : snp.SubMenu.Select(x => new { Title = x.Title, ProviderId = x.Href, Order = x.Order }).ToList()),
                ExternalBalance = eb == null ? new List<int>() : eb.SubMenu.Select(x => Convert.ToInt32(x.Type)).ToList(),
                LicenseUrl = license == null ? string.Empty : license.Href,
                FooterVisibility = (fv == null ? new List<object>().Select(x => new { Title = "", Type = "" }).ToList() : fv.SubMenu.Select(x => new { Title = x.Title, Type = x.Type }).ToList()),
                HomeBGImage = hbg == null ? string.Empty : hbg.Href,
                GameLayouts = gl == null ? new List<object>().Select(x => new { Type = "", Href = "" }).ToList() : gl.SubMenu.OrderBy(x => x.Order).Select(x => new { x.Type, x.Href }).ToList(),
                TimeZone = tz == null ? string.Empty : tz.Type,
                SelfExclusionPeriod = sep == null ? "1" : sep.Href,
                ErrorDisplayTime = edp == null ? "5000" : edp.Href,
                AccountTemplateType = att == null ? "1" : att.Href,
                SportOpenMode = som == null ? "frame" : som.Href,
                ShowLogoutInfoPopup = slp == null ? "0" : slp.Href,
                ShowLastLoginInfoPopup = sllp == null ? "0" : sllp.Href,
                PassRegEx = partner.PasswordRegExp,
                CheckPortrait = cp == null ? "0" : "1",
                WebFragments = wf?.Where(x => !x.OpenInRouting).GroupBy(x => x.Title.Split('_')[0])
                .ToDictionary(k => k.Key, v => v.GroupBy(x => x.Type).Select(x => new
                {
                    Position = x.Key,
                    Items = x.Select(z => new { z.Id, z.Order, z.Title, z.Href })
                })),
                MobileFragments = mf?.Where(x => !x.OpenInRouting).GroupBy(x => x.Title.Split('_')[0])
                .ToDictionary(k => k.Key, v => v.GroupBy(x => x.Type).Select(x => new
                {
                    Position = x.Key,
                    Items = x.Select(z => new { z.Id, z.Order, z.Title, z.Href })
                })),
                WinnersWidget = (ww != null && ww.Href == "0") ? 0 : 1,
                RegExProperty = new RegExProperty(partner.PasswordRegExp),
                WebSlideMode = wsm != null ? wsm.Href : "btt",
                VerificationKeyFormat = !string.IsNullOrEmpty(partnerSetting) && partnerSetting == "1",
                FormValidationType = fvt != null ? fvt.Href : "blur",
                DefaultMode = dm != null ? dm.Href : "dark",
				CharactersEnabled = che != null && che.Href != "0",
                AllowCancelConfirmedWithdraw = accw != null && accw.Href != "0"
			};

            var manifest = new
            {
                background_color = "#19212c",
                description = partner.Name,
                display = "standalone",
                icons = new List<object> {
                new
                {
                    src = "../../assets/images/a2hs.png",
                    sizes = "192x192",
                    type = "image/png",
                }},
                name = partner.Name,
                short_name = partner.Name,
                start_url = "../../index.html"
            };

            UploadFile(JsonConvert.SerializeObject(config), "/coreplatform/website/" + partner.Name.ToLower() + "/assets/json/config.json", ftpInput);
            UploadFile(JsonConvert.SerializeObject(manifest), "/coreplatform/website/" + partner.Name.ToLower() + "/assets/json/manifest.json", ftpInput);

            UploadFile(string.Format("window.VERSION = {0};", new Random().Next(1, 1000)), "/coreplatform/website/" + partner.Name.ToLower() + "/assets/js/version.js", ftpInput);
        }

        public void GenerateAssets(int partnerId, FtpModel ftpInput)
        {
            CheckPermission(Constants.Permissions.EditConfig);
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            if (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleObjects.All(x => x != partnerId))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            var partner = CacheManager.GetPartnerById(partnerId);
            var webSiteJs = Db.WebSiteMenus.Where(x => x.PartnerId == partnerId && x.Type == Constants.WebSiteConfiguration.Assets)
                .Select(x => new BllMenu
                {
                    Id = x.Id,
                    Type = x.Type,
                    StyleType = x.StyleType,
                    Items = x.WebSiteMenuItems.Select(y => new BllMenuItem
                    {
                        Id = y.Id,
                        Icon = y.Icon,
                        Title = y.Title,
                        Type = y.Type,
                        Href = y.Href,
                        OpenInRouting = y.OpenInRouting,
                        Orientation = y.Orientation,
                        Order = y.Order,
                        SubMenu = y.WebSiteSubMenuItems.Select(z => new BllSubMenuItem
                        {
                            Id = z.Id,
                            Icon = z.Icon,
                            Title = z.Title,
                            Type = z.Type,
                            Href = z.Href,
                            OpenInRouting = z.OpenInRouting,
                            Order = z.Order
                        }).OrderBy(z => z.Order).ToList()
                    }).OrderBy(y => y.Order).ToList()
                }).FirstOrDefault();

            var jses = webSiteJs?.Items.FirstOrDefault(x => x.Title == "Js");
            if (jses != null)
            {
                foreach (var js in jses.SubMenu)
                {
                    UploadFile(js.Href, "/coreplatform/website/" + partner.Name.ToLower() + "/assets/js/" + js.Title + ".js", ftpInput);
                }
                UploadFile(string.Format("window.VERSION = {0};", (new Random()).Next(1, 1000)), "/coreplatform/website/" + partner.Name.ToLower() + "/assets/js/version.js", ftpInput);
            }
        }

        public void GeneratePartnerSettings(int partnerId, FtpModel ftpInput)
        {
            CheckPermission(Constants.Permissions.EditPartnerProductSetting);
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            if (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleObjects.All(x => x != partnerId))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            var partner = CacheManager.GetPartnerById(partnerId);
            var registrationKYCTypes = CacheManager.GetConfigParameters(partner.Id, Constants.PartnerKeys.RegistrationKYCTypes);
            var menu = Db.WebSiteMenus.Where(x => x.PartnerId == partnerId &&
               x.Type != Constants.WebSiteConfiguration.Translations && x.Type != Constants.WebSiteConfiguration.Styles &&
               x.Type != Constants.WebSiteConfiguration.News && x.Type != Constants.WebSiteConfiguration.Config)
               .Select(x => new BllMenu
               {
                   Id = x.Id,
                   Type = x.Type,
                   StyleType = x.StyleType,
                   Items = x.WebSiteMenuItems.Select(y => new BllMenuItem
                   {
                       Id = y.Id,
                       Icon = y.Icon,
                       Title = y.Title,
                       Type = y.Type,
                       StyleType = y.StyleType,
                       Href = y.Href,
                       OpenInRouting = y.OpenInRouting,
                       Orientation = y.Orientation,
                       Order = y.Order,
                       SubMenu = y.WebSiteSubMenuItems.Select(z => new BllSubMenuItem
                       {
                           Id = z.Id,
                           Icon = z.Icon,
                           Title = z.Title,
                           Type = z.Type,
                           StyleType = z.StyleType,
                           Href = z.Href,
                           OpenInRouting = z.OpenInRouting,
                           Order = z.Order
                       }).OrderBy(z => z.Order).ToList()
                   }).OrderBy(y => y.Order).ToList()
               }).ToList();

            var cm = menu.FirstOrDefault(x => x.Type == Constants.WebSiteConfiguration.CasinoMenu);
            foreach (var cmi in cm?.Items)
            {
                if (Int32.TryParse(cmi.Type, out int href))
                    cmi.Href = cmi.Type;
            }

            var documentTypes = JsonConvert.SerializeObject(BaseBll.GetEnumerations(typeof(KYCDocumentTypes).Name, Constants.DefaultLanguageId)
                                                            .Where(x => registrationKYCTypes.ContainsKey(x.Value.ToString()))
                .Select(x => new
                {
                    Id = x.Value,
                    Value = x.Text,
                    Country = registrationKYCTypes[x.Value.ToString()]
                }).ToList());


            menu.AddRange(Db.WebSiteMenus.Where(x => x.PartnerId == partnerId && x.Type == Constants.WebSiteConfiguration.Config)
               .Select(x => new BllMenu
               {
                   Id = x.Id,
                   Type = Constants.WebSiteConfiguration.Registration,
                   StyleType = x.StyleType,
                   Items = x.WebSiteMenuItems.Where(y => y.Title == "FullRegister" || y.Title == "QuickRegister").Select(y => new BllMenuItem
                   {
                       Id = y.Id,
                       Icon = y.Icon,
                       Title = y.Title,
                       Type = y.Type,
                       StyleType = y.StyleType,
                       Href = y.Href,
                       OpenInRouting = y.OpenInRouting,
                       Orientation = y.Orientation,
                       Order = y.Order,
                       SubMenu = y.WebSiteSubMenuItems.Select(z => new BllSubMenuItem
                       {
                           Id = z.Id,
                           Icon = z.Icon,
                           Title = z.Title,
                           Type = z.Type,
                           Href = z.Href,
                           OpenInRouting = z.OpenInRouting,
                           Order = z.Order,
                           StyleType = z.Title == Constants.WebSiteConfiguration.DocumentType ? documentTypes : null
                       }).OrderBy(z => z.Order).ToList()
                   }).OrderBy(y => y.Order).ToList()
               }).ToList());

            var result = new BllPartnerSettings
            {
                MenuList = menu
            };

            var input = new
            {
                MenuList = result.MenuList.Select(x => x.ToApiMenu()).ToList()
            };

            UploadFile(JsonConvert.SerializeObject(input), "/coreplatform/website/" + partner.Name.ToLower() + "/assets/json/menu.json", ftpInput);
            UploadFile(string.Format("window.VERSION = {0};", new Random().Next(1, 1000)), "/coreplatform/website/" + partner.Name.ToLower() + "/assets/js/version.js", ftpInput);
        }

        private void GenerateWebSiteTranslations(int partnerId, string languageId, FtpModel ftpInput)
        {
            string content = string.Empty;
            var currentVersion = CacheManager.GetConfigKey(partnerId, Constants.PartnerKeys.TermsConditionVersion);
            Dictionary<string, fnTranslation> pages;
            var translations = Db.WebSiteMenuItems.Where(x => x.WebSiteMenu.PartnerId == partnerId && x.WebSiteMenu.Type == Constants.WebSiteConfiguration.Translations &&
                x.Type != "promotion" && x.Type != "news" && x.Type != "page").Select(x => new
                {
                    x.Title,
                    SubMenu = x.WebSiteSubMenuItems.Where(y => !x.Title.Replace("_", "").ToLower().Contains(Constants.WebSiteConfiguration.TermsConditions.ToLower())
                   || (!string.IsNullOrEmpty(currentVersion) && y.Title == currentVersion)
                    ).Select(z => new MenuTranslationItem
                    {
                        Title = !x.Title.Replace("_", "").ToLower().Contains(Constants.WebSiteConfiguration.TermsConditions.ToLower()) ? z.Title : "Content",
                        Text = Db.TranslationEntries.Where(o => o.TranslationId == z.TranslationId && o.LanguageId == languageId).Select(o => o.Text).FirstOrDefault(),
                        DefaultTextValue = Db.TranslationEntries.Where(o => o.TranslationId == z.TranslationId && o.LanguageId == Constants.DefaultLanguageId).Select(o => o.Text).FirstOrDefault()
                    })
                }).ToList();

            var defaultTranslations = Db.WebSiteMenuItems.Where(x => x.WebSiteMenu.PartnerId == 1 &&
                                                                     x.WebSiteMenu.Type == Constants.WebSiteConfiguration.Translations &&
                                                                     x.Type != "promotion" && x.Type != "news" && x.Type != "page").Select(x => new
                                                                     {
                                                                         x.Title,
                                                                         SubMenu = x.WebSiteSubMenuItems.Where(y => !x.Title.Replace("_", "").ToLower().Contains(Constants.WebSiteConfiguration.TermsConditions.ToLower())
                                                                        || (!string.IsNullOrEmpty(currentVersion) && y.Title == currentVersion)
               ).Select(z => new
               {
                   Title = !x.Title.Replace("_", "").ToLower().Contains(Constants.WebSiteConfiguration.TermsConditions.ToLower()) ?
                   z.Title : "Content",
                   Value = Db.fn_Translation(languageId).Where(o => o.TranslationId == z.TranslationId).FirstOrDefault()
               })
                                                                     }).AsEnumerable().ToDictionary(y => y.Title, y => y.SubMenu.ToDictionary(z => z.Title, (z => z.Value == null ? string.Empty : z.Value.Text)));

            foreach (var t in translations)
            {
                var contains = defaultTranslations.ContainsKey(t.Title);
                foreach (var subT in t.SubMenu)
                {
                    if (string.IsNullOrEmpty(subT.Text))
                    {
                        if (contains && defaultTranslations[t.Title].ContainsKey(subT.Title))
                        {
                            subT.Text = defaultTranslations[t.Title][subT.Title] == "#" ? string.Empty : defaultTranslations[t.Title][subT.Title];
                        }
                        else
                            subT.Text = subT.DefaultTextValue == "#" ? string.Empty : subT.DefaultTextValue;
                    }
                    else if (subT.Text == "#")
                        subT.Text = string.Empty;
                }
            }
            pages = Db.WebSiteMenuItems.Include(x => x.WebSiteSubMenuItems).FirstOrDefault(x => x.WebSiteMenu.PartnerId == partnerId && x.WebSiteMenu.Type == Constants.WebSiteConfiguration.Translations && x.Type == "page")?.
                WebSiteSubMenuItems.Select(x => new
                {
                    PageName = x.Title,
                    Content = Db.fn_Translation(languageId).
                    Where(o => o.TranslationId == x.TranslationId).FirstOrDefault()
                }).ToDictionary(y => y.PageName, y => y.Content);
            var partner = CacheManager.GetPartnerById(partnerId);

            content = JsonConvert.SerializeObject(translations.ToDictionary(y => y.Title,
                                               y => y.SubMenu.ToDictionary(z => z.Title,
                                                                          (z => z.Text == null ? string.Empty : z.Text))));
            UploadFile(content, "/coreplatform/website/" + partner.Name.ToLower() + "/assets/json/translations/" + languageId.ToLower() + ".json", ftpInput);
            if (pages != null)
                foreach (var page in pages)
                {
                    UploadFile(page.Value.Text, "/coreplatform/website/" + partner.Name.ToLower() + "/assets/html/" + page.Key + "_" + languageId.ToLower() + ".html", ftpInput);
                }
            UploadFile(string.Format("window.VERSION = {0};", (new Random()).Next(1, 1000)), "/coreplatform/website/" + partner.Name.ToLower() + "/assets/js/version.js", ftpInput);
        }

        private void GenerateAdminTranslations(string languageId, FtpModel ftpInput)
        {
            var translations = Db.WebSiteMenuItems.Where(x => x.WebSiteMenu.PartnerId == Constants.MainPartnerId &&
                x.WebSiteMenu.Type == Constants.WebSiteConfiguration.Translations).Select(x => new
                {
                    x.Title,
                    SubMenu = x.WebSiteSubMenuItems.Select(z => new MenuTranslationItem
                    {
                        Title = z.Title,
                        Text = Db.TranslationEntries.Where(o => o.TranslationId == z.TranslationId && o.LanguageId == languageId).Select(o => o.Text).FirstOrDefault(),
                        DefaultTextValue = Db.TranslationEntries.Where(o => o.TranslationId == z.TranslationId &&
                            o.LanguageId == Constants.DefaultLanguageId).Select(o => o.Text).FirstOrDefault()
                    })
                }).ToList();

            var content = JsonConvert.SerializeObject(translations.ToDictionary(y => y.Title,
                                                                                y => y.SubMenu.ToDictionary(z => z.Title, z => z.Text ?? string.Empty)));
            UploadFile(content, "/managementsystem/assets/i18n/" + languageId.ToLower() + ".json", ftpInput);
        }

        public void GenerateWebSiteAllTranslations(int partnerId, FtpModel ftpInput)
        {
            CheckPermission(Constants.Permissions.EditWebSiteMenuTranslationEntry);
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            if (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleObjects.All(x => x != partnerId))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);

            var partner = CacheManager.GetPartnerById(partnerId);
            var languages = Db.PartnerLanguageSettings.Where(x => x.PartnerId == partnerId && x.State == (int)PartnerLanguageStates.Active).Select(x => x.LanguageId).ToList();
            foreach (var lang in languages)
            {
                GenerateWebSiteTranslations(partnerId, lang, ftpInput);
            }
        }

        public void GenerateAdminTranslations(FtpModel ftpInput)
        {
            CheckPermission(Constants.Permissions.EditAdminTranslation);

            var languages = Db.PartnerLanguageSettings.Where(x => x.PartnerId == Constants.MainPartnerId && x.State == (int)PartnerLanguageStates.Active)
                                                      .Select(x => x.LanguageId).ToList();
            foreach (var lang in languages)
            {
                GenerateAdminTranslations(lang, ftpInput);
            }
        }

        public void GenerateWebSiteAllPromotions(int partnerId, FtpModel ftpInput)
        {
            CheckPermission(Constants.Permissions.EditPromotions);
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            if (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleObjects.All(x => x != partnerId))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            var languages = Db.PartnerLanguageSettings.Where(x => x.PartnerId == partnerId && x.State == (int)PartnerLanguageStates.Active)
                                      .Select(x => x.LanguageId).ToList();
            //Parallel.ForEach(languages, lang =>
            //{
            //    GenerateWebSitePromotions(partnerId, lang, ftpInput);
            //});
            foreach(var lang in languages)
                GenerateWebSitePromotions(partnerId, lang, ftpInput);
        }

        private void GenerateWebSitePromotions(int partnerId, string languageId, FtpModel ftpInput)
        {
            var partner = CacheManager.GetPartnerById(partnerId);
            var pathTemplate = "/assets/json/promotions/{0}_{1}.json";
            var pts = CacheManager.GetEnumerations(nameof(PromotionTypes), languageId);
            Db.fn_Promotion(languageId, true).Where(x => x.PartnerId == partnerId && x.State == (int)BaseStates.Active)
                                                                .Select(x => new PromotionItem
                                                                {
                                                                    id = x.Id,
                                                                    type = x.Type.ToString(),
                                                                    image = x.ImageName,
                                                                    content = x.Content,
                                                                    description = x.Description,
                                                                    title = x.Title
                                                                }).ToList().ForEach(p =>
                                                                {
                                                                    p.type = pts.FirstOrDefault(y => y.Value == Convert.ToUInt32(p.type)).Text;
                                                                    UploadFile(JsonConvert.SerializeObject(p), "/coreplatform/website/" + partner.Name.ToLower() + String.Format(pathTemplate, p.id, languageId.ToLower()), ftpInput);
                                                                });
            UploadFile($"window.VERSION = {new Random().Next(1, 1000)};", $"/coreplatform/website/{partner.Name.ToLower()}/assets/js/version.js", ftpInput);
        }

        public void GenerateWebSiteAllNews(int partnerId, FtpModel ftpInput)
        {
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.EditNews
            });
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            if (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleObjects.All(x => x != partnerId))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            var partner = CacheManager.GetPartnerById(partnerId);
            var languages = Db.PartnerLanguageSettings.Where(x => x.PartnerId == partnerId && x.State == (int)PartnerLanguageStates.Active).Select(x => x.LanguageId).ToList();
            foreach (var lang in languages)
            {
                GenerateWebSiteNews(partnerId, lang, ftpInput);
            }
        }

        private void GenerateWebSiteNews(int partnerId, string languageId, FtpModel ftpInput)
        {
            string content = string.Empty;
            Dictionary<string, string> contents = new Dictionary<string, string>();
            using (var db = new IqSoftCorePlatformEntities())
            {
                var translations = Db.WebSiteMenuItems.Where(x => x.WebSiteMenu.PartnerId == partnerId && x.WebSiteMenu.Type == Constants.WebSiteConfiguration.Translations &&
                    x.Type == "news").Select(x => new
                    {
                        x.Title,
                        SubMenu = x.WebSiteSubMenuItems.Select(z => new
                        {
                            z.Title,
                            Value = db.fn_Translation(languageId).Where(o => o.TranslationId == z.TranslationId).FirstOrDefault()
                        })
                    }).ToList();

                var items = Db.WebSiteMenuItems.Include(x => x.WebSiteSubMenuItems).Where(x => x.WebSiteMenu.PartnerId == partnerId &&
                x.WebSiteMenu.Type == Constants.WebSiteConfiguration.News).OrderBy(x => x.Order).ToList();
                var contentItems = new List<object>();
                foreach (var item in items)
                {
                    var translation = translations.FirstOrDefault(x => x.Title == item.Title);
                    var translationItems = translation?.SubMenu;

                    var title = item.WebSiteSubMenuItems.FirstOrDefault(x => x.Title == "Title");
                    var description = item.WebSiteSubMenuItems.FirstOrDefault(x => x.Title == "Description");
                    var cnt = item.WebSiteSubMenuItems.FirstOrDefault(x => x.Title == "Content");
                    var t_title = translationItems?.FirstOrDefault(x => x.Title == "Title");
                    var t_description = translationItems?.FirstOrDefault(x => x.Title == "Description");
                    var t_cnt = translationItems?.FirstOrDefault(x => x.Title == "Content");

                    contentItems.Add(new
                    {
                        id = item.Id,
                        image = item.Icon,
                        date = item.Type,
                        title = (t_title == null ? string.Empty : t_title.Value.Text),
                        description = (t_description == null ? string.Empty : t_description.Value.Text)
                    });
                    contents.Add("/assets/json/news/" + item.Id + "_" + languageId.ToLower() + ".json", JsonConvert.SerializeObject(
                    new
                    {
                        id = item.Id,
                        image = item.Icon,
                        date = item.Type,
                        title = (t_title == null ? string.Empty : t_title.Value.Text),
                        description = (t_description == null ? string.Empty : t_description.Value.Text),
                        content = (t_cnt == null ? string.Empty : t_cnt.Value.Text)
                    }));
                }
                content = JsonConvert.SerializeObject(contentItems);
            }
            var partner = CacheManager.GetPartnerById(partnerId);

            UploadFile(content, "/coreplatform/website/" + partner.Name.ToLower() + "/assets/json/news/list_" + languageId.ToLower() + ".json", ftpInput);
            foreach (var c in contents)
            {
                UploadFile(c.Value, "/coreplatform/website/" + partner.Name.ToLower() + c.Key, ftpInput);
            }
            UploadFile(string.Format("window.VERSION = {0};", (new Random()).Next(1, 1000)), "/coreplatform/website/" + partner.Name.ToLower() + "/assets/js/version.js", ftpInput);
        }

        private void UploadMenuImage(int partnerId, string imageName, string image, string menuItemName, 
            string submenuItemName, string menuType)
        {
            using (var partnerBl = new PartnerBll(Identity, Log))
            {
                var partner = CacheManager.GetPartnerById(partnerId);
                if (partner == null)
                    throw BaseBll.CreateException(LanguageId, Constants.Errors.PartnerNotFound);
                var ftpModel = partnerBl.GetPartnerEnvironments(partnerId).First();

                var path = "ftp://" + ftpModel.Value.Url + "/coreplatform/website/" + partner.Name + "/assets/";
                if (menuItemName.ToLower() == "fonts")
                    path += "fonts/" + imageName;
                else
                    path += "images/";

                if (menuItemName.StartsWith("Images"))
                    path += (menuItemName.ToLower() != "images" ? menuItemName.Split('_')[1].ToLower() + "/" + imageName : imageName);
                else if (menuType == Constants.WebSiteConfiguration.AccountTabsList)
                    path += ("account-tabs-list/" + imageName);
                else if (menuType == Constants.WebSiteConfiguration.HeaderPanel1Menu)
                    path += ("header-panel-1-menu/" + imageName);
                else if (menuType == Constants.WebSiteConfiguration.CasinoMenu)
                    path += ("casino-menu/" + imageName);
                else if (menuType == Constants.WebSiteConfiguration.FooterMenu)
                    path += ("footer-menu/" + imageName);
                else if (menuType == Constants.WebSiteConfiguration.HomeMenu)
                    path += ("home/home-menu/" + imageName);
                else if (menuType == Constants.WebSiteConfiguration.MobileHomeMenu)
                    path += ("home/home-menu/" + imageName);
                else if (menuType == Constants.WebSiteConfiguration.MobileCentralMenu)
                    path += ("mobile-central-menu/" + imageName);
                else if (menuType == Constants.WebSiteConfiguration.MobileFooterMenu)
                    path += ("mobile-footer-menu/" + imageName);
                else if (menuType == Constants.WebSiteConfiguration.MobileBottomMenu)
                    path += ("mobile-bottom-menu/" + imageName);
                else if (menuType == Constants.WebSiteConfiguration.MobileMenu)
                    path += ("mobile-menu/" + imageName);
                else if (menuType == Constants.WebSiteConfiguration.MobileRightSidebar)
                    path += ("mobile-right-sidebar/" + imageName);
                else if (menuType == Constants.WebSiteConfiguration.MobileHeaderPanel)
                    path += ("mobile_header_panel/" + imageName);
                else if (menuType == Constants.WebSiteConfiguration.News)
                {
                    if (submenuItemName == "Title")
                        path += ("news/small/" + imageName);
                    else if (submenuItemName == "Description")
                        path += ("news/medium/" + imageName);
                    else if (submenuItemName == "Content")
                        path += ("news/" + imageName);
                }
                byte[] bytes = Convert.FromBase64String(image);
                UploadFtpImage(bytes, ftpModel.Value, path);
            }
        }

        #endregion

        public CRMSetting SaveCRMSetting(CRMSetting setting)
        {
            var triggerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.EditCRMSetting
            });
            var partnersAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            if (!triggerAccess.HaveAccessForAllObjects ||
                (!partnersAccess.HaveAccessForAllObjects && partnersAccess.AccessibleObjects.All(x => x != setting.PartnerId)))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            if (setting.Type == (int)CRMSettingTypes.MissedDeposit && setting.Sequence == null && !Int32.TryParse(setting.Condition, out int condition))
                throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);

            if (setting.Id == 0)
            {
                if (!Enum.IsDefined(typeof(CRMSettingTypes), setting.Type))
                    throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);
                Db.CRMSettings.Add(setting);
                Db.SaveChanges();
                return setting;
            }
            else
            {
                var dbSetting = Db.CRMSettings.FirstOrDefault(x => x.Id == setting.Id);
                dbSetting.NickeName = setting.NickeName;
                dbSetting.StartTime = setting.StartTime;
                dbSetting.FinishTime = setting.FinishTime;
                dbSetting.State = setting.State;
                dbSetting.Condition = setting.Condition;
                dbSetting.Sequence = setting.Sequence;
                Db.SaveChanges();
                return dbSetting;
            }
        }

        public List<CRMSetting> GetCRMSettings()
        {
            var triggerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewCRMSetting
            });
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            if (!triggerAccess.HaveAccessForAllObjects)
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            if (!partnerAccess.HaveAccessForAllObjects)
                return Db.CRMSettings.Where(x => partnerAccess.AccessibleObjects.Contains(x.PartnerId)).OrderByDescending(x => x.Id).ToList();
            return Db.CRMSettings.OrderByDescending(x => x.Id).ToList();
        }

        public CRMSetting GetCRMSettingById(int settingId)
        {
            var triggerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewCRMSetting
            });
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });

            if (!triggerAccess.HaveAccessForAllObjects)
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            var res = Db.CRMSettings.FirstOrDefault(x => x.Id == settingId &&
            (partnerAccess.HaveAccessForAllObjects || partnerAccess.AccessibleObjects.Contains(x.PartnerId)));
            if (res == null)
                throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);
            return res;
        }

        public List<MessageTemplate> GetMessageTemplates()
        {
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            CheckPermission(Constants.Permissions.ViewPartnerMessageTemplate);

            var query = Db.MessageTemplates.Where(x => x.State != (int)MessageTemplateStates.Deleted);
            if (!partnerAccess.HaveAccessForAllObjects)
                query = query.Where(x => partnerAccess.AccessibleObjects.Contains(x.PartnerId));
            return query.ToList();
        }

        public MessageTemplate SaveMessageTemplate(MessageTemplate messageTemplate)
        {
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            if (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleObjects.All(x => x != messageTemplate.PartnerId))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            CheckPermission(Constants.Permissions.EditPartnerMessageTemplate);

            if (messageTemplate.Id == 0)
            {
                var t = CreateTranslation(new fnTranslation { LanguageId = Constants.DefaultLanguageId, ObjectTypeId = (int)ObjectTypes.MessageTemplate, Text = messageTemplate.NickName });
                Db.Translations.Add(t);
                Db.SaveChanges();
                messageTemplate.TranslationId = t.Id;
                messageTemplate.State = (int)MessageTemplateStates.Active;
                Db.MessageTemplates.Add(messageTemplate);
                Db.SaveChanges();
                return messageTemplate;
            }
            var dbMessageTemplate = Db.MessageTemplates.FirstOrDefault(x => x.Id == messageTemplate.Id) ??
                throw CreateException(LanguageId, Constants.Errors.MessageNotFound);
            if (!Enum.IsDefined(typeof(MessageTemplateStates), messageTemplate.State))
                throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);
            dbMessageTemplate.NickName = messageTemplate.NickName;
            dbMessageTemplate.ClientInfoType = messageTemplate.ClientInfoType;
            dbMessageTemplate.ExternalTemplateId = messageTemplate.ExternalTemplateId;
            dbMessageTemplate.State = messageTemplate.State;
            Db.SaveChanges();
            CacheManager.RemoveKeysFromCache(string.Format("{0}_{1}_{2}_", Constants.CacheItems.MessageTemplates, messageTemplate.PartnerId, messageTemplate.ClientInfoType));
            return dbMessageTemplate;
        }
        public void RemoveMessageTemplate(int templateId, out int partnerId)
        {
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.EditPartnerMessageTemplate,
                ObjectTypeId = ObjectTypes.MessageTemplate
            });
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            var dbMessageTemplate = Db.MessageTemplates.FirstOrDefault(x => x.Id == templateId);
            if (dbMessageTemplate == null)
                throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);

            partnerId = dbMessageTemplate.PartnerId;

            if (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleObjects.All(x => x != dbMessageTemplate.PartnerId))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);

            dbMessageTemplate.State = (int)MessageTemplateStates.Deleted;
            Db.SaveChanges();
        }

        public void SaveAnnouncement(ApiAnnouncement apiAnnouncement, bool isFromAdmin)
        {

            var partner = CacheManager.GetPartnerById(apiAnnouncement.PartnerId) ??
                throw BaseBll.CreateException(LanguageId, Constants.Errors.PartnerNotFound);

            if (isFromAdmin)
            {
                var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewPartner,
                    ObjectTypeId = ObjectTypes.Partner
                });
                if (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleObjects.All(x => x != apiAnnouncement.PartnerId))
                    throw CreateException(LanguageId, Constants.Errors.DontHavePermission);

                CheckPermission(Constants.Permissions.ViewAnnouncement);
                CheckPermission(Constants.Permissions.EditAnnouncement);
            }
            var currentDate = DateTime.UtcNow;
            if (!Enum.IsDefined(typeof(AnnouncementTypes), apiAnnouncement.Type) ||
                !Enum.IsDefined(typeof(AnnouncementReceiverTypes), apiAnnouncement.ReceiverType) ||
                !Enum.IsDefined(typeof(BaseStates), apiAnnouncement.State) ||
                (apiAnnouncement.ReceiverType == (int)AnnouncementReceiverTypes.Client && apiAnnouncement.UserIds != null && apiAnnouncement.UserIds.Any()) ||
                (apiAnnouncement.ReceiverType != (int)AnnouncementReceiverTypes.Client && 
                ((apiAnnouncement.ClientIds != null && apiAnnouncement.ClientIds.Any()) ||(apiAnnouncement.SegmentIds != null && apiAnnouncement.SegmentIds.Any()))))
                throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);

            if (apiAnnouncement.ClientIds!= null && apiAnnouncement.ClientIds.Any())
            {
                var clients = Db.Clients.Where(x => x.PartnerId == apiAnnouncement.PartnerId && apiAnnouncement.ClientIds.Contains(x.Id))
                                        .Select(x => x.Id).ToList();
                if (apiAnnouncement.ClientIds.Any(x => !clients.Contains(x)))
                    throw CreateException(LanguageId, Constants.Errors.ClientNotFound);
            }
            if (apiAnnouncement.UserIds != null && apiAnnouncement.UserIds.Any())
            {
                var users = Db.Users.Where(x => x.PartnerId == apiAnnouncement.PartnerId && apiAnnouncement.UserIds.Contains(x.Id))
                                        .Select(x => x.Id).ToList();
                if (apiAnnouncement.UserIds.Any(x => !users.Contains(x)))
                    throw CreateException(LanguageId, Constants.Errors.UserNotFound);
            }
            if (apiAnnouncement.SegmentIds != null && apiAnnouncement.SegmentIds.Any())
            {
                var segments = Db.Segments.Where(x => x.PartnerId == apiAnnouncement.PartnerId && apiAnnouncement.SegmentIds.Contains(x.Id) && 
                                                      x.State == (int)SegmentStates.Active)
                                          .Select(x => x.Id).ToList();
                if (apiAnnouncement.SegmentIds.Any(x => !segments.Contains(x)))
                    throw CreateException(LanguageId, Constants.Errors.SegmentNotFound);
            }
            Announcement dbAnnouncement = null;
            using (var scope = CommonFunctions.CreateTransactionScope())
            {
                if (apiAnnouncement.Id.HasValue)
                {
                    dbAnnouncement = Db.Announcements.Include(x => x.AnnouncementSettings).Where(x => x.Id == apiAnnouncement.Id).FirstOrDefault() ??
                       throw CreateException(LanguageId, Constants.Errors.AnnouncementNotFound);
                    var oldAnnouncement = new
                    {
                        dbAnnouncement.Id,
                        dbAnnouncement.PartnerId,
                        dbAnnouncement.Type,
                        dbAnnouncement.ReceiverType,
                        dbAnnouncement.State,
                        dbAnnouncement.NickName,
                        dbAnnouncement.UserId,
                        dbAnnouncement.CreationDate,
                        dbAnnouncement.LastUpdateDate,
                        ClientIds = dbAnnouncement.AnnouncementSettings.Where(x => x.ObjectTypeId==(int)ObjectTypes.Client).Select(x => x.ObjectId).ToList(),
                        UserIds = dbAnnouncement.AnnouncementSettings.Where(x => x.ObjectTypeId==(int)ObjectTypes.User).Select(x => x.ObjectId).ToList(),
                        SegmentIds = dbAnnouncement.AnnouncementSettings.Where(x => x.ObjectTypeId==(int)ObjectTypes.Segment).Select(x => x.ObjectId).ToList()
                    };
                    SaveChangesWithHistory((int)ObjectTypes.Announcement, dbAnnouncement.Id, JsonConvert.SerializeObject(oldAnnouncement));
                    dbAnnouncement.NickName = apiAnnouncement.NickName;
                    dbAnnouncement.State = apiAnnouncement.State;
                    dbAnnouncement.LastUpdateDate = currentDate;
                    Db.SaveChanges();
                    apiAnnouncement.ClientIds = apiAnnouncement.ClientIds ?? new List<int>();
                    apiAnnouncement.UserIds = apiAnnouncement.UserIds ?? new List<int>();
                    apiAnnouncement.SegmentIds = apiAnnouncement.SegmentIds ?? new List<int>();
                    var deleteQuery = Db.AnnouncementSettings.Where(x => x.AnnouncementId == apiAnnouncement.Id.Value &&
                    ((x.ObjectTypeId == (int)ObjectTypes.Client && !apiAnnouncement.ClientIds.Contains(x.ObjectId)) ||
                    (x.ObjectTypeId == (int)ObjectTypes.User && !apiAnnouncement.UserIds.Contains(x.ObjectId)) ||
                    (x.ObjectTypeId == (int)ObjectTypes.Segment &&  !apiAnnouncement.SegmentIds.Contains(x.ObjectId)))).DeleteFromQuery();
                }
                else
                {
                    var date = currentDate.Year * 1000000 + currentDate.Month * 10000 + currentDate.Day * 100 + currentDate.Hour;
                    dbAnnouncement = new Announcement
                    {
                        PartnerId = apiAnnouncement.PartnerId,
                        UserId = Identity.Id,
                        State = apiAnnouncement.State,
                        Type = apiAnnouncement.Type,
                        ReceiverType = apiAnnouncement.ReceiverType,
                        NickName = apiAnnouncement.NickName,
                        CreationDate = currentDate,
                        LastUpdateDate = currentDate,
                        Date = date,
                        Translation = CreateTranslation(new fnTranslation
                        {
                            LanguageId = Constants.DefaultLanguageId,
                            ObjectTypeId = (int)ObjectTypes.Announcement,
                            Text = apiAnnouncement.NickName
                        })
                    };
                    dbAnnouncement = Db.Announcements.Add(dbAnnouncement);
                }
                Db.SaveChanges();
                if (apiAnnouncement.ClientIds!=null)
                {
                    var dbClientSettings = Db.AnnouncementSettings.Where(x => x.AnnouncementId==dbAnnouncement.Id && x.ObjectTypeId == (int)ObjectTypes.Client &&
                                                                             apiAnnouncement.ClientIds.Contains(x.ObjectId)).Select(x => x.ObjectId).ToList();
                    apiAnnouncement.ClientIds.Where(x => !dbClientSettings.Contains(x)).ToList().ForEach(c =>
                    {
                        Db.AnnouncementSettings.Add(new AnnouncementSetting { AnnouncementId = dbAnnouncement.Id, ObjectTypeId =(int)ObjectTypes.Client, ObjectId = c });
                    });
                }
                if (apiAnnouncement.UserIds!=null)
                {
                    var dbUserSettings = Db.AnnouncementSettings.Where(x => x.AnnouncementId==dbAnnouncement.Id && x.ObjectTypeId == (int)ObjectTypes.User &&
                                                                             apiAnnouncement.UserIds.Contains(x.ObjectId)).Select(x => x.ObjectId).ToList();
                    apiAnnouncement.UserIds.Where(x => !dbUserSettings.Contains(x)).ToList().ForEach(u =>
                    {
                        Db.AnnouncementSettings.Add(new AnnouncementSetting { AnnouncementId = dbAnnouncement.Id, ObjectTypeId =(int)ObjectTypes.User, ObjectId = u });
                    });
                }
                if (apiAnnouncement.SegmentIds!=null)
                {
                    var dbSegmentSettings = Db.AnnouncementSettings.Where(x => x.AnnouncementId==dbAnnouncement.Id && x.ObjectTypeId == (int)ObjectTypes.Segment &&
                                                                             apiAnnouncement.SegmentIds.Contains(x.ObjectId)).Select(x => x.ObjectId).ToList();
                    apiAnnouncement.SegmentIds.Where(x => !dbSegmentSettings.Contains(x)).ToList().ForEach(s =>
                    {
                        Db.AnnouncementSettings.Add(new AnnouncementSetting { AnnouncementId = dbAnnouncement.Id, ObjectTypeId =(int)ObjectTypes.Segment, ObjectId = s });
                    });
                }
                Db.SaveChanges();
                scope.Complete();
                if (dbAnnouncement.Type == (int)AnnouncementTypes.Ticker)
                    CacheManager.RemoveKeysFromCache(string.Format("{0}_{1}_", Constants.CacheItems.Ticker, dbAnnouncement.PartnerId));
            }

        }

        public ApiAnnouncement GetAnnouncementById(int announcementId)
        {
            var dbAnnouncement = Db.Announcements.Include(x => x.AnnouncementSettings).FirstOrDefault(x => x.Id == announcementId) ??
                throw BaseBll.CreateException(LanguageId, Constants.Errors.AnnouncementNotFound);
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            if (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleObjects.All(x => x != dbAnnouncement.PartnerId))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            CheckPermission(Constants.Permissions.ViewAnnouncement);

            return new ApiAnnouncement
            {
                Id = dbAnnouncement.Id,
                PartnerId = dbAnnouncement.PartnerId,
                Type = dbAnnouncement.Type,
                ReceiverType = dbAnnouncement.ReceiverType,
                State = dbAnnouncement.State,
                NickName = dbAnnouncement.NickName,
                UserId = dbAnnouncement.UserId ?? 0,
                CreationDate = dbAnnouncement.CreationDate,
                LastUpdateDate = dbAnnouncement.LastUpdateDate,
                ClientIds = dbAnnouncement.AnnouncementSettings.Where(x => x.ObjectTypeId==(int)ObjectTypes.Client).Select(x => x.ObjectId).ToList(),
                UserIds = dbAnnouncement.AnnouncementSettings.Where(x => x.ObjectTypeId==(int)ObjectTypes.User).Select(x => x.ObjectId).ToList(),
                SegmentIds = dbAnnouncement.AnnouncementSettings.Where(x => x.ObjectTypeId==(int)ObjectTypes.Segment).Select(x => x.ObjectId).ToList()
            };
        }

        public PagedModel<fnAnnouncement> GetAnnouncements(FilterAnnouncement filter, bool checkPermission)
        {
            if (checkPermission)
            {
                var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewPartner,
                    ObjectTypeId = ObjectTypes.Partner
                });
                CheckPermission(Constants.Permissions.ViewAnnouncement);
                filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnAnnouncement>>
                {
                    new CheckPermissionOutput<fnAnnouncement>
                    {
                        AccessibleObjects = partnerAccess.AccessibleObjects,
                        HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                        Filter = x=> partnerAccess.AccessibleObjects.Contains(x.PartnerId)
                    }
                };
            }
            return new PagedModel<fnAnnouncement>
            {
                Entities = filter.FilterObjects(Db.fn_Announcement(LanguageId), announcements => announcements.OrderByDescending(x => x.Id)).ToList(),
                Count = filter.SelectedObjectsCount(Db.fn_Announcement(LanguageId))
            };
        }

        public List<fnTranslation> SaveTranslationEntries(List<fnTranslation> translationEntries, out Dictionary<string, int> broadcastKey)
        {
            if (translationEntries.Count > 0)
            {
                switch (translationEntries[0].ObjectTypeId)
                {
                    case (int)ObjectTypes.BannerBody:
                    case (int)ObjectTypes.BannerHead:
                        GetPermissionsToObject(new CheckPermissionInput
                        {
                            Permission = Constants.Permissions.SaveBanner,
                        });
                        break;
                    case (int)ObjectTypes.Bonus:
                        GetPermissionsToObject(new CheckPermissionInput
                        {
                            Permission = Constants.Permissions.EditBonuses,
                        });
                        break;
                    case (int)ObjectTypes.WebSiteTranslation:
                        GetPermissionsToObject(new CheckPermissionInput
                        {
                            Permission = Constants.Permissions.EditWebSiteMenu,
                        });
                        GetPermissionsToObject(new CheckPermissionInput
                        {
                            Permission = Constants.Permissions.EditWebSiteMenuTranslationEntry,
                        });
                        break;
                    case (int)ObjectTypes.MessageTemplate:
                        GetPermissionsToObject(new CheckPermissionInput
                        {
                            Permission = Constants.Permissions.EditPartnerMessageTemplate,
                        });
                        break;
                    case (int)ObjectTypes.JobArea:
                        GetPermissionsToObject(new CheckPermissionInput
                        {
                            Permission = Constants.Permissions.EditJobArea,
                        });
                        break;
                    case (int)ObjectTypes.ProductCategory:
                        GetPermissionsToObject(new CheckPermissionInput
                        {
                            Permission = Constants.Permissions.EditProductCategory,
                        });
                        break;
                    case (int)ObjectTypes.Product:
                        GetPermissionsToObject(new CheckPermissionInput
                        {
                            Permission = Constants.Permissions.CreateProduct,
                        });
                        break;
                    case (int)ObjectTypes.Promotion:
                    case (int)ObjectTypes.PromotionContent:
                    case (int)ObjectTypes.PromotionDescription:
                        GetPermissionsToObject(new CheckPermissionInput
                        {
                            Permission = Constants.Permissions.EditPromotions,
                        });
                        break;
                    case (int)ObjectTypes.PartnerBank:
                        GetPermissionsToObject(new CheckPermissionInput
                        {
                            Permission = Constants.Permissions.EditPartnerBank,
                        });
                        break;
                    case (int)ObjectTypes.Announcement:
                        GetPermissionsToObject(new CheckPermissionInput
                        {
                            Permission = Constants.Permissions.EditAnnouncement,
                        });
                        break;
                    case (int)ObjectTypes.CommentTemplate:
                        GetPermissionsToObject(new CheckPermissionInput
                        {
                            Permission = Constants.Permissions.EditPartnerCommentTemplate,
                        });
                        break;
                    default:
                        GetPermissionsToObject(new CheckPermissionInput
                        {
                            Permission = Constants.Permissions.EditTranslationEntry,
                        });
                        break;

                }
            }
            var result = new List<fnTranslation>();
            broadcastKey = new Dictionary<string, int>();
            foreach (var translationEntry in translationEntries)
            {
                var translation = new fnTranslation
                {
                    LanguageId = translationEntry.LanguageId,
                    ObjectTypeId = translationEntry.ObjectTypeId,
                    Text = string.IsNullOrEmpty(translationEntry.Text) ? string.Empty : translationEntry.Text,
                    TranslationId = translationEntry.TranslationId
                };
                if (translationEntry.ObjectTypeId == (int)ObjectTypes.WebSiteTranslation)
                {
                    var webSiteItem = Db.WebSiteSubMenuItems.Include(x => x.WebSiteMenuItem.WebSiteMenu).FirstOrDefault(x => x.TranslationId == translationEntry.TranslationId);
                    if (webSiteItem.WebSiteMenuItem.Title == Constants.SubMenuConfiguration.TermsAndConditions)
                    {
                        var te = Db.TranslationEntries.FirstOrDefault(x => x.TranslationId == translationEntry.TranslationId && x.LanguageId == translation.LanguageId);
                        if (te != null && te.Text != null && te.Text != "")
                            throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
                    }
                }
                var t = SaveTranslation(translation);

                switch (translationEntry.ObjectTypeId)
                {
                    case (int)ObjectTypes.MessageTemplate:
                        var messageTemplate = Db.MessageTemplates.FirstOrDefault(x => x.TranslationId == translationEntry.TranslationId);
                        if (messageTemplate != null)
                        {
                            var lgs = Db.PartnerLanguageSettings.Where(x => x.PartnerId == messageTemplate.PartnerId && x.State == (int)PartnerLanguageStates.Active)
                                                                .Select(x => x.LanguageId).ToList();
                            foreach (var l in lgs)
                            {
                                CacheManager.RemoveMessageTemplateFromCache(messageTemplate.PartnerId, messageTemplate.ClientInfoType, l);
                                broadcastKey.Add(string.Format("{0}_{1}_{2}_{3}", Constants.CacheItems.MessageTemplates, messageTemplate.PartnerId,
                                    messageTemplate.ClientInfoType, l), messageTemplate.PartnerId);
                            }
                        }
                        break;
                    case (int)ObjectTypes.CommentTemplate:
                        var commentTemplate = Db.CommentTemplates.FirstOrDefault(x => x.TranslationId == translationEntry.TranslationId);
                        if (commentTemplate != null)
                        {
                            CacheManager.RemoveCommentTemplateFromCache(commentTemplate.PartnerId, commentTemplate.Type);
                             var lgs = Db.PartnerLanguageSettings.Where(x => x.PartnerId == commentTemplate.PartnerId && x.State == (int)PartnerLanguageStates.Active)
                                                                .Select(x => x.LanguageId).ToList();
                            foreach (var l in lgs)
                            {
                                broadcastKey.Add(string.Format("{0}_{1}_{2}_{3}", Constants.CacheItems.CommentTemplates, commentTemplate.PartnerId,
                                                                                  commentTemplate.Type, l), commentTemplate.PartnerId);
                            }
                        }
                        break;
                    case (int)ObjectTypes.WebSiteTranslation:
                        var webSiteItem = Db.WebSiteSubMenuItems.Include(x => x.WebSiteMenuItem.WebSiteMenu).FirstOrDefault(x => x.TranslationId == translationEntry.TranslationId);
                        t.PartnerId = webSiteItem.WebSiteMenuItem.WebSiteMenu.PartnerId;
                        break;
                    case (int)ObjectTypes.ErrorType:
                        CacheManager.RemovefnErrorTypes(translationEntry.LanguageId);
                        broadcastKey.Add(string.Format("{0}_{1}", Constants.CacheItems.fnErrorTypes, translationEntry.LanguageId), 0);
                        break;
                    case (int)ObjectTypes.BannerBody:
                    case (int)ObjectTypes.BannerHead:
                        var banner = Db.Banners.FirstOrDefault(x => x.BodyTranslationId == translationEntry.TranslationId ||
                                            x.HeadTranslationId == translationEntry.TranslationId);
                        CacheManager.RemoveBanners(banner.PartnerId, banner.Type, translationEntry.LanguageId);
                        broadcastKey.Add(string.Format("{0}_{1}_{2}_{3}", Constants.CacheItems.Banners, banner.PartnerId, banner.Type, translationEntry.LanguageId), banner.PartnerId);
                        break;
                    case (int)ObjectTypes.Bonus:
                        break;
                    case (int)ObjectTypes.JobArea:
                        var key = string.Format("{0}_{1}", Constants.CacheItems.JobAreas, translationEntry.LanguageId);
                        CacheManager.RemoveFromCache(key);
                        broadcastKey.Add(key, 0);
                        break;
                    case (int)ObjectTypes.ProductCategory:
                        var pcPartnerIds = Db.Partners.Select(x => x.Id).ToList();
                        foreach (var id in pcPartnerIds)
                        {
                            var pcKey = string.Format("{0}_{1}_{2}", Constants.CacheItems.PartnerProductCategories, id, translationEntry.LanguageId);
                            broadcastKey.Add(pcKey, id);
                            CacheManager.RemoveProductCategories(id, translationEntry.LanguageId);
                        }
                        break;
                    case (int)ObjectTypes.Product:
                        var product = Db.Products.FirstOrDefault(x => x.TranslationId == translationEntry.TranslationId);
                        CacheManager.UpdateProductById(product.Id);
                        var partners = Db.PartnerProductSettings.Where(x => x.ProductId == product.Id && x.State == (int)ProductStates.Active)
                                                       .Select(x => x.PartnerId).ToList();
                        foreach (var partnerId in partners)
                        {
                            CacheManager.RemovePartnerProductSettings(partnerId);
                            broadcastKey.Add(string.Format("{0}_{1}_{2}_Desktop", Constants.CacheItems.PartnerProductSettings, partnerId, translationEntry.LanguageId), partnerId);
                            broadcastKey.Add(string.Format("{0}_{1}_{2}_Mobile", Constants.CacheItems.PartnerProductSettings, partnerId, translationEntry.LanguageId), partnerId);
                        }
                        broadcastKey.Add(string.Format("{0}_{1}", Constants.CacheItems.Products, product.Id), 0);
                        broadcastKey.Add(string.Format("{0}_{1}_{2}", Constants.CacheItems.Products, product.GameProviderId, product.ExternalId), 0);
                        break;
                    case (int)ObjectTypes.Promotion:
                    case (int)ObjectTypes.PromotionContent:
                    case (int)ObjectTypes.PromotionDescription:
                        var promotion = Db.Promotions.FirstOrDefault(x => x.TitleTranslationId == translationEntry.TranslationId ||
                                                                          x.DescriptionTranslationId == translationEntry.TranslationId ||
                                                                          x.ContentTranslationId == translationEntry.TranslationId);
                        CacheManager.RemovePromotions(promotion.PartnerId);
                        broadcastKey.Add(string.Format("{0}_{1}_{2}", Constants.CacheItems.Promotions, promotion.PartnerId, translationEntry.LanguageId), promotion.PartnerId);
                        break;
                    case (int)ObjectTypes.SecurityQuestion:
                        var securityQuestion = Db.SecurityQuestions.FirstOrDefault(x => x.TranslationId == translationEntry.TranslationId);
                        if (securityQuestion != null)
                        {
                            var lgs = Db.PartnerLanguageSettings.Where(x => x.PartnerId == securityQuestion.PartnerId && x.State == (int)PartnerLanguageStates.Active)
                                                                .Select(x => x.LanguageId).ToList();
                            foreach (var l in lgs)
                            {
                                CacheManager.RemovePartnerSecurityQuestionsByKey(securityQuestion.PartnerId, l);
                                broadcastKey.Add(string.Format("{0}_{1}_{2}", Constants.CacheItems.SecurityQuestions, securityQuestion.PartnerId, l), securityQuestion.PartnerId);
                            }
                        }
                        break;
                    case (int)ObjectTypes.Announcement:
                        var announcement = Db.Announcements.FirstOrDefault(x => x.TranslationId == translation.TranslationId);
                        if(announcement.Type == (int)AnnouncementTypes.Ticker)
                        {
                            CacheManager.RemovePartnerTickerFromCache(announcement.PartnerId, translation.LanguageId);
                            broadcastKey.Add(string.Format("{0}_{1}_{2}", Constants.CacheItems.Ticker, announcement.PartnerId, translationEntry.LanguageId),
                                             announcement.PartnerId);
                        }
                        break;
                    default:
                        break;
                }
                result.Add(t);
            }
            return result;
        }

        public List<fnCommentTemplate> GetCommentTemplates(int? commentTypeId, int? partnerId, bool checkPermission = true)
        {
            var query = Db.fn_CommentTemplate(LanguageId).AsQueryable();
            if (checkPermission)
            {
                var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewPartner,
                    ObjectTypeId = ObjectTypes.Partner
                });
                CheckPermission(Constants.Permissions.ViewPartnerCommentTemplate);
                if (!partnerAccess.HaveAccessForAllObjects)
                    query = query.Where(x => partnerAccess.AccessibleObjects.Contains(x.PartnerId));
            }
            if (commentTypeId.HasValue && !Enum.IsDefined(typeof(CommentTemplateTypes), commentTypeId.Value))
                throw CreateException(LanguageId, Constants.Errors.CommentTemplateNotFound);
            if (commentTypeId.HasValue)
                query = query.Where(x => x.Type == commentTypeId);
            if (partnerId.HasValue)
                query = query.Where(x => x.PartnerId == partnerId);
            return query.ToList();
        }

        public CommentTemplate SaveCommentTemplate(CommentTemplate commentTemplate)
        {
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            if (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleObjects.All(x => x != commentTemplate.PartnerId))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);

            CheckPermission(Constants.Permissions.EditPartnerCommentTemplate);
            if (!Enum.IsDefined(typeof(CommentTemplateTypes), commentTemplate.Type))
                throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);

            if (commentTemplate.Id > 0)
            {
                var dbCommentTemplate = Db.CommentTemplates.FirstOrDefault(x => x.Id == commentTemplate.Id);
                if (dbCommentTemplate == null)
                    throw CreateException(LanguageId, Constants.Errors.CommentTemplateNotFound);
                dbCommentTemplate.NickName = commentTemplate.NickName;
                Db.SaveChanges();
                return dbCommentTemplate;
            }
            commentTemplate.Status = (int)BaseStates.Active;
            commentTemplate.Translation = CreateTranslation(new fnTranslation
            {
                LanguageId = Constants.DefaultLanguageId,
                ObjectTypeId = (int)ObjectTypes.CommentTemplate,
                Text = commentTemplate.NickName
            });
            Db.CommentTemplates.Add(commentTemplate);
            Db.SaveChanges();
            return commentTemplate;
        }

        public void RemoveCommentTemplate(CommentTemplate commentTemplate)
        {
            var dbCommentTemplate = Db.CommentTemplates.FirstOrDefault(x => x.Id == commentTemplate.Id) ??
                throw CreateException(LanguageId, Constants.Errors.CommentTemplateNotFound);
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });
            if (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleObjects.All(x => x != dbCommentTemplate.PartnerId))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);

            CheckPermission(Constants.Permissions.EditPartnerCommentTemplate);
            dbCommentTemplate.Status = (int)BaseStates.Inactive;
            Db.SaveChanges();
            CacheManager.RemoveCommentTemplateFromCache(commentTemplate.PartnerId, commentTemplate.Type);
        }

        public List<fnJobArea> GetfnJobAreas(bool checkPermission, string languageId)
        {
            if (checkPermission)
            {
                var checkPermissionResult = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewJobArea,
                    ObjectTypeId = ObjectTypes.JobArea
                });
                if (!checkPermissionResult.HaveAccessForAllObjects)
                    throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            }
            return Db.fn_JobArea(languageId).ToList();
        }

        public List<JobArea> GetJobAreas(bool checkPermission)
        {
            if (checkPermission)
            {
                var checkPermissionResult = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewJobArea,
                    ObjectTypeId = ObjectTypes.JobArea
                });
                if (!checkPermissionResult.HaveAccessForAllObjects)
                    throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            }
            return Db.JobAreas.ToList();
        }

        public JobArea SaveJobArea(JobArea jobArea)
        {
            var checkPermissionResult = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.EditJobArea,
                ObjectTypeId = ObjectTypes.JobArea
            });
            if (!checkPermissionResult.HaveAccessForAllObjects)
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            var dbJobArea = Db.JobAreas.FirstOrDefault(x => x.Id == jobArea.Id);
            if (dbJobArea != null)
                dbJobArea.Info = jobArea.Info;
            else
            {
                jobArea.Translation = CreateTranslation(new fnTranslation
                {
                    ObjectTypeId = (int)ObjectTypes.JobArea,
                    Text = jobArea.NickName,
                    LanguageId = Constants.DefaultLanguageId
                });
                Db.JobAreas.Add(jobArea);
            }
            Db.SaveChanges();
            if (dbJobArea != null)
                CacheManager.RemoveJobAreasFromCache(string.Empty);
            return jobArea;
        }

        public List<fnEnumeration> GetEnumerations(string languageId)
        {
            CheckPermission(Constants.Permissions.ViewEnumerations);
            return Db.fn_Enumeration().Where(x => x.LanguageId == languageId).ToList();
        }

        public Segment SaveSegment(SegmentModel segmentModel)
        {
            var checkPartnerPermission = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });

            var PartnerPaymentSegmentEditPermission = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.EditSegment
            });
            if (!checkPartnerPermission.HaveAccessForAllObjects && checkPartnerPermission.AccessibleObjects.All(x => x != segmentModel.PartnerId))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);

            if (segmentModel.State.HasValue && !Enum.IsDefined(typeof(PartnerPaymentSettingStates), segmentModel.State))
                throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);

            var dbSegment = Db.Segments.FirstOrDefault(x => x.Id != segmentModel.Id && x.Name == segmentModel.Name &&
                                                            x.PartnerId == segmentModel.PartnerId);
            if (dbSegment != null)
                throw CreateException(LanguageId, Constants.Errors.SegmentExist);

            if (segmentModel.Id > 0)
            {
                dbSegment = Db.Segments.FirstOrDefault(x => x.Id == segmentModel.Id);
                if (dbSegment == null)
                    throw CreateException(LanguageId, Constants.Errors.SegmentNotFound);
                dbSegment.LastUpdateTime = DateTime.UtcNow;
                dbSegment.Name = segmentModel.Name;
                dbSegment.State = segmentModel.State ?? (int)SegmentStates.Active;
                dbSegment.IsKYCVerified = segmentModel.IsKYCVerified;
                dbSegment.Gender = segmentModel.Gender;
                dbSegment.IsTermsConditionAccepted = segmentModel.IsTermsConditionAccepted;
                dbSegment.ClientStatus = segmentModel.ClientStatus?.ToString();
                dbSegment.SegmentId = segmentModel.SegmentId?.ToString();
                dbSegment.ClientId = segmentModel.ClientId?.ToString();
                dbSegment.Email = segmentModel.Email?.ToString();
                dbSegment.FirstName = segmentModel.FirstName?.ToString();
                dbSegment.LastName = segmentModel.LastName?.ToString();
                dbSegment.Region = segmentModel.Region?.ToString();
                dbSegment.AffiliateId = segmentModel.AffiliateId?.ToString();
                dbSegment.MobileCode = segmentModel.MobileCode?.ToString();
                dbSegment.SessionPeriod = segmentModel.SessionPeriod?.ToString();
                dbSegment.SignUpPeriod = segmentModel.SignUpPeriod?.ToString();
                dbSegment.TotalDepositsCount = segmentModel.TotalDepositsCount?.ToString();
                dbSegment.TotalDepositsAmount = segmentModel.TotalDepositsAmount?.ToString();
                dbSegment.TotalWithdrawalsCount = segmentModel.TotalWithdrawalsCount?.ToString();
                dbSegment.TotalWithdrawalsAmount = segmentModel.TotalWithdrawalsAmount?.ToString();
                dbSegment.TotalBetsCount = segmentModel.TotalBetsCount?.ToString();
                dbSegment.TotalBetsAmount = segmentModel.TotalBetsAmount?.ToString();
                dbSegment.SportBetsCount = segmentModel.SportBetsCount?.ToString();
                dbSegment.CasinoBetsCount = segmentModel.CasinoBetsCount?.ToString();
                dbSegment.Profit = segmentModel.Profit?.ToString();
                dbSegment.Bonus = segmentModel.Bonus?.ToString();
                dbSegment.SuccessDepositPaymentSystem = segmentModel.SuccessDepositPaymentSystem?.ToString();
                dbSegment.SuccessWithdrawalPaymentSystem = segmentModel.SuccessWithdrawalPaymentSystem?.ToString();
                dbSegment.ComplimentaryPoint = segmentModel.ComplimentaryPoint?.ToString();
                Db.SaveChanges();
                segmentModel.Id = dbSegment.Id;
                segmentModel.PartnerId = dbSegment.PartnerId;
                segmentModel.CreationTime = dbSegment.CreationTime;
                CacheManager.RemoveSegmentSettingFromCache(dbSegment.Id);
                Db.ClientClassifications.Where(x => x.SegmentId == dbSegment.Id && 
                    x.ProductId == (int)Constants.PlatformProductId).DeleteFromQuery(); //Save change history
            }
            else
            {
                var currentTime = DateTime.UtcNow;
                segmentModel.CreationTime = currentTime;
                segmentModel.LastUpdateTime = currentTime;
                dbSegment = segmentModel.MapToSegment();
                Db.Segments.Add(dbSegment);
                Db.SaveChanges();
            }

            return dbSegment;
        }

        public Segment DeleteSegment(int id)
        {
            var dbSegment = Db.Segments.FirstOrDefault(x => x.Id == id);
            if (dbSegment == null)
                throw CreateException(LanguageId, Constants.Errors.SegmentNotFound);
            var checkPartnerPermission = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });

            var PartnerPaymentSegmentEditPermission = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.EditSegment
            });
            if (!checkPartnerPermission.HaveAccessForAllObjects && checkPartnerPermission.AccessibleObjects.All(x => x != dbSegment.PartnerId))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);

            dbSegment.LastUpdateTime = DateTime.UtcNow;
            dbSegment.State = (int)SegmentStates.Inactive;
            Db.SaveChanges();
            Db.ClientClassifications.Where(x => x.SegmentId == id).DeleteFromQuery();
            Db.BannerSegmentSettings.Where(x => x.SegmentId==id).DeleteFromQuery();
            Db.BonusSegmentSettings.Where(x => x.SegmentId==id).DeleteFromQuery();
            Db.PromotionSegmentSettings.Where(x => x.SegmentId==id).DeleteFromQuery();
            CacheManager.RemoveSegmentSettingFromCache(dbSegment.Id);
            return dbSegment;
        }
      
        public List<Segment> GetSegments(int? id, int? partnerId)
        {
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = ObjectTypes.Partner
            });

            var PartnerPaymentSegmentEditPermission = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewSegment
            });
            var query = Db.Segments.Where(x => x.State != (int)SegmentStates.Inactive).AsQueryable();
            if (partnerId.HasValue)
                query = query.Where(x => x.PartnerId == partnerId.Value);
            else if (!partnerAccess.HaveAccessForAllObjects)
                query = query.Where(x => partnerAccess.AccessibleObjects.Contains(x.PartnerId));

            if(id.HasValue)
                query = query.Where(x => x.Id == id.Value);

            return query.ToList();
        }
    }
}