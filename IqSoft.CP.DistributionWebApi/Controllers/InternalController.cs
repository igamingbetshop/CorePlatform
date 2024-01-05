using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;

namespace IqSoft.CP.DistributionWebApi.Controllers
{
	[RoutePrefix("internal")]
	public class InternalController : ApiController
	{
		[Route("launchsport"), HttpGet]
		public HttpResponseMessage StartScript()
		{
			var script = "<html lang='en'><head><meta charset = 'utf-8'><title> SportsbookProjects </title><base href = '/website/'>" +
						 "<meta name = 'apple-mobile-web-app-capable' content = 'yes'>" +
						 "<meta name = 'viewport' content = 'user-scalable=no,width=device-width,initial-scale=1.0,maximum-scale=1.0,minimum-scale=1.0'>" +
						 "<link rel = 'icon' type = 'image/x-icon' href = 'favicon.ico'>" +
						 "<link rel = 'preload' href = 'assets/fonts/font-awesome-4.4.0/css/font-awesome.min.css' as= 'style'>" +
						 "<link rel = 'stylesheet' href = 'assets/fonts/font-awesome-4.4.0/css/font-awesome.min.css'>'" +
						 "<link rel = 'preload' href = 'assets/flags.css' as= 'style'>" +
						 "<link rel = 'stylesheet' href = 'assets/flags.css'>" + 
						 "<link rel = 'stylesheet' href = 'styles.36292f612bd55b46bfac.css' ></head><body><app-root></app-root>" +
						 "<script type = 'text/javascript' src = 'assets/js/jquery-2.2.0.min.js' ></script>" +
						 "<script defer type = 'text/javascript' src = 'assets/js/fraction.js' ></script >" +
						 "<script defer type = 'text/javascript' src = 'https://live.statscore.com/livescorepro/generator?auto_init=false' ></script>" +
						 "<script>function bannerDispatchData(data){const event = new CustomEvent('bannerClick', { detail: data});window.dispatchEvent(event);}</script>" +
						 "<script src = 'runtime-es2015.b2cb1085ba082e203103.js' type='module'></script><script src = 'polyfills-es2015.99414c6182bcc1f3317e.js' type='module'></script>" +
						 "<script src = 'runtime-es5.0f970e4eb6b5c83f2fb4.js' nomodule></script><script src = 'polyfills-es5.fbe87bdaac31766b59fd.js' nomodule></script>" +
						 "<script src = 'scripts.9bfcda1f1fcb0fb81995.js'></script><script src='main-es2015.16c58069c7f273e24db4.js' type='module'></script>" +
						 "<script src = 'main-es5.2df526847010e76e3752.js' nomodule></script></body></html>";

			HttpResponseMessage response = Request.CreateResponse(System.Net.HttpStatusCode.OK);
			response.Content = new StringContent(script, System.Text.Encoding.UTF8, "text/html");
			response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
			return response;
		}
	}
}