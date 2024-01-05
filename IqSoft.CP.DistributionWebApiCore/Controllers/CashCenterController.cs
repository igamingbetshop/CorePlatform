using Microsoft.AspNetCore.Mvc;

namespace IqSoft.CP.DistributionWebApi.Controllers
{
    [Route("cashcenter")]
    [ApiController]
    public class CashCenterController : ControllerBase
    {
        [Route("redirectrequest"), HttpGet]
        public ActionResult RedirectRequest([FromQuery] string code)
        {
            var currentDomain = Request.Host.Host;
            currentDomain = currentDomain.Replace("distribution.", string.Empty);
            var script = "<!DOCTYPE html><html lang='en'><head>" +
                "<script>window.location.href=\"https://" + currentDomain + "?ExternalPlatformId=1&Code=" + code+"\"</script>" +
                "</head></html>";
            return Ok(script);
        }
    }
}