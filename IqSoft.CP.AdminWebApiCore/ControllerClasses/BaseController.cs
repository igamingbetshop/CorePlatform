using System.Collections.Generic;
using log4net;
using IqSoft.CP.Common;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.AdminWebApi.Filters;
using IqSoft.CP.AdminWebApi.Helpers;
using Newtonsoft.Json;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.AdminWebApi.Models.CommonModels;
using IqSoft.CP.AdminWebApi.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace IqSoft.CP.AdminWebApi.ControllerClasses
{
    public static class BaseController
    {
        public static ApiResponseBase CallFunction(RequestBase request, SessionIdentity identity, ILog log)
        {
            switch (request.Method)
            {
                case "GetObjectTypes":
                    return GetObjectTypes(identity, log);
                case "GetTranslationEntries":
                    return
                        GetTranslationEntries(
                            JsonConvert.DeserializeObject<ApiFilterTranslationEntry>(request.RequestData), identity, log);
                case "SaveTranslationEntries":
                    return
                        SaveTranslationEntries(
                            JsonConvert.DeserializeObject<List<TranslationEntryModel>>(request.RequestData), identity, log);
            }
            throw BaseBll.CreateException(string.Empty, Constants.Errors.MethodNotFound);
        }

        private static ApiResponseBase GetObjectTypes(SessionIdentity identity, ILog log)
        {
            using var utilBl = new UtilBll(identity, log);
            var response = new ApiResponseBase
            {
                ResponseObject = utilBl.GetObjectTypes().MapToObjectTypeModels()
            };
            return response;
        }

        private static ApiResponseBase GetTranslationEntries(ApiFilterTranslationEntry filter, SessionIdentity identity, ILog log)
        {
            using var utilBl = new UtilBll(identity, log);
            return new ApiResponseBase
            {
                ResponseObject = utilBl.GetfnTranslationEntriesPagedModel(filter.MaptToFilterTranslation())
            };
        }

        private static ApiResponseBase SaveTranslationEntries(IEnumerable<TranslationEntryModel> translations, SessionIdentity identity, ILog log)
        {
            using var contentBl = new ContentBll(identity, log);
            var resp = contentBl.SaveTranslationEntries(translations.MapToTranslationEntries(), out Dictionary<string, int> broadcastKey);
            foreach (var key in broadcastKey)
            {
                Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", key.Key);
                BroadcastCacheChanges(key.Value, key.Key);
            }
            return new ApiResponseBase
            {
                ResponseObject = resp.MapToTranslationModels()
            };
        }

        public static void BroadcastCacheChanges(int partnerId, string key)
        {
            WebSiteHub.CurrentContext?.Clients.Group("WebSiteWebApi_" + partnerId).SendAsync("BroadcastCacheChanges", "CoreSlave_" + key);
        }
    }
}