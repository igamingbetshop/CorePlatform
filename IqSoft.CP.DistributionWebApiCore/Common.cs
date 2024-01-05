using IqSoft.CP.DistributionWebApi.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace IqSoft.CP.DistributionWebApi
{
	public static class Common
	{
		public static string SendHttpRequest(HttpRequestInput input, out string contentType, SecurityProtocolType type = SecurityProtocolType.Tls12)
		{
			contentType = string.Empty;
			var response = SendHttpRequestForStream(input, type);
			if (response == null)
				return string.Empty;
			contentType = response.Content.Headers.ContentType.ToString();
			return response.Content.ReadAsStringAsync().Result;
		}

		public static HttpResponseMessage SendHttpRequestForStream(HttpRequestInput input, SecurityProtocolType type)
		{
			using var request = new HttpRequestMessage(input.RequestMethod, input.Url);
			request.Content = new StringContent(input.PostData, Encoding.UTF8, input.ContentType);
			using var httpClient = new HttpClient() { Timeout = TimeSpan.FromTicks(60000) };
			request.Headers.Accept.TryParseAdd(input.Accept);
			if (input.RequestHeaders != null)
			{
				foreach (var headerValuePair in input.RequestHeaders)
				{
					httpClient.DefaultRequestHeaders.Add(headerValuePair.Key, headerValuePair.Value);
				}
			}
			return httpClient.Send(request);			
		}

		public static string ComputeMd5(string data)
		{
			using (MD5 md5Hash = MD5.Create())
			{
				var bytes = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(data));
				var sBuilder = new StringBuilder();
				foreach (var t in bytes)
				{
					sBuilder.Append(t.ToString("x2"));
				}
				return sBuilder.ToString();
			}
		}
	}
}