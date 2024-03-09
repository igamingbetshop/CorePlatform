using System.Linq;
using Newtonsoft.Json;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.AdminWebApi.Models.ContentModels;
using IqSoft.CP.AdminWebApi.Helpers;
using IqSoft.CP.AdminWebApi.Models.CommonModels;
using log4net;
using System;
using System.Net;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.AdminWebApi.Models.PartnerModels;
using IqSoft.CP.DAL;
using IqSoft.CP.AdminWebApi.Filters;
using System.Data.Entity.Validation;
using IqSoft.CP.Integration.Platforms.Models.Webflow;
using IqSoft.CP.AdminWebApi.Models.CRM;
using IqSoft.CP.BLL.Helpers;
using IqSoft.CP.DAL.Models.Segment;
using IqSoft.CP.Common.Models.AdminModels;
using System.Collections.Generic;
using IqSoft.CP.Common.Helpers;

namespace IqSoft.CP.AdminWebApi.ControllerClasses
{
    public static class ContentController
    {
        public static ApiResponseBase CallFunction(RequestBase request, SessionIdentity identity, ILog log)
        {
            switch (request.Method)
            {
                case "SaveWebSiteBanner":
                    return SaveWebSiteBanner(JsonConvert.DeserializeObject<ApiBanner>(request.RequestData), identity, log);
                case "RemoveWebSiteBanner":
                    return RemoveWebSiteBanner(Convert.ToInt32(request.RequestData), identity, log);
                case "GetWebSiteBanners":
                    return GetWebSiteBanners(JsonConvert.DeserializeObject<ApiFilterBanner>(request.RequestData), identity, log);
                case "GetBannerById":
                    return GetBannerById(JsonConvert.DeserializeObject<ApiFilterContent>(request.RequestData), identity, log);
                case "GetBannerFragments":
                    return GetBannerFragments(Convert.ToInt32(request.RequestData), identity, log);
                case "GetPromotions":
                    return GetPromotions(JsonConvert.DeserializeObject<ApiFilterContent>(request.RequestData), identity, log);
                case "GetNews":
                    return GetNews(JsonConvert.DeserializeObject<ApiFilterContent>(request.RequestData), identity, log);
                case "GetPromotionById":
                    return GetPromotionById(JsonConvert.DeserializeObject<ApiFilterContent>(request.RequestData), identity, log);
                case "GetNewsById":
                    return GetNewsById(JsonConvert.DeserializeObject<ApiFilterContent>(request.RequestData), identity, log);
                case "SavePromotion":
                    return SavePromotion(JsonConvert.DeserializeObject<ApiPromotion>(request.RequestData), identity, log);
                case "SaveNews":
                    return SaveNews(JsonConvert.DeserializeObject<ApiNews>(request.RequestData), identity, log);
                case "RemovePromotion":
                    return RemovePromotion(Convert.ToInt32(request.RequestData), identity, log);
                case "RemoveNews":
                    return RemoveNews(Convert.ToInt32(request.RequestData), identity, log);
                case "SavePopup":
                    return SavePopup(JsonConvert.DeserializeObject<ApiPopup>(request.RequestData), identity, log);
                case "GetPopups":
                    return GetPopups(JsonConvert.DeserializeObject<ApiFilterPopup>(request.RequestData), identity, log);
                case "GetPopupById":
                    return GetPopupById(Convert.ToInt32(request.RequestData), identity, log);
                case "RemovePopup":
                    return RemovePopup(Convert.ToInt32(request.RequestData), identity, log);
                case "BroadcastPopup":
                    return BaseController.BroadcastPopup(Convert.ToInt32(request.RequestData), identity, log);
                case "GetWebSiteMenus":
                    return GetWebSiteMenus(JsonConvert.DeserializeObject<ApiFilterContent>(request.RequestData), identity, log);
                case "SaveWebSiteMenu":
                    return SaveWebSiteMenu(JsonConvert.DeserializeObject<ApiWebSiteMenu>(request.RequestData), identity, log);
                case "GetWebSiteMenuItems":
                    return GetWebSiteMenuItems(Convert.ToInt32(request.RequestData), identity, log);
                case "GetWebSiteSubMenuItems":
                    return GetWebSiteSubMenuItems(Convert.ToInt32(request.RequestData), identity, log);
                case "SaveWebSiteMenuItem":
                    return SaveWebSiteMenuItem(JsonConvert.DeserializeObject<ApiWebSiteMenuItem>(request.RequestData), identity, log);
                case "SaveWebSiteSubMenuItem":
                    return SaveWebSiteSubMenuItem(JsonConvert.DeserializeObject<ApiWebSiteSubMenuItem>(request.RequestData), identity, log);
                case "RemoveWebSiteMenuItem":
                    return RemoveWebSiteMenuItem(Convert.ToInt32(request.RequestData), identity, log);
                case "RemoveWebSiteSubMenuItem":
                    return RemoveWebSiteSubMenuItem(Convert.ToInt32(request.RequestData), identity, log);
                case "GetItemTranslations":
                    return GetItemTranslations(Convert.ToInt32(request.RequestData), identity, log);
                case "CloneWebSiteMenuByPartnerId":
                    return CloneWebSiteMenuByPartnerId(JsonConvert.DeserializeObject<ApiCloneObject>(request.RequestData), identity, log);
                case "FindSubMenuItemByTitle":
                    return FindSubMenuItemByTitle(JsonConvert.DeserializeObject<ApiWebSiteMenu>(request.RequestData), identity, log);
                case "SaveCRMSetting":
                    return SaveCRMSetting(JsonConvert.DeserializeObject<ApiCRMSetting>(request.RequestData), identity, log);
                case "GetCRMSettings":
                    return GetCRMSettings(identity, log);
                case "GetCRMSettingById":
                    return GetCRMSettingById(Convert.ToInt32(request.RequestData), identity, log);
                case "GetMessageTemplates":
                    return GetMessageTemplates(identity, log);
                case "SaveMessageTemplate":
                    return SaveMessageTemplate(JsonConvert.DeserializeObject<MessageTemplateModel>(request.RequestData), identity, log);
                case "RemoveMessageTemplate":
                    return RemoveMessageTemplate(Convert.ToInt32(request.RequestData), identity, log);
                case "GetObjectTranslations":
                    return GetObjectTranslations(JsonConvert.DeserializeObject<ObjectModel>(request.RequestData), identity, log);
                case "UploadConfig":
                    return UploadConfigFile(JsonConvert.DeserializeObject<FileUploadInput>(request.RequestData), identity, log);
                case "UploadMenus":
                    return UploadMenus(JsonConvert.DeserializeObject<FileUploadInput>(request.RequestData), identity, log);
                case "UploadStyles":
                    return UploadWebSiteStylesFile(JsonConvert.DeserializeObject<FileUploadInput>(request.RequestData), identity, log);
                case "UploadTranslations":
                    return UploadWebSiteTranslations(JsonConvert.DeserializeObject<FileUploadInput>(request.RequestData), identity, log);
                case "UploadPromotions":
                    return UploadWebSitePromotions(JsonConvert.DeserializeObject<FileUploadInput>(request.RequestData), identity, log);
                case "UploadNews":
                    return UploadWebSiteNews(JsonConvert.DeserializeObject<FileUploadInput>(request.RequestData), identity, log);
                case "UploadImage":
                    return UploadImage(JsonConvert.DeserializeObject<FileUploadInput>(request.RequestData), identity, log);
                case "SaveAnnouncement":
                    return SaveAnnouncement(JsonConvert.DeserializeObject<ApiAnnouncement>(request.RequestData), identity, log);
                case "GetAnnouncements":
                    return GetAnnouncements(JsonConvert.DeserializeObject<ApiFilterAnnouncement>(request.RequestData), identity, log);
                case "GetAnnouncementById":
                    return GetAnnouncementById(Convert.ToInt32(request.RequestData), identity, log);
                case "GetCommentTemplates":
                    return GetCommentTemplates(request.RequestData, identity, log);
                case "SaveCommentTemplate":
                    return SaveCommentTemplate(JsonConvert.DeserializeObject<CommentTemplate>(request.RequestData), identity, log);
                case "RemoveCommentTemplate":
                    return RemoveCommentTemplate(JsonConvert.DeserializeObject<CommentTemplate>(request.RequestData), identity, log);
                case "GetWebflowSites":
                    return GetWebflowItems(JsonConvert.DeserializeObject<ApiWebflowInput>(request.RequestData), (int)WebflowItemTypes.Site, identity, log);
                case "GetWebflowCollections":
                    return GetWebflowItems(JsonConvert.DeserializeObject<ApiWebflowInput>(request.RequestData), (int)WebflowItemTypes.Collection, identity, log);
                case "GetWebflowItems":
                    return GetWebflowItems(JsonConvert.DeserializeObject<ApiWebflowInput>(request.RequestData), (int)WebflowItemTypes.Item, identity, log);
                case "GetWebflowItemtData":
                    return GetWebflowItems(JsonConvert.DeserializeObject<ApiWebflowInput>(request.RequestData), (int)WebflowItemTypes.ItemBody, identity, log);
                case "GetJobAreas":
                    return GetJobAreas(identity, log);
                case "SaveJobArea":
                    return SaveJobArea(JsonConvert.DeserializeObject<JobArea>(request.RequestData), identity, log);
                case "SaveSegment":
                    return SaveSegment(JsonConvert.DeserializeObject<SegmentModel>(request.RequestData), identity, log);
                case "DeleteSegment":
                    return DeleteSegment(JsonConvert.DeserializeObject<ApiBaseFilter>(request.RequestData).Id.Value, identity, log);
                case "GetSegments":
                    return GetSegments(JsonConvert.DeserializeObject<ApiBaseFilter>(request.RequestData), identity, log);
                case "GetAdminTranslations":
                    return GetAdminTranslations(identity, log);
                case "SaveAdminTranslation":
                    return SaveAdminTranslation(JsonConvert.DeserializeObject<ApiWebSiteMenuItem>(request.RequestData), identity, log);
                case "GetAdminTranslationItems":
                    return GetAdminTranslationItems(Convert.ToInt32(request.RequestData), identity, log);
                case "SaveAdminTranslationItem":
                    return SaveAdminTranslationItem(JsonConvert.DeserializeObject<ApiWebSiteSubMenuItem>(request.RequestData), identity, log);
                case "RemoveAdminTranslation":
                    return RemoveAdminTranslation(Convert.ToInt32(request.RequestData), identity, log);
                case "RemoveAdminTranslationItem":
                    return RemoveAdminTranslationItem(Convert.ToInt32(request.RequestData), identity, log);
                case "GetAdminItemTranslations":
                    return GetAdminItemTranslations(Convert.ToInt32(request.RequestData), identity, log);
                case "UploadAdminTranslations":
                    return UploadAdminTranslations(identity, log);
            }
            throw BaseBll.CreateException(string.Empty, Constants.Errors.MethodNotFound);
        }

        private static ApiResponseBase RemoveWebSiteBanner(int bannerId, SessionIdentity identity, ILog log)
        {
            using (var contentBl = new ContentBll(identity, log))
            {
                using (var partnerBl = new PartnerBll(contentBl))
                {
                    contentBl.RemoveWebSiteBanner(bannerId, out int partnerId, out int bannerType);
                    CacheManager.RemoveBanners(partnerId, bannerType);
                    Helpers.Helpers.InvokeMessage("RemoveBanners", partnerId, bannerType);
                    var languages = CacheManager.GetAvailableLanguages();
                    foreach (var lan in languages)
                    {
                        BaseController.BroadcastCacheChanges(partnerId, string.Format("{0}_{1}_{2}_{3}", Constants.CacheItems.Banners, partnerId, bannerType, lan.Id));
                    }
                    var ftpModels = partnerBl.GetPartnerEnvironments(partnerId);
                    if (ftpModels == null)
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.PartnerKeyNotFound);
                    var imgName = bannerId.ToString() + ".png";
                    var partner = CacheManager.GetPartnerById(partnerId);
                    foreach (var ftpModel in ftpModels)
                    {
                        try
                        {
                            var ftpRequest = (FtpWebRequest)WebRequest.Create(new Uri("ftp://" + ftpModel.Value.Url + "/coreplatform/website/" + partner.Name + "/assets/images/b/" + imgName));
                            ftpRequest.Credentials = new NetworkCredential(ftpModel.Value.UserName, ftpModel.Value.Password);
                            ftpRequest.Method = WebRequestMethods.Ftp.DeleteFile;
                            ftpRequest.GetResponse();
                        }
                        catch
                        {

                        }
                    }
                    return new ApiResponseBase();
                }
            }
        }

        private static ApiResponseBase SaveWebSiteBanner(ApiBanner apiBannerInput, SessionIdentity identity, ILog log)
        {
            using (var contentBl = new ContentBll(identity, log))
            {
                using (var partnerBl = new PartnerBll(contentBl))
                {
                    try
                    {
                        if (string.IsNullOrEmpty(apiBannerInput.ImageData))
                            apiBannerInput.ImageSize = String.Empty;
                        var banner = apiBannerInput.MapToBanner();
                        banner = contentBl.SaveWebSiteBanner(banner);
                        CacheManager.RemoveBanners(apiBannerInput.PartnerId, apiBannerInput.Type);
                        Helpers.Helpers.InvokeMessage("RemoveBanners", apiBannerInput.PartnerId, apiBannerInput.Type);
                        var partner = CacheManager.GetPartnerById(banner.PartnerId);
                        if (!string.IsNullOrEmpty(banner.Image) && !string.IsNullOrEmpty(apiBannerInput.ImageData))
                        {
                            var ftpModel = partnerBl.GetPartnerEnvironments(apiBannerInput.PartnerId)[apiBannerInput.EnvironmentTypeId];
                            if (ftpModel == null)
                                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.PartnerKeyNotFound);
                            var ext = banner.Image.Split('.', '?');
                            var extension = ext.Length > 1 ? "." + ext[1] : ".png";
                            var imgName = banner.Id.ToString() + extension;
                            if (!string.IsNullOrEmpty(apiBannerInput.ImageSize))
                                imgName = $"{banner.Id}-{apiBannerInput.ImageSize}{extension}";
                            byte[] bytes = Convert.FromBase64String(apiBannerInput.ImageData);
                            try
                            {
                                var path = "ftp://" + ftpModel.Url + "/coreplatform/website/" + partner.Name + "/assets/images/b/" + imgName;
                                contentBl.UploadFtpImage(bytes, ftpModel, path);
                                if (extension == ".webp")
                                {
                                    /*path = path.Replace(".webp", ".jp2");
                                    using (var readerStream = new MemoryStream(bytes))
                                    {
                                        using (var img = Aspose.Imaging.Image.Load(readerStream))
                                        {
                                            using (var writerStream = new MemoryStream())
                                            {
                                                img.Save(writerStream, new Aspose.Imaging.ImageOptions.Jpeg2000Options());
                                                UploadFtpImage(writerStream.ToArray(), ftpModel, path);
                                            }
                                        }
                                    }*/
                                }
                            }
                            catch (Exception e)
                            {
                                log.Error(e);
                            }
                        }
                        var languages = CacheManager.GetAvailableLanguages();
                        foreach (var lan in languages)
                        {
                            BaseController.BroadcastCacheChanges(banner.PartnerId, string.Format("{0}_{1}_{2}_{3}", Constants.CacheItems.Banners, banner.PartnerId, banner.Type, lan.Id));
                        }
                        return new ApiResponseBase
                        {
                            ResponseObject = banner.ToApiBanner(identity.TimeZone)
                        };
                    }
                    catch (DbEntityValidationException e)
                    {
                        var msg = string.Empty;
                        foreach (var eve in e.EntityValidationErrors)
                        {
                            msg += string.Format("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                                eve.Entry.Entity.GetType().Name, eve.Entry.State);
                            foreach (var ve in eve.ValidationErrors)
                            {
                                msg += string.Format("- Property: \"{0}\", Error: \"{1}\"",
                                    ve.PropertyName, ve.ErrorMessage);
                            }
                        }
                        log.Error(msg);
                        throw;
                    }
                }
            }
        }

        private static ApiResponseBase GetWebSiteBanners(ApiFilterBanner input, SessionIdentity identity, ILog log)
        {
            using (var contentBl = new ContentBll(identity, log))
            {
                var resp = contentBl.GetBanners(input.MaptToFilterfnBanner());
                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        resp.Count,
                        Entities = resp.Entities.Select(x => new ApiBanner
                        {
                            Id = x.Id,
                            PartnerId = x.PartnerId,
                            NickName = x.NickName,
                            Type = x.Type,
                            Head = x.Head,
                            Body = x.Body,
                            Link = x.Link,
                            Order = x.Order,
                            Image = x.Image.Split(',')[0],
                            ImageSizes = x.Image.Split(',').Skip(1).ToList(),
                            IsEnabled = x.IsEnabled,
                            ShowDescription = x.ShowDescription,
                            ShowRegistration = x.ButtonType.HasValue && Convert.ToBoolean(x.ButtonType.ToString().Select(y => y.Equals('1')).AsEnumerable().ElementAtOrDefault(1)),
                            ShowLogin = x.ButtonType.HasValue && Convert.ToBoolean(x.ButtonType.ToString().Select(y => y.Equals('1')).AsEnumerable().ElementAtOrDefault(2)),
                            StartDate = x.StartDate,
                            EndDate = x.EndDate,
                            Visibility = string.IsNullOrEmpty(x.Visibility) ? new List<int>() : JsonConvert.DeserializeObject<List<int>>(x.Visibility),
                            FragmentName = x.FragmentName
                        }).ToList()
                    }
                };
            }
        }

        private static ApiResponseBase GetBannerById(ApiFilterContent input, SessionIdentity identity, ILog log)
        {
            using (var contentBl = new ContentBll(identity, log))
            {
                var apiBanner = contentBl.GetBannerById(input.PartnerId, input.Id ?? 0)?.ToApiBanner(identity.TimeZone);
				var partner = CacheManager.GetPartnerById(apiBanner.PartnerId);
				var siteurl = partner.SiteUrl.Split(',')[0];
				apiBanner.SiteUrl = siteurl;
				return new ApiResponseBase
                {
                    ResponseObject = apiBanner
				};
            }
        }

        private static ApiResponseBase GetBannerFragments(int partnerId, SessionIdentity identity, ILog log)
        {
            using (var contentBl = new ContentBll(identity, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = contentBl.GetWebSiteFragments(partnerId)
                };
            }
        }

        private static ApiResponseBase SavePromotion(ApiPromotion apiPromotionInput, SessionIdentity identity, ILog log)
        {
            if ((apiPromotionInput.Languages != null && !Enum.IsDefined(typeof(BonusSettingConditionTypes), apiPromotionInput.Languages.Type)) ||
                (apiPromotionInput.Segments != null && !Enum.IsDefined(typeof(BonusSettingConditionTypes), apiPromotionInput.Segments.Type)))
                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongInputParameters);

            using (var contentBl = new ContentBll(identity, log))
            {
                using (var partnerBl = new PartnerBll(contentBl))
                {
                    var dbPromotion = contentBl.SavePromotion(apiPromotionInput.MapToPromotion());

                    CacheManager.RemovePromotions(apiPromotionInput.PartnerId);
                    Helpers.Helpers.InvokeMessage("RemovePromotions", apiPromotionInput.PartnerId);
                    var partner = CacheManager.GetPartnerById(apiPromotionInput.PartnerId);
                    if (!string.IsNullOrEmpty(apiPromotionInput.ImageName) && (!string.IsNullOrEmpty(apiPromotionInput.ImageData) ||
																			   !string.IsNullOrEmpty(apiPromotionInput.ImageDataMedium) ||
																			   !string.IsNullOrEmpty(apiPromotionInput.ImageDataSmall)))

					{
                        var ftpModel = partnerBl.GetPartnerEnvironments(apiPromotionInput.PartnerId)[apiPromotionInput.EnvironmentTypeId];
                        if (ftpModel == null)
                            throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.PartnerKeyNotFound);
                        var ext = apiPromotionInput.ImageName.Split('.', '?');
                        var extension = ext.Length > 1 ? "." + ext[1] : ".png";
                        var imgName = dbPromotion.Id.ToString() + extension;

                        var path = "ftp://" + ftpModel.Url + "/coreplatform/website/" + partner.Name + "/assets/images/promotions/";
                        try
                        {
                            if (!string.IsNullOrEmpty(apiPromotionInput.ImageData))
                            {
                                byte[] bytes = Convert.FromBase64String(apiPromotionInput.ImageData);
                                contentBl.UploadFtpImage(bytes, ftpModel, path + imgName);
                            }
                            if (!string.IsNullOrEmpty(apiPromotionInput.ImageDataMedium))
                            {
                                byte[] bytesMedium = Convert.FromBase64String(apiPromotionInput.ImageDataMedium);
                                contentBl.UploadFtpImage(bytesMedium, ftpModel, path + "medium/" + imgName);
                            }
                            if (!string.IsNullOrEmpty(apiPromotionInput.ImageDataSmall))
                            {
                                byte[] bytesSmall = Convert.FromBase64String(apiPromotionInput.ImageDataSmall);
                                contentBl.UploadFtpImage(bytesSmall, ftpModel, path+ "small/" + imgName);
                            }
                        }
                        catch (Exception e)
                        {
                            log.Error(e);
                        }
                    }
                    var languages = CacheManager.GetAvailableLanguages();
                    foreach (var lan in languages)
                    {
                        BaseController.BroadcastCacheChanges(dbPromotion.PartnerId, string.Format("{0}_{1}_{2}", Constants.CacheItems.Promotions, dbPromotion.PartnerId, lan.Id));
                    }
                    return new ApiResponseBase
                    {
                        ResponseObject = new ApiPromotion
                        {
                            Id = dbPromotion.Id,
                            PartnerId = dbPromotion.PartnerId,
                            NickName = dbPromotion.NickName,
                            Type = dbPromotion.Type,
                            State = dbPromotion.State,
                            Title = apiPromotionInput.Title,
                            Description = apiPromotionInput.Description,
                            ImageName = dbPromotion.ImageName,
                            StartDate = dbPromotion.StartDate,
                            FinishDate = dbPromotion.FinishDate,
                            CreationTime = dbPromotion.CreationTime,
                            LastUpdateTime = dbPromotion.LastUpdateTime,
                            Order = dbPromotion.Order,
                            Segments = dbPromotion.PromotionSegmentSettings != null && dbPromotion.PromotionSegmentSettings.Any() ? new ApiSetting
                            {
                                Type = dbPromotion.PromotionSegmentSettings.First().Type,
                                Ids = dbPromotion.PromotionSegmentSettings.Select(x => x.SegmentId).ToList()
                            } : new ApiSetting { Type = (int)BonusSettingConditionTypes.InSet, Ids = new List<int>() },
                            Languages = dbPromotion.PromotionLanguageSettings != null && dbPromotion.PromotionLanguageSettings.Any() ? new ApiSetting
                            {
                                Type = dbPromotion.PromotionLanguageSettings.First().Type,
                                Names = dbPromotion.PromotionLanguageSettings.Select(x => x.LanguageId).ToList()
                            } : new ApiSetting { Type = (int)BonusSettingConditionTypes.InSet, Names = new List<string>() },
                            StyleType = dbPromotion.StyleType
                        }
                    };
                }
            }
        }

        private static ApiResponseBase SaveNews(ApiNews apiNewsInput, SessionIdentity identity, ILog log)
        {
            if ((apiNewsInput.Languages != null && !Enum.IsDefined(typeof(BonusSettingConditionTypes), apiNewsInput.Languages.Type)) ||
                (apiNewsInput.Segments != null && !Enum.IsDefined(typeof(BonusSettingConditionTypes), apiNewsInput.Segments.Type)))
                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongInputParameters);

            using (var contentBl = new ContentBll(identity, log))
            {
                using (var partnerBl = new PartnerBll(contentBl))
                {
                    var dbNews = contentBl.SaveNews(apiNewsInput.ToNews());

                    CacheManager.RemoveNews(apiNewsInput.PartnerId);
                    Helpers.Helpers.InvokeMessage("RemoveNews", apiNewsInput.PartnerId);
                    var partner = CacheManager.GetPartnerById(apiNewsInput.PartnerId);
                    if (!string.IsNullOrEmpty(apiNewsInput.ImageName) && (!string.IsNullOrEmpty(apiNewsInput.ImageData) ||
                                                                               !string.IsNullOrEmpty(apiNewsInput.ImageDataMedium) ||
                                                                               !string.IsNullOrEmpty(apiNewsInput.ImageDataSmall)))

                    {
                        var ftpModel = partnerBl.GetPartnerEnvironments(apiNewsInput.PartnerId)[apiNewsInput.EnvironmentTypeId];
                        if (ftpModel == null)
                            throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.PartnerKeyNotFound);
                        var ext = apiNewsInput.ImageName.Split('.', '?');
                        var extension = ext.Length > 1 ? "." + ext[1] : ".png";
                        var imgName = dbNews.Id.ToString() + extension;

                        var path = "ftp://" + ftpModel.Url + "/coreplatform/website/" + partner.Name + "/assets/images/news/";
                        try
                        {
                            if (!string.IsNullOrEmpty(apiNewsInput.ImageData))
                            {
                                byte[] bytes = Convert.FromBase64String(apiNewsInput.ImageData);
                                contentBl.UploadFtpImage(bytes, ftpModel, path + imgName);
                            }
                            if (!string.IsNullOrEmpty(apiNewsInput.ImageDataMedium))
                            {
                                byte[] bytesMedium = Convert.FromBase64String(apiNewsInput.ImageDataMedium);
                                contentBl.UploadFtpImage(bytesMedium, ftpModel, path + "medium/" + imgName);
                            }
                            if (!string.IsNullOrEmpty(apiNewsInput.ImageDataSmall))
                            {
                                byte[] bytesSmall = Convert.FromBase64String(apiNewsInput.ImageDataSmall);
                                contentBl.UploadFtpImage(bytesSmall, ftpModel, path + "small/" + imgName);
                            }
                        }
                        catch (Exception e)
                        {
                            log.Error(e);
                        }
                    }
                    var languages = CacheManager.GetAvailableLanguages();
                    foreach (var lan in languages)
                    {
                        BaseController.BroadcastCacheChanges(dbNews.PartnerId, string.Format("{0}_{1}_{2}", Constants.CacheItems.News, dbNews.PartnerId, lan.Id));
                    }
                    return new ApiResponseBase
                    {
                        ResponseObject = new ApiNews
                        {
                            Id = dbNews.Id,
                            PartnerId = dbNews.PartnerId,
                            NickName = dbNews.NickName,
                            Type = dbNews.Type,
                            State = dbNews.State,
                            Title = apiNewsInput.Title,
                            Description = apiNewsInput.Description,
                            ImageName = dbNews.ImageName,
                            StartDate = dbNews.StartDate,
                            FinishDate = dbNews.FinishDate,
                            CreationTime = dbNews.CreationTime,
                            LastUpdateTime = dbNews.LastUpdateTime,
                            Order = dbNews.Order,
                            Segments = dbNews.NewsSegmentSettings != null && dbNews.NewsSegmentSettings.Any() ? new ApiSetting
                            {
                                Type = dbNews.NewsSegmentSettings.First().Type,
                                Ids = dbNews.NewsSegmentSettings.Select(x => x.SegmentId).ToList()
                            } : new ApiSetting { Type = (int)BonusSettingConditionTypes.InSet, Ids = new List<int>() },
                            Languages = dbNews.NewsLanguageSettings != null && dbNews.NewsLanguageSettings.Any() ? new ApiSetting
                            {
                                Type = dbNews.NewsLanguageSettings.First().Type,
                                Names = dbNews.NewsLanguageSettings.Select(x => x.LanguageId).ToList()
                            } : new ApiSetting { Type = (int)BonusSettingConditionTypes.InSet, Names = new List<string>() },
                            StyleType = dbNews.StyleType
                        }
                    };
                }
            }
        }

        private static ApiResponseBase SavePopup(ApiPopup apiPopupInput, SessionIdentity identity, ILog log)
        {
            using (var contentBl = new ContentBll(identity, log))
            {
                using (var partnerBl = new PartnerBll(contentBl))
                {
                    var input = new Popup
                    {
                        Id =apiPopupInput.Id,
                        PartnerId = apiPopupInput.PartnerId,
                        NickName = apiPopupInput.NickName,
                        Type = apiPopupInput.Type,
                        State = apiPopupInput.State,
                        ImageName = apiPopupInput.ImageName,
                        Order = apiPopupInput.Order,
                        Page = apiPopupInput.Page,
                        SegmentIds = apiPopupInput.SegmentIds,
                        ClientIds = apiPopupInput.ClientIds,
                        StartDate = apiPopupInput.StartDate,
                        FinishDate = apiPopupInput.FinishDate
                    };
                    var dbPopup = contentBl.SavePopup(input);
                    apiPopupInput.Id = dbPopup.Id;
                    apiPopupInput.CreationTime = dbPopup.CreationTime;
                    apiPopupInput.LastUpdateTime = dbPopup.LastUpdateTime;
                    if (!string.IsNullOrEmpty(apiPopupInput.ImageName) && !string.IsNullOrEmpty(apiPopupInput.ImageData))
                    {
                        var ftpModel = partnerBl.GetPartnerEnvironments(apiPopupInput.PartnerId)[apiPopupInput.EnvironmentTypeId] ??
                            throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.PartnerKeyNotFound);
                        var ext = apiPopupInput.ImageName.Split('.', '?');
                        var extension = ext.Length > 1 ? "." + ext[1] : ".png";
                        apiPopupInput.ImageName = dbPopup.Id.ToString() + extension;
                        var partner = CacheManager.GetPartnerById(apiPopupInput.PartnerId);
                        var path = "ftp://" + ftpModel.Url + "/coreplatform/website/" + partner.Name + "/assets/images/popup/";
                        try
                        {
                            if (!string.IsNullOrEmpty(apiPopupInput.ImageData))
                            {
                                byte[] bytes = Convert.FromBase64String(apiPopupInput.ImageData);
                                contentBl.UploadFtpImage(bytes, ftpModel, $"{path}/web/{apiPopupInput.ImageName}");
                            }
                            if (!string.IsNullOrEmpty(apiPopupInput.MobileImageData))
                            {
                                byte[] bytes = Convert.FromBase64String(apiPopupInput.MobileImageData);
                                contentBl.UploadFtpImage(bytes, ftpModel, $"{path}/mobile/{apiPopupInput.ImageName}");
                            }
                        }
                        catch (Exception e)
                        {
                            log.Error(e);
                        }
                    }
                    Helpers.Helpers.InvokeMessage("RemoveFromCache", string.Format("{0}_{1}_{2}", Constants.CacheItems.Popups, apiPopupInput.PartnerId, apiPopupInput.Type));
                    return new ApiResponseBase
                    {
                        ResponseObject = apiPopupInput
                    };
                }
            }
        }

        private static ApiResponseBase GetPopups(ApiFilterPopup apiFilterPopup, SessionIdentity identity, ILog log)
        {
            using (var contentBl = new ContentBll(identity, log))
            {
                var res = contentBl.GetPopups(apiFilterPopup.MaptToFilterPopup());
                return new ApiResponseBase
                {
                    ResponseObject =  new
                    {
                        res.Count,
                        Entities = res.Entities.Select(x=> new
                        {
                            x.Id,
                            x.PartnerId,
                            x.NickName,
                            x.Type,
                            x.State,
                            x.Page,
                            x.Order,
                            StartDate = x.StartDate.GetGMTDateFromUTC(identity.TimeZone),
                            FinishDate = x.FinishDate.GetGMTDateFromUTC(identity.TimeZone),
                            CreationTime = x.CreationTime.GetGMTDateFromUTC(identity.TimeZone),
                            LastUpdateTime = x.LastUpdateTime.GetGMTDateFromUTC(identity.TimeZone)
                        })
                    }                
                };
            }
        }

        private static ApiResponseBase GetPopupById(int popupId, SessionIdentity identity, ILog log)
        {
            using (var contentBl = new ContentBll(identity, log))
            {
                var apiPopup = contentBl.GetPopupById(popupId).MapToApiPopup(identity.TimeZone);
                var partner = CacheManager.GetPartnerById(apiPopup.PartnerId);
                var siteurl = partner.SiteUrl.Split(',')[0];
                apiPopup.SiteUrl = siteurl;

                return new ApiResponseBase
                {
                    ResponseObject =  apiPopup
                };
            }
        }

        private static ApiResponseBase RemovePopup(int popupId, SessionIdentity identity, ILog log)
        {
            using (var contentBl = new ContentBll(identity, log))
            {
                contentBl.RemovePopup(popupId);
                return new ApiResponseBase();
            }
        }

        private static ApiResponseBase RemovePromotion(int promotionId, SessionIdentity identity, ILog log)
        {
            using (var contentBl = new ContentBll(identity, log))
            {
                var promotion = contentBl.RemovePromotion(promotionId);
                CacheManager.RemovePromotions(promotion.PartnerId);
                Helpers.Helpers.InvokeMessage("RemovePromotions", promotion.PartnerId);
                var partner = CacheManager.GetPartnerById(promotion.PartnerId);
                var languages = CacheManager.GetAvailableLanguages();
                foreach (var lan in languages)
                {
                    BaseController.BroadcastCacheChanges(promotion.PartnerId,
                        string.Format("{0}_{1}_{2}", Constants.CacheItems.Promotions, promotion.Id, lan.Id));
                }
                return new ApiResponseBase
                {
                    ResponseObject = new ApiPromotion
                    {
                        Id = promotion.Id
                    }
                };
            }
        }

        private static ApiResponseBase RemoveNews(int newsId, SessionIdentity identity, ILog log)
        {
            using (var contentBl = new ContentBll(identity, log))
            {
                var news = contentBl.RemoveNews(newsId);
                CacheManager.RemoveNews(news.PartnerId);
                Helpers.Helpers.InvokeMessage("RemoveNews", news.PartnerId);
                var partner = CacheManager.GetPartnerById(news.PartnerId);
                var languages = CacheManager.GetAvailableLanguages();
                foreach (var lan in languages)
                {
                    BaseController.BroadcastCacheChanges(news.PartnerId,
                        string.Format("{0}_{1}_{2}", Constants.CacheItems.News, news.Id, lan.Id));
                }
                return new ApiResponseBase
                {
                    ResponseObject = new ApiNews
                    {
                        Id = news.Id
                    }
                };
            }
        }

        private static ApiResponseBase GetPromotions(ApiFilterContent promotionFilter, SessionIdentity identity, ILog log)
        {
            using (var contentBl = new ContentBll(identity, log))
            {
                var result = contentBl.GetPromotions(promotionFilter.Id, promotionFilter.PartnerId,
                    promotionFilter.ParentId, promotionFilter.SkipCount, promotionFilter.TakeCount);
                return new ApiResponseBase
                {
                    ResponseObject = result.Select(x => new ApiPromotion
                    {
                        Id = x.Id,
                        PartnerId = x.PartnerId,
                        NickName = x.NickName,
                        Type = x.Type,
                        State = x.State,
                        Title = x.Title,
                        Description = x.Description,
                        ImageName = x.ImageName,
                        StartDate = x.StartDate,
                        FinishDate = x.FinishDate,
                        CreationTime  = x.CreationTime,
                        LastUpdateTime = x.LastUpdateTime,
                        Order = x.Order,
                        ParentId = x.ParentId,
                        StyleType = x.StyleType
                    }).ToList()
                };
            }
        }

        private static ApiResponseBase GetNews(ApiFilterContent filter, SessionIdentity identity, ILog log)
        {
            using (var contentBl = new ContentBll(identity, log))
            {
                var result = contentBl.GetNews(filter.Id, filter.PartnerId, filter.ParentId, filter.SkipCount, filter.TakeCount);
                return new ApiResponseBase
                {
                    ResponseObject = result.Select(x => new ApiPromotion
                    {
                        Id = x.Id,
                        PartnerId = x.PartnerId,
                        NickName = x.NickName,
                        Type = x.Type,
                        State = x.State,
                        Title = x.Title,
                        Description = x.Description,
                        ImageName = x.ImageName,
                        StartDate = x.StartDate,
                        FinishDate = x.FinishDate,
                        CreationTime = x.CreationTime,
                        LastUpdateTime = x.LastUpdateTime,
                        Order = x.Order,
                        ParentId = x.ParentId,
                        StyleType = x.StyleType
                    }).ToList()
                };
            }
        }

        private static ApiResponseBase GetPromotionById(ApiFilterContent input, SessionIdentity identity, ILog log)
        {
            using (var contentBl = new ContentBll(identity, log))
            {
                var apiPromotion = contentBl.GetPromotionById(input.Id ?? 0).ToApiPromotion(identity.TimeZone);
				var partner = CacheManager.GetPartnerById(apiPromotion.PartnerId);
				var siteurl = partner.SiteUrl.Split(',')[0];
				apiPromotion.SiteUrl = siteurl;

				return new ApiResponseBase
                {
                    ResponseObject = apiPromotion
				};
            }
        }

        private static ApiResponseBase GetNewsById(ApiFilterContent input, SessionIdentity identity, ILog log)
        {
            using (var contentBl = new ContentBll(identity, log))
            {
                var apiNews = contentBl.GetNewsById(input.Id ?? 0).ToApiNews(identity.TimeZone);
                var partner = CacheManager.GetPartnerById(apiNews.PartnerId);
                var siteurl = partner.SiteUrl.Split(',')[0];
                apiNews.SiteUrl = siteurl;

                return new ApiResponseBase
                {
                    ResponseObject = apiNews
                };
            }
        }

        private static ApiResponseBase GetWebSiteMenus(ApiFilterContent input, SessionIdentity identity, ILog log)
        {
            using (var contentBl = new ContentBll(identity, log))
            {
                var result = contentBl.GetWebSiteMenus(input.PartnerId.Value, input.DeviceType);
                if (input.PartnerId.Value == Constants.MainPartnerId)
                    result = result.Where(x => x.Type != Constants.WebSiteConfiguration.Translations).ToList();

                return new ApiResponseBase
                {
                    ResponseObject = result.MapToApiWebSiteMenus()
                };
            }
        }

        private static ApiResponseBase SaveWebSiteMenu(ApiWebSiteMenu apiWebSiteMenu, SessionIdentity identity, ILog log)
        {
            using (var contentBl = new ContentBll(identity, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = contentBl.SaveWebSiteMenu(apiWebSiteMenu.MapToWebSiteMenu()).MapToApiWebSiteMenu()
                };
            }
        }

        private static ApiResponseBase FindSubMenuItemByTitle(ApiWebSiteMenu apiWebSiteMenu, SessionIdentity identity, ILog log)
        {
            using (var contentBl = new ContentBll(identity, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = contentBl.FindSubMenuItemByTitle(apiWebSiteMenu.Id, apiWebSiteMenu.Title)?.MapToApiWebSiteSubMenuItem()
                };
            }
        }

        private static ApiResponseBase GetWebSiteMenuItems(int menuId, SessionIdentity identity, ILog log)
        {
            using (var contentBl = new ContentBll(identity, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = contentBl.GetWebSiteMenuItems(menuId).MapToApiWebSiteMenuItems()
                };
            }
        }

        private static ApiResponseBase GetWebSiteSubMenuItems(int menuItemId, SessionIdentity identity, ILog log)
        {
            using (var contentBl = new ContentBll(identity, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = contentBl.GetWebSiteSubMenuItems(menuItemId).MapToApiWebSiteSubMenuItems()
                };
            }
        }

        private static ApiResponseBase SaveWebSiteMenuItem(ApiWebSiteMenuItem apiWebSiteMenuItem, SessionIdentity identity, ILog log)
        {
            using (var contentBl = new ContentBll(identity, log))
            {
                var res = contentBl.SaveWebSiteMenuItem(apiWebSiteMenuItem.MapToWebSiteMenuItem(), out bool broadcastChanges, out int partnerId).MapToApiWebSiteMenuItem();
                if (broadcastChanges)
                    BaseController.BroadcastCacheChanges(partnerId, string.Format("{0}_{1}", Constants.CacheItems.Restrictions, partnerId));
                Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}_{2}", Constants.CacheItems.ConfigParameters, partnerId, res.Title));
                return new ApiResponseBase
                {
                    ResponseObject = res
                };
            }
        }

        private static ApiResponseBase SaveWebSiteSubMenuItem(ApiWebSiteSubMenuItem apiWebSiteSubMenuItem, SessionIdentity identity, ILog log)
        {
            using (var contentBl = new ContentBll(identity, log))
            {
                var input = apiWebSiteSubMenuItem.MapToWebSiteSubMenuItem();
                var output = contentBl.SaveWebSiteSubMenuItem(input, out bool broadcastChanges);
                Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}_{2}", Constants.CacheItems.ConfigParameters, output.PartnerId, output.MenuItemName));
                if (broadcastChanges)
                    BaseController.BroadcastCacheChanges(input.PartnerId, string.Format("{0}_{1}", Constants.CacheItems.Restrictions, input.PartnerId));
                if (input.PartnerId == Constants.MainPartnerId && output.MenuItemName.Contains(Constants.CacheItems.WhitelistedIps))
                {
                    Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.WhitelistedIps, output.MenuItemName));
                    Helpers.Helpers.InvokeMessage("UpdateWhitelistedIps", output.MenuItemName);
                }
                return new ApiResponseBase
                {
                    ResponseObject = output.MapToApiWebSiteSubMenuItem()
                };
            }
        }

        private static ApiResponseBase RemoveWebSiteMenuItem(int menuItemId, SessionIdentity identity, ILog log)
        {
            using (var contentBl = new ContentBll(identity, log))
            {
                var res = contentBl.RemoveWebSiteMenuItem(menuItemId, out bool broadcastChanges);
                var cacheKey = string.Format("{0}_{1}_{2}", Constants.CacheItems.ConfigParameters, res.Key, res.Value);
                CacheManager.RemoveFromCache(cacheKey);
                Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", cacheKey);
                if (broadcastChanges)
                    BaseController.BroadcastCacheChanges(res.Key, string.Format("{0}_{1}", Constants.CacheItems.Restrictions, res.Key));
                return new ApiResponseBase();
            }
        }

        private static ApiResponseBase RemoveWebSiteSubMenuItem(int subMenuItemId, SessionIdentity identity, ILog log)
        {
            using (var contentBl = new ContentBll(identity, log))
            {
                var res = contentBl.RemoveWebSiteSubMenuItem(subMenuItemId, out bool broadcastChanges);
                var cacheKey = string.Format("{0}_{1}_{2}", Constants.CacheItems.ConfigParameters, res.Key, res.Value);
                CacheManager.RemoveFromCache(cacheKey);
                Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", cacheKey);

                if (broadcastChanges)
                    BaseController.BroadcastCacheChanges(res.Key, string.Format("{0}_{1}", Constants.CacheItems.Restrictions, res.Key));
                if (res.Key == Constants.MainPartnerId && res.Value.Contains(Constants.CacheItems.WhitelistedIps))
                {
                    Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.WhitelistedIps, res.Value));
                    Helpers.Helpers.InvokeMessage("UpdateWhitelistedIps", res.Value);
                }
                return new ApiResponseBase();
            }
        }

        private static ApiResponseBase GetItemTranslations(int subMenuItemId, SessionIdentity identity, ILog log)
        {
            using (var contentBl = new ContentBll(identity, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = contentBl.GetWebSiteTranslations(subMenuItemId)
                };
            }
        }

        private static ApiResponseBase CloneWebSiteMenuByPartnerId(ApiCloneObject apiCloneWebSiteMenu, SessionIdentity identity, ILog log)
        {
            using (var contentBl = new ContentBll(identity, log))
            {
                contentBl.CloneWebSiteMenu(apiCloneWebSiteMenu.FromPartnerId, apiCloneWebSiteMenu.ToPartnerId, apiCloneWebSiteMenu.MenuItemId);
                return new ApiResponseBase();
            }
        }

        private static ApiResponseBase SaveCRMSetting(ApiCRMSetting apiCRMSetting, SessionIdentity identity, ILog log)
        {
            using (var contentBl = new ContentBll(identity, log))
            {
                var setting = apiCRMSetting.ToCRMSetting();
                return new ApiResponseBase
                {
                    ResponseObject = contentBl.SaveCRMSetting(setting).ToApiCRMSetting()
                };
            }
        }

        private static ApiResponseBase GetCRMSettings(SessionIdentity identity, ILog log)
        {
            using (var contentBl = new ContentBll(identity, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = contentBl.GetCRMSettings().Select(x => x.ToApiCRMSetting()).ToList()
                };
            }
        }

        private static ApiResponseBase GetCRMSettingById(int settingId, SessionIdentity identity, ILog log)
        {
            using (var contentBl = new ContentBll(identity, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = contentBl.GetCRMSettingById(settingId).ToApiCRMSetting()
                };
            }
        }

        private static ApiResponseBase GetMessageTemplates(SessionIdentity identity, ILog log)
        {
            using (var contentBl = new ContentBll(identity, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = contentBl.GetMessageTemplates().Select(x => x.MapToMessageTemplateModel()).ToList()
                };
            }
        }

        private static ApiResponseBase SaveMessageTemplate(MessageTemplateModel messageTemplate, SessionIdentity identity, ILog log)
        {
            using (var contentBl = new ContentBll(identity, log))
            {
                var result = contentBl.SaveMessageTemplate(messageTemplate.MapToMessageTemplate()).MapToMessageTemplateModel();
                Helpers.Helpers.InvokeMessage("RemoveKeysFromCache", string.Format("{0}_{1}_{2}_", Constants.CacheItems.MessageTemplates, result.PartnerId, result.ClientInfoType));
                return new ApiResponseBase
                {
                    ResponseObject = result
                };
            }
        }
        private static ApiResponseBase RemoveMessageTemplate(int templateId, SessionIdentity identity, ILog log)
        {
            var contentBl = new ContentBll(identity, log);

            contentBl.RemoveMessageTemplate(templateId, out int partnerId);
            CacheManager.RemoveMessageTemplate(partnerId, templateId);
            Helpers.Helpers.InvokeMessage("RemoveMessageTemplates", partnerId);
            var languages = CacheManager.GetAvailableLanguages();
            foreach (var lan in languages)
            {
                BaseController.BroadcastCacheChanges(partnerId, string.Format("{0}_{1}_{2}", Constants.CacheItems.MessageTemplates, partnerId, lan.Id));
            }

            return new ApiResponseBase();
        }

        private static ApiResponseBase GetObjectTranslations(ObjectModel objectModel, SessionIdentity identity, ILog log)
        {
            using (var contentBl = new ContentBll(identity, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = contentBl.GetObjectTranslations(objectModel.ObjectTypeId, objectModel.ObjectId, objectModel.LanguageId)
                };
            }
        }

        private static ApiResponseBase UploadConfigFile(FileUploadInput input, SessionIdentity identity, ILog log)
        {
            using (var contentBl = new ContentBll(identity, log))
            {
                using (var partnerBl = new PartnerBll(contentBl))
                {
                    var partner = CacheManager.GetPartnerById(input.PartnerId);
                    if (partner == null)
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.PartnerNotFound);
                    var ftpModel = partnerBl.GetPartnerEnvironments(input.PartnerId)[input.EnvironmentTypeId];
                    if (ftpModel == null)
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.PartnerKeyNotFound);

                    contentBl.GenerateConfigFile(input.PartnerId, ftpModel);
                    contentBl.GenerateAssets(input.PartnerId, ftpModel);
                }
            }
            return new ApiResponseBase();
        }

        private static ApiResponseBase UploadMenus(FileUploadInput input, SessionIdentity identity, ILog log)
        {
            using (var contentBl = new ContentBll(identity, log))
            {
                using (var partnerBl = new PartnerBll(contentBl))
                {
                    var partner = CacheManager.GetPartnerById(input.PartnerId);
                    if (partner == null)
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.PartnerNotFound);
                    var ftpModel = partnerBl.GetPartnerEnvironments(input.PartnerId)[input.EnvironmentTypeId];
                    if (ftpModel == null)
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.PartnerKeyNotFound);
                    contentBl.GeneratePartnerSettings(input.PartnerId, ftpModel);
                }
            }
            return new ApiResponseBase();
        }

        private static ApiResponseBase UploadWebSiteStylesFile(FileUploadInput input, SessionIdentity identity, ILog log)
        {
            using (var contentBl = new ContentBll(identity, log))
            {
                using (var partnerBl = new PartnerBll(contentBl))
                {
                    var partner = CacheManager.GetPartnerById(input.PartnerId);
                    if (partner == null)
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.PartnerNotFound);
                    var ftpModel = partnerBl.GetPartnerEnvironments(input.PartnerId)[input.EnvironmentTypeId];
                    if (ftpModel == null)
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.PartnerKeyNotFound);
                    contentBl.GenerateWebSiteStylesFile(input.PartnerId, ftpModel);
                }
                return new ApiResponseBase();
            }
        }

        private static ApiResponseBase UploadWebSiteTranslations(FileUploadInput input, SessionIdentity identity, ILog log)
        {
            using (var partnerBl = new PartnerBll(identity, log))
            {
                using (var contentBl = new ContentBll(partnerBl))
                {
                    var partner = CacheManager.GetPartnerById(input.PartnerId);
                    if (partner == null)
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.PartnerNotFound);
                    var ftpModel = partnerBl.GetPartnerEnvironments(input.PartnerId)[input.EnvironmentTypeId];
                    if (ftpModel == null)
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.PartnerKeyNotFound);
                    contentBl.GenerateWebSiteAllTranslations(input.PartnerId, ftpModel);
                }
            }
            return new ApiResponseBase();
        }

        private static ApiResponseBase UploadAdminTranslations(SessionIdentity identity, ILog log)
        {
            using (var partnerBl = new PartnerBll(identity, log))
            {
                using (var contentBl = new ContentBll(partnerBl))
                {
                    var ftpModel = partnerBl.GetPartnerEnvironments(Constants.MainPartnerId).FirstOrDefault();
                    if (ftpModel.Value == null)
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.PartnerKeyNotFound);

                    contentBl.GenerateAdminTranslations(ftpModel.Value);
                }
            }
            return new ApiResponseBase();
        }

        private static ApiResponseBase UploadWebSitePromotions(FileUploadInput input, SessionIdentity identity, ILog log)
        {
            using (var partnerBl = new PartnerBll(identity, log))
            {
                using (var contentBl = new ContentBll(partnerBl))
                {
                    var partner = CacheManager.GetPartnerById(input.PartnerId);
                    if (partner == null)
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.PartnerNotFound);
                    var ftpModel = partnerBl.GetPartnerEnvironments(input.PartnerId)[input.EnvironmentTypeId];
                    if (ftpModel == null)
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.PartnerKeyNotFound);
                    contentBl.GenerateWebSiteAllPromotions(input.PartnerId, ftpModel);
                }
            }
            return new ApiResponseBase();
        }

        private static ApiResponseBase UploadWebSiteNews(FileUploadInput input, SessionIdentity identity, ILog log)
        {
            using (var partnerBl = new PartnerBll(identity, log))
            {
                using (var contentBl = new ContentBll(partnerBl))
                {
                    var partner = CacheManager.GetPartnerById(input.PartnerId);
                    if (partner == null)
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.PartnerNotFound);
                    var ftpModel = partnerBl.GetPartnerEnvironments(input.PartnerId)[input.EnvironmentTypeId];
                    if (ftpModel == null)
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.PartnerKeyNotFound);
                    contentBl.GenerateWebSiteAllNews(input.PartnerId, ftpModel);
                }
            }
            return new ApiResponseBase();
        }

        private static ApiResponseBase UploadImage(FileUploadInput input, SessionIdentity identity, ILog log)
        {
            using (var partnerBl = new PartnerBll(identity, log))
            {
                using (var contentBl = new ContentBll(partnerBl))
                {
                    var partner = CacheManager.GetPartnerById(input.PartnerId);
                    if (partner == null)
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.PartnerNotFound);
                    var ftpModel = partnerBl.GetPartnerEnvironments(input.PartnerId)[input.EnvironmentTypeId];
                    if (ftpModel == null)
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.PartnerKeyNotFound);
                    var path = input.ImageType.ToLower() != "main" ? string.Format("{0}/assets/images/{1}/{2}", partner.Name.ToLower(), input.ImageType.ToLower(), input.Image)
                        : string.Format("{0}/assets/images/{1}", partner.Name.ToLower(), input.Image);
                    contentBl.UploadFile(input.ImageData, "/coreplatform/website/" + path, ftpModel);
                }
            }
            return new ApiResponseBase();
        }

        private static ApiResponseBase SaveAnnouncement(ApiAnnouncement apiAnnouncement, SessionIdentity identity, ILog log)
        {
            using (var contentBl = new ContentBll(identity, log))
                contentBl.SaveAnnouncement(apiAnnouncement, true, null);
            if (apiAnnouncement.Type == (int)AnnouncementTypes.Ticker)
                Helpers.Helpers.InvokeMessage("RemoveKeysFromCache", string.Format("{0}_{1}_", Constants.CacheItems.Ticker, apiAnnouncement.PartnerId));
            return new ApiResponseBase();
        }

        public static ApiResponseBase GetAnnouncements(ApiFilterAnnouncement apiAnnouncement, SessionIdentity identity, ILog log)
        {
            using (var contentBl = new ContentBll(identity, log))
            {
                var announcements = contentBl.GetAnnouncements(apiAnnouncement.MapToFilterAnnouncement(), true);
                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        announcements.Count,
                        Entities = announcements.Entities.Select(x => x.MapToApiAnnouncement(identity.TimeZone)).ToList()
                    }
                };
            }
        }

        public static ApiResponseBase GetAnnouncementById(int announcementId, SessionIdentity identity, ILog log)
        {
            using (var contentBl = new ContentBll(identity, log))
            {
                var announcement = contentBl.GetAnnouncementById(announcementId);
                announcement.CreationDate = announcement.CreationDate.GetGMTDateFromUTC(identity.TimeZone);
                announcement.LastUpdateDate = announcement.LastUpdateDate.GetGMTDateFromUTC(identity.TimeZone);
                return new ApiResponseBase
                {
                    ResponseObject = announcement
                };
            }
        }

        public static ApiResponseBase GetCommentTemplates(string commentType, SessionIdentity identity, ILog log)
        {
            using (var contentBl = new ContentBll(identity, log))
            {
                int? commentTypeId = null;
                if (int.TryParse(commentType, out int id))
                    commentTypeId = id;
                var res = contentBl.GetCommentTemplates(commentTypeId, null);
                return new ApiResponseBase
                {
                    ResponseObject = res.Select(x => new
                    {
                        x.Id,
                        Name = x.NickName,
                        NickName = x.Text,
                        x.PartnerId,
                        x.Type
                    })
                };
            }
        }

        public static ApiResponseBase SaveCommentTemplate(CommentTemplate commentTemplate, SessionIdentity identity, ILog log)
        {
            using (var contentBl = new ContentBll(identity, log))
            {
                var res = contentBl.SaveCommentTemplate(commentTemplate);
                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        res.Id,
                        res.NickName,
                        res.PartnerId,
                        res.Type
                    }
                };
            }
        }

        public static ApiResponseBase RemoveCommentTemplate(CommentTemplate commentTemplate, SessionIdentity identity, ILog log)
        {
            using (var contentBl = new ContentBll(identity, log))
            {
                contentBl.RemoveCommentTemplate(commentTemplate);
                Helpers.Helpers.InvokeMessage("RemoveCommentTemplateFromCache", commentTemplate.PartnerId, commentTemplate.Type);
                return new ApiResponseBase();
            }
        }

        public static ApiResponseBase GetWebflowItems(ApiWebflowInput input, int type, SessionIdentity identity, ILog log)
        {
            input.WebflowItemType = type;
            return new ApiResponseBase
            {
                ResponseObject = Integration.Platforms.Helpers.Webflow.GetWebflowItemByType(input, identity.LanguageId)

            };
        }

        public static ApiResponseBase GetJobAreas(SessionIdentity identity, ILog log)
        {
            using (var contentBl = new ContentBll(identity, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = contentBl.GetJobAreas(true).Select(x => new { x.Id, x.NickName, x.Info, x.TranslationId }).ToList()
                };
            }
        }

        public static ApiResponseBase SaveJobArea(JobArea jobArea, SessionIdentity identity, ILog log)
        {
            using (var contentBl = new ContentBll(identity, log))
            {
                var res = contentBl.SaveJobArea(jobArea);
                Helpers.Helpers.InvokeMessage("RemoveJobAreasFromCache", string.Empty);
                return new ApiResponseBase
                {
                    ResponseObject = new { res.Id, res.NickName, res.Info, res.TranslationId }
                };
            }
        }

        public static ApiResponseBase SaveSegment(SegmentModel segmentModel, SessionIdentity identity, ILog log)
        {
            using (var contentBl = new ContentBll(identity, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = contentBl.SaveSegment(segmentModel).MapToSegmentModel(identity.TimeZone)
                };
            }
        }

        public static ApiResponseBase DeleteSegment(int segmentId, SessionIdentity identity, ILog log)
        {
            using (var contentBl = new ContentBll(identity, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = contentBl.DeleteSegment(segmentId).MapToSegmentModel(identity.TimeZone)
                };
            }
        }

        public static ApiResponseBase GetSegments(ApiBaseFilter filter, SessionIdentity identity, ILog log)
        {
            using (var contentBl = new ContentBll(identity, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = contentBl.GetSegments(filter.Id, filter.PartnerId).Select(x => x.MapToSegmentModel(identity.TimeZone))
                                                                                       .OrderByDescending(x => x.Id).ToList()
                };
            }
        }

        private static ApiResponseBase GetAdminTranslations(SessionIdentity identity, ILog log)
        {
            using (var contentBl = new ContentBll(identity, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = contentBl.GetAdminTranslations().Select(x => x.MapToApiWebSiteMenuItem()).ToList()
                };
            }
        }

        private static ApiResponseBase GetAdminTranslationItems(int menuItemId, SessionIdentity identity, ILog log)
        {
            using (var contentBl = new ContentBll(identity, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = contentBl.GetAdminTranslationItems(menuItemId).MapToApiWebSiteSubMenuItems()
                };
            }
        }

        private static ApiResponseBase SaveAdminTranslation(ApiWebSiteMenuItem apiWebSiteMenuItem, SessionIdentity identity, ILog log)
        {
            using (var contentBl = new ContentBll(identity, log))
            {
                var res = contentBl.SaveAdminTramslation(apiWebSiteMenuItem.MapToWebSiteMenuItem()).MapToApiWebSiteMenuItem();
                
                return new ApiResponseBase
                {
                    ResponseObject = res
                };
            }
        }

        private static ApiResponseBase SaveAdminTranslationItem(ApiWebSiteSubMenuItem apiWebSiteSubMenuItem, SessionIdentity identity, ILog log)
        {
            using (var contentBl = new ContentBll(identity, log))
            {
                var input = apiWebSiteSubMenuItem.MapToWebSiteSubMenuItem();
                var output = contentBl.SaveAdminTranslationItem(input);
               
                return new ApiResponseBase
                {
                    ResponseObject = output.MapToApiWebSiteSubMenuItem()
                };
            }
        }

        private static ApiResponseBase RemoveAdminTranslation(int menuItemId, SessionIdentity identity, ILog log)
        {
            using (var contentBl = new ContentBll(identity, log))
            {
                contentBl.RemoveAdminTranslation(menuItemId);
                return new ApiResponseBase();
            }
        }

        private static ApiResponseBase RemoveAdminTranslationItem(int subMenuItemId, SessionIdentity identity, ILog log)
        {
            using (var contentBl = new ContentBll(identity, log))
            {
                contentBl.RemoveAdminTranslationItem(subMenuItemId);
                return new ApiResponseBase();
            }
        }

        private static ApiResponseBase GetAdminItemTranslations(int subMenuItemId, SessionIdentity identity, ILog log)
        {
            using (var contentBl = new ContentBll(identity, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = contentBl.GetAdminTranslations(subMenuItemId)
                };
            }
        }
    }
}