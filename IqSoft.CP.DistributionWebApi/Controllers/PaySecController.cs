using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Web.Http;

namespace IqSoft.CP.DistributionWebApi.Controllers
{
    [RoutePrefix("paysec")]
    public class PaySecController : ApiController
    {
        private static readonly string PaymentForm = "<form method='post' action='{0}'>" +
                                                    "<input type='text' id='token' name='token' size='50' value='{1}'>" +
                                                    "<input type='submit' value='Submit' id='sendtoken'>" +
                                                    "</form><script>document.getElementById('sendtoken').click();</script>";

        [Route("paymentrequest"), HttpGet]
        public HttpResponseMessage PaymentRequest(string endpoint)
        {
            var queryString = HttpContext.Current.Request.QueryString;
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(string.Format(PaymentForm, endpoint, queryString["params"]), Encoding.UTF8, "text/html");
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }
    }
}