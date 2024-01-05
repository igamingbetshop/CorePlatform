using IqSoft.CP.Common.Helpers;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Web.Http;

namespace IqSoft.CP.DistributionWebApi.Controllers
{
    [RoutePrefix("paymentasia")]
    public class PaymentAsiaController : ApiController
    {
        private static readonly string PaymentForm = "<body onload=\"document.forms[0].submit()\"><form action=\"{0}\" method=\"post\" target=\"\">{1}</form></body>";
        private static readonly string ItemTemplate = "<input type=\"hidden\" name=\"{0}\" value=\"{1}\" /> ";

        [Route("paymentrequest"), HttpGet]
        public HttpResponseMessage PaymentRequest([FromUri] string p)
        {
            var input = Uri.UnescapeDataString(HttpContext.Current.Request.QueryString.ToString());
            input = AESEncryptHelper.DecryptDistributionString(input.Substring(2, input.Length - 2));
            var requestBody = input.Split(new string[] { "apiUrl=" }, StringSplitOptions.None);
            var queryString = HttpUtility.ParseQueryString(requestBody[0]);
            var queryParams = new StringBuilder();
            foreach (var item in queryString.AllKeys)
            {
                if (!string.IsNullOrEmpty(item))
                    queryParams.AppendLine(string.Format(ItemTemplate, item, queryString[item]));
            }
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
            var pameters = string.Format(PaymentForm, AESEncryptHelper.DecryptDistributionString(requestBody[1]), queryParams.ToString());
            response.Content =  new StringContent(pameters, Encoding.UTF8, "text/html");
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }
    }
}