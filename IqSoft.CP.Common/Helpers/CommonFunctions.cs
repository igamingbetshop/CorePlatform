using IqSoft.CP.Common.Models;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Transactions;
using System.Xml;
using System.Xml.Serialization;

namespace IqSoft.CP.Common.Helpers
{
    public static class CommonFunctions
    {
        public static string GetRandomString(int size)
        {
            const string chars = "ABCDEFGHJKMNPQRSTUVWXYZ23456789abcdefghijkmnpqrstuvwxyz";
            var random = new Random(Guid.NewGuid().GetHashCode());
            return new string(
                Enumerable.Repeat(chars, size)
                          .Select(s => s[random.Next(s.Length)])
                          .ToArray());
        }

        public static string GetRandomNumber(int size)
        {
            const string chars = "0123456789";
            var random = new Random(Guid.NewGuid().GetHashCode());
            return new string(Enumerable.Repeat(chars, size)
                                        .Select(s => s[random.Next(s.Length)])
                                        .ToArray());
        }

        public static List<int> GetRandomNumbers(int minValue, int maxValue, int count)
        {
            var numbers = new List<int>();
            for (int i = 0; i <= maxValue - minValue; i++)
            {
                numbers.Add(minValue + i);
            }
            var result = new List<int>();
            var random = new Random(Guid.NewGuid().GetHashCode());
            for (int i = 0; i < count; i++)
            {
                var index = random.Next(maxValue - minValue + 1 - i);
                result.Add(numbers[index]);
                numbers.RemoveAt(index);
            }
            return result;
        }

        public static string ComputeSha512(string path)
        {
            using (var hash = new SHA512Managed())
            {
                var messageBytes = Encoding.UTF8.GetBytes(path);
                var hashValue = hash.ComputeHash(messageBytes);
                return hashValue.Aggregate(String.Empty, (current, b) => current + String.Format("{0:x2}", b));
            }
        }

        public static string ComputeUserPasswordHash(string password, int salt)
        {
            return ComputeSha512(string.Format("{0}{1}{2}", password, salt, password));
        }

        public static string ComputeClientPasswordHash(string password, int salt)
        {
            return ComputeSha512(string.Format("{0}{1}{2}", salt, password, salt));
        }

        public static string ComputeSha1(string rawData)
        {
            using (SHA1 sha1Hash = SHA1.Create())
            {
                byte[] bytes = sha1Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public static string ComputeSha256(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
        public static string ComputeSha384(string rawData)
        {
            using (var sha384Hash = SHA384.Create())
            {
                byte[] sourceBytes = Encoding.UTF8.GetBytes(rawData);
                byte[] hashBytes = sha384Hash.ComputeHash(sourceBytes);
                return BitConverter.ToString(hashBytes).Replace("-", String.Empty);
            }
        }
        public static string ComputeHMACSha384(string rawData, string secretKey)
        {
            var keyByte = Encoding.UTF8.GetBytes(secretKey);
            using (var sha384Hash = new HMACSHA384(keyByte))
            {
                byte[] sourceBytes = Encoding.UTF8.GetBytes(rawData);
                byte[] hashBytes = sha384Hash.ComputeHash(sourceBytes);
                return BitConverter.ToString(hashBytes).Replace("-", String.Empty);
            }
        }
        public static string ComputeHMACSha256(string message, string secretKey)
        {
            var keyByte = Encoding.UTF8.GetBytes(secretKey);
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                var msg = Encoding.UTF8.GetBytes(message);
                var hash = hmacsha256.ComputeHash(msg);
                return BitConverter.ToString(hash).Replace("-", string.Empty);
            }
        }

        public static string ComputeHMACSha512(string message, string secretKey)
        {
            var keyByte = Encoding.UTF8.GetBytes(secretKey);
            using (var hmacsha512 = new HMACSHA512(keyByte))
            {
                var msg = Encoding.UTF8.GetBytes(message);
                var hash = hmacsha512.ComputeHash(msg);
                return BitConverter.ToString(hash).Replace("-", string.Empty);
            }
        }

        public static string ComputeHMACSha1(string message, string secretKey)
        {
            var keyByte = Encoding.UTF8.GetBytes(secretKey);
            using (var hmacsha1 = new HMACSHA1(keyByte))
            {
                var msg = Encoding.UTF8.GetBytes(message);
                var hash = hmacsha1.ComputeHash(msg);
                return BitConverter.ToString(hash).Replace("-", string.Empty);
            }
        }

        public static string GetIpAddress()
        {
            var ipAddress = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            return ipAddress != null ? ipAddress.ToString() : string.Empty;
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

        public static byte[] ComputeMd5Bytes(string data)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                return md5Hash.ComputeHash(Encoding.UTF8.GetBytes(data));
            }
        }

        public static string GetUriEndocingFromObject<T>(T obj)
        {
            var properties = from p in obj.GetType().GetProperties()
                             select p.Name + "=" +
                            Uri.EscapeDataString(p.GetValue(obj, null) != null ? p.GetValue(obj, null).ToString() : string.Empty);

            var requestData = string.Join("&", properties.ToArray().Where(x => !string.IsNullOrEmpty(x) && x[x.Length - 1] != '='));
            return requestData;
        }

        public static string GetUriDataFromObject<T>(T obj)
        {
            var properties = from p in obj.GetType().GetProperties()
                             select p.Name + "=" +
                            (p.GetValue(obj, null) != null ? p.GetValue(obj, null).ToString() : string.Empty);

            var requestData = string.Join("&", properties.ToArray().Where(x => !string.IsNullOrEmpty(x) && x[x.Length - 1] != '='));
            return requestData;
        }

        public static HttpResponseMessage ConvertObjectToXml(object response, string httpContentTypes,
            CustomXmlFormatter.XmlFormatterTypes xmlFormatterTypes = CustomXmlFormatter.XmlFormatterTypes.Standard)
        {
            var formatter = new CustomXmlFormatter(xmlFormatterTypes);
            var output = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ObjectContent(response.GetType(), response, formatter, httpContentTypes)
            };
            return output;
        }

        public static string ToXML(object sourceObject)
        {
            var settings = new XmlWriterSettings
            {
                Indent = true,
                OmitXmlDeclaration = true
            };
            var serializer = new XmlSerializer(sourceObject.GetType());
            using (var stream = new StringWriter())
            using (var writer = XmlWriter.Create(stream, settings))
            {
                var ns = new XmlSerializerNamespaces();
                ns.Add(string.Empty, string.Empty);
                serializer.Serialize(writer, sourceObject, ns);
                return stream.ToString();
            }
        }


        public static string SendHttpRequest(HttpRequestInput input, out WebHeaderCollection responseHeaders,
                                             SecurityProtocolType type = SecurityProtocolType.Tls12, int timeout = 60000,
                                             X509Certificate2 certificate = null, bool ignoreCertificate = false)
        {
            var dataStream = SendHttpRequestForStream(input, out responseHeaders, type, timeout, certificate, ignoreCertificate);
            if (dataStream == null)
                return string.Empty;
            using (var reader = new StreamReader(dataStream))
            {
                var responseFromServer = reader.ReadToEnd();
                reader.Close();
                dataStream.Close();
                return responseFromServer;
            }
        }

        public static Stream SendHttpRequestForStream(HttpRequestInput input, out WebHeaderCollection responseHeaders,
                                                      SecurityProtocolType type, int timeout = 60000,
                                                      X509Certificate2 certificate = null, bool ignoreCertificate = false)
        {
            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = type;
                if (ignoreCertificate)
                    ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                var request = (HttpWebRequest)WebRequest.Create(input.Url);
                request.ContentLength = 0;
                request.Method = input.RequestMethod;
                request.Timeout = timeout;   //Infinite
                request.KeepAlive = true;
                if (certificate != null)
                    request.ClientCertificates.Add(certificate);
                if (input.Date != DateTime.MinValue)
                    request.Date = input.Date;
                if (input.RequestHeaders != null)
                {
                    foreach (var headerValuePair in input.RequestHeaders)
                    {
                        request.Headers[headerValuePair.Key] = headerValuePair.Value;
                    }
                }
                request.ContentType = input.ContentType;
                request.Accept = input.Accept;
                if (!string.IsNullOrWhiteSpace(input.PostData))
                {
                    var data = Encoding.UTF8.GetBytes(input.PostData);
                    request.ContentLength = data.Length;
                    using (var stream = request.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                        stream.Close();
                    }
                }

                var response = (HttpWebResponse)request.GetResponse();
                var dataStream = response.GetResponseStream();
                responseHeaders = response.Headers;
                return dataStream;
            }
            catch (WebException ex)
            {
                if (input.Log != null)
                    input.Log.Error(JsonConvert.SerializeObject(input.PostData) + "_" + input.Url);
                if (ex.Response == null)
                    throw;

                using (var stream = ex.Response.GetResponseStream())
                {
                    using (var reader = new StreamReader(stream))
                    {
                        var response = ex.Response as HttpWebResponse;
                        var msg = reader.ReadToEnd();
                        throw new Exception(JsonConvert.SerializeObject(new
                        {
                            StatusCode = response?.StatusCode,
                            Message = msg,
                            PostData = input.PostData,
                            Url = input.Url,
                            Headers = input.RequestHeaders
                        }));
                    }
                }
            }
            catch (Exception ex)
            {
                if (input.Log != null)
                    input.Log.Error(JsonConvert.SerializeObject(input.PostData) + "_" + input.Url + "_" + ex.Message);
                throw new Exception(JsonConvert.SerializeObject(new
                {
                    PostData = input.PostData,
                    Url = input.Url,
                    Exception = ex
                }));
            }
        }

        public static int CalculateCheckSumEan13(long data)
        {
            if (data > 1000000000000)
                return -1;
            var sum = 0;
            var isEven = false;
            while (data > 0)
            {
                var digit = (int)(data % 10);
                sum += (isEven ? 1 : 3) * digit;

                data /= 10;
                isEven = !isEven;
            }
            var mod = sum % 10;
            return mod == 0 ? 0 : 10 - mod;
        }

        public static long CalculateBarcode(long data)
        {
            var currentTime = DateTime.UtcNow;
            var value = (long)Math.Max(currentTime.Year % 100, 10) * 10000000000;
            value += (long)currentTime.Month * 100000000;
            value += (data % 10000) * 10000;
            value += Convert.ToInt32(GetRandomNumber(4));

            return value * 10 + CalculateCheckSumEan13(value);
        }

        public static TransactionScope CreateTransactionScope(int timeoutMinues = 1)
        {
            return new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadCommitted,
                Timeout = new TimeSpan(0, timeoutMinues, 0)
            });
        }

        public static TransactionScope CreateReadUncommittedTransactionScope()
        {
            return new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadUncommitted
            });
        }

        public static T DeserializeToObject<T>(string input)
        {
            var serializer = new XmlSerializer(typeof(T));
            T result;
            using (TextReader reader = new StringReader(input))
            {
                result = (T)serializer.Deserialize(reader);
            }
            return result;
        }

        public static IpInfo GetIpInfo()
        {
            string externalip = new WebClient().DownloadString("http://icanhazip.com");
            var url = string.Format("{0}/{1}", "http://freegeoip.net/json", externalip);
            using (var wc = new WebClient())
            {
                var result = wc.DownloadString(url);
                return JsonConvert.DeserializeObject<IpInfo>(result);
            }
        }

        public static string GetSortedValuesAsString(object paymentRequest, string delimiter = "")
        {
            var sortedParams = new SortedDictionary<string, object>();
            var properties = paymentRequest.GetType().GetProperties();
            foreach (var field in properties)
            {
                var value = field.GetValue(paymentRequest, null);
                if ((delimiter != string.Empty && value == null) || field.Name.ToLower().Contains("sign"))
                    continue;
                sortedParams.Add(field.Name, value == null ? string.Empty : value.ToString());
            }
            var result = sortedParams.Aggregate(string.Empty, (current, par) => current + par.Value + delimiter);

            return result.Remove(result.LastIndexOf(delimiter), delimiter.Length);
        }

        public static string GetSortedParamWithValuesAsString(object sourceObj, string delimiter = "", bool withEscape = true)
        {
            var sortedParams = new SortedDictionary<string, string>();
            var properties = sourceObj.GetType().GetProperties();
            foreach (var field in properties)
            {
                var value = field.GetValue(sourceObj, null);
                if ((delimiter != string.Empty && value == null) || field.Name.ToLower().Contains("sign"))
                    continue;
                sortedParams.Add(field.Name, value == null ? string.Empty : value.ToString());
            }
            var result = sortedParams.Aggregate(string.Empty, (current, par) => current + par.Key + "=" + ( withEscape ? Uri.EscapeDataString(par.Value) : par.Value ) + delimiter);

            return string.IsNullOrEmpty(result) ? result : result.Remove(result.LastIndexOf(delimiter), delimiter.Length);
        }

        public static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long GetCurrentUnixTimestampMillis()
        {
            return (long)(DateTime.UtcNow.AddMinutes(-2) - UnixEpoch).TotalMilliseconds;
        }

        public static long GetCurrentUnixTimestampSeconds()
        {
            return (long)(DateTime.UtcNow.AddMinutes(-0.5) - UnixEpoch).TotalSeconds;
        }

        public static string ConvertToXmlWithoutNamespace(object input)
        {
            XmlWriterSettings settings = new XmlWriterSettings()
            {
                Indent = false,
                OmitXmlDeclaration = true
            };
            var stream = new MemoryStream();
            using (XmlWriter xw = XmlWriter.Create(stream, settings))
            {
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces(
                                   new[] { XmlQualifiedName.Empty });
                XmlSerializer x = new XmlSerializer(input.GetType(), string.Empty);
                x.Serialize(xw, input, ns);
            }
            var xmlText = Encoding.UTF8.GetString(stream.ToArray());
            var _byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
            if (xmlText.StartsWith(_byteOrderMarkUtf8))
                xmlText = xmlText.Remove(0, _byteOrderMarkUtf8.Length);

            return xmlText;
        }

        public static bool IsIpEqual(this string sourceIp, string ip)
        {
            if (sourceIp == ip)
                return true;
            var sourceIpDottedNums = sourceIp.Split('.');
            var ipDottedNums = ip.Split('.');
            for (int i = 0; i < sourceIpDottedNums.Length; ++i)
            {
                if (sourceIpDottedNums[i] != ipDottedNums[i] && sourceIpDottedNums[i] != "*")
                    return false;
            }
            return true;
        }
        public static bool IsIpInRange(this string sourceIp, string ip)
        {
            if (sourceIp == ip)
                return true;
            var lowerInclusive = IPAddress.Parse(sourceIp.Replace("*", "0"));
            var address = IPAddress.Parse(ip);
            byte[] lowerBytes = lowerInclusive.GetAddressBytes();
            byte[] upperBytes = IPAddress.Parse(sourceIp.Replace("*", "9").Replace("999", "255")).GetAddressBytes();
            var addressFamily = lowerInclusive.AddressFamily;
            if (address.AddressFamily != addressFamily)
            {
                return false;
            }

            byte[] addressBytes = address.GetAddressBytes();
            bool lowerBoundary = true, upperBoundary = true;
            for (int i = 0; i < lowerBytes.Length && (lowerBoundary || upperBoundary); i++)
            {
                if ((lowerBoundary && addressBytes[i] < lowerBytes[i]) ||
                    (upperBoundary && addressBytes[i] > upperBytes[i]))
                    return false;
                lowerBoundary &= (addressBytes[i] == lowerBytes[i]);
                upperBoundary &= (addressBytes[i] == upperBytes[i]);
            }

            return true;
        }

        public static string UploadImage(int clientId, string imageData, string imageName, string path, ILog log)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string imgName = Guid.NewGuid().ToString() + "_" + clientId.ToString() + "_" + imageName;
            var imgSavePath = Path.Combine(path, imgName);

            byte[] bytes = Convert.FromBase64String(imageData);

            Image clientDocImg;
            using (var ms = new MemoryStream(bytes, 0, bytes.Length))
            {
                var result = Image.FromStream(ms, true);
                clientDocImg = new Bitmap(result);
            }
            clientDocImg.Save(imgSavePath);
            return imgName;
        }

        public static string GeteratePin(string code)
        {
            var enc = new Base32Encoder();
            byte[] data = new byte[10];
            RandomNumberGenerator rng = RandomNumberGenerator.Create();
            rng.GetBytes(data);
            data = enc.Decode(code);
            return GenerateResponseCode(GetCurrentInterval(), data);
        }

        private static long GetCurrentInterval()
        {
            int intervalLength = 30;
            TimeSpan TS = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            long currentTimeSeconds = (long)Math.Floor(TS.TotalSeconds);
            long currentInterval = currentTimeSeconds / intervalLength; // 30 Seconds
            return currentInterval;
        }

        public static string GenerateQRCode()
        {
            byte[] data = new byte[10];
            var rnd = new RNGCryptoServiceProvider();
            rnd.GetBytes(data);
            var enc = new Base32Encoder();
            return enc.Encode(data);
        }

        private static string GenerateResponseCode(long challenge, byte[] randomBytes)
        {
            int pinCodeLength = 6;
            int pinModulo = (int)Math.Pow(10, pinCodeLength);
            HMACSHA1 myHmac = new HMACSHA1(randomBytes);
            myHmac.Initialize();

            byte[] value = BitConverter.GetBytes(challenge);
            Array.Reverse(value); //reverses the challenge array due to differences in c# vs java
            myHmac.ComputeHash(value);
            byte[] hash = myHmac.Hash;
            int offset = hash[hash.Length - 1] & 0xF;
            byte[] SelectedFourBytes = new byte[4];
            //selected bytes are actually reversed due to c# again, thus the weird stuff here
            SelectedFourBytes[0] = hash[offset];
            SelectedFourBytes[1] = hash[offset + 1];
            SelectedFourBytes[2] = hash[offset + 2];
            SelectedFourBytes[3] = hash[offset + 3];
            Array.Reverse(SelectedFourBytes);
            int finalInt = BitConverter.ToInt32(SelectedFourBytes, 0);
            int truncatedHash = finalInt & 0x7FFFFFFF; //remove the most significant bit for interoperability as per HMAC standards
            string pinValue = (truncatedHash % pinModulo).ToString();//generate 10^d digits where d is the number of digits
            for (int i = pinValue.Length; i < pinCodeLength; i++)
            {
                pinValue = "0" + pinValue;
            }
            return pinValue;
        }
        private static readonly string PrivateKey =
            "<RSAKeyValue><Modulus>xDV6Tkh5noY4NWUd5G3qmdLkhBCRwwG06buiCc34Xyshtc8eRMaLZarHeQpEkPk43f7Xo7UhOekqQ1+/JlZOrCKRUb8VPdLv/gA9lxzDAJDuSTN8RY0BBJlwNjrP3nrDLIfxAe5OZXxgdwvSi5RxnghWaiM7qtrcaAKcnZnoNrHXvouw9CbLEJa92RJjKIUE94GbFozDSM62CokXrKC9RgcwZfLQijzlgKM90pjLjAlw1Iz/IN3g5k+TqrbHX7zo8I85MIn/1yRyUIGMT3BUMiPKRkCqVZ1NGPLLsuz4vCe/wA9dmhZxrEAECLEFp6/9PoT9abcIxIYJpV1PhxeG1w==</Modulus><Exponent>AQAB</Exponent><P>5LLJ45YKO6chOa/GPvmnGy5oIzY3al3StqK4b4O+Iw37ERnCzuksp6t0P/v21Rxce9Dn0wd5iiR6H+ZLbo7nvdQTV7ryVDWDO0SkNvH1XD6J4gptDFWz7M0i3vG10KDwhzaC7N9j5NPy4B+PjOsDoB8iFgFeTVO4PvfFSe9kzds=</P><Q>26HIir0MLHdeX2WusUTHSe2ukowl8UnFFaQoxD/ttXTsdBQMZC6xgWgxb9KtU/26v3oQv8uFqtLldP5eRefoV7Mivq6CQ0TQVWd3xMkd8R07ybXt9lME5/DgWPz6SbEdXFlHjum2y4usJ6kYUgXCOwUctpgz4rqbmimNScYWYbU=</Q><DP>WJj+54Ef075Ke9uhtJHo7/nJdCKz0ywnzoM5alIiXdgztItDUf85QneEoKkPFb5YAcuLk9BogGDjQupnvJv2IS9AkxMkgAT/Iv3TlEmmISdFKWGan1WwT4OlB7OiGQHQTMGMdRGR1HtbswHnDdOZ4vVMsjOzgcd2MEaykpMAfVM=</DP><DQ>q/r1Q95gx/j4xw6iSmEnBHa/ejWQCG7Riu6ulW3Rv4M9HHAOe+wsRr7F52A7JUfLkeANeYHuuyLFVmVQgMDlqLa3AEU5717VG+sXV9p8Pa+8f2icW4QKlWyC4GvHuSidaxDl/bx4zM4kEjJQvvmPbBPGthxclK+25HKhFiGsqPk=</DQ><InverseQ>iePVWEunHFD6l6A6Y74ppPtNP0zjetRNSjdppS+eU4dQsYU1K56pGdvg+RqQlT4hXLpdnjHUlyWBrOSm7MuicSVLasCJ3285bRpCvWtJcLlglLCE9fkTOPNLjnQ1foQk1sWB7NLUclaG9xppnvmgLZvx0YQoVhP2Vr1wDm1bpWA=</InverseQ><D>VQLCoiZeo2ON+Px9rho9mjY4kkvHi9EyfE6yj0LxiPJcIbTCbZQEk6Eh2fyr5pBEplKjRafV5Ix0pkpWvJqKbaRwiBWdc3LwToH2LYHlr1ocFBU9k7jbJw4AA08J/1/7LlEcB/UjfG8eMJYrvBQuAgWkw0nOsWEwO9Rd3R7w8Ljs/BpmVviNlkyWvjZ6spfo3R18jTQY102+jOoyIh3w++W9vo4XypTnWOWm4Wnd9Da4IYrwPfDrV5QeM9s4RTlPBAb9vXo2BHjwOtVShiWkL78JxIPTWWKIDwZcZJrm1turfjJqEKzzfGsHwAr2/iuFaRLeVWCRkKdARAWOiOwPGQ==</D></RSAKeyValue>";
        public static string RSADecrypt(string data)
        {
            using (var provider = new RSACryptoServiceProvider(2048))
            {
                provider.FromXmlString(PrivateKey);
                return Encoding.UTF8.GetString(provider.Decrypt(Convert.FromBase64String(data), false));
            }
        }

        public static Image ResizeImage(Image image, Size size)
        {
            Image newImage = new Bitmap(size.Width, size.Height);
            using (Graphics graphicsHandle = Graphics.FromImage(newImage))
            {
                graphicsHandle.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphicsHandle.DrawImage(image, 0, 0, size.Width, size.Height);
            }
            return newImage;
        }

        public static IEnumerable<T> Traverse<T>(this T root, Func<T, IEnumerable<T>> childSelector)
        {
            var stack = new Stack<T>();
            stack.Push(root);

            while (stack.Any())
            {
                var next = stack.Pop();
                yield return next;
                foreach (var child in childSelector(next))
                    stack.Push(child);
            }
        }

        public static object CopyValues<T>(object target, T source)
        {
            Type t = typeof(T);
            if (target == null || target.ToString() == "{}")
                return source;
            var properties = t.GetProperties().Where(prop => prop.CanRead && prop.CanWrite);
            var targetProperties = target.GetType().GetProperties().Where(prop => prop.CanRead && prop.CanWrite);
            foreach (var prop in properties)
            {
                var value = prop.GetValue(source, null);
                if (value != null && targetProperties.Any(x=>x.Name == prop.Name))
                    prop.SetValue(target, value, null);
            }
            return target;
        }
    }
}
