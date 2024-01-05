using IqSoft.CP.DistributionWebApi.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

namespace IqSoft.CP.DistributionWebApi.Controllers
{
    [RoutePrefix("neteller")]
    public class NetellerController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage NotifyResult([FromUri] string requestId, [FromUri] string redirectUrl, [FromUri] string notifyUrl)
        {
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = "GET",
                Url = string.Format("{0}/api/Neteller/ApiRequest?paymentRequestId={1}", notifyUrl, requestId)
            };
            Common.SendHttpRequest(httpRequestInput, out string contentType);
            var script = "<!DOCTYPE html><html lang='en'><head>" +
                "<script>window.location.href=\"" + redirectUrl + "\"</script>" +
                "</head></html>";
            HttpResponseMessage response = Request.CreateResponse(System.Net.HttpStatusCode.OK);
            response.Content = new StringContent(script, System.Text.Encoding.UTF8, "text/html");
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }
    }
}
