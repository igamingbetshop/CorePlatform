using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace IqSoft.CP.Common.Helpers
{
    public static class SerializeAndDeserialize
    {
        public static string SerializeToXml<T>(T input, string root)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T), new XmlRootAttribute(root));
            string xml = string.Empty;
            StreamReader stream = null;
            try
            {
                using (var ms = new MemoryStream())
                {
                    using (var xmlWriter = XmlWriter.Create(ms))
                    {
                        serializer.Serialize(xmlWriter, input);
                        xmlWriter.Flush();
                        ms.Seek(0, SeekOrigin.Begin);
                        stream = new StreamReader(ms, Encoding.UTF8);
                        xml = stream.ReadToEnd();
                    }
                }
            }
            finally
            {
                if (stream != null) stream.Dispose();
            }
            return xml;
        }

        public static string SerializeToXmlWithoutRoot<T>(T input)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            string xml = string.Empty;
            StreamReader stream = null;
            try
            {
                using (var ms = new MemoryStream())
                {
                    using (var xmlWriter = XmlWriter.Create(ms,
                                                                new XmlWriterSettings
                                                                {
                                                                    Indent = true,
                                                                    ConformanceLevel = ConformanceLevel.Auto,
                                                                    OmitXmlDeclaration = true
                                                                }
                                                           )
                          )
                    {
                        XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                        ns.Add("", "");
                        serializer.Serialize(xmlWriter, input, ns);
                        xmlWriter.Flush();
                        ms.Seek(0, SeekOrigin.Begin);
                        stream = new StreamReader(ms, Encoding.UTF8);
                        xml = stream.ReadToEnd();
                    }
                }
            }
            finally
            {
                if (stream != null) stream.Dispose();
            }
            return xml;
        }

        public static T XmlDeserializeFromString<T>(string objectData)
        {
            var serializer = new XmlSerializer(typeof(T));
            object result;

            using (TextReader reader = new StringReader(objectData))
            {
                result = serializer.Deserialize(reader);
            }

            return (T)result;
        }
    }
}
