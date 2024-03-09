using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Cors;
using System.Web.Http;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using IqSoft.CP.Common.Helpers;

namespace IqSoft.CP.DistributionWebApi.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "GET,POST")]
    [RoutePrefix("changelly")]
    public class ChangellyController : ApiController
    {
        [HttpGet]
        [Route("paymentprocessing")]
        public HttpResponseMessage PaymentProcessing([FromUri] string data)
        {
            var queryString = Uri.UnescapeDataString(HttpContext.Current.Request.QueryString.ToString());
            var inputString = AESEncryptHelper.DecryptDistributionString(queryString.Replace("data=", string.Empty));

            var script = $"<iframe width=\"100%\" height=\"100%\" frameborder='none' allow=\"camera\"" +
                         $" src=\"https://widget.changelly.com?{inputString}\">Can't load widget</iframe>";

            HttpResponseMessage response = Request.CreateResponse(System.Net.HttpStatusCode.OK);
            response.Content = new StringContent(script, Encoding.UTF8, "text/html");
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }
    }
}