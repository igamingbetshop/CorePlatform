using IqSoft.CP.DistributionWebApi.Models.PaymentProcessing;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Specialized;
using IqSoft.CP.Common.Helpers;
using System.Text;
using System.Web;
using System;

namespace IqSoft.CP.DistributionWebApi.Controllers
{
    [Route("paymentform")]
    [ApiController]
    public class PaymentFormController : ControllerBase
    {
        [HttpGet]
        [Route("paymentprocessing")]
        public ActionResult PaymentProcessing([FromQuery]string data)
        {
            var queryString = Uri.UnescapeDataString(Request.QueryString.ToString());
            var inputString = AESEncryptHelper.DecryptDistributionString(queryString.Replace("data=", string.Empty));
            var input = JsonConvert.DeserializeObject<PaymentProcessingInput>(inputString);

            var script = "<!DOCTYPE html><html lang=\"en\"><head><script> " +
                         "var externalData = {{OrderId:'{0}', ResponseUrl:'{1}', RedirectUrl:'{2}', Amount:{3}, " +
                         "Currency:'{4}', Address:'{5}', HolderName:'{6}', Country:'{7}', City:'{8}', ZipCode:'{9}', " +
                         "Domain:'{10}', Language:'{11}', CancelUrl:'{12}', PartnerId:'{13}', PayAddress:'{14}', " +
                         "MinAmount:{15}, MaxAmount:{16}, ResourcesUrl:'{17}' }} </script></head><body>" +
                         "<script src=\"{17}/paymentcontents/{18}/init.js\"></script></body></html>";

            script = string.Format(script, input.OrderId, input.ResponseUrl, input.RedirectUrl, input.Amount, input.Currency,
                                           input.BillingAddress, input.HolderName, input.CountryCode, input.City, input.ZipCode,
                                           input.PartnerDomain, input.LanguageId, input.CancelUrl, input.PartnerId, input.PayAddress,
                                           input.MinAmount, input.MaxAmount, input.ResourcesUrl, input.PaymentSystemName);

            return Ok(script);
        }

        [Route("paymentrequest"), HttpGet]
        public ActionResult PaymentRequest(string apiUrl)
        {
            var paramTemplate = "<input name=\"{0}\" value=\"{1}\">";
            var script = "<form style=\"display:none\" action='{0}' method='post'> " +
                "<input type='submit' value='Submit' id='sendtoken'> " +
                "{1} </form> <script>document.getElementById('sendtoken').click();</script>";
            var queryString = new NameValueCollection(HttpUtility.ParseQueryString(Request.QueryString.ToString()));
            var queryParams = new StringBuilder();
            foreach (var item in queryString.AllKeys)
            {
                if (item != "apiUrl")
                    queryParams.AppendLine(string.Format(paramTemplate, item, queryString[item]));
            }
            script = string.Format(script, apiUrl, queryParams.ToString());
            return Ok(script);
        }
    }
}