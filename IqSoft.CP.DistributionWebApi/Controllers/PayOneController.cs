using System.Net.Http;
using System.Web.Http;
using System.Net.Http.Headers;
using IqSoft.CP.DistributionWebApi.Models;

namespace IqSoft.CP.DistributionWebApi.Controllers
{
    [RoutePrefix("payone")]
    public class PayOneController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage NotifyResult([FromUri]string id, [FromUri]string url, [FromUri]string notifyUrl)
        {
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = "GET",
                Url = string.Format("{0}/api/PayOne/ApiRequest?paymentRequestId={1}", notifyUrl, id)
            };
            Common.SendHttpRequest(httpRequestInput, out string contentType);
            var script = "<!DOCTYPE html><html lang='en'><head>" +
                "<script>window.location.href=\"" + url + "\"</script>" +
                "</head></html>";
            HttpResponseMessage response = Request.CreateResponse(System.Net.HttpStatusCode.OK);
            response.Content = new StringContent(script, System.Text.Encoding.UTF8, "text/html");
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }

        [HttpGet]
        public HttpResponseMessage NotifyStatus([FromUri] string id, [FromUri] string url, [FromUri] string notifyUrl)
        {
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = "GET",
                Url = string.Format("https://{0}/api/PayOne/ApiRequest?paymentRequestId={1}", notifyUrl, id)
            };
            Common.SendHttpRequest(httpRequestInput, out string contentType);
            var script = "<!DOCTYPE html><html lang='en'><head>" +
                "<script>window.location.href=\"" + url + "\"</script>" +
                "</head></html>";
            HttpResponseMessage response = Request.CreateResponse(System.Net.HttpStatusCode.OK);
            response.Content = new StringContent(script, System.Text.Encoding.UTF8, "text/html");
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }
    }
}
