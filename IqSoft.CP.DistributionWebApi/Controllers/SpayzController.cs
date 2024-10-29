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
using IqSoft.CP.DistributionWebApi.Models.Spayz;

namespace IqSoft.CP.DistributionWebApi.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "GET")]
    [RoutePrefix("spayz")]
    public class SpayzController : ApiController
    {
        [HttpGet]
        [Route("paymentprocessing")]
        public HttpResponseMessage PaymentProcessing([FromUri] string data)
        {
            var queryString = Uri.UnescapeDataString(HttpContext.Current.Request.QueryString.ToString());
            var inputString = AESEncryptHelper.DecryptDistributionString(queryString.Replace("data=", string.Empty));
            var input = JsonConvert.DeserializeObject<PaymentInput>(inputString);

            var script = "<!DOCTYPE html><html lang=\"en\"><head><script> " +
                         $"var jsonData ={input.JsonString}</script></head><body>" +
                         "<script src=\"https://code.jquery.com/jquery-3.4.1.js\" " +
                         "integrity=\"sha256-WpOohJOqMqqyKL9FccASB9O0KwACQJpFTUBLTYOVvVU=\" crossorigin=\"anonymous\"></script>" +
                         $"<script src=\"{input.ResourcesUrl}/paymentcontents/spayz/init.js\"></script></body></html>";

            HttpResponseMessage response = Request.CreateResponse(System.Net.HttpStatusCode.OK);
            response.Content = new StringContent(script, Encoding.UTF8, "text/html");
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }
    }
}