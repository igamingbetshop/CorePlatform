using IqSoft.CP.DistributionWebApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace IqSoft.CP.DistributionWebApi.Controllers
{
    [Route("cardtocard")]
    [ApiController]
    public class CardToCardController : ControllerBase
    {
        [Route("NotifyResult"), HttpGet, HttpPost]
        public ActionResult NotifyResult([FromQuery] string orderId, [FromQuery] string url)
        {
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = System.Net.Http.HttpMethod.Get,
                Url = string.Format("https://paymentgateway.iqsoftllc.com/api/CardToCard/ApiRequest?paymentRequestId={0}", orderId)
            };
            Common.SendHttpRequest(httpRequestInput, out _);
            var script = "<!DOCTYPE html><html lang='en'><head>" +
                "<script>window.location.href=\"https://" + url + "\"</script>" +
                "</head></html>";
            return Ok(script);
        }
    }
}