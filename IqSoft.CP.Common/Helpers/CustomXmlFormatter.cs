using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace IqSoft.CP.Common.Helpers
{
    public class CustomXmlFormatter : XmlMediaTypeFormatter
    {
        private readonly XmlFormatterTypes _formatterType;

        public CustomXmlFormatter(XmlFormatterTypes formatterType)
        {
            _formatterType = formatterType;
        }

        public enum XmlFormatterTypes
        {
            Standard = 1,
            WithoutNamesPacesAndOmitDeclaration = 2, 
            Utf8Format = 3
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content,
            TransportContext transportContext)
        {
            try
            {
                var task = Task.Factory.StartNew(() =>
                {
                    switch (_formatterType)
                    {
                        case XmlFormatterTypes.Standard:
                            SerializeStandard(type, value, writeStream);
                            break;
                        case XmlFormatterTypes.WithoutNamesPacesAndOmitDeclaration:
                            SerializeWithoutNamesPacesAndOmitDeclaration(type, value, writeStream);
                            break;
                        case XmlFormatterTypes.Utf8Format:
                            SerializeInUtf8Format(type, value, writeStream);
                            break;
                        default:
                            base.WriteToStreamAsync(type, value, writeStream, content, transportContext);
                            break;
                    }
                });

                return task;
            }
            catch (Exception)
            {
                return base.WriteToStreamAsync(type, value, writeStream, content, transportContext);
            }
        }

        private void SerializeStandard(Type type, object value, Stream writeStream)
        {
            var serializer = new XmlSerializer(type);
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add(string.Empty, string.Empty);
            using (var writer = XmlWriter.Create(writeStream))
            {
                serializer.Serialize(writer, value, ns);
            }
        }

        private void SerializeWithoutNamesPacesAndOmitDeclaration(Type type, object value, Stream writeStream)
        {
            var settings = new XmlWriterSettings { OmitXmlDeclaration = true };
            var serializer = new XmlSerializer(type);
            var serializerNamespaces = new XmlSerializerNamespaces();
            serializerNamespaces.Add(string.Empty, string.Empty);
            using (var writer = XmlWriter.Create(writeStream, settings))
            {
                serializer.Serialize(writer, value, serializerNamespaces);
            }
        }

        private void SerializeInUtf8Format(Type type, object value, Stream writeStream)
        {
            var settings = new XmlWriterSettings();
            settings.Encoding = new UTF8Encoding(false);
            var serializer = new XmlSerializer(type);
            var serializerNamespaces = new XmlSerializerNamespaces();
            serializerNamespaces.Add(string.Empty, string.Empty);
            using (var writer = XmlWriter.Create(writeStream, settings))
            {
                serializer.Serialize(writer, value, serializerNamespaces);
            }
        }
        
        public static string GetXml(Type type, object value)
        {
            var settings = new XmlWriterSettings { OmitXmlDeclaration = true };
            var serializer = new XmlSerializer(type);
            var serializerNamespaces = new XmlSerializerNamespaces();
            serializerNamespaces.Add(string.Empty, string.Empty);
            using (var writeStream = new StringWriter())
            {
                using (var writer = XmlWriter.Create(writeStream, settings))
                {
                    serializer.Serialize(writer, value, serializerNamespaces);
                }
                return writeStream.ToString();
            }
        }
    }
}