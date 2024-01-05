using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using IqSoft.CP.DistributionWebApi.Models.PaymentIQ;

namespace IqSoft.CP.DistributionWebApi.Controllers
{
    [RoutePrefix("paymentiq")]
    public class PaymentIQController : ApiController
    {
        [HttpGet]
        [Route("paymentprocessing")]
        public HttpResponseMessage PaymentProcessing([FromUri] PaymentInput paymentInput)
        {
            var script = "<!DOCTYPE html><html lang=\"{0}\"><head><meta charset=\"UTF-8\">" +
                         "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">" +
                         "<meta http-equiv=\"X-UA-Compatible\" content=\"ie=edge\"><title>{1}</title> " +
                         "<script type=text/javascript src='https://static.paymentiq.io/cashier/cashier.js'></script> " +
                         "<style> html, body {{ margin: 0px; overflow: hidden; }} #cashier {{ height: 100vh; }}</style> " +
                         "</head><body><div id='cashier'></div><script> \r\n" +
                         "var CashierInstance = new _PaymentIQCashier('#cashier',{{merchantId:{2},userId:{3},sessionId:{4},environment:'{5}',method:'{6}',showAmount:'false'{7}}}, \r\n " +
                         "(api)=>{{api.on({{cashierInitLoad: () => console.log('Cashier init load')," +
                         "failure: data => window.location.href = '{10}'," +
                         "pending: data => window.location.href = '{10}'," +
                         "unresolved: data => window.location.href = '{10}'," +
                         "validationFailed: data => window.location.href = '{10}'," +
                         "cancelled: data => window.location.href = '{10}'" +
                         "}}) \r\n" +
                         "api.set({{config: {{amount: {8}}}, \r\n" +
                         "attributes: {{ merchantTxId : {9}}} }})\r\n" +
                         "api.css(`.your-custom-css {{color: blue;}}`)}})</script></body></html>";

            script = string.Format(script, paymentInput.Lang, paymentInput.PartnerName, paymentInput.MerchantId, paymentInput.UserId,
                                           paymentInput.SessionId, paymentInput.Environment, paymentInput.Method,
                                           !string.IsNullOrEmpty(paymentInput.ProviderType) ? $",providerType:'{paymentInput.ProviderType​}'" : string.Empty, 
                                           paymentInput.Amount, paymentInput.PaymentRequestId, paymentInput.Cashier);
            HttpResponseMessage response = Request.CreateResponse(System.Net.HttpStatusCode.OK);
            response.Content = new StringContent(script, System.Text.Encoding.UTF8, "text/html");
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }
    }
}