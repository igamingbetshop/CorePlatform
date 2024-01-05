using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;

namespace IqSoft.CP.DistributionWebApi.Controllers
{
	[RoutePrefix("tomhorn")]
	public class TomHornController : ApiController
	{
		[Route("launchgame"), HttpGet]
		public HttpResponseMessage LaunchGame(string module, string siteUrl, string languageId,long? sessionid, string productExternalId, bool isForMobile)
		{
			var b = System.Convert.FromBase64String(module);
			module = Encoding.UTF8.GetString(b);
			string script = "<!DOCTYPE html><html lang = \"en\">" +
							  "<head><meta charset = \"UTF-8\">" +
							  "<title>Title</title>" +
							  "<script src = \"https://ajax.googleapis.com/ajax/libs/jquery/3.2.1/jquery.min.js\" ></script>" +
							  "<script>var urlParams = {externalId:\"" + productExternalId + "\",token:\"" + sessionid + 
							                          "\",languageid:\"" + languageId + "\",viewtype:\"" + (isForMobile ? 2 : 1) +
							  "\",data:" + module + "}</script>" +
							  "<style>html,body{margin:0;padding:0;width:100%;height:100%}body{background-color: #0E0E0E}" +
							  "iframe{margin: 0 auto;display: block; width:100%; height:100%; border: none}</style></head>" +
							  "<body><div id = \"gameClientPlaceholder\"><h2>Game Client starting procedure failed!</ h2 ></div>" +
							  "<script type = \"text/javascript\" src = \"https://resources." + siteUrl + "/productcontents/tomhorn/main.js\"></script>" +
							  "<script src = \"https://resources." + siteUrl + "/productcontents/tomhorn/swfobject.js\" type = \"text/javascript\"></script></body></html>";

			var response = Request.CreateResponse(System.Net.HttpStatusCode.OK);
			response.Content = new StringContent(script, Encoding.UTF8, "text/html");
			response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
			return response;
		}
	}
}