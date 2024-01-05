using IqSoft.CP.DistributionWebApi.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace IqSoft.CP.DistributionWebApi.Controllers
{
	[RoutePrefix("softgaming")]
	public class SoftGamingController : ApiController
	{
		[Route("launchgame"), HttpGet]
		public HttpResponseMessage LaunchGame([FromUri]string requestUrl, [FromUri]string inputData, [FromUri]string hash)
		{
			var response = Request.CreateResponse(HttpStatusCode.OK);
			inputData = inputData.Replace("\\", "");
			var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(inputData);
			var hostName = Dns.GetHostName();
			var serverIPs = Dns.GetHostEntry(hostName.Replace("https://", string.Empty).Replace("http://", string.Empty)).AddressList;
			var ip = serverIPs[serverIPs.Length - 1].ToString();
			data["UserIP"] = ip;
			var encodedInput = string.Join("&", data.Select(kvp =>
							   string.Format("{0}={1}", kvp.Key, HttpUtility.UrlEncode(kvp.Value))));		
			var res = string.Empty;
			try
			{
				var httpRequestInput = new HttpRequestInput
				{
					RequestMethod = "GET",
					Url = string.Format("{0}?{1}&Hash={2}", requestUrl, encodedInput, hash)
				};
				res = Common.SendHttpRequest(httpRequestInput, out string contentType);
				var ind = res.IndexOf(',');
				if (ind >= 0)
					response.Content = new StringContent(ind != res.Length - 1 ? res.Substring(ind + 1, res.Length - ind - 1) : res.Substring(0, res.Length - 1), System.Text.Encoding.UTF8, "text/html");
				else
					response.Content = new StringContent(res, System.Text.Encoding.UTF8, "text/html");
			}
			catch (Exception ex)
			{
				response.Content = new StringContent(ex.Message + " " + res, System.Text.Encoding.UTF8, "text/html");
			}
			return response;
		}

		[Route("redirectgame"), HttpGet]
		public HttpResponseMessage RedirectGame([FromUri] string requestUrl, [FromUri] string inputData, [FromUri] string hash)
		{
			var response = Request.CreateResponse(HttpStatusCode.OK);

			inputData = inputData.Replace("\\", "");
			var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(inputData);
			data["UserIP"] = "103.187.243.255";

			var encodedInput = string.Join("&", data.Select(kvp =>
							   string.Format("{0}={1}", kvp.Key, HttpUtility.UrlEncode(kvp.Value))));
			var content = string.Empty;
			try
			{
				var httpRequestInput = new HttpRequestInput
				{
					RequestMethod = "GET",
					Url = string.Format("{0}?{1}&Hash={2}", requestUrl, encodedInput, hash)
				};

				content = Common.SendHttpRequest(httpRequestInput, out string contentType);
				var res = content.Split(',');

				if (res.Count() == 2)
					response.Content = new StringContent(res[1], System.Text.Encoding.UTF8, "text/html");
				else
					response.Content = new StringContent(content, System.Text.Encoding.UTF8, "text/html");
			}
			catch (Exception ex)
			{
				response.Content = new StringContent(ex.Message + " " + content, System.Text.Encoding.UTF8, "text/html");
			}
			return response;
		}

	}
}