using IqSoft.CP.DistributionWebApi.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

namespace IqSoft.CP.DistributionWebApi.Controllers
{
    [RoutePrefix("omid")]
    public class OmidController : ApiController
    {
        [HttpGet, HttpPost]
        public HttpResponseMessage NotifyResult([FromUri]string orderId, [FromUri]string returnUrl, [FromUri]string domain)
        {
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = "GET",
                Url = string.Format("https://{0}/api/Omid/ApiRequest?paymentRequestId={1}", domain, orderId)
            };
            Common.SendHttpRequest(httpRequestInput, out string contentType);
            var script = "<!DOCTYPE html><html lang='en'><head>" +
                "<script>window.location.href=\"https://" + returnUrl + "\"</script>" +
                "</head></html>";
            HttpResponseMessage response = Request.CreateResponse(System.Net.HttpStatusCode.OK);
            response.Content = new StringContent(script, System.Text.Encoding.UTF8, "text/html");
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }
    }
}
