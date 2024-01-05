using IqSoft.CP.DistributionWebApi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;

namespace IqSoft.CP.DistributionWebApi.Controllers
{
    [Route("omid")]
    [ApiController]
    public class OmidController : ControllerBase
    {
        [Route("NotifyResult"), HttpGet, HttpPost]
        public ActionResult NotifyResult([FromQuery] string orderId, [FromQuery]string returnUrl, [FromQuery] string domain)
        {
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = HttpMethod.Get,
                Url = string.Format("https://{0}/api/Omid/ApiRequest?paymentRequestId={1}", domain, orderId)
            };
            Common.SendHttpRequest(httpRequestInput, out _);
            var script = "<!DOCTYPE html><html lang='en'><head>" +
                "<script>window.location.href=\"https://" + returnUrl + "\"</script>" +
                "</head></html>";
            return Ok(script);
        }
    }
}
