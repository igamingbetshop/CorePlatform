using System.Linq;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.DAL.Filters;
using IqSoft.CP.AdminWebApi.Helpers;
using Newtonsoft.Json;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Common;
using IqSoft.CP.AdminWebApi.Models.CommonModels;
using log4net;

namespace IqSoft.CP.AdminWebApi.ControllerClasses
{
    public static class RegionController
    {
        public static ApiResponseBase CallFunction(RequestBase request, SessionIdentity identity, ILog log)
        {
            switch (request.Method)
            {
                case "GetRegions":
                    return GetRegions(JsonConvert.DeserializeObject<FilterRegion>(request.RequestData), identity, log);
                case "SaveRegion":
                    return SaveRegion(JsonConvert.DeserializeObject<FnRegionModel>(request.RequestData), identity, log);
            }
            throw BaseBll.CreateException(string.Empty, Constants.Errors.MethodNotFound);
        }

        private static ApiResponseBase GetRegions(FilterRegion filter, SessionIdentity identity, ILog log)
        {
            using (var regionBl = new RegionBll(identity, log))
            {
                return new ApiResponseBase
                {
                    ResponseObject = regionBl.GetfnRegions(filter, identity.LanguageId, true, null).OrderBy(x => x.Name).MapTofnRegionModels()
                };
            }
        }

        private static ApiResponseBase SaveRegion(FnRegionModel region, SessionIdentity identity, ILog log)
        {
            using (var regionBl = new RegionBll(identity, log))
            {
                var result = regionBl.SaveRegion(region.MapToRegion());

                return new ApiResponseBase
                {
                    ResponseObject = regionBl.GetfnRegionById(result.Id).MapTofnRegionModel()
                };
            }
        }
    }
}