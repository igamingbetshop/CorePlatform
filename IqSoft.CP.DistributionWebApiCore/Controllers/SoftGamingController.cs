using IqSoft.CP.DistributionWebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;

namespace IqSoft.CP.DistributionWebApi.Controllers
{
    [Route("softgaming")]
	[ApiController]
	public class SoftGamingController : ControllerBase
	{
		[Route("launchgame"), HttpGet]
		public ActionResult LaunchGame([FromQuery]string requestUrl, [FromQuery]string inputData, [FromQuery]string hash)
		{
			Response.ContentType = "text/html";
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
					RequestMethod = HttpMethod.Get,
					Url = string.Format("{0}?{1}&Hash={2}", requestUrl, encodedInput, hash)
				};
				res = Common.SendHttpRequest(httpRequestInput, out string contentType);
				var ind = res.IndexOf(',');
				if (ind >= 0)
					return Ok(ind != res.Length - 1 ? res.Substring(ind + 1, res.Length - ind - 1) : res.Substring(0, res.Length - 1));
				return Ok(res);
			}
			catch (Exception ex)
			{
				return Ok(ex.Message + " " + res);
			}
		}

		[Route("redirectgame"), HttpGet]
		public ActionResult RedirectGame([FromQuery] string requestUrl, [FromQuery] string inputData, [FromQuery] string hash)
		{
			Response.ContentType = "text/html";
			inputData = inputData.Replace("\\", "");
			var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(inputData);
			var hostName = Dns.GetHostName();
			var serverIPs = Dns.GetHostEntry(hostName.Replace("https://", string.Empty).Replace("http://", string.Empty)).AddressList;
			var ip = serverIPs[serverIPs.Length - 1].ToString();
			data["UserIP"] = ip;
			var encodedInput = string.Join("&", data.Select(kvp =>
							   string.Format("{0}={1}", kvp.Key, HttpUtility.UrlEncode(kvp.Value))));
			var content = string.Empty;
			try
			{
				var httpRequestInput = new HttpRequestInput
				{
					RequestMethod = HttpMethod.Get,
					Url = string.Format("{0}?{1}&Hash={2}", requestUrl, encodedInput, hash)
				};
				content = Common.SendHttpRequest(httpRequestInput, out string contentType);
				var res = content.Split(',');

				if (res.Count() == 2)
					return Ok(res[1]);

				return Ok(res[0]);
			}
			catch (Exception ex)
			{
				return Ok(ex.Message + " " + content);
			}
		}
	}
}