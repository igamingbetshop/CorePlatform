using Microsoft.AspNetCore.Mvc;

namespace IqSoft.CP.DistributionWebApi.Controllers
{
    [Route("tvbet")]
    [ApiController]
    public class TVBetController : ControllerBase
    {
        [Route("launchgame"), HttpGet]
        public ActionResult LaunchGame([FromQuery]string iframe, [FromQuery]string lang, [FromQuery]string token, [FromQuery]string partner, [FromQuery]string exitUrl)
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
            return Ok(script);
        }
    }
}
