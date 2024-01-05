using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace IqSoft.CP.BLL.Helpers
{
    public static class SMSHelpers
    {
        public static void SendSMS(int notificationServiceId, ApiSMSModel apiSMSModel, ILog log)
        {
            switch (notificationServiceId)
            {
                case (int)NotificationServices.KCell:
                    SendSmsToKCell(apiSMSModel);
                    break;
                case (int)NotificationServices.Mas:
                    SendSmsToMas(apiSMSModel);
                    break;
                case (int)NotificationServices.Oztekno:
                    SendSmsToOztekno(apiSMSModel);
                    break;
                case (int)NotificationServices.Clickatell:
                    SendSmsToClickatell(apiSMSModel);
                    break;
                case (int)NotificationServices.Nexmo:
                    SendSmsToNeximo(apiSMSModel, log);
                    break;
                case (int)NotificationServices.SMSCentre:
                    SendSmsToSMSCentre(apiSMSModel);
                    break;
                case (int)NotificationServices.Twilio:
                    SendSmsToSMSTwilio(apiSMSModel);
                    break;
                case (int)NotificationServices.Salesforce:
                    SendSmsToSalesforce(apiSMSModel);
                    break;
                case (int)NotificationServices.SMSTo:
                    SendSmsToSMSTo(apiSMSModel, log);
                    break;
                case (int)NotificationServices.GeezSms:
					SendSmsToGeezSms(apiSMSModel, log);
                    break;
                default:
                    break;
            }
        }

        private static void SendSmsToClickatell(ApiSMSModel apiSMSModel)
        {
            string sender = CacheManager.GetNotificationServiceValueByKey(apiSMSModel.PartnerId, Constants.PartnerKeys.SmsSender, (int)NotificationServices.Clickatell);
            string url = CacheManager.GetNotificationServiceValueByKey(apiSMSModel.PartnerId, Constants.PartnerKeys.SmsGwUrl, (int)NotificationServices.Clickatell);

            var messageRequest = new ClickatellInput
            {
                apiKey = sender,
                to =apiSMSModel.Recipient.Replace("+", string.Empty),
                content = apiSMSModel.MessegeText
            };

            var httpRequestInput =  new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Get,
                Url = url + "?" + CommonFunctions.GetUriEndocingFromObject<ClickatellInput>(messageRequest)
            };
            CommonFunctions.SendHttpRequest(httpRequestInput, out _);
        }
        private static void SendSmsToKCell(ApiSMSModel apiSMSModel)
        {
            string sender = CacheManager.GetNotificationServiceValueByKey(apiSMSModel.PartnerId, Constants.PartnerKeys.SmsSender, (int)NotificationServices.KCell);
            string login = CacheManager.GetNotificationServiceValueByKey(apiSMSModel.PartnerId, Constants.PartnerKeys.SmsGwLogin, (int)NotificationServices.KCell);
            string password = CacheManager.GetNotificationServiceValueByKey(apiSMSModel.PartnerId, Constants.PartnerKeys.SmsGwPass, (int)NotificationServices.KCell);
            string url = CacheManager.GetNotificationServiceValueByKey(apiSMSModel.PartnerId, Constants.PartnerKeys.SmsGwUrl, (int)NotificationServices.KCell);
            string timeBounds = CacheManager.GetNotificationServiceValueByKey(apiSMSModel.PartnerId, Constants.PartnerKeys.SmsTimeBounds, (int)NotificationServices.KCell);

            var messageRequest = new KCellMessageRequest
            {
                ClientMessageId = apiSMSModel.MessageId.ToString(),
                TimeBounds = timeBounds,
                Sender = sender,
                Recipient = apiSMSModel.Recipient.Replace("+", ""),
                MessageText = apiSMSModel.MessegeText
            };
            var requestHeaders = new Dictionary<string, string>
            { { "Authorization", "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(login + ":" + password)) } };

            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Post,
                Url = url,
                PostData = JsonConvert.SerializeObject(messageRequest),
                RequestHeaders = requestHeaders
            };
            CommonFunctions.SendHttpRequest(httpRequestInput, out _);
        }
        private static void SendSmsToMas(ApiSMSModel apiSMSModel)
        {
            string sender = CacheManager.GetNotificationServiceValueByKey(apiSMSModel.PartnerId, Constants.PartnerKeys.SmsSender, (int)NotificationServices.Mas);
            string login = CacheManager.GetNotificationServiceValueByKey(apiSMSModel.PartnerId, Constants.PartnerKeys.SmsGwLogin, (int)NotificationServices.Mas);
            string password = CacheManager.GetNotificationServiceValueByKey(apiSMSModel.PartnerId, Constants.PartnerKeys.SmsGwPass, (int)NotificationServices.Mas);
            string url = CacheManager.GetNotificationServiceValueByKey(apiSMSModel.PartnerId, Constants.PartnerKeys.SmsGwUrl, (int)NotificationServices.Mas);

            var messageRequest = new MasMessageRequest
            {
                apiNo = 1,
                user = login,
                pass = password,
                mesaj = apiSMSModel.MessegeText,
                numaralar = apiSMSModel.Recipient.Replace("+90", ""),
                baslik = sender
            };
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                RequestMethod = Constants.HttpRequestMethods.Post,
                Url = url,
                PostData = CommonFunctions.GetUriEndocingFromObject<MasMessageRequest>(messageRequest)
            };
            CommonFunctions.SendHttpRequest(httpRequestInput, out _);
        }
        private static void SendSmsToOztekno(ApiSMSModel apiSMSModel)
        {
            string login = CacheManager.GetNotificationServiceValueByKey(apiSMSModel.PartnerId, Constants.PartnerKeys.SmsGwLogin, (int)NotificationServices.Oztekno);
            string password = CacheManager.GetNotificationServiceValueByKey(apiSMSModel.PartnerId, Constants.PartnerKeys.SmsGwPass, (int)NotificationServices.Oztekno);
            string url = CacheManager.GetNotificationServiceValueByKey(apiSMSModel.PartnerId, Constants.PartnerKeys.SmsGwUrl, (int)NotificationServices.Oztekno);

            var messageRequest = new OzteknoMessageRequest
            {
                ServiceInfo = new Info
                {
                    UserName = login,
                    Password = password,
                    SendDate = DateTime.UtcNow.AddHours(3).ToString("yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture)
                },
                ProcessInfo = new Process
                {
                    SendInfo = new Send
                    {
                        MassageText = apiSMSModel.MessegeText,
                        MobileNumber = apiSMSModel.Recipient.Replace("+90", "")
                    }
                }
            };
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationXml,
                RequestMethod = Constants.HttpRequestMethods.Post,
                Url = url,
                PostData = SerializeAndDeserialize.SerializeToXmlWithoutRoot<OzteknoMessageRequest>(messageRequest)
            };
            var response = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
            if (!response.Contains("OK 1 Islem No:"))
                throw new ArgumentNullException(response);
        }

        private static void SendSmsToNeximo(ApiSMSModel apiSMSModel, ILog log)
        {
            string sender = CacheManager.GetNotificationServiceValueByKey(apiSMSModel.PartnerId, 
                Constants.PartnerKeys.SmsSender, (int)NotificationServices.Nexmo);
            string apiKey = CacheManager.GetNotificationServiceValueByKey(apiSMSModel.PartnerId, 
                Constants.PartnerKeys.SmsGwLogin, (int)NotificationServices.Nexmo);
            string apiSecret = CacheManager.GetNotificationServiceValueByKey(apiSMSModel.PartnerId, 
                Constants.PartnerKeys.SmsGwPass, (int)NotificationServices.Nexmo);

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var messageRequest = new
            {
                from = sender,
                to = apiSMSModel.Recipient.Replace("+", string.Empty),
                text = apiSMSModel.MessegeText,
                api_key = apiKey,
                api_secret = apiSecret
            };

            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Post,
                Url = "https://rest.nexmo.com/sms/json",
                PostData = JsonConvert.SerializeObject(messageRequest)
            };
            var resp = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
            log.Info(resp);
        }
        public static void SendSmsToSMSCentre(ApiSMSModel apiSMSModel)
        {
            string login = CacheManager.GetNotificationServiceValueByKey(apiSMSModel.PartnerId, Constants.PartnerKeys.SmsGwLogin, (int)NotificationServices.SMSCentre);
            string password = CacheManager.GetNotificationServiceValueByKey(apiSMSModel.PartnerId, Constants.PartnerKeys.SmsGwPass, (int)NotificationServices.SMSCentre);
            string url = CacheManager.GetNotificationServiceValueByKey(apiSMSModel.PartnerId, Constants.PartnerKeys.SmsGwUrl, (int)NotificationServices.SMSCentre);

            var smsRequestInput = new
            {
                login,
                psw = password,
                phones = "+" + apiSMSModel.Recipient.Replace(" ", string.Empty).Replace("+", string.Empty),
                mes = apiSMSModel.MessegeText
            };

            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                RequestMethod = Constants.HttpRequestMethods.Get,
                Url = string.Format("{0}?{1}", url, CommonFunctions.GetUriDataFromObject(smsRequestInput))
            };
            CommonFunctions.SendHttpRequest(httpRequestInput, out _);
        }
        public static void SendSmsToSMSTwilio(ApiSMSModel apiSMSModel)
        {
            var sid = CacheManager.GetNotificationServiceValueByKey(apiSMSModel.PartnerId, Constants.PartnerKeys.SmsGwLogin, (int)NotificationServices.Twilio);
            var token = CacheManager.GetNotificationServiceValueByKey(apiSMSModel.PartnerId, Constants.PartnerKeys.SmsGwPass, (int)NotificationServices.Twilio);
            var sender = CacheManager.GetNotificationServiceValueByKey(apiSMSModel.PartnerId, Constants.PartnerKeys.SmsSender, (int)NotificationServices.Twilio);
            var url = CacheManager.GetNotificationServiceValueByKey(apiSMSModel.PartnerId, Constants.PartnerKeys.SmsGwUrl, (int)NotificationServices.Twilio);
            var smsRequestInput = new
            {
                Body = apiSMSModel.MessegeText,
                From = sender,
                To = "+" + apiSMSModel.Recipient.Replace(" ", string.Empty).Replace("+", string.Empty)
            };

            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                RequestMethod = Constants.HttpRequestMethods.Post,
                Url = string.Format(url, sid),
                RequestHeaders = new Dictionary<string, string> { { "Authorization", "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(string.Format("{0}:{1}", sid, token))) } },
                PostData = CommonFunctions.GetUriDataFromObject(smsRequestInput)
            };
            CommonFunctions.SendHttpRequest(httpRequestInput, out _);
        }
        public static void SendSmsToSalesforce(ApiSMSModel apiSMSModel)
        {
            var apiClientSecret = CacheManager.GetNotificationServiceValueByKey(apiSMSModel.PartnerId, Constants.PartnerKeys.SmsGwPass, (int)NotificationServices.Salesforce);
            var apiClientId = CacheManager.GetNotificationServiceValueByKey(apiSMSModel.PartnerId, Constants.PartnerKeys.SmsSender, (int)NotificationServices.Salesforce);
            var apiKeys = CacheManager.GetNotificationServiceValueByKey(apiSMSModel.PartnerId, Constants.PartnerKeys.SmsGwLogin, (int)NotificationServices.Salesforce);
            var apiKeyword = apiKeys.Split(',')[0];
            var apiId = apiKeys.Split(',')[1];
            var url = CacheManager.GetNotificationServiceValueByKey(apiSMSModel.PartnerId, Constants.PartnerKeys.SmsGwUrl, (int)NotificationServices.Salesforce);
            var tokenInput = new
            {
                grant_type = "client_credentials",
                client_id = apiClientId,
                client_secret = apiClientSecret
            };
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Post,
                Url = string.Format("{0}/token", url),
                PostData = JsonConvert.SerializeObject(tokenInput)
            };
            var resp = JsonConvert.DeserializeObject<SalesforceTokenOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
            var subscribers = new
            {
                MobileNumber = apiSMSModel.Recipient.Replace(" ", string.Empty).Replace("+", string.Empty),
                SubscriberKey = apiSMSModel.MessageId,
            };
            var smsRequestInput = new
            {
                Subscribers = new List<object> { subscribers },
                Override = true,
                Subscribe = true,
                Resubscribe = true,
                keyword = apiKeyword,
                messageText = apiSMSModel.MessegeText
            };
            httpRequestInput.Url = string.Format("{0}/sms/v1/messageContact/{1}/send", resp.RestInstanceUrl, apiId);
            httpRequestInput.RequestHeaders = new Dictionary<string, string> { { "Authorization", "Bearer " + resp.AccessToken } };
            httpRequestInput.PostData = JsonConvert.SerializeObject(smsRequestInput);
            CommonFunctions.SendHttpRequest(httpRequestInput, out _);
        }

        public static void SendSmsToSMSTo(ApiSMSModel apiSMSModel, ILog log)
        {
            var partner = CacheManager.GetPartnerById(apiSMSModel.PartnerId);
            var apiKey = CacheManager.GetNotificationServiceValueByKey(apiSMSModel.PartnerId, Constants.PartnerKeys.SMSToApiKey, (int)NotificationServices.SMSTo);
            var url = CacheManager.GetNotificationServiceValueByKey(apiSMSModel.PartnerId, Constants.PartnerKeys.SMSToApiUrl, (int)NotificationServices.SMSTo);
            var smsRequestInput = new
            {
                message = apiSMSModel.MessegeText,
                to = "+" + apiSMSModel.Recipient.Replace(" ", string.Empty).Replace("+", string.Empty),
                sender_id = partner.Name
            };
            log.Info(JsonConvert.SerializeObject(smsRequestInput) + "_" + apiKey + "_" + url);
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Post,
                Url = url,
                RequestHeaders = new Dictionary<string, string> { { "Authorization", "Bearer " + apiKey } },
                PostData = JsonConvert.SerializeObject(smsRequestInput)
            };
            var output = JsonConvert.DeserializeObject<Common.Models.Notification.SMSToOutput>(CommonFunctions.SendHttpRequest(httpRequestInput, out _));
            if (!output.Success)
                throw new Exception(output.Message);
        }


		private static void SendSmsToGeezSms(ApiSMSModel apiSMSModel, ILog log)
		{
			string token = CacheManager.GetNotificationServiceValueByKey(apiSMSModel.PartnerId, Constants.PartnerKeys.GeezSmsToken, (int)NotificationServices.GeezSms);
			string url = CacheManager.GetNotificationServiceValueByKey(apiSMSModel.PartnerId, Constants.PartnerKeys.GeezSmsUrl, (int)NotificationServices.GeezSms);

			var input = new 
			{
				token,
				phone = apiSMSModel.Recipient.Replace("+", string.Empty),
				msg = apiSMSModel.MessegeText
			};

			var httpRequestInput = new HttpRequestInput
			{
				ContentType = Constants.HttpContentTypes.ApplicationJson,
				RequestMethod = Constants.HttpRequestMethods.Post,
                PostData = JsonConvert.SerializeObject(input),
				Url = url
            };
            var resp = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
			log.Info(resp);
		}

    }
}
