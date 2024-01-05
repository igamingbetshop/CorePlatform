using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

namespace IqSoft.CP.DistributionWebApi.Controllers
{
    [RoutePrefix("tvbet")]
    public class TVBetController : ApiController
    {
        [Route("launchgame"), HttpGet]
        public HttpResponseMessage LaunchGame([FromUri]string iframe, [FromUri]string lang, [FromUri]string token, [FromUri]string partner, [FromUri]string exitUrl)
        {
            var script = "<!DOCTYPE html><html lang='en'><head>" +
                "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no\">"+
                "<script type=\"text/javascript\" src=\"https://" +iframe+"/assets/frame.js\"></script>"+
                "<script>(function() {"+
                "new TvbetFrame({"+
                "'lng' : '" + lang + "', " +
                "'clientId' : '" + partner + "', "+
                "'tokenAuth' : '" + token + "', " +
                "'server' : 'https://" + iframe + "', " +
                "'floatTop' : '#fTop', " +
                "'containerId' : 'tvbet-game', " +
                "'exitUrl' : 'https://" + exitUrl + "'});  })();</script>" +
                "</form></body></html>";

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(script, System.Text.Encoding.UTF8, "text/html");
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }
    }
}
