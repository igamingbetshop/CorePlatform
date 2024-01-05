using Microsoft.AspNetCore.Mvc;

namespace IqSoft.CP.DistributionWebApi.Controllers
{
	[Route("inbet")]
	[ApiController]
	public class InBetController : ControllerBase
	{
		[Route("launchgame"), HttpGet]
		public ActionResult StartScript(string game, string billing, string token, int kf, string currency, string language, string homepage)
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
			return Ok(script);
		}
	}
}
