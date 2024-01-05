using System.Linq;
using Newtonsoft.Json;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using log4net;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.AgentWebApi.Models;
using IqSoft.CP.AgentWebApi.Filters;
using IqSoft.CP.AgentWebApi.Helpers;
using IqSoft.CP.AgentWebApi.Models.ProductModels;

namespace IqSoft.CP.AgentWebApi.ControllerClasses
{
    public static class ProductController
    {
        internal static ApiResponseBase CallFunction(RequestBase request, SessionIdentity identity, ILog log)
        {
            switch (request.Method)
            {
                case "GetProducts":
                    return GetProducts(JsonConvert.DeserializeObject<ApiFilterfnProduct>(request.RequestData), identity, log);
                case "GetGameProviders":
                    return GetGameProviders(JsonConvert.DeserializeObject<ApiFilterGameProvider>(request.RequestData), identity, log);
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

                    return new ApiResponseBase
                    {
                        ResponseObject = products.Entities.Select(x => x.MapTofnProductModel()).ToList()
                    };
                }
            }
        }

        private static ApiResponseBase GetGameProviders(ApiFilterGameProvider filter, SessionIdentity identity, ILog log)
        {
            using (var productsBl = new ProductBll(identity, log))
            {
                using (var userBl = new UserBll(productsBl))
                {
                    var user = userBl.GetUserById(identity.Id);
                    if (user == null)
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.UserNotFound);
                    var providers = productsBl.GetGameProviders(filter.MapToFilterGameProvider(), !(user.Type == (int)UserTypes.MasterAgent ||
                            user.Type == (int)UserTypes.Agent || user.Type == (int)UserTypes.AgentEmployee));
                    var result = new ApiResponseBase
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
                    return result;
                }
            }
        }
       
	}
}