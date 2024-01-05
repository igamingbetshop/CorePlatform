using IqSoft.CP.DistributionWebApi.Models;
using IqSoft.CP.DistributionWebApi.Models.TotalProcessing;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

namespace IqSoft.CP.DistributionWebApi.Controllers
{
    [RoutePrefix("totalprocessing")]
    public class TotalProcessingController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage LoadPaymentPage([FromUri]string redirectUrl, [FromUri]string checkoutId, [FromUri]string paymentWay, [FromUri]string website)
        {
            var url = string.Format("https://oppwa.com/v1/paymentWidgets.js?checkoutId={0}", checkoutId);

            var script = "<!DOCTYPE html><html lang='en'><head>"+
                "<style>body {background-color:#f6f6f5;}</style>"+
                "<script>var wpwlOptions = {style:\"card\"}</script><script src=\""+url+"\"></script>"+
                "</head><body><form action=\"https://distribution."+ website + "/TotalProcessing/CheckPaymentStatus?redirectUrl=" + redirectUrl + "\" class=\"paymentWidgets\" data-brands=\"" + paymentWay+"\"></form></body></html>";


            HttpResponseMessage response = Request.CreateResponse(System.Net.HttpStatusCode.OK);
            response.Content = new StringContent(script, System.Text.Encoding.UTF8, "text/html");
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }

		[HttpGet]
		public HttpResponseMessage CheckPaymentStatus([FromUri]string redirectUrl, [FromUri]string resourcePath)
		{
			var httpRequestInput = new HttpRequestInput
			{
				ContentType = "application/x-www-form-urlencoded",
				RequestMethod = "GET",
				Url = "https://oppwa.com" + resourcePath
			};
			string contentType;
			var resp = JsonConvert.DeserializeObject<StatusResponse>(Common.SendHttpRequest(httpRequestInput, out contentType));

			httpRequestInput = new HttpRequestInput
			{
				ContentType = "application/json",
				RequestMethod = "GET",
				Url = string.Format("https://paymentgateway.iqsoftllc.com/api/TotalProcessing/ApiRequest?paymentRequestId={0}&status={1}", resp.merchantTransactionId, resp.result.code)
			};
			Common.SendHttpRequest(httpRequestInput, out contentType);
			var script = "<!DOCTYPE html><html lang='en'><head>" +
				"<script>window.location.href=\"" + redirectUrl + "\"</script>" +
				"</head></html>";
			HttpResponseMessage response = Request.CreateResponse(System.Net.HttpStatusCode.OK);
			response.Content = new StringContent(script, System.Text.Encoding.UTF8, "text/html");
			response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
			return response;
		}
	}
}
