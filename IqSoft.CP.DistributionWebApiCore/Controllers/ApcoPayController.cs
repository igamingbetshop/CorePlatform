using IqSoft.CP.Common.Helpers;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Specialized;
using System.IO;
using System.Web;
using System.Xml;

namespace IqSoft.CP.DistributionWebApi.Controllers
{
    [Route("apcopay")]
    [ApiController]
    public class ApcoPayController : ControllerBase
    {
        private static readonly string PaymentForm = "<html><head>" +
            "</head><body onload=\"document.form1.submit()\">" +
            "<form name=\"form1\" method=\"post\" action=\"{0}\" >" +
            "<input name=\"params\" type=\"hidden\" value=\"{1}\"></form></body></html>";

        [Route("paymentrequest"), HttpGet]
        public ActionResult PaymentRequest([FromQuery]string endpoint)
        {
            var queryString = new NameValueCollection(HttpUtility.ParseQueryString(Request.QueryString.ToString()));
            var xmlTextRead = new XmlTextReader(new StringReader(queryString["params"].ToString().Replace("\\", string.Empty)));
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlTextRead);
            xmlDoc.ChildNodes[0].Attributes["hash"].Value = CommonFunctions.ComputeMd5(xmlDoc.InnerXml);
            var paramsss = HttpUtility.HtmlEncode(xmlDoc.InnerXml);
            return Ok(string.Format(PaymentForm, endpoint, paramsss));
        }
    }
}