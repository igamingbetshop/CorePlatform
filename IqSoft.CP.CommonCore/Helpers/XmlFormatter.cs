using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace IqSoft.CP.Common.Helpers
{
    public class XmlFormatter : MediaTypeFormatter
    {
        private readonly Encoding _encoder;

        public XmlFormatter(string encoding)
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationXml));
            SupportedMediaTypes.Add(new MediaTypeHeaderValue(Constants.HttpContentTypes.TextXml));
            SupportedMediaTypes.Add(new MediaTypeHeaderValue(Constants.HttpContentTypes.ApplicationUrlEncoded));
            _encoder = Encoding.GetEncoding(encoding);
        }

        public override bool CanReadType(Type type)
        {
            return true;
        }

        public override bool CanWriteType(Type type)
        {
            return true;
        }

        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content,
            IFormatterLogger formatterLogger)
        {
            return Task.Factory.StartNew(() =>
            {
                using (var streamReader = new StreamReader(readStream, _encoder))
                {
                    var serializer = new XmlSerializer(type);
                    return serializer.Deserialize(streamReader);
                }
            });
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content,
            TransportContext transportContext)
        {
            var serializer = new XmlSerializer(type);
            return Task.Factory.StartNew(() =>
            {
                using (var streamWriter = new StreamWriter(writeStream, _encoder))
                {
                    var ns = new XmlSerializerNamespaces();
                    ns.Add("", "");
                    serializer.Serialize(streamWriter, value, ns);
                }
            });
        }
    }
}