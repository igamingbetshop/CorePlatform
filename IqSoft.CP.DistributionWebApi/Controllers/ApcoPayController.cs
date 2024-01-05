using IqSoft.CP.Common.Helpers;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Xml;

namespace IqSoft.CP.DistributionWebApi.Controllers
{
    [RoutePrefix("apcopay")]
    public class ApcoPayController : ApiController
    {
        private static readonly string PaymentForm = "<html><head>" +
            "</head><body onload=\"document.form1.submit()\">" +
            "<form name=\"form1\" method=\"post\" action=\"{0}\" >" +
            "<input name=\"params\" type=\"hidden\" value=\"{1}\"></form></body></html>";

        [Route("paymentrequest"), HttpGet]
        public HttpResponseMessage PaymentRequest(string endpoint)
        {
            var queryString = HttpContext.Current.Request.QueryString;
            var xmlTextRead = new XmlTextReader(new StringReader(queryString["params"].ToString().Replace("\\", string.Empty)));
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlTextRead);
            xmlDoc.ChildNodes[0].Attributes["hash"].Value = CommonFunctions.ComputeMd5(xmlDoc.InnerXml);
            var paramsss = HttpUtility.HtmlEncode(xmlDoc.InnerXml);
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(string.Format(PaymentForm, endpoint, paramsss), Encoding.UTF8, "text/html");
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");

            return response;
        }
    }
}