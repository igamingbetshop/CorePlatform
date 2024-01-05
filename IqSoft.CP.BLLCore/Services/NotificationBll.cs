using System;
using System.Linq;
using Newtonsoft.Json;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Models;
using log4net;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.Common.Models.Notification;
using IqSoft.CP.BLL.Interfaces;
using System.ServiceModel;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.BLL.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Collections.Generic;
using IqSoft.CP.DAL.Models.Notification;
using System.Text;

namespace IqSoft.CP.BLL.Services
{
    public class NotificationBll : BaseBll, INotificationBll
    {
        #region Constructors

        public NotificationBll(SessionIdentity identity, ILog log)
            : base(identity, log)
        {

        }

        public NotificationBll(BaseBll baseBl)
            : base(baseBl)
        {

        }

        #endregion

        public int RegisterActiveEmail(int partnerId, string email, string subject, string body, int? templateId)
        {

            var dbEmail = new Email
            {
                PartnerId = partnerId,
                Receiver = email,
                Status = (int)EmailStates.Active,
                Subject = subject,
                Body = body,
                MessageTemplateId = templateId,
                CreationTime = GetServerDate()
            };
            Db.Emails.Add(dbEmail);
            Db.SaveChanges();
            return dbEmail.Id;
        }

        public void UpdateEmailStatus(int emailId, int status)
        {
            Db.Emails.Where(d => d.Id == emailId).UpdateFromQuery(e => new Email { Status = status });
        }

        public bool SendEmail(string clientId, int partnerId, string email, string subject, string body,
            string externalTemplateId, string fileName = "", string fileContent = "")
        {
            try
            {
                var notificationService = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.EmailNotificationService);
                if (notificationService != null && notificationService.Id > 0 && notificationService.NumericValue.HasValue)
                {
                    var notificationServieId = (int)notificationService.NumericValue.Value;
                    var fromEmail = CacheManager.GetNotificationServiceValueByKey(partnerId, Constants.PartnerKeys.NotificationMail, notificationServieId);

                    var apiEmailModel = new ApiEmailModel
                    {
                        ClientId = clientId,
                        ParnerId = partnerId,
                        FromEmail = fromEmail,
                        ToEmail = email,
                        Subject = subject,
                        Body = body,
                        ExternalTemplateId = externalTemplateId,
                        AttachedFileName = fileName,
                        AttachedContent= fileContent
                    };
                    EmailHelpers.SendEmail(notificationServieId, apiEmailModel);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return false;
            }
        }

        public bool SendSms(int partnerId, string mobileNumber, string messegeText, long messageId)
        {
            try
            {
                var notificationService = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.NotificationService);
                if (notificationService != null && notificationService.Id > 0 && notificationService.NumericValue.HasValue)
                {
                    var notificationServieId = (int)notificationService.NumericValue.Value;
                    var apiSMSModel = new ApiSMSModel
                    {
                        PartnerId = partnerId,
                        Recipient = mobileNumber,
                        MessegeText = messegeText,
                        MessageId = messageId
                    };
                    SMSHelpers.SendSMS(notificationServieId, apiSMSModel);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return false;
            }
        }

        public ClientMessage SaveEmailMessage(int partnerId, int clientId, string receiver, string subject, string body, int? templateId)
        {
            if (string.IsNullOrEmpty(receiver))
                throw BaseBll.CreateException(string.Empty, Constants.Errors.EmailCantBeEmpty);
            if (!IsValidEmail(receiver))
                throw BaseBll.CreateException(string.Empty, Constants.Errors.InvalidEmail);

            var currentTime = DateTime.UtcNow;
            var clientMessage = new ClientMessage
            {
                PartnerId = partnerId,
                ClientId = clientId,
                Message = body,
                Subject = subject,
                Type = (int)ClientMessageTypes.Email,
                Email = new Email
                {
                    PartnerId = partnerId,
                    Receiver = receiver,
                    Status = (int)EmailStates.Active,
                    Subject = subject,
                    Body = body,
                    MessageTemplateId = templateId,
                    CreationTime = currentTime
                },
                CreationTime = currentTime
            };
            Db.ClientMessages.Add(clientMessage);
            Db.SaveChanges();
            return clientMessage;
        }

        public void SendInternalTicket(int clientId, int? notificationType, string messageText = "", DAL.Models.Notification.PaymentNotificationInfo paymentInfo = null)
        {
            var client = CacheManager.GetClientById(clientId);
            if (client == null)
                throw CreateException(LanguageId, Constants.Errors.WrongClientId);

            if (notificationType != null)
                messageText = CacheManager.GetPartnerMessageTemplate(client.PartnerId, (int)notificationType.Value, LanguageId)?.Text;

            if (string.IsNullOrEmpty(messageText))
                return;
            var composite = new Composite
            {
                Domain = Identity.Domain,
                ClientId = client.Id,
                UserName = client.UserName,
                FirstName = client.FirstName,
                Email = client.Email,
                MobileNumber = client.MobileNumber,
                Currency = client.CurrencyId,
                PaymentRequestState = paymentInfo?.State,
                Amount = paymentInfo?.Amount
            };
            messageText = MapTemplateData(messageText, composite);
            var ticketSubject = CacheManager.GetPartnerMessageTemplate(client.PartnerId, (int)ClientInfoTypes.NotificationTicketSubject, LanguageId)?.Text;
            var currentTime = DateTime.UtcNow;
            var currentDate = (long)currentTime.Year * 100000000 + currentTime.Month * 1000000 + currentTime.Day * 10000 + currentTime.Hour * 100 + currentTime.Minute;
            var message = new TicketMessage
            {
                Message = messageText,
                Type = (int)ClientMessageTypes.MessageFromSystem,
                CreationDate = currentDate,
                CreationTime = currentTime,
                Ticket = new Ticket
                {
                    ClientId = clientId,
                    PartnerId = client.PartnerId,
                    Type = (int)TicketTypes.Notification,
                    Subject = ticketSubject ?? "Notification",
                    Status = (int)MessageTicketState.Closed,
                    ClientUnreadMessagesCount = 1,
                    CreationTime = currentTime,
                    LastMessageTime = currentTime,
                    LastMessageDate = currentDate
                }
            };
            Db.TicketMessages.Add(message);
            Db.SaveChanges();
            CacheManager.UpdateClientUnreadTicketsCount(clientId, CacheManager.GetClientUnreadTicketsCount(clientId).Count + 1);
        }

        public int SendVerificationCodeToEmail(int clientId, string email)
        {
            var client = Db.Clients.FirstOrDefault(x => x.Id == clientId);
            if (client == null)
                throw CreateException(LanguageId, Constants.Errors.WrongClientId);
            var changed = client.Email == email;
            if (changed)
            {
                if (string.IsNullOrWhiteSpace(email))
                    throw CreateException(LanguageId, Constants.Errors.EmailCantBeEmpty);
                if (!IsValidEmail(email))
                    throw CreateException(LanguageId, Constants.Errors.InvalidEmail);
            }
            var partner = CacheManager.GetPartnerById(client.PartnerId);
            var partnerSetting = CacheManager.GetConfigKey(client.PartnerId, Constants.PartnerKeys.VerificationKeyNumberOnly);
            var verificationKey = (!string.IsNullOrEmpty(partnerSetting) && partnerSetting == "1") ? CommonFunctions.GetRandomNumber(partner.EmailVerificationCodeLength) :
                                                                                                     CommonFunctions.GetRandomString(partner.EmailVerificationCodeLength);
            var notificationModel = new NotificationModel
            {
                PartnerId = client.PartnerId,
                ClientId = client.Id,
                MobileOrEmail = changed ? email : client.Email,
                ClientInfoType = (int)ClientInfoTypes.EmailVerificationKey,
                VerificationCode = verificationKey
            };
            var activePeriodInMinutes = SendNotificationMessage(notificationModel);
            if (changed)
            {
                client.Email = email;
                client.LastUpdateTime = GetServerDate();
                Db.SaveChanges();
                CacheManager.RemoveClientFromCache(client.Id);
            }
            return activePeriodInMinutes;
        }

        public int SendVerificationCodeToMobileNumber(int clientId, string mobileNumber)
        {
            var client = Db.Clients.FirstOrDefault(x => x.Id == clientId);
            if (client == null)
                throw CreateException(LanguageId, Constants.Errors.WrongClientId);
            bool changed = client.MobileNumber == mobileNumber;
            if (!IsMobileNumber(mobileNumber))
                throw CreateException(LanguageId, Constants.Errors.MobileNumberCantBeEmpty);
            var partner = CacheManager.GetPartnerById(client.PartnerId);
            var partnerSetting = CacheManager.GetConfigKey(client.PartnerId, Constants.PartnerKeys.VerificationKeyNumberOnly);
            var verificationKey = (!string.IsNullOrEmpty(partnerSetting) && partnerSetting == "1") ? CommonFunctions.GetRandomNumber(partner.MobileVerificationCodeLength) :
                                                                                                     CommonFunctions.GetRandomString(partner.MobileVerificationCodeLength);
            var notificationModel = new NotificationModel
            {
                PartnerId = client.PartnerId,
                ClientId = client.Id,
                MobileOrEmail = changed ? mobileNumber : client.MobileNumber,
                ClientInfoType = (int)ClientInfoTypes.MobileVerificationKey,
                VerificationCode = verificationKey
            };
            var activePeriodInMinutes = SendNotificationMessage(notificationModel);

            if (changed)
            {
                client.MobileNumber = mobileNumber;
                client.LastUpdateTime = DateTime.UtcNow;
                Db.SaveChanges();
                CacheManager.RemoveClientFromCache(client.Id);
            }
            return activePeriodInMinutes;
        }

        public void SendInvitationToAffiliateClient(int clientId, string toEmail)
        {
            var client = CacheManager.GetClientById(clientId);
            var partner = CacheManager.GetPartnerById(client.PartnerId);

            if (!IsValidEmail(toEmail))
                throw CreateException(LanguageId, Constants.Errors.InvalidEmail);
            var dbClient = Db.Clients.FirstOrDefault(x => x.PartnerId == client.PartnerId && x.Email == toEmail);
            if (dbClient != null)
                throw CreateException(LanguageId, Constants.Errors.EmailExists);
            var messageTemplate = CacheManager.GetPartnerMessageTemplate(client.PartnerId, (int)ClientInfoTypes.AffiliateClientInvitationEmail, LanguageId);
            var messageTextTemplate = messageTemplate.Text;
            var msgText = messageTextTemplate.Replace("\\n", Environment.NewLine)
                                             .Replace("{u}", client.UserName)
                                             .Replace("{w}", Identity.Domain)
                                             .Replace("{pc}", clientId.ToString())
                                             .Replace("{fn}", client.FirstName);
            SaveEmailMessage(client.PartnerId, client.Id, string.IsNullOrEmpty(toEmail) ? client.Email : toEmail, partner.Name, msgText, messageTemplate.Id);
        }

        public int SendNotificationMessage(NotificationModel notificationModel)
        {
            try
            {

                var messageTextTemplate = string.Empty;
                int? messageTemplateId = null;
                var languageId = string.IsNullOrEmpty(notificationModel.LanguageId) ? Identity.LanguageId : notificationModel.LanguageId;
                if (notificationModel.ClientInfoType != null)
                {
                    if (!Enum.IsDefined(typeof(ClientInfoTypes), notificationModel.ClientInfoType.Value))
                        throw CreateException(Identity.LanguageId, Constants.Errors.WrongInputParameters);
                    var messageTemplate = CacheManager.GetPartnerMessageTemplate(notificationModel.PartnerId, notificationModel.ClientInfoType.Value, languageId);
                    messageTextTemplate = messageTemplate?.Text;
                    if (string.IsNullOrEmpty(messageTextTemplate))
                        throw CreateException(null, Constants.Errors.CommentTemplateNotFound);
                    messageTemplateId = messageTemplate.Id;
                }
                else
                    messageTextTemplate = notificationModel.MessageText;
                var partner = CacheManager.GetPartnerById(notificationModel.PartnerId);
                var verificationCodeActivePeriod = partner.VerificationKeyActiveMinutes;
                var composite = new Composite
                {
                    VerificationCode = notificationModel.VerificationCode,
                    Domain = Identity.Domain,
                    Parameters = notificationModel.Parameters
                };
                if (notificationModel.ClientId.HasValue && notificationModel.ClientId.Value > 0)
                {
                    CheckLimit(notificationModel.ClientInfoType, notificationModel.PartnerId, notificationModel.ClientId.Value);
                    var client = CacheManager.GetClientById(notificationModel.ClientId.Value);
                    composite.ClientId = client.Id;
                    composite.UserName = client.UserName;
                    composite.FirstName = client.FirstName;
                    composite.Email = client.Email;
                    composite.MobileNumber = client.MobileNumber;
                    composite.Currency = client.CurrencyId;
                    composite.PaymentRequestState = notificationModel.PaymentInfo?.State;
                    composite.Amount = notificationModel.PaymentInfo?.Amount;
                    composite.Reason = notificationModel.PaymentInfo?.Reason;
                    composite.BankName = notificationModel.PaymentInfo?.BankName;
                    composite.BankBranchName = notificationModel.PaymentInfo?.BankBranchName;
                    composite.BankCode = notificationModel.PaymentInfo?.BankCode;
                    composite.BankAccountNumber = notificationModel.PaymentInfo?.BankAccountNumber;
                    composite.BankAccountHolder = notificationModel.PaymentInfo?.BankAccountHolder;
                }
                messageTextTemplate = MapTemplateData(messageTextTemplate, composite);
                var currentDate = DateTime.UtcNow;
                if (notificationModel.ClientInfoType != null)
                    notificationModel.MessageType = (int)MapToMessageType(notificationModel.ClientInfoType.Value);
                var message = new ClientMessage
                {
                    PartnerId = notificationModel.PartnerId,
                    ClientId = notificationModel.ClientId == 0 ? (int?)null : notificationModel.ClientId,
                    Subject = partner.Name,
                    MobileOrEmail = notificationModel.MobileOrEmail,
                    Type = notificationModel.MessageType,
                    Message = notificationModel.MessageType == (int)ClientMessageTypes.SecuredSms || notificationModel.MessageType == (int)ClientMessageTypes.SecuredEmail ? "Secured" : messageTextTemplate,
                    CreationTime = currentDate
                };
                Db.ClientMessages.Add(message);
                if (!string.IsNullOrEmpty(notificationModel.VerificationCode) && notificationModel.ClientInfoType != (int)ClientInfoTypes.NewIpLoginEmail)
                {
                    var activeClientInfoesCount = CheckActiveClientInfoes(notificationModel.PartnerId, notificationModel.MobileOrEmail);
                    if (activeClientInfoesCount > 0)
                        verificationCodeActivePeriod /= 2;
                    Db.ClientInfoes.Where(x => (x.State==(int)ClientInfoStates.Active || x.State==(int)ClientInfoStates.Verified) &&
                                                x.MobileOrEmail == notificationModel.MobileOrEmail &&  x.PartnerId == notificationModel.PartnerId)
                                   .UpdateFromQuery(x => new ClientInfo { State  = (int)ClientInfoStates.Expired });
                    Db.ClientInfoes.Add(new ClientInfo
                    {
                        PartnerId = notificationModel.PartnerId,
                        ClientId = notificationModel.ClientId == 0 ? null : notificationModel.ClientId,
                        CreationTime = activeClientInfoesCount > 0 ? currentDate.AddMinutes(-verificationCodeActivePeriod) : currentDate,
                        Data = notificationModel.VerificationCode,
                        Type = notificationModel.ClientInfoType.Value,
                        State = (int)ClientInfoStates.Active,
                        MobileOrEmail = notificationModel.MobileOrEmail,
                        Ip = Identity.LoginIp
                    });
                }
                Db.SaveChanges();
                if (notificationModel.MessageType == (int)ClientMessageTypes.Sms)
                {
                    SendSms(notificationModel.PartnerId, notificationModel.MobileOrEmail, messageTextTemplate, message.Id);
                    return verificationCodeActivePeriod;
                }
                var emailSubject = notificationModel.SubjectType.HasValue ? CacheManager.GetPartnerMessageTemplate(notificationModel.PartnerId, notificationModel.SubjectType.Value, languageId)?.Text : partner.Name;
                // change subject for below types
                if (notificationModel.ClientInfoType == (int)ClientInfoTypes.PasswordRecoveryEmailKey || notificationModel.ClientInfoType == (int)ClientInfoTypes.EmailVerificationKey)
                {
                    var subjectKey = notificationModel.ClientInfoType == (int)ClientInfoTypes.PasswordRecoveryEmailKey ? (int)ClientInfoTypes.PasswordRecoveryEmailSubject : (int)ClientInfoTypes.EmailVerificationSubject;
                    var sub = CacheManager.GetPartnerMessageTemplate(notificationModel.PartnerId, subjectKey, languageId);
                    if (sub != null && !string.IsNullOrEmpty(sub.Text))
                        emailSubject = sub.Text;
                }
                if (string.IsNullOrEmpty(emailSubject))
                    emailSubject = partner.Name;
                var emailId = RegisterActiveEmail(notificationModel.PartnerId, notificationModel.MobileOrEmail, emailSubject, messageTextTemplate, messageTemplateId);
                message.EmailId = emailId;
                Db.SaveChanges();
                return verificationCodeActivePeriod;
            }
            catch (FaultException<BllFnErrorType> e)
            {
                Log.Error(JsonConvert.SerializeObject(e.Detail));
                throw;
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            return 0;
        }

        public void SendDepositNotification(int clientId, int requestState, decimal amount, string reason)
        {
            try
            {
                var client = CacheManager.GetClientById(clientId);
                if (client == null)
                    throw CreateException(LanguageId, Constants.Errors.ClientNotFound);
                var paymentInfo = new PaymentNotificationInfo
                {
                    State = requestState,
                    Amount = amount,
                    Reason = reason
                };
                var emailType = (int)ClientInfoTypes.DepositEmail;
                var ticketType = (int)ClientInfoTypes.DepositTicket;
                if (requestState != (int)PaymentRequestStates.Approved && requestState != (int)PaymentRequestStates.ApprovedManually)
                {
                    emailType = (int)ClientInfoTypes.FailedDepositEmail;
                    ticketType = (int)ClientInfoTypes.FailedDepositTicket;
                }
                var partnerConfig = CacheManager.GetConfigKey(client.PartnerId, Constants.PartnerKeys.SendDepositEmailNotification);
                if (!string.IsNullOrEmpty(partnerConfig) && partnerConfig == "1")
                {
                    var notificationModel = new NotificationModel
                    {
                        PartnerId = client.PartnerId,
                        ClientId = client.Id,
                        MobileOrEmail = client.Email,
                        ClientInfoType = emailType,
                        PaymentInfo = paymentInfo
                    };
                    SendNotificationMessage(notificationModel);
                }
                SendInternalTicket(client.Id, ticketType, paymentInfo: paymentInfo);
            }
            catch (FaultException<BllFnErrorType> e)
            {
                Log.Error(JsonConvert.SerializeObject(e.Detail));
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public void SendWitdrawNotification(int clientId, int requestState, decimal amount, string reason)
        {
            try
            {
                var client = CacheManager.GetClientById(clientId);
                if (client == null)
                    throw CreateException(LanguageId, Constants.Errors.ClientNotFound);
                var partnerConfig = CacheManager.GetConfigKey(client.PartnerId, Constants.PartnerKeys.SendWithdrawEmailNotification);
                var paymentInfo = new PaymentNotificationInfo
                {
                    State = requestState,
                    Amount = amount,
                    Reason = reason
                };
                var emailType = (int)ClientInfoTypes.PendingWithdrawEmail;
                var ticketType = (int)ClientInfoTypes.PendingWithdrawTicket;
                switch (requestState)
                {
                    case (int)PaymentRequestStates.Approved:
                    case (int)PaymentRequestStates.ApprovedManually:
                        emailType = (int)ClientInfoTypes.ApproveWithdrawEmail;
                        ticketType = (int)ClientInfoTypes.ApproveWithdrawTicket;
                        break;
                    case (int)PaymentRequestStates.Failed:
                    case (int)PaymentRequestStates.CanceledByUser:
                        emailType = (int)ClientInfoTypes.RejectWithdrawEmail;
                        ticketType = (int)ClientInfoTypes.RejectWithdrawTicket;
                        break;
                    case (int)PaymentRequestStates.Confirmed:
                        emailType = (int)ClientInfoTypes.ConfirmWithdrawEmail;
                        ticketType = (int)ClientInfoTypes.ConfirmWithdrawTicket;
                        break;
                    case (int)PaymentRequestStates.WaitingForKYC:
                        emailType = (int)ClientInfoTypes.WaitingForKYCWithdrawEmail;
                        ticketType = (int)ClientInfoTypes.WaitingForKYCWithdrawTicket;
                        break;
                    case (int)PaymentRequestStates.Frozen:
                        emailType = (int)ClientInfoTypes.FrozenWithdrawEmail;
                        ticketType = (int)ClientInfoTypes.FrozenWithdrawTicket;
                        break;
                    case (int)PaymentRequestStates.InProcess:
                        emailType = (int)ClientInfoTypes.InProcessWithdrawEmail;
                        ticketType = (int)ClientInfoTypes.InProcessWithdrawTicket;
                        break;
                    default: break;
                }
                if (!string.IsNullOrEmpty(partnerConfig) && partnerConfig == "1")
                {
                    var notificationModel = new NotificationModel
                    {
                        PartnerId = client.PartnerId,
                        ClientId = client.Id,
                        MobileOrEmail = client.Email,
                        ClientInfoType = emailType,
                        PaymentInfo = paymentInfo
                    };
                    SendNotificationMessage(notificationModel);
                }
                SendInternalTicket(client.Id, ticketType, paymentInfo: paymentInfo);
            }
            catch (FaultException<BllFnErrorType> e)
            {
                Log.Error(JsonConvert.SerializeObject(e.Detail));
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public void RegistrationNotification(int partnerId, int clientId, AffiliatePlatform affiliatePlatform, string clieckId, string affiliateId)
        {
            NotifyAffiliator(partnerId, clientId, affiliatePlatform, clieckId, string.Empty, null, null, false);
        }

        public void DepositAffiliateNotification(BllClient client, decimal depositAmount, long transactionId, int depositCount)
        {
            var ar = Db.AffiliateReferrals.Include(x => x.AffiliatePlatform).FirstOrDefault(x => x.Id == client.AffiliateReferralId);
            if (ar != null)
                NotifyAffiliator(client.PartnerId, client.Id, ar.AffiliatePlatform, ar.RefId, client.CurrencyId, depositAmount, transactionId, false, depositCount);
        }

        public void WithdrawAffiliateNotification(BllClient client, decimal withdrawAmount, long transactionId)
        {
            if (client.AffiliateReferralId != null)
            {
                var ar = Db.AffiliateReferrals.Include(x => x.AffiliatePlatform).FirstOrDefault(x => x.Id == client.AffiliateReferralId);
                if (ar != null)
                    NotifyAffiliator(client.PartnerId, client.Id, ar.AffiliatePlatform, ar.RefId, client.CurrencyId, withdrawAmount, transactionId, true, null);
            }
        }


        private List<int> GetClientRequiredDocuments(int clientId)
        {
            var client = CacheManager.GetClientById(clientId);
            var partnerConfig = CacheManager.GetConfigKey(client.PartnerId, Constants.PartnerKeys.PartnerKYCTypes);
            if (!string.IsNullOrEmpty(partnerConfig))
            {
                var requiredDocumentTypes = JsonConvert.DeserializeObject<List<int>>(partnerConfig);
                if (requiredDocumentTypes.Any())
                {
                    var clientIdentityTypes = Db.ClientIdentities.Where(x => x.ClientId == clientId && requiredDocumentTypes.Contains(x.DocumentTypeId) &&
                                                                           (x.Status == (int)KYCDocumentStates.Approved || x.Status == (int)KYCDocumentStates.InProcess))
                                                               .Select(x => x.DocumentTypeId).ToList();
                    return requiredDocumentTypes.Except(clientIdentityTypes).ToList();
                }
            }
            return new List<int>();
        }

        private int CheckActiveClientInfoes(int partnerId, string mobileOrEmail)
        {
            var fromDate = DateTime.Now.Date;
            return Db.ClientInfoes.Count(x => x.MobileOrEmail == mobileOrEmail && x.CreationTime > fromDate && x.PartnerId == partnerId);
        }

        private void CheckLimit(int? type, int partnerId, int clientId)
        {
            if (type != null && Constants.PartnerKeyByClientInfoType.ContainsKey(type.Value))
            {
                var client = CacheManager.GetClientById(clientId);
                if (client == null)
                    throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);

                var notificationLimit = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeyByClientInfoType[type.Value]);
                if (notificationLimit.Id > 0 && notificationLimit.NumericValue > 0)
                {
                    var currentDate = DateTime.UtcNow;
                    var fromDate = currentDate.AddHours(-24);
                    var notificationCount = Db.ClientInfoes.Count(x => x.ClientId == clientId &&
                                                                       x.Type == type && x.CreationTime > fromDate);
                    if (notificationCount >= notificationLimit.NumericValue)
                        throw CreateException(LanguageId, Constants.Errors.ClientMaxLimitExceeded);
                }
            }
        }
        private string MapTemplateData(string msgText, Composite composite)
        {
            msgText = msgText.Replace("\\n", Environment.NewLine)
                             .Replace("{c}", composite.VerificationCode)
                             .Replace("{w}", Identity.Domain);

            if (composite.ClientId.HasValue && composite.ClientId.Value > 0)
            {
                msgText = msgText.Replace("{pc}", composite.ClientId.ToString())
                                            .Replace("{u}", composite.UserName)
                                            .Replace("{fn}", composite.FirstName)
                                            .Replace("{e}", composite.Email)
                                            .Replace("{m}", composite.MobileNumber)
                                            .Replace("{a}", composite.Amount?.ToString("F"))
                                            .Replace("{cr}", composite.Currency)
                                            .Replace("{r}", composite.Reason)
                                            .Replace("{bn}", composite.BankName)
                                            .Replace("{bbn}", composite.BankBranchName)
                                            .Replace("{bc}", composite.BankCode)
                                            .Replace("{ban}", composite.BankAccountNumber)
                                            .Replace("{bah}", composite.BankAccountHolder);
                if (msgText.Contains("{d}"))
                {
                    var docTypes = GetClientRequiredDocuments(composite.ClientId.Value);
                    var typesNames = Db.fn_Enumeration().Where(x => x.LanguageId == LanguageId && x.EnumType == nameof(KYCDocumentTypes) &&
                                                                    docTypes.Contains(x.Value)).Select(x => x.Text).ToList();
                    msgText = msgText.Replace("{d}", string.Join(", ", typesNames));
                }
                if (msgText.Contains("{b}"))
                    msgText = msgText.Replace("{b}", BaseBll.GetObjectBalance((int)ObjectTypes.Client, composite.ClientId.Value).AvailableBalance.ToString("F"));
                if (!string.IsNullOrEmpty(composite.Parameters))
                {
                    var pairs = composite.Parameters.Replace("{", string.Empty).Replace("}", string.Empty).Replace("\"", string.Empty).Split(',').ToList();
                    foreach (var p in pairs)
                    {
                        var keyvalue = p.Split(':');
                        if (keyvalue.Length >= 2)
                            msgText = msgText.Replace("{" + keyvalue[0] + "}", keyvalue[1]);
                    }
                }
                if (composite.PaymentRequestState.HasValue)
                {
                    var typesName = Db.fn_Enumeration().Where(x => x.LanguageId == LanguageId && x.EnumType == nameof(PaymentRequestStates) &&
                                                                    x.Value == composite.PaymentRequestState.Value).Select(x => x.Text).FirstOrDefault();
                    msgText = msgText.Replace("{d}", typesName);
                }

            }
            return msgText;
        }

        private ClientMessageTypes MapToMessageType(int type)
        {
            if (Constants.ClientInfoEmailTypes.Contains(type))
            {
                if (Constants.ClientInfoSecuredTypes.Contains(type))
                    return ClientMessageTypes.SecuredEmail;
                else
                    return ClientMessageTypes.Email;
            }
            else if (Constants.ClientInfoSecuredTypes.Contains(type))
                return ClientMessageTypes.SecuredSms;
            return ClientMessageTypes.Sms;
        }

        private void NotifyAffiliator(int partnerId, int clientId, AffiliatePlatform affiliatePlatform, string clickId, string currency, decimal? amount, long? transactionId, bool isWithdraw, int? depositCount = 0)
        {
            try
            {
                var url = CacheManager.GetPartnerSettingByKey(partnerId, string.Format("{0}_{1}", Constants.PartnerKeys.AffiliateUrl, affiliatePlatform.Id)).StringValue;
                var postData = string.Empty;
                Dictionary<string, string> headers = null;
                switch (affiliatePlatform.Name)
                {
                    case AffiliatePlatforms.Evadav:
                        if (!isWithdraw)
                        {
                            var requestInput = new AffiliateNotificationInput
                            {
                                status = (int)AffiliatePlatformStatuses.Success,
                                clickid = clickId,
                            };
                            if (amount.HasValue)
                            {
                                requestInput.sum = ConvertCurrency(currency, Constants.Currencies.USADollar, amount.Value) * 0.95m;
                                requestInput.action_id = transactionId.Value;
                                requestInput.goal = (int)AffiliatePlatformGoalTypes.Deposit;
                            }
                            else
                                requestInput.goal = (int)AffiliatePlatformGoalTypes.Regiter;

                            postData = CommonFunctions.GetSortedParamWithValuesAsString(requestInput, "&");
                        }
                        break;
                    case AffiliatePlatforms.Alfaleads:
                        if (!isWithdraw)
                        {
                            var secureKey = CacheManager.GetPartnerSettingByKey(partnerId, string.Format("{0}_{1}", Constants.PartnerKeys.AffiliateSecure, affiliatePlatform.Id)).StringValue;
                            var notifyInput = new AffiliateNotificationInput
                            {
                                secure = secureKey,
                                status = 1,
                                clickid = clickId
                            };
                            if (amount.HasValue)
                            {
                                notifyInput.goal = 3;
                                notifyInput.sum = ConvertCurrency(currency, Constants.Currencies.USADollar, amount.Value);
                            }
                            else
                                notifyInput.goal = 2;
                            postData = CommonFunctions.GetSortedParamWithValuesAsString(notifyInput, "&");
                        }
                        break;
                    case AffiliatePlatforms.Voluum:
                        if (amount.HasValue)
                        {
                            postData = CommonFunctions.GetSortedParamWithValuesAsString(
                                new
                                {
                                    cid = clickId,
                                    payout = amount,
                                    txid = transactionId,
                                    et = depositCount == 1 ? "deposit1" : "deposit2"
                                }, "&");
                        }
                        else
                            postData = CommonFunctions.GetSortedParamWithValuesAsString(new { cid = clickId, et = "signup" }, "&");
                        break;
                    case AffiliatePlatforms.Affise:
                        if (amount.HasValue)
                        {
                            var transactionAmount = ConvertCurrency(currency, Constants.Currencies.USADollar, amount.Value);
                            if (!isWithdraw && depositCount == 1 && transactionAmount >= 5)
                            {
                                postData = CommonFunctions.GetSortedParamWithValuesAsString(
                                new
                                {
                                    clickid = clickId,
                                    sum = transactionAmount,
                                    goal = "firstdep",
                                    action_id = transactionId
                                }, "&");
                                var httpRequestInput = new HttpRequestInput
                                {
                                    RequestMethod = HttpMethod.Get,
                                    Url = string.Format("{0}?{1}", url, postData)
                                };
                                CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                            }
                            postData = CommonFunctions.GetSortedParamWithValuesAsString(
                            new
                            {
                                clickid = clickId,
                                sum = isWithdraw ? -transactionAmount : transactionAmount,
                                goal = isWithdraw ? "out" : "dep",
                                action_id = transactionId
                            }, "&");
                        }
                        else
                            postData = CommonFunctions.GetSortedParamWithValuesAsString(new { clickid = clickId, goal = "registration" }, "&");
                        break;
                    case AffiliatePlatforms.Dzhaweb:
                        if (!isWithdraw)
                        {
                            if (amount.HasValue)
                            {
                                postData = CommonFunctions.GetSortedParamWithValuesAsString(new
                                {
                                    type = "deposit",
                                    clickid = clickId,
                                    deposit = ConvertCurrency(currency, Constants.Currencies.USADollar, amount.Value),
                                    firstDeposit = depositCount == 1
                                }, "&");
                            }
                            else
                                postData = CommonFunctions.GetSortedParamWithValuesAsString(new { clickid = clickId, type = "register" }, "&");
                        }
                        break;
                    case AffiliatePlatforms.Intelitics:
                        url = String.Format(url, clickId);
                        if (!isWithdraw)
                        {
                            if (amount.HasValue)
                            {
                                postData = CommonFunctions.GetSortedParamWithValuesAsString(new
                                {
                                    type = "deposit",
                                    method = "postback",
                                    currency,
                                    amount,
                                    player_id = clientId,
                                    transaction_id = transactionId,
                                }, "&");
                            }
                            else
                                postData = CommonFunctions.GetSortedParamWithValuesAsString(new { type = "registration", method = "postback" }, "&");
                        }
                        break;
                    case AffiliatePlatforms.MyAffiliates:
                        if (!amount.HasValue)
                        {
                            var client = CacheManager.GetClientById(clientId);
                            var clientGroup = CacheManager.GetPartnerSettingByKey(partnerId, string.Format("{0}_{1}", Constants.PartnerKeys.AffiliateSecure, affiliatePlatform.Id)).StringValue;
                            var apiUsername = CacheManager.GetPartnerSettingByKey(partnerId, string.Format("{0}_{1}", Constants.PartnerKeys.AffiliateUsername, affiliatePlatform.Id)).StringValue;
                            var apiPassword = CacheManager.GetPartnerSettingByKey(partnerId, string.Format("{0}_{1}", Constants.PartnerKeys.AffiliatePassword, affiliatePlatform.Id)).StringValue;
                            headers = new Dictionary<string, string> { { "Authorization", "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(apiUsername + ":" + apiPassword)) } };
                            postData = CommonFunctions.GetSortedParamWithValuesAsString(new
                            {
                                FEED_ID = 8,
                                CLIENT_REFERENCE = clientId,
                                CLIENT_GROUP = clientGroup,
                                JOIN_DATE = client.CreationTime.ToString("yyyy-MM-dd"),
                                DISPLAY_NAME = clientId,
                                TOKEN = clickId,
                                COUNTRY = Identity.Country
                            }, "&");
                        }
                        break;
                    default:
                        break;
                }
                if (!string.IsNullOrEmpty(postData))
                {
                    var httpRequestInput = new HttpRequestInput
                    {
                        RequestMethod = HttpMethod.Get,
                        Url = string.Format("{0}?{1}", url, postData),
                        RequestHeaders = headers
                    };

                    Log.Info(JsonConvert.SerializeObject(httpRequestInput));
                    var resp = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                    Log.Info(resp);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}