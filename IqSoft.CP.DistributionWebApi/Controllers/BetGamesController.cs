using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

namespace IqSoft.CP.DistributionWebApi.Controllers
{
    [RoutePrefix("betgames")]
    public class BetGamesController : ApiController
    {
        [Route("launchgame"), HttpGet]
        public HttpResponseMessage LaunchGame([FromUri]string iframe, [FromUri]string lang, [FromUri]string token, [FromUri]string partner,
                                              [FromUri]string home, [FromUri]decimal timeZone, [FromUri] bool isMobile)
        {
            var script = "<script type=\"text/javascript\"> " +
                         "var _bt = _bt || []; " +
                         "_bt.push(['server', '" + iframe + "']);" +
                         "_bt.push(['partner', '" + partner + "']); " +
                         "_bt.push(['token', '" + token + "']); " +
                         "_bt.push(['language', '" + lang + "']); " +
                         "_bt.push(['timezone', '" + timeZone + "']); " +
                         "_bt.push(['is_mobile', '" + isMobile + "']); " +
                         "_bt.push(['current_game', '" + -1 + "']); " + // will redirect to Lobby page
                         "_bt.push(['home_url', '" + home + "']); " +
                         "(function(){ " +
                         " document.write('<' + 'script type=\"text/javascript\" src=\"" + iframe + "/design/client/js/betgames.js?ts=' + Date.now() + '\"><' + '/script>'); " +
                         "})();</script> " +
                         "<script type=\"text/javascript\"> BetGames.frame(_bt);</script> ";
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(script, System.Text.Encoding.UTF8, "text/html");
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }
    }
}