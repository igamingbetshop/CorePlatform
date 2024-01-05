using IqSoft.CP.DistributionWebApi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;

namespace IqSoft.CP.DistributionWebApi.Controllers
{
    [Route("notify")]
    [ApiController]
    public class NotifyController : ControllerBase
    {
        [Route("NotifyResult"), HttpGet, HttpPost]
        public ActionResult NotifyResult([FromQuery] string providerName, [FromQuery] string methodName,
                                                [FromQuery] string orderId, [FromQuery] string returnUrl, [FromQuery] string domain)
        {
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = HttpMethod.Get,
                Url = string.Format("{0}/api/{1}/{2}?paymentRequestId={3}", domain, providerName, methodName, orderId)
            };
            Common.SendHttpRequest(httpRequestInput, out _);
            var script = "<!DOCTYPE html><html lang='en'><head>" +
                "<script>window.location.href=\"" + returnUrl + "\"</script>" +
                "</head></html>";
            return Ok(script);
        }
    }
}