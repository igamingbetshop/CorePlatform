using IqSoft.CP.AutomationTest.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace IqSoft.CP.AutomationTest.Helpers
{
    public static class Common1
    {
        public static string SendHttpRequest(HttpRequestInput input, 
                                           SecurityProtocolType type = SecurityProtocolType.Tls12, int timeout = 60000,
                                           X509Certificate2 certificate = null, bool ignoreCertificate = false)
        {
            using var dataStream = SendHttpRequestForStream(input, type, timeout, certificate, ignoreCertificate);
            if (dataStream == null)
                return string.Empty;
            using var reader = new StreamReader(dataStream);
            var responseFromServer = reader.ReadToEnd();
            reader.Close();
            dataStream.Close();
            return responseFromServer;
        }

        public static Stream SendHttpRequestForStream(HttpRequestInput input,
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
                using var httpClient = new HttpClient(handler) { Timeout = TimeSpan.FromTicks(timeout) };
                request.Headers.Accept.TryParseAdd(input.Accept);
                if (input.RequestHeaders != null)
                {
                    foreach (var headerValuePair in input.RequestHeaders)
                    {
                        httpClient.DefaultRequestHeaders.Add(headerValuePair.Key, headerValuePair.Value);
                    }
                }

                var response = httpClient.Send(request);
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

    }
}
