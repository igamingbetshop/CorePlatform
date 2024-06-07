using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Net;
using log4net;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Integration.Payments.Models.SerosPay;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common;
using Newtonsoft.Json;
using System.Web.Script.Serialization;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL;

namespace IqSoft.CP.Integration.Payments.Helpers
{
    public static class SerosPayHelpers
    {
        private static class PaymentRequestStatus
        {
            public const string Accepted = "ACCEPTED";
        }

        public static UserLoginOutput UserLogin(int paymentSystemId, int partnerId, out CookieCollection cookieContainer, SessionIdentity session, ILog log)
        {
            using (var partnerBl = new PartnerBll(session, log))
            {
                cookieContainer = new CookieCollection();
                var partner = CacheManager.GetPartnerById(partnerId);
                if (partner == null || partner.Id == 0)
                    return null;
                var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(partnerId, paymentSystemId, partner.CurrencyId, (int)PaymentRequestTypes.Deposit);

                if (partnerPaymentSetting == null ||
                    (string.IsNullOrEmpty(partnerPaymentSetting.UserName) && string.IsNullOrEmpty(partnerPaymentSetting.Password)))
                    return null;

                if (string.IsNullOrEmpty(partnerPaymentSetting.UserName) || string.IsNullOrEmpty(partnerPaymentSetting.Password))
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerPaymentSettingNotFound);

                var email = partnerPaymentSetting.UserName;
                var password = partnerPaymentSetting.Password;
                var url = partnerBl.GetPaymentValueByKey(partnerId, paymentSystemId, Constants.PartnerKeys.BankTransferUrl);
				if (string.IsNullOrEmpty(url))
					return null;

                var loginInput = new Models.SerosPay.LoginInput { Email = email, Password = password };
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    Url = url + "/api/1.0/user/login",
                    PostData = JsonConvert.SerializeObject(loginInput)
                };
                var serializer = new JavaScriptSerializer();
                return serializer.Deserialize<UserLoginOutput>(SendHttpRequest(httpRequestInput, ref cookieContainer));
            }
        }

        public static List<PaymentModel> EntryListing(int paymentSystemId, int partnerId, CookieCollection cookieCollection, SessionIdentity session, ILog log)
        {
            using (var partnerBl = new PartnerBll(session, log))
            {
                var url = partnerBl.GetPaymentValueByKey(partnerId, paymentSystemId, Constants.PartnerKeys.BankTransferUrl);
                var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = Constants.HttpRequestMethods.Get,
                    Url = url + "/api/1.0/entry/listing"
                };
                var serializer = new JavaScriptSerializer();
                var entryList = serializer.Deserialize<PaymentModel[]>(SendHttpRequest(httpRequestInput, ref cookieCollection))
                                         .Where(x => x.Status == "ENTERED").ToList();
                return entryList;
            }
        }

        private static PaymentModel UpdateEntry(PaymentModel entry, int paymentSystemId, int partnerId, CookieCollection cookieCollection, SessionIdentity session, ILog log)
        {
            ChangeEntryStatus(entry, PaymentRequestStatus.Accepted, paymentSystemId, partnerId, cookieCollection, session, log);
			return entry;
        }

        private static void ChangeEntryStatus(PaymentModel entry, string status, int paymentSystemId, int partnerId,
                                       CookieCollection cookieCollection, SessionIdentity session, ILog log)
        {
            using (var partnerBl = new PartnerBll(session, log))
            {
                var url = partnerBl.GetPaymentValueByKey(partnerId, paymentSystemId, Constants.PartnerKeys.BankTransferUrl);
                var entryField = new FieldModel { Entry = entry, Field = "status", Original = entry.Status };
                entryField.Entry.Status = status;
				log.Info(JsonConvert.SerializeObject(entryField));
				var httpRequestInput = new HttpRequestInput
                {
                    ContentType = Constants.HttpContentTypes.ApplicationJson,
                    RequestMethod = Constants.HttpRequestMethods.Post,
                    Url = url + "/api/1.0/entry/update",
                    PostData = JsonConvert.SerializeObject(entryField)
                };
                var resp = SendHttpRequest(httpRequestInput, ref cookieCollection);
				log.Info(resp);
			}
        }

        public static PaymentModel UpdatePaymentEntries(PaymentModel entryField, PaymentRequest paymentRequest, SessionIdentity session, ILog log)
        {
            var client = CacheManager.GetClientById(paymentRequest.ClientId.Value);
            if (UserLogin(paymentRequest.PaymentSystemId, client.PartnerId, out CookieCollection cookieCollection, session, log) != null)
                return UpdateEntry(entryField, paymentRequest.PaymentSystemId, client.PartnerId, cookieCollection, session, log);

			throw BaseBll.CreateException(session.LanguageId, Constants.Errors.WrongApiCredentials);
        }

        public static List<PaymentModel> GetPaymentEntries(int paymentSystemId, int partnerId, SessionIdentity session, ILog log)
        {
            CookieCollection cookieCollection;
            if (UserLogin(paymentSystemId, partnerId, out cookieCollection, session, log) != null)
                return EntryListing(paymentSystemId, partnerId, cookieCollection, session, log);
            return new List<PaymentModel>();
        }

        public static string SendHttpRequest(HttpRequestInput input, ref CookieCollection cookie)
        {
            var dataStream = SendHttpRequestForStream(input, ref cookie);
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

        public static Stream SendHttpRequestForStream(HttpRequestInput input, ref CookieCollection cookieCollection)
        {
            var request = (HttpWebRequest)WebRequest.Create(input.Url);
            request.ContentLength = 0;
            request.Method = input.RequestMethod;
            var cookie = new CookieContainer();
            if (cookieCollection != null)
                cookie.Add(cookieCollection);
            request.CookieContainer = cookie;
            if (!string.IsNullOrWhiteSpace(input.PostData))
            {
                var data = Encoding.UTF8.GetBytes(input.PostData);
                request.ContentType = input.ContentType;
                request.ContentLength = data.Length;
                request.Accept = input.Accept;
                if (input.RequestHeaders != null)
                {
                    foreach (var headerValuePair in input.RequestHeaders)
                    {
                        request.Headers[headerValuePair.Key] = headerValuePair.Value;
                    }
                }
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                    stream.Close();
                }
            }
            var response = (HttpWebResponse)request.GetResponse();
            cookieCollection = response.Cookies;
            var dataStream = response.GetResponseStream();
            return dataStream;
        }
    }
}
