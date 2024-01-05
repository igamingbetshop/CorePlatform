using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.Common.Models.WebSiteModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel;
using System.Text;
using IqSoft.CP.ProductGateway.Models.MGTCompliance;
using System.Web.Http;

namespace IqSoft.CP.ProductGateway.Controllers
{
    public class MGTComplianceController : ApiController
    {
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps("MGTComplianceWhitelistedIps");

        [HttpPost]
        [Route("{partnerId}/api/MGTCompliance/ApiRequest")]
        public HttpResponseMessage GetPlayerInfo(VerifyResult verifyResult)
        {
            var response = new ApiResponseBase();
            try
            {
                // BaseBll.CheckIp(WhitelistedIps);
                //var services = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.MGTComplianceService).StringValue.Split(',');

               
                
            }
            catch (FaultException<BllFnErrorType> fex)
            {
              //  WebApiApplication.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_   ErrorCode: " + fex.Detail?.Id + "_   ErrorMessage: " + fex.Detail?.Message);
                response.ResponseCode = fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id;
                response.Description = fex.Detail == null ? fex.Message : fex.Detail.Message;
            }
            catch (Exception ex)
            {
             //  WebApiApplication.DbLogger.Error("Input: " + JsonConvert.SerializeObject(input) + "_   ErrorMessage: " + ex.Message);
                response.ResponseCode = Constants.Errors.GeneralException;
                response.Description = ex.Message;
            }
            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(response), Encoding.UTF8)
            };
            httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationJson);
            return httpResponseMessage;
        }
    }
}