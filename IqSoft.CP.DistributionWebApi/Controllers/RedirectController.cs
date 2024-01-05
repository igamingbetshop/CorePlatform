using IqSoft.CP.Common.Helpers;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using IqSoft.CP.DistributionWebApi.Models.PaymentProcessing;
using System.Web;
using IqSoft.CP.DistributionWebApi.Models;
using System;

namespace IqSoft.CP.DistributionWebApi.Controllers
{
    [RoutePrefix("redirect")]
    public class RedirectController : ApiController
    {
        [Route("redirectrequest"), HttpPost, HttpGet]
        public HttpResponseMessage RedirectRequest([FromUri] string redirectUrl)
        {
            var script = "<!DOCTYPE html><html lang='en'><head>" +
                "<script>window.location.href=\"" + redirectUrl + "\"</script>" +
                "</head></html>";
            HttpResponseMessage response = Request.CreateResponse(System.Net.HttpStatusCode.OK);
            response.Content = new StringContent(script, System.Text.Encoding.UTF8, "text/html");
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }

        [Route("rp"), HttpGet]
        public HttpResponseMessage RedirectPaymentRequest(HttpRequestMessage httpRequestMessage)
        {
            var query= Uri.UnescapeDataString(HttpContext.Current.Request.QueryString.ToString());
            var redirectData =query.Split('&')[0].Replace("rd=", string.Empty);

            RedirectDataModel redirectDataObject;
            try
            {
                redirectDataObject = JsonConvert.DeserializeObject<RedirectDataModel>(AESEncryptHelper.DecryptDistributionString(redirectData + "="));
            }
            catch
            {
                redirectDataObject = JsonConvert.DeserializeObject<RedirectDataModel>(AESEncryptHelper.DecryptDistributionString(redirectData));
            }
            var queryString = HttpContext.Current.Request.QueryString;
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = "GET",
                Url = string.Format("{0}?{1}", redirectDataObject.PaymentGatewayUrl, queryString.ToString())
            };
            Common.SendHttpRequest(httpRequestInput, out string contentType);
            var script = "<!DOCTYPE html><html lang='en'><head>" +
                "<script>window.location.href=\"" + redirectDataObject.CashierPageUrl + "\"</script>" +
                "</head></html>";
            HttpResponseMessage response = Request.CreateResponse(System.Net.HttpStatusCode.OK);
            response.Content = new StringContent(script, System.Text.Encoding.UTF8, "text/html");
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }
    }
}