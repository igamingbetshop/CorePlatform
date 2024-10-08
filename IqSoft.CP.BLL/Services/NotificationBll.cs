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
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.BLL.Helpers;
using System.Collections.Generic;
using System.Text;
using IqSoft.CP.DAL.Models.Notification;
using IqSoft.CP.Common.Models.AffiliateModels;
using System.Web.UI.WebControls;

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

        public int RegisterActiveEmail(int partnerId, string email, string subject, string body, int? templateId, long? objectId, int? objectTypeId)
        {

            var dbEmail = new Email
            {
                PartnerId = partnerId,
                Receiver = email,
                Status = (int)EmailStates.Active,
                Subject = subject,
                Body = body,
                MessageTemplateId = templateId,
                CreationTime = GetServerDate(),
                ObjectId = objectId,
                ObjectTypeId = objectTypeId
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
                        AttachedContent = fileContent
                    };
                    EmailHelpers.SendEmail(notificationServieId, apiEmailModel, Log);
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
                    SMSHelpers.SendSMS(notificationServieId, apiSMSModel, Log);
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
            var client = CacheManager.GetClientById(clientId) ??
                throw BaseBll.CreateException(string.Empty, Constants.Errors.ClientNotFound);

            var composite = new Composite
            {
                ClientId = clientId,
                UserName = client.UserName,
                FirstName = client.FirstName,
                Email = client.Email,
                MobileNumber = client.MobileNumber,
                Currency = client.CurrencyId
            };
            var currentTime = DateTime.UtcNow;
            var clientMessage = new ClientMessage
            {
                PartnerId = partnerId,
                ObjectId = clientId,
                ObjectTypeId = (int)ObjectTypes.Client,
                Message = MapTemplateData(body, composite),
                Subject = subject,
                Type = (int)ClientMessageTypes.Email,
                MobileOrEmail = receiver,
                Email = new Email
                {
                    PartnerId = partnerId,
                    Receiver = receiver,
                    Status = (int)EmailStates.Active,
                    Subject = subject,
                    Body = body,
                    MessageTemplateId = templateId,
                    CreationTime = currentTime,
                    ObjectId = clientId,
                    ObjectTypeId = (int)ObjectTypes.Client
                },
                CreationTime = currentTime
            };
            Db.ClientMessages.Add(clientMessage);
            Db.SaveChanges();
            return clientMessage;
        }

        public void SendInternalTicket(int clientId, int? notificationType, string messageText = "", PaymentNotificationInfo paymentInfo = null)
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
                ObjectId = client.Id,
                ObjectTypeId = (int)ObjectTypes.Client,
                MobileOrEmail = changed ? email : client.Email,
                ClientInfoType = (int)ClientInfoTypes.EmailVerificationKey,
                VerificationCode = verificationKey
            };
            var activePeriodInMinutes = SendNotificationMessage(notificationModel, out int responseCode);
            if (responseCode > 0)
                throw CreateException(LanguageId, responseCode);
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
                ObjectId = client.Id,
                ObjectTypeId = (int)ObjectTypes.Client,
                MobileOrEmail = changed ? mobileNumber : client.MobileNumber,
                ClientInfoType = (int)ClientInfoTypes.MobileVerificationKey,
                VerificationCode = verificationKey
            };
            var activePeriodInMinutes = SendNotificationMessage(notificationModel, out int responseCode);
            if (responseCode > 0)
                throw CreateException(LanguageId, responseCode);
            if (changed)
            {
                client.MobileNumber = mobileNumber;
                client.LastUpdateTime = DateTime.UtcNow;
                Db.SaveChanges();
                CacheManager.RemoveClientFromCache(client.Id);
            }
            return activePeriodInMinutes;
        }
        public int SendRecoveryEmail(int partnerId, int objectId, int objectTypeId, string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw CreateException(LanguageId, Constants.Errors.EmailCantBeEmpty);

            var partner = CacheManager.GetPartnerById(partnerId);
            var partnerSetting = CacheManager.GetConfigKey(partnerId, Constants.PartnerKeys.VerificationKeyNumberOnly);
            var verificationKey = (!string.IsNullOrEmpty(partnerSetting) && partnerSetting == "1") ? CommonFunctions.GetRandomNumber(partner.EmailVerificationCodeLength) :
                                                                                                     CommonFunctions.GetRandomString(partner.EmailVerificationCodeLength);
            return SendNotificationMessage(new NotificationModel
            {
                PartnerId = partnerId,
                ObjectId = objectId,
                ObjectTypeId = objectTypeId,
                VerificationCode = verificationKey,
                MobileOrEmail = email,
                ClientInfoType = (int)ClientInfoTypes.PasswordRecoveryEmailKey
            }, out _);
        }

        public int SendRecoverySMS(int partnerId, int objectId, int objectTypeId, string mobileNumber)
        {
            if (string.IsNullOrWhiteSpace(mobileNumber))
                throw CreateException(LanguageId, Constants.Errors.MobileNumberCantBeEmpty);

            var partner = CacheManager.GetPartnerById(partnerId);
            var partnerSetting = CacheManager.GetConfigKey(partnerId, Constants.PartnerKeys.VerificationKeyNumberOnly);
            var verificationKey = (!string.IsNullOrEmpty(partnerSetting) && partnerSetting == "1") ? CommonFunctions.GetRandomNumber(partner.MobileVerificationCodeLength) :
                                                                                                     CommonFunctions.GetRandomString(partner.MobileVerificationCodeLength);
            return SendNotificationMessage(new NotificationModel
            {
                PartnerId = partnerId,
                ObjectId = objectId,
                ObjectTypeId = objectTypeId,
                VerificationCode = verificationKey,
                MobileOrEmail = mobileNumber,
                ClientInfoType = (int)ClientInfoTypes.PasswordRecoveryMobileKey
            }, out _);
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
            var messageTemplate = CacheManager.GetPartnerMessageTemplate(client.PartnerId, (int)ClientInfoTypes.AffiliateClientInvitationEmail, LanguageId) ??
                throw CreateException(LanguageId, Constants.Errors.CommentTemplateNotFound);
            var subject = CacheManager.GetPartnerMessageTemplate(client.PartnerId, (int)ClientInfoTypes.AffiliateClientInvitationEmailSubject, LanguageId);

            var messageTextTemplate = messageTemplate.Text;
            var msgText = messageTextTemplate.Replace("\\n", Environment.NewLine)
                                             .Replace("{u}", client.UserName)
                                             .Replace("{w}", Identity.Domain)
                                             .Replace("{pc}", clientId.ToString())
                                             .Replace("{fn}", client.FirstName);
            SaveEmailMessage(client.PartnerId, client.Id, string.IsNullOrEmpty(toEmail) ? client.Email : toEmail, subject?.Text ?? partner.Name, msgText, messageTemplate.Id);
        }

        public int SendNotificationMessage(NotificationModel notificationModel, out int responseCode)
        {
            try
            {
                responseCode = 0;
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
                if (notificationModel.ObjectId > 0)
                {
                    CheckLimit(notificationModel.ClientInfoType, notificationModel.PartnerId, notificationModel.ObjectId, notificationModel.ObjectTypeId);

                    switch (notificationModel.ObjectTypeId)
                    {
                        case (int)ObjectTypes.Client:
                            var client = CacheManager.GetClientById(notificationModel.ObjectId);
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
                            break;
                        case (int)ObjectTypes.Affiliate:
                            var affiliate = Db.Affiliates.FirstOrDefault(x => x.Id == notificationModel.ObjectId);
                            composite.AffiliateId = affiliate.Id;
                            composite.UserName = affiliate.UserName;
                            break;
                        case (int)ObjectTypes.Partner:
                            composite.Email = notificationModel.RequesterEmail;
                            composite.MessageText = notificationModel.MessageText;
                            break;
                        default: break;
                    }
                }
                messageTextTemplate = MapTemplateData(messageTextTemplate, composite);

                var currentDate = DateTime.UtcNow;
                if (notificationModel.ClientInfoType != null)
                    notificationModel.MessageType = (int)MapToMessageType(notificationModel.ClientInfoType.Value);
                var message = Db.ClientMessages.Add(
                    new ClientMessage
                    {
                        PartnerId = notificationModel.PartnerId,
                        ObjectId = notificationModel.ObjectId,
                        ObjectTypeId = notificationModel.ObjectTypeId,
                        Subject = partner.Name,
                        MobileOrEmail = notificationModel.MobileOrEmail,
                        Type = notificationModel.MessageType,
                        Message = notificationModel.MessageType == (int)ClientMessageTypes.SecuredSms ||
                            notificationModel.MessageType == (int)ClientMessageTypes.SecuredEmail ? "Secured" : messageTextTemplate,
                        CreationTime = currentDate
                    });
                if (!string.IsNullOrEmpty(notificationModel.VerificationCode) && notificationModel.ClientInfoType != (int)ClientInfoTypes.NewIpLoginEmail)
                {
                    var activeClientInfoesCount = CheckActiveClientInfoes(notificationModel.PartnerId, notificationModel.MobileOrEmail);
                    if (activeClientInfoesCount > 0)
                        verificationCodeActivePeriod /= 2;
                    Db.ClientInfoes.Where(x => (x.State == (int)ClientInfoStates.Active || x.State == (int)ClientInfoStates.Verified) &&
                                                x.MobileOrEmail == notificationModel.MobileOrEmail && x.PartnerId == notificationModel.PartnerId)
                                   .UpdateFromQuery(x => new ClientInfo { State = (int)ClientInfoStates.Expired });
                    Db.ClientInfoes.Add(new ClientInfo
                    {
                        PartnerId = notificationModel.PartnerId,
                        ObjectId = notificationModel.ObjectId,
                        ObjectTypeId = notificationModel.ObjectTypeId,
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

                var emailSubject = !string.IsNullOrEmpty(notificationModel.SubjectText) ? notificationModel.SubjectText :
                                   (notificationModel.SubjectType.HasValue ? CacheManager.GetPartnerMessageTemplate(notificationModel.PartnerId,
                                    notificationModel.SubjectType.Value, languageId)?.Text : partner.Name);
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
                var emailId = RegisterActiveEmail(notificationModel.PartnerId, notificationModel.MobileOrEmail, emailSubject, messageTextTemplate, messageTemplateId, notificationModel.ObjectId, notificationModel.ObjectTypeId);
                message.EmailId = emailId;
                Db.SaveChanges();
                return verificationCodeActivePeriod;
            }
            catch (FaultException<BllFnErrorType> e)
            {
                Log.Error($"Code: {e.Detail.Id} Message: {e.Detail.Message} Notification: {JsonConvert.SerializeObject(notificationModel)}");
                responseCode = e.Detail.Id;
            }
            catch (Exception e)
            {
                Log.Error($"Code: {Constants.Errors.GeneralException} Message: {e} Notification: {JsonConvert.SerializeObject(notificationModel)}");
                responseCode = Constants.Errors.GeneralException;
            }
            return 0;
        }

        public void SendDepositNotification(int clientId, int requestState, decimal amount, string reason)
        {
            try
            {
                if (requestState != (int)PaymentRequestStates.PayPanding)
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
                    var subjectType = (int)ClientInfoTypes.DepositNotificationSubject;
                    if (requestState != (int)PaymentRequestStates.Approved && requestState != (int)PaymentRequestStates.ApprovedManually)
                    {
                        emailType = (int)ClientInfoTypes.FailedDepositEmail;
                        ticketType = (int)ClientInfoTypes.FailedDepositTicket;
                        subjectType = (int)ClientInfoTypes.FailedDepositSubject;
                    }
                    var partnerConfig = CacheManager.GetConfigKey(client.PartnerId, Constants.PartnerKeys.SendDepositEmailNotification);
                    if (!string.IsNullOrEmpty(partnerConfig) && partnerConfig == "1")
                    {
                        var notificationModel = new NotificationModel
                        {
                            PartnerId = client.PartnerId,
                            ObjectId = client.Id,
                            ObjectTypeId = (int)ObjectTypes.Client,
                            MobileOrEmail = client.Email,
                            ClientInfoType = emailType,
                            SubjectType = subjectType,
                            PaymentInfo = paymentInfo
                        };
                        SendNotificationMessage(notificationModel, out int responseCode);
                    }
                    SendInternalTicket(client.Id, ticketType, paymentInfo: paymentInfo);
                }
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
                        ObjectId = client.Id,
                        ObjectTypeId = (int)ObjectTypes.Client,
                        MobileOrEmail = client.Email,
                        ClientInfoType = emailType,
                        SubjectType = (int)ClientInfoTypes.WithdrawStatusChangeSubject,
                        PaymentInfo = paymentInfo
                    };
                    SendNotificationMessage(notificationModel, out int responseCode);
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

        private void CheckLimit(int? type, int partnerId, int objectId, int objectTypeId)
        {
            if (type != null && Constants.PartnerKeyByClientInfoType.ContainsKey(type.Value))
            {

                var notificationLimit = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeyByClientInfoType[type.Value]);
                if (notificationLimit.Id > 0 && notificationLimit.NumericValue > 0)
                {
                    var currentDate = DateTime.UtcNow;
                    var fromDate = currentDate.AddHours(-24);
                    var notificationCount = Db.ClientInfoes.Count(x => x.ObjectId == objectId && x.ObjectTypeId == objectTypeId &&
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
                                 .Replace("{em}", composite.MessageText)
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
                    msgText = msgText.Replace("{ps}", typesName);
                }

            }
            if (composite.AffiliateId.HasValue && composite.AffiliateId.Value > 0)
                msgText = msgText.Replace("{u}", composite.UserName);
            else
                msgText = msgText.Replace("{e}", composite.Email).Replace("{em}", composite.MessageText);
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

        public static string NotifyAffiliator(NotificationTypes notificationTypes, AffiliateData affiliateData, ILog log)
        {
            try
            {
                if (affiliateData.AffiliatePlatformId != 0)
                {
                    switch (notificationTypes)
                    {
                        case NotificationTypes.Registration:
                            return AffiliateRegisterationNotification(affiliateData, log);
                        case NotificationTypes.Deposit:
                            return AffiliateDepositNotification(affiliateData);
                        case NotificationTypes.Withdraw:
                            return AffiliateWithdrawNotification(affiliateData);
                        default:
                            return string.Empty;

                    }
                }
                return string.Empty;

            }
            catch (Exception e)
            {
                log.Error(e);
                throw;
            }
        }

        private static string AffiliateRegisterationNotification(AffiliateData affiliateData, ILog log)
        {
            var url = CacheManager.GetPartnerSettingByKey(affiliateData.PartnerId, string.Format("{0}_{1}", Constants.PartnerKeys.AffiliateUrl, affiliateData.AffiliatePlatformId)).StringValue;
            var postData = string.Empty;
            Dictionary<string, string> headers = null;
            var response = string.Empty;
            switch (affiliateData.AffiliatePlatformName)
            {
                case AffiliatePlatforms.Evadav:
                    var requestInput = new AffiliateNotificationInput
                    {
                        status = (int)AffiliatePlatformStatuses.Success,
                        clickid = affiliateData.ClickId,
                        goal = (int)AffiliatePlatformGoalTypes.Regiter
                    };
                    postData = CommonFunctions.GetSortedParamWithValuesAsString(requestInput, "&");
                    break;
                case AffiliatePlatforms.Alfaleads:
                    var secureKey = CacheManager.GetPartnerSettingByKey(affiliateData.PartnerId, string.Format("{0}_{1}", Constants.PartnerKeys.AffiliateSecure, affiliateData.AffiliatePlatformId)).StringValue;
                    var notifyInput = new AffiliateNotificationInput
                    {
                        secure = secureKey,
                        status = 1,
                        clickid = affiliateData.ClickId,
                        goal = 2
                    };
                    postData = CommonFunctions.GetSortedParamWithValuesAsString(notifyInput, "&");
                    break;
                case AffiliatePlatforms.Voluum:
                    postData = CommonFunctions.GetSortedParamWithValuesAsString(new { cid = affiliateData.ClickId, et = "signup" }, "&");
                    break;
                case AffiliatePlatforms.Affise:
                    postData = CommonFunctions.GetSortedParamWithValuesAsString(new { clickid = affiliateData.ClickId, goal = "registration" }, "&");
                    break;
                case AffiliatePlatforms.Dzhaweb:
                    postData = CommonFunctions.GetSortedParamWithValuesAsString(new { clickid = affiliateData.ClickId, type = "register" }, "&");
                    break;
                case AffiliatePlatforms.Intelitics:
                    url = String.Format(url, affiliateData.ClickId);
                    postData = CommonFunctions.GetSortedParamWithValuesAsString(new { type = "registration", method = "postback" }, "&");
                    break;
                case AffiliatePlatforms.MyAffiliates:
                case AffiliatePlatforms.VipAffiliate:
                    var sendPostback = CacheManager.GetPartnerSettingByKey(affiliateData.PartnerId, $"{affiliateData.AffiliatePlatformName}_{Constants.PartnerKeys.AffiliateSendPostback}").StringValue;
                    if (sendPostback != "0")
                    {
                        var client = CacheManager.GetClientById(affiliateData.ClientId);
                        var clientGroup = CacheManager.GetPartnerSettingByKey(affiliateData.PartnerId, string.Format("{0}_{1}", Constants.PartnerKeys.AffiliateSecure, affiliateData.AffiliatePlatformId)).StringValue;
                        var apiUsername = CacheManager.GetPartnerSettingByKey(affiliateData.PartnerId, string.Format("{0}_{1}", Constants.PartnerKeys.AffiliateUsername, affiliateData.AffiliatePlatformId)).StringValue;
                        var apiPassword = CacheManager.GetPartnerSettingByKey(affiliateData.PartnerId, string.Format("{0}_{1}", Constants.PartnerKeys.AffiliatePassword, affiliateData.AffiliatePlatformId)).StringValue;
                        headers = new Dictionary<string, string> { { "Authorization", "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(apiUsername + ":" + apiPassword)) } };
                        postData = CommonFunctions.GetSortedParamWithValuesAsString(new
                        {
                            FEED_ID = 8,
                            CLIENT_REFERENCE = affiliateData.ClientId,
                            CLIENT_GROUP = clientGroup,
                            JOIN_DATE = client.CreationTime.ToString("yyyy-MM-dd"),
                            DISPLAY_NAME = affiliateData.ClientId,
                            TOKEN = affiliateData.ClickId,
                            COUNTRY = CacheManager.GetRegionById(client.CountryId ?? client.RegionId, client.LanguageId)?.IsoCode
                        }, "&");
                    }
                    break;
                case AffiliatePlatforms.Affilka:
                    var player = CacheManager.GetClientById(affiliateData.ClientId);
                    var apiKey = CacheManager.GetPartnerSettingByKey(affiliateData.PartnerId, AffiliatePlatforms.Affilka + Constants.PartnerKeys.AffiliateFtpUsername).StringValue;
                    var apiSecret = CacheManager.GetPartnerSettingByKey(affiliateData.PartnerId, AffiliatePlatforms.Affilka + Constants.PartnerKeys.AffiliateFtpPassword).StringValue;
                    var inputJson = string.Empty;
                    var httpInput = new HttpRequestInput
                    {
                        RequestMethod = Constants.HttpRequestMethods.Post,
                        ContentType = Constants.HttpContentTypes.ApplicationJson,
                        Url = $"{url}/players"
                    };

                    if (!affiliateData.ClickId.Contains("_"))
                    {
                        inputJson = JsonConvert.SerializeObject(new
                        {
                            players = new List<object> { { new
                            {
                               bonus_code = affiliateData.ClickId,
                               user_id = player.Id.ToString()
                            } } }
                        });
                        httpInput.RequestHeaders = new Dictionary<string, string>
                        {
                            { "X-Authorization-Key", apiKey },
                            { "X-Authorization-Sign", CommonFunctions.ComputeHMACSha512(inputJson, apiSecret).ToLower() }
                        };
                        httpInput.PostData = inputJson;
                        var resp = CommonFunctions.SendHttpRequest(httpInput, out _);

                        try
                        {
                            var stagResponse = JsonConvert.DeserializeObject<List<AffilkaOutput>>(resp);
                            if (!stagResponse.Any())
                                throw new Exception($"PostDate: {inputJson}, ResponseBase: {resp}, Exception: NotFound");
                            using (var clientBll = new ClientBll(new SessionIdentity(), log))
                            {
                                affiliateData.ClickId = stagResponse.First().Stag;
                                clientBll.ChangCilentAffiliateData(player.Id, affiliateData.AffiliatePlatformId.Value, affiliateData.AffiliatePlatformId.ToString(), affiliateData.ClickId);
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Error($"PostDate: {inputJson}, ResponseBase: {resp}, Exception: {ex}");
                            using (var clientBll = new ClientBll(new SessionIdentity(), log))
                                clientBll.ChangCilentAffiliateData(player.Id, 0, string.Empty, string.Empty);

                        }
                    }
                    inputJson = JsonConvert.SerializeObject(new
                    {
                        players = new List<object> { { new
                            {
                               tag = affiliateData.ClickId,
                               email = player.Email,
                               user_id = player.Id.ToString(),
                               country = CacheManager.GetRegionById(player.CountryId ?? player.RegionId, player.LanguageId)?.IsoCode,
                               sign_up_at = player.CreationTime.ToString("yyyy-MM-ddTHH:mm:ss.fff+00:00")
                            } } }
                    });
                    httpInput.RequestHeaders = new Dictionary<string, string>
                    {
                        { "X-Authorization-Key", apiKey },
                        { "X-Authorization-Sign", CommonFunctions.ComputeHMACSha512(inputJson, apiSecret).ToLower() }
                    };
                    httpInput.PostData = inputJson;
                    var res = CommonFunctions.SendHttpRequest(httpInput, out _);
                    response = $"PostDate: {inputJson}, ResponseBase: {res}";
                    break;
                case AffiliatePlatforms.Trackier:
                    player = CacheManager.GetClientById(affiliateData.ClientId);
                    apiKey = CacheManager.GetPartnerSettingByKey(affiliateData.PartnerId, AffiliatePlatforms.Trackier + Constants.PartnerKeys.AffiliateApiKey).StringValue;

                    var currentTime = DateTime.UtcNow;
                    inputJson = JsonConvert.SerializeObject(new
                    {
                        customerId = player.Id.ToString(),
                        customerName = player.UserName,
                        date = currentTime.ToString("yyyy-MM-dd"),
                        currency = player.CurrencyId.ToLower(),
                        productId = "1",
                        trackingToken = affiliateData.ClickId,
                        country = CacheManager.GetRegionById(player.CountryId ?? player.RegionId, player.LanguageId)?.IsoCode,
                    });
                    httpInput = new HttpRequestInput
                    {
                        RequestMethod = Constants.HttpRequestMethods.Post,
                        ContentType = Constants.HttpContentTypes.ApplicationJson,
                        Url = $"{url}/customers",
                        RequestHeaders = new Dictionary<string, string> { { "x-api-key", apiKey } },
                        PostData = inputJson
                    };
                    log.Info("Trackier request: " + JsonConvert.SerializeObject(httpInput));
                    var r = CommonFunctions.SendHttpRequest(httpInput, out _);
                    log.Info("Trackier resp: " + r);
                    response = $"PostDate: {inputJson}, ResponseBase: {r}";
                    break;
                default:
                    break;
            }
            if (!string.IsNullOrEmpty(postData))
            {
                var httpRequestInput = new HttpRequestInput
                {
                    RequestMethod = Constants.HttpRequestMethods.Get,
                    Url = string.Format("{0}?{1}", url, postData),
                    RequestHeaders = headers
                };
                log.Info(JsonConvert.SerializeObject(httpRequestInput));
                var resp = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                log.Info(resp);
                return $"PostDate: {postData}, ResponseBase: {resp}";
            }
            return response;
        }

        private static string AffiliateDepositNotification(AffiliateData affiliateData)
        {
            var url = CacheManager.GetPartnerSettingByKey(affiliateData.PartnerId, string.Format("{0}_{1}", Constants.PartnerKeys.AffiliateUrl, affiliateData.AffiliatePlatformId)).StringValue;
            var postData = string.Empty;
            Dictionary<string, string> headers = null;
            switch (affiliateData.AffiliatePlatformName)
            {
                case AffiliatePlatforms.Evadav:
                    var requestInput = new AffiliateNotificationInput
                    {
                        status = (int)AffiliatePlatformStatuses.Success,
                        clickid = affiliateData.ClickId,
                        sum = ConvertCurrency(affiliateData.CurrencyId, Constants.Currencies.USADollar, affiliateData.Amount.Value) * 0.95m,
                        action_id = affiliateData.TransactionId,
                        goal = (int)AffiliatePlatformGoalTypes.Deposit
                    };
                    postData = CommonFunctions.GetSortedParamWithValuesAsString(requestInput, "&");
                    break;
                case AffiliatePlatforms.Alfaleads:
                    var secureKey = CacheManager.GetPartnerSettingByKey(affiliateData.PartnerId, string.Format("{0}_{1}", Constants.PartnerKeys.AffiliateSecure, affiliateData.AffiliatePlatformId)).StringValue;
                    var notifyInput = new AffiliateNotificationInput
                    {
                        secure = secureKey,
                        status = 1,
                        clickid = affiliateData.ClickId,
                        goal = 3,
                        sum = ConvertCurrency(affiliateData.CurrencyId, Constants.Currencies.USADollar, affiliateData.Amount ?? 0)
                    };
                    postData = CommonFunctions.GetSortedParamWithValuesAsString(notifyInput, "&");
                    break;
                case AffiliatePlatforms.Voluum:
                    postData = CommonFunctions.GetSortedParamWithValuesAsString(
                        new
                        {
                            cid = affiliateData.ClickId,
                            payout = affiliateData.Amount ?? 0,
                            txid = affiliateData.TransactionId,
                            et = affiliateData.DepositCount == 1 ? "deposit1" : "deposit2"
                        }, "&");
                    break;
                case AffiliatePlatforms.Affise:
                    var transactionAmount = ConvertCurrency(affiliateData.CurrencyId, Constants.Currencies.USADollar, affiliateData.Amount ?? 0);
                    if (affiliateData.DepositCount == 1 && transactionAmount >= 5)
                    {
                        postData = CommonFunctions.GetSortedParamWithValuesAsString(
                          new
                          {
                              clickid = affiliateData.ClickId,
                              sum = transactionAmount,
                              goal = "firstdep",
                              action_id = affiliateData.TransactionId
                          }, "&");
                    }
                    else
                        postData = CommonFunctions.GetSortedParamWithValuesAsString(
                        new
                        {
                            clickid = affiliateData.ClientId,
                            sum = transactionAmount,
                            goal = "dep",
                            action_id = affiliateData.TransactionId
                        }, "&");
                    break;
                case AffiliatePlatforms.Dzhaweb:
                    postData = CommonFunctions.GetSortedParamWithValuesAsString(new
                    {
                        type = "deposit",
                        clickid = affiliateData.ClickId,
                        deposit = ConvertCurrency(affiliateData.CurrencyId, Constants.Currencies.USADollar, affiliateData.Amount ?? 0),
                        firstDeposit = affiliateData.DepositCount == 1
                    }, "&");
                    break;
                case AffiliatePlatforms.Intelitics:
                    url = String.Format(url, affiliateData.ClickId);
                    postData = CommonFunctions.GetSortedParamWithValuesAsString(new
                    {
                        type = "deposit",
                        method = "postback",
                        currency = affiliateData.CurrencyId,
                        amount = affiliateData.Amount ?? 0,
                        player_id = affiliateData.ClientId,
                        transaction_id = affiliateData.TransactionId,
                    }, "&");
                    break;
                default:
                    break;
            }
            if (!string.IsNullOrEmpty(postData))
            {
                var httpRequestInput = new HttpRequestInput
                {
                    RequestMethod = Constants.HttpRequestMethods.Get,
                    Url = string.Format("{0}?{1}", url, postData),
                    RequestHeaders = headers
                };
                var resp = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                return $"PostDate: {postData}, ResponseBase: {resp}";
            }
            return string.Empty;
        }

        private static string AffiliateWithdrawNotification(AffiliateData affiliateData)
        {
            var url = CacheManager.GetPartnerSettingByKey(affiliateData.PartnerId, string.Format("{0}_{1}", Constants.PartnerKeys.AffiliateUrl, affiliateData.AffiliatePlatformId)).StringValue;
            var postData = string.Empty;
            Dictionary<string, string> headers = null;
            switch (affiliateData.AffiliatePlatformName)
            {
                case AffiliatePlatforms.Affise:
                    var transactionAmount = ConvertCurrency(affiliateData.CurrencyId, Constants.Currencies.USADollar, affiliateData.Amount ?? 0);
                    postData = CommonFunctions.GetSortedParamWithValuesAsString(
                        new
                        {
                            clickid = affiliateData.ClickId,
                            sum = -transactionAmount,
                            goal = "out",
                            action_id = affiliateData.TransactionId
                        }, "&");
                    break;
                default:
                    break;
            }
            if (!string.IsNullOrEmpty(postData))
            {
                var httpRequestInput = new HttpRequestInput
                {
                    RequestMethod = Constants.HttpRequestMethods.Get,
                    Url = string.Format("{0}?{1}", url, postData),
                    RequestHeaders = headers
                };
                var resp = CommonFunctions.SendHttpRequest(httpRequestInput, out _);
                return $"PostDate: {postData}, ResponseBase: {resp}";
            }
            return string.Empty;
        }

        public void UpdateNotificationStatus(int notificationId, int state, string reason = null)
        {
            var trigers = Db.NotificationTriggers.Where(n => n.Id == notificationId).FirstOrDefault();

            if (trigers.State == (int)NotificationStates.Failed || state == (int)NotificationStates.Failed)
                state = (int)NotificationStates.Failed;

            trigers.State = state;
            trigers.Reason += reason ?? string.Empty;
            trigers.LastUpdateDate = DateTime.UtcNow;
            Db.SaveChanges();
        }

        public static string CrmNotification(NotificationTypes notificationTypes, AffiliateData affiliateData, ILog log)
        {
            try
            {
                switch (notificationTypes)
                {
                    case NotificationTypes.Registration:
                        return CustomerIoRegisterationNot(affiliateData, log);
                    case NotificationTypes.Deposit:
                        return CustomerIoDepositNot(affiliateData, log);
                    case NotificationTypes.Withdraw:
                        return CustomerIoWithdrawNot(affiliateData, log);
                    case NotificationTypes.Login:
                        return CustomerIoLoginNot(affiliateData, log);
                    default:
                        return string.Empty;
                }
            }
            catch (Exception e)
            {
                log.Error(e);
                throw;
            }
        }

        private static string CustomerIoRegisterationNot(AffiliateData affiliateData, ILog log)
        {
            var res = string.Empty;
            var url = CacheManager.GetPartnerSettingByKey(affiliateData.PartnerId, Constants.PartnerKeys.CustomerIoUrl).StringValue;
            url = $"{url}/identify";
            var customer = CacheManager.GetClientById(affiliateData.ClientId);
            var partnerName = CacheManager.GetPartnerById(customer.PartnerId).Name;
            var apikey = CacheManager.GetPartnerSettingByKey(customer.PartnerId, Constants.PartnerKeys.CustomerIoApiKey).StringValue;

            var postData = JsonConvert.SerializeObject(new
            {
                userId = customer.Id.ToString(),
                traits = new
                {
                    partnerId = customer.PartnerId,
                    currencyId = customer.CurrencyId,
                    userName = customer.UserName,
                    email = customer.Email,
                    firstname = customer?.FirstName,
                    lastName = customer?.LastName,
                    gender = customer.Gender,
                    mobileNumber = customer.MobileNumber,
                    zipCode = customer.ZipCode,
                    birthDate = customer.BirthDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    regionId = customer.RegionId,
                    categoryId = customer.CategoryId,
                    brandName = partnerName,
                    state = Enum.GetName(typeof(ClientStates), customer.State),
                    countryId = CacheManager.GetRegionById(customer.CountryId.Value, customer.LanguageId).IsoCode3,
                    city = customer.City,
                    languageId = customer.LanguageId,
                    isDocumentVerified = customer.IsDocumentVerified,
                    documentNumber = customer.DocumentNumber,
                    documentIssuedBy = customer.DocumentIssuedBy,
                    sendPromotions = customer.SendPromotions,
                    affiliateReferralId = affiliateData?.ClickId,
                    creationTime = customer.CreationTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    address = customer.Address,
                    realBalance = 0,
                    registrationdate = customer?.CreationTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    affiliateid = affiliateData?.AffiliatePlatId,
                }

            });

            var httpRequest = new HttpRequestInput
            {
                RequestMethod = Constants.HttpRequestMethods.Post,
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                Url = url,
                RequestHeaders = new Dictionary<string, string> { { "Authorization", "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes($"{apikey}:")) } }, // ??
                PostData = postData
            };
            log.Info("identify" + JsonConvert.SerializeObject(httpRequest));
            return CommonFunctions.SendHttpRequest(httpRequest, out _);
        }

        private static string CustomerIoWithdrawNot(AffiliateData affiliateData, ILog log)
        {
            var res = string.Empty;
            var url = CacheManager.GetPartnerSettingByKey(affiliateData.PartnerId, Constants.PartnerKeys.CustomerIoUrl).StringValue;
            url = $"{url}/track";
            var customer = CacheManager.GetClientById(affiliateData.ClientId);
            var partnerName = CacheManager.GetPartnerById(customer.PartnerId).Name;
            var apikey = CacheManager.GetPartnerSettingByKey(customer.PartnerId, Constants.PartnerKeys.CustomerIoApiKey).StringValue;

            var postData = JsonConvert.SerializeObject(new
            {
                userId = customer.Id.ToString(),
                type = "track",
                @event = "Withdraw",
                properties = new
                {
                    affiliateData?.Amount,
                    affiliateData?.TransactionId,
                    currency = customer.CurrencyId
                }
            });

            var httpRequest = new HttpRequestInput
            {
                RequestMethod = Constants.HttpRequestMethods.Post,
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                Url = url,
                RequestHeaders = new Dictionary<string, string> { { "Authorization", "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes($"{apikey}:")) } },
                PostData = postData
            };

            log.Info("Withdraw" + JsonConvert.SerializeObject(httpRequest));
            //  var am = CommonFunctions.SendHttpRequest(httpRequest, out _);
            res = CommonFunctions.SendHttpRequest(httpRequest, out _);

            postData = JsonConvert.SerializeObject(new
            {
                userId = customer.Id.ToString(),
                type = "track",
                @event = "Update Profile",
                properties = new
                {
                    realBalance = Math.Floor(BaseBll.GetObjectBalance((int)ObjectTypes.Client, customer.Id).Balances.Where(y => y.TypeId != (int)AccountTypes.ClientCompBalance &&
                                                    y.TypeId != (int)AccountTypes.ClientCoinBalance &&
                                                    y.TypeId != (int)AccountTypes.ClientBonusBalance).Sum(y => y.Balance) * 100) / 100
                }
            });

            httpRequest = new HttpRequestInput
            {
                RequestMethod = Constants.HttpRequestMethods.Post,
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                Url = url,
                RequestHeaders = new Dictionary<string, string> { { "Authorization", "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes($"{apikey}:")) } },
                PostData = postData
            };


            log.Info("UpdateProfileWithdraw" + JsonConvert.SerializeObject(httpRequest));
            res += CommonFunctions.SendHttpRequest(httpRequest, out _);
            return res;
        }

        private static string CustomerIoDepositNot(AffiliateData affiliateData, ILog log)
        {
            var res = string.Empty;
            var url = CacheManager.GetPartnerSettingByKey(affiliateData.PartnerId, Constants.PartnerKeys.CustomerIoUrl).StringValue;
            url = $"{url}/track";
            var customer = CacheManager.GetClientById(affiliateData.ClientId);
            var partnerName = CacheManager.GetPartnerById(customer.PartnerId).Name;
            var apikey = CacheManager.GetPartnerSettingByKey(customer.PartnerId, Constants.PartnerKeys.CustomerIoApiKey).StringValue;

            var postData = JsonConvert.SerializeObject(new
            {
                userId = customer.Id.ToString(),
                type = "track",
                @event = "Deposit",
                properties = new
                {
                    affiliateData?.Amount,
                    affiliateData?.TransactionId,
                    currency = customer.CurrencyId,
                    affiliateData?.DepositCount
                }
            });

            var httpRequest = new HttpRequestInput
            {
                RequestMethod = Constants.HttpRequestMethods.Post,
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                Url = url,
                RequestHeaders = new Dictionary<string, string> { { "Authorization", "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes($"{apikey}:")) } },
                PostData = postData
            };

            log.Info("Deposit" + JsonConvert.SerializeObject(httpRequest));
            res = CommonFunctions.SendHttpRequest(httpRequest, out _);

            postData = JsonConvert.SerializeObject(new
            {
                userId = customer.Id.ToString(),
                type = "track",
                @event = "Update Profile",
                properties = new
                {
                    realBalance = Math.Floor(BaseBll.GetObjectBalance((int)ObjectTypes.Client, customer.Id).Balances.Where(y => y.TypeId != (int)AccountTypes.ClientCompBalance &&
                                                    y.TypeId != (int)AccountTypes.ClientCoinBalance &&
                                                    y.TypeId != (int)AccountTypes.ClientBonusBalance).Sum(y => y.Balance) * 100) / 100,
                    lastDepositDate = affiliateData?.CreationDate.ToString("yyyy-MM-dd HH:mm:ss")
                }
            });

            httpRequest = new HttpRequestInput
            {
                RequestMethod = Constants.HttpRequestMethods.Post,
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                Url = url,
                RequestHeaders = new Dictionary<string, string> { { "Authorization", "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes($"{apikey}:")) } },
                PostData = postData
            };

            log.Info("UpdateProfileDepsoit" + JsonConvert.SerializeObject(httpRequest));
            res += CommonFunctions.SendHttpRequest(httpRequest, out _);
            return res;
        }

        private static string CustomerIoLoginNot(AffiliateData affiliateData, ILog log)
        {
            var res = string.Empty;
            var url = CacheManager.GetPartnerSettingByKey(affiliateData.PartnerId, Constants.PartnerKeys.CustomerIoUrl).StringValue;
            url = $"{url}/track";
            var customer = CacheManager.GetClientById(affiliateData.ClientId);
            var partnerName = CacheManager.GetPartnerById(customer.PartnerId).Name;
            var apikey = CacheManager.GetPartnerSettingByKey(customer.PartnerId, Constants.PartnerKeys.CustomerIoApiKey).StringValue;
            PaymentRequestHistoryClients paymantReques;
            using (var paymentSystemBl = new PaymentSystemBll(new SessionIdentity(), log))
                paymantReques = paymentSystemBl.SuccessPaymantRequest(customer.Id);

            var postData = JsonConvert.SerializeObject(new
            {
                userId = customer.Id.ToString(),
                type = "track",
                @event = "Update Profile",
                properties = new
                {
                    partnerId = customer.PartnerId,
                    currencyId = customer.CurrencyId,
                    userName = customer.UserName,
                    email = customer.Email,
                    firstname = customer.FirstName,
                    lastName = customer.LastName,
                    gender = customer.Gender,
                    mobileNumber = customer.MobileNumber,
                    zipCode = customer.ZipCode,
                    birthDate = customer.BirthDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    regionId = customer.RegionId,
                    categoryId = customer.CategoryId,
                    brandName = partnerName,
                    state = Enum.GetName(typeof(ClientStates), customer.State),
                    countryId = CacheManager.GetRegionById(customer.CountryId.Value, customer.LanguageId).IsoCode3,
                    city = customer.City,
                    languageId = customer.LanguageId,
                    isDocumentVerified = customer.IsDocumentVerified,
                    documentNumber = customer.DocumentNumber,
                    documentIssuedBy = customer.DocumentIssuedBy,
                    sendPromotions = customer.SendPromotions,
                    affiliateReferralId = affiliateData?.ClickId,
                    creationTime = customer.CreationTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    address = customer.Address,
                    realBalance = Math.Floor(BaseBll.GetObjectBalance((int)ObjectTypes.Client, customer.Id).Balances.Where(y => y.TypeId != (int)AccountTypes.ClientCompBalance &&
                                                        y.TypeId != (int)AccountTypes.ClientCoinBalance &&
                                                        y.TypeId != (int)AccountTypes.ClientBonusBalance).Sum(y => y.Balance) * 100) / 100,
                    lastSessionDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                    lastlogindate = affiliateData.CreationDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    withdrawalamount = paymantReques?.TotalWithdrawAmount,
                    totaldepositamount = paymantReques?.TotalDepositAmount,
                    lastdepositdate = paymantReques?.LastDepositDate?.ToString("yyyy-MM-dd HH:mm:ss"),
                    numberofdepositsLastsevenDays = paymantReques.CountOfDepositLastWeek,
                    numberofdeposits = paymantReques?.CountOfDeposits,
                    firstdepositdate = paymantReques?.FirstDepositDate?.ToString("yyyy-MM-dd HH:mm:ss"),
                    lastwithdrawaldate = paymantReques?.LastWithdrawDate?.ToString("yyyy-MM-dd HH:mm:ss")
                }
            });


            var httpRequest = new HttpRequestInput
            {
                RequestMethod = Constants.HttpRequestMethods.Post,
                ContentType = Constants.HttpContentTypes.ApplicationJson,
                Url = url,
                RequestHeaders = new Dictionary<string, string> { { "Authorization", "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes($"{apikey}:")) } },
                PostData = postData
            };

            log.Info("track" + JsonConvert.SerializeObject(httpRequest));
            res = CommonFunctions.SendHttpRequest(httpRequest, out _);

            return res;
        }

        /* public string NotifyAffiliators(NotificationTypes notificationTypes, AffiliateData affiliatePlatform)// AffiliatePlatform affiliatePlatform)
          {
              var test = string.Empty;
              DateTime? depositDate = DateTime.UtcNow;
              try
              {
                  int partnerId = 1;
                  int clientId = 0;
                  string clickId = "";
                  string currency = "";
                  decimal? amount = 0;
                  bool isWithdraw = false;
                  int? depositCount = 0;
                  bool isLogin = false;
                  long? transactionId = 0;


                  //affiliateData.PartnerId,
                  //affiliateData.ClientId, 
                  //affTrigger.AffiliatePlatform,
                  //affiliateData?.ClickId,
                  //affiliateData.CurrencyId,
                  //affiliateData?.Amount, 
                  ///*affiliateData?.TransactionId,
                  //isWithdraw,
                  //affiliateData.DepositCount,
                  //isLogin
                  isLogin = notificationTypes == NotificationTypes.Login ? true : false;
                  isWithdraw = notificationTypes == NotificationTypes.Withdraw ? true : false;

                  var postData = string.Empty;
                  Dictionary<string, string> headers = null;
                  var customer = CacheManager.GetClientById(affiliatePlatform.ClientId);
                  var url = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.CustomerIoUrl).StringValue;
                  var partnerName = CacheManager.GetPartnerById(customer.PartnerId).Name;
                  var reqList = new List<string>();

                  if (isLogin)//login
                  {
                      postData = JsonConvert.SerializeObject(new
                      {
                          userId = customer.Id.ToString(),
                          type = "track",
                          @event = "Update Profile",
                          properties = new
                          {
                              partnerId = customer.PartnerId,
                              currencyId = customer.CurrencyId,
                              userName = customer.UserName,
                              email = customer.Email,
                              firstname = customer.FirstName,
                              lastName = customer.LastName,
                              gender = customer.Gender,
                              mobileNumber = customer.MobileNumber,
                              zipCode = customer.ZipCode,
                              birthDate = customer.BirthDate.ToString("yyyy-MM-dd HH:mm:ss"),
                              regionId = customer.RegionId,
                              categoryId = customer.CategoryId,
                              brandName = partnerName,
                              state = Enum.GetName(typeof(ClientStates), customer.State),
                              countryId = CacheManager.GetRegionById(customer.CountryId.Value, customer.LanguageId).IsoCode3,
                              city = customer.City,
                              languageId = customer.LanguageId,
                              isDocumentVerified = customer.IsDocumentVerified,
                              documentNumber = customer.DocumentNumber,
                              documentIssuedBy = customer.DocumentIssuedBy,
                              sendPromotions = customer.SendPromotions,
                              affiliateReferralId = clickId,
                              creationTime = customer.CreationTime.ToString("yyyy-MM-dd HH:mm:ss"),
                              address = customer.Address,
                              realBalance = Math.Floor(BaseBll.GetObjectBalance((int)ObjectTypes.Client, customer.Id).Balances.Where(y => y.TypeId != (int)AccountTypes.ClientCompBalance &&
                                                                  y.TypeId != (int)AccountTypes.ClientCoinBalance &&
                                                                  y.TypeId != (int)AccountTypes.ClientBonusBalance).Sum(y => y.Balance) * 100) / 100,
                              lastSessionDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
                          }
                      });
                      reqList.Add(postData);
                      url = $"{url}/track";
                  }
                  else if (amount != 0)// deposit withdraw
                  {
                      if (isWithdraw)
                      {
                          postData = JsonConvert.SerializeObject(new
                          {
                              userId = customer.Id.ToString(),
                              type = "track",
                              @event = "Withdraw",
                              properties = new
                              {
                                  amount,
                                  transactionId,
                                  currency = customer.CurrencyId
                              }
                          });
                          reqList.Add(postData);
                          postData = JsonConvert.SerializeObject(new
                          {
                              userId = customer.Id.ToString(),
                              type = "track",
                              @event = "Update Profile",
                              properties = new
                              {
                                  realBalance = Math.Floor(BaseBll.GetObjectBalance((int)ObjectTypes.Client, customer.Id).Balances.Where(y => y.TypeId != (int)AccountTypes.ClientCompBalance &&
                                                                  y.TypeId != (int)AccountTypes.ClientCoinBalance &&
                                                                  y.TypeId != (int)AccountTypes.ClientBonusBalance).Sum(y => y.Balance) * 100) / 100
                              }
                          });
                          reqList.Add(postData);
                      }
                      else
                      {
                          postData = JsonConvert.SerializeObject(new
                          {
                              userId = customer.Id.ToString(),
                              type = "track",
                              @event = "Deposit",
                              properties = new
                              {
                                  amount,
                                  transactionId,
                                  currency = customer.CurrencyId,
                                  depositCount
                              }
                          });
                          reqList.Add(postData);
                          postData = JsonConvert.SerializeObject(new
                          {
                              userId = customer.Id.ToString(),
                              type = "track",
                              @event = "Update Profile",
                              properties = new
                              {
                                  realBalance = Math.Floor(BaseBll.GetObjectBalance((int)ObjectTypes.Client, customer.Id).Balances.Where(y => y.TypeId != (int)AccountTypes.ClientCompBalance &&
                                                                  y.TypeId != (int)AccountTypes.ClientCoinBalance &&
                                                                  y.TypeId != (int)AccountTypes.ClientBonusBalance).Sum(y => y.Balance) * 100) / 100,
                                  lastDepositDate = depositDate?.ToString("yyyy-MM-dd HH:mm:ss")
                              }
                          });
                          reqList.Add(postData);
                      }
                      url = $"{url}/track";
                  }
                  else // register
                  {
                      postData = JsonConvert.SerializeObject(new
                      {
                          userId = customer.Id.ToString(),
                          traits = new
                          {
                              partnerId = customer.PartnerId,
                              currencyId = customer.CurrencyId,
                              userName = customer.UserName,
                              email = customer.Email,
                              firstname = customer.FirstName,
                              lastName = customer.LastName,
                              gender = customer.Gender,
                              mobileNumber = customer.MobileNumber,
                              zipCode = customer.ZipCode,
                              birthDate = customer.BirthDate.ToString("yyyy-MM-dd HH:mm:ss"),
                              regionId = customer.RegionId,
                              categoryId = customer.CategoryId,
                              brandName = partnerName,
                              state = Enum.GetName(typeof(ClientStates), customer.State),
                              countryId = CacheManager.GetRegionById(customer.CountryId.Value, customer.LanguageId).IsoCode3,
                              city = customer.City,
                              languageId = customer.LanguageId,
                              isDocumentVerified = customer.IsDocumentVerified,
                              documentNumber = customer.DocumentNumber,
                              documentIssuedBy = customer.DocumentIssuedBy,
                              sendPromotions = customer.SendPromotions,
                              affiliateReferralId = clickId,
                              creationTime = customer.CreationTime.ToString("yyyy-MM-dd HH:mm:ss"),
                              address = customer.Address,
                              realBalance = 0
                          }

                      });
                      reqList.Add(postData);
                      url = $"{url}/identify";
                  }
                  var apikey = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.CustomerIoApiKey).StringValue;
                  foreach (var item in reqList)
                  {
                      var httpRequest = new HttpRequestInput
                      {
                          RequestMethod = Constants.HttpRequestMethods.Post,
                          ContentType = Constants.HttpContentTypes.ApplicationJson,
                          Url = url,
                          RequestHeaders = new Dictionary<string, string> { { "Authorization", "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes($"{apikey}:")) } },
                          PostData = item
                      };
                      Log.Info(JsonConvert.SerializeObject(httpRequest));
                      //  var am = CommonFunctions.SendHttpRequest(httpRequest, out _);
                      test = CommonFunctions.SendHttpRequest(httpRequest, out _);
                  }
                  return test;

              }
              catch (Exception e)
              {
                  Log.Error(e);
                  return e.Message;
              }
          }*/
    }
}