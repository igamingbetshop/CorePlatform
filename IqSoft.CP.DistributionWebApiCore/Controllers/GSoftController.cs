using System;
using System.Net.Http;
using IqSoft.CP.DistributionWebApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace IqSoft.CP.DistributionWebApi.Controllers
{
	[Route("gsoft")]
    [ApiController]
    public class GSoftController : ControllerBase
    {
        [Route("CallGSoftApi"), HttpPost]
        public string CallGSoftApi(RequestInput request)
        {
            try
            {
                var uri = new Uri(request.Content);
                var url = string.Format("{0}://{1}{2}", uri.Scheme, uri.Authority, uri.AbsolutePath);

                var input = new HttpRequestInput
                {
                    ContentType = HttpContentTypes.ApplicationUrlEncoded,
                    RequestMethod = HttpMethod.Post,
                    Url = url,
                    PostData= uri.Query.Remove(0, 1)
                };
               return Common.SendHttpRequest(input, out _);
            }
            catch (Exception exc)
            {
                return "Catch message:" + exc.Message + "\n request.Content:" + request.Content;
            }
        }
    }
}
