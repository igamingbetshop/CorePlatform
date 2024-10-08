using System;
using System.IO;
using System.Net;
using System.Text;
using static IqSoft.CP.Common.Constants;
using IqSoft.CP.Common.Models.WebSiteModels;
using Newtonsoft.Json;
using Serilog;
using System.Net.Http;

namespace IqSoft.CP.WebSiteWebApi.Common
{
	public static class MasterCacheIntegration
	{
        #region Common
 
        private static string SendHttpRequest(HttpRequestInput input)
		{
            try
            {
                using var httpClient = new HttpClient() { Timeout = TimeSpan.FromMilliseconds(60000) };
                using var request = new HttpRequestMessage(input.RequestMethod, input.Url);                
                if (!string.IsNullOrEmpty(input.PostData))
                    request.Content = new StringContent(input.PostData, Encoding.UTF8, input.ContentType);
                if (input.RequestHeaders != null)
                {
                    foreach (var headerValuePair in input.RequestHeaders)
                    {
                        httpClient.DefaultRequestHeaders.Add(headerValuePair.Key, headerValuePair.Value);
                    }
                }
                var response = httpClient.Send(request);
                return response.Content.ReadAsStringAsync().Result;              
            }
            catch (Exception e)
            {
                Log.Error(e, "_Input_" + JsonConvert.SerializeObject(input));
                string message = string.Format("Remote server exception: {0}", e.Message);
                throw new WebException(message);
            }
        }

        public static T SendMasterCacheRequest<T>(int partnerId, string functionName, object inp) where T : ApiResponseBase
        {
            var input = new HttpRequestInput
            {
                ContentType = HttpContentTypes.ApplicationJson,
                RequestMethod = HttpMethod.Post,
                PostData = JsonConvert.SerializeObject(inp),
                Url = string.Format("{0}/{1}/api/main/{2}",
                Program.AppSetting.MasterCacheConnectionUrl[new Random().Next(0, Program.AppSetting.MasterCacheConnectionUrl.Count - 1)], partnerId, functionName)
            };
            return JsonConvert.DeserializeObject<T>(SendHttpRequest(input));
        }

        #endregion
    }
}