﻿using IqSoft.CP.DistributionWebApi.Models;
using IqSoft.CP.DistributionWebApi.Models.CModule;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Web.Http;

namespace IqSoft.CP.DistributionWebApi.Controllers
{
	[RoutePrefix("cmodule")]
	public class CModuleController : ApiController
    {
		private static readonly string GameUrl = "https://partners.casinomobule.com/{0}.generate{1}StartConfig?{2}";

		[Route("launchgame"), HttpGet]
		public HttpResponseMessage LaunchGame(string homepage, string partneralias, string gamealias, string token, string currency, decimal balance, 
											  string languageId, bool isForDemo, bool isForMobile, string provider)
		{
			HttpResponseMessage response = Request.CreateResponse(System.Net.HttpStatusCode.OK);
			try
			{
				var parameters = string.Format("partner.alias={0}&game.alias={1}&partner.session={2}&currency={3}&balance={4}&lang={5}&mobile={6}", partneralias,
					gamealias, token, currency, balance, languageId, isForMobile ? "true" : "false");
				var url = string.Format(GameUrl, provider.ToLower(), isForDemo ? "Demo" : string.Empty, parameters);

				string resp = InitGame(url, homepage);

				var script = "<!DOCTYPE HTML><html><head>" +
					"<script src = 'https://ajax.googleapis.com/ajax/libs/jquery/3.2.1/jquery.min.js'></script>" +
					"<script src = 'https://shared-static.casinomobule.com/gameinclusion/library/gameinclusion.js'></script>" +
					"<style>html, body {width: 100%; height: 100%; overflow: hidden; margin: 0; padding: 0; }</style></head><body>" +
					"<iframe id = 'game' width = '100%' height = '100%' class='box' frameBorder='0'></iframe><script type = 'text/javascript'>";

				script += provider.ToLower() + ".launch(" + resp + ");</script></body></html>";
				response.Content = new StringContent(script, System.Text.Encoding.UTF8, "text/html");

			}
			catch (Exception ex)
			{
				response.Content = new StringContent(ex.Message, System.Text.Encoding.UTF8, "text/html");
			}
			return response;
		}

		private string InitGame(string url, string homepage)
		{
			string contentType;
			var resp = Common.SendHttpRequest(new HttpRequestInput
			{
				ContentType = HttpContentTypes.ApplicationJson,
				RequestMethod = HttpRequestMethods.Get,
				Url = url
			}, out contentType);
			var respObject = JsonConvert.DeserializeObject<InitGameOutput>(resp);
			if (respObject.status != "200")
				throw new Exception(respObject.message);
            var mobileParams = JObject.Parse(JsonConvert.SerializeObject(respObject.response));
            mobileParams["mobileParams"]["lobbyURL"] = "https://" + homepage;
            return JsonConvert.SerializeObject(mobileParams);
		}
	}
}