using IqSoft.CP.DistributionWebApi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;

namespace IqSoft.CP.DistributionWebApi.Controllers
{
    [Route("payone")]
    [ApiController]
    public class PayOneController : ControllerBase
    {
        [Route("NotifyResult"), HttpGet]
        public ActionResult NotifyResult([FromQuery] string id, [FromQuery] string url, [FromQuery] string notifyUrl)
        {
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = HttpMethod.Get,
                Url = string.Format("https://{0}/api/PayOne/ApiRequest?paymentRequestId={1}", notifyUrl, id)
            };
            Common.SendHttpRequest(httpRequestInput, out _);
            var script = "<!DOCTYPE html><html lang='en'><head>" +
                "<script>window.location.href=\"" + url + "\"</script>" +
                "</head></html>";
            return Ok(script);
        }

        [Route("NotifyStatus"), HttpGet]
        public ActionResult NotifyStatus([FromQuery]string id, [FromQuery]string url, [FromQuery]string notifyUrl)
        {
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = HttpMethod.Get,
                Url = string.Format("https://{0}/api/PayOne/ApiRequest?paymentRequestId={1}", notifyUrl, id)
            };
            Common.SendHttpRequest(httpRequestInput, out _);
            var script = "<!DOCTYPE html><html lang='en'><head>" +
                "<script>window.location.href=\"https://" + url + "\"</script>" +
                "</head></html>";
            return Ok(script);
        }
    }
}
