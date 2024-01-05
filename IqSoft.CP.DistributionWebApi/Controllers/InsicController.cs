using System;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Newtonsoft.Json;
using IqSoft.CP.DistributionWebApi.Models.Insic;
using IqSoft.CP.Common.Helpers;

namespace IqSoft.CP.DistributionWebApi.Controllers
{
    [RoutePrefix("insic")]
    public class InsicController : ApiController
    {
        [Route("widget"), HttpGet]
        public HttpResponseMessage InitWidget(HttpRequestMessage httpRequestMessage)
        {
            var query = Uri.UnescapeDataString(HttpContext.Current.Request.QueryString.ToString());
            var redirectData = query.Split('&')[0].Replace("rd=", string.Empty);

            var input = JsonConvert.DeserializeObject<RequestData>(AESEncryptHelper.DecryptDistributionString(redirectData));

            var script = "<script> window.__AVS_CONFIG__ = { " +
                         $"apiUrl: 'https://{input.Mode}.insic.de/api',  " +
                         $"resourceUrl: 'https://{input.Mode}.insic.de/frontend{input.Frontend}', " +
                         $"partnerId: '{input.PartnerId}', " +
                         $"startRoute: '/registration', " +
                         $"registrationType: 'simple', lang: '{input.Lang}', " +
                         $"token: '{input.UserToken}' " +
                         "}</script><div id=\"___avs-wrapper\"></div><script " +
                         $"src=\"https://{input.Mode}.insic.de/frontend{input.Frontend}/static/js/avs-loader.min.js\"></script>";
            HttpResponseMessage response = Request.CreateResponse(System.Net.HttpStatusCode.OK);
            response.Content = new StringContent(script, System.Text.Encoding.UTF8, "text/html");
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }
    }
}