using System.Collections.Generic;
using log4net;
using IqSoft.CP.Common;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.AdminWebApi.Filters;
using IqSoft.CP.AdminWebApi.Helpers;
using Newtonsoft.Json;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.AdminWebApi.Models.CommonModels;
using Microsoft.AspNet.SignalR;
using IqSoft.CP.AdminWebApi.Hubs;

namespace IqSoft.CP.AdminWebApi.ControllerClasses
{
    public static class BaseController
    {
        private static readonly dynamic _connectedClients = GlobalHost.ConnectionManager.GetHubContext<WebSiteHub>().Clients;

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
            using (var utilBl = new UtilBll(identity, log))
            {
                var response = new ApiResponseBase
                {
                    ResponseObject = utilBl.GetObjectTypes().MapToObjectTypeModels()
                };
                return response;
            }
        }

        private static ApiResponseBase GetTranslationEntries(ApiFilterTranslationEntry filter, SessionIdentity identity, ILog log)
        {
            using (var utilBl = new UtilBll(identity, log))
            {
                var result = utilBl.GetfnTranslationEntriesPagedModel(filter.MaptToFilterTranslation());

                return new ApiResponseBase
                {
                    ResponseObject = result
                };
            }
        }

        private static ApiResponseBase SaveTranslationEntries(IEnumerable<TranslationEntryModel> translations, SessionIdentity identity, ILog log)
        {
            using (var contentBl = new ContentBll(identity, log))
            {
                using (var partnerBl = new PartnerBll(contentBl))
                {
                    var resp = contentBl.SaveTranslationEntries(translations.MapToTranslationEntries(), out Dictionary<string, int> broadcastKey);
                    foreach (var key in broadcastKey)
                    {
                        Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", key.Key);
                        if (key.Value != 0)
                            BroadcastCacheChanges(key.Value, key.Key);
                        else
                        {
                            var partnerIds = partnerBl.GetActivePartnerIds();
                            foreach(var id in partnerIds)
                                BroadcastCacheChanges(id, key.Key);
                        }
                    }
                    return new ApiResponseBase
                    {
                        ResponseObject = resp.MapToTranslationModels()
                    };
                }
            }
        }

        public static void BroadcastCacheChanges(int partnerId, string key)
        {
            _connectedClients.Group("WebSiteWebApi").BroadcastCacheChanges("CoreSlave_" + key);
        }
    }
}