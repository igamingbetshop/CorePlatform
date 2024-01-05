using IqSoft.CP.DistributionWebApi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;

namespace IqSoft.CP.DistributionWebApi.Controllers
{
    [Route("neteller")]
    [ApiController]
    public class NetellerController : ControllerBase
    {
        [Route("NotifyResult"), HttpGet]
        public ActionResult NotifyResult([FromQuery] string requestId, [FromQuery] string redirectUrl, [FromQuery] string notifyUrl)
        {
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = HttpMethod.Get,
                Url = string.Format("{0}/api/Neteller/ApiRequest?paymentRequestId={1}", notifyUrl, requestId)
            };
            Common.SendHttpRequest(httpRequestInput, out _);
            var script = "<!DOCTYPE html><html lang='en'><head>" +
                "<script>window.location.href=\"" + redirectUrl + "\"</script>" +
                "</head></html>";
            return Ok(script);
        }
    }
}
