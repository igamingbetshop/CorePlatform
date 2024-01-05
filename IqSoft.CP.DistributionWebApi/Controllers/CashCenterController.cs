using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;

namespace IqSoft.CP.DistributionWebApi.Controllers
{
    [RoutePrefix("cashcenter")]
    public class CashCenterController : ApiController
    {
        [Route("redirectrequest"), HttpGet]
        public HttpResponseMessage RedirectRequest([FromUri] string code)
        {
            var currentDomain = HttpContext.Current.Request.Url.Host;
            currentDomain = currentDomain.Replace("distribution.", string.Empty);
            var script = "<!DOCTYPE html><html lang='en'><head>" +
                "<script>window.location.href=\"https://" + currentDomain + "?ExternalPlatformId=1&Code=" + code+"\"</script>" +
                "</head></html>";
            HttpResponseMessage response = Request.CreateResponse(System.Net.HttpStatusCode.OK);
            response.Content = new StringContent(script, System.Text.Encoding.UTF8, "text/html");
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }
    }
}