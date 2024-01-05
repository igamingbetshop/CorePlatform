using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

namespace IqSoft.CP.DistributionWebApi.Controllers
{
	[RoutePrefix("inbet")]
	public class InBetController : ApiController
	{
		[Route("launchgame"), HttpGet]
		public HttpResponseMessage StartScript(string game, string billing, string token, int kf, string currency, string language, string homepage)
		{
			var script = "<html>" +
						 "<meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge,chrome=1\"/>" +
						 "<meta name=\"viewport\" content=\"width=device-width,initial-scale=1.0,maximum-scale=1.0,minimum-scale=1.0,user-scalable=no,minimal-ui\"/>" +
						 "<meta name=\"apple-mobile-web-app-capable\" content=\"yes\"/>" +
						 "<meta name=\"mobile-web-app-capable\" content=\"yes\"/>" +
						 "<meta name=\"apple-mobile-web-app-status-bar-style\" content=\"black\"/>" +
						 "<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"/>" +
						 "<script type=\"text/javascript\" src=\"https://inbet." + homepage + "/inbet/inbetmedia/gamelist_data.js\"></script>" +
						 "<script src=\"https://inbet." + homepage + "/inbet/inbetmedia/loader/build/app.js\"></script>" +
						 "<script type=\"text/javascript\">" +
						 "window.init_loader({" +
						 		"path: \"/inbet/inbetmedia/\"," +
						 		"game: \"" + game + "\"," +
						 		"billing: \"" + billing + "\"," +
						 		"token: \"" + token + "\"," +
						 		"kf: " + kf + "," +
						 		"currency: \"" + currency + "\"," +
						 		"language: \"" + language + "\"," +
								"home_page: \"https://" + homepage + "\"," +
								"button: \"classic\"});" +
								"</script></html><body><div id=\"game-content\"></div></body>";
			HttpResponseMessage response = Request.CreateResponse(System.Net.HttpStatusCode.OK);
			response.Content = new StringContent(script, System.Text.Encoding.UTF8, "text/html");
			response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
			return response;
		}
	}
}
