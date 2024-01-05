using IqSoft.CP.DistributionWebApi.Models;
using System.Net;
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
			contentType = response.ContentType;
		
			var dataStream = response.GetResponseStream();
			
			using (var reader = new System.IO.StreamReader(dataStream))
			{
				var responseFromServer = reader.ReadToEnd();
				reader.Close();
				dataStream.Close();
				return responseFromServer;
			}
		}

		public static HttpWebResponse SendHttpRequestForStream(HttpRequestInput input, SecurityProtocolType type)
		{
			ServicePointManager.Expect100Continue = true;
			ServicePointManager.SecurityProtocol = type;
			var request = (HttpWebRequest)WebRequest.Create(input.Url);
			request.ContentLength = 0;
			request.Method = input.RequestMethod;
			request.Timeout = 60000;   //Infinite
			request.KeepAlive = true;
			if (input.RequestHeaders != null)
			{
				foreach (var headerValuePair in input.RequestHeaders)
				{
					request.Headers[headerValuePair.Key] = headerValuePair.Value;
				}
			}
			if (!string.IsNullOrWhiteSpace(input.PostData))
			{
				var data = Encoding.UTF8.GetBytes(input.PostData);
				request.ContentType = input.ContentType;
				request.ContentLength = data.Length;
				request.Accept = input.Accept;
				using (var stream = request.GetRequestStream())
				{
					stream.Write(data, 0, data.Length);
					stream.Close();
				}
			}
			var response = (HttpWebResponse)request.GetResponse();
			return response;
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