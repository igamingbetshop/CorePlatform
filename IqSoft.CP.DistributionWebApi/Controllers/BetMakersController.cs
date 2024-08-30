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
using IqSoft.CP.DistributionWebApi.Models.BetMakers;

namespace IqSoft.CP.DistributionWebApi.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "GET,POST")]
    [RoutePrefix("betmakers")]
    public class BetMakersController : ApiController
    {
        [HttpGet]
        [Route("launchgame")]
        public HttpResponseMessage LaunchGame([FromUri]string data)
        {
            var queryString = Uri.UnescapeDataString(HttpContext.Current.Request.QueryString.ToString());
            var inputString = AESEncryptHelper.DecryptDistributionString(queryString.Replace("data=", string.Empty));
            var input = JsonConvert.DeserializeObject<LaunchGameInput>(inputString);

            var script = "<!DOCTYPE html><html lang=\"en\"><head><script> " +
                         "var externalData = {{OrderId:'{0}', ResponseUrl:'{1}', RedirectUrl:'{2}', Amount:{3}, " +
                         "Currency:'{4}', Address:'{5}', HolderName:'{6}', Country:'{7}', City:'{8}', ZipCode:'{9}', " +
                         "Domain:'{10}', Language:'{11}', CancelUrl:'{12}', PartnerId:'{13}', PayAddress:'{14}', " +
                         "MinAmount:{15}, MaxAmount:{16}, ResourcesUrl:'{17}' }} </script></head><body>" +
                         "<script src=\"{17}/paymentcontents/{18}/init.js\"></script></body></html>";

            //script = string.Format(script, input.OrderId, input.ResponseUrl, input.RedirectUrl, input.Amount, input.Currency,
            //                               input.BillingAddress, input.HolderName, input.CountryCode, input.City, input.ZipCode,
            //                               input.PartnerDomain, input.LanguageId, input.CancelUrl, input.PartnerId, input.PayAddress,
            //                               input.MinAmount, input.MaxAmount, input.ResourcesUrl, input.PaymentSystemName);

            HttpResponseMessage response = Request.CreateResponse(System.Net.HttpStatusCode.OK);
            response.Content = new StringContent(script, Encoding.UTF8, "text/html");
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }
    }
}