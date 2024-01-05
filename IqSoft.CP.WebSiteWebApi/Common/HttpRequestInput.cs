using System.Collections.Generic;
using System.Net.Http;

namespace IqSoft.CP.WebSiteWebApi.Common
{
	public class HttpRequestInput
	{
		public string Url { get; set; }
		public string PostData { get; set; }
		public HttpMethod RequestMethod { get; set; }
		public string ContentType { get; set; }
		public string Accept { get; set; }
		public Dictionary<string, string> RequestHeaders { get; set; }
	}
}