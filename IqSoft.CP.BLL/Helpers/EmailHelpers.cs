using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.Notification;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace IqSoft.CP.BLL.Helpers
{
    public static class EmailHelpers
    {
        public static void SendEmail(int notificationServiceId, ApiEmailModel apiEmailModel, ILog log)
        {

            switch (notificationServiceId)
            {
                case (int)EmailNotificationServices.Smtp:
                    SendSmtpEmail(apiEmailModel);
                    break;
                case (int)EmailNotificationServices.OneSender:
                    SendOnesender(apiEmailModel);
                    break;
                case (int)EmailNotificationServices.UniSender:
                    SendUnisender(apiEmailModel, log);
                    break;
                case (int)EmailNotificationServices.Customer:
                    SendCustomer(apiEmailModel);
                    break;
                case (int)EmailNotificationServices.Mailgun:
                    SendMailgun(apiEmailModel);
                    break;
                default:
                    throw BaseBll.CreateException(null, Constants.Errors.MethodNotFound);
            }
        }

        private static void SendOnesender(ApiEmailModel apiEmailModel)
        {
            var partner = CacheManager.GetPartnerById(apiEmailModel.ParnerId);
            var url = CacheManager.GetNotificationServiceValueByKey(apiEmailModel.ParnerId, Constants.PartnerKeys.UniSenderApiUrl, (int)EmailNotificationServices.OneSender);
            var oneSenderInput = new OneSenderInput
            {
                ApiKey = CacheManager.GetNotificationServiceValueByKey(apiEmailModel.ParnerId, Constants.PartnerKeys.UniSenderApi, (int)EmailNotificationServices.OneSender),
                UserName = apiEmailModel.FromEmail,
                MessageBody = new Message
                {
                    Subject = apiEmailModel.Subject,
                    SenderEmail = apiEmailModel.FromEmail,
                    SenderName = partner.Name,
                    Recipients = new Recipient[] { new Recipient { Email = apiEmailModel.ToEmail } }
                }
            };
            if (string.IsNullOrEmpty(apiEmailModel.ExternalTemplateId))
                oneSenderInput.MessageBody.BodyObject = new Body
                {
                    Html = apiEmailModel.Body
                };
            else
                oneSenderInput.MessageBody.TemplateId = apiEmailModel.ExternalTemplateId;

            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Post,
                Url = url,
                PostData = JsonConvert.SerializeObject(oneSenderInput)
            };
            CommonFunctions.SendHttpRequest(httpRequestInput, out _);
        }

        private static void SendUnisender(ApiEmailModel apiEmailModel, ILog log)
        {
            var partner = CacheManager.GetPartnerById(apiEmailModel.ParnerId);
            var campaignId = CacheManager.GetNotificationServiceValueByKey(apiEmailModel.ParnerId, Constants.PartnerKeys.UniSenderApiCampaignId, (int)EmailNotificationServices.UniSender);
            var url = CacheManager.GetNotificationServiceValueByKey(apiEmailModel.ParnerId, Constants.PartnerKeys.UniSenderApiUrl, (int)EmailNotificationServices.UniSender);

            var uniSenderInput = new UniSenderInput
            {
                format = "json",
                api_key = CacheManager.GetNotificationServiceValueByKey(apiEmailModel.ParnerId, Constants.PartnerKeys.UniSenderApi, (int)EmailNotificationServices.UniSender),
                sender_name = partner.Name,
                sender_email = apiEmailModel.FromEmail,
                email = apiEmailModel.ToEmail,
                body = apiEmailModel.Body,
                subject = apiEmailModel.Subject,
                list_id = campaignId
            };
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                RequestMethod = Constants.HttpRequestMethods.Post,
                Url = url,
                PostData = CommonFunctions.GetUriEndocingFromObject(uniSenderInput)
            };
            if (!string.IsNullOrEmpty(apiEmailModel.AttachedFileName))
                httpRequestInput.PostData += string.Format("&attachments[{0}]={1}", apiEmailModel.AttachedFileName, apiEmailModel.AttachedContent);
            var resp = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
        }

        private static void SendSmtpEmail(ApiEmailModel apiEmailModel)
        {
            using (var mail = new MailMessage())
            {
                mail.From = new MailAddress(apiEmailModel.FromEmail);
                mail.To.Add(apiEmailModel.ToEmail);
                mail.Subject = apiEmailModel.ToEmail;
                mail.Body = apiEmailModel.Body;
                mail.IsBodyHtml = true;
                var smtpServer = CacheManager.GetNotificationServiceValueByKey(apiEmailModel.ParnerId, Constants.PartnerKeys.SmtpServer, (int)EmailNotificationServices.Smtp);
                var smtpPort = CacheManager.GetNotificationServiceValueByKey(apiEmailModel.ParnerId, Constants.PartnerKeys.SmtpPort, (int)EmailNotificationServices.Smtp);
                var smtpPwd = CacheManager.GetNotificationServiceValueByKey(apiEmailModel.ParnerId, Constants.PartnerKeys.NotificationMailPass, (int)EmailNotificationServices.Smtp);
                if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(smtpPort) ||
                    string.IsNullOrEmpty(smtpPwd) || !int.TryParse(smtpPort, out int port))
                    throw BaseBll.CreateException(null, Constants.Errors.PartnerKeyNotFound);

                using (var smtp = new SmtpClient(smtpServer, port))
                {
                    smtp.Credentials = new NetworkCredential(apiEmailModel.FromEmail, smtpPwd);
                    smtp.EnableSsl = true;
                    if (!string.IsNullOrEmpty(apiEmailModel.AttachedFileName))
                    {
                        var byteArray = Encoding.UTF8.GetBytes(apiEmailModel.AttachedContent);
                        var memoryStream = new MemoryStream(byteArray);
                        mail.Attachments.Add(new Attachment(memoryStream, apiEmailModel.AttachedFileName));
                    }
                    smtp.Send(mail);
                }
            }
        }

        private static void SendCustomer(ApiEmailModel apiEmailModel)
        {
            var partner = CacheManager.GetPartnerById(apiEmailModel.ParnerId);
            var apiKey = CacheManager.GetNotificationServiceValueByKey(apiEmailModel.ParnerId, Constants.PartnerKeys.CustomerApiKey, (int)EmailNotificationServices.Customer);
            var url = CacheManager.GetNotificationServiceValueByKey(apiEmailModel.ParnerId, Constants.PartnerKeys.CustomerApiUrl, (int)EmailNotificationServices.Customer);

            var input = new CustomerInput
            {
                ToEmail = apiEmailModel.ToEmail,
                FromEmail = string.Format("{0} <{1}>", partner.Name, apiEmailModel.FromEmail),
                Subject = apiEmailModel.Subject,
                Body = apiEmailModel.Body,
                Identifiers = new Dictionary<string, string> { { "id", apiEmailModel.ClientId } }
            };
            if (!string.IsNullOrEmpty(apiEmailModel.AttachedFileName))
                input.Attachments =new Dictionary<string, string> { { apiEmailModel.AttachedFileName, apiEmailModel.AttachedContent } };
            var headers = new Dictionary<string, string> { { "Authorization", string.Format("Bearer {0}", apiKey) } };
            var httpRequestInput = new HttpRequestInput
            {
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                RequestMethod = Constants.HttpRequestMethods.Post,
                RequestHeaders = headers,
                Url = url,
                PostData = JsonConvert.SerializeObject(input)
            };
            CommonFunctions.SendHttpRequest(httpRequestInput, out _);
        }

		private static void SendMailgun(ApiEmailModel apiEmailModel)
		{
            var url = CacheManager.GetNotificationServiceValueByKey(apiEmailModel.ParnerId, Constants.PartnerKeys.MailgunUrl, (int)EmailNotificationServices.Mailgun);
			var domain = CacheManager.GetNotificationServiceValueByKey(apiEmailModel.ParnerId, Constants.PartnerKeys.MailgunDomain, (int)EmailNotificationServices.Mailgun); 
			var userName = CacheManager.GetNotificationServiceValueByKey(apiEmailModel.ParnerId, Constants.PartnerKeys.MailgunUsername, (int)EmailNotificationServices.Mailgun);
			var password = CacheManager.GetNotificationServiceValueByKey(apiEmailModel.ParnerId, Constants.PartnerKeys.MailgunPassword, (int)EmailNotificationServices.Mailgun);

			var postData = new 
			{
				domain = domain,
				from = apiEmailModel.FromEmail,
				to = apiEmailModel.ToEmail,
				subject = apiEmailModel.Subject,
				text = apiEmailModel.Body
			};
            var byteArray = Encoding.Default.GetBytes($"{userName}:{password}");
			var headers = new Dictionary<string, string>
						{
							{ "Authorization", "Basic " + Convert.ToBase64String(byteArray) }
						};
			var httpRequestInput = new HttpRequestInput
			{
				ContentType = Constants.HttpContentTypes.ApplicationUrlEncoded,
                RequestHeaders = headers,
				RequestMethod = Constants.HttpRequestMethods.Post,
				Url = $"{url}{domain}/messages",
				PostData = CommonFunctions.GetUriDataFromObject(postData)
			};
			CommonFunctions.SendHttpRequest(httpRequestInput, out _);
		}
	}
}