using IqSoft.CP.DistributionWebApi.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

namespace IqSoft.CP.DistributionWebApi.Controllers
{
    [RoutePrefix("cardtocard")]
    public class CardToCardController : ApiController
    {
        [HttpGet, HttpPost]
        public HttpResponseMessage NotifyResult([FromUri]string orderId, [FromUri]string url)
        {
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = "GET",
                Url = string.Format("https://paymentgateway.iqsoftllc.com/api/CardToCard/ApiRequest?paymentRequestId={0}", orderId)
            };
            Common.SendHttpRequest(httpRequestInput, out string contentType);
            var script = "<!DOCTYPE html><html lang='en'><head>" +
                "<script>window.location.href=\"https://" + url + "\"</script>" +
                "</head></html>";
            HttpResponseMessage response = Request.CreateResponse(System.Net.HttpStatusCode.OK);
            response.Content = new StringContent(script, System.Text.Encoding.UTF8, "text/html");
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }
    }
}
