using Microsoft.AspNetCore.Mvc;
using System.Collections.Specialized;
using System.Text;
using System.Web;

namespace IqSoft.CP.DistributionWebApi.Controllers
{
    [Route("help2pay")]
    [ApiController]
    public class Help2PayController : ControllerBase
    {
        private static readonly string PaymentForm = "<!DOCTYPE html>\r\n<html>\r\n<head>\r\n    <title></title>\r\n</head>\r\n<body>\r\n" +
            "<form method=\"post\" name=\"SendForm\" action=\"{0}\">\r\n {1}" +
            "id='sendtoken'></form>\r\n</body>\r\n</html>\r\n<script>history.forward();</script>\r\n<script>SendForm.submit();</script>";
        private static readonly string ItemTemplate = "<input type='hidden' name='{0}' value='{1}' /> ";

        [Route("paymentrequest"), HttpGet]
        public ActionResult PaymentRequest(string apiName)
        {
            var queryString = new NameValueCollection(HttpUtility.ParseQueryString(Request.QueryString.ToString()));
            var queryParams = new StringBuilder();
            foreach (var item in queryString.AllKeys)
            {
                queryParams.AppendLine(string.Format(ItemTemplate, item, queryString[item]));
            }
            return Ok(string.Format(PaymentForm, apiName, queryParams.ToString()));
        }

        [Route("errorRequest"), HttpGet]
        public ActionResult ErrorRequest()
        {
            var queryString = new NameValueCollection(HttpUtility.ParseQueryString(Request.QueryString.ToString()));
            return Ok(queryString["params"]);
        }
    }
}