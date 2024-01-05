using log4net;
using System.Net.Http;

namespace IqSoft.CP.AutomationTest.Models
{
   public class HttpRequestInput
    {
        public ILog Log { get; set; }
        public string Url { get; set; }
        public string PostData { get; set; }
        public HttpMethod RequestMethod { get; set; }
        public string ContentType { get; set; }
        public string Accept { get; set; }
        public System.Collections.Generic.Dictionary<string, string> RequestHeaders { get; set; }
    }
}
