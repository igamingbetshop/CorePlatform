using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.Common.Models.WebSiteModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel;
using System.Text;
using IqSoft.CP.ProductGateway.Models.Insic;
using System.Web.Http;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Models.Clients;

namespace IqSoft.CP.ProductGateway.Controllers
{
    public class InsicController : ApiController
    {
        public static List<string> WhitelistedIps = CacheManager.GetProviderWhitelistedIps("Insic");

        private static readonly Dictionary<string, int> VerificationServiceNames = new Dictionary<string, int>
        {
            {"schufa.identity", 1 },
            {"schufa.bankAccount", 2 } ,
            {"ebics.1cent", 3 },
            {"face.recognition", 4 },
            {"schufa.credit", 5 },
            {"finapi-service", 6 }
            //"document.reader",
            //"yes-identity"
        };

        private static readonly List<string> SanctionsServiceNames = new List<string>
        {
            "tolerant.sanctions",
            "tolerant.pep"
        };

        [HttpPost]
        [Route("{partnerId}/api/Insic/ApiRequest")]
        public HttpResponseMessage ApiRequest(HttpRequestMessage httpRequestMessage, [FromUri]int partnerId )
        {
            var response = new ApiResponseBase();
            var inputString = httpRequestMessage.Content.ReadAsStringAsync().Result;
            try
            {
                // BaseBll.CheckIp(WhitelistedIps);
                WebApiApplication.DbLogger.Info("InputString: " + inputString);
                var input = JsonConvert.DeserializeObject<VerificationResult>(inputString);
                WebApiApplication.DbLogger.Info("Input: " + JsonConvert.SerializeObject(input));
                var client = CacheManager.GetClientByEmail(partnerId, input.Email) ??
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                using (var clientBl = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    if (VerificationServiceNames.ContainsKey(input.ServiceName))
                    {
                        clientBl.SaveClientSetting(client.Id, $"{Constants.ClientSettings.VerificationServiceName}_{VerificationServiceNames[input.ServiceName]}",
                                                   input.ServiceName, input.ServiceStatus, DateTime.UtcNow);
                        if(VerificationServiceNames[input.ServiceName] == 5 || VerificationServiceNames[input.ServiceName] == 6)
                            clientBl.SaveClientSetting(client.Id, Constants.ClientSettings.LimitConfirmed, true.ToString(), 1, DateTime.UtcNow);
                        if (input.ServiceStatus == (int)VerificationStatuses.SUCCESS)
                            clientBl.ChangeClientVerificationStatus(client.Id, true);
                    }
                    else if (SanctionsServiceNames.Contains(input.ServiceName) && input.ServiceStatus == (int)VerificationStatuses.FAILURE)
                    {
                        clientBl.ChangeClientState(client.Id, (int)ClientStates.Suspended, null);
                        var clientSettings = new ClientCustomSettings
                        {
                            ClientId = client.Id,
                            UnderMonitoring = (int)UnderMonitoringTypes.PEPSanction
                        };
                        clientBl.SaveClientSetting(clientSettings);
                    }
                }
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                WebApiApplication.DbLogger.Error("InputString: " + inputString + "_   ErrorCode: " + fex.Detail?.Id + "_   ErrorMessage: " + fex.Detail?.Message);
                response.ResponseCode = fex.Detail == null ? Constants.Errors.GeneralException : fex.Detail.Id;
                response.Description = fex.Detail == null ? fex.Message : fex.Detail.Message;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error("InputString: " + inputString + "_   ErrorMessage: " + ex);
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