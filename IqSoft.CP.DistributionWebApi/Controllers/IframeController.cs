using IqSoft.CP.Common.Helpers;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using IqSoft.CP.DistributionWebApi.Models;

namespace IqSoft.CP.DistributionWebApi.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "GET,POST")]
    [RoutePrefix("iframe")]
    public class IframeController : ApiController
    {
        [Route("iframeurl"), HttpGet]
        public HttpResponseMessage GetIframeUrl([FromUri]string data)
        {
            var queryString = Uri.UnescapeDataString(HttpContext.Current.Request.QueryString.ToString());
            var inputString = AESEncryptHelper.DecryptDistributionString(queryString.Replace("data=", string.Empty));

            var input = JsonConvert.DeserializeObject<IframeInput>(inputString);

            var script = "<!DOCTYPE html><html lang=\"en\"><head><script> " +
                         "var externalData = {{PartnerId:{0}, ClientId:{1}, Token:'{2}', RedirectUrl:'{3}', ResourcesUrl:'{4}', LanguageId:'{6}', Domain:'{7}' }} </script></head><body>" +
                         "<script src=\"{4}/iframe/{5}/init.js\"></script></body></html>";

            script = string.Format(script, input.PartnerId, input.ClientId, input.Token, input.RedirectUrl, input.ResourcesUrl,
                                input.PlatformName, input.LanguageId, input.Domain);

            HttpResponseMessage response = Request.CreateResponse(System.Net.HttpStatusCode.OK);
            response.Content = new StringContent(script, Encoding.UTF8, "text/html");
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }
    }
}