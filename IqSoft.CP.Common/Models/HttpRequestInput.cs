using log4net;

namespace IqSoft.CP.Common.Models
{
    public class HttpRequestInput
    {
        public ILog Log { get; set; }
        public System.DateTime Date { get; set; }
        public string Url { get; set; }
        public string PostData { get; set; }
        public string RequestMethod { get; set; }
        public string ContentType { get; set; }
        public string Accept { get; set; }
        public System.Collections.Generic.Dictionary<string, string> RequestHeaders { get; set; }
    }
}