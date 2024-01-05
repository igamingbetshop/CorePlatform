using Microsoft.AspNetCore.Mvc;

namespace IqSoft.CP.DistributionWebApi.Controllers
{
    [Route("betgames")]
    [ApiController]
    public class BetGamesController : ControllerBase
    {
        [Route("launchgame"), HttpGet]
        public ActionResult LaunchGame([FromQuery]string iframe, [FromQuery] string lang, [FromQuery] string token, [FromQuery] string partner,
                                              [FromQuery]string home, [FromQuery] decimal timeZone, [FromQuery] bool isMobile)
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
            return Ok(script);
        }
    }
}