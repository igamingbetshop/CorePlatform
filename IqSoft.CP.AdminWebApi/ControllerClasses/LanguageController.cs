using System;
using System.Linq;
using Newtonsoft.Json;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.AdminWebApi.Helpers;
using IqSoft.CP.AdminWebApi.Models.CommonModels;
using log4net;

namespace IqSoft.CP.AdminWebApi.ControllerClasses
{
    public static class LanguageController
    {
        public static ApiResponseBase CallFunction(RequestBase request, SessionIdentity identity, ILog log)
        {
            switch (request.Method)
            {
                case "GetLanguages":
                    return GetLanguages(identity, log);
                case "GetPartnerLanguageSettings":
                    return GetPartnerLanguageSettings(Convert.ToInt32(request.RequestData), identity, log);
                case "SavePartnerLanguageSetting":
                    return SavePartnerLanguageSetting(JsonConvert.DeserializeObject<PartnerLanguageSetting>(request.RequestData), identity, log);
            }
            throw BaseBll.CreateException(string.Empty, Constants.Errors.MethodNotFound);
        }

        private static ApiResponseBase GetLanguages(SessionIdentity identity, ILog log)
        {
            using (var languageBl = new LanguageBll(identity, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = languageBl.GetLanguages()
                };
            }
        }

        private static ApiResponseBase GetPartnerLanguageSettings(int partnerId, SessionIdentity identity, ILog log)
        {
            using (var languageBl = new LanguageBll(identity, log))
            {
                var partnerLanguages = languageBl.GetPartnerLanguages(partnerId).MapToPartnerLanguageSettingModels(languageBl.GetUserIdentity().TimeZone);
                var ids = partnerLanguages.Select(x => x.LanguageId).ToList();
                var languages = languageBl.GetLanguages().Where(x => !ids.Contains(x.Id)).Select(x => new { x.Id, x.Name }).ToList();

                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        partnerLanguages,
                        languages
                    }
                };
            }
        }

        private static ApiResponseBase SavePartnerLanguageSetting(PartnerLanguageSetting partnerLanguageSetting, SessionIdentity identity, ILog log)
        {
            using (var languageBl = new LanguageBll(identity, log))
            {
                var result = languageBl.SavePartnerLanguageSetting(partnerLanguageSetting);
                return new ApiResponseBase
                {
                    ResponseObject = new
                    {
                        Id = result.Id,
                        PartnerId = result.PartnerId,
                        LanguageId = result.LanguageId,
                        CreationTime = result.CreationTime,
                        LastUpdateTime = result.LastUpdateTime,
                        State = result.State,
                        Order = result.Order
                    }
                };
            }
        }
    }
}