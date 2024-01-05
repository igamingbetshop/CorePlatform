using System;
using System.Web.Http;
using System.Web.Http.Cors;
using IqSoft.CP.DistributionWebApi.Models;

namespace IqSoft.CP.DistributionWebApi.Controllers
{
	[RoutePrefix("gsoft")]
	public class GSoftController : ApiController
    {
        [HttpPost]
        public string CallGSoftApi(RequestInput request)
        {
            try
            {
                var uri = new Uri(request.Content);
                var url = string.Format("{0}://{1}{2}", uri.Scheme, uri.Authority, uri.AbsolutePath);

                var input = new HttpRequestInput
                {
                    ContentType = HttpContentTypes.ApplicationUrlEncoded,
                    RequestMethod = HttpRequestMethods.Post,
                    Url = url,
                    PostData= uri.Query.Remove(0, 1)
                };
			   string contentType;
               return Common.SendHttpRequest(input, out contentType);
            }
            catch (Exception exc)
            {
                return "Catch message:" + exc.Message + "\n request.Content:" + request.Content;
            }
        }
    }
}
