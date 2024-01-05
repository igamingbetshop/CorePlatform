using Microsoft.AspNetCore.Mvc;

namespace IqSoft.CP.DistributionWebApi.Controllers
{
    [Route("paymentiq")]
    [ApiController]
    public class PaymentIQController : ControllerBase
    {
        [HttpGet]
        [Route("paymentprocessing")]
        public ActionResult PaymentProcessing([FromQuery] string merchantId, [FromQuery] string userId, [FromQuery] string sessionId, [FromQuery]string lang,
                                              [FromQuery]string environment, [FromQuery] string method, [FromQuery] string partnerName, [FromQuery] string cashier,
                                              [FromQuery] decimal amount, [FromQuery] long paymentRequestId, [FromQuery] string providerType)
        {
            var script = "<!DOCTYPE html><html lang=\"{0}\"><head><meta charset=\"UTF-8\">" +
                         "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">" +
                         "<meta http-equiv=\"X-UA-Compatible\" content=\"ie=edge\"><title>{1}</title> " +
                         "<script type=text/javascript src='https://static.paymentiq.io/cashier/cashier.js'></script> " +
                         "<style> html, body {{ margin: 0px; overflow: hidden; }} #cashier {{ height: 100vh; }}</style> " +
                         "</head><body><div id='cashier'></div><script> \r\n" +
                         "var CashierInstance = new _PaymentIQCashier('#cashier',{{merchantId:{2},userId:{3},sessionId:{4},environment:'{5}',method:'{6}',showAmount:'false',providerType:'{7}'}}, \r\n " +
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

            script = string.Format(script, lang, partnerName, merchantId, userId, sessionId, environment, method, providerType​, amount, paymentRequestId, cashier);
            return Ok(script);
        }
    }
}