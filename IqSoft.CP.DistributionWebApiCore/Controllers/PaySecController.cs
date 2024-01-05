using Microsoft.AspNetCore.Mvc;
using System.Collections.Specialized;
using System.Web;

namespace IqSoft.CP.DistributionWebApi.Controllers
{
    [Route("paysec")]
    [ApiController]
    public class PaySecController : ControllerBase
    {
        private static readonly string PaymentForm = "<form method='post' action='{0}'>" +
                                                    "<input type='text' id='token' name='token' size='50' value='{1}'>" +
                                                    "<input type='submit' value='Submit' id='sendtoken'>" +
                                                    "</form><script>document.getElementById('sendtoken').click();</script>";

        [Route("paymentrequest"), HttpGet]
        public ActionResult PaymentRequest(string endpoint)
        {
            var queryString = new NameValueCollection(HttpUtility.ParseQueryString(Request.QueryString.ToString()));
            return Ok(string.Format(PaymentForm, endpoint, queryString["params"]));
        }
    }
}