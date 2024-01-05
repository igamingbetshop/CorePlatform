using IqSoft.CP.DistributionWebApi.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

namespace IqSoft.CP.DistributionWebApi.Controllers
{
    [RoutePrefix("notify")]
    public class NotifyController : ApiController
    {
        [HttpPost]
        [HttpGet]
        [Route("NotifyResult")]
        public HttpResponseMessage NotifyResult([FromUri]string providerName, [FromUri]string methodName, 
                                                [FromUri]string orderId, [FromUri]string returnUrl, [FromUri]string domain)
        {
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = "GET",
                Url = string.Format("{0}/api/{1}/{2}?paymentRequestId={3}", domain, providerName, methodName, orderId)
            };
            Common.SendHttpRequest(httpRequestInput, out string contentType);
            var script = "<!DOCTYPE html><html lang='en'><head>" +
                "<script>window.location.href=\"" + returnUrl + "\"</script>" +
                "</head></html>";
            HttpResponseMessage response = Request.CreateResponse(System.Net.HttpStatusCode.OK);
            response.Content = new StringContent(script, System.Text.Encoding.UTF8, "text/html");
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }
    }
}
