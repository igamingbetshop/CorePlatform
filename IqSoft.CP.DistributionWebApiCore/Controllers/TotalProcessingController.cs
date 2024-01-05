using IqSoft.CP.DistributionWebApi.Models;
using IqSoft.CP.DistributionWebApi.Models.TotalProcessing;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;

namespace IqSoft.CP.DistributionWebApi.Controllers
{
    [Route("totalprocessing")]
	[ApiController]
	public class TotalProcessingController : ControllerBase
    {
		[Route("LoadPaymentPage"), HttpGet]
		public ActionResult LoadPaymentPage([FromQuery]string redirectUrl, [FromQuery] string checkoutId, [FromQuery] string paymentWay, [FromQuery] string website)
        {
            var url = string.Format("https://oppwa.com/v1/paymentWidgets.js?checkoutId={0}", checkoutId);

            var script = "<!DOCTYPE html><html lang='en'><head>"+
                "<style>body {background-color:#f6f6f5;}</style>"+
                "<script>var wpwlOptions = {style:\"card\"}</script><script src=\""+url+"\"></script>"+
                "</head><body><form action=\"https://distribution."+ website + "/TotalProcessing/CheckPaymentStatus?redirectUrl=" + redirectUrl + "\" class=\"paymentWidgets\" data-brands=\"" + paymentWay+"\"></form></body></html>";
			return Ok(script);
        }

		[Route("CheckPaymentStatus"), HttpGet]
		public ActionResult CheckPaymentStatus([FromQuery] string redirectUrl, [FromQuery] string resourcePath)
		{
			var httpRequestInput = new HttpRequestInput
			{
				ContentType = "application/x-www-form-urlencoded",
				RequestMethod = HttpMethod.Get,
				Url = "https://oppwa.com" + resourcePath
			};
			var resp = JsonConvert.DeserializeObject<StatusResponse>(Common.SendHttpRequest(httpRequestInput, out _));

			httpRequestInput = new HttpRequestInput
			{
				ContentType = "application/json",
				RequestMethod = HttpMethod.Get,
				Url = string.Format("https://paymentgateway.iqsoftllc.com/api/TotalProcessing/ApiRequest?paymentRequestId={0}&status={1}", resp.merchantTransactionId, resp.result.code)
			};
			Common.SendHttpRequest(httpRequestInput, out _);
			var script = "<!DOCTYPE html><html lang='en'><head>" +
				"<script>window.location.href=\"" + redirectUrl + "\"</script>" +
				"</head></html>";
			Response.ContentType = "text/html";
			return Ok(script);
		}
	}
}
