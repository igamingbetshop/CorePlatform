using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.WebSiteWebApi.Common;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Reflection;
using static IqSoft.CP.Common.Constants;

namespace IqSoft.CP.WebSiteWebApi.Controllers
{
    [Route("{partnerId}/api/Integration/[action]")]
    [ApiController]
    public class IntegrationController : ControllerBase
    {
        [HttpPost]
        public ApiResponseBase OpenGame(int partnerId, OpenGameInput input)
        {
            try
            {
                input.PartnerId = partnerId;
                return MasterCacheIntegration.SendMasterCacheRequest<ApiResponseBase>(partnerId, MethodBase.GetCurrentMethod().Name, input);
            }
            catch(Exception e)
            {
                return new ApiResponseBase { ResponseCode = Errors.GeneralException };
            }
        }

        [HttpPost]
        public ApiResponseBase GetGameReport(int partnerId, OpenGameInput input)
        {
            try
            {
                input.PartnerId = partnerId;
                return MasterCacheIntegration.SendMasterCacheRequest<ApiResponseBase>(partnerId, MethodBase.GetCurrentMethod().Name, input);
            }
            catch (Exception e)
            {
                return new ApiResponseBase { ResponseCode = Errors.GeneralException };
            }
        }

        [HttpPost]
        public ApiResponseBase GetBetInfo(int partnerId, RequestBase input)
        {
            try
            {
                input.PartnerId = partnerId;
                return MasterCacheIntegration.SendMasterCacheRequest<ApiResponseBase>(partnerId, MethodBase.GetCurrentMethod().Name, input);
            }
            catch (Exception e)
            {
                return new ApiResponseBase { ResponseCode = Errors.GeneralException };
            }
        }
    }
}