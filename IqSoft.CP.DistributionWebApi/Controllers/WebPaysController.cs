using IqSoft.CP.Common.Helpers;
using IqSoft.CP.DistributionWebApi.Models.WebPays;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace IqSoft.CP.DistributionWebApi.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "GET")]
    [RoutePrefix("webpays")]
    public class WebPaysController : ApiController
    {
        private static readonly string PaymentForm = "<form method=\"post\" action=\"https://gw.paywb.co/checkout\" name=\"checkoutForm\">{0}" +
            "<input type=\"submit\" name=\"submit\" value=\"SUBMIT\" id=\"submitform\" />" +
            "<script>document.checkoutForm.submit();</script>" +
            "<script>document.getElementById('submitform').click();</script>" +
            "</form>";
        private static readonly string ItemTemplate = "<input type='hidden' name='{0}' value='{1}' /> ";

        [HttpGet]
        [Route("paymentprocessing")]
        public HttpResponseMessage PaymentProcessing([FromUri] string data)
        {
            var queryString = Uri.UnescapeDataString(HttpContext.Current.Request.QueryString.ToString());
            var inputString = AESEncryptHelper.DecryptDistributionString(queryString.Replace("data=", string.Empty));
            var formInput = JsonConvert.DeserializeObject<FormInput>(inputString);
            var queryParams = new StringBuilder();
            var properties = formInput.GetType().GetProperties();
            foreach (var field in properties)
                queryParams.AppendLine(string.Format(ItemTemplate, field.Name, field.GetValue(formInput, null)));

    
            HttpResponseMessage response = Request.CreateResponse(System.Net.HttpStatusCode.OK);
            response.Content = new StringContent(string.Format(PaymentForm, queryParams.ToString()), Encoding.UTF8, "text/html");
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }
    }
}