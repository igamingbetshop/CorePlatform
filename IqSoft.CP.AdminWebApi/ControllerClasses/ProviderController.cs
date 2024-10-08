using IqSoft.CP.AdminWebApi.Helpers;
using IqSoft.CP.AdminWebApi.Models.CommonModels;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.DAL.Models;
using log4net;
using System.Linq;

namespace IqSoft.CP.AdminWebApi.ControllerClasses
{
    public static class ProviderController
    {
        public static ApiResponseBase CallFunction(RequestBase request, SessionIdentity identity, ILog log)
        {
            switch (request.Method)
            {
                case "GetAffiliatePlatforms":
                    return GetAffiliatePlatforms(identity, log);
                case "GetNotificationServices":
                    return GetNotificationServices(identity, log);
            }
            throw BaseBll.CreateException(string.Empty, Constants.Errors.MethodNotFound);
        }

        public static ApiResponseBase GetAffiliatePlatforms(SessionIdentity identity, ILog log)
        {
            using (var providerBl = new ProviderBll(identity, log))
            {
                var result = providerBl.GetAffiliatePlatforms();
                return new ApiResponseBase
                {
                    ResponseObject =
                        result.Select(
                            x => x.MapToAffiliatePlatformModel(identity.TimeZone))
                };
            }
        }

        public static ApiResponseBase GetNotificationServices(SessionIdentity identity, ILog log)
        {
            using (var providerBl = new ProviderBll(identity, log))
            {
                var result = providerBl.GetNotificationServices();
                return new ApiResponseBase
                {
                    ResponseObject =
                        result.Select(
                            x => x.MapToNotificationServiceModel(identity.TimeZone))
                };
            }
        }
    }
}