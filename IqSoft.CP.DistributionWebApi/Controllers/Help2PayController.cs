using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Web.Http;

namespace IqSoft.CP.DistributionWebApi.Controllers
{
    [RoutePrefix("help2pay")]
    public class Help2PayController : ApiController
    {
        private static readonly string PaymentForm = "<!DOCTYPE html>\r\n<html>\r\n<head>\r\n    <title></title>\r\n</head>\r\n<body>\r\n" +
            "<form method=\"post\" name=\"SendForm\" action=\"{0}\">\r\n {1}" +
            "id='sendtoken'></form>\r\n</body>\r\n</html>\r\n<script>history.forward();</script>\r\n<script>SendForm.submit();</script>";
        private static readonly string ItemTemplate = "<input type='hidden' name='{0}' value='{1}' /> ";

        [Route("paymentrequest"), HttpGet]
        public HttpResponseMessage PaymentRequest(string apiName)
        {
            var queryString = HttpContext.Current.Request.QueryString;
            var queryParams = new StringBuilder();
            foreach (var item in queryString.AllKeys)
            {
                queryParams.AppendLine(string.Format(ItemTemplate, item, queryString[item]));
            }

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(string.Format(PaymentForm, apiName, queryParams.ToString()), Encoding.UTF8, "text/html");
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }

        [Route("errorRequest"), HttpGet]
        public HttpResponseMessage ErrorRequest()
        {
            var queryString = HttpContext.Current.Request.QueryString;
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(queryString["params"], Encoding.UTF8, "text/html");
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }
    }
}