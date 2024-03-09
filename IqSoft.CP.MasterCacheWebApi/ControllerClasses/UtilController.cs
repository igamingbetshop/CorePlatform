using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Integration.Platforms.Helpers;
using log4net;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Web.Http;

namespace IqSoft.CP.MasterCacheWebApi.ControllerClasses
{
    public class UtilController : ApiController
    {
        public static ApiResponseBase CallFunction(RequestBase request, SessionIdentity session, ILog log)
        {
            switch (request.Method)
            {
                case "OpenTicketingSystem":
                    return OpenTicketingSystem(Convert.ToInt32(request.RequestData), session);

                default:
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.MethodNotFound);
            }
        }

        private static ApiResponseBase OpenTicketingSystem(int clientId, SessionIdentity session)
        {
            var client = CacheManager.GetClientById(clientId);
            if (string.IsNullOrEmpty(client.Email))
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.EmailCantBeEmpty);
            if (client.IsEmailVerified)
                throw BaseBll.CreateException(session.LanguageId, Constants.Errors.EmailNotVerified);
            return new ApiResponseBase
            {
                ResponseObject = new { Url = TicketingSystem.CallTicketSystemApi(clientId, session) }
            };
        }
    }
}