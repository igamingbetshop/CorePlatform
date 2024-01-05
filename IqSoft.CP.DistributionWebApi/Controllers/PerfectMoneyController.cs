using IqSoft.CP.DistributionWebApi.Models.PerfectMoney;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

namespace IqSoft.CP.DistributionWebApi.Controllers
{
    [RoutePrefix("perfectmoney")]
    public class PerfectMoneyController : ApiController
    {
        [Route("paymentrequest"), HttpGet]
        public HttpResponseMessage PaymentRequest([FromUri] PaymentRequestInput input)
        {
            var sign = input.InputData;
            input.InputData = string.Empty;
            var properties = from p in input.GetType().GetProperties()
                             select (p.GetValue(input, null) != null ? p.GetValue(input, null).ToString() : string.Empty);

            input.InputData = Common.ComputeMd5(Common.ComputeMd5(string.Join(":", properties.ToArray().
                                                         Where(x => !string.IsNullOrEmpty(x) && x[x.Length - 1] != '='))));
            if (sign != input.InputData)
                throw new Exception("Wrong input data");
            var script = "<form action=\"https://perfectmoney.is/api/step1.asp\" method=\"POST\">"
                       + "<input type=\"hidden\" name=\"PAYEE_ACCOUNT\" value=\"" + input.MerchantId + "\">"
                       + "<input type=\"hidden\" name=\"PAYEE_NAME\" value=\"" + input.MerchantName + "\">"
                       + "<input type=\"hidden\" name=\"PAYMENT_UNITS\" value=\"" + input.CurrencyId + "\">"
                       + "<input type=\"hidden\" name=\"STATUS_URL\" value=\"" + input.StatusUrl + "\">"
                       + "<input type=\"hidden\" name=\"PAYMENT_URL_METHOD\" value=\"LINK\">"
                       + "<input type=\"hidden\" name=\"NOPAYMENT_URL\" value=\"" + input.PaymentUrl + "\">"
                       + "<input type=\"hidden\" name=\"PAYMENT_URL\" value=\"" + input.PaymentUrl + "\">"
                       + "<input type=\"hidden\" name=\"NOPAYMENT_URL_METHOD\" value=\"LINK\">"
                       + "<input type=\"hidden\" name=\"BAGGAGE_FIELDS\" value=\"ORDER_NUM CUST_NUM\">"
                        + "<input type=\"hidden\" name=\"PAYMENT_ID\" value=\"" + input.PaymentRequestId + "\">"
                       + "<input type=\"hidden\" name=\"PAYMENT_AMOUNT\" value=\"" + input.Amount + "\">"
                       + "<input type=\"hidden\" name=\"FORCED_PAYMENT_METHOD\" value=\"" + input.PaymentMethod + "\">"
                       + "<input type=\"submit\" name=\"PAYMENT_METHOD\" value=\"PayPerfectMoney\" id='sendrequest'>"
                       + "</form><script>document.getElementById('sendrequest').click();</script>";

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(script, System.Text.Encoding.UTF8, "text/html");
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }
    }
}
