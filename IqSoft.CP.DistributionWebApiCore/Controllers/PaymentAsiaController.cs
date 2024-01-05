using IqSoft.CP.Common.Helpers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text;
using System.Web;

namespace IqSoft.CP.DistributionWebApi.Controllers
{
    [Route("paymentasia")]
    [ApiController]
    public class PaymentAsiaController : ControllerBase
    {
        private static readonly string PaymentForm = "<body onload=\"document.forms[0].submit()\"><form action=\"{0}\" method=\"post\" target=\"\">{1}</form></body>";
        private static readonly string ItemTemplate = "<input type=\"hidden\" name=\"{0}\" value=\"{1}\" /> ";

        [Route("paymentrequest"), HttpGet]
        public ActionResult PaymentRequest([FromQuery] string p)
        {
            var input = Uri.UnescapeDataString(Request.QueryString.ToString());
            input = AESEncryptHelper.DecryptDistributionString(input.Substring(2, input.Length - 2));
            var requestBody = input.Split(new string[] { "apiUrl=" }, StringSplitOptions.None);
            var queryString = HttpUtility.ParseQueryString(requestBody[0]);
            var queryParams = new StringBuilder();
            foreach (var item in queryString.AllKeys)
            {
                if (!string.IsNullOrEmpty(item))
                    queryParams.AppendLine(string.Format(ItemTemplate, item, queryString[item]));
            }
            return Ok(string.Format(PaymentForm, AESEncryptHelper.DecryptDistributionString(requestBody[1]), queryParams.ToString()));
        }
    }
}