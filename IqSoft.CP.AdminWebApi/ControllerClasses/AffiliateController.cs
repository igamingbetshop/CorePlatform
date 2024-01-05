using IqSoft.CP.AdminWebApi.ClientModels.Models;
using IqSoft.CP.AdminWebApi.Filters;
using IqSoft.CP.AdminWebApi.Helpers;
using IqSoft.CP.AdminWebApi.Models.AffiliateModels;
using IqSoft.CP.AdminWebApi.Models.CommonModels;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.DAL.Models;
using log4net;
using Newtonsoft.Json;
using System.Linq;

namespace IqSoft.CP.AdminWebApi.ControllerClasses
{
    public static class AffiliateController
    {
        public static ApiResponseBase CallFunction(RequestBase request, SessionIdentity identity, ILog log)
        {
            switch (request.Method)
            {
                case "GetAffiliates":
                    return GetfnAffiliates(
                        JsonConvert.DeserializeObject<ApiFilterfnAffiliate>(request.RequestData), identity, log);
                case "GetAffiliateById":
                    return GetAffiliateById(JsonConvert.DeserializeObject<int>(request.RequestData), identity, log);
                case "UpdateAffiliate":
                    return UpdateAffiliate(
                        JsonConvert.DeserializeObject<ApiFnAffiliateModel>(request.RequestData), identity, log);
                case "UpdateCommissionPlan":
                    return UpdateCommissionPlan(JsonConvert.DeserializeObject<Common.Models.AffiliateModels.ApiAffiliateCommission>(request.RequestData), identity, log);
            }
            throw BaseBll.CreateException(string.Empty, Constants.Errors.MethodNotFound);
        }

        private static ApiResponseBase GetfnAffiliates(ApiFilterfnAffiliate filter, SessionIdentity identity, ILog log)
        {
            using (var affiliateBl = new AffiliateService(identity, log))
            {
                var input = filter.MapToFilterfnAffiliate();
                var resp = affiliateBl.GetfnAffiliates(input);
                return new ApiResponseBase
                {
                    ResponseObject = new { resp.Count, Entities = resp.Entities.Select(x => x.ToApifnAffiliateModel(identity.TimeZone)).ToList() }
                };
            }
        }

        private static ApiResponseBase GetAffiliateById(int id, SessionIdentity identity, ILog log)
        {
            using (var affiliateBl = new AffiliateService(identity, log))
            {
                var resp = affiliateBl.GetAffiliateById(id, true);
                return new ApiResponseBase
                {
                    ResponseObject = resp.ToApifnAffiliateModel(identity.TimeZone)
                };
            }
        }

        private static ApiResponseBase UpdateAffiliate(ApiFnAffiliateModel input, SessionIdentity identity, ILog log)
        {
            using (var affiliateBl = new AffiliateService(identity, log))
			{
				var partner = CacheManager.GetPartnerById(identity.PartnerId);
				identity.Domain = partner.SiteUrl.Split(',')[0];
				var resp = affiliateBl.UpdateAffiliate(input.ToFnAffiliate());
				return new ApiResponseBase();
            }
        }

        private static ApiResponseBase UpdateCommissionPlan(Common.Models.AffiliateModels.ApiAffiliateCommission input, SessionIdentity identity, ILog log)
        {
            using (var affiliateBl = new AffiliateService(identity, log))
            {
                affiliateBl.UpdateCommissionPlan(input);
                return new ApiResponseBase();
            }
        }
    }
}