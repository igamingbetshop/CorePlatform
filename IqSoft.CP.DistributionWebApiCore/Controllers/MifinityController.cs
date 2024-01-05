using Microsoft.AspNetCore.Mvc;

namespace IqSoft.CP.DistributionWebApi.Controllers
{
    [Route("mifinity")]
    [ApiController]
    public class MifinityController : ControllerBase
    {
		[Route("init"), HttpGet]
		public ActionResult InitPayment([FromQuery]string it, [FromQuery]string e, string d)
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
            return Ok(script);
		}
	}
}