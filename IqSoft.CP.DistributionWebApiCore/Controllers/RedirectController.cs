using IqSoft.CP.Common.Helpers;
using IqSoft.CP.DistributionWebApi.Models;
using IqSoft.CP.DistributionWebApi.Models.PaymentProcessing;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Net.Http;

namespace IqSoft.CP.DistributionWebApi.Controllers
{
    [Route("redirect")]
    [ApiController]
    public class RedirectController : ControllerBase
    {
        [Route("redirectrequest"), HttpPost, HttpGet]
        public ActionResult RedirectRequest([FromQuery] string redirectUrl)
        {
            var script = "<!DOCTYPE html><html lang='en'><head>" +
                "<script>window.location.href=\"" + redirectUrl + "\"</script>" +
                "</head></html>";
            return Ok(script);
        }

        [Route("rp"), HttpGet]
        public ActionResult RedirectPaymentRequest(HttpRequestMessage httpRequestMessage)
        {
            var query = Uri.UnescapeDataString(Request.QueryString.ToString());
            var redirectData = query.Split('&')[0].Replace("rd=", string.Empty);
            RedirectDataModel redirectDataObject;
            try
            {
                redirectDataObject = JsonConvert.DeserializeObject<RedirectDataModel>(AESEncryptHelper.DecryptDistributionString(redirectData + "="));
            }
            catch
            {
                redirectDataObject = JsonConvert.DeserializeObject<RedirectDataModel>(AESEncryptHelper.DecryptDistributionString(redirectData));
            }
            var queryString = Request.QueryString;
            var httpRequestInput = new HttpRequestInput
            {
                RequestMethod = HttpMethod.Get,
                Url = string.Format("{0}?{1}", redirectDataObject.PaymentGatewayUrl, queryString.ToString())
            };
            Common.SendHttpRequest(httpRequestInput, out string contentType);
            var script = "<!DOCTYPE html><html lang='en'><head>" +
                "<script>window.location.href=\"" + redirectDataObject.CashierPageUrl + "\"</script>" +
                "</head></html>";
            return Ok(script);
        }

    }
}