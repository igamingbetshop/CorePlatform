using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

namespace IqSoft.CP.DistributionWebApi.Controllers
{
    [RoutePrefix("mifinity")]
    public class MifinityController : ApiController
    {
		[Route("init"), HttpGet]
		public HttpResponseMessage InitPayment(string it, string e, string d)
		{
			var script = $"<script src=\"https://{e}.mifinity.com/widgets/sgpg.js?58190a411dc3\"></script>" +
                         $"<div id=\"widget-container\"></div>" +
                         "<script> var widget = showPaymentIframe(\"widget-container\", { " +
                         $"token: \"{it}\", " + 
                         "fail: function() { window.location = " +
                         $"\"https://{d}/user/1/deposit\"; }}, " + 
                         "success: function() { window.location = " +
                         $"\"https://{d}/user/1/deposit\"; }}, " + 
                         "complete: function() { " +
                         "setTimeout(function() { " +
                         "}, 5000);} });</script>";
			HttpResponseMessage response = Request.CreateResponse(System.Net.HttpStatusCode.OK);
			response.Content = new StringContent(script, System.Text.Encoding.UTF8, "text/html");
			response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
			return response;
		}
    }
}