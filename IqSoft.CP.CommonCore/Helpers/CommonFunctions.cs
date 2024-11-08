﻿using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;
using System.Transactions;
using Newtonsoft.Json;
using IqSoft.CP.Common.Models;
using System.Net.Http;
using System.Collections.Generic;
using System.Xml;
using System.Security.Cryptography.X509Certificates;

using System.Net.Http.Headers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

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

        public static long GenerateRandomNumber()
        {
            byte[] gb = Guid.NewGuid().ToByteArray();
            long longNumber = BitConverter.ToInt64(gb, 0);
            return longNumber;
        }

        public static string ComputeSha512(string path)
        {
            using var hash = SHA512.Create();
            var messageBytes = Encoding.UTF8.GetBytes(path);
            var hashValue = hash.ComputeHash(messageBytes);
            return hashValue.Aggregate(String.Empty, (current, b) => current + String.Format("{0:x2}", b));
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
            using var sha1Hash = SHA1.Create();
            byte[] bytes = sha1Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            var builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }

        public static string ComputeSha256(string rawData)
        {
            using var sha256Hash = SHA256.Create();
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            var builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }


        public static string ComputeHMACSha256(string message, string secretKey)
        {
            var keyByte = Encoding.UTF8.GetBytes(secretKey);
            using var hmacsha256 = new HMACSHA256(keyByte);
            var msg = Encoding.UTF8.GetBytes(message);
            var hash = hmacsha256.ComputeHash(msg);
            return BitConverter.ToString(hash).Replace("-", string.Empty);
        }
        public static string ComputeSha384(string rawData)
        {
            using var sha384Hash = SHA384.Create();
            byte[] sourceBytes = Encoding.UTF8.GetBytes(rawData);
            byte[] hashBytes = sha384Hash.ComputeHash(sourceBytes);
            return BitConverter.ToString(hashBytes).Replace("-", String.Empty);
        }
        public static string ComputeHMACSha512(string message, string secretKey)
        {
            var keyByte = Encoding.UTF8.GetBytes(secretKey);
            using var hmacsha512 = new HMACSHA512(keyByte);
            var msg = Encoding.UTF8.GetBytes(message);
            var hash = hmacsha512.ComputeHash(msg);
            return BitConverter.ToString(hash).Replace("-", string.Empty);
        }

        public static string ComputeHMACSha1(string message, string secretKey)
        {
            var keyByte = Encoding.UTF8.GetBytes(secretKey);
            using var hmacsha1 = new HMACSHA1(keyByte);
            var msg = Encoding.UTF8.GetBytes(message);
            var hash = hmacsha1.ComputeHash(msg);
            return BitConverter.ToString(hash).Replace("-", string.Empty);
        }

        public static string GetIpAddress()
        {
            var ipAddress = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            return ipAddress != null ? ipAddress.ToString() : string.Empty;
        }

        public static string ComputeMd5(string data)
        {
            using var md5Hash = MD5.Create();
            var bytes = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(data));
            var sBuilder = new StringBuilder();
            foreach (var t in bytes)
            {
                sBuilder.Append(t.ToString("x2"));
            }
            return sBuilder.ToString();
        }

        public static byte[] ComputeMd5Bytes(string data)
        {
            using var md5Hash = MD5.Create();
            return md5Hash.ComputeHash(Encoding.UTF8.GetBytes(data));
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

        public static ObjectContent ConvertObjectToXml(object response, string httpContentTypes,
            CustomXmlFormatter.XmlFormatterTypes xmlFormatterTypes = CustomXmlFormatter.XmlFormatterTypes.Standard)
        {
            var formatter = new CustomXmlFormatter(xmlFormatterTypes);
            return new ObjectContent(response.GetType(), response, formatter, httpContentTypes);
        }

        public static string SendHttpRequest(HttpRequestInput input, out HttpResponseHeaders responseHeaders,
                                           SecurityProtocolType type = SecurityProtocolType.Tls12, int timeout = 60000,
                                           X509Certificate2 certificate = null, bool ignoreCertificate = false)
        {
            using var dataStream = SendHttpRequestForStream(input, out responseHeaders, type, timeout, certificate, ignoreCertificate);
            if (dataStream == null)
                return string.Empty;
            using var reader = new StreamReader(dataStream);
            var responseFromServer = reader.ReadToEnd();
            reader.Close();
            dataStream.Close();
            return responseFromServer;
        }

        public static Stream SendHttpRequestForStream(HttpRequestInput input, out HttpResponseHeaders responseHeaders,
                                                      SecurityProtocolType type, int timeout = 60000,
                                                      X509Certificate2 certificate = null, bool ignoreCertificate = false)
        {
            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = type;
                if (ignoreCertificate)
                    ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                using var handler = new HttpClientHandler();
                if (certificate != null)
                    handler.ClientCertificates.Add(certificate);
                using var request = new HttpRequestMessage(input.RequestMethod, input.Url);
                if (!string.IsNullOrEmpty(input.PostData))
                    request.Content = new StringContent(input.PostData, Encoding.UTF8, input.ContentType);
                using var httpClient = new HttpClient(handler) { Timeout = TimeSpan.FromMilliseconds(timeout) };
                request.Headers.Accept.TryParseAdd(input.Accept);
                if (input.Date != DateTime.MinValue)
                    request.Headers.Date = input.Date;
                if (input.RequestHeaders != null)
                {
                    foreach (var headerValuePair in input.RequestHeaders)
                    {
                        httpClient.DefaultRequestHeaders.Add(headerValuePair.Key, headerValuePair.Value);
                    }
                }

                var response = httpClient.Send(request);
                responseHeaders = response.Headers;
                return response.Content.ReadAsStream();
            }
            catch (HttpRequestException ex)
            {
                if (input.Log != null)
                    input.Log.Error(JsonConvert.SerializeObject(input.PostData) + "_" + input.Url +"_" + ex.Message);
                throw new Exception(string.Format("Statuse code: {0}, Body: {1}", ex.StatusCode, ex.Message));

            }
            catch (Exception ex)
            {
                if (input.Log != null)
                    input.Log.Error(JsonConvert.SerializeObject(input.PostData) + "_" + input.Url + "_" + ex.Message);

                throw;
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
            data += 100000000000;
            return data * 10 + CalculateCheckSumEan13(data);
        }

        public static TransactionScope CreateTransactionScope(int timeoutMinues = 1)
        {
            return new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadCommitted,
                Timeout = new TimeSpan(0, timeoutMinues, 0)
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

        public static string GetSortedParamWithValuesAsString(object paymentRequest, string delimiter = "")
        {
            var sortedParams = new SortedDictionary<string, string>();
            var properties = paymentRequest.GetType().GetProperties();
            foreach (var field in properties)
            {
                var value = field.GetValue(paymentRequest, null);
                if ((delimiter != string.Empty && value == null) || field.Name.ToLower().Contains("sign"))
                    continue;
                sortedParams.Add(field.Name, value == null ? string.Empty : value.ToString());
            }
            var result = sortedParams.Aggregate(string.Empty, (current, par) => current + par.Key + "=" + Uri.EscapeDataString(par.Value) + delimiter);

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
            var settings = new XmlWriterSettings()
            {
                Indent = false,
                OmitXmlDeclaration = true
            };
            var stream = new MemoryStream();
            using (XmlWriter xw = XmlWriter.Create(stream, settings))
            {
                var ns = new XmlSerializerNamespaces(
                                   new[] { XmlQualifiedName.Empty });
                var x = new XmlSerializer(input.GetType(), string.Empty);
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
        public static void SaveImage(byte[] bytes, string imgSavePath)
        {
            using Image image = Image.Load(bytes);
            image.Save(imgSavePath);
        }

        public static string UploadImage(int clientId, string imageData, string imageName, string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string imgName = Guid.NewGuid().ToString() + "_" + clientId.ToString() + "_" + imageName;
            var imgSavePath = Path.Combine(path, imgName);
            byte[] bytes = Convert.FromBase64String(imageData);
            SaveImage(bytes, imgSavePath);
            return imgName;
        }
        public static string GeteratePin(string code)
        {
            var enc = new Base32Encoder();
            byte[] data = new byte[10];
            var rng = RandomNumberGenerator.Create();
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
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(data);
            var enc = new Base32Encoder();
            return enc.Encode(data);
        }

        private static string GenerateResponseCode(long challenge, byte[] randomBytes)
        {
            int pinCodeLength = 6;
            int pinModulo = (int)Math.Pow(10, pinCodeLength);
            var myHmac = new HMACSHA1(randomBytes);
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

        public static string ToXML(object sourceObject)
        {
            var settings = new XmlWriterSettings
            {
                Indent = true,
                OmitXmlDeclaration = true
            };
            var serializer = new XmlSerializer(sourceObject.GetType());
            using var stream = new StringWriter();
            using var writer = XmlWriter.Create(stream, settings);
            var ns = new XmlSerializerNamespaces();
            ns.Add(string.Empty, string.Empty);
            serializer.Serialize(writer, sourceObject, ns);
            return stream.ToString();
        }

        private static readonly string PrivateKey =
          "<RSAKeyValue><Modulus>xDV6Tkh5noY4NWUd5G3qmdLkhBCRwwG06buiCc34Xyshtc8eRMaLZarHeQpEkPk43f7Xo7UhOekqQ1+/JlZOrCKRUb8VPdLv/gA9lxzDAJDuSTN8RY0BBJlwNjrP3nrDLIfxAe5OZXxgdwvSi5RxnghWaiM7qtrcaAKcnZnoNrHXvouw9CbLEJa92RJjKIUE94GbFozDSM62CokXrKC9RgcwZfLQijzlgKM90pjLjAlw1Iz/IN3g5k+TqrbHX7zo8I85MIn/1yRyUIGMT3BUMiPKRkCqVZ1NGPLLsuz4vCe/wA9dmhZxrEAECLEFp6/9PoT9abcIxIYJpV1PhxeG1w==</Modulus><Exponent>AQAB</Exponent><P>5LLJ45YKO6chOa/GPvmnGy5oIzY3al3StqK4b4O+Iw37ERnCzuksp6t0P/v21Rxce9Dn0wd5iiR6H+ZLbo7nvdQTV7ryVDWDO0SkNvH1XD6J4gptDFWz7M0i3vG10KDwhzaC7N9j5NPy4B+PjOsDoB8iFgFeTVO4PvfFSe9kzds=</P><Q>26HIir0MLHdeX2WusUTHSe2ukowl8UnFFaQoxD/ttXTsdBQMZC6xgWgxb9KtU/26v3oQv8uFqtLldP5eRefoV7Mivq6CQ0TQVWd3xMkd8R07ybXt9lME5/DgWPz6SbEdXFlHjum2y4usJ6kYUgXCOwUctpgz4rqbmimNScYWYbU=</Q><DP>WJj+54Ef075Ke9uhtJHo7/nJdCKz0ywnzoM5alIiXdgztItDUf85QneEoKkPFb5YAcuLk9BogGDjQupnvJv2IS9AkxMkgAT/Iv3TlEmmISdFKWGan1WwT4OlB7OiGQHQTMGMdRGR1HtbswHnDdOZ4vVMsjOzgcd2MEaykpMAfVM=</DP><DQ>q/r1Q95gx/j4xw6iSmEnBHa/ejWQCG7Riu6ulW3Rv4M9HHAOe+wsRr7F52A7JUfLkeANeYHuuyLFVmVQgMDlqLa3AEU5717VG+sXV9p8Pa+8f2icW4QKlWyC4GvHuSidaxDl/bx4zM4kEjJQvvmPbBPGthxclK+25HKhFiGsqPk=</DQ><InverseQ>iePVWEunHFD6l6A6Y74ppPtNP0zjetRNSjdppS+eU4dQsYU1K56pGdvg+RqQlT4hXLpdnjHUlyWBrOSm7MuicSVLasCJ3285bRpCvWtJcLlglLCE9fkTOPNLjnQ1foQk1sWB7NLUclaG9xppnvmgLZvx0YQoVhP2Vr1wDm1bpWA=</InverseQ><D>VQLCoiZeo2ON+Px9rho9mjY4kkvHi9EyfE6yj0LxiPJcIbTCbZQEk6Eh2fyr5pBEplKjRafV5Ix0pkpWvJqKbaRwiBWdc3LwToH2LYHlr1ocFBU9k7jbJw4AA08J/1/7LlEcB/UjfG8eMJYrvBQuAgWkw0nOsWEwO9Rd3R7w8Ljs/BpmVviNlkyWvjZ6spfo3R18jTQY102+jOoyIh3w++W9vo4XypTnWOWm4Wnd9Da4IYrwPfDrV5QeM9s4RTlPBAb9vXo2BHjwOtVShiWkL78JxIPTWWKIDwZcZJrm1turfjJqEKzzfGsHwAr2/iuFaRLeVWCRkKdARAWOiOwPGQ==</D></RSAKeyValue>";
        public static string RSADecrypt(string data)
        {
            using var provider = new RSACryptoServiceProvider(2048);
            provider.FromXmlString(PrivateKey);
            return Encoding.UTF8.GetString(provider.Decrypt(Convert.FromBase64String(data), false));
        }

        //test key
        private static readonly string PublicKey = "<RSAKeyValue><Modulus>21wEnTU+mcD2w0Lfo1Gv4rtcSWsQJQTNa6gio05AOkV/Er9w3Y13Ddo5wGtjJ19402S71HUeN0vbKILLJdRSES5MHSdJPSVrOqdrll/vLXxDxWs/U0UT1c8u6k/Ogx9hTtZxYwoeYqdhDblof3E75d9n2F0Zvf6iTb4cI7j6fMs=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

        public static string RSAEncrypt(string data)
        {
            using var provider = new RSACryptoServiceProvider(2048);
            provider.FromXmlString(PublicKey);
            var byteData = Encoding.UTF8.GetBytes(data);
            var encryptedData = provider.Encrypt(byteData, false);
            return Convert.ToBase64String(encryptedData);
        }

        public static MemoryStream ResizeImage(byte[] bytes, int maxWidth, int maxHeight)
        {
            using var outStream = new MemoryStream();
                using Image image = Image.Load(bytes);
            image.Mutate(x => x.Resize(new Size(Math.Min(image.Width, maxWidth), Math.Min(image.Height, maxHeight))));
            image.SaveAsJpeg(outStream);
            return outStream;
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
    }
}


