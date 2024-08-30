using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Helpers;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.DAL;
using IqSoft.CP.DAL.Filters;
using IqSoft.CP.DAL.Filters.Clients;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.DAL.Models.Clients;
using IqSoft.CP.DAL.Models.Job;
using Newtonsoft.Json;
using IqSoft.CP.DAL.Filters.Messages;
using IqSoft.CP.BLL.Interfaces;
using IqSoft.CP.Common.Models;
using IqSoft.CP.DAL.Models.Payments;
using System.IO;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.DAL.Models.Bonuses;
using IqSoft.CP.Common.Models.WebSiteModels.Clients;
using IqSoft.CP.DAL.Models.User;
using IqSoft.CP.Common.Models.Bonus;
using static IqSoft.CP.Common.Constants;
using System.Collections.Immutable;
using System.ServiceModel;
using IqSoft.CP.DAL.Models.Integration.ProductsIntegration;
using IqSoft.CP.DAL.Filters.Reporting;
using log4net;
using Microsoft.EntityFrameworkCore;
using IqSoft.CP.Common.Models.CacheModels;
using IqSoft.CP.Common.Models.Products;
using Microsoft.AspNetCore.Hosting;
using IqSoft.CP.DAL.Models.Document;
using IqSoft.CP.DAL.Models.Notification;

namespace IqSoft.CP.BLL.Services
{
    public class ClientBll : PermissionBll, IClientBll
    {
        private const string UserNameRegEx = "^[A-Za-z0-9._-]+$";
        #region Constructors

        public ClientBll(SessionIdentity identity, ILog log)
            : base(identity, log)
        {

        }

        public ClientBll(BaseBll baseBl)
            : base(baseBl)
        {

        }

        #endregion

        public Client RegisterClient(ClientRegistrationInput clientRegistrationInput, IWebHostEnvironment env)
        {
            using var regionBl = new RegionBll(Identity, Log);
            using var notificationBl = new NotificationBll(regionBl);
            using var bonusBl = new BonusService(regionBl);
            var partner = CacheManager.GetPartnerById(clientRegistrationInput.ClientData.PartnerId);
            var dbClient = new Client();
            AffiliatePlatform affPlatform = null;
            using (var scope = CommonFunctions.CreateTransactionScope())
            {
                var currentTime = GetServerDate();
                var rand = new Random();
                var salt = rand.Next();
                if (clientRegistrationInput.ReferralData != null)
                {
                    affPlatform = Db.AffiliatePlatforms.FirstOrDefault(x => x.Id == clientRegistrationInput.ReferralData.AffiliatePlatformId);
                    if (affPlatform == null || affPlatform.PartnerId != partner.Id)
                        throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);
                    if (affPlatform.Name.ToLower() == AffiliatePlatforms.Internal.ToLower())
                    {
                        clientRegistrationInput.ReferralData.RefId = CommonFunctions.GetRandomString(20);
                        clientRegistrationInput.ReferralData.LastProcessedBonusTime = currentTime;
                        clientRegistrationInput.ReferralData.Type = (int)AffiliateReferralTypes.WebsiteInvitation;

                        affPlatform = null;
                    }
                    else
                        clientRegistrationInput.ReferralData.Type = (int)AffiliateReferralTypes.ExternalAffiliatePlatform;

                    Db.AffiliateReferrals.Add(clientRegistrationInput.ReferralData);
                    Db.SaveChanges();
                    dbClient.AffiliateReferralId = clientRegistrationInput.ReferralData.Id;
                }
                else if (clientRegistrationInput.ClientData.RegionId > 0)
                {
                    var regionPath = regionBl.GetRegionPath(clientRegistrationInput.ClientData.RegionId);
                    var country = regionPath.FirstOrDefault(x => x.TypeId == (int)RegionTypes.Country);
                    if (country != null && !string.IsNullOrEmpty(country.IsoCode))
                    {
                        var internalAffiliate = Db.AffiliateReferrals.FirstOrDefault(x => x.AffiliatePlatform.Name == AffiliatePlatforms.Internal &&
                            x.AffiliatePlatform.PartnerId == clientRegistrationInput.ClientData.PartnerId && x.AffiliateId == country.IsoCode);

                        if (internalAffiliate != null)
                            dbClient.AffiliateReferralId = internalAffiliate.Id;
                    }
                }
                VerifyClientFields(clientRegistrationInput.ClientData, partner, clientRegistrationInput.ReCaptcha, clientRegistrationInput.IsFromAdmin);
                var isWelcomeBonusOn = !clientRegistrationInput.IsQuickRegistration && IsBonusTypeAvailableForPartner(partner.Id, (int)BonusTypes.SignupRealBonus);

                if (clientRegistrationInput.ClientData.RegionId == 0)
                {
                    clientRegistrationInput.ClientData.RegionId = Constants.DefaultRegionId;
                }
                if (string.IsNullOrEmpty(clientRegistrationInput.ClientData.LanguageId))
                    clientRegistrationInput.ClientData.LanguageId = LanguageId;
                if (string.IsNullOrEmpty(clientRegistrationInput.ClientData.CurrencyId))
                    clientRegistrationInput.ClientData.CurrencyId = partner.CurrencyId;
                MapClient(dbClient, clientRegistrationInput.ClientData, currentTime, salt);

                Db.Clients.Add(dbClient);
                if (clientRegistrationInput.ClientData.CategoryId == 0)
                    clientRegistrationInput.ClientData.CategoryId = (int)ClientCategories.New;

                Db.ClientClassifications.Add(new ClientClassification
                {
                    Client = dbClient,
                    CategoryId = clientRegistrationInput.ClientData.CategoryId,
                    ProductId = Constants.PlatformProductId,
                    SessionId = Identity.IsAdminUser ? Identity.SessionId : (long?)null,
                    LastUpdateTime = currentTime
                });
                if (!string.IsNullOrEmpty(clientRegistrationInput.PromoCode) && Int32.TryParse(clientRegistrationInput.PromoCode, out int managerId))
                {
                    var managerSetting = Db.ClientSettings.FirstOrDefault(x => x.ClientId == managerId && x.Name == Constants.ClientSettings.IsAffiliateManager);
                    if (managerSetting != null && managerSetting.StringValue == "false")
                        throw CreateException(LanguageId, Constants.Errors.PromoCodeNotExists);

                    var promoCodePlatform = Db.AffiliatePlatforms.FirstOrDefault(x => x.Name == AffiliatePlatforms.Internal && x.PartnerId == dbClient.PartnerId);
                    if (promoCodePlatform != null)
                    {
                        var affReferral = Db.AffiliateReferrals.FirstOrDefault(x => x.AffiliatePlatformId == promoCodePlatform.Id &&
                                                                                    x.AffiliateId == clientRegistrationInput.PromoCode &&
                                                                                    x.Type == (int)AffiliateReferralTypes.WebsiteInvitation);
                        if (affReferral == null)
                            affReferral = new AffiliateReferral
                            {
                                AffiliatePlatformId = promoCodePlatform.Id,
                                AffiliateId = clientRegistrationInput.PromoCode,
                                RefId = CommonFunctions.GetRandomString(20),
                                Type = (int)AffiliateReferralTypes.WebsiteInvitation,
                                LastProcessedBonusTime = currentTime
                            };
                        dbClient.AffiliateReferral = affReferral;
                    }
                }
                if (clientRegistrationInput.SecurityQuestions != null && clientRegistrationInput.SecurityQuestions.Any())
                {
                    foreach (var sq in clientRegistrationInput.SecurityQuestions)
                    {
                        Db.ClientSecurityAnswers.Add(new ClientSecurityAnswer
                        {
                            ClientId = dbClient.Id,
                            SecurityQuestionId = sq.Id,
                            Answer = sq.Answer
                        });
                    }
                }
                Db.SaveChanges();
                UploadRegistrationDocuments(dbClient.Id, clientRegistrationInput, env);
                if (clientRegistrationInput.GeneratedUsername)
                {
                    dbClient.UserName = "U" + dbClient.Id;
                    Db.SaveChanges();
                }
                AutoClaim(bonusBl, dbClient.Id, (int)TriggerTypes.SignUp,
                          clientRegistrationInput.PromoCode, null, out int awardedStatus, 0, null);
                if (isWelcomeBonusOn)
                    dbClient.WelcomeBonusActivationKey = CreateClientInfo(dbClient.Id, dbClient.PartnerId, (int)ClientInfoTypes.WelcomeBonusActivationKey);
                var currDate = currentTime.Year * 10000 + currentTime.Month * 100 + currentTime.Day;
                var clientettings = new ClientCustomSettings
                {
                    ClientId = dbClient.Id,
                    TermsConditionsAcceptanceVersion = CacheManager.GetConfigKey(dbClient.PartnerId, Constants.PartnerKeys.TermsConditionVersion),
                    PasswordChangedDate = currDate,
                    ReferralType = clientRegistrationInput.ReferralType
                };
                SaveClientSetting(clientettings);
                var account = new Account
                {
                    ObjectId = dbClient.Id,
                    ObjectTypeId = (int)ObjectTypes.Client,
                    TypeId = (int)AccountTypes.ClientUnusedBalance,
                    Balance = 0,
                    CurrencyId = dbClient.CurrencyId,
                    SessionId = Identity.IsAdminUser ? Identity.SessionId : (long?)null,
                    CreationTime = currentTime,
                    LastUpdateTime = currentTime
                };
                Db.Accounts.Add(account);
                Db.SaveChanges();
                scope.Complete();
            }
            if (!clientRegistrationInput.IsQuickRegistration)
            {
                SendRegistrationNotifications(dbClient, notificationBl);
            }
            if (affPlatform != null)
            {
                notificationBl.RegistrationNotification(dbClient.PartnerId, dbClient.Id, affPlatform, clientRegistrationInput.ReferralData.RefId, clientRegistrationInput.ReferralData.AffiliateId);
            }
            AddClientJobTrigger(dbClient.Id, (int)JobTriggerTypes.ReconsiderSegments);
            return dbClient;
        }

        public Affiliate RegisterAffiliate(ClientRegistrationInput clientRegistrationInput)
        {
            using (var regionBl = new RegionBll(Identity, Log))
            {
                using (var notificationBl = new NotificationBll(regionBl))
                {
                    using (var bonusBl = new BonusService(regionBl))
                    {
                        var partner = CacheManager.GetPartnerById(clientRegistrationInput.ClientData.PartnerId);

                        var dbAffiliate = new Affiliate();
                        using (var scope = CommonFunctions.CreateTransactionScope())
                        {
                            var currentTime = GetServerDate();
                            var rand = new Random();
                            var salt = rand.Next();

                            VerifyAffiliateFields(clientRegistrationInput.ClientData, partner, clientRegistrationInput.ReCaptcha, clientRegistrationInput.IsFromAdmin);
                            if (clientRegistrationInput.ClientData.RegionId == 0)
                                clientRegistrationInput.ClientData.RegionId = Constants.DefaultRegionId;
                            if (string.IsNullOrEmpty(clientRegistrationInput.ClientData.LanguageId))
                                clientRegistrationInput.ClientData.LanguageId = LanguageId;
                            if (string.IsNullOrEmpty(clientRegistrationInput.ClientData.CurrencyId))
                                clientRegistrationInput.ClientData.CurrencyId = partner.CurrencyId;
                            MapAffiliate(dbAffiliate, clientRegistrationInput.ClientData, currentTime, salt);

                            Db.Affiliates.Add(dbAffiliate);
                            if (clientRegistrationInput.ClientData.CategoryId == 0)
                                clientRegistrationInput.ClientData.CategoryId = (int)ClientCategories.New;
                            Db.SaveChanges();
                            var currDate = currentTime.Year * 10000 + currentTime.Month * 100 + currentTime.Day;
                            var clientettings = new ClientCustomSettings
                            {
                                ClientId = dbAffiliate.Id,
                                TermsConditionsAcceptanceVersion = CacheManager.GetConfigKey(dbAffiliate.PartnerId, Constants.PartnerKeys.TermsConditionVersion),
                                PasswordChangedDate = currDate
                            };
                            SaveClientSetting(clientettings);
                            var account = new Account
                            {
                                ObjectId = dbAffiliate.Id,
                                ObjectTypeId = (int)ObjectTypes.Client,
                                TypeId = (int)AccountTypes.ClientUnusedBalance,
                                Balance = 0,
                                CurrencyId = dbAffiliate.CurrencyId,
                                SessionId = Identity.IsAdminUser ? Identity.SessionId : (long?)null,
                                CreationTime = currentTime,
                                LastUpdateTime = currentTime
                            };
                            Db.Accounts.Add(account);
                            Db.SaveChanges();
                            scope.Complete();
                        }
                        //notificationBl.SendNotificationMessage(dbAffiliate.Id, ClientInfoTypes.SuccessRegistrationTicket);
                        return dbAffiliate;
                    }
                }
            }
        }

        private void MapClient(Client target, Client source, DateTime currentTime, int salt)
        {
            target.CreationTime = currentTime;
            target.Email = source.Email;
            target.IsEmailVerified = source.IsEmailVerified;
            target.CurrencyId = source.CurrencyId;
            target.UserName = !string.IsNullOrWhiteSpace(source.UserName) ? source.UserName : source.Email.Replace("@", "").Replace(".", "");
            target.PasswordHash = CommonFunctions.ComputeClientPasswordHash(source.Password, salt);
            target.Salt = salt;
            target.PartnerId = source.PartnerId;
            target.Gender = source.Gender == 0 ? (int)Gender.Male : source.Gender;
            target.BirthDate = source.BirthDate;
            target.SendMail = source.SendMail;
            target.SendSms = source.SendSms;
            target.CallToPhone = source.CallToPhone;
            target.SendPromotions = source.SendPromotions;
            target.RegionId = source.RegionId;
            target.IsDocumentVerified = false;
            target.IsMobileNumberVerified = source.IsMobileNumberVerified;
            target.HasNote = false;
            target.State = Enum.IsDefined(typeof(ClientStates), source.State) ? source.State : (int)ClientStates.Active;
            target.CategoryId = source.CategoryId == 0 ? (int)ClientCategories.New : source.CategoryId;
            target.LastUpdateTime = currentTime;
            target.FirstName = string.IsNullOrEmpty(source.FirstName) ? string.Empty : source.FirstName;
            target.LastName = string.IsNullOrEmpty(source.LastName) ? string.Empty : source.LastName;
            target.NickName = string.IsNullOrEmpty(source.NickName) ? string.Empty : source.NickName;
            target.RegistrationIp = source.RegistrationIp;
            target.DocumentNumber = source.DocumentNumber == null ? string.Empty : source.DocumentNumber.Trim();
            target.DocumentType = source.DocumentType;
            target.DocumentIssuedBy = string.IsNullOrEmpty(source.DocumentIssuedBy) ? string.Empty : source.DocumentIssuedBy;
            target.Address = string.IsNullOrEmpty(source.Address) ? string.Empty : source.Address;
            target.MobileNumber = string.IsNullOrEmpty(source.MobileNumber) ? string.Empty : source.MobileNumber.Replace(" ", string.Empty);
            target.PhoneNumber = string.IsNullOrEmpty(source.PhoneNumber) ? string.Empty : source.PhoneNumber;
            target.LanguageId = string.IsNullOrEmpty(source.LanguageId) ? Constants.DefaultLanguageId : source.LanguageId;
            target.ZipCode = string.IsNullOrEmpty(source.ZipCode) ? string.Empty : source.ZipCode;
            target.City = source.City;
            target.UserId = source.UserId;
            target.Citizenship = source.Citizenship;
            target.JobArea = source.JobArea;
            target.Apartment = source.Apartment;
            target.BuildingNumber = source.BuildingNumber;
            target.Info = source.Info;
        }

        private void MapAffiliate(Affiliate target, Client source, DateTime currentTime, int salt)
        {
            target.CreationTime = currentTime;
            target.Email = source.Email;
            target.CurrencyId = source.CurrencyId;
            target.PasswordHash = CommonFunctions.ComputeClientPasswordHash(source.Password, salt);
            target.Salt = salt;
            target.PartnerId = source.PartnerId;
            target.Gender = source.Gender ?? (int)Gender.Male;
            target.RegionId = source.RegionId;
            target.State = Enum.IsDefined(typeof(ClientStates), source.State) ? source.State : (int)ClientStates.Active;
            target.LastUpdateTime = currentTime;
            target.FirstName = string.IsNullOrEmpty(source.FirstName) ? string.Empty : source.FirstName;
            target.LastName = string.IsNullOrEmpty(source.LastName) ? string.Empty : source.LastName;
            target.NickName = string.IsNullOrEmpty(source.NickName) ? string.Empty : source.NickName;
            target.MobileNumber = string.IsNullOrEmpty(source.MobileNumber) ? string.Empty : source.MobileNumber.Replace(" ", string.Empty);
            target.LanguageId = string.IsNullOrEmpty(source.LanguageId) ? Constants.DefaultLanguageId : source.LanguageId;

        }

        public Client RegisterClient(Client client)
        {
            var currentTime = DateTime.UtcNow;
            client.CreationTime = currentTime;
            client.IsEmailVerified = false;
            client.PasswordHash = string.Empty;
            client.Salt = 0;
            client.SendMail = true;
            client.SendSms = false;
            client.CallToPhone = false;
            client.SendPromotions = false;
            client.RegionId = Constants.DefaultRegionId;
            client.IsDocumentVerified = false;
            client.IsMobileNumberVerified = false;
            client.HasNote = false;
            client.State = (int)ClientStates.Active;
            client.CategoryId = (int)ClientCategories.New;
            client.LastUpdateTime = currentTime;
            client.FirstName = string.IsNullOrWhiteSpace(client.FirstName) ? string.Empty : client.FirstName;
            client.LastName = string.IsNullOrWhiteSpace(client.LastName) ? string.Empty : client.LastName;
            client.NickName = string.IsNullOrWhiteSpace(client.NickName) ? string.Empty : client.NickName;
            client.Email = string.IsNullOrWhiteSpace(client.Email) ? string.Empty : client.Email;
            client.RegistrationIp = Constants.DefaultIp;
            client.DocumentNumber = string.Empty;
            client.DocumentIssuedBy = string.Empty;
            client.Address = string.Empty;
            client.MobileNumber = string.Empty;
            client.PhoneNumber = string.Empty;
            client.LanguageId = Constants.DefaultLanguageId;
            client.ZipCode = string.Empty;


            Db.Clients.Add(client);
            Db.SaveChanges();

            Db.ClientClassifications.Add(new ClientClassification
            {
                ClientId = client.Id,
                CategoryId = client.CategoryId == 0 ? (int)ClientCategories.New : client.CategoryId,
                ProductId = Constants.PlatformProductId,
                SessionId = null,
                LastUpdateTime = currentTime
            });
            var account = new Account
            {
                ObjectId = client.Id,
                ObjectTypeId = (int)ObjectTypes.Client,
                TypeId = (int)AccountTypes.ClientUnusedBalance,
                Balance = 0,
                CurrencyId = client.CurrencyId,
                SessionId = null,
                CreationTime = currentTime,
                LastUpdateTime = currentTime
            };
            Db.Accounts.Add(account);
            Db.SaveChanges();

            return client;
        }

        public Client InsertClient(Client client)
        {
            var currentTime = DateTime.UtcNow;
            client.CreationTime = currentTime;

            Db.Clients.Add(client);
            Db.SaveChanges();

            Db.ClientClassifications.Add(new ClientClassification
            {
                ClientId = client.Id,
                CategoryId = client.CategoryId == 0 ? (int)ClientCategories.New : client.CategoryId,
                ProductId = Constants.PlatformProductId,
                SessionId = null,
                LastUpdateTime = currentTime
            });
            var account = new Account
            {
                ObjectId = client.Id,
                ObjectTypeId = (int)ObjectTypes.Client,
                TypeId = (int)AccountTypes.ClientUnusedBalance,
                Balance = 0,
                CurrencyId = client.CurrencyId,
                SessionId = null,
                CreationTime = currentTime,
                LastUpdateTime = currentTime
            };
            Db.Accounts.Add(account);
            Db.SaveChanges();

            return client;
        }


        private void UploadRegistrationDocuments(int clientId, ClientRegistrationInput clientRegistrationInput, IWebHostEnvironment env)
        {
            if (clientRegistrationInput.IdCardDocumentByte != null)
                SaveKYCDocument(new ClientIdentity
                {
                    ClientId = clientId,
                    DocumentTypeId = (int)KYCDocumentTypes.IDCard,
                    Status = (int)KYCDocumentStates.InProcess
                }, string.Format("{0}.png", nameof(KYCDocumentTypes.IDCard)), clientRegistrationInput.IdCardDocumentByte, false, env);
            if (clientRegistrationInput.UtilityBillDocumentByte != null)
                SaveKYCDocument(new ClientIdentity
                {
                    ClientId = clientId,
                    DocumentTypeId = (int)KYCDocumentTypes.UtilityBill,
                    Status = (int)KYCDocumentStates.InProcess
                }, string.Format("{0}.png", nameof(KYCDocumentTypes.UtilityBill)), clientRegistrationInput.UtilityBillDocumentByte, false, env);
            if (clientRegistrationInput.PassportDocumentByte != null)
                SaveKYCDocument(new ClientIdentity
                {
                    ClientId = clientId,
                    DocumentTypeId = (int)KYCDocumentTypes.Passport,
                    Status = (int)KYCDocumentStates.InProcess
                }, string.Format("{0}.png", nameof(KYCDocumentTypes.Passport)), clientRegistrationInput.PassportDocumentByte, false, env);
            if (clientRegistrationInput.DriverLicenseDocumentByte != null)
                SaveKYCDocument(new ClientIdentity
                {
                    ClientId = clientId,
                    DocumentTypeId = (int)KYCDocumentTypes.DriverLicense,
                    Status = (int)KYCDocumentStates.InProcess
                }, string.Format("{0}.png", nameof(KYCDocumentTypes.DriverLicense)), clientRegistrationInput.DriverLicenseDocumentByte, false, env);
        }

        private void SendRegistrationNotifications(Client client, NotificationBll notificationBl)
        {
            var sendRegSMS = false;
            var sendRegEmail = false;
            var partnerConfig = CacheManager.GetConfigKey(client.PartnerId, Constants.PartnerKeys.SendRegistrationSMS);
            if (!string.IsNullOrEmpty(partnerConfig) && partnerConfig == "1")
                sendRegSMS = true;
            partnerConfig = CacheManager.GetConfigKey(client.PartnerId, Constants.PartnerKeys.SendRegistrationEmail);
            if (!string.IsNullOrEmpty(partnerConfig) && partnerConfig == "1")
                sendRegEmail = true;

            if (sendRegSMS)
            {
                if (!client.IsMobileNumberVerified)
                    notificationBl.SendVerificationCodeToMobileNumber(client.Id, client.MobileNumber);
                else if (!sendRegEmail || client.IsEmailVerified)
                {
                    notificationBl.SendNotificationMessage(new NotificationModel
                    {
                        PartnerId = client.PartnerId,
                        ClientId = client.Id,
                        MobileOrEmail =  client.MobileNumber,
                        ClientInfoType = (int)ClientInfoTypes.SuccessRegistrationSMS
                    });
                }
            }
            if (sendRegEmail)
            {
                if (!client.IsEmailVerified)
                    notificationBl.SendVerificationCodeToEmail(client.Id, client.Email);
                else if (!sendRegSMS || client.IsMobileNumberVerified)
                {
                    notificationBl.SendNotificationMessage(new NotificationModel
                    {
                        PartnerId = client.PartnerId,
                        ClientId = client.Id,
                        MobileOrEmail =  client.Email,
                        ClientInfoType = (int)ClientInfoTypes.SuccessRegistrationEmail,
                        SubjectType = (int)ClientInfoTypes.SuccessRegistrationEmailSubject
                    });
                }
            }
            partnerConfig = CacheManager.GetConfigKey(client.PartnerId, Constants.PartnerKeys.PartnerKYCTypes);
            if (!string.IsNullOrEmpty(partnerConfig))
            {
                var requiredDocumentTypes = JsonConvert.DeserializeObject<List<int>>(partnerConfig);
                if (requiredDocumentTypes.Any())
                {
                    var sendSMS = false;
                    var sendEmail = false;
                    partnerConfig = CacheManager.GetConfigKey(client.PartnerId, Constants.PartnerKeys.SendWaitingKYCDocumentSMS);
                    if (!string.IsNullOrEmpty(partnerConfig) && partnerConfig == "1" && client.IsMobileNumberVerified)
                        sendSMS = true;
                    partnerConfig = CacheManager.GetConfigKey(client.PartnerId, Constants.PartnerKeys.SendWaitingKYCDocumentEmail);
                    if (!string.IsNullOrEmpty(partnerConfig) && partnerConfig == "1")
                        sendEmail = true;
                    var waitingNewDocuments = !Db.ClientIdentities.Where(x => x.ClientId == client.Id && requiredDocumentTypes.Contains(x.DocumentTypeId) &&
                                                                                (x.Status == (int)KYCDocumentStates.InProcess ||
                                                                                 x.Status == (int)KYCDocumentStates.Approved))
                                                                  .Select(x => x.DocumentTypeId).OrderBy(x => x).ToList()
                                                                  .Intersect(requiredDocumentTypes).SequenceEqual(requiredDocumentTypes);
                    var notificationModel = new NotificationModel
                    {
                        PartnerId = client.PartnerId,
                        ClientId = client.Id,
                    };
                    if (waitingNewDocuments)
                    {

                        if (sendSMS)
                        {
                            notificationModel.MobileOrEmail = client.MobileNumber;
                            notificationModel.ClientInfoType = (int)ClientInfoTypes.WaitingKYCDocumentSMS;
                            notificationBl.SendNotificationMessage(notificationModel);
                        }
                        if (sendEmail)
                        {
                            notificationModel.MobileOrEmail = client.Email;
                            notificationModel.ClientInfoType = (int)ClientInfoTypes.WaitingKYCDocumentEmail;
                            notificationBl.SendNotificationMessage(notificationModel);
                        }
                        notificationBl.SendInternalTicket(client.Id, (int)ClientInfoTypes.WaitingKYCDocumentTicket);
                    }
                    else
                    {
                        if (sendSMS)
                        {
                            notificationModel.MobileOrEmail = client.MobileNumber;
                            notificationModel.ClientInfoType = (int)ClientInfoTypes.PendingKYCVerificationSMS;
                            notificationBl.SendNotificationMessage(notificationModel);
                        }
                        if (sendEmail)
                        {
                            notificationModel.MobileOrEmail = client.Email;
                            notificationModel.ClientInfoType = (int)ClientInfoTypes.PendingKYCVerificationEmail;
                            notificationBl.SendNotificationMessage(notificationModel);
                        }
                        notificationBl.SendInternalTicket(client.Id, (int)ClientInfoTypes.PendingKYCVerificationTicket);
                    }
                }
            }
            notificationBl.SendInternalTicket(client.Id, (int)ClientInfoTypes.SuccessRegistrationTicket);
        }

        public List<int> VerifyClientMobileNumber(string key, string mobileNumber, int? clientId, int partnerId,
                                                  bool expire, List<Common.Models.WebSiteModels.SecurityQuestion> securityQuestions, bool checkSecQuestions = true)
        {
            if (clientId.HasValue)
            {
                var client = Db.Clients.First(x => x.Id == clientId);
                if (string.IsNullOrWhiteSpace(client.MobileNumber))
                    throw CreateException(LanguageId, Constants.Errors.MobileNumberCantBeEmpty);
                var clientInfo = Db.ClientInfoes.FirstOrDefault(x => x.Data == key && x.ClientId == client.Id && x.Type == (int)ClientInfoTypes.MobileVerificationKey);
                if (clientInfo == null || clientInfo.Data != key)
                    throw CreateException(LanguageId, Constants.Errors.WrongVerificationKey);
                if (clientInfo.State == (int)ClientInfoStates.Expired)
                    throw CreateException(LanguageId, Constants.Errors.VerificationKeyExpired);
                if (checkSecQuestions)
                {
                    CheckClientSecurityAnswers(client.Id, Identity.LanguageId, securityQuestions);
                    if (!client.IsMobileNumberVerified)
                    {
                        client.IsMobileNumberVerified = true;
                        client.LastUpdateTime = DateTime.UtcNow;
                        clientInfo.State = (int)ClientInfoStates.Expired;
                    }
                    Db.SaveChanges();
                }
            }
            else
            {
                var clientInfo = Db.ClientInfoes.OrderByDescending(x => x.Id)
                                                .FirstOrDefault(x => x.Data == key && x.MobileOrEmail == mobileNumber && x.PartnerId == partnerId &&
                                                                     (x.Type == (int)ClientInfoTypes.MobileVerificationKey || x.Type == (int)ClientInfoTypes.PasswordRecoveryMobileKey));
                if (clientInfo == null || clientInfo.Data != key)
                    throw CreateException(LanguageId, Constants.Errors.WrongVerificationKey);
                if (clientInfo.State == (int)ClientInfoStates.Expired)
                    throw CreateException(LanguageId, Constants.Errors.VerificationKeyExpired);
                if (expire)
                    clientInfo.State = (int)ClientInfoStates.Expired;
                else
                    clientInfo.State = (int)ClientInfoStates.Verified;
                Db.SaveChanges();
                if (clientInfo.ClientId != null)
                    return Db.ClientSecurityAnswers.Where(x => x.ClientId == clientInfo.ClientId).Select(x => x.SecurityQuestionId).ToList();
            }

            return new List<int>();
        }

        public List<int> VerifyClientEmail(string code, string email, int? clientId, int partnerId, bool expire,
                                           List<Common.Models.WebSiteModels.SecurityQuestion> securityQuestions, bool checkSecQuestions = true)
        {
            if (clientId.HasValue)
            {
                var client = Db.Clients.First(x => x.Id == clientId);

                if (string.IsNullOrWhiteSpace(client.Email))
                    throw CreateException(LanguageId, Constants.Errors.EmailCantBeEmpty);
                var clientInfo = Db.ClientInfoes.FirstOrDefault(x => x.Data == code && x.ClientId == client.Id && x.Type == (int)ClientInfoTypes.EmailVerificationKey);
                if (clientInfo == null || clientInfo.Data != code)
                    throw CreateException(LanguageId, Constants.Errors.WrongVerificationKey);
                if (clientInfo.State == (int)ClientInfoStates.Expired)
                    throw CreateException(LanguageId, Constants.Errors.VerificationKeyExpired);
                if (checkSecQuestions)
                {
                    CheckClientSecurityAnswers(client.Id, Identity.LanguageId, securityQuestions);
                    if (!client.IsEmailVerified)
                    {
                        client.IsEmailVerified = true;
                        client.LastUpdateTime = DateTime.UtcNow;
                        clientInfo.State = (int)ClientInfoStates.Expired;
                        Db.SaveChanges();
                    }
                }
            }
            else
            {
                var clientInfo = Db.ClientInfoes.OrderByDescending(x => x.Id)
                                                .FirstOrDefault(x => x.Data == code && x.MobileOrEmail == email && x.PartnerId == partnerId &&
                                                                     (x.Type == (int)ClientInfoTypes.EmailVerificationKey || x.Type == (int)ClientInfoTypes.PasswordRecoveryEmailKey));

                if (clientInfo == null || clientInfo.Data != code)
                    throw CreateException(LanguageId, Constants.Errors.WrongVerificationKey);
                if (clientInfo.State == (int)ClientInfoStates.Expired)
                    throw CreateException(LanguageId, Constants.Errors.VerificationKeyExpired);
                if (expire)
                    clientInfo.State = (int)ClientInfoStates.Expired;
                else
                    clientInfo.State = (int)ClientInfoStates.Verified;
                Db.SaveChanges();
                if (clientInfo.ClientId != null)
                    return Db.ClientSecurityAnswers.Where(x => x.ClientId == clientInfo.ClientId).Select(x => x.SecurityQuestionId).ToList();
            }
            return new List<int>();
        }
        /*
        public List<int> VerifyRecoveryCode(string code, int clientId, int partnerId, bool expire, out string mobileOrEmail)
        {
            var client = CacheManager.GetClientById(clientId);
            if (client == null)
                throw BaseBll.CreateException(LanguageId, Constants.Errors.ClientNotFound);

            var clientInfo = Db.ClientInfoes.OrderByDescending(x => x.Id)
                                                .FirstOrDefault(x => x.Data == code && x.PartnerId == partnerId &&
                                                (x.MobileOrEmail == client.Email || x.MobileOrEmail == client.MobileNumber)
                                                && (x.Type == (int)ClientInfoTypes.ResetPasswordEmail || x.Type == (int)ClientInfoTypes.ResetPasswordSMS));

            if (clientInfo == null || clientInfo.Data != code)
                throw CreateException(LanguageId, Constants.Errors.WrongVerificationKey);
            if (clientInfo.State == (int)ClientInfoStates.Expired)
                throw CreateException(LanguageId, Constants.Errors.VerificationKeyExpired);
            if (expire)
                clientInfo.State = (int)ClientInfoStates.Expired;
            else
                clientInfo.State = (int)ClientInfoStates.Verified;
            Db.SaveChanges();
            mobileOrEmail = clientInfo.MobileOrEmail;
            if (clientInfo.ClientId != null)
                return Db.ClientSecurityAnswers.Where(x => x.ClientId == clientInfo.ClientId).Select(x => x.SecurityQuestionId).ToList();
            return new List<int>();
        }

        */
        public Client GetClientById(int id, bool checkPermission = false)
        {
            var client = Db.Clients.Include(x => x.AffiliateReferral).FirstOrDefault(x => x.Id == id);
            if (client == null)
                throw CreateException(LanguageId, Constants.Errors.ClientNotFound);
            if (checkPermission)
            {
                var checkClientPermission = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewClient,
                    ObjectTypeId = (int)ObjectTypes.Client
                });

                var checkPartnerPermission = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewPartner,
                    ObjectTypeId = (int)ObjectTypes.Partner
                });

                var affiliateReferralAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewAffiliateReferral,
                    ObjectTypeId = (int)ObjectTypes.AffiliateReferral
                });

                if ((!checkClientPermission.HaveAccessForAllObjects && checkClientPermission.AccessibleObjects.All(x => x != id)) ||
                     (!checkPartnerPermission.HaveAccessForAllObjects && checkPartnerPermission.AccessibleObjects.All(x => x != client.PartnerId)) ||
                     (!affiliateReferralAccess.HaveAccessForAllObjects &&
                     affiliateReferralAccess.AccessibleObjects.All(x => client.AffiliateReferralId.HasValue && x != client.AffiliateReferralId.Value))
                   )
                    throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            }
            var parentState = CacheManager.GetClientSettingByName(client.Id, ClientSettings.ParentState);
            if (parentState.NumericValue.HasValue && CustomHelper.Greater((ClientStates)parentState.NumericValue.Value, (ClientStates)client.State))
                client.State = Convert.ToInt32(parentState.NumericValue.Value);
            return client;
        }

        public Client GetClientByUserName(int partnerId, string userName)
        {
            var resp = Db.Clients.FirstOrDefault(x => x.PartnerId == partnerId && x.UserName == userName);
            return resp;
        }

        public List<Client> GetClients(FilterClient filter)
        {
            CreateFilterForGetClients(filter);

            return filter.FilterObjects(Db.Clients).ToList();
        }

        public PagedModel<fnClient> GetfnClientsPagedModel(FilterfnClient filter, bool checkPermission)
        {
            if (checkPermission)
                CreateFilterForGetfnClients(filter);

            Func<IQueryable<fnClient>, IOrderedQueryable<fnClient>> orderBy;

            if (filter.OrderBy.HasValue)
            {
                if (filter.OrderBy.Value)
                {
                    orderBy = QueryableUtilsHelper.OrderByFunc<fnClient>(filter.FieldNameToOrderBy, true);
                }
                else
                {
                    orderBy = QueryableUtilsHelper.OrderByFunc<fnClient>(filter.FieldNameToOrderBy, false);
                }
            }
            else
            {
                orderBy = clients => clients.OrderByDescending(x => x.Id);
            }

            return new PagedModel<fnClient>
            {
                Entities = filter.FilterObjects(Db.fn_Client(), orderBy),
                Count = filter.SelectedObjectsCount(Db.fn_Client())
            };
        }

        public List<Client> GetAgentClients(FilterClientModel filter, int agentId, bool withDownlines, int? clientId)
        {
            var query = Db.Clients.Include(x => x.User).Include(x => x.AgentCommissions).AsQueryable();
            if (clientId.HasValue)
                query = query.Where(x => x.Id == clientId);
            if (!withDownlines)
                query=  query.Where(x => x.User.Path.Contains("/" + agentId + "/"));
            else
                query =  query.Where(x => x.UserId == agentId);
            Func<IQueryable<Client>, IOrderedQueryable<Client>> orderBy;

            if (filter.OrderBy.HasValue)
            {
                if (filter.OrderBy.Value)
                {
                    orderBy = QueryableUtilsHelper.OrderByFunc<Client>(filter.FieldNameToOrderBy, true);
                }
                else
                {
                    orderBy = QueryableUtilsHelper.OrderByFunc<Client>(filter.FieldNameToOrderBy, false);
                }
            }
            else
            {
                orderBy = clients => clients.OrderByDescending(x => x.Id);
            }
            return filter.FilterObjects(query, orderBy).ToList();
        }

        public BllClient ProductsAuthorization(string token, out string newToken, out int productId, out string languageId, bool expireOld = false)
        {
            if (string.IsNullOrEmpty(token))
                throw CreateException(LanguageId, Constants.Errors.WrongToken);
            var session = GetClientProductSession(token, LanguageId, null, false);
            if (session.ProductId == Constants.PlatformProductId)
                throw CreateException(LanguageId, Constants.Errors.SessionNotFound);

            var client = CacheManager.GetClientById(session.Id);
            newToken = token;

            var clientCategory = CacheManager.GetClientCategory(client.CategoryId);
            if (clientCategory != null)
                client.CategoryName = clientCategory.Name;

            if (expireOld)
            {
                newToken = CreateNewProductToken(session);
            }
            productId = session.ProductId;
            languageId = session.LanguageId;
            return client;
        }

        public BllClient PlatformAuthorization(string token, out SessionIdentity session)
        {
            if (string.IsNullOrEmpty(token))
                throw CreateException(LanguageId, Constants.Errors.WrongToken);
            session = GetClientProductSession(token, LanguageId, null, true);
            var client = CacheManager.GetClientById(session.Id);
            var clientCategory = CacheManager.GetClientCategory(client.CategoryId);
            if (clientCategory != null)
                client.CategoryName = clientCategory.Name;

            return client;
        }

        public string CreateNewProductToken(SessionIdentity session, string newToken = "")
        {
            var currentDate = GetServerDate();
            var newSession = new ClientSession
            {
                ClientId = session.Id,
                LanguageId = session.LanguageId,
                ProductId = session.ProductId,
                Ip = session.LoginIp,
                DeviceType = session.DeviceType,
                ParentId = session.ParentId
            };
            var savedSession = AddClientSession(newSession, newToken);
            var oldSession = Db.ClientSessions.FirstOrDefault(x => x.Id == session.SessionId);
            if (oldSession != null)
            {
                oldSession.State = (int)SessionStates.Inactive;
                oldSession.LastUpdateTime = currentDate;
                oldSession.EndTime = currentDate;
                oldSession.LogoutType = (int)LogoutTypes.System;
            }
            Db.SaveChanges();
            return savedSession.Token;
        }

        public List<ClientSession> GetClientSessionInfo(long sessionId)
        {
            return Db.ClientSessions.Where(x => x.ParentId == sessionId).ToList();
        }

        public BllClient GetClientByToken(string token, out ClientLoginOut clientLoginOut, string languageId)
        {
            var session = CacheManager.GetClientSessionByToken(token, Constants.PlatformProductId);
            if (session == null)
                throw CreateException(languageId, Constants.Errors.SessionNotFound);
            if (session.State == (int)SessionStates.Inactive)
                throw CreateException(languageId, Constants.Errors.SessionExpired, integerInfo: session.LogoutType);
            var client = CacheManager.GetClientById(session.ClientId);
            var currency = CacheManager.GetCurrencyById(client.CurrencyId);
            client.CurrencySymbol = currency.Symbol;
            clientLoginOut = new ClientLoginOut { NewToken = token };
            var currentTime = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(languageId) && languageId != session.LanguageId)
            {
                Db.ClientSessions.Where(x => x.Id == session.Id).UpdateFromQuery(x => new ClientSession
                { State = (int)SessionStates.Inactive, EndTime = currentTime, LogoutType = (int)LogoutTypes.System });
                var newSession = new ClientSession
                {
                    ClientId = session.ClientId,
                    Country = session.Country,
                    DeviceType = session.DeviceType,
                    ExternalToken = session.ExternalToken,
                    ProductId = Constants.PlatformProductId,
                    LanguageId = languageId,
                    Ip = session.Ip,
                    State = (int)SessionStates.Active,
                    StartTime = currentTime,
                    LastUpdateTime = currentTime,
                    Token = GetToken()
                };
                Db.ClientSessions.Add(newSession);
                Db.SaveChanges();
                CacheManager.RemoveClientPlatformSession(newSession.ClientId);
                CacheManager.RemoveClientProductSession(token, Constants.PlatformProductId);
                clientLoginOut.NewToken = newSession.Token;
            }

            GetClientRegionInfo(client.RegionId, ref clientLoginOut);
            var showLastLoginInfo = CacheManager.GetConfigKey(client.PartnerId, Constants.PartnerKeys.ShowLastLoginInfo);
            if (!string.IsNullOrEmpty(showLastLoginInfo) && client.LastSessionId.HasValue)
            {
                var showLoginInfoConfig = JsonConvert.DeserializeObject<ShowLoginInfoConfig>(showLastLoginInfo);
                clientLoginOut.LastSession = CacheManager.GetClientPlatformSessionById(client.LastSessionId.Value);
                if (!showLoginInfoConfig.ShowStartTime.HasValue || !showLoginInfoConfig.ShowStartTime.Value)
                    clientLoginOut.LastSession.StartTime = null;
                if (!showLoginInfoConfig.ShowEndTime.HasValue || !showLoginInfoConfig.ShowEndTime.Value)
                    clientLoginOut.LastSession.EndTime = null;
                if (!showLoginInfoConfig.ShowIp.HasValue || !showLoginInfoConfig.ShowIp.Value)
                    clientLoginOut.LastSession.Ip = null;
            }
            var clientPasswordExpiryPeriod = CacheManager.GetConfigKey(client.PartnerId, Constants.PartnerKeys.ClientPasswordExpiryPeriod);
            if (clientPasswordExpiryPeriod != null && Int32.TryParse(clientPasswordExpiryPeriod, out int period) && period > 0)
            {
                var currentDate = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day);
                var clientPasswordChangedDate = CacheManager.GetClientSettingByName(client.Id, ClientSettings.PasswordChangedDate);
                if (clientPasswordChangedDate == null || clientPasswordChangedDate.Id == 0)
                    clientLoginOut.ResetPassword = true;
                else
                {
                    var date = Convert.ToInt32(clientPasswordChangedDate.NumericValue);
                    if ((currentDate - new DateTime(date / 10000, (date / 100) % 100, date % 100)).TotalDays >= period)
                        clientLoginOut.ResetPassword = true;
                }
            }
            var termsConditionVersion = CacheManager.GetConfigKey(client.PartnerId, Constants.PartnerKeys.TermsConditionVersion);
            if (!string.IsNullOrEmpty(termsConditionVersion))
            {
                var termsConditionsAcceptanceVersion = CacheManager.GetClientSettingByName(client.Id, ClientSettings.TermsConditionsAcceptanceVersion);
                if (termsConditionsAcceptanceVersion != null && termsConditionsAcceptanceVersion.Id > 0 && termsConditionsAcceptanceVersion.StringValue != termsConditionVersion)
                    clientLoginOut.AcceptTermsConditions = true;
            }
            /*var documentExpirationPeriod = CacheManager.GetConfigKey(client.PartnerId, Constants.PartnerKeys.DocumentExpirationPeriod);
            if (int.TryParse(documentExpirationPeriod, out int expirationPeriod) )
            {
                var expiredTime = currentTime.AddDays(expirationPeriod);
                var expiredData = expiredTime.Year * 10000 + expiredTime.Month * 100 +
                                     expiredTime.Day;
                var documents = GetClientIdentities(client.Id);
                if (documents.Any(x => x.ExpirationDate < expiredData && x.Status == (int)KYCDocumentStates.Approved))
                    clientLoginOut.DocumentExpirationStatus = (int)KYCDocumentStates.CloseToExpiration;
                else if(!documents.Any(x => x.Status == (int)KYCDocumentStates.Approved) &&
                                              documents.Any(x => x.Status == (int)KYCDocumentStates.Expired))
                    clientLoginOut.DocumentExpirationStatus = (int)KYCDocumentStates.CloseToExpiration;
            }*/
            return client;
        }

        public void GetClientRegionInfo(int regionId, ref ClientLoginOut clientLoginOut)
        {
            var regionPath = Db.fn_RegionPath(regionId).ToList();
            var country = regionPath.FirstOrDefault(x => x.TypeId == (int)RegionTypes.Country);
            if (country != null)
                clientLoginOut.CountryId = country.Id ?? regionId;
            var city = regionPath.FirstOrDefault(x => x.TypeId == (int)RegionTypes.City);
            if (city != null)
                clientLoginOut.CityId = city.Id ?? regionId;
            var district = regionPath.FirstOrDefault(x => x.TypeId == (int)RegionTypes.District);
            if (district != null)
                clientLoginOut.DistrictId = district.Id ?? regionId;
            var town = regionPath.FirstOrDefault(x => x.TypeId == (int)RegionTypes.Town);
            if (town != null)
                clientLoginOut.TownId = town.Id ?? regionId;
            var region = regionPath.FirstOrDefault(x => x.TypeId == (int)RegionTypes.Region);
            if (region != null)
                clientLoginOut.RegionId = region.Id ?? regionId;
        }

        public static SessionIdentity GetClientProductSession(string token, string languageId, int? productId = null, bool checkExpiration = true)
        {
            var clientSession = CacheManager.GetClientSessionByToken(token, productId);
            if (clientSession == null)
                throw CreateException(languageId, Constants.Errors.SessionNotFound);
            if (checkExpiration && clientSession.State == (int)SessionStates.Inactive)
                throw CreateException(languageId, Constants.Errors.SessionExpired);
            var resp = new SessionIdentity
            {
                Id = clientSession.ClientId,
                LoginIp = string.IsNullOrEmpty(clientSession.Ip) ? "127.0.0.1" : clientSession.Ip,
                LanguageId = clientSession.LanguageId,
                SessionId = clientSession.Id,
                Token = clientSession.Token,
                ProductId = clientSession.ProductId,
                Country = clientSession.Country,
                DeviceType = clientSession.DeviceType,
                StartTime = clientSession.StartTime.Value,
                LastUpdateTime = clientSession.LastUpdateTime,
                State = clientSession.State,
                CurrentPage = clientSession.CurrentPage,
                ParentId = clientSession.ParentId,
                CurrencyId = clientSession.CurrencyId
            };
            if (clientSession.ParentId != null)
            {
                var pSession = CacheManager.GetClientPlatformSession(clientSession.ClientId, clientSession.ParentId.Value);
                if (pSession == null && checkExpiration)
                    throw CreateException(languageId, Constants.Errors.SessionExpired);
                if (pSession != null)
                    resp.ParentSessionStartTime = pSession.StartTime.Value;
            }

            return resp;
        }

        public static ClientSession GetClientSessionByProductId(int clientId, int productId)
        {
            using var db = new IqSoftCorePlatformEntities();
            var clientSession = db.ClientSessions.FirstOrDefault(x => x.ClientId == clientId && x.ProductId == productId &&
                                                                 x.State == (int)SessionStates.Active);
            if (clientSession == null)
                throw CreateException(Constants.DefaultLanguageId, Constants.Errors.SessionNotFound);
            var currentTime = DateTime.UtcNow;
            clientSession.LastUpdateTime = currentTime;
            if (clientSession.ParentId != null)
            {
                var pSession = CacheManager.GetClientPlatformSession(clientSession.ClientId, clientSession.ParentId.Value);
                if (pSession == null)
                    throw CreateException(Constants.DefaultLanguageId, Constants.Errors.SessionExpired);
            }
            db.SaveChanges();
            return clientSession;
        }

        public ClientSession RefreshClientSession(string token, bool expireOld = false)
        {
            var oldSession = GetClientProductSession(token, LanguageId);
            var session = new ClientSession
            {
                ClientId = oldSession.Id,
                LanguageId = oldSession.LanguageId,
                ProductId = oldSession.ProductId,
                Ip = oldSession.LoginIp,
                Country = oldSession.Country,
                DeviceType = oldSession.DeviceType,
                ParentId = oldSession.ParentId
            };
            var savedSession = AddClientSession(session);
            if (expireOld)
            {
                Db.ClientSessions.Where(x => x.Id == oldSession.SessionId).
                UpdateFromQuery(x => new ClientSession { State = (int)SessionStates.Inactive, EndTime = GetServerDate(), LogoutType = (int)LogoutTypes.System });
                CacheManager.RemoveClientProductSession(oldSession.Token, oldSession.ProductId);
            }
            Db.SaveChanges();
            return savedSession;
        }

        public int LogoutClient(string token)
        {
            var clientSession = Db.ClientSessions.FirstOrDefault(x => x.Token == token && x.ProductId == (int)Constants.PlatformProductId);
            if (clientSession == null)
                throw CreateException(LanguageId, Constants.Errors.WrongToken);
            LogoutClientById(clientSession.ClientId);
            return clientSession.ClientId;
        }

        public void LogoutClientById(int clientId, int? logoutType = null)
        {
            var currentTime = DateTime.UtcNow;
            var sessions = Db.ClientSessions.Where(x => x.ClientId == clientId && x.ProductId == Constants.PlatformProductId && x.State == (int)SessionStates.Active);
            var lastSession = sessions.OrderByDescending(x => x.Id).FirstOrDefault();
            if (lastSession != null)
                Db.Clients.Where(c => c.Id == clientId).UpdateFromQuery(x => new Client { LastSessionId = lastSession.Id });
            var tokens = new List<string>();
            foreach (var s in sessions)
            {
                s.State = (int)SessionStates.Inactive;
                s.EndTime = currentTime;
                s.LogoutType = logoutType == null ? (int)LogoutTypes.Manual : logoutType.Value;
                tokens.Add(s.Token);
            }
            Db.SaveChanges();
            CacheManager.RemoveClientPlatformSession(clientId);
            foreach (var token in tokens)
            {
                CacheManager.RemoveClientProductSession(token, Constants.PlatformProductId);
            }
            CacheManager.RemoveClientFromCache(clientId);
        }

        public List<fnAccount> GetClientAccounts(int clientId, bool checkPermission)
        {
            var client = Db.Clients.FirstOrDefault(x => x.Id == clientId);
            if (client == null)
                throw CreateException(LanguageId, Constants.Errors.ClientNotFound);

            if (checkPermission)
            {
                var checkClientPermission = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewClient,
                    ObjectTypeId = (int)ObjectTypes.Client
                });

                var checkPartnerPermission = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewPartner,
                    ObjectTypeId = (int)ObjectTypes.Partner
                });

                var affiliateReferralAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewAffiliateReferral,
                    ObjectTypeId = (int)ObjectTypes.AffiliateReferral
                });
                if ((!checkClientPermission.HaveAccessForAllObjects && checkClientPermission.AccessibleObjects.All(x => x != clientId)) ||
                    (!checkPartnerPermission.HaveAccessForAllObjects && checkPartnerPermission.AccessibleObjects.All(x => x != client.PartnerId)) ||
                    (!affiliateReferralAccess.HaveAccessForAllObjects &&
                      affiliateReferralAccess.AccessibleObjects.All(x => client.AffiliateReferralId.HasValue && x != client.AffiliateReferralId.Value)))
                    throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            }
            else
            {
                if (client.UserId != Identity.Id)
                    throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            }

            return GetfnAccounts(new FilterfnAccount
            {
                ObjectId = clientId,
                ObjectTypeId = (int)ObjectTypes.Client
            });
        }

        public PagedModel<ClientSession> GetClientLoginsPagedModel(FilterClientSession filter)
        {
            var clientAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewClient,
                ObjectTypeId = (int)ObjectTypes.Client
            });
            //var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            //{
            //    Permission = Constants.Permissions.ViewPartner,
            //    ObjectTypeId = (int)ObjectTypes.Partner
            //});

            filter.CheckPermissionResuts = new List<CheckPermissionOutput<ClientSession>>
            {
                new CheckPermissionOutput<ClientSession>
                {
                    AccessibleObjects = clientAccess.AccessibleObjects,
                    HaveAccessForAllObjects = clientAccess.HaveAccessForAllObjects,
                    Filter = x => clientAccess.AccessibleObjects.AsEnumerable().Contains(x.ClientId)
                }//,
                //new CheckPermissionOutput<ClientSession>
                //{
                //    AccessibleObjects = partnerAccess.AccessibleObjects,
                //    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                //    Filter = x => partnerAccess.AccessibleObjects.AsEnumerable().Contains(x.PartnerId)
                //}
            };

            Func<IQueryable<ClientSession>, IOrderedQueryable<ClientSession>> orderBy;

            if (filter.OrderBy.HasValue)
            {
                if (filter.OrderBy.Value)
                {
                    orderBy = QueryableUtilsHelper.OrderByFunc<ClientSession>(filter.FieldNameToOrderBy, true);
                }
                else
                {
                    orderBy = QueryableUtilsHelper.OrderByFunc<ClientSession>(filter.FieldNameToOrderBy, false);
                }
            }
            else
            {
                orderBy = clients => clients.OrderByDescending(x => x.Id);
            }

            return new PagedModel<ClientSession>
            {
                Entities = filter.FilterObjects(Db.ClientSessions, orderBy).ToList(),
                Count = filter.SelectedObjectsCount(Db.ClientSessions)

            };
        }
        public void ChangeClientNickName(int clientId, string nickName)
        {
            var pattern = "(?=^.{5,20}$)[a-zA-Z0-9]+$";
            var userNamePattern = "(^P[A-Z0-9]{3}$)|(^C[A-Z0-9]{3,10}$)|(^S[A-Z0-9]{3,10}$)|(^([A-OQR1-9]{1})([A-Z0-9]{10})$)";
            if (!Regex.IsMatch(nickName, pattern))
                throw CreateException(LanguageId, Constants.Errors.InvalidUserName);
            var dbClient = Db.Clients.FirstOrDefault(x => x.Id == clientId);
            if (dbClient == null)
                throw CreateException(LanguageId, Constants.Errors.ClientNotFound);
            var partnerSetting = CacheManager.GetPartnerSettingByKey(dbClient.PartnerId, Constants.PartnerKeys.IsUserNameGeneratable);
            if (partnerSetting != null && partnerSetting.NumericValue.HasValue && partnerSetting.NumericValue != 0 &&
              (dbClient.UserName != nickName && (Regex.IsMatch(nickName, userNamePattern) ||
               nickName.Length == 6 || nickName.Length == 8 || nickName.Length == 11)))
                throw CreateException(LanguageId, Constants.Errors.InvalidUserName);
            if (Db.Clients.Any(x => ((x.UserName == nickName && x.Id != clientId) || x.NickName == nickName) && x.PartnerId == dbClient.PartnerId))
                throw CreateException(LanguageId, Constants.Errors.NickNameExists);
            var currentTime = GetServerDate();
            dbClient.NickName = nickName;
            dbClient.LastUpdateTime = currentTime;
            Db.SaveChanges();
            CacheManager.RemoveClientFromCache(clientId);
        }

        public void ChangeClientPassword(ChangeClientPasswordInput input, bool isReset = false)
        {
            var dbClient = Db.Clients.FirstOrDefault(x => x.Id == input.ClientId);
            if (dbClient == null)
                throw CreateException(LanguageId, Constants.Errors.ClientNotFound);
            if (!isReset && input.SecurityQuestions != null && input.SecurityQuestions.Any())
                VerifyClientMobileNumber(input.SMSCode, dbClient.MobileNumber, dbClient.Id, dbClient.PartnerId, true, input.SecurityQuestions, true);
            var newPasswordHash = CommonFunctions.ComputeClientPasswordHash(input.NewPassword, dbClient.Salt);
            var oldPasswordHash = CommonFunctions.ComputeClientPasswordHash(input.OldPassword, dbClient.Salt);
            if (!isReset && oldPasswordHash != dbClient.PasswordHash)
                throw CreateException(LanguageId, Constants.Errors.WrongPassword);
            if (!isReset && newPasswordHash == oldPasswordHash)
                throw CreateException(LanguageId, Constants.Errors.PasswordMatches);
            VerifyClientPassword(input.NewPassword, dbClient);
            var currentTime = DateTime.UtcNow;
            dbClient.PasswordHash = CommonFunctions.ComputeClientPasswordHash(input.NewPassword, dbClient.Salt);
            dbClient.LastUpdateTime = currentTime;
            Db.SaveChanges();
            var currDate = currentTime.Year * 10000 + currentTime.Month * 100 + currentTime.Day;
            var clientettings = new ClientCustomSettings
            {
                ClientId = dbClient.Id,
                PasswordChangedDate = currDate
            };
            SaveClientSetting(clientettings);
        }

        public void ChangeClientPassword(NewPasswordInput input)
        {
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.EditClientPass
            });
            var client = CacheManager.GetClientById(input.ClientId);
            if (client == null)
                throw CreateException(LanguageId, Constants.Errors.ClientNotFound);
            var partner = CacheManager.GetPartnerById(client.PartnerId);
            var partnerSetting = CacheManager.GetConfigKey(client.PartnerId, Constants.PartnerKeys.VerificationKeyNumberOnly);
            var codeLenght = input.NotificationType == 2 ? partner.MobileVerificationCodeLength : partner.EmailVerificationCodeLength;
            var verificationKey = (!string.IsNullOrEmpty(partnerSetting) && partnerSetting == "1") ? CommonFunctions.GetRandomNumber(codeLenght) :
                                                                                                     CommonFunctions.GetRandomString(codeLenght);

            using (var notificationBll = new NotificationBll(Identity, Log))
            {
                var notificationModel = new NotificationModel
                {
                    PartnerId = client.PartnerId,
                    ClientId = client.Id,
                    VerificationCode = verificationKey,
                    LanguageId = client.LanguageId
                };
                if (!string.IsNullOrWhiteSpace(client.Email) && input.NotificationType == 1)
                {
                    notificationModel.MobileOrEmail = client.Email;
                    notificationModel.ClientInfoType = (int)ClientInfoTypes.ResetPasswordEmail;
                    notificationBll.SendNotificationMessage(notificationModel);
                }
                else if (!string.IsNullOrWhiteSpace(client.MobileNumber) && input.NotificationType == 2)
                {
                    notificationModel.MobileOrEmail = client.MobileNumber;
                    notificationModel.ClientInfoType = (int)ClientInfoTypes.ResetPasswordSMS;
                    notificationBll.SendNotificationMessage(notificationModel);
                }
                Db.SaveChanges();
            }
        }

        public Client ChangeClientDataFromWebSite(ChangeClientFieldsInput clientData)
        {
            var dbClient = Db.Clients.FirstOrDefault(x => x.Id == clientData.ClientId);
            if (dbClient == null)
                throw CreateException(LanguageId, Constants.Errors.ClientNotFound);
            var partner = CacheManager.GetPartnerById(dbClient.PartnerId);
            var oldValue = JsonConvert.SerializeObject(dbClient.ToClientInfo());
            using (var transactionScope = CommonFunctions.CreateTransactionScope())
            {
                CheckClientSecurityAnswers(dbClient.Id, LanguageId, clientData.SecurityQuestions);
                if (string.IsNullOrWhiteSpace(clientData.Email) && !string.IsNullOrWhiteSpace(dbClient.Email))
                    throw BaseBll.CreateException(LanguageId, Constants.Errors.InvalidEmail);
                else if (!string.IsNullOrWhiteSpace(clientData.Email) && clientData.Email != dbClient.Email)
                {
                    var restrictEmailChange = CacheManager.GetConfigKey(dbClient.PartnerId, Constants.PartnerKeys.RestrictEmailChanges);
                    if (!string.IsNullOrEmpty(restrictEmailChange) && restrictEmailChange == "1")
                        throw CreateException(LanguageId, Constants.Errors.NotAllowed);

                    if (!IsValidEmail(clientData.Email))
                        throw CreateException(LanguageId, Constants.Errors.InvalidEmail);
                    if (Db.Clients.Any(x => x.PartnerId == dbClient.PartnerId && x.Email == clientData.Email))
                        throw CreateException(LanguageId, Constants.Errors.EmailExists);
                    dbClient.Email = clientData.Email;
                    dbClient.IsEmailVerified = false;
                }

                clientData.MobileNumber = string.IsNullOrEmpty(clientData.MobileNumber) ? string.Empty : "+" + clientData.MobileNumber.Replace(" ", string.Empty).Replace("+", string.Empty);
                if (string.IsNullOrWhiteSpace(clientData.MobileNumber) && !string.IsNullOrWhiteSpace(dbClient.MobileNumber))
                    throw BaseBll.CreateException(LanguageId, Constants.Errors.InvalidMobile);
                else if (!string.IsNullOrEmpty(clientData.MobileNumber) && clientData.MobileNumber != dbClient.MobileNumber)
                {
                    if (!IsMobileNumber(clientData.MobileNumber))
                        throw CreateException(LanguageId, Constants.Errors.InvalidMobile);
                    if (Db.Clients.Any(x => x.PartnerId == dbClient.PartnerId && x.MobileNumber == clientData.MobileNumber))
                        throw CreateException(LanguageId, Constants.Errors.MobileExists);
                    dbClient.MobileNumber = clientData.MobileNumber;
                    dbClient.IsMobileNumberVerified = false;
                }
                var updateUtilityBillStatus = false;
                if (dbClient.Address != clientData.Address)
                {
                    dbClient.Address = clientData.Address;
                    updateUtilityBillStatus = true;
                }
                if (clientData.BirthDate.HasValue)
                {
                    if (clientData.BirthDate > DateTime.UtcNow.AddYears(-partner.ClientMinAge))
                        throw CreateException(LanguageId, Constants.Errors.InvalidBirthDate);
                    dbClient.BirthDate = clientData.BirthDate.Value;
                }

                if ((dbClient.FirstName ?? string.Empty) != (clientData.FirstName ?? string.Empty))
                {
                    var restrictNameChange = CacheManager.GetConfigKey(dbClient.PartnerId, Constants.PartnerKeys.RestrictNameChanges);
                    if (!string.IsNullOrEmpty(restrictNameChange) && restrictNameChange == "1")
                        throw CreateException(LanguageId, Constants.Errors.NotAllowed);

                    dbClient.FirstName = clientData.FirstName;
                }
                if ((dbClient.LastName ?? string.Empty) != (clientData.LastName ?? string.Empty))
                {
                    var restrictNameChange = CacheManager.GetConfigKey(dbClient.PartnerId, Constants.PartnerKeys.RestrictNameChanges);
                    if (!string.IsNullOrEmpty(restrictNameChange) && restrictNameChange == "1")
                        throw CreateException(LanguageId, Constants.Errors.NotAllowed);

                    dbClient.LastName = clientData.LastName;
                }

                if ((dbClient.NickName ?? string.Empty) != (clientData.NickName ?? string.Empty))
                {
                    var restrictNameChange = CacheManager.GetConfigKey(dbClient.PartnerId, Constants.PartnerKeys.RestrictNameChanges);
                    if (!string.IsNullOrEmpty(restrictNameChange) && restrictNameChange == "1")
                        throw CreateException(LanguageId, Constants.Errors.NotAllowed);

                    dbClient.NickName = clientData.NickName;
                }

                if (dbClient.SecondName != clientData.SecondName)
                    dbClient.SecondName = clientData.SecondName;
                if (dbClient.SecondSurname != clientData.SecondSurname)
                    dbClient.SecondSurname = clientData.SecondSurname;
                if (dbClient.PhoneNumber != clientData.PhoneNumber)
                    dbClient.PhoneNumber = clientData.PhoneNumber;
                if (!string.IsNullOrWhiteSpace(clientData.DocumentNumber) && clientData.DocumentNumber != dbClient.DocumentNumber)
                {
                    dbClient.DocumentNumber = clientData.DocumentNumber;
                    dbClient.IsDocumentVerified = false;
                }
                if (dbClient.DocumentIssuedBy != clientData.DocumentIssuedBy)
                    dbClient.DocumentIssuedBy = clientData.DocumentIssuedBy;
                if (dbClient.ZipCode != clientData.ZipCode)
                    dbClient.ZipCode = clientData.ZipCode;
                if (clientData.SendPromotions.HasValue)
                    dbClient.SendPromotions = clientData.SendPromotions.Value;
                if (clientData.CallToPhone.HasValue)
                    dbClient.CallToPhone = clientData.CallToPhone.Value;
                if (clientData.SendMail.HasValue)
                    dbClient.SendMail = clientData.SendMail.Value;
                if (clientData.SendSms.HasValue)
                    dbClient.SendSms = clientData.SendSms.Value;
                int regionId = clientData.RegionId ?? dbClient.RegionId;
                if (clientData.TownId.HasValue && clientData.TownId != 0)
                    regionId = clientData.TownId.Value;
                else if (clientData.CityId.HasValue && clientData.CityId != 0)
                    regionId = clientData.CityId.Value;
                else if (clientData.CountryId.HasValue)
                    regionId = clientData.CountryId.Value;
                if (dbClient.RegionId != regionId)
                    updateUtilityBillStatus = true;
                if (!string.IsNullOrEmpty(clientData.City) && clientData.City != dbClient.City)
                {
                    dbClient.City = clientData.City;
                    updateUtilityBillStatus = true;
                }
                dbClient.RegionId = regionId;
                if (!string.IsNullOrEmpty(clientData.LanguageId))
                    dbClient.LanguageId = clientData.LanguageId;
                if (!dbClient.Citizenship.HasValue && clientData.Citizenship.HasValue)
                    dbClient.Citizenship = clientData.Citizenship;
                dbClient.LastUpdateTime = GetServerDate();
                AddClientJobTrigger(dbClient.Id, (int)JobTriggerTypes.ReconsiderSegments);
                SaveChangesWithHistory((int)ObjectTypes.Client, dbClient.Id, oldValue, dbClient.Comment);
                if (updateUtilityBillStatus)
                {
                    Db.ClientIdentities.Where(x => x.ClientId == dbClient.Id && x.DocumentTypeId == (int)KYCDocumentTypes.UtilityBill &&
                                                   x.Status == (int)KYCDocumentStates.Approved)
                                       .UpdateFromQuery(x => new ClientIdentity { Status = (int)KYCDocumentStates.Expired });
                    var partnerConfig = CacheManager.GetConfigKey(dbClient.PartnerId, Constants.PartnerKeys.PartnerKYCTypes);
                    if (!string.IsNullOrEmpty(partnerConfig))
                    {
                        var requiredDocumentTypes = JsonConvert.DeserializeObject<List<int>>(partnerConfig);
                        if (requiredDocumentTypes.Any() && requiredDocumentTypes.Contains((int)KYCDocumentTypes.UtilityBill))
                        {
                            dbClient.IsDocumentVerified = false;
                            Db.SaveChanges();

                            var sendSMS = false;
                            var sendEmail = false;
                            partnerConfig = CacheManager.GetConfigKey(dbClient.PartnerId, Constants.PartnerKeys.SendWaitingKYCDocumentSMS);
                            if (!string.IsNullOrEmpty(partnerConfig) && partnerConfig == "1" && dbClient.IsMobileNumberVerified)
                                sendSMS = true;
                            partnerConfig = CacheManager.GetConfigKey(dbClient.PartnerId, Constants.PartnerKeys.SendWaitingKYCDocumentEmail);
                            if (!string.IsNullOrEmpty(partnerConfig) && partnerConfig == "1")
                                sendEmail = true;
                            using (var notificationBl = new NotificationBll(this))
                            {
                                var notificationModel = new NotificationModel
                                {
                                    PartnerId = dbClient.PartnerId,
                                    ClientId = dbClient.Id
                                };
                                if (sendSMS)
                                {
                                    notificationModel.MobileOrEmail = dbClient.MobileNumber;
                                    notificationModel.ClientInfoType = (int)ClientInfoTypes.WaitingKYCDocumentSMS;
                                    notificationBl.SendNotificationMessage(notificationModel);
                                }
                                if (sendEmail)
                                {
                                    notificationModel.MobileOrEmail = dbClient.Email;
                                    notificationModel.ClientInfoType = (int)ClientInfoTypes.WaitingKYCDocumentEmail;
                                    notificationBl.SendNotificationMessage(notificationModel);
                                }
                                notificationBl.SendInternalTicket(dbClient.Id, (int)ClientInfoTypes.WaitingKYCDocumentTicket);
                            }
                        }
                    }
                }
                transactionScope.Complete();
                return dbClient;
            }
        }

        public Client ChangeClientDataFromAgent(ChangeClientFieldsInput clientData)
        {
            var dbClient = Db.Clients.FirstOrDefault(x => x.Id == clientData.ClientId);
            if (dbClient == null)
                throw CreateException(LanguageId, Constants.Errors.ClientNotFound);

            var oldValue = JsonConvert.SerializeObject(dbClient.ToClientInfo());
            using var transactionScope = CommonFunctions.CreateTransactionScope();
            clientData.MobileNumber = string.IsNullOrEmpty(clientData.MobileNumber) ? string.Empty : "+" + clientData.MobileNumber.Replace(" ", string.Empty).Replace("+", string.Empty);
            if (!string.IsNullOrEmpty(clientData.MobileNumber) && clientData.MobileNumber != dbClient.MobileNumber)
            {
                if (!IsMobileNumber(clientData.MobileNumber))
                    throw CreateException(LanguageId, Constants.Errors.InvalidMobile);
                if (dbClient.IsMobileNumberVerified)
                    throw CreateException(LanguageId, Constants.Errors.MobileNumberAlreadyVerified);
                if (Db.Clients.Any(x => x.PartnerId == dbClient.PartnerId && x.MobileNumber == clientData.MobileNumber))
                    throw CreateException(LanguageId, Constants.Errors.MobileExists);
                dbClient.MobileNumber = clientData.MobileNumber;
                dbClient.IsMobileNumberVerified = false;
            }

            if (!string.IsNullOrWhiteSpace(clientData.FirstName))
                dbClient.FirstName = clientData.FirstName;
            if (!string.IsNullOrWhiteSpace(clientData.LastName))
                dbClient.LastName = clientData.LastName;
            if (!string.IsNullOrWhiteSpace(clientData.SecondName))
                dbClient.SecondName = clientData.SecondName;
            if (!string.IsNullOrWhiteSpace(clientData.SecondSurname))
                dbClient.SecondSurname = clientData.SecondSurname;
            if (!string.IsNullOrWhiteSpace(clientData.PhoneNumber))
                dbClient.PhoneNumber = clientData.PhoneNumber;
            if (clientData.CategoryId != dbClient.CategoryId)
            {
                dbClient.CategoryId = clientData.CategoryId;
                var dbClientClassification = Db.ClientClassifications.FirstOrDefault(x => x.ClientId == clientData.ClientId &&
                    x.ProductId == Constants.PlatformProductId);
                if (dbClientClassification != null)
                    dbClientClassification.CategoryId = clientData.CategoryId;
            }
            dbClient.LastUpdateTime = GetServerDate();
            SaveChangesWithHistory((int)ObjectTypes.Client, dbClient.Id, oldValue, dbClient.Comment);
            transactionScope.Complete();
            return dbClient;
        }

        public bool MobileNumberExists(int partnerId, string mobileNumber)
        {
            return Db.Clients.Any(x => x.PartnerId == partnerId && x.MobileNumber == mobileNumber);
        }

        public Client ChangeClientState(int clientId, int? state, bool? isDocumentVerified)
        {
            var dbClient = Db.Clients.FirstOrDefault(x => x.Id == clientId);
            if (dbClient == null)
                throw CreateException(LanguageId, Constants.Errors.ClientNotFound);
            if (state.HasValue)
                dbClient.State = state.Value;
            if (isDocumentVerified.HasValue)
                dbClient.IsDocumentVerified = isDocumentVerified.Value;
            dbClient.LastUpdateTime = GetServerDate();
            AddClientJobTrigger(dbClient.Id, (int)JobTriggerTypes.ReconsiderSegments);
            return dbClient;
        }

        public Client UpdateAgentClient(int agentId, int clientId, int state, string pass, DocumentBll documentBl)
        {
            var user = CacheManager.GetUserById(agentId);
            if (user == null)
                throw CreateException(Identity.LanguageId, Constants.Errors.UserNotFound);
            if (user.Type == (int)UserTypes.AgentEmployee)
            {
                CheckPermission(Constants.Permissions.EditClient);
                agentId = user.ParentId.Value;
            }

            var client = GetClientById(clientId);
            if (client == null)
                throw CreateException(Identity.LanguageId, Constants.Errors.ClientNotFound);
            var pSt = user.State;
            if (client.UserId != agentId)
            {
                var clientParent = CacheManager.GetUserById(client.UserId.Value);
                var parentAgentState = clientParent.State;
                var parentSetting = CacheManager.GetUserSetting(clientParent.Id);
                if (parentSetting != null && parentSetting.ParentState.HasValue && CustomHelper.Greater((UserStates)parentSetting.ParentState.Value, (UserStates)parentAgentState))
                    parentAgentState = parentSetting.ParentState.Value;
                pSt = CustomHelper.MapUserStateToClient[parentAgentState];
                if (!clientParent.Path.Contains("/" + agentId + "/") || (CustomHelper.Greater((ClientStates)pSt, (ClientStates)state) && pSt != state &&
                    (pSt != (int)UserStates.ForceBlock)))
                    throw CreateException(Identity.LanguageId, Constants.Errors.NotAllowed);
            }
            if (Enum.IsDefined(typeof(ClientStates), state))
            {
                var clientState = client.State;
                var parentState = CacheManager.GetClientSettingByName(client.Id, ClientSettings.ParentState);
                if (parentState.NumericValue.HasValue && CustomHelper.Greater((ClientStates)parentState.NumericValue, (ClientStates)clientState))
                    clientState = Convert.ToInt32(parentState.NumericValue.Value);
                if (clientState == (int)ClientStates.Disabled ||
                   (state == (int)ClientStates.Disabled && clientState != (int)ClientStates.FullBlocked))
                    throw CreateException(Identity.LanguageId, Constants.Errors.NotAllowed);
                if (clientState != state)
                {
                    client.State = state;
                    var clientSetting = new ClientCustomSettings
                    {
                        ClientId = clientId,
                        ParentState = state
                    };
                    SaveClientSetting(clientSetting);
                    if (state == (int)ClientStates.Disabled)
                    {
                        var balance = CacheManager.GetClientCurrentBalance(clientId).Balances
                            .Where(y => y.TypeId != (int)AccountTypes.ClientBonusBalance &&
                                        y.TypeId != (int)AccountTypes.ClientCompBalance &&
                                        y.TypeId != (int)AccountTypes.ClientCoinBalance)
                            .Sum(y => y.Balance);
                        var clientCorrectionInput = new ClientCorrectionInput
                        {
                            Amount = balance,
                            CurrencyId = client.CurrencyId,
                            ClientId = clientId
                        };
                        CreateCreditCorrectionOnClient(clientCorrectionInput, documentBl, false);
                        var clientCreditSetting = new ClientCustomSettings
                        {
                            ClientId = clientId,
                            MaxCredit = 0
                        };
                        SaveClientSetting(clientCreditSetting);
                    }
                    else if (state == (int)ClientStates.FullBlocked)
                        LogoutClientById(clientId, (int)LogoutTypes.Admin);
                }
            }
            if (!string.IsNullOrEmpty(pass))
            {
                VerifyClientPassword(pass, client);
                client.PasswordHash = CommonFunctions.ComputeClientPasswordHash(pass, client.Salt);
                Db.SaveChanges();
                var currentTime = GetServerDate();
                var currDate = currentTime.Year * 10000 + currentTime.Month * 100 + currentTime.Day;
                var clientettings = new ClientCustomSettings
                {
                    ClientId = client.Id,
                    PasswordChangedDate = currDate
                };
                SaveClientSetting(clientettings);
            }
            CacheManager.RemoveClientFromCache(client.Id);
            return client;
        }

        private void VerifyClientPassword(string password, Client client)
        {
            var partner = CacheManager.GetPartnerById(client.PartnerId);
            var unallowedKeys = new List<string>();
            if (!string.IsNullOrEmpty(client.FirstName))
                unallowedKeys.Add(client.FirstName.ToLower());
            if (!string.IsNullOrEmpty(client.LastName))
                unallowedKeys.Add(client.LastName.ToLower());
            if (!string.IsNullOrEmpty(client.UserName))
                unallowedKeys.Add(client.UserName.ToLower());
            if (!string.IsNullOrEmpty(client.DocumentNumber))
                unallowedKeys.Add(client.DocumentNumber.ToLower());
            if (client.BirthDate != DateTime.MinValue)
            {
                unallowedKeys.Add(client.BirthDate.ToString("MMddyyyy"));
                unallowedKeys.Add(client.BirthDate.ToString("ddmmyyyy"));
            }
            if (!Regex.IsMatch(password, partner.PasswordRegExp) || unallowedKeys.Any(password.ToLower().Contains))
                throw CreateException(LanguageId, Constants.Errors.PasswordContainsPersonalData);
        }
        public Client ChangeClientDepositInfo(int clientId, int depositCount, decimal amount)
        {
            var client = Db.Clients.FirstOrDefault(x => x.Id == clientId);
            if (client == null)
                throw CreateException(LanguageId, Constants.Errors.ClientNotFound);
            var currDate = GetServerDate();
            client.LastDepositDate = currDate;
            client.LastDepositAmount = amount;
            if (depositCount == 1)
                client.FirstDepositDate = currDate;
            Db.SaveChanges();
            return client;
        }
        public void CheckClientDepositLimits(int clientId, decimal depositAmount)
        {
            var clientSetting = new ClientCustomSettings();
            var depositLimitDaily = CacheManager.GetClientSettingByName(clientId, nameof(clientSetting.DepositLimitDaily));
            var systemDepositLimitDaily = CacheManager.GetClientSettingByName(clientId, nameof(clientSetting.SystemDepositLimitDaily));
            if ((depositLimitDaily != null && depositLimitDaily.Id > 0 && depositLimitDaily.NumericValue.HasValue) ||
                (systemDepositLimitDaily != null && systemDepositLimitDaily.Id > 0 && systemDepositLimitDaily.NumericValue.HasValue))
            {
                decimal limitAmount = -1;
                if (depositLimitDaily != null && depositLimitDaily.Id > 0 && depositLimitDaily.NumericValue.HasValue)
                    limitAmount = depositLimitDaily.NumericValue.Value;
                if (systemDepositLimitDaily != null && systemDepositLimitDaily.Id > 0 && systemDepositLimitDaily.NumericValue.HasValue)
                    limitAmount = limitAmount == -1 ? systemDepositLimitDaily.NumericValue.Value : Math.Min(limitAmount, systemDepositLimitDaily.NumericValue.Value);

                var dailyDepositAmount = CacheManager.GetTotalDepositAmounts(clientId, (int)PeriodsOfTime.Daily);
                if (dailyDepositAmount + depositAmount > limitAmount)
                    throw BaseBll.CreateException(LanguageId, Constants.Errors.ClientMaxLimitExceeded);
            }
            var depositLimitWeekly = CacheManager.GetClientSettingByName(clientId, nameof(clientSetting.DepositLimitWeekly));
            var systemDepositLimitWeekly = CacheManager.GetClientSettingByName(clientId, nameof(clientSetting.SystemDepositLimitWeekly));
            if ((depositLimitWeekly != null && depositLimitWeekly.Id > 0 && depositLimitWeekly.NumericValue.HasValue) ||
                (systemDepositLimitWeekly != null && systemDepositLimitWeekly.Id > 0 && systemDepositLimitWeekly.NumericValue.HasValue))
            {
                decimal limitAmount = -1;
                if (depositLimitWeekly != null && depositLimitWeekly.Id > 0 && depositLimitWeekly.NumericValue.HasValue)
                    limitAmount = depositLimitWeekly.NumericValue.Value;
                if (systemDepositLimitWeekly != null && systemDepositLimitWeekly.Id > 0 && systemDepositLimitWeekly.NumericValue.HasValue)
                    limitAmount = limitAmount == -1 ? systemDepositLimitWeekly.NumericValue.Value : Math.Min(limitAmount, systemDepositLimitWeekly.NumericValue.Value);

                var weeklyDepositAmount = CacheManager.GetTotalDepositAmounts(clientId, (int)PeriodsOfTime.Weekly);
                if (weeklyDepositAmount + depositAmount > limitAmount)
                    throw BaseBll.CreateException(LanguageId, Constants.Errors.ClientMaxLimitExceeded);
            }
            var depositLimitMonthly = CacheManager.GetClientSettingByName(clientId, nameof(clientSetting.DepositLimitMonthly));
            var systemDepositLimitMonthly = CacheManager.GetClientSettingByName(clientId, nameof(clientSetting.SystemDepositLimitMonthly));
            if ((depositLimitMonthly != null && depositLimitMonthly.Id > 0 && depositLimitMonthly.NumericValue.HasValue) ||
                (systemDepositLimitMonthly != null && systemDepositLimitMonthly.Id > 0 && systemDepositLimitMonthly.NumericValue.HasValue))
            {
                decimal limitAmount = -1;
                if (depositLimitMonthly != null && depositLimitMonthly.Id > 0 && depositLimitMonthly.NumericValue.HasValue)
                    limitAmount = depositLimitMonthly.NumericValue.Value;
                if (systemDepositLimitMonthly != null && systemDepositLimitMonthly.Id > 0 && systemDepositLimitMonthly.NumericValue.HasValue)
                    limitAmount = limitAmount == -1 ? systemDepositLimitMonthly.NumericValue.Value : Math.Min(limitAmount, systemDepositLimitMonthly.NumericValue.Value);

                var monthlyDepositAmount = CacheManager.GetTotalDepositAmounts(clientId, (int)PeriodsOfTime.Monthly);
                if (monthlyDepositAmount + depositAmount > limitAmount)
                    throw BaseBll.CreateException(LanguageId, Constants.Errors.ClientMaxLimitExceeded);
            }
        }

        public Client QuickRegisteration(QuickRegistrationInput quickRegistrationInput, IWebHostEnvironment env)
        {
            bool sendPass = false;
            if (string.IsNullOrWhiteSpace(quickRegistrationInput.Password))
            {
                var partner = CacheManager.GetPartnerById(quickRegistrationInput.PartnerId);
                quickRegistrationInput.Password = RegExProperty.StringBasedOnRegEx(partner.PasswordRegExp);
                sendPass = true;
            }

            quickRegistrationInput.EmailOrMobile = quickRegistrationInput.EmailOrMobile.Replace(" ", string.Empty);

            if (quickRegistrationInput.IsMobile)
            {
                if (string.IsNullOrWhiteSpace(quickRegistrationInput.EmailOrMobile) || !IsMobileNumber(quickRegistrationInput.EmailOrMobile))
                    throw CreateException(LanguageId, Constants.Errors.MobileNumberCantBeEmpty);
                if (MobileExists(quickRegistrationInput.EmailOrMobile, quickRegistrationInput.PartnerId))
                    throw CreateException(LanguageId, Constants.Errors.MobileExists);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(quickRegistrationInput.EmailOrMobile) || !IsValidEmail(quickRegistrationInput.EmailOrMobile))
                    throw CreateException(LanguageId, Constants.Errors.EmailCantBeEmpty);
                if (EmailExists(quickRegistrationInput.EmailOrMobile, quickRegistrationInput.PartnerId))
                    throw CreateException(LanguageId, Constants.Errors.EmailExists);
            }

            var rand = new Random();
            var salt = rand.Next();
            var currentTime = DateTime.UtcNow;
            var regionId = 0;

            var region = Db.Regions.FirstOrDefault(x => x.IsoCode == quickRegistrationInput.CountryCode);

            if (region != null)
            {
                regionId = region.Id;
                if (string.IsNullOrEmpty(quickRegistrationInput.CurrencyId) && !string.IsNullOrEmpty(region.CurrencyId))
                {
                    var cList = CacheManager.GetPartnerCurrencies(quickRegistrationInput.PartnerId).Select(x => x.CurrencyId).ToList();
                    var partner = CacheManager.GetPartnerById(quickRegistrationInput.PartnerId);
                    if (!cList.Contains(region.CurrencyId))
                        quickRegistrationInput.CurrencyId = partner.CurrencyId;
                    else
                        quickRegistrationInput.CurrencyId = region.CurrencyId;
                }
            }

            var client = new Client
            {
                CreationTime = currentTime,
                Email = quickRegistrationInput.IsMobile ? string.Empty : quickRegistrationInput.EmailOrMobile,
                IsEmailVerified = false,
                CurrencyId = quickRegistrationInput.CurrencyId,
                UserName = quickRegistrationInput.UserName,
                Password = quickRegistrationInput.Password,
                PasswordHash = CommonFunctions.ComputeClientPasswordHash(quickRegistrationInput.Password, salt),
                Salt = salt,
                PartnerId = quickRegistrationInput.PartnerId,
                Gender = (int)Gender.Male,
                BirthDate = Constants.DefaultDateTime,
                SendMail = true,
                SendSms = false,
                CallToPhone = false,
                SendPromotions = false,
                RegionId = regionId,
                IsDocumentVerified = false,
                IsMobileNumberVerified = false,
                HasNote = false,
                State = (int)ClientStates.Active,
                CategoryId = (int)ClientCategories.New,
                LastUpdateTime = currentTime,
                MobileNumber = (quickRegistrationInput.IsMobile ? "+" + quickRegistrationInput.EmailOrMobile.Replace("+", string.Empty) : string.Empty),
                RegistrationIp = quickRegistrationInput.Ip,
                FirstName = String.IsNullOrEmpty(quickRegistrationInput.FirstName) ? string.Empty : quickRegistrationInput.FirstName,
                LastName = String.IsNullOrEmpty(quickRegistrationInput.LastName) ? string.Empty : quickRegistrationInput.LastName,
                UserId = quickRegistrationInput.UserId
            };

            var clientRegInput = new ClientRegistrationInput
            {
                ClientData = client,
                ReCaptcha = quickRegistrationInput.ReCaptcha,
                IsQuickRegistration = true,
                GeneratedUsername = quickRegistrationInput.GeneratedUsername,
                PromoCode = quickRegistrationInput.PromoCode,
                ReferralData = quickRegistrationInput.ReferralData
            };

            client = RegisterClient(clientRegInput, env);
            using (var notificationBll = new NotificationBll(Identity, Log))
            {
                try
                {
                    var notificationModel = new NotificationModel
                    {
                        PartnerId = client.PartnerId,
                        ClientId = client.Id
                    };
                    if (quickRegistrationInput.IsMobile)
                    {
                        if (sendPass)
                        {
                            notificationModel.MobileOrEmail = client.MobileNumber;
                            notificationModel.ClientInfoType = (int)ClientInfoTypes.QuickSmsRegistration;
                            notificationModel.VerificationCode = quickRegistrationInput.Password;
                            notificationBll.SendNotificationMessage(notificationModel);
                        }
                        else
                        {
                            var partnerConfig = CacheManager.GetConfigKey(client.PartnerId, Constants.PartnerKeys.SendRegistrationSMS);
                            if (!string.IsNullOrEmpty(partnerConfig) && partnerConfig == "1")
                            {
                                notificationModel.MobileOrEmail = client.MobileNumber;
                                notificationModel.ClientInfoType = (int)ClientInfoTypes.SuccessRegistrationSMS;
                                notificationBll.SendNotificationMessage(notificationModel);
                            }
                        }
                    }
                    else
                    {
                        if (sendPass)
                        {
                            notificationModel.MobileOrEmail = client.Email;
                            notificationModel.ClientInfoType = (int)ClientInfoTypes.QuickEmailRegistration;
                            notificationModel.VerificationCode = quickRegistrationInput.Password;
                            notificationBll.SendNotificationMessage(notificationModel);
                        }

                        else
                            notificationBll.SendVerificationCodeToEmail(client.Id, client.Email);
                    }
                }
                catch
                {
                    client.State = (int)ClientStates.FullBlocked;
                    AddClientJobTrigger(client.Id, (int)JobTriggerTypes.ReconsiderSegments);
                    if (quickRegistrationInput.IsMobile)
                        throw CreateException(LanguageId, Constants.Errors.InvalidMobile);
                    else
                        throw CreateException(LanguageId, Constants.Errors.InvalidEmail);
                }
            }
            client.Password = quickRegistrationInput.Password;
            return client;
        }

        public int SendRecoveryToken(int partnerId, string languageId, string identifier, string recaptcha)
        {
            CheckSiteCaptcha(partnerId, recaptcha);
            BllClient client = null;
            var isValidEmail = IsValidEmail(identifier);
            if (isValidEmail)
                client = CacheManager.GetClientByEmail(partnerId, identifier);
            else if (IsMobileNumber(identifier))
            {
                identifier = "+" + identifier.Replace(" ", string.Empty).Replace("+", string.Empty);
                client = CacheManager.GetClientByMobileNumber(partnerId, identifier);
            }
            else
            {
                client = CacheManager.GetClientByUserName(partnerId, identifier);
                isValidEmail = true;
            }
            if (client == null)
                return CacheManager.GetPartnerById(partnerId).VerificationKeyActiveMinutes;
            if (isValidEmail)
                return SendRecoveryMessageToClientEmail(client);
            return SendRecoveryMessageToClientMobile(client);
        }

        public int SendRecoveryMessageToClientEmail(BllClient client)
        {
            if (string.IsNullOrWhiteSpace(client.Email))
                throw CreateException(LanguageId, Constants.Errors.EmailCantBeEmpty);

            using (var notificationBll = new NotificationBll(Identity, Log))
            {
                var partner = CacheManager.GetPartnerById(client.PartnerId);
                var partnerSetting = CacheManager.GetConfigKey(client.PartnerId, Constants.PartnerKeys.VerificationKeyNumberOnly);
                var verificationKey = (!string.IsNullOrEmpty(partnerSetting) && partnerSetting == "1") ? CommonFunctions.GetRandomNumber(partner.EmailVerificationCodeLength) :
                                                                                                         CommonFunctions.GetRandomString(partner.EmailVerificationCodeLength);
                return notificationBll.SendNotificationMessage(new NotificationModel
                {
                    PartnerId = client.PartnerId,
                    ClientId = client.Id,
                    VerificationCode = verificationKey,
                    MobileOrEmail = client.Email,
                    ClientInfoType = (int)ClientInfoTypes.PasswordRecoveryEmailKey
                });
            }
        }

        public int SendRecoveryMessageToClientMobile(BllClient client)
        {
            if (string.IsNullOrWhiteSpace(client.MobileNumber))
                throw CreateException(LanguageId, Constants.Errors.MobileNumberCantBeEmpty);

            using (var notificationBll = new NotificationBll(Identity, Log))
            {
                var partner = CacheManager.GetPartnerById(client.PartnerId);
                var partnerSetting = CacheManager.GetConfigKey(client.PartnerId, Constants.PartnerKeys.VerificationKeyNumberOnly);
                var verificationKey = (!string.IsNullOrEmpty(partnerSetting) && partnerSetting == "1") ? CommonFunctions.GetRandomNumber(partner.MobileVerificationCodeLength) :
                                                                                                         CommonFunctions.GetRandomString(partner.MobileVerificationCodeLength);
                return notificationBll.SendNotificationMessage(new NotificationModel
                {
                    PartnerId = client.PartnerId,
                    ClientId = client.Id,
                    MobileOrEmail = client.MobileNumber,
                    ClientInfoType = (int)ClientInfoTypes.PasswordRecoveryMobileKey,
                    VerificationCode = verificationKey
                });
            }
        }

        public DAL.Models.Clients.ClientInfo GetClientInfo(int clientId, bool checkPermission = true)
        {
            var result = new DAL.Models.Clients.ClientInfo();
            var client = CacheManager.GetClientById(clientId);
            if (client == null)
                return result;
            if (checkPermission)
            {
                var checkClientPermission = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewClient,
                    ObjectTypeId = (int)ObjectTypes.Client
                });

                var checkPartnerPermission = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewPartner,
                    ObjectTypeId = (int)ObjectTypes.Partner
                });

                var affiliateReferralAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewAffiliateReferral,
                    ObjectTypeId = (int)ObjectTypes.AffiliateReferral
                });
                if ((!checkClientPermission.HaveAccessForAllObjects && checkClientPermission.AccessibleObjects.All(x => x != clientId)) ||
                    (!checkPartnerPermission.HaveAccessForAllObjects && checkPartnerPermission.AccessibleObjects.All(x => x != client.PartnerId)) ||
                    (!affiliateReferralAccess.HaveAccessForAllObjects &&
                      affiliateReferralAccess.AccessibleObjects.All(x => client.AffiliateReferralId.HasValue && x != client.AffiliateReferralId.Value))
                    )
                    throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            }

            result.Id = clientId;
            result.UserName = client.UserName;
            result.CategoryId = client.CategoryId;
            result.CurrencyId = client.CurrencyId;
            result.FirstName = client.FirstName;
            result.LastName = client.LastName;
            result.NickName = client.NickName;
            result.SecondName = client.SecondName;
            result.SecondSurname = client.SecondSurname;
            result.RegistrationDate = client.CreationTime;
            result.IsDocumentVerified = client.IsDocumentVerified;
            var clientBalance = CacheManager.GetClientCurrentBalance(clientId).Balances;
            result.Balance = Math.Round(clientBalance.Where(y => y.TypeId != (int)AccountTypes.ClientBonusBalance &&
                                        y.TypeId != (int)AccountTypes.ClientCompBalance &&
                                        y.TypeId != (int)AccountTypes.ClientCoinBalance)
                                        .Sum(y => y.Balance), 2);
            result.BonusBalance = clientBalance.FirstOrDefault(y => y.TypeId == (int)AccountTypes.ClientBonusBalance)?.Balance ?? 0;

            result.TotalDepositsCount = Db.PaymentRequests.Count(x => x.ClientId == clientId && x.Type == (int)PaymentRequestTypes.Deposit &&
                                        (x.Status == (int)PaymentRequestStates.Approved || x.Status == (int)PaymentRequestStates.ApprovedManually));
            result.TotalDepositsAmount = Db.PaymentRequests.Where(x => x.ClientId == clientId && x.Type == (int)PaymentRequestTypes.Deposit &&
                                          (x.Status == (int)PaymentRequestStates.Approved || x.Status == (int)PaymentRequestStates.ApprovedManually))
                                        .Select(x => x.Amount).AsEnumerable().DefaultIfEmpty(0).Sum();
            result.TotalWithdrawalsCount = Db.PaymentRequests.Count(x => x.ClientId == clientId && x.Type == (int)PaymentRequestTypes.Withdraw &&
                                          (x.Status == (int)PaymentRequestStates.Approved || x.Status == (int)PaymentRequestStates.ApprovedManually));
            result.TotalWithdrawalsAmount = Db.PaymentRequests.Where(x => x.ClientId == clientId && x.Type == (int)PaymentRequestTypes.Withdraw &&
                                            (x.Status == (int)PaymentRequestStates.Approved || x.Status == (int)PaymentRequestStates.ApprovedManually))
                                            .Select(x => x.Amount).AsEnumerable().DefaultIfEmpty(0).Sum();
            result.FailedDepositsAmount = Db.PaymentRequests.Where(x => x.ClientId == clientId && x.Type == (int)PaymentRequestTypes.Deposit &&
                                            x.Status == (int)PaymentRequestStates.Failed)
                                            .Select(x => x.Amount).AsEnumerable().DefaultIfEmpty(0).Sum();
            result.TotalDebitCorrection = Db.Documents.Where(x => x.OperationTypeId == (int)OperationTypes.DebitCorrectionOnClient &&
                x.ClientId == clientId).Select(x => x.Amount).AsEnumerable().DefaultIfEmpty(0).Sum();
            result.TotalCreditCorrection = Db.Documents.Where(x => x.OperationTypeId == (int)OperationTypes.CreditCorrectionOnClient &&
                x.ClientId == clientId).Select(x => x.Amount).AsEnumerable().DefaultIfEmpty(0).Sum();

            var risk = result.TotalDepositsAmount + result.TotalDebitCorrection - result.TotalWithdrawalsAmount - result.TotalCreditCorrection - result.Balance;
            result.Risk =(result.TotalDepositsAmount + result.TotalDebitCorrection) <= 0 ? 0 :
                (risk < 0 ? Convert.ToInt32(-2 * Math.Atan(Math.Abs((double)risk)) / Math.PI * 100) :
                Convert.ToInt32(risk * 100 /(result.TotalDepositsAmount + result.TotalDebitCorrection)));
            result.GGR = Db.Bets.Where(x => x.ClientId == clientId).Select(x => x.Rake == null ? x.BetAmount - x.WinAmount : 0).AsEnumerable().DefaultIfEmpty(0).Sum();
            result.Rake = Db.Bets.Where(x => x.ClientId == clientId).Select(x => x.Rake ?? 0).DefaultIfEmpty(0).Sum();
            result.NGR = risk;
            result.Status = client.State;
            result.IsOnline = CacheManager.OnlineClients(CurrencyId).Any(x => x.Id == clientId);
            result.Email = client.Email;

            return result;
        }

        public Client RecoverPassword(int partnerId, string recoveryToken, string newPassword,
             string languageId, List<Common.Models.WebSiteModels.SecurityQuestion> securityQuestions)
        {
            var clientInfo = Db.ClientInfoes.FirstOrDefault(x => x.Data == recoveryToken && x.PartnerId == partnerId);
            if (clientInfo == null)
                throw CreateException(languageId, Constants.Errors.WrongToken);
            if (clientInfo.State == (int)ClientInfoStates.Expired)
                throw CreateException(languageId, Constants.Errors.TokenExpired);
            var client = Db.Clients.First(x => x.Id == clientInfo.ClientId);
            CheckClientSecurityAnswers(client.Id, languageId, securityQuestions);
            VerifyClientPassword(newPassword, client);
            var currentTime = GetServerDate();
            var passwordHash = CommonFunctions.ComputeClientPasswordHash(newPassword, client.Salt);
            client.PasswordHash = passwordHash;
            client.LastUpdateTime = currentTime;
            clientInfo.State = (int)ClientInfoStates.Expired;
            if (clientInfo.Type == (int)ClientInfoTypes.PasswordRecoveryMobileKey && !client.IsMobileNumberVerified)
                client.IsMobileNumberVerified = true;
            if (clientInfo.Type == (int)ClientInfoTypes.PasswordRecoveryEmailKey && !client.IsEmailVerified)
                client.IsEmailVerified = true;
            if (client.State == (int)ClientStates.ForceBlock)
            {
                var previouseState = CacheManager.GetClientSettingByName(client.Id, ClientSettings.ParentState);
                if (previouseState.NumericValue.HasValue)
                    client.State = Convert.ToInt32(previouseState.NumericValue.Value);
                else
                    client.State = (int)ClientStates.Active;
            }
            Db.SaveChanges();
            CacheManager.RemoveClientSetting(client.Id, ClientSettings.ParentState);
            CacheManager.RemoveClientFailedLoginCount(client.Id);
            var blockedForInactivity = CacheManager.GetClientSettingByName(client.Id, ClientSettings.BlockedForInactivity);
            if (blockedForInactivity != null && blockedForInactivity.Id > 0 && blockedForInactivity.NumericValue == 1)
            {
                AddOrUpdateClientSetting(client.Id, Constants.ClientSettings.BlockedForInactivity, 0, string.Empty, null, null, "System");
                CacheManager.RemoveClientSetting(client.Id, ClientSettings.BlockedForInactivity);
            }

            var currDate = currentTime.Year * 10000 + currentTime.Month * 100 + currentTime.Day;
            var clientettings = new ClientCustomSettings
            {
                ClientId = client.Id,
                PasswordChangedDate = currDate
            };
            SaveClientSetting(clientettings);
            return client;
        }

        private void CheckClientSecurityAnswers(int clientId, string languageId, List<Common.Models.WebSiteModels.SecurityQuestion> securityQuestions)
        {
            var sAnswers = Db.ClientSecurityAnswers.Where(x => x.ClientId == clientId).ToList();
            if (sAnswers.Any())
            {
                if (securityQuestions == null || securityQuestions.Count < 2)
                    throw CreateException(languageId, Constants.Errors.WrongSecurityQuestionAnswer);
                foreach (var sa in securityQuestions)
                {
                    var item = sAnswers.FirstOrDefault(x => x.SecurityQuestionId == sa.Id);
                    if (item == null || item.Answer.ToLower() != sa.Answer.ToLower())
                        throw CreateException(languageId, Constants.Errors.WrongSecurityQuestionAnswer);
                }
            }
        }

        private void VerifyClientFields(Client client, BllPartner partner, string reCaptcha, bool isFromAdmin)
        {
            if (string.IsNullOrWhiteSpace(client.Email) && string.IsNullOrWhiteSpace(client.MobileNumber) && string.IsNullOrWhiteSpace(client.UserName))
                throw CreateException(LanguageId, Constants.Errors.UserNameCantBeEmpty);
            var dbClient =
                Db.Clients.Where(x => x.PartnerId == client.PartnerId
                    && ((!string.IsNullOrEmpty(client.Email) && x.Email.ToLower() == client.Email.ToLower() && x.IsEmailVerified) ||
                        (!string.IsNullOrEmpty(client.MobileNumber) && x.MobileNumber == client.MobileNumber && x.IsMobileNumberVerified) ||
                        (!string.IsNullOrEmpty(client.DocumentNumber) && x.DocumentNumber.ToLower() == client.DocumentNumber.ToLower()))).FirstOrDefault();

            if (dbClient != null)
            {
                if (client.Id == 0 && !string.IsNullOrWhiteSpace(client.Email) && dbClient.Email.ToLower() == client.Email.ToLower())
                    throw CreateException(LanguageId, Constants.Errors.EmailExists);
                if (client.Id == 0 && !string.IsNullOrWhiteSpace(client.MobileNumber) && dbClient.MobileNumber == client.MobileNumber)
                    throw CreateException(LanguageId, Constants.Errors.MobileExists);
                if (client.Id == 0 && !string.IsNullOrWhiteSpace(client.DocumentNumber) && dbClient.DocumentNumber == client.DocumentNumber)
                    throw CreateException(LanguageId, Constants.Errors.ClientDocumentAlreadyExists);
            }

            //UserName checks (can't be valid mobile number or email)
            CheckUserName(client.UserName, partner.Id);

            // Mobile and Email checks (mobile or email field must be filled)
            //if (string.IsNullOrWhiteSpace(client.EmailOrMobile))
            //    throw CreateException(LanguageId, Constants.Errors.EmailOrMobileMustBeFilled);

            if (!string.IsNullOrWhiteSpace(client.Email))
            {
                if (EmailExists(client.Email, partner.Id))
                    throw CreateException(LanguageId, Constants.Errors.EmailExists);
                if (!IsValidEmail(client.Email))
                    throw CreateException(LanguageId, Constants.Errors.InvalidEmail);
            }
            if (!string.IsNullOrWhiteSpace(client.MobileNumber))
            {
                if (MobileExists(client.MobileNumber, partner.Id))
                    throw CreateException(LanguageId, Constants.Errors.MobileExists);
                if (!IsMobileNumber(client.MobileNumber))
                    throw CreateException(LanguageId, Constants.Errors.InvalidMobile);
            }

            //Document checks
            if (!string.IsNullOrWhiteSpace(client.DocumentNumber) && IsClientDocumentExists(client.DocumentNumber, partner.Id))
                throw CreateException(LanguageId, Constants.Errors.ClientDocumentAlreadyExists);

            //check partnerSettings
            if (client.BirthDate > GetServerDate().AddYears(-partner.ClientMinAge)) //change
                throw CreateException(LanguageId, Constants.Errors.InvalidBirthDate);
            if (client.Citizenship.HasValue && !Db.Regions.Any(x => x.Id == client.Citizenship && x.TypeId == (int)RegionTypes.Country))
                throw CreateException(LanguageId, Constants.Errors.RegionNotFound);
            VerifyClientPassword(client.Password, client);
            if (!isFromAdmin)
                CheckSiteCaptcha(partner.Id, reCaptcha);
        }

        private void VerifyAffiliateFields(Client client, BllPartner partner, string reCaptcha, bool isFromAdmin)
        {
            if (string.IsNullOrWhiteSpace(client.Email))
                throw CreateException(LanguageId, Constants.Errors.UserNameCantBeEmpty);
            var dbAffiliate = Db.Affiliates.Where(x => x.PartnerId == client.PartnerId && (x.Email.ToLower() == client.Email.ToLower() ||
                        (!string.IsNullOrEmpty(client.MobileNumber) && x.MobileNumber == client.MobileNumber))).FirstOrDefault();

            if (dbAffiliate != null)
            {
                if (client.Id == 0 && dbAffiliate.Email.ToLower() == client.Email.ToLower())
                    throw CreateException(LanguageId, Constants.Errors.EmailExists);
                if (client.Id == 0 && !string.IsNullOrWhiteSpace(client.MobileNumber) && dbAffiliate.MobileNumber == client.MobileNumber)
                    throw CreateException(LanguageId, Constants.Errors.MobileExists);
            }
            if (EmailExists(client.Email, partner.Id))
                throw CreateException(LanguageId, Constants.Errors.EmailExists);
            if (!IsValidEmail(client.Email))
                throw CreateException(LanguageId, Constants.Errors.InvalidEmail);

            if (!string.IsNullOrWhiteSpace(client.MobileNumber))
            {
                if (MobileExists(client.MobileNumber, partner.Id))
                    throw CreateException(LanguageId, Constants.Errors.MobileExists);
                if (!IsMobileNumber(client.MobileNumber))
                    throw CreateException(LanguageId, Constants.Errors.InvalidMobile);
            }

            //check partnerSettings
            if (client.BirthDate > GetServerDate().AddYears(-partner.ClientMinAge)) //change
                throw CreateException(LanguageId, Constants.Errors.InvalidBirthDate);

            VerifyClientPassword(client.Password, client);
            if (!isFromAdmin)
                CheckSiteCaptcha(partner.Id, reCaptcha);
        }


        private void CheckSiteCaptcha(int partnerId, string reCaptcha)
        {
            if (CacheManager.GetConfigKey(partnerId, Constants.PartnerKeys.CaptchaEnabled) != "0")
            {
                var captchaResponse = CaptchaHelpers.CallCaptchaApi(reCaptcha, new SessionIdentity { LanguageId = Identity.LanguageId, PartnerId = partnerId });
                if (!captchaResponse.Success)
                {
                    Log.Info(JsonConvert.SerializeObject(captchaResponse));
                    throw CreateException(Identity.LanguageId, Constants.Errors.InvalidSecretKey);
                }
            }
        }

        private bool IsBonusTypeAvailableForPartner(int partnerId, int type)
        {
            var currentTime = DateTime.UtcNow;
            var bonus = Db.Bonus.Where(x => x.Status && x.PartnerId == partnerId &&
                                       x.StartTime <= currentTime && x.FinishTime > currentTime &&
                                      (!x.MaxGranted.HasValue || x.TotalGranted < x.MaxGranted) &&
                                      (!x.MaxReceiversCount.HasValue || x.TotalReceiversCount < x.MaxReceiversCount) &&
                                       x.BonusType == type).FirstOrDefault();
            return (bonus != null);
        }

        public bool EmailExists(string email, int partnerId)
        {
            if (string.IsNullOrEmpty(email)) return false;
            return Db.Clients.Any(x => x.Email == email && x.PartnerId == partnerId);
        }

        public bool MobileExists(string mobile, int partnerId)
        {
            if (string.IsNullOrEmpty(mobile)) return false;
            return Db.Clients.Any(x => x.MobileNumber == mobile && x.PartnerId == partnerId);
        }

        public bool IsClientUserNameExists(string userName, int partnerId)
        {
            return Db.Clients.Any(x => (x.UserName == userName || x.NickName == userName) && x.PartnerId == partnerId);
        }

        private bool IsClientDocumentExists(string documentNumber, int partnerId)
        {
            return Db.Clients.Any(x => x.DocumentNumber == documentNumber && x.IsDocumentVerified && x.PartnerId == partnerId);
        }

        private void CheckUserName(string userName, int partnerId)
        {
            if (!string.IsNullOrEmpty(userName))
            {
                if (IsClientUserNameExists(userName, partnerId))
                    throw CreateException(LanguageId, Constants.Errors.UserNameExists);
                if (userName.Contains("@"))
                    throw CreateException(LanguageId, Constants.Errors.UserNameCanNotContainMailSymbol);
                if (userName.Contains(Constants.ExternalClientPrefix))
                    throw CreateException(LanguageId, Constants.Errors.InvalidUserName);
            }
            var partnerSetting = CacheManager.GetConfigKey(partnerId, Constants.PartnerKeys.AllowDigitalUsername);
            if ((string.IsNullOrEmpty(partnerSetting) || partnerSetting == "0") && IsMobileNumber(userName))
                throw CreateException(LanguageId, Constants.Errors.UserNameMustContainCharacter);
            if (!Regex.IsMatch(userName, UserNameRegEx))
                throw CreateException(LanguageId, Constants.Errors.InvalidUserName);
        }

        private ClientSession AddClientSession(ClientSession session, string token = "")
        {
            session.State = (int)SessionStates.Active;
            session.StartTime = GetServerDate();
            session.LastUpdateTime = session.StartTime;
            session.Token = string.IsNullOrEmpty(token) ? GetToken() : token;
            Db.ClientSessions.Add(session);
            return session;
        }

        private void CreateFilterForGetClients(FilterClient filter)
        {
            var clientAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewClient,
                ObjectTypeId = (int)ObjectTypes.Client
            });
            var clientCategoryAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewClientByCategory,
                ObjectTypeId = (int)ObjectTypes.ClientCategory
            });
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });

            var affiliateReferralAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewAffiliateReferral,
                ObjectTypeId = (int)ObjectTypes.AffiliateReferral
            });

            filter.CheckPermissionResuts = new List<CheckPermissionOutput<Client>>
            {
                new CheckPermissionOutput<Client>
                {
                    AccessibleObjects = clientAccess.AccessibleObjects,
                    HaveAccessForAllObjects = clientAccess.HaveAccessForAllObjects,
                    Filter = x => clientAccess.AccessibleObjects.AsEnumerable().Contains(x.Id)
                },
                new CheckPermissionOutput<Client>
                {
                    AccessibleObjects = clientCategoryAccess.AccessibleObjects,
                    HaveAccessForAllObjects = clientCategoryAccess.HaveAccessForAllObjects,
                    Filter = x => clientCategoryAccess.AccessibleObjects.AsEnumerable().Contains(x.CategoryId)
                },
                new CheckPermissionOutput<Client>
                {
                    AccessibleObjects = partnerAccess.AccessibleObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleObjects.AsEnumerable().Contains(x.PartnerId)
                },
                new CheckPermissionOutput<Client>
                {
                    AccessibleObjects = affiliateReferralAccess.AccessibleObjects,
                    HaveAccessForAllObjects = affiliateReferralAccess.HaveAccessForAllObjects,
                    Filter = x =>  x.AffiliateReferralId.HasValue && affiliateReferralAccess.AccessibleObjects.AsEnumerable().Contains(x.AffiliateReferralId.Value)
                }
            };
        }

        private void CreateFilterForGetfnClients(FilterfnClient filter)
        {
            var clientAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewClient,
                ObjectTypeId = (int)ObjectTypes.Client
            });
            var clientCategoryAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewClientByCategory,
                ObjectTypeId = (int)ObjectTypes.ClientCategory
            });
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });

            var affiliateReferralAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewAffiliateReferral,
                ObjectTypeId = (int)ObjectTypes.AffiliateReferral
            });

            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnClient>>
            {
                new CheckPermissionOutput<fnClient>
                {
                    AccessibleObjects = clientAccess.AccessibleObjects,
                    HaveAccessForAllObjects = clientAccess.HaveAccessForAllObjects,
                    Filter = x => clientAccess.AccessibleObjects.AsEnumerable().Contains(x.Id)
                },
                new CheckPermissionOutput<fnClient>
                {
                    AccessibleObjects = clientCategoryAccess.AccessibleObjects,
                    HaveAccessForAllObjects = clientCategoryAccess.HaveAccessForAllObjects,
                    Filter = x => clientCategoryAccess.AccessibleObjects.AsEnumerable().Contains(x.CategoryId)
                },
                new CheckPermissionOutput<fnClient>
                {
                    AccessibleObjects = partnerAccess.AccessibleObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleObjects.AsEnumerable().Contains(x.PartnerId)
                },
                new CheckPermissionOutput<fnClient>
                {
                    AccessibleObjects = affiliateReferralAccess.AccessibleObjects,
                    HaveAccessForAllObjects = affiliateReferralAccess.HaveAccessForAllObjects,
                    Filter = x =>  x.AffiliatePlatformId.HasValue && affiliateReferralAccess.AccessibleObjects.AsEnumerable().Contains(x.AffiliatePlatformId.Value)
                }
            };
        }

        public Note SaveNote(Note note)
        {
            int clientId = Convert.ToInt32(note.ObjectId);
            var client = Db.Clients.FirstOrDefault(x => x.Id == clientId);
            if (client == null)
                throw CreateException(LanguageId, Constants.Errors.ClientNotFound);

            var clientAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewClient,
                ObjectTypeId = (int)ObjectTypes.Client
            });
            var noteAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.CreateNote,
                ObjectTypeId = (int)ObjectTypes.Client
            });
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });
            var affiliateAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewAffiliateReferral,
                ObjectTypeId = (int)ObjectTypes.AffiliateReferral
            });
            if ((!clientAccess.HaveAccessForAllObjects && clientAccess.AccessibleObjects.All(x => x != clientId)) ||
                (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleObjects.All(x => x != client.PartnerId)) || !noteAccess.HaveAccessForAllObjects ||
                (!affiliateAccess.HaveAccessForAllObjects &&
                  affiliateAccess.AccessibleObjects.All(x => client.AffiliateReferralId.HasValue && x != client.AffiliateReferralId.Value))
               )
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);

            var currentTime = GetServerDate();
            var dbNote = Db.Notes.FirstOrDefault(x => x.Id == note.Id);

            if (dbNote == null)
            {
                dbNote = new Note { CreationTime = currentTime, SessionId = SessionId };
                Db.Notes.Add(dbNote);
            }
            note.CreationTime = dbNote.CreationTime;
            note.SessionId = dbNote.SessionId;
            note.LastUpdateTime = currentTime;
            Db.Entry(dbNote).CurrentValues.SetValues(note);

            if (!client.HasNote && note.Type == (int)NoteTypes.Standard && note.State == (int)NoteStates.Active)
            {
                client.HasNote = true;
                client.LastUpdateTime = currentTime;
            }
            else if (client.HasNote && note.Type == (int)NoteTypes.Standard && note.State == (int)NoteStates.Deleted)
            {
                var notesCount = Db.Notes.Where(x => x.ObjectTypeId == (int)ObjectTypes.Client && x.ObjectId == client.Id &&
                    x.Id != note.Id && x.State == (int)NoteStates.Active).Count();
                if (notesCount == 0)
                    client.HasNote = false;
            }
            Db.SaveChanges();
            return dbNote;
        }

        public List<AccountsBalanceHistoryElement> GetClientAccountsBalanceHistoryPaging(FilterAccountsBalanceHistory filter)
        {
            var client = CacheManager.GetClientById(filter.ClientId);
            if (client == null)
                throw CreateException(LanguageId, Constants.Errors.ClientNotFound);

            var checkClientPermission = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewClient,
                ObjectTypeId = (int)ObjectTypes.Client
            });

            var checkPartnerPermission = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });

            var affiliateReferralAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewAffiliateReferral,
                ObjectTypeId = (int)ObjectTypes.AffiliateReferral
            });
            if ((!checkClientPermission.HaveAccessForAllObjects && checkClientPermission.AccessibleObjects.All(x => x != filter.ClientId)) ||
                (!checkPartnerPermission.HaveAccessForAllObjects && checkPartnerPermission.AccessibleObjects.All(x => x != client.PartnerId)) ||
                (!affiliateReferralAccess.HaveAccessForAllObjects &&
                  affiliateReferralAccess.AccessibleObjects.All(x => client.AffiliateReferralId.HasValue && x != client.AffiliateReferralId.Value)))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);

            var accountTypeKinds = GetEnumerations(Constants.EnumerationTypes.AccountTypeKinds, LanguageId).Select(x => new
            {
                Id = x.Value,
                Name = x.Text
            }).ToList();
            var operationTypes = GetOperationTypes();
            var balances = GetAccountsBalances((int)ObjectTypes.Client, filter.ClientId, filter.FromDate);
            var fDate = filter.FromDate.Year * 1000000 + filter.FromDate.Month * 10000 + filter.FromDate.Day * 100 + filter.FromDate.Hour;
            var tDate = filter.ToDate.Year * 1000000 + filter.ToDate.Month * 10000 + filter.ToDate.Day * 100 + filter.ToDate.Hour;
            var operations =
                Db.Transactions.Include(x => x.Account.Type).Include(x => x.Document.PartnerPaymentSetting)
                    .Where(x => x.Account.ObjectTypeId == (int)ObjectTypes.Client &&
                                x.Account.ObjectId == filter.ClientId && x.Date >= fDate &&
                                x.Date < tDate).ToList()
                    .OrderBy(x => x.CreationTime)
                    .ThenBy(x => x.Account.TypeId)
                    .ToList();

            var result = new List<AccountsBalanceHistoryElement>();
            foreach (var operation in operations)
            {
                if (operation.Amount > 0)
                {
                    var balance = balances.First(x => x.AccountId == operation.AccountId);
                    var pId = operation.Document?.PartnerPaymentSetting?.PaymentSystemId;
                    result.Add(new AccountsBalanceHistoryElement
                    {
                        TransactionId = operation.Id,
                        DocumentId = operation.DocumentId,
                        AccountId = operation.AccountId,
                        AccountType = accountTypeKinds.First(x => x.Id == operation.Account.Type.Kind).Name,
                        BalanceBefore = balance.Balance,
                        OperationType = operationTypes.First(x => x.Id == operation.OperationTypeId).Name,
                        OperationAmount = operation.Amount,
                        BalanceAfter = balance.Balance + (operation.Type == (int)TransactionTypes.Credit ? -operation.Amount : operation.Amount),
                        OperationTime = operation.CreationTime,
                        PaymentSystemName = pId == null ? string.Empty : CacheManager.GetPaymentSystemById(pId.Value).Name
                    });
                    balance.Balance += (operation.Type == (int)TransactionTypes.Credit ? -operation.Amount : operation.Amount);
                }
            }
            return result;
        }

        public PagedModel<fnClientLog> GetClientLogs(FilterfnClientLog filter)
        {
            var clientAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewClient,
                ObjectTypeId = (int)ObjectTypes.Client
            });
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });

            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnClientLog>>
            {
                new CheckPermissionOutput<fnClientLog>
                {
                    AccessibleObjects = clientAccess.AccessibleObjects,
                    HaveAccessForAllObjects = clientAccess.HaveAccessForAllObjects,
                    Filter = x => clientAccess.AccessibleObjects.AsEnumerable().Contains(x.Id)
                }//,
                //new CheckPermissionOutput<fnClientLog>
                //{
                //    AccessibleObjects = partnerAccess.AccessibleObjects,
                //    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                //    Filter = x => partnerAccess.AccessibleObjects.AsEnumerable().Contains(x.PartnerId)
                //}
            };
            var clientLogs = new PagedModel<fnClientLog>
            {
                Entities = filter.FilterObjects(Db.fn_ClientLog(), logs => logs.OrderByDescending(y => y.Id)),
                Count = filter.SelectedObjectsCount(Db.fn_ClientLog())
            };
            return clientLogs;
        }

        private void CheckClientKYCState(int clientId)
        {
            var client = CacheManager.GetClientById(clientId);
            var partnerConfig = CacheManager.GetConfigKey(client.PartnerId, Constants.PartnerKeys.PartnerKYCTypes);
            if (!string.IsNullOrEmpty(partnerConfig))
            {
                var requiredDocumentTypes = JsonConvert.DeserializeObject<List<int>>(partnerConfig);
                if (requiredDocumentTypes.Any())
                    if (Db.ClientIdentities.Where(x => x.ClientId == clientId && requiredDocumentTypes.Contains(x.DocumentTypeId) &&
                                                       x.Status == (int)KYCDocumentStates.Approved)
                                           .Select(x => x.DocumentTypeId).OrderBy(x => x).ToList()
                                           .Intersect(requiredDocumentTypes).SequenceEqual(requiredDocumentTypes))
                        Db.Clients.Where(x => x.Id == client.Id).UpdateFromQuery(x => new Client { IsDocumentVerified = true });

                    else
                        Db.Clients.Where(x => x.Id == client.Id).UpdateFromQuery(x => new Client { IsDocumentVerified = false });
            }
        }

        public ClientIdentity SaveKYCDocument(ClientIdentity input, string documentName, byte[] imageData, bool checkPermission, IWebHostEnvironment env)
        {
            var client = CacheManager.GetClientById(input.ClientId);
            if (client == null)
                throw CreateException(LanguageId, Constants.Errors.ClientNotFound);
            var docSavePath = string.Empty;
            if (checkPermission)
            {
                var clientAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewClient,
                    ObjectTypeId = (int)ObjectTypes.Client
                });
                var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewPartner,
                    ObjectTypeId = (int)ObjectTypes.Partner
                });
                GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.CreateClientIdentity
                });

                var affiliateReferralAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewAffiliateReferral,
                    ObjectTypeId = (int)ObjectTypes.AffiliateReferral
                });
                if ((!clientAccess.HaveAccessForAllObjects && clientAccess.AccessibleObjects.All(x => x != input.ClientId)) ||
                    (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleObjects.All(x => x != client.PartnerId)) ||
                    (!affiliateReferralAccess.HaveAccessForAllObjects &&
                      affiliateReferralAccess.AccessibleObjects.All(x => client.AffiliateReferralId.HasValue && x != client.AffiliateReferralId.Value)))
                    throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            }
            else
                input.Status = (int)KYCDocumentStates.InProcess;
            if (!Enum.IsDefined(typeof(KYCDocumentTypes), input.DocumentTypeId) ||
                !Enum.IsDefined(typeof(KYCDocumentStates), input.Status))
                throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);
            var currentTime = DateTime.UtcNow;
            ClientIdentity dbClientIdentity;
            if (checkPermission && input.Id > 0)
            {
                dbClientIdentity = Db.ClientIdentities.FirstOrDefault(x => x.Id == input.Id);
                if (dbClientIdentity == null)
                    throw CreateException(LanguageId, Constants.Errors.ClientIdentityNotFound);
                var oldClientIdentity = new
                {
                    dbClientIdentity.Id,
                    dbClientIdentity.ClientId,
                    UserId = Identity.Id,
                    dbClientIdentity.DocumentTypeId,
                    dbClientIdentity.ImagePath,
                    dbClientIdentity.Status,
                    dbClientIdentity.CreationTime,
                    dbClientIdentity.LastUpdateTime,
                    dbClientIdentity.ExpirationTime
                };

                dbClientIdentity.Status = input.Status;
                dbClientIdentity.LastUpdateTime = currentTime;
                dbClientIdentity.DocumentTypeId = input.DocumentTypeId;
                dbClientIdentity.UserId = Identity.Id;
                dbClientIdentity.ExpirationTime = input.ExpirationTime;
                dbClientIdentity.ExpirationDate = input.ExpirationTime.HasValue ? input.ExpirationTime.Value.Year * 10000 + input.ExpirationTime.Value.Month * 100 +
                                 input.ExpirationTime.Value.Day : (long?)null;
                SaveChangesWithHistory((int)ObjectTypes.ClientIdentity, dbClientIdentity.Id, JsonConvert.SerializeObject(oldClientIdentity), string.Empty);
            }
            else
            {
                if (!checkPermission && Db.ClientIdentities.FirstOrDefault(x => x.ClientId == input.ClientId &&
                                                                             x.DocumentTypeId == input.DocumentTypeId &&
                                                                             x.Status == (int)KYCDocumentStates.Approved) != null)
                    throw BaseBll.CreateException(LanguageId, Constants.Errors.ClientDocumentAlreadyExists);

                dbClientIdentity = new ClientIdentity
                {
                    ClientId = input.ClientId,
                    UserId = checkPermission ? Identity.Id : (int?)null,
                    CreationTime = currentTime,
                    LastUpdateTime = currentTime,
                    ImagePath = docSavePath,
                    DocumentTypeId = input.DocumentTypeId,
                    Status = input.Status,
                    ExpirationTime = input.ExpirationTime,
                    ExpirationDate = (input.ExpirationTime.HasValue && input.ExpirationTime != DateTime.MinValue) ?
                                     input.ExpirationTime.Value.Year * 10000 +input.ExpirationTime.Value.Month * 100 +input.ExpirationTime.Value.Day : (long?)null
                };

                Db.ClientIdentities.Add(dbClientIdentity);
                Db.SaveChanges();
            }
            if (imageData != null)
            {
                var currentPath = env.ContentRootPath;
                var parentPath = Path.GetDirectoryName(currentPath);
                string[] paths = { Path.GetDirectoryName(parentPath), "AdminWebApi", "ClientDocuments" };
                var localPath = Path.Combine(paths);
                if (!Directory.Exists(localPath))
                    Directory.CreateDirectory(localPath);
                var docName = string.Format("{0}_{1}_{2}", Guid.NewGuid().ToString(), input.ClientId.ToString(), documentName);
                docSavePath = Path.Combine(localPath, docName);
                try
                {
                    CommonFunctions.SaveImage(imageData, docSavePath);
                }
                catch
                {
                    if (!docSavePath.Contains(".pdf"))
                        docSavePath = docSavePath.Substring(0, docSavePath.LastIndexOf('.') + 1) + "pdf";
                    if (!docName.Contains(".pdf"))
                        docName = docName.Substring(0, docName.LastIndexOf('.') + 1) + "pdf";
                    GeneratePDF(docSavePath, imageData);
                }
                dbClientIdentity.ImagePath = String.Format("ClientDocuments/{0}", docName);
                Db.SaveChanges();
            }
            if (checkPermission)
            {
                CheckClientKYCState(client.Id);
                CacheManager.RemoveClientFromCache(input.ClientId);
            }
            return dbClientIdentity;
        }

        public ClientIdentity RemoveClientIdentity(int clientIdentityId, bool checkPermission)
        {
            var clientIdentity = Db.ClientIdentities.Where(x => x.Id == clientIdentityId).FirstOrDefault();
            if (clientIdentity == null)
                throw CreateException(LanguageId, Constants.Errors.ClientIdentityNotFound);
            var client = CacheManager.GetClientById(clientIdentity.ClientId);
            if (checkPermission)
            {
                var clientAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewClient,
                    ObjectTypeId = (int)ObjectTypes.Client
                });
                var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewPartner,
                    ObjectTypeId = (int)ObjectTypes.Partner
                });
                GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.CreateClientIdentity
                });

                var affiliateReferralAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewAffiliateReferral,
                    ObjectTypeId = (int)ObjectTypes.AffiliateReferral
                });
                if ((!clientAccess.HaveAccessForAllObjects && clientAccess.AccessibleObjects.All(x => x != client.Id)) ||
                    (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleObjects.All(x => x != client.PartnerId)) ||
                    (!affiliateReferralAccess.HaveAccessForAllObjects &&
                      affiliateReferralAccess.AccessibleObjects.All(x => client.AffiliateReferralId.HasValue && x != client.AffiliateReferralId.Value)))
                    throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            }
            var oldClientIdentity = new
            {
                Id = clientIdentity.Id,
                ClientId = clientIdentity.ClientId,
                UserId = Identity.Id,
                DocumentTypeId = clientIdentity.DocumentTypeId,
                ImagePath = clientIdentity.ImagePath,
                Status = clientIdentity.Status,
                CreationTime = clientIdentity.CreationTime,
                LastUpdateTime = clientIdentity.LastUpdateTime,
                ExpirationTime = clientIdentity.ExpirationTime
            };
            Db.ClientIdentities.Where(x => x.Id == clientIdentityId).DeleteFromQuery();
            SaveChangesWithHistory((int)ObjectTypes.ClientIdentity, clientIdentity.Id, JsonConvert.SerializeObject(oldClientIdentity), "Delete");
            return clientIdentity;
        }

        public List<fnClientIdentity> GetClientIdentityInfo(int clientId, bool checkPermission)
        {
            var client = CacheManager.GetClientById(clientId);
            if (client == null)
                throw CreateException(LanguageId, Constants.Errors.ClientNotFound);
            if (checkPermission)
            {
                GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewClientIdentity
                });

                var clientAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewClient,
                    ObjectTypeId = (int)ObjectTypes.Client
                });
                var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewPartner,
                    ObjectTypeId = (int)ObjectTypes.Partner
                });
                var affiliateAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewAffiliateReferral,
                    ObjectTypeId = (int)ObjectTypes.AffiliateReferral
                });
                if ((!clientAccess.HaveAccessForAllObjects && clientAccess.AccessibleObjects.All(x => x != clientId)) ||
                    (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleObjects.All(x => x != client.PartnerId)) ||
                    (!affiliateAccess.HaveAccessForAllObjects &&
                      affiliateAccess.AccessibleObjects.All(x => client.AffiliateReferralId.HasValue && x != client.AffiliateReferralId.Value)))
                    throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            }
            return Db.fn_ClientIdentity().Where(x => x.ClientId == clientId).ToList();
        }
        public List<ClientIdentity> GetClientIdentities(int clientId)
        {
            return Db.ClientIdentities.Where(x => x.ClientId == clientId).ToList();
        }

        public ResponseBase SetPaymentLimit(PaymentLimit paymentLimit, bool checkPermission)
        {
            var client = CacheManager.GetClientById(paymentLimit.ClientId);
            if (client == null)
                throw CreateException(LanguageId, Constants.Errors.ClientNotFound);
            if (checkPermission)
            {
                var clientAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.EditClient,
                    ObjectTypeId = (int)ObjectTypes.Client
                });
                var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewPartner,
                    ObjectTypeId = (int)ObjectTypes.Partner
                });
                var affiliateAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewAffiliateReferral,
                    ObjectTypeId = (int)ObjectTypes.AffiliateReferral
                });
                if ((!clientAccess.HaveAccessForAllObjects && clientAccess.AccessibleObjects.All(x => x != client.Id)) ||
                      (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleObjects.All(x => x != client.PartnerId)) ||
                      (!affiliateAccess.HaveAccessForAllObjects &&
                        affiliateAccess.AccessibleObjects.All(x => client.AffiliateReferralId.HasValue && x != client.AffiliateReferralId.Value)))
                    throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            }
            if ((paymentLimit.MaxDepositsCountPerDay.HasValue && paymentLimit.MaxDepositsCountPerDay.Value < 0) ||
                (paymentLimit.MaxDepositAmount.HasValue && paymentLimit.MaxDepositAmount.Value < 0) ||
                (paymentLimit.MaxTotalDepositsAmountPerDay.HasValue && paymentLimit.MaxTotalDepositsAmountPerDay.Value < 0) ||
                (paymentLimit.MaxTotalDepositsAmountPerWeek.HasValue && paymentLimit.MaxTotalDepositsAmountPerWeek.Value < 0) ||
                (paymentLimit.MaxTotalDepositsAmountPerMonth.HasValue && paymentLimit.MaxTotalDepositsAmountPerMonth.Value < 0) ||
                (paymentLimit.MaxWithdrawAmount.HasValue && paymentLimit.MaxWithdrawAmount.Value < 0) ||
                (paymentLimit.MaxTotalWithdrawsAmountPerDay.HasValue && paymentLimit.MaxTotalWithdrawsAmountPerDay.Value < 0) ||
                (paymentLimit.MaxTotalWithdrawsAmountPerWeek.HasValue && paymentLimit.MaxTotalWithdrawsAmountPerWeek.Value < 0) ||
                (paymentLimit.MaxTotalWithdrawsAmountPerMonth.HasValue && paymentLimit.MaxTotalWithdrawsAmountPerMonth.Value < 0))
                throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);
            var dbPaymentLimit = Db.PaymentLimits.FirstOrDefault(x => x.ClientId == paymentLimit.ClientId);
            if (dbPaymentLimit == null)
                Db.PaymentLimits.Add(paymentLimit);
            else
            {
                paymentLimit.Id = dbPaymentLimit.Id;
                Db.Entry(dbPaymentLimit).CurrentValues.SetValues(paymentLimit);
            }
            Db.SaveChanges();
            return new ResponseBase();
        }
        public PaymentLimit GetPaymentLimit(int clientId)
        {
            var client = CacheManager.GetClientById(clientId);
            if (client == null)
                throw CreateException(LanguageId, Constants.Errors.ClientNotFound);
            var checkClientPermission = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewClient,
                ObjectTypeId = (int)ObjectTypes.Client
            });

            var checkPartnerPermission = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });

            var affiliateAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewAffiliateReferral,
                ObjectTypeId = (int)ObjectTypes.AffiliateReferral
            });
            if ((!checkClientPermission.HaveAccessForAllObjects && checkClientPermission.AccessibleObjects.All(x => x != clientId)) ||
                (!checkPartnerPermission.HaveAccessForAllObjects && checkPartnerPermission.AccessibleObjects.All(x => x != client.PartnerId)) ||
                (!affiliateAccess.HaveAccessForAllObjects &&
                  affiliateAccess.AccessibleObjects.All(x => client.AffiliateReferralId.HasValue && x != client.AffiliateReferralId.Value)))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            return Db.PaymentLimits.FirstOrDefault(x => x.ClientId == clientId);
        }

        public List<fnAccountType> GetAccountTypes(string languageId)
        {
            return Db.fn_AccountType(languageId).ToList();
        }

        public List<ClientProductBet> GetCashBackBonusBets(int partnerId, DateTime fromDate, DateTime toDate)
        {
            return (from b in Db.Bets
                    where b.Client.PartnerId == partnerId && b.CalculationTime != null && b.CalculationTime >= fromDate &&
                          b.CalculationTime < toDate && b.BonusId == null && b.ClientId.HasValue
                    group b by new { b.ClientId, b.ProductId, b.CurrencyId } into groupedBets
                    select new ClientProductBet
                    {
                        ClientId = groupedBets.Key.ClientId.Value,
                        CurrencyId = groupedBets.Key.CurrencyId,
                        ProductId = groupedBets.Key.ProductId,
                        Amount = groupedBets.Sum(x => x.BetAmount - x.WinAmount),
                        Percent = 0
                    }).ToList();
        }

        #region Export to excel


        public List<fnClient> ExportClients(FilterfnClient filter)
        {
            var exportAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ExportClients
            });

            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnClient>>
            {
                new CheckPermissionOutput<fnClient>
                {
                    AccessibleObjects = exportAccess.AccessibleObjects,
                    HaveAccessForAllObjects = exportAccess.HaveAccessForAllObjects,
                    Filter = x => exportAccess.AccessibleObjects.AsEnumerable().Contains(x.PartnerId)
                }
            };

            filter.TakeCount = 0;
            filter.SkipCount = 0;

            CreateFilterForGetfnClients(filter);

            var result = filter.FilterObjects(Db.fn_Client(), clients => clients.OrderByDescending(x => x.Id)).ToList();
            return result;
        }

        public List<fnClientIdentity> ExportClientIdentity(int clientId)
        {
            return Db.fn_ClientIdentity().Where(x => x.ClientId == clientId).ToList();
        }

        #endregion

        public void ChangeClientDetails(Client client, out bool isBonusAccepted, out bool isSessionExpired)
        {
            using var bonusBl = new BonusService(this);
            using var documentBl = new DocumentBll(this);
            using var transactionScope = CommonFunctions.CreateTransactionScope();
            Db.Procedures.sp_GetClientLockAsync(client.Id).Wait();
            CheckPermission(Constants.Permissions.EditClient);
            isBonusAccepted = false;
            isSessionExpired = false;
            var result = Db.Clients.Where(c => c.Id == client.Id).First();
            var codeId = result.Id.ToString();
            if (client.UserId != result.UserId)
            {
                if (client.UserId.HasValue)
                {
                    var user = CacheManager.GetUserById(client.UserId.Value);
                    if (user == null)
                        throw CreateException(LanguageId, Constants.Errors.UserNotFound);
                    if (result.PartnerId != user.PartnerId || result.CurrencyId != user.CurrencyId ||
                       (user.Type != (int)UserTypes.MasterAgent && user.Type != (int)UserTypes.Agent))
                        throw CreateException(LanguageId, Constants.Errors.NotAllowed);
                }
                result.UserId = client.UserId;
            }
            var oldValue = JsonConvert.SerializeObject(result.ToClientInfo());
            result.IsEmailVerified = client.IsEmailVerified;
            if (result.Email != client.Email)
            {
                result.Email = client.Email;
                result.IsEmailVerified = false;
            }
            result.IsMobileNumberVerified = client.IsMobileNumberVerified;
            if (result.MobileNumber != client.MobileNumber)
            {
                result.MobileNumber = client.MobileNumber;
                result.IsMobileNumberVerified = false;
            }
            if (result.State != client.State)
            {
                if (result.State == (int)ClientStates.Disabled)
                    throw CreateException(LanguageId, Constants.Errors.NotAllowed);
                result.State = client.State;
                if (result.State == (int)ClientStates.FullBlocked || result.State == (int)ClientStates.Disabled)
                {
                    LogoutClientById(client.Id, (int)LogoutTypes.Admin);
                    isSessionExpired = true;
                }
                var oldSettings = GetClientsSettings(client.Id, false).Select(x => new
                {
                    x.Name,
                    StringValue = string.IsNullOrEmpty(x.StringValue) ?
                    (x.NumericValue.HasValue ? x.NumericValue.Value.ToString() : String.Empty) : x.StringValue,
                    DateValue = x.DateValue ?? x.CreationTime,
                    LastUpdateTime = x.LastUpdateTime
                }).ToList();
                SaveChangesWithHistory((int)ObjectTypes.ClientSetting, client.Id, JsonConvert.SerializeObject(oldSettings), string.Empty);
            }

            result.Gender = client.Gender;
            result.BirthDate = client.BirthDate;
            result.SendMail = client.SendMail;
            result.SendSms = client.SendSms;
            result.CallToPhone = client.CallToPhone;
            result.SendPromotions = client.SendPromotions;
            result.City = client.City;
            result.FirstName = client.FirstName;
            result.LastName = client.LastName;
            result.NickName = client.NickName;
            result.SecondName = client.SecondName;
            result.SecondSurname = client.SecondSurname;
            result.RegionId = client.RegionId;
            result.ZipCode = client.ZipCode;
            if (!string.IsNullOrEmpty(client.DocumentNumber) &&
                Db.Clients.Any(x => x.DocumentNumber == client.DocumentNumber && x.IsDocumentVerified && x.PartnerId == result.PartnerId && x.Id != client.Id))
                throw CreateException(LanguageId, Constants.Errors.ClientDocumentAlreadyExists);
            result.DocumentNumber = client.DocumentNumber;
            result.DocumentIssuedBy = client.DocumentIssuedBy;
            result.Address = client.Address;
            result.PhoneNumber = client.PhoneNumber;
            result.LanguageId = client.LanguageId;
            result.CategoryId = client.CategoryId;
            result.Comment = client.Comment;
            result.DocumentType = client.DocumentType;
            result.Info = client.Info;
            result.BetShopId = client.BetShopId;
            result.Citizenship = client.Citizenship;
            result.JobArea = client.JobArea;
            result.BuildingNumber = client.BuildingNumber;
            result.Apartment = client.Apartment;

            if (result.IsDocumentVerified != client.IsDocumentVerified)
            {
                result.IsDocumentVerified = client.IsDocumentVerified;
                if (client.IsDocumentVerified)
                {
                    if (string.IsNullOrEmpty(result.DocumentNumber) ||
                    Db.Clients.Any(x => x.DocumentNumber == result.DocumentNumber && x.IsDocumentVerified && x.PartnerId == result.PartnerId && x.Id != client.Id))
                        throw CreateException(LanguageId, Constants.Errors.WrongDocumentNumber);

                    client.PartnerId = result.PartnerId;
                    bonusBl.GiveWelcomeRealBonus(client, documentBl);
                    isBonusAccepted = true;
                }
                var oldSettings = GetClientsSettings(client.Id, false).Select(x => new
                {
                    x.Name,
                    StringValue = string.IsNullOrEmpty(x.StringValue) ?
                                                (x.NumericValue.HasValue ? x.NumericValue.Value.ToString() : String.Empty) : x.StringValue,
                    DateValue = x.DateValue ?? x.CreationTime,
                    LastUpdateTime = x.LastUpdateTime
                }).ToList();
                SaveChangesWithHistory((int)ObjectTypes.ClientSetting, client.Id, JsonConvert.SerializeObject(oldSettings), string.Empty);
            }
            AddClientJobTrigger(client.Id, (int)JobTriggerTypes.ReconsiderSegments);
            SaveChangesWithHistory((int)ObjectTypes.Client, client.Id, oldValue, client.Comment);

            transactionScope.Complete();
        }

        public static string LoginClientByCard(int partnerId, string clientCardNumber, string ip, string languageId, out int clientId)
        {
            using var db = CreateEntities();
            Client client = null;
            client = db.Clients.FirstOrDefault(x => x.PartnerId == partnerId && x.Info == clientCardNumber);

            if (client == null)
                throw CreateException(languageId, Constants.Errors.ClientNotFound);
            if (client.State == (int)ClientStates.FullBlocked || client.State == (int)ClientStates.Disabled)
                throw CreateException(languageId, Constants.Errors.ClientBlocked);
            clientId = client.Id;
            var currentTime = DateTime.UtcNow;
            var sessions = db.ClientSessions.Where(x => x.ClientId == client.Id && x.ProductId == Constants.PlatformProductId && x.State == (int)SessionStates.Active);
            sessions.UpdateFromQuery(x => new ClientSession { State = (int)SessionStates.Inactive, LogoutType = (int)LogoutTypes.MultipleDevice, EndTime = currentTime });
            var lastSession = sessions.OrderByDescending(x => x.Id).FirstOrDefault();
            client.LastSessionId = lastSession.Id;
            var session = new ClientSession
            {
                ClientId = client.Id,
                ProductId = Constants.PlatformProductId,
                LanguageId = languageId,
                Ip = ip,
                State = (int)SessionStates.Active,
                StartTime = currentTime,
                LastUpdateTime = currentTime,
                Token = GetToken()
            };

            db.ClientSessions.Add(session);
            db.SaveChanges();
            CacheManager.RemoveClientPlatformSession(client.Id);
            foreach (var s in sessions)
            {
                CacheManager.RemoveClientProductSession(s.Token, Constants.PlatformProductId);
            }
            CacheManager.RemoveClientFromCache(client.Id);
            var codeId = client.Id.ToString();
            return session.Token;
        }

        public static void BlockClientForce(int clientId)
        {
            using (var db = new IqSoftCorePlatformEntities())
            {
                var currentTime = DateTime.UtcNow;
                var objectAction = new ObjectAction
                {
                    ObjectId = clientId,
                    ObjectTypeId = (int)ObjectTypes.Client,
                    Type = (int)ObjectActionTypes.BlockClientForce,
                    State = (int)BaseStates.Active,
                    StartTime = currentTime,
                    FinishTime = currentTime.AddHours(1)
                };
                var dbAction = db.ObjectActions.Where(x => x.ObjectId == clientId && x.ObjectTypeId == (int)ObjectTypes.Client &&
                x.Type == (int)ObjectActionTypes.BlockClientForce && x.State == (int)BaseStates.Active).FirstOrDefault();
                if (dbAction == null)
                    db.ObjectActions.Add(objectAction);
                var dbClient = db.Clients.FirstOrDefault(x => x.Id == clientId);
                var dbclientStateSetting = db.ClientSettings.FirstOrDefault(x => x.ClientId == dbClient.Id && x.Name == Constants.ClientSettings.ParentState);
                if (dbclientStateSetting != null)
                {
                    dbclientStateSetting.NumericValue = dbClient.State;
                    dbclientStateSetting.LastUpdateTime = currentTime;
                }
                else
                {
                    db.ClientSettings.Add(new ClientSetting
                    {
                        ClientId = dbClient.Id,
                        Name = Constants.ClientSettings.ParentState,
                        NumericValue = dbClient.State,
                        CreationTime = currentTime,
                        LastUpdateTime = currentTime
                    });
                }
                dbClient.State = (int)ClientStates.ForceBlock;
                dbClient.LastUpdateTime = currentTime;

                var clientSetting = new
                {
                    Name = "Excluded",
                    StringValue = "1"
                };
                db.ObjectChangeHistories.Add(new ObjectChangeHistory
                {
                    ChangeDate = DateTime.UtcNow,
                    ObjectId = 0,
                    ObjectTypeId = (int)ObjectTypes.ClientSetting,
                    Object = JsonConvert.SerializeObject(clientSetting),
                    SessionId = (long?)null,
                    Comment = string.Empty
                });
                var jobTrigger = new JobTrigger
                {
                    ClientId = clientId,
                    Type = (int)JobTriggerTypes.ReconsiderSegments
                };
                db.JobTriggers.AddIfNotExists(jobTrigger, x => x.ClientId == clientId && x.Type == (int)JobTriggerTypes.ReconsiderSegments);
                db.SaveChanges();
                CacheManager.RemoveClientFromCache(clientId);
                CacheManager.RemoveClientSetting(clientId, Constants.ClientSettings.ParentState);
            }
        }

        public static void CheckClientStatus(BllClient client, string languageId)
        {
            var currentDate = DateTime.UtcNow;
            if (client.State == (int)ClientStates.ForceBlock)
                throw CreateException(languageId, Constants.Errors.ClientForceBlocked);
            if (client.State == (int)ClientStates.FullBlocked || client.State == (int)ClientStates.Disabled)
                throw CreateException(languageId, Constants.Errors.ClientBlocked);
            var blockedForInactivity = CacheManager.GetClientSettingByName(client.Id, ClientSettings.BlockedForInactivity);
            if (blockedForInactivity != null && blockedForInactivity.Id > 0 && blockedForInactivity.NumericValue == 1)
                throw CreateException(languageId, Constants.Errors.InactivityBlock);
            var currentTime = DateTime.UtcNow;
            var selfExcluded = CacheManager.GetClientSettingByName(client.Id, ClientSettings.SelfExcluded);
            if (selfExcluded != null && selfExcluded.Id > 0 && selfExcluded.NumericValue == 1 && selfExcluded.DateValue > currentTime)
                throw CreateException(languageId, Constants.Errors.SelfExcluded);
            var systemExcluded = CacheManager.GetClientSettingByName(client.Id, ClientSettings.SystemExcluded);
            if (systemExcluded != null && systemExcluded.Id > 0 && systemExcluded.NumericValue == 1 && systemExcluded.DateValue > currentTime)
                throw CreateException(languageId, Constants.Errors.SystemExcluded);
            var cautionSuspension = CacheManager.GetClientSettingByName(client.Id, ClientSettings.CautionSuspension);
            if (cautionSuspension != null && cautionSuspension.Id > 0 && cautionSuspension.StringValue == "1")
                throw CreateException(languageId, Constants.Errors.CautionSuspension);
            var partner = CacheManager.GetPartnerById(client.PartnerId);
            bool younger = (client.BirthDate != DateTime.MinValue &&
                        (currentDate.Year - client.BirthDate.Year < partner.ClientMinAge ||
                        (currentDate.Year - client.BirthDate.Year == partner.ClientMinAge && currentDate.Month < client.BirthDate.Month) ||
                        (currentDate.Year - client.BirthDate.Year == partner.ClientMinAge && currentDate.Month == client.BirthDate.Month &&
                        currentDate.Day < client.BirthDate.Day)));
            if (younger)
                throw CreateException(languageId, Constants.Errors.Younger);

            if (CacheManager.GetConfigKey(client.PartnerId, Constants.PartnerKeys.AMLVerification) == "1")
            {
                var amlVerified = CacheManager.GetClientSettingByName(client.Id, ClientSettings.AMLVerified);
                if (amlVerified != null && amlVerified.Id > 0 && amlVerified.StringValue == "1")
                {
                    var amlStatus = CacheManager.GetClientSettingByName(client.Id, ClientSettings.AMLProhibited);
                    if (amlStatus != null && amlStatus.Id > 0 && amlStatus.StringValue == "2")
                        throw BaseBll.CreateException(languageId, Constants.Errors.AMLProhibited);
                }
            }
        }

        public static BllClient LoginClient(ClientLoginInput input, BllClient client, out string newToken, out RegionTree regionDetails, ILog log)
        {
            var sendEmail = false;
            CheckClientStatus(client, input.LanguageId);
            var passwordHash = CommonFunctions.ComputeClientPasswordHash(input.Password, client.Salt);
            if (client.PasswordHash != passwordHash)
            {
                CreateNewFailedSession(client.Id, input.LanguageId, input.Ip, input.CountryCode, null, input.DeviceType, input.Source, Constants.Errors.WrongPassword);
                var partnerSetting = CacheManager.GetConfigParameters(client.PartnerId, Constants.PartnerKeys.AllowedFaildLoginCount).FirstOrDefault(x => x.Key == "Client");
                if (!partnerSetting.Equals(default(KeyValuePair<string, string>)) && int.TryParse(partnerSetting.Value, out int allowedNumber))
                {
                    var count = CacheManager.UpdateClientFailedLoginCount(client.Id);
                    if (count > allowedNumber)
                    {
                        BlockClientForce(client.Id);
                        throw CreateException(input.LanguageId, Constants.Errors.ClientForceBlocked);
                    }
                }
                throw CreateException(input.LanguageId, Constants.Errors.WrongLoginParameters);
            }
            var lastIp = CacheManager.GetClientLastLoginIp(client.Id);
            if (lastIp != null && lastIp.Ip != input.Ip)
                sendEmail = true;

            if (CacheManager.GetConfigKey(client.PartnerId, Constants.PartnerKeys.AMLVerification) == "1")
            {
                var amlVerified = CacheManager.GetClientSettingByName(client.Id, ClientSettings.AMLVerified);
                if (amlVerified != null && amlVerified.Id > 0 && amlVerified.StringValue == "1")
                {
                    var amlStatus = CacheManager.GetClientSettingByName(client.Id, ClientSettings.AMLProhibited);
                    if (amlStatus != null && amlStatus.Id > 0 && amlStatus.StringValue == "2")
                        throw BaseBll.CreateException(input.LanguageId, Constants.Errors.AMLProhibited);
                }
            }

            using (var clientBl = new ClientBll(new SessionIdentity { LanguageId = input.LanguageId }, log))
            {
                using var regionBl = new RegionBll(clientBl);
                regionDetails = new RegionTree();
                var regionPath = regionBl.GetRegionPath(client.RegionId);
                var country = regionPath.FirstOrDefault(x => x.TypeId == (int)RegionTypes.Country);
                if (country != null)
                    regionDetails.CountryId = country.Id ?? client.RegionId;
                var city = regionPath.FirstOrDefault(x => x.TypeId == (int)RegionTypes.City);
                if (city != null)
                    regionDetails.CityId = city.Id ?? client.RegionId;
                var district = regionPath.FirstOrDefault(x => x.TypeId == (int)RegionTypes.District);
                if (district != null)
                    regionDetails.DistrictId = district.Id ?? client.RegionId;
                var region = regionPath.FirstOrDefault(x => x.TypeId == (int)RegionTypes.Region);
                if (region != null)
                    regionDetails.RegionId = region.Id ?? client.RegionId;
                var town = regionPath.FirstOrDefault(x => x.TypeId == (int)RegionTypes.Town);
                if (town != null)
                    regionDetails.TownId = town.Id ?? client.RegionId;

                var session = CreateNewPlatformSession(client.Id, input.LanguageId, input.Ip, input.CountryCode, null, input.DeviceType, input.Source);
                newToken = session.Token;
                CacheManager.UpdateClientLastLoginIp(client.Id, input.Ip);
                CacheManager.RemoveClientFailedLoginCount(client.Id);
                var currency = CacheManager.GetCurrencyById(client.CurrencyId);
                client.CurrencySymbol = currency.Symbol;

                var ad = clientBl.GetClientAlternativeDomain(client.Id);
                if (ad != null && ad.Any())
                {
                    client.AlternativeDomain = ad.FirstOrDefault(x => x.Name == Constants.SegmentSettings.AlternativeDomain)?.StringValue;
                    client.AlternativeDomainMessage = ad.FirstOrDefault(x => x.Name == Constants.SegmentSettings.DomainTextTranslationKey)?.StringValue;
                }
                using var bonusBl = new BonusService(clientBl);
                clientBl.AutoClaim(bonusBl, client.Id, (int)TriggerTypes.SignIn, string.Empty, null, out int awardedStatus, 0, null);
                clientBl.AddClientJobTrigger(client.Id, (int)JobTriggerTypes.ReconsiderSegments);
            }
            if (sendEmail)
            {
                try
                {
                    using var notificationBll = new NotificationBll(new SessionIdentity { LanguageId = input.LanguageId }, log);
                    notificationBll.SendNotificationMessage(new NotificationModel
                    {
                        PartnerId = client.PartnerId,
                        ClientId = client.Id,
                        MobileOrEmail = client.Email,
                        ClientInfoType = (int)ClientInfoTypes.NewIpLoginEmail,
                        VerificationCode = input.Ip
                    });
                }
                catch
                {
                }
            }
            return client;
        }

        public List<SegmentSetting> GetClientAlternativeDomain(int clientId)
        {
            return Db.ClientClassifications.Where(x => x.ClientId == clientId && x.Segment.State == (int)BaseStates.Active &&
                                                                   x.Segment.SegmentSettings.Any(y => y.Name == Constants.SegmentSettings.AlternativeDomain &&
                                                                                                      y.Name == Constants.SegmentSettings.DomainTextTranslationKey))
                                           .Select(x => x.Segment.SegmentSettings).FirstOrDefault()?.ToList();
        }

        public List<Bonu> AutoClaim(BonusService bonusBl, int clientId, int triggerType, string promoCode, decimal? sourceAmount, out int awardedStatus, int depositsCount, int? paymentSystemId)
        {
            Log.Info("AutoClaim_Start_" + clientId + "_" + triggerType + "_" + promoCode + "_" + sourceAmount + "_" + depositsCount + "_" + paymentSystemId);
            awardedStatus = 0;
            var currentTime = DateTime.UtcNow;

            if (!AutoclaimingTriggers.Contains(triggerType))
                return new List<Bonu>();

            var client = CacheManager.GetClientById(clientId);
            var clientSetting = CacheManager.GetClientSettingByName(clientId, ClientSettings.BlockedForBonuses);
            if (clientSetting != null && clientSetting.Id > 0 && clientSetting.StringValue == "1")
            {
                awardedStatus = 3;
                return new List<Bonu>();
            }
            var clientSegmentsIds = new List<int>();
            var clientClasifications = CacheManager.GetClientClasifications(client.Id);
            if (clientClasifications.Any())
                clientSegmentsIds = clientClasifications.Where(x => x.SegmentId.HasValue && x.ProductId == (int)Constants.PlatformProductId)
                                                        .Select(x => x.SegmentId.Value).ToList();


            var bonus = Db.Bonus.Include(x => x.TriggerGroups).ThenInclude(x => x.TriggerGroupSettings)
                                .Where(x => Constants.ClaimingBonusTypes.Contains(x.BonusType) &&
                                x.Status && x.PartnerId == client.PartnerId &&
                                            x.StartTime < currentTime && x.FinishTime > currentTime &&
                                          (!x.MaxGranted.HasValue || x.TotalGranted < x.MaxGranted) &&
                                          (!x.MaxReceiversCount.HasValue || x.TotalReceiversCount < x.MaxReceiversCount) &&
                                          (!x.BonusSegmentSettings.Any() ||
                                            x.BonusSegmentSettings.Any(y => (y.Type == (int)BonusSettingConditionTypes.InSet && clientSegmentsIds.Contains(y.SegmentId))) ||
                                           !x.BonusSegmentSettings.Any(y => (y.Type == (int)BonusSettingConditionTypes.OutOfSet && clientSegmentsIds.Contains(y.SegmentId)))) &&
                                          (!x.BonusCountrySettings.Any() ||
                                            x.BonusCountrySettings.Any(y => y.Type == (int)BonusSettingConditionTypes.InSet && y.CountryId == client.RegionId) ||
                                           !x.BonusCountrySettings.Any(y => y.Type == (int)BonusSettingConditionTypes.OutOfSet && y.CountryId == client.RegionId)
                                          ) &&
                                          (!x.BonusCurrencySettings.Any() ||
                                            x.BonusCurrencySettings.Any(y => y.Type == (int)BonusSettingConditionTypes.InSet && y.CurrencyId == client.CurrencyId) ||
                                           !x.BonusCurrencySettings.Any(y => y.Type == (int)BonusSettingConditionTypes.OutOfSet && y.CurrencyId == client.CurrencyId)) &&
                                          (!x.BonusLanguageSettings.Any() ||
                                            x.BonusLanguageSettings.Any(y => y.Type == (int)BonusSettingConditionTypes.InSet && y.LanguageId == client.LanguageId) ||
                                           !x.BonusLanguageSettings.Any(y => y.Type == (int)BonusSettingConditionTypes.OutOfSet && y.LanguageId == client.LanguageId)
                                          )).ToList();
            var claimedBonuses = new List<ClientBonusInfo>();
            foreach (var b in bonus)
            {
                var bon = CacheManager.GetBonusById(b.Id).TriggerGroups.FirstOrDefault(x => x.Priority == 0 && x.TriggerGroupSettings.Count == 1);
                if (bon != null)
                {
                    var setting = CacheManager.GetTriggerSettingById(bon.TriggerGroupSettings[0].SettingId);
                    if ((setting.Type == triggerType || (triggerType == (int)TriggerTypes.SignUp && setting.Type == (int)TriggerTypes.SignupCode && setting.BonusSettingCodes == promoCode)) &&
                        (triggerType != (int)TriggerTypes.PromotionalCode || setting.BonusSettingCodes == promoCode) &&
                        (triggerType != (int)TriggerTypes.AnyDeposit || setting.PaymentSystemIds == null || !setting.PaymentSystemIds.Any() || setting.PaymentSystemIds.Contains(paymentSystemId ?? 0)) &&
                        (triggerType != (int)TriggerTypes.NthDeposit || setting.PaymentSystemIds == null || !setting.PaymentSystemIds.Any() || setting.PaymentSystemIds.Contains(paymentSystemId ?? 0)) &&
                        (triggerType != (int)TriggerTypes.NthDeposit || depositsCount.ToString() == setting.Condition) &&
                        setting.StartTime <= currentTime && setting.FinishTime > currentTime)
                    {
                        var input = new ClientBonusItem
                        {
                            PartnerId = client.PartnerId,
                            BonusId = b.Id,
                            BonusType = b.BonusType,
                            ClientId = client.Id,
                            ClientUserName = client.UserName,
                            ClientCurrencyId = client.CurrencyId,
                            AccountTypeId = b.AccountTypeId ?? (int)AccountTypes.ClientUnusedBalance,
                            ReusingMaxCount = b.ReusingMaxCount,
                            IgnoreEligibility = b.IgnoreEligibility,
                            ValidForAwarding = b.ValidForAwarding == null ? (DateTime?)null : currentTime.AddHours(b.ValidForAwarding.Value),
                            ValidForSpending = b.ValidForSpending == null ? (DateTime?)null : currentTime.AddHours(b.ValidForSpending.Value)
                        };
                        var reuseNumber = bonusBl.GiveCompainToClient(input, out bool alreadyGiven).ReuseNumber;
                        claimedBonuses.Add(new ClientBonusInfo { BonusId = b.Id, ReuseNumber = reuseNumber });
                        awardedStatus = 1;
                    }
                }
            }

            CacheManager.RemoveClientNotAwardedCampaigns(client.Id);
            var clientBonuses = CacheManager.GetClientNotAwardedCampaigns(client.Id);

            FairClientBonusTrigger(new ClientTriggerInput
            {
                ClientId = client.Id,
                ClientBonuses = claimedBonuses,
                TriggerType = triggerType,
                SourceAmount = sourceAmount,
                PromoCode = promoCode,
                PaymentSystemId = paymentSystemId,
                DepositsCount = depositsCount
            }, out bool alreadyAdded);
            if (alreadyAdded)
                awardedStatus = 2;

            return bonus.Where(x => !claimedBonuses.Any(y => y.BonusId == x.Id)).ToList();
        }


        public static Client LoginTerminalClient(TerminalClientInput terminalClientInput) //to check
        {
            using var db = CreateEntities();
            var terminalClient = db.Clients.FirstOrDefault(x => x.UserName == terminalClientInput.TerminalId &&
            x.PartnerId == terminalClientInput.PartnerId && x.BetShopId == terminalClientInput.BetShopId);
            if (terminalClientInput == null)
                throw CreateException(terminalClientInput.LanguageId, Constants.Errors.ClientNotFound);
            if (terminalClient.PasswordHash != terminalClientInput.AuthToken)
                throw CreateException(terminalClientInput.LanguageId, Constants.Errors.WrongLoginParameters);

            var betshop = CacheManager.GetBetShopById(terminalClientInput.BetShopId);
            var ips = (string.IsNullOrEmpty(betshop.Ips) ? new List<string>() : JsonConvert.DeserializeObject<List<string>>(betshop.Ips));
            if (!ips.Any(x => x.IsIpInRange(terminalClientInput.Ip)))
                throw CreateException(terminalClientInput.LanguageId, Constants.Errors.NotAllowed);
            var session = CreateNewPlatformSession(terminalClient.Id, terminalClientInput.LanguageId, terminalClientInput.Ip, null, null, (int)DeviceTypes.Terminal, terminalClientInput.Source);
            terminalClient.Token = session.Token;
            var currency = CacheManager.GetCurrencyById(terminalClient.CurrencyId);
            terminalClient.CurrencySymbol = currency.Symbol;
            return terminalClient;
        }

        public static int GetClientStateByProduct(BllClient client, BllProduct product)
        {
            if (product.Id == Constants.PlatformProductId)
                return client.State;

            return GetClientState(client, product.Id, client.State);
        }

        public ProductInfo GetPartnerProductInfo(int? clientId, int productId, int partnerId)
        {
            var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(partnerId, productId);
            var volatility = string.Empty;
            if (partnerProductSetting != null && partnerProductSetting.Volatility.HasValue)
                volatility =  CacheManager.GetEnumerations(nameof(VolatilityTypes), LanguageId).FirstOrDefault(x => x.Value == partnerProductSetting.Volatility.Value)?.Text;
            var productInfo = new ProductInfo
            {
                Name = partnerProductSetting.NickName,
                RTP = partnerProductSetting.RTP ?? 0,
                Volatility = volatility
            };
            if (clientId.HasValue)
            {
                var client = CacheManager.GetClientById(clientId.Value);
                var bets = Db.Bets.Where(x => x.ProductId == productId && x.State!= (int)BetDocumentStates.Deleted);
                productInfo.Profit = Math.Round(bets.Select(x => x.BetAmount - x.WinAmount).DefaultIfEmpty(0).Sum(), 2);
                productInfo.Turnover = Math.Round(bets.Select(x => x.BetAmount).DefaultIfEmpty(0).Sum(), 2);

                var favoriteProduct = CacheManager.GetClientFavoriteProducts(clientId.Value).FirstOrDefault(x => x.ProductId == productId);
                productInfo.IsFavorite = favoriteProduct != null;
            }
            return productInfo;
        }

        public static BllClientSession GetClientPlatformSession(int clientId, long? sessionId = null)
        {
            var clientSession = CacheManager.GetClientPlatformSession(clientId, null);
            if (clientSession == null)
                throw CreateException(string.Empty, Constants.Errors.SessionNotFound);
            else if (sessionId != null && clientSession.Id != sessionId)
                throw CreateException(string.Empty, Constants.Errors.SessionNotFound);
            return clientSession;
        }

        public static ClientSession CreateNewProductSession(SessionIdentity session, out List<BllClientSession> updatedSessions, int? productId = null,
                                                     int? deviceType = null, string token = null, int? maxLenght = null)
        {
            ClientSession newSession = null;
            updatedSessions = new List<BllClientSession>();

            using (var db = CreateEntities())
            {
                var currentTime = DateTime.UtcNow;
                updatedSessions = db.ClientSessions.Where(x => x.ClientId == session.Id && x.ProductId == (productId ?? session.ProductId) &&
                                                   x.State == (int)SessionStates.Active).Select(x => new BllClientSession { ProductId = x.ProductId, Token = x.Token }).ToList();

                db.ClientSessions.Where(x => x.ClientId == session.Id && x.ProductId == (productId ?? session.ProductId) &&
                x.State == (int)SessionStates.Active).
                UpdateFromQuery(x => new ClientSession { State = (int)SessionStates.Inactive, EndTime = currentTime, LogoutType = (int)LogoutTypes.System });

                newSession = new ClientSession
                {
                    ClientId = session.Id,
                    LanguageId = session.LanguageId,
                    Ip = session.LoginIp,
                    Country = session.Country,
                    ProductId = productId ?? session.ProductId,
                    DeviceType = deviceType ?? (int)DeviceTypes.Desktop,
                    ParentId = session.SessionId == 0 ? (long?)null : session.SessionId,
                    State = (int)SessionStates.Active,
                    StartTime = currentTime,
                    LastUpdateTime = currentTime,
                    Token = token ?? GetToken(maxLenght),
                };
                db.ClientSessions.Add(newSession);
                db.SaveChanges();
            }
            foreach (var s in updatedSessions)
                CacheManager.RemoveClientProductSession(s.Token, s.ProductId);
            return newSession;
        }
        public static ClientSession CreateNewPlatformSession(int clientId, string languageId, string ip, string countryCode,
            string token, int deviceType, string source, string externalToken = null)
        {
            ClientSession session = null;
            using (var db = new IqSoftCorePlatformEntities())
            {
                var currentTime = DateTime.UtcNow;
                var activeSession = CacheManager.GetClientPlatformSession(clientId, null);
                if (activeSession != null)
                {
                    db.ClientSessions.Where(x => x.Id == activeSession.Id).UpdateFromQuery(x => new ClientSession
                    {
                        State = (int)SessionStates.Inactive,
                        LogoutType = (int)LogoutTypes.MultipleDevice,
                        EndTime = currentTime
                    });

                    db.Clients.Where(c => c.Id == clientId).UpdateFromQuery(x => new Client { LastSessionId = activeSession.Id });
                }
                session = new ClientSession
                {
                    ClientId = clientId,
                    ProductId = Constants.PlatformProductId,
                    LanguageId = languageId,
                    Ip = ip,
                    Country = countryCode,
                    State = (int)SessionStates.Active,
                    StartTime = currentTime,
                    LastUpdateTime = currentTime,
                    Token = string.IsNullOrEmpty(token) ? GetToken() : token,
                    ExternalToken = externalToken,
                    DeviceType = deviceType,
                    Source = source
                };
                db.ClientSessions.Add(session);
                db.SaveChanges();

                if (activeSession != null)
                {
                    CacheManager.RemoveClientPlatformSession(clientId);
                    CacheManager.RemoveClientProductSession(activeSession.Token, Constants.PlatformProductId);
                    CacheManager.RemoveClientFromCache(clientId);
                }

            }
            return session;
        }

        public static void CreateNewFailedSession(int clientId, string languageId, string ip, string countryCode,
            string token, int deviceType, string source, int id, string externalToken = null)
        {
            try
            {
                using var db = new IqSoftCorePlatformEntities();
                var currentTime = DateTime.UtcNow;
                var session = new ClientSession
                {
                    ClientId = clientId,
                    ProductId = Constants.PlatformProductId,
                    LanguageId = languageId,
                    Ip = ip,
                    Country = countryCode,
                    State = id,
                    StartTime = currentTime,
                    LastUpdateTime = currentTime,
                    Token = string.IsNullOrEmpty(token) ? GetToken() : token,
                    ExternalToken = externalToken,
                    DeviceType = deviceType,
                    Source = source
                };
                db.ClientSessions.Add(session);
                db.SaveChanges();
            }
            catch (Exception)
            {

            }
        }

        public List<ClientCategory> GetClientCategories()
        {
            return Db.ClientCategories.ToList();
        }
        public void DeleteClientCategory(int categoryId)
        {
            if (Db.Clients.Any(x => x.CategoryId == categoryId))
                throw CreateException(LanguageId, Constants.Errors.NotAllowed);
            Db.ClientCategories.Where(x => x.Id == categoryId).DeleteFromQuery();
        }

        public ClientCategory SaveClientCategory(ClientCategory clientCategory)
        {
            if (string.IsNullOrEmpty(clientCategory.NickName))
                throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);
            if (string.IsNullOrEmpty(clientCategory.Color))
                clientCategory.Color = "#ffffff";
            var dbClientCategory = Db.ClientCategories.FirstOrDefault(x => x.NickName == clientCategory.NickName);
            if (dbClientCategory != null)
            {
                dbClientCategory.Color = clientCategory.Color;
                clientCategory.Id = dbClientCategory.Id;
            }
            else
            {
                clientCategory.Translation = CreateTranslation(new fnTranslation
                {
                    ObjectTypeId = (int)ObjectTypes.ClientCategory,
                    Text = clientCategory.NickName,
                    LanguageId = Constants.DefaultLanguageId
                });
                Db.ClientCategories.Add(clientCategory);
            }
            Db.SaveChanges();
            return clientCategory;
        }

        private static int GetClientState(BllClient client, int productId, int currentState)
        {
            var classifications = CacheManager.GetClientClasifications(client.Id);
            var classification = classifications.FirstOrDefault(c => c.ProductId == productId);
            if (classification != null && classification.State.HasValue)
                return Math.Max(currentState, classification.State.Value);

            return currentState;
        }

        public PaymentLimit GetPaymentLimitExclusion(int clientId, bool checkPermission)
        {
            var client = CacheManager.GetClientById(clientId);
            if (client == null)
                throw CreateException(LanguageId, Constants.Errors.ClientNotFound);
            if (checkPermission)
            {
                var checkClientPermission = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewClient,
                    ObjectTypeId = (int)ObjectTypes.Client
                });

                var checkPartnerPermission = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewPartner,
                    ObjectTypeId = (int)ObjectTypes.Partner
                });

                var affiliateAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewAffiliateReferral,
                    ObjectTypeId = (int)ObjectTypes.AffiliateReferral
                });

                if ((!checkClientPermission.HaveAccessForAllObjects && checkClientPermission.AccessibleObjects.All(x => x != clientId)) ||
                    (!checkPartnerPermission.HaveAccessForAllObjects && checkPartnerPermission.AccessibleObjects.All(x => x != client.PartnerId)) ||
                    (!affiliateAccess.HaveAccessForAllObjects &&
                      affiliateAccess.AccessibleObjects.All(x => client.AffiliateReferralId.HasValue && x != client.AffiliateReferralId.Value)))
                    throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            }
            return Db.PaymentLimits.FirstOrDefault(x => x.ClientId == clientId && x.LimitTypeId == (int)LimitTypes.SelfExclusionLimit && x.RowState == (int)LimitRowStates.Active);
        }

        public ResponseBase SetPaymentLimitExclusion(PaymentLimit paymentLimit)
        {
            CheckPermission(Constants.Permissions.EditClient);
            var limit = Db.PaymentLimits.FirstOrDefault(x => x.ClientId == paymentLimit.ClientId && x.LimitTypeId == (int)LimitTypes.SelfExclusionLimit && x.RowState == (int)LimitRowStates.Active);
            if (limit != null)
            {
                limit.MaxDepositAmount = paymentLimit.MaxDepositAmount;
                limit.StartTime = paymentLimit.StartTime;
                limit.EndTime = paymentLimit.EndTime;
                limit.RowState = paymentLimit.RowState;
            }
            Db.SaveChanges();
            return new ResponseBase();
        }

        private static string GetToken(int? lenght = null)
        {
            if (lenght.HasValue)
                return CommonFunctions.GetRandomString(lenght.Value);
            return Guid.NewGuid().ToString().Replace("-", string.Empty);
        }

        public ClientPaymentInfo RegisterClientPaymentAccountDetails(ClientPaymentInfo info, string code, bool checkCode)
        {
            try
            {
                if (checkCode)
                    VerifySmsCode(info.ClientId, code);
                if (!string.IsNullOrEmpty(info.CardNumber))
                    info.CardNumber = info.CardNumber.Replace(" ", string.Empty);
                if (!Enum.IsDefined(typeof(ClientPaymentInfoTypes), info.Type) ||
                    (info.Type == (int)ClientPaymentInfoTypes.BankAccount && string.IsNullOrEmpty(info.BankAccountNumber)) ||
                    (info.Type == (int)ClientPaymentInfoTypes.Wallet && string.IsNullOrEmpty(info.WalletNumber)) ||
                    (info.Type == (int)ClientPaymentInfoTypes.CreditCard && string.IsNullOrEmpty(info.CardNumber)))
                    return null;
                var client = CacheManager.GetClientById(info.ClientId);

                if (info.PartnerPaymentSystemId.HasValue)
                {
                    var partnrPaymentSetting = Db.PartnerPaymentSettings.FirstOrDefault(x => x.Id == info.PartnerPaymentSystemId.Value);
                    if (partnrPaymentSetting == null)
                        throw CreateException(Identity.LanguageId, Constants.Errors.PartnerPaymentSettingNotFound);
                    var infon = Db.ClientPaymentInfoes.FirstOrDefault(x => x.ClientId == client.Id && x.PartnerPaymentSystemId == info.PartnerPaymentSystemId.Value &&
                           ((info.Type == (int)ClientPaymentInfoTypes.BankAccount && x.BankAccountNumber == info.BankAccountNumber) ||
                           (info.Type == (int)ClientPaymentInfoTypes.CreditCard && x.CardNumber == info.CardNumber) ||
                           (info.Type == (int)ClientPaymentInfoTypes.Wallet && x.WalletNumber == info.WalletNumber)));

                    if (infon != null)
                        return infon;

                    if (partnrPaymentSetting.AllowMultiplePaymentInfoes.HasValue && !partnrPaymentSetting.AllowMultiplePaymentInfoes.Value)
                    {
                        if (Db.ClientPaymentInfoes.Any(x => x.ClientId == client.Id && x.PartnerPaymentSystemId == info.PartnerPaymentSystemId.Value &&
                           ((info.Type == (int)ClientPaymentInfoTypes.BankAccount && x.BankAccountNumber != info.BankAccountNumber && x.State != (int)ClientPaymentInfoStates.Blocked) ||
                           (info.Type == (int)ClientPaymentInfoTypes.CreditCard && x.CardNumber != info.CardNumber && x.State != (int)ClientPaymentInfoStates.Blocked) ||
                           (info.Type == (int)ClientPaymentInfoTypes.Wallet && x.WalletNumber != info.WalletNumber && x.State != (int)ClientPaymentInfoStates.Blocked))))
                            throw CreateException(Identity.LanguageId, Constants.Errors.PaymentRequestNotAllowed);
                    }
                    if (partnrPaymentSetting.AllowMultipleClientsPerPaymentInfo.HasValue && !partnrPaymentSetting.AllowMultipleClientsPerPaymentInfo.Value)
                    {
                        if (Db.ClientPaymentInfoes.Any(x => x.ClientId != client.Id && x.PartnerPaymentSystemId == info.PartnerPaymentSystemId.Value &&
                          ((info.Type == (int)ClientPaymentInfoTypes.BankAccount && x.BankAccountNumber == info.BankAccountNumber && x.State != (int)ClientPaymentInfoStates.Blocked) ||
                          (info.Type == (int)ClientPaymentInfoTypes.CreditCard && x.CardNumber == info.CardNumber && x.State != (int)ClientPaymentInfoStates.Blocked) ||
                          (info.Type == (int)ClientPaymentInfoTypes.Wallet && x.WalletNumber == info.WalletNumber && x.State != (int)ClientPaymentInfoStates.Blocked))))
                            throw CreateException(Identity.LanguageId, Constants.Errors.PaymentRequestNotAllowed);
                    }
                }
                var currentTime = GetServerDate();
                info.CreationTime = currentTime;
                info.LastUpdateTime = currentTime;
                var partnerSetting = CacheManager.GetConfigKey(client.PartnerId, Constants.PartnerKeys.PaymentDetailsValidation);
                if (!string.IsNullOrEmpty(partnerSetting) && partnerSetting == "1")
                    info.State = (int)ClientPaymentInfoStates.Pending;
                else
                    info.State = (int)ClientPaymentInfoStates.Verified;
                Db.ClientPaymentInfoes.Add(info);
                Db.SaveChanges();
                if (info.State == (int)ClientPaymentInfoStates.Pending)
                {
                    try
                    {
                        using var notificationBll = new NotificationBll(new SessionIdentity { LanguageId = LanguageId }, Log);
                        notificationBll.SendNotificationMessage(new NotificationModel
                        {
                            PartnerId = client.PartnerId,
                            ClientId = client.Id,
                            MobileOrEmail = client.Email,
                            ClientInfoType = (int)ClientInfoTypes.PaymentInfoVerificationEmail,
                            LanguageId = LanguageId
                        });
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                    }
                }
                return info;
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                var m = string.Empty;
                foreach (var eve in e.EntityValidationErrors)
                {
                    m += string.Format("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:", eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        m += string.Format("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }

                return null;
            }
            catch (FaultException<Common.Models.CacheModels.BllFnErrorType> ex)
            {
                var exp = ex.Detail == null ? ex : new Exception(ex.Detail.Id + " " + ex.Detail.NickName);
                Log.Error(exp);
                return null;
            }
            catch (Exception e)
            {
                Log.Error(e);
                return null;
            }
        }

        public ClientPaymentInfo UpdateClientPaymentAccount(ClientPaymentInfo clientAccount)
        {
            var client = CacheManager.GetClientById(clientAccount.ClientId);
            var checkClientPermission = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewClient,
                ObjectTypeId = (int)ObjectTypes.Client
            });

            var checkPartnerPermission = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });

            var affiliateAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewAffiliateReferral,
                ObjectTypeId = (int)ObjectTypes.AffiliateReferral
            });
            CheckPermission(Constants.Permissions.EditClient);

            if ((!checkClientPermission.HaveAccessForAllObjects && checkClientPermission.AccessibleObjects.All(x => x != client.Id)) ||
                (!checkPartnerPermission.HaveAccessForAllObjects && checkPartnerPermission.AccessibleObjects.All(x => x != client.PartnerId)) ||
                (!affiliateAccess.HaveAccessForAllObjects &&
                  affiliateAccess.AccessibleObjects.All(x => client.AffiliateReferralId.HasValue && x != client.AffiliateReferralId.Value)))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            if (clientAccount.State.HasValue && !Enum.IsDefined(typeof(ClientPaymentInfoStates), clientAccount.State.Value))
                throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);

            var currTime = GetServerDate();
            clientAccount.LastUpdateTime = currTime;
            if (!Enum.IsDefined(typeof(ClientPaymentInfoTypes), clientAccount.Type) ||
               (clientAccount.Type == (int)ClientPaymentInfoTypes.CreditCard && string.IsNullOrEmpty(clientAccount.CardNumber)) ||
               (clientAccount.Type == (int)ClientPaymentInfoTypes.BankAccount && string.IsNullOrEmpty(clientAccount.BankAccountNumber)) ||
               (clientAccount.Type == (int)ClientPaymentInfoTypes.Wallet && string.IsNullOrEmpty(clientAccount.WalletNumber)))
                throw CreateException(Identity.LanguageId, Constants.Errors.WrongParameters);
            if (clientAccount.Id > 0)
            {
                var dbClientAccount = Db.ClientPaymentInfoes.Where(x => x.Id == clientAccount.Id).FirstOrDefault();
                if (dbClientAccount == null)
                    throw CreateException(LanguageId, Constants.Errors.AccountNotFound);
                clientAccount.CreationTime = dbClientAccount.CreationTime;
                Db.Entry(dbClientAccount).CurrentValues.SetValues(clientAccount);
            }
            else
            {
                clientAccount.CreationTime = currTime;
                Db.ClientPaymentInfoes.Add(clientAccount);
            }
            Db.SaveChanges();
            return clientAccount;
        }

        private void VerifySmsCode(int clientId, string code)
        {
            var client = CacheManager.GetClientById(clientId);
            var accountDetailsMobileVerification = CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.AccountDetailsMobileVerification);
            if (accountDetailsMobileVerification != null && accountDetailsMobileVerification.NumericValue != null && accountDetailsMobileVerification.NumericValue == 1)
            {
                var clientInfo = Db.ClientInfoes.FirstOrDefault(x => x.Data == code && x.ClientId == client.Id && x.Type == (int)ClientInfoTypes.AccountDetailsMobileKey);
                if (clientInfo == null)
                    throw CreateException(LanguageId, Constants.Errors.WrongVerificationKey);
                if (clientInfo.State == (int)ClientInfoStates.Expired)
                    throw CreateException(LanguageId, Constants.Errors.VerificationKeyExpired);
                clientInfo.State = (int)ClientInfoStates.Expired;
                Db.SaveChanges();
            }
        }

        public List<ClientPaymentInfo> GetClientPaymentAccountDetails(int clientId, int? paymentSystemId, List<int> accountTypes, bool checkPermission)
        {
            var client = Db.Clients.FirstOrDefault(x => x.Id == clientId);
            if (client == null)
                throw CreateException(LanguageId, Constants.Errors.ClientNotFound);
            if (checkPermission)
            {
                var checkClientPermission = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewClient,
                    ObjectTypeId = (int)ObjectTypes.Client
                });

                var checkPartnerPermission = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewPartner,
                    ObjectTypeId = (int)ObjectTypes.Partner
                });

                var affiliateAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewAffiliateReferral,
                    ObjectTypeId = (int)ObjectTypes.AffiliateReferral
                });
                if ((!checkClientPermission.HaveAccessForAllObjects && checkClientPermission.AccessibleObjects.All(x => x != clientId)) ||
                    (!checkPartnerPermission.HaveAccessForAllObjects && checkPartnerPermission.AccessibleObjects.All(x => x != client.PartnerId)) ||
                     (!affiliateAccess.HaveAccessForAllObjects &&
                     affiliateAccess.AccessibleObjects.All(x => client.AffiliateReferralId.HasValue && x != client.AffiliateReferralId.Value)))
                    throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            }
            var query = Db.ClientPaymentInfoes.Where(x => x.ClientId == clientId);
            if (accountTypes.Any())
                query = query.Where(x => accountTypes.Contains(x.Type));
            if (paymentSystemId != null)
                query = query.Where(x => x.PartnerPaymentSystem.PaymentSystemId == paymentSystemId && x.State == (int)ClientPaymentInfoStates.Verified);

            return query.ToList();
        }

        public List<AffiliateClientModel> GetClientsOfAffiliateManager(int managerId, int hours)
        {
            if (hours > 1000)
                hours = 1000;
            var fromDate = GetServerDate().AddHours(-hours);
            var fDate = fromDate.Year * 1000000 + fromDate.Month * 10000 + fromDate.Day * 100 + fromDate.Hour;
            var affiliateClients = Db.Clients.Include(x => x.AffiliateReferral).Where(x => x.AffiliateReferral.AffiliateId == managerId.ToString() &&
                                   x.AffiliateReferral.Type == (int)AffiliateReferralTypes.WebsiteInvitation)
                                   .Select(x => new AffiliateClientModel
                                   {
                                       Id = x.Id,
                                       FirstName = x.FirstName,
                                       LastName = x.LastName,
                                       Email = x.Email,
                                       Status = x.State,
                                       CreationTime = x.CreationTime
                                   }).ToList();

            var bonuses = Db.Documents.Where(x => x.ClientId == managerId && x.OperationTypeId == (int)OperationTypes.AffiliateBonus && x.Date >= fDate)
                                      .GroupBy(x => x.Creator)
                                      .Select(x => new { ClientId = x.Key, Amount = x.Sum(y => y.Amount) }).ToList();
            affiliateClients.ForEach(x =>
            {
                var bonusAmount = bonuses.FirstOrDefault(y => y.ClientId == x.Id)?.Amount;
                x.BonusAmount = bonusAmount ?? 0;
            });
            return affiliateClients;
        }

        public List<fnTicket> OpenTickets(Ticket ticket, TicketMessage message, List<int> clientIds, bool checkPermissions)
        {
            if (checkPermissions)
            {
                CheckPermissionToSaveObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.CreateClientMessage,
                    ObjectTypeId = (int)ObjectTypes.ClientMessage
                });
            }
            var currentTime = GetServerDate();
            ticket.CreationTime = currentTime;
            ticket.LastMessageTime = currentTime;
            ticket.LastMessageDate = (long)currentTime.Year * 100000000 + currentTime.Month * 1000000 + currentTime.Day * 10000 + currentTime.Hour * 100 + currentTime.Minute;
            message.CreationDate = ticket.LastMessageDate;
            message.CreationTime = currentTime;
            if (message.Type == (int)ClientMessageTypes.MessageFromClient)
            {
                ticket.ClientUnreadMessagesCount = 0;
                ticket.UserUnreadMessagesCount = 1;
            }
            else
            {
                ticket.ClientUnreadMessagesCount = 1;
                ticket.UserUnreadMessagesCount = 0;
            }
            var tickets = new List<long>();
            if (clientIds != null)
            {
                foreach (var clientId in clientIds)
                {
                    ticket.ClientId = clientId;
                    message.Ticket = ticket;
                    Db.Tickets.Add(ticket);
                    Db.TicketMessages.Add(message);
                    Db.SaveChanges();
                    tickets.Add(ticket.Id);
                    if (message.Type == (int)ClientMessageTypes.MessageFromUser)
                        CacheManager.UpdateClientUnreadTicketsCount(ticket.ClientId.Value, CacheManager.GetClientUnreadTicketsCount(clientId).Count + 1);
                }
            }
            return Db.fn_Ticket().Where(x => tickets.Contains(x.Id)).ToList();
        }

        public PagedModel<TicketModel> GetClientTickets(int clientId, int partnerId, int skipCount, int takeCount)
        {
            var query = Db.Tickets.Where(x => x.PartnerId == partnerId && x.ClientId == clientId &&
                                              x.Status != (int)MessageTicketState.Deleted);

            return new PagedModel<TicketModel>
            {
                Entities = query.OrderByDescending(x => x.LastMessageDate)
                                 .Skip(skipCount * takeCount).Take(takeCount)
                                 .Select(x => new TicketModel
                                 {
                                     Id = x.Id,
                                     ClientId = x.ClientId ?? 0,
                                     PartnerId = x.PartnerId,
                                     Status = x.Status,
                                     Subject = x.Subject,
                                     Type = x.Type,
                                     CreationTime = x.CreationTime,
                                     LastMessageTime = x.LastMessageTime,
                                     UnreadMessagesCount = x.ClientUnreadMessagesCount ?? 0,
                                     LastMessage = x.TicketMessages.OrderByDescending(y => y.Id).FirstOrDefault().Message
                                 }
                                 ).ToList(),
                Count = query.Count()
            };
        }

        public List<TicketMessage> GetMessagesByTicket(int ticketId, bool IsClient)
        {
            var messages = Db.TicketMessages.Include(x => x.User).Where(x => x.TicketId == ticketId)
                                                      .OrderBy(x => x.CreationDate).ToList();

            var ticket = Db.Tickets.FirstOrDefault(x => x.Id == ticketId);
            if (IsClient)
            {
                if (ticket.ClientUnreadMessagesCount > 0)
                {
                    ticket.ClientUnreadMessagesCount = 0;
                    var oldCount = CacheManager.GetClientUnreadTicketsCount(ticket.ClientId.Value);
                    CacheManager.UpdateClientUnreadTicketsCount(ticket.ClientId.Value, oldCount.Count - 1);
                }
            }
            else
                ticket.UserUnreadMessagesCount = 0;

            Db.SaveChanges();
            return messages;
        }

        public void ChangeTicketStatus(int ticketId, int? clientId, MessageTicketState ticketMessageState)
        {
            var query = Db.Tickets.Where(x => x.Id == ticketId);
            if (clientId.HasValue)
                query = query.Where(x => x.ClientId == clientId.Value);
            var ticket = query.FirstOrDefault();
            if (ticket == null)
                throw CreateException(LanguageId, Constants.Errors.TicketNotFound);
            ticket.Status = (int)ticketMessageState;
            Db.SaveChanges();
        }

        public PagedModel<fnTicket> GetTickets(FilterTicket filter, bool checkPermissions)
        {
            if (checkPermissions)
            {
                var clientAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewClient,
                    ObjectTypeId = (int)ObjectTypes.Client
                });
                var affiliateAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewAffiliateReferral,
                    ObjectTypeId = (int)ObjectTypes.AffiliateReferral
                });
                var clientMessageAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewClientMessage,
                    ObjectTypeId = (int)ObjectTypes.ClientMessage
                });
                var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewPartner,
                    ObjectTypeId = (int)ObjectTypes.Partner
                });
                filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnTicket>>
                {
                    new CheckPermissionOutput<fnTicket>
                    {
                        AccessibleObjects = clientMessageAccess.AccessibleObjects,
                        HaveAccessForAllObjects = clientMessageAccess.HaveAccessForAllObjects,
                        Filter = x => clientMessageAccess.AccessibleObjects.AsEnumerable().Contains(x.ClientId.Value)
                    },
                    new CheckPermissionOutput<fnTicket>
                    {
                        AccessibleObjects = partnerAccess.AccessibleObjects,
                        HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                        Filter = x => partnerAccess.AccessibleObjects.AsEnumerable().Contains(x.PartnerId)
                    },
                    new CheckPermissionOutput<fnTicket>
                    {
                        AccessibleObjects = clientAccess.AccessibleObjects,
                        HaveAccessForAllObjects = clientAccess.HaveAccessForAllObjects,
                        Filter = x => clientAccess.AccessibleObjects.AsEnumerable().Contains(x.ClientId.Value)
                    },
                    new CheckPermissionOutput<fnTicket>
                    {
                        AccessibleObjects = affiliateAccess.AccessibleObjects,
                        HaveAccessForAllObjects = affiliateAccess.HaveAccessForAllObjects,
                        Filter = x => x.AffiliateReferralId.HasValue && affiliateAccess.AccessibleObjects.AsEnumerable().Contains(x.AffiliateReferralId.Value)
                    }
                };
            }
            var tickets = new PagedModel<fnTicket>
            {
                Entities = filter.FilterObjects(Db.fn_Ticket(), ticket => ticket.OrderByDescending(y => y.Id)),
                Count = filter.SelectedObjectsCount(Db.fn_Ticket())
            };
            return tickets;
        }

        public PagedModel<fnClientMessage> GetClientMessages(FilterClientMessage filter, bool checkPermissions)
        {
            if (checkPermissions)
            {
                var clientAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewClient,
                    ObjectTypeId = (int)ObjectTypes.Client
                });

                var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewPartner,
                    ObjectTypeId = (int)ObjectTypes.Partner
                });

                var affiliateReferralAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewAffiliateReferral,
                    ObjectTypeId = (int)ObjectTypes.AffiliateReferral
                });

                GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewClientMessage,
                    ObjectTypeId = (int)ObjectTypes.ClientMessage
                });

                filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnClientMessage>>
                {
                    new CheckPermissionOutput<fnClientMessage>
                    {
                        AccessibleObjects = clientAccess.AccessibleObjects,
                        HaveAccessForAllObjects = clientAccess.HaveAccessForAllObjects,
                        Filter = x =>  clientAccess.AccessibleObjects.AsEnumerable().Contains(x.ClientId.Value)
                    },
                    new CheckPermissionOutput<fnClientMessage>
                    {
                        AccessibleObjects = partnerAccess.AccessibleObjects,
                        HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                        Filter = x => partnerAccess.AccessibleObjects.AsEnumerable().Contains(x.PartnerId)
                    },
                    new CheckPermissionOutput<fnClientMessage>
                    {
                        AccessibleObjects = affiliateReferralAccess.AccessibleObjects,
                        HaveAccessForAllObjects = affiliateReferralAccess.HaveAccessForAllObjects,
                        Filter = x => x.AffiliateReferralId.HasValue && affiliateReferralAccess.AccessibleObjects.AsEnumerable().Contains(x.AffiliateReferralId.Value)
                    }
                };
            }
            return new PagedModel<fnClientMessage>
            {
                Entities = filter.FilterObjects(Db.fn_ClientMessage(), clientMessage => clientMessage.OrderByDescending(y => y.Id)),
                Count = filter.SelectedObjectsCount(Db.fn_ClientMessage())
            };
        }

        public DAL.ClientInfo GetClientInfoByKey(string key, int clientInfoType, bool expire)
        {
            var clientInfo = Db.ClientInfoes.FirstOrDefault(x => x.Data == key && x.Type == clientInfoType);
            if (clientInfo == null)
                throw CreateException(LanguageId, Constants.Errors.WrongVerificationKey);
            if (clientInfo.State == (int)ClientInfoStates.Expired)
                throw CreateException(LanguageId, Constants.Errors.VerificationKeyExpired);
            if (expire)
                clientInfo.State = (int)ClientInfoStates.Expired;
            Db.SaveChanges();

            return clientInfo;
        }

        public PartnerBankInfo GetBankInfo(int clientId, int? bankInfoId, out DateTime viewDate)
        {
            var currentDate = GetServerDate();
            viewDate = currentDate;
            var clientBankInfo = Db.ClientBankInfoes.Include(x => x.BankInfo).FirstOrDefault(x => x.ClientId == clientId);

            if (clientBankInfo != null)
            {
                if (bankInfoId == null)
                {
                    if (clientBankInfo.LastViewDate < currentDate.AddHours(-24))
                        throw CreateException(LanguageId, Constants.Errors.BankIsUnavailable);
                    viewDate = clientBankInfo.LastViewDate;
                }
                else
                {
                    if (clientBankInfo.LastViewDate < currentDate.AddHours(-24))
                    {
                        clientBankInfo.BankInfoId = bankInfoId.Value;
                        clientBankInfo.LastViewDate = currentDate;
                        viewDate = clientBankInfo.LastViewDate;
                    }
                    else if (clientBankInfo.BankInfoId == bankInfoId)
                        viewDate = clientBankInfo.LastViewDate;
                    else
                        throw CreateException(LanguageId, Constants.Errors.BankIsUnavailable);

                    Db.SaveChanges();
                }
            }
            else
            {
                if (bankInfoId == null)
                    throw CreateException(LanguageId, Constants.Errors.BankIsUnavailable);

                clientBankInfo = new ClientBankInfo
                {
                    ClientId = clientId,
                    BankInfoId = bankInfoId.Value,
                    LastViewDate = currentDate
                };
                Db.ClientBankInfoes.Add(clientBankInfo);
                Db.SaveChanges();

                clientBankInfo.BankInfo = Db.PartnerBankInfoes.FirstOrDefault(x => x.Id == bankInfoId);
            }

            return clientBankInfo.BankInfo;
        }

        public void ResetClientBankInfo(int clientId)
        {
            var client = CacheManager.GetClientById(clientId);
            if (client == null)
                throw CreateException(LanguageId, Constants.Errors.ClientNotFound);
            var checkClientPermission = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewClient,
                ObjectTypeId = (int)ObjectTypes.Client
            });

            var checkPartnerPermission = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });
            var affiliateAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewAffiliateReferral,
                ObjectTypeId = (int)ObjectTypes.AffiliateReferral
            });

            if ((!checkClientPermission.HaveAccessForAllObjects && checkClientPermission.AccessibleObjects.All(x => x != clientId)) ||
                (!checkPartnerPermission.HaveAccessForAllObjects && checkPartnerPermission.AccessibleObjects.All(x => x != client.PartnerId)) ||
                 (!affiliateAccess.HaveAccessForAllObjects &&
                     affiliateAccess.AccessibleObjects.All(x => client.AffiliateReferralId.HasValue && x != client.AffiliateReferralId.Value)))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            CheckPermission(Constants.Permissions.EditClient);

            var currentDate = GetServerDate();
            Db.ClientBankInfoes.Where(x => x.ClientId == clientId).UpdateFromQuery(y => new ClientBankInfo { LastViewDate = currentDate.AddHours(-24) });
        }

        public void ChangeClientPaymentSettingState(int clientId, int partnerPaymentSettingId, int state)
        {
            var client = CacheManager.GetClientById(clientId);
            if (client == null)
                throw CreateException(LanguageId, Constants.Errors.ClientNotFound);
            var checkClientPermission = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewClient,
                ObjectTypeId = (int)ObjectTypes.Client
            });

            var checkPartnerPermission = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });

            var affiliateAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewAffiliateReferral,
                ObjectTypeId = (int)ObjectTypes.AffiliateReferral
            });

            if ((!checkClientPermission.HaveAccessForAllObjects && checkClientPermission.AccessibleObjects.All(x => x != clientId)) ||
                (!checkPartnerPermission.HaveAccessForAllObjects && checkPartnerPermission.AccessibleObjects.All(x => x != client.PartnerId)) ||
                (!affiliateAccess.HaveAccessForAllObjects &&
                 affiliateAccess.AccessibleObjects.All(x => client.AffiliateReferralId.HasValue && x != client.AffiliateReferralId.Value)))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            CheckPermission(Constants.Permissions.EditClient);
            if (!Enum.IsDefined(typeof(ClientPaymentStates), state))
                throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);

            var clientPaymentSetting = Db.ClientPaymentSettings.FirstOrDefault(x => x.ClientId == clientId &&
                                                                        x.PartnerPaymentSettingId == partnerPaymentSettingId);
            if (state == (int)ClientPaymentStates.Active && clientPaymentSetting != null)
            {
                Db.ClientPaymentSettings.Where(x => x.Id == clientPaymentSetting.Id).DeleteFromQuery();
                return;
            }
            if (clientPaymentSetting == null)
            {
                clientPaymentSetting = new ClientPaymentSetting
                {
                    ClientId = clientId,
                    PartnerPaymentSettingId = partnerPaymentSettingId,
                    State = state
                };
                Db.ClientPaymentSettings.Add(clientPaymentSetting);
            }
            else
                clientPaymentSetting.State = state;
            Db.SaveChanges();
        }

        public ClientPaymentSetting BlockClientPaymentSettingState(int clientId, int partnerPaymentSettingId) // should be deleted
        {
            var client = CacheManager.GetClientById(clientId);
            if (client == null)
                throw CreateException(LanguageId, Constants.Errors.ClientNotFound);
            var checkClientPermission = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewClient,
                ObjectTypeId = (int)ObjectTypes.Client
            });

            var checkPartnerPermission = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });

            var affiliateAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewAffiliateReferral,
                ObjectTypeId = (int)ObjectTypes.AffiliateReferral
            });

            if ((!checkClientPermission.HaveAccessForAllObjects && checkClientPermission.AccessibleObjects.All(x => x != clientId)) ||
                (!checkPartnerPermission.HaveAccessForAllObjects && checkPartnerPermission.AccessibleObjects.All(x => x != client.PartnerId)) ||
                (!affiliateAccess.HaveAccessForAllObjects &&
                 affiliateAccess.AccessibleObjects.All(x => client.AffiliateReferralId.HasValue && x != client.AffiliateReferralId.Value)))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            CheckPermission(Constants.Permissions.EditClient);

            var response = Db.ClientPaymentSettings.FirstOrDefault(x => x.ClientId == clientId && x.State == (int)ClientPaymentStates.Blocked &&
                                                                        x.PartnerPaymentSettingId == partnerPaymentSettingId);
            if (response == null)
            {
                response = new ClientPaymentSetting
                {
                    ClientId = clientId,
                    PartnerPaymentSettingId = partnerPaymentSettingId,
                    State = (int)ClientPaymentStates.Blocked
                };
                Db.ClientPaymentSettings.Add(response);
                Db.SaveChanges();
            }
            response.PartnerPaymentSetting = Db.PartnerPaymentSettings.Include(x => x.PaymentSystem).FirstOrDefault(x => x.Id == partnerPaymentSettingId);
            return response;
        }

        public void AddClientJobTrigger(int clientId, int type, decimal? amount = null)
        {
            var jobTrigger = new JobTrigger
            {
                ClientId = clientId,
                Type = type,
                Amount = amount
            };
            if (amount == null)
                Db.JobTriggers.AddIfNotExists(jobTrigger, x => x.ClientId == clientId && x.Type == type);
            else
                Db.JobTriggers.Add(jobTrigger);

            Db.SaveChanges();
        }

        public void ActivateClientPaymentSetting(int id)
        {
            CheckPermission(Constants.Permissions.EditClient);
            Db.ClientPaymentSettings.Where(x => x.Id == id).DeleteFromQuery();
            Db.SaveChanges();
        }

        public List<ClientPaymentSetting> GetClientPaymentSettings(int clientId, int? state, bool checkPermission)
        {
            var client = CacheManager.GetClientById(clientId);
            if (client == null)
                throw CreateException(LanguageId, Constants.Errors.ClientNotFound);
            if (checkPermission)
            {
                var clientAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewClient,
                    ObjectTypeId = (int)ObjectTypes.Client
                });

                var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewPartner,
                    ObjectTypeId = (int)ObjectTypes.Partner
                });

                if ((!clientAccess.HaveAccessForAllObjects && clientAccess.AccessibleObjects.All(x => x != clientId)) ||
                    (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleObjects.All(x => x != client.PartnerId)))
                    throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            }
            var partnerPaymentSettings = Db.PartnerPaymentSettings.Where(x => x.PartnerId == client.PartnerId && x.CurrencyId == client.CurrencyId).ToList();
            var clientPaymentSettings = Db.ClientPaymentSettings.Include(x => x.PartnerPaymentSetting.PaymentSystem).Where(x => x.ClientId == clientId).ToList();

            partnerPaymentSettings.ForEach(x =>
            {
                if (!clientPaymentSettings.Any(y => y.PartnerPaymentSettingId == x.Id))
                    clientPaymentSettings.Add(new ClientPaymentSetting
                    {
                        ClientId = clientId,
                        PartnerPaymentSetting = x,
                        PartnerPaymentSettingId = x.Id,
                        State = (int)ClientPaymentStates.Active
                    });
            });
            if (state.HasValue)
                return clientPaymentSettings.Where(x => x.State == state).ToList();
            return clientPaymentSettings;
        }

        public ClientFavoriteProduct ChangeClientFavoriteProduct(int clientId, int productId, bool isFavorite)
        {
            var product = Db.Products.FirstOrDefault(x => x.Id == productId);
            if (product == null)
                throw CreateException(LanguageId, Constants.Errors.ProductNotFound);
            var client = CacheManager.GetClientById(clientId);
            if (client == null)
                throw CreateException(LanguageId, Constants.Errors.ClientNotFound);
            var clientFavoriteProduct = Db.ClientFavoriteProducts.FirstOrDefault(x => x.ProductId == productId && x.ClientId == clientId);
            if (clientFavoriteProduct == null && isFavorite)
            {
                clientFavoriteProduct = new ClientFavoriteProduct
                {
                    ClientId = clientId,
                    ProductId = productId
                };
                Db.ClientFavoriteProducts.Add(clientFavoriteProduct);
            }
            else if (clientFavoriteProduct != null && !isFavorite)
            {
                Db.ClientFavoriteProducts.Where(x => x.ProductId == productId && x.ClientId == clientId).DeleteFromQuery();
            }
            Db.SaveChanges();
            CacheManager.RemoveClientFavoriteProducts(clientId);
            return clientFavoriteProduct;
        }

        public List<ClientFavoriteProduct> GetClientFavoriteProducts(int clientId)
        {
            return Db.ClientFavoriteProducts.Include(x => x.Product).Where(x => x.ClientId == clientId).ToList();
        }

        private string CreateClientInfo(int clientId, int partnerId, int type)
        {
            var currentTime = DateTime.UtcNow;
            var clientInfo = new DAL.ClientInfo
            {
                PartnerId = partnerId,
                ClientId = clientId,
                CreationTime = currentTime,
                Data = CommonFunctions.GetRandomString(20),
                Type = type,
                State = (int)ClientInfoStates.Active
            };
            Db.ClientInfoes.Add(clientInfo);
            Db.SaveChanges();
            return clientInfo.Data;
        }

        public void FairSegmentTriggers()
        {
            using var transactionScope = CommonFunctions.CreateTransactionScope(20);
            var dbJobTriggers = Db.JobTriggers.Where(x => x.Type == (int)JobTriggerTypes.FairSegmentTriggers).ToList();
            dbJobTriggers.ForEach(x =>
            {
                FairClientBonusTrigger(new ClientTriggerInput
                {
                    ClientId = x.ClientId,
                    ClientBonuses = CacheManager.GetClientNotAwardedCampaigns(x.ClientId),
                    TriggerType = (int)TriggerTypes.SegmentChange,
                    SegmentId = x.SegmentId
                }, out bool alreadyAdded);
                Db.JobTriggers.RemoveRange(dbJobTriggers);
                Db.SaveChanges();
                transactionScope.Complete();
            });
        }

        public List<ClientBonusTrigger> FairClientBonusTrigger(ClientTriggerInput clientTriggerInput, out bool alreadyAdded)
        {
            var result = new List<ClientBonusTrigger>();
            alreadyAdded = false;
            var currentTime = GetServerDate();
            var client = CacheManager.GetClientById(clientTriggerInput.ClientId);
            var partner = CacheManager.GetPartnerById(client.PartnerId);
            var betTriggers = new List<int> { (int)TriggerTypes.BetPlacement, (int)TriggerTypes.BetSettlement,
                        (int)TriggerTypes.BetPlacementAndSettlement, (int)TriggerTypes.CrossProductBetPlacement };

            foreach (var cb in clientTriggerInput.ClientBonuses)
            {
                var bonus = CacheManager.GetBonusById(cb.BonusId);
                foreach (var g in bonus.TriggerGroups)
                {
                    foreach (var tgs in g.TriggerGroupSettings)
                    {
                        var trigger = CacheManager.GetTriggerSettingById(tgs.SettingId);
                        if (trigger.StartTime <= currentTime && trigger.FinishTime > currentTime && (!trigger.DayOfWeek.HasValue || trigger.DayOfWeek == (int)DateTime.UtcNow.DayOfWeek)
                            && ((trigger.Type == clientTriggerInput.TriggerType
                                        && (clientTriggerInput.TriggerType != (int)TriggerTypes.PromotionalCode || trigger.BonusSettingCodes == clientTriggerInput.PromoCode)
                                        && (clientTriggerInput.TriggerType != (int)TriggerTypes.NthDeposit || trigger.Condition == clientTriggerInput.DepositsCount.ToString()
                                        && (clientTriggerInput.TriggerType != (int)TriggerTypes.SegmentChange || trigger.SegmentId == clientTriggerInput.SegmentId)))
                                || (clientTriggerInput.TriggerType == (int)TriggerTypes.BetSettlement && trigger.Type == (int)TriggerTypes.BetPlacementAndSettlement)
                                || (clientTriggerInput.TriggerType == (int)TriggerTypes.NthDeposit && trigger.Type == (int)TriggerTypes.AnyDeposit)
                                || (clientTriggerInput.TriggerType == (int)TriggerTypes.SignUp && trigger.Type == (int)TriggerTypes.SignupCode && trigger.BonusSettingCodes == clientTriggerInput.PromoCode))
                           && (!clientTriggerInput.PaymentSystemId.HasValue || trigger.PaymentSystemIds == null ||
                           !trigger.PaymentSystemIds.Any() || trigger.PaymentSystemIds.Contains(clientTriggerInput.PaymentSystemId.Value)))
                        {
                            var fTrigger = Db.ClientBonusTriggers.FirstOrDefault(x => x.ClientId == clientTriggerInput.ClientId &&
                                x.TriggerId == trigger.Id && x.BonusId == cb.BonusId && x.ReuseNumber == cb.ReuseNumber);
                            var triggerMinAmount = trigger.MinAmount.HasValue ? ConvertCurrency(partner.CurrencyId, client.CurrencyId, trigger.MinAmount.Value) : (decimal?)null;
                            var triggerMaxAmount = trigger.MaxAmount.HasValue ? ConvertCurrency(partner.CurrencyId, client.CurrencyId, trigger.MaxAmount.Value) : (decimal?)null;
                            var triggerUpToAmount = trigger.UpToAmount.HasValue ? ConvertCurrency(partner.CurrencyId, client.CurrencyId, trigger.UpToAmount.Value) : (decimal?)null;

                            if (fTrigger == null || (fTrigger.BetCount != null && fTrigger.BetCount < trigger.MinBetCount) ||
                                (fTrigger.WageringAmount != null && fTrigger.WageringAmount < triggerMinAmount))
                            {
                                bool accept = true;
                                decimal? percent = null;
                                if ((trigger.Type == (int)TriggerTypes.BetPlacement || trigger.Type == (int)TriggerTypes.BetSettlement ||
                                    trigger.Type == (int)TriggerTypes.BetPlacementAndSettlement) && !string.IsNullOrEmpty(trigger.Condition))
                                {
                                    var conditions = JsonConvert.DeserializeObject<Common.Models.Bonus.BonusCondition>(trigger.Condition);
                                    var info = string.IsNullOrEmpty(clientTriggerInput.TicketInfo) ? new BonusTicketInfo() : JsonConvert.DeserializeObject<BonusTicketInfo>(clientTriggerInput.TicketInfo);
                                    info.BetAmount = clientTriggerInput.SourceAmount != null ? clientTriggerInput.SourceAmount.Value : (triggerMinAmount ?? 0);
                                    if (!string.IsNullOrEmpty(clientTriggerInput.WinInfo))
                                    {
                                        var win = JsonConvert.DeserializeObject<BonusTicketInfo>(clientTriggerInput.WinInfo);
                                        info.NumberOfWonSelections = win.NumberOfWonSelections;
                                        info.NumberOfLostSelections = win.NumberOfLostSelections;
                                        info.BetStatus = win.BetStatus;
                                        if (trigger.BonusSettingCodes == "2")
                                            clientTriggerInput.SourceAmount = win.WinAmount - info.BetAmount;
                                    }
                                    accept = IsCorrectConditionGroup(conditions, info);
                                    Log.Info("FairClientBonusTrigger_" + clientTriggerInput.ClientId + trigger.Condition + "_" + JsonConvert.SerializeObject(info) + "_" +
                                        JsonConvert.SerializeObject(trigger) + "_" + accept);
                                }
                                else if (trigger.Type == (int)TriggerTypes.CrossProductBetPlacement && clientTriggerInput.ProductId != null)
                                {
                                    while (true)
                                    {
                                        var pr = trigger.ProductSettings.FirstOrDefault(x => x.ProductId == clientTriggerInput.ProductId.Value);
                                        if (pr != null)
                                        {
                                            accept = (pr.Percent > 0);
                                            percent = pr.Percent;
                                            break;
                                        }
                                        else
                                        {
                                            var product = CacheManager.GetProductById(clientTriggerInput.ProductId.Value);
                                            if (!product.ParentId.HasValue)
                                                break;
                                            clientTriggerInput.ProductId = product.ParentId.Value;
                                        }
                                    }
                                }

                                if (accept)
                                {
                                    var amount = (clientTriggerInput.SourceAmount != null && trigger.Percent > 0) ? clientTriggerInput.SourceAmount.Value * trigger.Percent / 100 : triggerMinAmount;
                                    bool isBetTrigger = betTriggers.Contains(trigger.Type);
                                    if (triggerUpToAmount != null && amount > triggerUpToAmount)
                                        amount = triggerUpToAmount;

                                    if (isBetTrigger || ((triggerMinAmount == null || amount >= triggerMinAmount.Value) && (triggerMaxAmount == null || amount <= triggerMaxAmount.Value)))
                                    {
                                        if (fTrigger == null)
                                        {
                                            fTrigger = new ClientBonusTrigger
                                            {
                                                ClientId = clientTriggerInput.ClientId,
                                                TriggerId = trigger.Id,
                                                BonusId = cb.BonusId,
                                                SourceAmount = amount,
                                                CreationTime = DateTime.UtcNow,
                                                BetCount = isBetTrigger ? 1 : (int?)null,
                                                WageringAmount = isBetTrigger && triggerMinAmount != null ? (clientTriggerInput.SourceAmount * (percent ?? 100)) / 100 : null,
                                                ReuseNumber = cb.ReuseNumber
                                            };
                                            if (isBetTrigger && triggerMaxAmount.HasValue && triggerMaxAmount.Value < amount)
                                                fTrigger.SourceAmount = triggerMaxAmount;
                                            Db.ClientBonusTriggers.Add(fTrigger);
                                        }
                                        else
                                        {
                                            if (fTrigger.BetCount != null)
                                                fTrigger.BetCount = fTrigger.BetCount.Value + 1;
                                            if (fTrigger.WageringAmount != null)
                                                fTrigger.WageringAmount = fTrigger.WageringAmount.Value + (clientTriggerInput.SourceAmount * (percent ?? 100)) / 100;
                                            if (isBetTrigger)
                                            {
                                                fTrigger.SourceAmount += amount;
                                                if (triggerMaxAmount != null && fTrigger.SourceAmount >= triggerMaxAmount)
                                                    fTrigger.SourceAmount = triggerMaxAmount;
                                            }
                                        }
                                        Db.SaveChanges();
                                        result.Add(fTrigger);
                                    }
                                }
                            }
                            else
                                alreadyAdded = true;
                        }
                    }
                }
            }

            return result;
        }

        public bool CheckWagerAvailability(int clientId, int bonusId, string ticketInfo)
        {
            //var result = new List<ClientBonusTrigger>();
            //var currentTime = GetServerDate();
            //var client = CacheManager.GetClientById(clientId);
            var bonus = CacheManager.GetBonusById(bonusId);
            if (string.IsNullOrEmpty(bonus.Condition))
                return true;
            var conditions = JsonConvert.DeserializeObject<Common.Models.Bonus.BonusCondition>(bonus.Condition);
            var info = string.IsNullOrEmpty(ticketInfo) ? new BonusTicketInfo() : JsonConvert.DeserializeObject<BonusTicketInfo>(ticketInfo);
            return IsCorrectConditionGroup(conditions, info);
        }

        private void UpdateClientFreeBetBonus(int bonusId, int clientId, decimal betAmount)
        {
            var bonus = Db.ClientBonus.Include(x => x.Bonus).FirstOrDefault(x => x.BonusId == bonusId && x.ClientId == clientId && x.Status == (int)BonusStatuses.Active);
            if (bonus == null)
                throw CreateException(LanguageId, Constants.Errors.BonusNotFound);
            if (bonus.TurnoverAmountLeft < betAmount)
                throw CreateException(LanguageId, Constants.Errors.WrongOperationAmount);
            bonus.TurnoverAmountLeft -= betAmount;
            if (bonus.TurnoverAmountLeft <= 0 || bonus.Bonus.AllowSplit == null || !bonus.Bonus.AllowSplit.Value)
            {
                bonus.Status = (int)BonusStatuses.Closed;
                bonus.CalculationTime = DateTime.UtcNow;
            }
            Db.SaveChanges();
        }

        public bool IsCorrectConditionGroup(Common.Models.Bonus.BonusCondition group, BonusTicketInfo info)
        {
            if (group.GroupingType == (int)GroupingTypes.All)
            {
                if (group.Conditions != null)
                {
                    foreach (var c in group.Conditions)
                    {
                        if (!IsCorrectCondition(c, info))
                            return false;
                    }
                }
                if (group.Groups != null)
                {
                    foreach (var g in group.Groups)
                    {
                        if (!IsCorrectConditionGroup(JsonConvert.DeserializeObject<Common.Models.Bonus.BonusCondition>(JsonConvert.SerializeObject(g)), info))
                            return false;
                    }
                }
                return true;
            }
            else
            {
                if (group.Conditions != null)
                {
                    foreach (var c in group.Conditions)
                    {
                        if (IsCorrectCondition(c, info))
                            return true;
                    }
                }
                if (group.Groups != null)
                {
                    foreach (var g in group.Groups)
                    {
                        if (IsCorrectConditionGroup(JsonConvert.DeserializeObject<Common.Models.Bonus.BonusCondition>(JsonConvert.SerializeObject(g)), info))
                            return true;
                    }
                }
                return false;
            }
        }

        public static bool IsCorrectCondition(BonusConditionItem item, BonusTicketInfo info)
        {
            switch (item.ConditionType)
            {
                case (int)BonusConditionTypes.Sport:
                    return IsCorrectConditionItem(item, info.BetSelections, "SportId");
                case (int)BonusConditionTypes.Region:
                    return IsCorrectConditionItem(item, info.BetSelections, "RegionId");
                case (int)BonusConditionTypes.Competition:
                    return IsCorrectConditionItem(item, info.BetSelections, "CompetitionId");
                case (int)BonusConditionTypes.Match:
                    return IsCorrectConditionItem(item, info.BetSelections, "MatchId");
                case (int)BonusConditionTypes.Market:
                    return IsCorrectConditionItem(item, info.BetSelections, "MarketId");
                case (int)BonusConditionTypes.Selection:
                    return IsCorrectConditionItem(item, info.BetSelections, "SelectionId");
                case (int)BonusConditionTypes.MarketType:
                    return IsCorrectConditionItem(item, info.BetSelections, "MarketTypeId");
                case (int)BonusConditionTypes.SelectionType:
                    return IsCorrectConditionItem(item, info.BetSelections, "SelectionTypeId");
                case (int)BonusConditionTypes.MatchStatus:
                    return IsCorrectConditionItem(item, info.BetSelections, "MatchStatus");
                case (int)BonusConditionTypes.PricePerSelection:
                    return IsCorrectConditionItem(item, info.BetSelections, "Price");
                case (int)BonusConditionTypes.Price:
                    return IsCorrectConditionItem(item, info.Price);
                case (int)BonusConditionTypes.BetType:
                    return IsCorrectConditionItem(item, info.BetType);
                case (int)BonusConditionTypes.NumberOfSelections:
                    return IsCorrectConditionItem(item, info.SelectionsCount);
                case (int)BonusConditionTypes.NumberOfWonSelections://necessery for triggers only
                    return IsCorrectConditionItem(item, info.NumberOfWonSelections);
                case (int)BonusConditionTypes.NumberOfLostSelections://necessery for triggers only
                    return IsCorrectConditionItem(item, info.NumberOfLostSelections);
                case (int)BonusConditionTypes.Stake://necessery for triggers only
                    return IsCorrectConditionItem(item, info.BetAmount);
                case (int)BonusConditionTypes.BetStatus://necessery for triggers only
                    return IsCorrectConditionItem(item, info.BetStatus);
            }
            return true;
        }

        public static bool IsCorrectConditionItem(BonusConditionItem item, List<BonusTicketSelection> betSelections, string prpertyName)
        {
            if (item.OperationTypeId == (int)FilterOperations.IsEqualTo)
            {
                if (Decimal.TryParse(item.StringValue, out decimal objectId))
                    return betSelections.All(x => Convert.ToDecimal(x.GetType().GetProperty(prpertyName).GetValue(x, null)) == objectId);
                return false;
            }
            else if (item.OperationTypeId == (int)FilterOperations.IsGreaterThenOrEqualTo)
            {
                if (Decimal.TryParse(item.StringValue, out decimal objectId))
                    return betSelections.All(x => Convert.ToDecimal(x.GetType().GetProperty(prpertyName).GetValue(x, null)) >= objectId);
                return false;
            }
            else if (item.OperationTypeId == (int)FilterOperations.IsGreaterThen)
            {
                if (Decimal.TryParse(item.StringValue, out decimal objectId))
                    return betSelections.All(x => Convert.ToDecimal(x.GetType().GetProperty(prpertyName).GetValue(x, null)) > objectId);
                return false;
            }
            else if (item.OperationTypeId == (int)FilterOperations.IsLessThenOrEqualTo)
            {
                if (Decimal.TryParse(item.StringValue, out decimal objectId))
                    return betSelections.All(x => Convert.ToDecimal(x.GetType().GetProperty(prpertyName).GetValue(x, null)) <= objectId);
                return false;
            }
            else if (item.OperationTypeId == (int)FilterOperations.IsLessThen)
            {
                if (Decimal.TryParse(item.StringValue, out decimal objectId))
                    return betSelections.All(x => Convert.ToDecimal(x.GetType().GetProperty(prpertyName).GetValue(x, null)) < objectId);
                return false;
            }
            else if (item.OperationTypeId == (int)FilterOperations.IsNotEqualTo)
            {
                if (Decimal.TryParse(item.StringValue, out decimal objectId))
                    return betSelections.All(x => Convert.ToDecimal(x.GetType().GetProperty(prpertyName).GetValue(x, null)) != objectId);
                return false;
            }
            else if (item.OperationTypeId == (int)FilterOperations.InSet)
            {
                if (string.IsNullOrEmpty(item.StringValue))
                    return false;
                var objects = item.StringValue.Split(',').ToList();
                var objectIds = new List<decimal>();
                foreach (var s in objects)
                {
                    if (Decimal.TryParse(s, out decimal objectId))
                        objectIds.Add(objectId);
                    else
                        return false;
                }
                return betSelections.All(x => objectIds.Contains(Convert.ToDecimal(x.GetType().GetProperty(prpertyName).GetValue(x, null))));
            }
            else if (item.OperationTypeId == (int)FilterOperations.OutOfSet)
            {
                if (string.IsNullOrEmpty(item.StringValue))
                    return false;
                var objects = item.StringValue.Split(',').ToList();
                var objectIds = new List<decimal>();
                foreach (var s in objects)
                {
                    if (Decimal.TryParse(s, out decimal objectId))
                        objectIds.Add(objectId);
                    else
                        return false;
                }
                return betSelections.All(x => !objectIds.Contains(Convert.ToDecimal(x.GetType().GetProperty(prpertyName).GetValue(x, null))));
            }
            else if (item.OperationTypeId == (int)FilterOperations.AtLeastOneInSet)
            {
                if (string.IsNullOrEmpty(item.StringValue))
                    return false;
                var objects = item.StringValue.Split(',').ToList();
                var objectIds = new List<decimal>();
                foreach (var s in objects)
                {
                    if (Decimal.TryParse(s, out decimal objectId))
                        objectIds.Add(objectId);
                    else
                        return false;
                }
                return betSelections.Any(x => objectIds.Contains(Convert.ToDecimal(x.GetType().GetProperty(prpertyName).GetValue(x, null))));
            }
            return false;
        }

        public static bool IsCorrectConditionItem(BonusConditionItem item, decimal value)
        {
            if (item.OperationTypeId == (int)FilterOperations.IsEqualTo)
            {
                if (Decimal.TryParse(item.StringValue, out decimal objectId))
                    return value == objectId;
                return false;
            }
            else if (item.OperationTypeId == (int)FilterOperations.IsGreaterThenOrEqualTo)
            {
                if (Decimal.TryParse(item.StringValue, out decimal objectId))
                    return value >= objectId;
                return false;
            }
            else if (item.OperationTypeId == (int)FilterOperations.IsGreaterThen)
            {
                if (Decimal.TryParse(item.StringValue, out decimal objectId))
                    return value > objectId;
                return false;
            }
            else if (item.OperationTypeId == (int)FilterOperations.IsLessThenOrEqualTo)
            {
                if (Decimal.TryParse(item.StringValue, out decimal objectId))
                    return value <= objectId;
                return false;
            }
            else if (item.OperationTypeId == (int)FilterOperations.IsLessThen)
            {
                if (Decimal.TryParse(item.StringValue, out decimal objectId))
                    return value < objectId;
                return false;
            }
            else if (item.OperationTypeId == (int)FilterOperations.IsNotEqualTo)
            {
                if (Decimal.TryParse(item.StringValue, out decimal objectId))
                    return value != objectId;
                return false;
            }
            else if (item.OperationTypeId == (int)FilterOperations.InSet)
            {
                if (string.IsNullOrEmpty(item.StringValue))
                    return false;
                var objects = item.StringValue.Split(',').ToList();
                var objectIds = new List<decimal>();
                foreach (var s in objects)
                {
                    if (Decimal.TryParse(s, out decimal objectId))
                        objectIds.Add(objectId);
                    else
                        return false;
                }
                return objectIds.Contains(value);
            }
            else if (item.OperationTypeId == (int)FilterOperations.OutOfSet)
            {
                if (string.IsNullOrEmpty(item.StringValue))
                    return false;
                var objects = item.StringValue.Split(',').ToList();
                var objectIds = new List<decimal>();
                foreach (var s in objects)
                {
                    if (Decimal.TryParse(s, out decimal objectId))
                        objectIds.Add(objectId);
                    else
                        return false;
                }
                return !objectIds.Contains(value);
            }
            return false;
        }

        public ClientBonusTrigger ChangeClientBonusTriggerManually(int clientId, int triggerId, int bonusId, int reuseNumber, decimal? sourceAmount, int status)
        {
            var client = CacheManager.GetClientById(clientId);
            if (client == null)
                throw CreateException(LanguageId, Constants.Errors.ClientNotFound);
            var bonusAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.CreateBonus
            });
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });
            var clientAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewClient,
                ObjectTypeId = (int)ObjectTypes.Client
            });
            if ((!clientAccess.HaveAccessForAllObjects && clientAccess.AccessibleObjects.All(x => x != clientId)) ||
                (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleObjects.All(x => x != client.PartnerId)))
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);

            if (!Enum.IsDefined(typeof(ClientBonusTriggerStatuses), status))
                throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);
            var dbTrigger = Db.TriggerSettings.FirstOrDefault(x => x.Id == triggerId);
            if (dbTrigger == null)
                throw CreateException(LanguageId, Constants.Errors.TriggerSettingNotFound);
            var dbBonus = Db.Bonus.FirstOrDefault(x => x.Id == bonusId);
            if (dbBonus == null)
                throw CreateException(LanguageId, Constants.Errors.BonusNotFound);

            var betTriggers = new List<int> { (int)TriggerTypes.BetPlacement, (int)TriggerTypes.BetSettlement,
                        (int)TriggerTypes.BetPlacementAndSettlement, (int)TriggerTypes.CrossProductBetPlacement };
            var partner = CacheManager.GetPartnerById(client.PartnerId);
            var triggerMinAmount = dbTrigger.MinAmount.HasValue ? ConvertCurrency(partner.CurrencyId, client.CurrencyId, dbTrigger.MinAmount.Value) : (decimal?)null;
            var clientBonusTrigger = new ClientBonusTrigger
            {
                ClientId = clientId,
                TriggerId = dbTrigger.Id,
                BonusId = bonusId,
                SourceAmount = sourceAmount != null ? sourceAmount.Value : triggerMinAmount,
                CreationTime = DateTime.UtcNow,
                BetCount = betTriggers.Contains(dbTrigger.Type) ? dbTrigger.MinBetCount : null,
                WageringAmount = betTriggers.Contains(dbTrigger.Type) ? triggerMinAmount : null,
                ReuseNumber = reuseNumber
            };
            if (status == (int)ClientBonusTriggerStatuses.NotRealised)
                Db.ClientBonusTriggers.Where(x => x.ClientId == clientId && x.Id == triggerId &&
                x.BonusId == bonusId && x.ReuseNumber == reuseNumber).DeleteFromQuery();
            else
            {
                var currentTime = GetServerDate();
                if (dbTrigger.StartTime > currentTime || dbTrigger.FinishTime < currentTime)
                    throw CreateException(LanguageId, Constants.Errors.TriggerSettingNotFound);
                if ((dbTrigger.Type == (int)TriggerTypes.NthDeposit || dbTrigger.Type == (int)TriggerTypes.AnyDeposit ||
                     dbTrigger.Type == (int)TriggerTypes.BetPlacement || dbTrigger.Type == (int)TriggerTypes.BetPlacementAndSettlement ||
                     dbTrigger.Type == (int)TriggerTypes.CrossProductBetPlacement) && !sourceAmount.HasValue)
                    throw CreateException(LanguageId, Constants.Errors.WrongOperationAmount);
                if (!Db.TriggerGroupSettings.Any(x => x.Group.BonusId == bonusId && x.SettingId == dbTrigger.Id))
                    throw CreateException(LanguageId, Constants.Errors.TriggerSettingNotFound);

                var t = Db.ClientBonusTriggers.FirstOrDefault(x => x.ClientId == clientId && x.TriggerId == dbTrigger.Id &&
                    x.BonusId == dbBonus.Id && x.ReuseNumber == reuseNumber);
                if (t == null)
                {
                    Db.ClientBonusTriggers.Add(clientBonusTrigger);
                    Db.SaveChanges();
                    return clientBonusTrigger;
                }
                else
                {
                    t.BetCount = clientBonusTrigger.BetCount;
                    t.WageringAmount = clientBonusTrigger.WageringAmount;
                    Db.SaveChanges();
                    return t;
                }
            }
            return clientBonusTrigger;
        }

        public List<TriggerSettingItem> GetClientBonusTriggers(int clientId, int bonusId, int reuseNumber, bool isForAdmin)
        {
            var client = CacheManager.GetClientById(clientId);
            if (client == null)
                throw CreateException(LanguageId, Constants.Errors.ClientNotFound);
            if (isForAdmin)
            {
                var bonusAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewBonuses
                });
                var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewPartner,
                    ObjectTypeId = (int)ObjectTypes.Partner
                });
                var clientAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewClient,
                    ObjectTypeId = (int)ObjectTypes.Client
                });
                if ((!clientAccess.HaveAccessForAllObjects && clientAccess.AccessibleObjects.All(x => x != clientId)) ||
                    (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleObjects.All(x => x != client.PartnerId)))
                    throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            }
            var partner = CacheManager.GetPartnerById(client.PartnerId);
            var settings = Db.TriggerGroups.Where(x => x.BonusId == bonusId).Select(x => new
            {
                Id = x.Id,
                Name = x.Name,
                Type = x.Type,
                TriggerSettings = x.TriggerGroupSettings.Select(y => new TriggerSettingItem
                {
                    Id = y.Setting.Id,
                    Name = y.Setting.Name,
                    TranslationId = y.Setting.TranslationId,
                    Description = y.Setting.Description,
                    Type = y.Setting.Type,
                    StartTime = y.Setting.StartTime,
                    FinishTime = y.Setting.FinishTime,
                    Percent = y.Setting.Percent,
                    BonusSettingCodes = y.Setting.BonusSettingCodes,
                    PartnerId = y.Setting.PartnerId,
                    CreationTime = y.Setting.CreationTime,
                    LastUpdateTime = y.Setting.LastUpdateTime,
                    MinAmount = y.Setting.MinAmount,
                    MaxAmount = y.Setting.MaxAmount,
                    Order = y.Order,
                    MinBetCount = y.Setting.MinBetCount
                }).ToList()
            });
            var resp = new List<TriggerSettingItem>();

            foreach (var s in settings)
            {
                if (!isForAdmin)
                {
                    foreach (var ts in s.TriggerSettings)
                    {
                        ts.Name = CacheManager.GetTranslation(ts.TranslationId, LanguageId);
                    }
                }
                resp.AddRange(s.TriggerSettings);
            }

            var triggers = Db.ClientBonusTriggers.Where(x => x.BonusId == bonusId && (x.ClientId == clientId || x.ClientId == null) &&
                x.ReuseNumber == reuseNumber).ToList();
            var betTriggers = new List<int> { (int)TriggerTypes.BetPlacement, (int)TriggerTypes.BetSettlement,
                        (int)TriggerTypes.BetPlacementAndSettlement, (int)TriggerTypes.CrossProductBetPlacement };

            foreach (var r in resp)
            {
                r.MinAmount = r.MinAmount.HasValue ? ConvertCurrency(partner.CurrencyId, client.CurrencyId, r.MinAmount.Value) : (decimal?)null;
                r.MaxAmount = r.MaxAmount.HasValue ? ConvertCurrency(partner.CurrencyId, client.CurrencyId, r.MaxAmount.Value) : (decimal?)null;
                var item = triggers.FirstOrDefault(x => x.TriggerId == r.Id);
                if (item != null)
                {
                    if (!betTriggers.Contains(r.Type) || (item.BetCount >= r.MinBetCount && (r.MinAmount == null || item.WageringAmount >= r.MinAmount)))
                    {
                        r.Status = (int)ClientBonusTriggerStatuses.Realised;
                        r.SourceAmount = item.SourceAmount;
                    }
                    else
                        r.Status = (int)ClientBonusTriggerStatuses.NotRealised;

                    r.BetCount = item.BetCount;
                    r.WageringAmount = item.WageringAmount;
                }
                else
                {
                    r.Status = (int)ClientBonusTriggerStatuses.NotRealised;

                    if (betTriggers.Contains(r.Type))
                    {
                        r.BetCount = 0;
                        r.WageringAmount = r.MinAmount.HasValue ? 0 : (decimal?)null;
                    }
                }
            }
            return resp;
        }
        public PagedModel<fnClientBonus> GetClientBonuses(int clientId, int? bonusType)
        {
            var clientAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewClient,
                ObjectTypeId = (int)ObjectTypes.Client
            });

            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });
            GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewBonuses
            });
            var filter = new FilterReportByBonus
            {
                ClientIds = new FiltersOperation
                {
                    IsAnd = true,
                    OperationTypeList = new List<FiltersOperationType> {
                            new FiltersOperationType
                            {
                                OperationTypeId = (int)FilterOperations.IsEqualTo,
                                IntValue = clientId
                            }
                        }
                },
                SkipCount = 0,
                TakeCount = 100,
                FromDate = new DateTime(2000, 1, 1),
                ToDate = DateTime.Now.AddDays(1)
            };
            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnClientBonus>>
            {
                new CheckPermissionOutput<fnClientBonus>
                {
                    AccessibleObjects = partnerAccess.AccessibleObjects,
                    HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                    Filter = x => partnerAccess.AccessibleObjects.AsEnumerable().Contains(x.PartnerId)
                },
                new CheckPermissionOutput<fnClientBonus>
                {
                    AccessibleObjects = clientAccess.AccessibleObjects,
                    HaveAccessForAllObjects = clientAccess.HaveAccessForAllObjects,
                    Filter = x => clientAccess.AccessibleObjects.AsEnumerable().Contains(x.ClientId)
                }
            };
            Func<IQueryable<fnClientBonus>, IOrderedQueryable<fnClientBonus>> orderBy;

            if (filter.OrderBy.HasValue)
            {
                if (filter.OrderBy.Value)
                {
                    orderBy = QueryableUtilsHelper.OrderByFunc<fnClientBonus>(filter.FieldNameToOrderBy, true);
                }
                else
                {
                    orderBy = QueryableUtilsHelper.OrderByFunc<fnClientBonus>(filter.FieldNameToOrderBy, false);
                }
            }
            else
            {
                orderBy = clientBonuses => clientBonuses.OrderByDescending(x => x.Id);
            }

            if (bonusType.HasValue)
            {
                filter.BonusTypes = new FiltersOperation
                {
                    IsAnd = true,
                    OperationTypeList = new List<FiltersOperationType> {
                        new FiltersOperationType
                        {
                            OperationTypeId = (int)FilterOperations.IsEqualTo,
                            IntValue = bonusType.Value
                        }
                    }
                };
            }
            return new PagedModel<fnClientBonus>
            {
                Entities = filter.FilterObjects(Db.fn_ClientBonus(LanguageId), orderBy).ToList(),
                Count = filter.SelectedObjectsCount(Db.fn_ClientBonus(LanguageId))
            };
        }

        public ClientBonu GetClientBonusById(int id)
        {
            return Db.ClientBonus.FirstOrDefault(x => x.Id == id);
        }

        public List<fnClientBonus> GetClientActiveBonuses(int clientId, int bonusType, string languegId)
        {
            var resp = Db.fn_ClientBonus(languegId).Where(x => x.ClientId == clientId && x.BonusType == bonusType && x.Status == (int)BonusStatuses.Active).ToList();
            return resp;
        }

        public ClientBonu GetClientFreeSpinBonus(int clientId, int productId)
        {
            var currDate = GetServerDate();
            var product = productId.ToString();
            return Db.ClientBonus.Where(x => x.ClientId == clientId && x.Bonus.BonusType == (int)BonusTypes.CompaignFreeSpin &&
            x.Bonus.Info == product && x.Bonus.StartTime <= currDate && x.Status == (int)BonusStatuses.Finished &&
            x.Bonus.FinishTime > currDate && x.Bonus.Status && x.ReuseNumber == 1).FirstOrDefault();
        }

        public fnClientBonus ApproveClientCashbackBonus(int clientBonusId)
        {
            var bonusAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.CreateBonus
            });
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });

            if (!bonusAccess.HaveAccessForAllObjects)
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);

            var dbClientBonus = Db.ClientBonus.Include(x => x.Bonus).FirstOrDefault(x => x.Id == clientBonusId);
            if (dbClientBonus == null)
                throw CreateException(LanguageId, Constants.Errors.BonusNotFound);
            if (dbClientBonus.Bonus.BonusType != (int)BonusTypes.CashBackBonus || dbClientBonus.Status != (int)BonusStatuses.NotAwarded)
                throw CreateException(LanguageId, Constants.Errors.NotAllowed);
            var oldValue = new
            {
                dbClientBonus.Id,
                dbClientBonus.BonusId,
                dbClientBonus.ClientId,
                dbClientBonus.Status,
                dbClientBonus.BonusPrize,
                dbClientBonus.CreationTime,
                dbClientBonus.AwardingTime,
                dbClientBonus.FinalAmount,
                dbClientBonus.CalculationTime,
                dbClientBonus.ValidUntil
            };
            SaveChangesWithHistory((int)ObjectTypes.ClientBonus, dbClientBonus.Id, JsonConvert.SerializeObject(oldValue), string.Empty);
            dbClientBonus.Status = (int)BonusStatuses.Active;
            Db.SaveChanges();
            return Db.fn_ClientBonus(Identity.LanguageId).FirstOrDefault(x => x.Id == clientBonusId);
        }

        public fnClientBonus CancelClientBonus(int clientBonusId, bool isFromAdmin)
        {
            var currentTime = DateTime.UtcNow;
            if (isFromAdmin)
            {
                var bonusAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.CreateBonus
                });
                var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewPartner,
                    ObjectTypeId = (int)ObjectTypes.Partner
                });

                if (!bonusAccess.HaveAccessForAllObjects)
                    throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            }
            fnClientBonus resp = null;
            using (var scope = CommonFunctions.CreateTransactionScope())
            {
                var dbClientBonus = Db.ClientBonus.Include(x => x.Bonus).FirstOrDefault(x => x.Id == clientBonusId);
                if (dbClientBonus == null)
                    throw CreateException(LanguageId, Constants.Errors.BonusNotFound);

                if (dbClientBonus.Status != (int)BonusStatuses.Active && dbClientBonus.Status != (int)BonusStatuses.NotAwarded)
                    throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);

                var oldValue = new
                {
                    dbClientBonus.Id,
                    dbClientBonus.BonusId,
                    dbClientBonus.ClientId,
                    dbClientBonus.Status,
                    dbClientBonus.BonusPrize,
                    dbClientBonus.CreationTime,
                    dbClientBonus.AwardingTime,
                    dbClientBonus.TurnoverAmountLeft,
                    dbClientBonus.FinalAmount,
                    dbClientBonus.CalculationTime,
                    dbClientBonus.ValidUntil,
                    dbClientBonus.TriggerId
                };
                SaveChangesWithHistory((int)ObjectTypes.ClientBonus, dbClientBonus.Id, JsonConvert.SerializeObject(oldValue), string.Empty);
                var client = CacheManager.GetClientById(dbClientBonus.ClientId);
                dbClientBonus.Status = (int)BonusStatuses.Closed;
                dbClientBonus.FinalAmount = 0;
                dbClientBonus.CalculationTime = DateTime.UtcNow;
                Db.SaveChanges();
                if (dbClientBonus.Bonus.BonusType == (int)BonusTypes.CampaignWagerCasino || dbClientBonus.Bonus.BonusType == (int)BonusTypes.CampaignWagerSport)
                {
                    using var documentBl = new DocumentBll(this);
                    var account = Db.Accounts.FirstOrDefault(x => x.ObjectTypeId == (int)ObjectTypes.Client &&
                        x.ObjectId == client.Id && x.Type.Id == (int)AccountTypes.ClientBonusBalance);
                    if (account != null && account.Balance != 0)
                    {
                        var input = new Operation
                        {
                            Amount = account.Balance,
                            CurrencyId = client.CurrencyId,
                            Type = (int)OperationTypes.WageringBonus,
                            ClientId = client.Id,
                            OperationItems = new List<OperationItem>
                                {
                                    new OperationItem
                                    {
                                        AccountTypeId = (int)AccountTypes.ClientBonusBalance,
                                        ObjectId = client.Id,
                                        ObjectTypeId = (int)ObjectTypes.Client,
                                        Amount = account.Balance,
                                        CurrencyId = client.CurrencyId,
                                        Type = (int)TransactionTypes.Credit,
                                        OperationTypeId =(int)OperationTypes.WageringBonus
                                    },
                                    new OperationItem
                                    {
                                        AccountTypeId = (int)AccountTypes.PartnerBalance,
                                        ObjectId = client.PartnerId,
                                        ObjectTypeId = (int)ObjectTypes.Partner,
                                        Amount = account.Balance,
                                        CurrencyId = client.CurrencyId,
                                        Type = (int)TransactionTypes.Debit,
                                        OperationTypeId = (int)OperationTypes.WageringBonus
                                    }
                                }
                        };
                        documentBl.CreateDocument(input);
                        Db.SaveChanges();
                    }
                    resp = Db.fn_ClientBonus(Identity.LanguageId).FirstOrDefault(x => x.Id == clientBonusId);
                    scope.Complete();

                }
                else
                {
                    resp = Db.fn_ClientBonus(Identity.LanguageId).FirstOrDefault(x => x.Id == clientBonusId);
                    scope.Complete();
                }
            }
            return resp;
        }

        public ClientBonu CancelClientFreespin(int clientBonusId, bool isFromAdmin)
        {
            var currentTime = DateTime.UtcNow;
            if (isFromAdmin)
            {
                var bonusAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.CreateBonus
                });
                var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewPartner,
                    ObjectTypeId = (int)ObjectTypes.Partner
                });

                if (!bonusAccess.HaveAccessForAllObjects)
                    throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            }
            using var scope = CommonFunctions.CreateTransactionScope();
            var dbClientBonus = Db.ClientBonus.Include(x => x.Bonus).FirstOrDefault(x => x.Id == clientBonusId);
            if (dbClientBonus == null)
                throw CreateException(LanguageId, Constants.Errors.BonusNotFound);

            if (dbClientBonus.Status != (int)BonusStatuses.Active)
                throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);

            var client = CacheManager.GetClientById(dbClientBonus.ClientId);
            dbClientBonus.Status = (int)BonusStatuses.Closed;
            dbClientBonus.FinalAmount = 0;
            Db.SaveChanges();
            return dbClientBonus;
        }

        public void ExpireClientPlatformSession(int clientId)
        {
            var currentTime = GetServerDate();
            var sessions = Db.ClientSessions.Where(x => x.ClientId == clientId && x.ProductId == Constants.PlatformProductId
                                       && x.State == (int)SessionStates.Active);
            var lastSession = sessions.OrderByDescending(x => x.Id).FirstOrDefault();
            if (lastSession != null)
            {
                sessions.UpdateFromQuery(x => new ClientSession
                {
                    State = (int)SessionStates.Inactive,
                    LogoutType = (int)LogoutTypes.Expired,
                    EndTime = currentTime
                });
                Db.Clients.Where(c => c.Id == clientId).UpdateFromQuery(x => new Client { LastSessionId = lastSession.Id });
                CacheManager.RemoveClientPlatformSession(clientId);
                foreach (var s in sessions)
                {
                    CacheManager.RemoveClientProductSession(s.Token, Constants.PlatformProductId);
                }
                CacheManager.RemoveClientFromCache(clientId);
            }
        }

        public ClientBonu GiveFreeSpinToClient(int clientId, int bonusSettingId, int spinsCount, DateTime StartTime, DateTime FinishTime)
        {
            var currentTime = DateTime.UtcNow;
            var bonusAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.CreateBonus
            });
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });
            if (!bonusAccess.HaveAccessForAllObjects)
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            var client = CacheManager.GetClientById(clientId);
            if (client == null)
                throw CreateException(Identity.LanguageId, Constants.Errors.ClientNotFound);
            var dbClientBonus = Db.ClientBonus.FirstOrDefault(x => x.ClientId == client.Id && x.BonusId == bonusSettingId && x.ReuseNumber == 1);
            if (dbClientBonus != null)
            {
                if (dbClientBonus.CalculationTime < currentTime && dbClientBonus.Status == (int)BonusStatuses.Active)
                    dbClientBonus.Status = (int)BonusStatuses.Finished;
                if (StartTime <= currentTime || FinishTime < StartTime ||
                dbClientBonus.CalculationTime > StartTime || dbClientBonus.Status == (int)BonusStatuses.Active)
                    throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);
            }
            var bonus = Db.Bonus.Include(x => x.BonusProducts).FirstOrDefault(x => x.Id == bonusSettingId && x.Status &&
                 x.StartTime <= currentTime && x.FinishTime > currentTime);
            if (bonus == null)
                throw CreateException(Identity.LanguageId, Constants.Errors.BonusNotFound);
            var bonusProduct = bonus.BonusProducts.FirstOrDefault(x => x.BonusId == bonusSettingId);
            if (bonusProduct == null)
                throw CreateException(Identity.LanguageId, Constants.Errors.BonusNotFound);
            var currentDate = DateTime.UtcNow;
            var clientBonus = new ClientBonu
            {
                BonusId = bonusSettingId,
                ClientId = client.Id,
                Status = (int)BonusStatuses.Active,
                BonusPrize = spinsCount,
                CreationTime = StartTime,
                AwardingTime = StartTime,
                CreationDate = (long)currentTime.Year * 100000000 + currentTime.Month * 1000000 +
                                currentTime.Day * 10000 + currentTime.Hour * 100 + currentTime.Minute,
                CalculationTime = FinishTime,
                ReuseNumber = 1
            };
            Db.ClientBonus.Add(clientBonus);
            Db.SaveChanges();
            return clientBonus;
        }

        public PaymentRequest GetClientLastDeposit(int paymentSystemId, int clientId)
        {
            return Db.PaymentRequests.Where(x => x.ClientId == clientId && x.Type == (int)PaymentRequestTypes.Deposit &&
                (x.Status == (int)PaymentRequestStates.Approved || x.Status == (int)PaymentRequestStates.ApprovedManually) &&
                x.PaymentSystemId == paymentSystemId).OrderByDescending(x => x.Id).FirstOrDefault();
        }

        public PaymentRequest GetClientLastDepositWithParams(int paymentSystemId, int clientId)
        {
            return Db.PaymentRequests.Where(x => x.ClientId == clientId && x.Type == (int)PaymentRequestTypes.Deposit &&
                (x.Status == (int)PaymentRequestStates.Approved || x.Status == (int)PaymentRequestStates.ApprovedManually) &&
                x.PaymentSystemId == paymentSystemId && x.Parameters != null).OrderByDescending(x => x.Id).FirstOrDefault();
        }

        public List<BonusInfo> GetClientDepositBonusInfo(BllClient client)
        {
            var currentTime = DateTime.UtcNow;
            var clientSegmentsIds = new List<int>();
            var clientClasifications = CacheManager.GetClientClasifications(client.Id);
            if (clientClasifications.Any())
                clientSegmentsIds = clientClasifications.Where(x => x.SegmentId.HasValue && x.ProductId == (int)Constants.PlatformProductId)
                                                        .Select(x => x.SegmentId.Value).ToList();

            var bonuses = Db.Bonus.Include(x => x.TriggerGroups).ThenInclude(y => y.TriggerGroupSettings).ThenInclude(x => x.Setting)
                                  .Where(x => x.Status && x.PartnerId == client.PartnerId &&
                                              x.StartTime < currentTime && x.FinishTime > currentTime &&
                                             (!x.MaxGranted.HasValue || x.TotalGranted < x.MaxGranted) &&
                                             (!x.MaxReceiversCount.HasValue || x.TotalReceiversCount < x.MaxReceiversCount) &&
                                  ((x.ReusingMaxCount == null && !x.ClientBonus.Any(y => y.ClientId == client.Id)) ||
                                  (x.ReusingMaxCount != null && !x.ClientBonus.Any(y => y.ClientId == client.Id && y.ReuseNumber >= x.ReusingMaxCount))) &&
                                  (!x.BonusSegmentSettings.Any() ||
                                    x.BonusSegmentSettings.Any(y => (y.Type == (int)BonusSettingConditionTypes.InSet && clientSegmentsIds.Contains(y.SegmentId))) ||
                                    (x.BonusSegmentSettings.Any(y => y.Type == (int)BonusSettingConditionTypes.OutOfSet) &&
                                     x.BonusSegmentSettings.All(y => !clientSegmentsIds.Contains(y.SegmentId)))
                                  ) &&
                                  (!x.BonusCountrySettings.Any() ||
                                    x.BonusCountrySettings.Any(y => y.Type == (int)BonusSettingConditionTypes.InSet && y.CountryId == client.RegionId) ||
                                    (x.BonusCountrySettings.Any(y => y.Type == (int)BonusSettingConditionTypes.OutOfSet) &&
                                     x.BonusCountrySettings.All(y => y.CountryId != client.RegionId))
                                  ) &&
                                  (!x.BonusCurrencySettings.Any() ||
                                    x.BonusCurrencySettings.Any(y => y.Type == (int)BonusSettingConditionTypes.InSet && y.CurrencyId == client.CurrencyId) ||
                                    (x.BonusCurrencySettings.Any(y => y.Type == (int)BonusSettingConditionTypes.OutOfSet) &&
                                     x.BonusCurrencySettings.All(y => y.CurrencyId != client.CurrencyId))
                                  ) &&
                                  (!x.BonusLanguageSettings.Any() ||
                                    x.BonusLanguageSettings.Any(y => y.Type == (int)BonusSettingConditionTypes.InSet && y.LanguageId == client.LanguageId) ||
                                    (x.BonusLanguageSettings.Any(y => y.Type == (int)BonusSettingConditionTypes.OutOfSet) &&
                                     x.BonusLanguageSettings.All(y => y.LanguageId != client.LanguageId))
                                  )).ToList();
            var depCount = CacheManager.GetClientDepositCount(client.Id);
            var availableBonuses = bonuses.Where(x => (x.TriggerGroups.Any(y => y.Priority == 0 && y.TriggerGroupSettings.Count == 1 &&
                                                      y.TriggerGroupSettings.Any(z => z.Setting.Type == (int)TriggerTypes.PromotionalCode))) ||

                                                      (!x.TriggerGroups.Any(y => y.Priority == 0 && y.TriggerGroupSettings.Count > 0) &&
                                                      x.TriggerGroups.Any(y => y.TriggerGroupSettings.Any(z => z.Setting.Type == (int)TriggerTypes.AnyDeposit ||
                                                      (z.Setting.Type == (int)TriggerTypes.NthDeposit && z.Setting.Condition == (depCount + 1).ToString()))))
                                                      ).ToList();
            var tIds = availableBonuses.Select(x => x.TranslationId).ToList();
            var translations = Db.fn_Translation(Identity.LanguageId).Where(x => tIds.Contains(x.TranslationId)).ToList();
            return availableBonuses.Select(x => new BonusInfo
            {
                Id = x.Id,
                Name = translations.FirstOrDefault(y => y.TranslationId == x.TranslationId)?.Text,
                BonusType = x.BonusType,
                MinAmount = x.MinAmount,
                MaxAmount = x.MaxAmount,
                HasPromo = x.TriggerGroups.Any(y => y.Priority == 0 && y.TriggerGroupSettings.Count == 1 &&
                                               y.TriggerGroupSettings.Any(z => z.Setting.Type == (int)TriggerTypes.PromotionalCode))
            }).ToList();
        }

        public PromoCode GetPromoCode(int partneId, string promoCode)
        {
            var dbPromoCode = Db.PromoCodes.FirstOrDefault(x => x.Code == promoCode && x.PartnerId == partneId);
            if (dbPromoCode == null)
                throw CreateException(Identity.LanguageId, Constants.Errors.PromoCodeNotExists);
            if (dbPromoCode.State != (int)PromoCodesState.Active)
                throw CreateException(Identity.LanguageId, Constants.Errors.PromoCodeExpired);
            return dbPromoCode;
        }

        /*
        public int GiveSpinBonus(int clientId)
        {
            var client = CacheManager.GetClientById(clientId);
            if (client == null)
                throw CreateException(Identity.LanguageId, Constants.Errors.ClientNotFound);
            if (!client.IsEmailVerified && !client.IsMobileNumberVerified)
                throw CreateException(Identity.LanguageId, Constants.Errors.ClientNotVerified);
            var currentTime = DateTime.UtcNow;

            var spinBonus = Db.Bonus.FirstOrDefault(x => x.PartnerId == client.PartnerId &&
            x.BonusType == (int)BonusTypes.SpinBonus &&
            x.Status && x.StartTime <= currentTime && x.FinishTime > currentTime);
            if (spinBonus == null)
                throw CreateException(Identity.LanguageId, Constants.Errors.BonusNotFound);
            if (Db.ClientBonus.Any(x => x.ClientId == client.Id && x.BonusId == spinBonus.Id && x.CalculationTime.Value.Subtract(currentTime).TotalHours < 24))
                throw CreateException(Identity.LanguageId, Constants.Errors.BonusNotFound);
            decimal[] spinSectors = spinBonus.Info.Split(',').Select(decimal.Parse).ToArray();// chnage to json object
            var rand = new Random();
            var val = rand.Next(1, 99);
            var boundary = 0m;
            var index = 0;
            for (int i = 0; i < spinSectors.Length; ++i)
            {
                boundary += spinSectors[i];
                if (val <= boundary)
                {
                    index = i;
                    break;
                }
            }
            //Give bonus
            return index;
        } //to check

        public void CheckSpinBonus(int clientId)
        {
            var client = CacheManager.GetClientById(clientId);
            if (client == null)
                throw CreateException(Identity.LanguageId, Constants.Errors.ClientNotFound);
            if (!client.IsEmailVerified && !client.IsMobileNumberVerified)
                throw CreateException(Identity.LanguageId, Constants.Errors.ClientNotVerified);
            var currentTime = DateTime.UtcNow;
            var spinBonus = Db.Bonus.FirstOrDefault(x => x.PartnerId == client.PartnerId &&
            x.BonusType == (int)BonusTypes.SpinBonus &&
            x.Status && x.StartTime <= currentTime && x.FinishTime > currentTime);
            if (spinBonus == null)
                throw CreateException(Identity.LanguageId, Constants.Errors.BonusNotFound);
            if (Db.ClientBonus.Any(x => x.ClientId == client.Id && x.BonusId == spinBonus.Id && x.CalculationTime.Value.Subtract(currentTime).TotalHours < 24))
                throw CreateException(Identity.LanguageId, Constants.Errors.BonusNotFound);
        } //to check
        */

        #region ClientDocument

        #region Withdraw

        public PaymentRequest CreateWithdrawPaymentRequest(PaymentRequestModel request, decimal percent, BllClient client,
                                                          DocumentBll documentBl, NotificationBll notificationBll)
        {
            using var scope = CommonFunctions.CreateTransactionScope();
            var currentTime = DateTime.UtcNow;
            var date = (long)currentTime.Year * 100000000 + (long)currentTime.Month * 1000000 + (long)currentTime.Day * 10000 +
                       (long)currentTime.Hour * 100 + currentTime.Minute;

            if (client.State == (int)ClientStates.FullBlocked || client.State == (int)ClientStates.Disabled ||
                client.State == (int)ClientStates.Suspended || client.State == (int)ClientStates.BlockedForWithdraw)
                throw CreateException(LanguageId, Constants.Errors.ClientBlocked);
            var paymentSystem = CacheManager.GetPaymentSystemById(request.PaymentSystemId);
            if (paymentSystem == null)
                throw CreateException(LanguageId, Constants.Errors.PaymentSystemNotFound);
            var isBetShop = paymentSystem.Name.ToLower() == Constants.PaymentSystems.BetShop.ToLower();
            var partner = CacheManager.GetPartnerById(request.PartnerId);
            var withdrawMaxKey = isBetShop ? CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.CashWithdrawMaxCountPerDayPerCustomer) :
                CacheManager.GetPartnerSettingByKey(client.PartnerId, Constants.PartnerKeys.WithdrawMaxCountPerDayPerCustomer);

            if (CacheManager.GetConfigKey(client.PartnerId, Constants.PartnerKeys.AMLVerification) == "1")
            {
                var amlVerified = CacheManager.GetClientSettingByName(client.Id, ClientSettings.AMLVerified);
                if (amlVerified == null || amlVerified.Id == 0 || amlVerified.StringValue != "1")
                    throw CreateException(LanguageId, Constants.Errors.AMLProhibited);
            }

            var documentSetting = CacheManager.GetConfigKey(client.PartnerId, Constants.PartnerKeys.RequireDocumentForWithdrawal);
            if (documentSetting == "1")
            {
                if (!client.IsDocumentVerified)
                    throw CreateException(LanguageId, Constants.Errors.DocumentNotVerified);

                var documents = GetClientIdentities(client.Id);
                var documentExpired = (!documents.Any(x => x.Status == (int)KYCDocumentStates.Approved) && documents.Any(x => x.Status == (int)KYCDocumentStates.Expired));
                if (documentExpired)
                    throw CreateException(LanguageId, Constants.Errors.DocumentExpired);
            }
            var suspensionSetting = CacheManager.GetClientSettingByName(client.Id, Constants.ClientSettings.CautionSuspension);
            if (suspensionSetting != null && suspensionSetting.Id > 0 && suspensionSetting.StringValue == "1")
                throw CreateException(LanguageId, Constants.Errors.CautionSuspension);

            if (withdrawMaxKey != null && withdrawMaxKey.NumericValue != null)
            {
                var startTime = currentTime.AddDays(-1);
                var fDate = (long)startTime.Year * 100000000 + (long)startTime.Month * 1000000 + (long)startTime.Day * 10000 +
                            (long)startTime.Hour * 100 + startTime.Minute;
                if (Db.PaymentRequests.Count(x => x.ClientId == client.Id && x.Date >= fDate && x.Type == (int)PaymentRequestTypes.Withdraw &&
                    x.Status != (int)PaymentRequestStates.CanceledByClient && x.Status != (int)PaymentRequestStates.CanceledByUser &&
                    x.Status != (int)PaymentRequestStates.Deleted && x.Status != (int)PaymentRequestStates.Failed) >= withdrawMaxKey.NumericValue.Value)
                    throw CreateException(LanguageId, Constants.Errors.PaymentRequestNotAllowed);
            }
            if (isBetShop)
            {
                if (!request.BetShopId.HasValue)
                    throw CreateException(LanguageId, Constants.Errors.BetShopNotFound);

                var betShop = CacheManager.GetBetShopById(request.BetShopId.Value);
                if (betShop == null)
                    throw CreateException(LanguageId, Constants.Errors.BetShopNotFound);
                if (betShop.CurrencyId != client.CurrencyId)
                    throw CreateException(LanguageId, Constants.Errors.ImpermissibleBetShop);
                if (string.IsNullOrEmpty(request.CashCode))
                    throw CreateException(LanguageId, Constants.Errors.WrongCashCode);
            }
            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(request.PartnerId, request.PaymentSystemId,
                request.CurrencyId, (int)PaymentRequestTypes.Withdraw);
            if (partnerPaymentSetting == null)
                throw CreateException(LanguageId, Constants.Errors.PartnerPaymentSettingNotFound);
            if (request.Amount < partnerPaymentSetting.MinAmount || request.Amount > partnerPaymentSetting.MaxAmount)
                throw CreateException(LanguageId, Constants.Errors.PaymentRequestInValidAmount);

            if (Db.ClientPaymentSettings.Any(x => x.ClientId == request.ClientId &&
                                                  x.PartnerPaymentSettingId == partnerPaymentSetting.Id &&
                                                  x.State == (int)ClientPaymentStates.Blocked))
                throw CreateException(LanguageId, Constants.Errors.PartnerPaymentSettingBlocked);
            if (string.IsNullOrEmpty(request.Info))
                throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);
            var paymentInfo = JsonConvert.DeserializeObject<Common.Models.PaymentInfo>(request.Info);
            paymentInfo.Domain = Identity.Domain;
            var partnerSetting = CacheManager.GetConfigKey(client.PartnerId, Constants.PartnerKeys.PaymentDetailsValidation);
            if (partnerSetting == "1")
            {
                if (Db.ClientPaymentInfoes.Any(x => x.ClientId == client.Id && x.State == (int)ClientPaymentInfoStates.Pending))
                    throw CreateException(LanguageId, Constants.Errors.ImpermissiblePaymentSetting);

                var partnerPaymentSettingDeposit = CacheManager.GetPartnerPaymentSettings(request.PartnerId, request.PaymentSystemId,
                    request.CurrencyId, (int)PaymentRequestTypes.Deposit);
                if (partnerPaymentSettingDeposit == null)
                    throw CreateException(LanguageId, Constants.Errors.PartnerPaymentSettingNotFound);

                if (paymentSystem.Name != PaymentSystems.BetShop)
                {
                    if (!Db.ClientPaymentInfoes.Any(x => x.ClientId == client.Id &&
                        x.State == (int)ClientPaymentInfoStates.Verified && x.PartnerPaymentSystemId == partnerPaymentSettingDeposit.Id && (
                        (x.Type == (int)ClientPaymentInfoTypes.BankAccount && x.BankAccountNumber == paymentInfo.BankAccountNumber) ||
                        (x.Type == (int)ClientPaymentInfoTypes.Wallet && x.WalletNumber == paymentInfo.WalletNumber) ||
                        (x.Type == (int)ClientPaymentInfoTypes.CreditCard && x.CardNumber == paymentInfo.CardNumber))
                        ))
                        throw CreateException(LanguageId, Constants.Errors.ImpermissiblePaymentSetting);
                }
            }
            var paymentLimit = Db.PaymentLimits.FirstOrDefault(x => x.ClientId == client.Id);
            if (paymentLimit != null)
                CheckWithdrawLimits(paymentLimit, currentTime, request.Amount);
            if (request.CurrencyId != client.CurrencyId)
            {
                request.Amount = ConvertCurrency(request.CurrencyId, client.CurrencyId, request.Amount);
                request.CurrencyId = client.CurrencyId;
            }
            var clientBalance = GetObjectBalance((int)ObjectTypes.Client, request.ClientId);
            if (clientBalance.Balances.Where(x => x.TypeId != (int)AccountTypes.ClientBonusBalance)
                .Sum(x => x.TypeId == (int)AccountTypes.ClientUnusedBalance ? x.Balance * (100 - percent) / 100 : x.Balance) < request.Amount)
                throw CreateException(LanguageId, Constants.Errors.LowBalance);
            if ((request.Amount - partnerPaymentSetting.FixedFee - request.Amount * partnerPaymentSetting.Commission / 100m) < 0)
                throw CreateException(LanguageId, Constants.Errors.LowBalance);
            var maxUsedBalance = clientBalance.Balances.Where(x => x.TypeId != (int)AccountTypes.ClientUnusedBalance && x.TypeId != (int)AccountTypes.ClientBonusBalance)
                .Sum(x => x.Balance);
            decimal extraAmount = 0;
            if (maxUsedBalance < request.Amount)
                extraAmount = (request.Amount - maxUsedBalance) * percent / (100 - percent);
            request.Info = JsonConvert.SerializeObject(paymentInfo, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore
            });
            if (request.Parameters == null || !request.Parameters.Any())
                request.Parameters = new Dictionary<string, string>();
            request.Parameters.Add(nameof(request.SavePaymentDetails), request.SavePaymentDetails.ToString());
            var paymentRequest = new PaymentRequest
            {
                ClientId = request.ClientId,
                Amount = request.Amount,
                CurrencyId = client.CurrencyId,
                Status = (int)PaymentRequestStates.Pending,
                PartnerPaymentSettingId = partnerPaymentSetting.Id,
                BetShopId = request.BetShopId,
                CashDeskId = request.CashDeskId,
                Parameters = JsonConvert.SerializeObject(request.Parameters),
                PaymentSystemId = request.PaymentSystemId,
                Info = string.IsNullOrEmpty(request.Info) ? "{}" : request.Info,
                CardNumber = paymentInfo?.CardNumber,
                Type = (int)PaymentRequestTypes.Withdraw,
                SessionId = Identity.IsAdminUser ? Identity.SessionId : (long?)null,
                CreationTime = currentTime,
                LastUpdateTime = currentTime,
                CashCode = request.CashCode,
                Date = date,
                CommissionAmount = request.Amount * partnerPaymentSetting.Commission / 100 + partnerPaymentSetting.FixedFee
            };
            Db.PaymentRequests.Add(paymentRequest);
            Db.SaveChanges();
            BookMoneyFromClient(paymentRequest, extraAmount, documentBl);
            Db.SaveChanges();
            notificationBll.SendWitdrawNotification(paymentRequest.ClientId, paymentRequest.Status, paymentRequest.Amount, string.Empty);
            scope.Complete();
            CacheManager.RemoveClientBalance(client.Id);
            return paymentRequest;
        }

        public void CancelWithdrawRelatedCampaigns()
        {
            Db.ClientBonus.Where(x => x.ClientId == Identity.Id && x.Bonus.ResetOnWithdraw == true &&
                                     (x.Status == (int)BonusStatuses.NotAwarded || x.Status == (int)BonusStatuses.Active))
                          .UpdateFromQuery(x => new ClientBonu { Status = (int)BonusStatuses.Lost });
        }

        private void BookMoneyFromClient(PaymentRequest request, decimal extraAmount, DocumentBll documentBl)
        {
            var operation = new Operation
            {
                Amount = request.Amount + extraAmount,
                CurrencyId = request.CurrencyId,
                Type = (int)OperationTypes.PaymentRequestBooking,
                PaymentRequestId = request.Id,
                ClientId = request.ClientId,
                PartnerPaymentSettingId = request.PartnerPaymentSettingId,
                OperationItems = new List<OperationItem>()
            };
            var item = new OperationItem
            {
                AccountTypeId = (int)AccountTypes.ClientBooking,
                ObjectId = request.ClientId,
                ObjectTypeId = (int)ObjectTypes.Client,
                Amount = request.Amount,
                CurrencyId = request.CurrencyId,
                Type = (int)TransactionTypes.Debit,
                OperationTypeId = (int)OperationTypes.PaymentRequestBooking
            };
            operation.OperationItems.Add(item);
            item = new OperationItem
            {
                AccountTypeId = (int)AccountTypes.PaymentSystemSettingDebtToPartner,
                ObjectId = request.PartnerPaymentSettingId.Value,
                ObjectTypeId = (int)ObjectTypes.PaymentSystem,
                Amount = extraAmount,
                CurrencyId = request.CurrencyId,
                Type = (int)TransactionTypes.Debit,
                OperationTypeId = (int)OperationTypes.PaymentRequestBooking
            };
            operation.OperationItems.Add(item);
            item = new OperationItem
            {
                ObjectId = request.ClientId,
                ObjectTypeId = (int)ObjectTypes.Client,
                Amount = request.Amount + extraAmount,
                CurrencyId = request.CurrencyId,
                Type = (int)TransactionTypes.Credit,
                OperationTypeId = (int)OperationTypes.PaymentRequestBooking
            };
            operation.OperationItems.Add(item);
            documentBl.CreateDocument(operation);
        }

        public ChangeWithdrawRequestStateOutput ChangeWithdrawRequestState(long requestId, PaymentRequestStates state, string comment,
              int? cashDeskId, int? cashierId, bool checkPermission, string parameters, DocumentBll documentBl, NotificationBll notificationBl,
              bool sendEmail = false)
        {
            ChangeWithdrawRequestStateOutput response;
            BllClient client;
            string requestParams;
            var currentDate = DateTime.UtcNow;
            using (var scope = CommonFunctions.CreateTransactionScope())
            {
                Db.Procedures.sp_GetPaymentRequestLockAsync(requestId).Wait();
                var request = Db.PaymentRequests.Include(x => x.PartnerPaymentSetting).Include(x => x.PaymentSystem).First(x => x.Id == requestId);
                if (request == null || request.Type != (int)PaymentRequestTypes.Withdraw)
                    throw CreateException(string.Empty, Constants.Errors.PaymentRequestNotFound);
                Db.Entry(request).Reload();
                client = CacheManager.GetClientById(request.ClientId);
                response = new ChangeWithdrawRequestStateOutput
                {
                    ClientId = request.ClientId,
                    CurrencyId = request.CurrencyId,
                    RequestId = request.Id,
                    RequestAmount = request.Amount,
                    CommissionAmount = request.CommissionAmount ?? 0,
                    ClientDocumentNumber = client.DocumentNumber,
                    ClientUserName = client.UserName,
                    PartnerId = client.PartnerId,
                    PartnerPaymentSettingId = request.PartnerPaymentSettingId,
                    PaymentSystemId = request.PaymentSystemId,
                    Info = request.Info,
                    ExternalTransactionId = request.ExternalTransactionId,
                    Status = (int)state,
                    BetShop = request.BetShop == null ? string.Empty : request.BetShop.Name,
                    BetShopAddress = request.BetShop == null ? string.Empty : request.BetShop.Address,
                    CashCode = request.CashCode
                };
                if (state == PaymentRequestStates.PayPanding && Constants.PaymentRequestFinalStates.Contains(request.Status))
                {
                    response.Status = request.Status;
                    return response;
                }

                if (Constants.PaymentRequestFinalStates.Contains(request.Status) ||
                (checkPermission && request.Status == (int)PaymentRequestStates.PayPanding &&
                request.PaymentSystem.Name != Constants.PaymentSystems.PayOne))
                    throw CreateException(LanguageId, Constants.Errors.CanNotChangePaymentRequestStatus);

                if (state == PaymentRequestStates.Approved && request.Status != (int)PaymentRequestStates.PayPanding &&
                    request.Status != (int)PaymentRequestStates.Confirmed)
                    throw BaseBll.CreateException(LanguageId, Constants.Errors.CanNotChangePaymentRequestStatus);
                if ((request.Status == (int)PaymentRequestStates.InProcess ||
                     request.Status == (int)PaymentRequestStates.Frozen ||
                     request.Status == (int)PaymentRequestStates.WaitingForKYC ||
                     request.Status == (int)PaymentRequestStates.PayPanding ||
                     request.Status == (int)PaymentRequestStates.Confirmed) &&
                    state == PaymentRequestStates.CanceledByClient)
                    throw CreateException(LanguageId, Constants.Errors.CanNotCancelRequestInCheckingState);

                if (state == PaymentRequestStates.Confirmed)
                {
                    if (checkPermission)
                        CheckPermission(Constants.Permissions.AllowPaymentRequest);

                    if (!(request.Status == (int)PaymentRequestStates.InProcess ||
                          request.Status == (int)PaymentRequestStates.Frozen ||
                          request.Status == (int)PaymentRequestStates.WaitingForKYC ||
                          request.Status == (int)PaymentRequestStates.Pending))
                        throw CreateException(LanguageId, Constants.Errors.CanNotAllowRequestFromThisState);
                }
                else if (state == PaymentRequestStates.CanceledByUser)
                {
                    CheckPermission(Constants.Permissions.CancelPaymentRequest);
                    if (request.Status == (int)PaymentRequestStates.Confirmed)
                        CheckPermission(Constants.Permissions.CancelPaymentRequestFromConfirmed);
                    if (request.Status == (int)PaymentRequestStates.PayPanding)
                        CheckPermission(Constants.Permissions.CancelPaymentRequestFromPayPending);
                }
                if (state == PaymentRequestStates.CanceledByClient || state == PaymentRequestStates.CanceledByUser ||
                    ((request.Status == (int)PaymentRequestStates.PayPanding ||
                    request.Status == (int)PaymentRequestStates.Confirmed) && state == PaymentRequestStates.Failed))
                {
                    var document = documentBl.GetDocuments(new FilterDocument
                    {
                        PaymentRequestId = requestId,
                        OperationTypeId = (int)OperationTypes.PaymentRequestBooking,
                        State = (int)BetDocumentStates.Uncalculated
                    }).FirstOrDefault();
                    if (document != null)
                        documentBl.DeleteDocument(document);
                }
                var history = new PaymentRequestHistory
                {
                    Comment = comment,
                    CreationTime = currentDate,
                    RequestId = request.Id,
                    SessionId = Identity.IsAdminUser ? Identity.SessionId : (long?)null,
                    Status = (int)state
                };
                Db.PaymentRequestHistories.Add(history);
                request.Status = (int)state;
                request.LastUpdateTime = currentDate;
                request.Parameters = parameters;
                Db.SaveChanges();
                requestParams = request.Parameters;
                scope.Complete();
            }
            notificationBl.SendWitdrawNotification(response.ClientId, response.Status, response.RequestAmount, comment);
            if (response.Status == (int)PaymentRequestStates.Approved ||
            response.Status == (int)PaymentRequestStates.ApprovedManually ||
            response.Status == (int)PaymentRequestStates.PayPanding ||
            response.Status == (int)PaymentRequestStates.CanceledByUser)
            {
                var languageId = !string.IsNullOrEmpty(Identity.LanguageId) ? Identity.LanguageId : Constants.DefaultLanguageId;
                var clientSession = Db.ClientSessions.Where(x => x.ClientId == client.Id && x.ProductId == Constants.PlatformProductId).
                    OrderByDescending(x => x.Id).FirstOrDefault();
                if (clientSession != null)
                    languageId = clientSession.LanguageId;
                if ((response.Status == (int)PaymentRequestStates.Approved || response.Status == (int)PaymentRequestStates.ApprovedManually) &&
                     !string.IsNullOrEmpty(requestParams))
                {
                    var par = JsonConvert.DeserializeObject<Dictionary<string, string>>(requestParams);
                    if (par.ContainsKey("SavePaymentDetails"))
                    {
                        var savePaymentDetails = par["SavePaymentDetails"];
                        if (Convert.ToBoolean(savePaymentDetails))
                        {
                            var paymentInfo = JsonConvert.DeserializeObject<Common.Models.PaymentInfo>(response.Info);
                            var infoType = (int)ClientPaymentInfoTypes.BankAccount;
                            if (!string.IsNullOrEmpty(paymentInfo.CardNumber))
                                infoType = (int)ClientPaymentInfoTypes.CreditCard;
                            else if (!string.IsNullOrEmpty(paymentInfo.WalletNumber))
                                infoType = (int)ClientPaymentInfoTypes.Wallet;
                            var clientPaymentInfo = new ClientPaymentInfo
                            {
                                Type = infoType,
                                CardNumber = paymentInfo.CardNumber,
                                ClientFullName = paymentInfo.BankAccountHolder,
                                WalletNumber = paymentInfo.WalletNumber,
                                BankName = paymentInfo.BankName,
                                BranchName = paymentInfo.BankBranchName,
                                BankAccountNumber = paymentInfo.BankAccountNumber,
                                BankIBAN = paymentInfo.BankIBAN,
                                PartnerPaymentSystemId = response.PaymentSystemId,
                                CreationTime = currentDate,
                                LastUpdateTime = currentDate,
                                ClientId = response.ClientId,
                                AccountNickName = CacheManager.GetPaymentSystemById(response.PaymentSystemId).Name
                            };
                            RegisterClientPaymentAccountDetails(clientPaymentInfo, null, false);
                        }
                    }
                }
                if (sendEmail)
                {
                    try
                    {
                        notificationBl.SendNotificationMessage(new NotificationModel
                        {
                            PartnerId = client.PartnerId,
                            ClientId = client.Id,
                            MobileOrEmail = client.Email,
                            ClientInfoType = response.Status == (int)PaymentRequestStates.CanceledByUser ?
                            (int)ClientInfoTypes.RejectWithdrawEmail : (int)ClientInfoTypes.ApproveWithdrawEmail,
                            Parameters = requestParams + ",amount:" + response.RequestAmount.ToString("0.00"),
                            LanguageId = languageId
                        });
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                    }
                }

                var rejectSubject = CacheManager.GetPartnerMessageTemplate(client.PartnerId, (int)ClientInfoTypes.RejectWithdrawEmailSubject, languageId);

                if (response.Status == (int)PaymentRequestStates.CanceledByUser && rejectSubject != null && !string.IsNullOrEmpty(comment))
                {
                    var ticket = new Ticket
                    {
                        PartnerId = client.PartnerId,
                        Type = (int)TicketTypes.Discussion,
                        Subject = rejectSubject == null ? string.Empty : rejectSubject.Text,
                        Status = (int)MessageTicketState.Closed,
                        LastMessageUserId = Identity.Id
                    };
                    var message = new TicketMessage
                    {
                        Message = comment,
                        Type = (int)ClientMessageTypes.MessageFromUser,
                        UserId = Identity.Id
                    };
                    OpenTickets(ticket, message, new List<int> { client.Id }, false);
                }
                if (cashDeskId != null && cashDeskId != 0)
                {
                    var cashDesk = CacheManager.GetCashDeskById(cashDeskId.Value);
                    var betShop = Db.BetShops.First(x => x.Id == cashDesk.BetShopId);
                    response.CashierBalance = GetObjectBalanceWithConvertion((int)ObjectTypes.CashDesk, cashDeskId.Value, betShop.CurrencyId).AvailableBalance;
                    response.ClientBalance = GetObjectBalanceWithConvertion((int)ObjectTypes.Client, client.Id, betShop.CurrencyId).AvailableBalance;
                    response.ObjectLimit = betShop.CurrentLimit;
                }
            }
            return response;
        }

        // pay payment request from BetShop
        public PayWithdrawFromBetShopOutput PayWithdrawFromBetShop(ChangeWithdrawRequestStateOutput resp, int cashDeskId, int? cashierId, DocumentBll documentBl)
        {
            CheckPermission(Constants.Permissions.PayWithdrawFromBetShop);

            var cashDesk = CacheManager.GetCashDeskById(cashDeskId);
            if (cashDesk == null)
                throw CreateException(LanguageId, Constants.Errors.CashDeskNotFound);
            Db.Procedures.sp_GetBetShopLockAsync(cashDesk.BetShopId).Wait();
            var betShop = Db.BetShops.FirstOrDefault(x => x.Id == cashDesk.BetShopId);
            if (betShop == null)
                throw CreateException(LanguageId, Constants.Errors.BetShopNotFound);
            var amount = Math.Floor((resp.RequestAmount - resp.CommissionAmount) * 100) / 100;
            betShop.CurrentLimit += ConvertCurrency(resp.CurrencyId, betShop.CurrencyId, amount);

            var operation = new Operation
            {
                Amount = resp.RequestAmount,
                UserId = cashierId,
                CurrencyId = resp.CurrencyId,
                Type = (int)OperationTypes.ClientTransferToBetShop,
                CashDeskId = cashDeskId,
                ClientId = resp.ClientId,
                PaymentRequestId = resp.RequestId,
                Info = string.Empty,
                OperationItems = new List<OperationItem>()
            };
            var item = new OperationItem
            {
                ObjectId = resp.ClientId,
                ObjectTypeId = (int)ObjectTypes.Client,
                AccountTypeId = (int)AccountTypes.ClientBooking,
                Amount = resp.RequestAmount,
                CurrencyId = resp.CurrencyId,
                Type = (int)TransactionTypes.Credit,
                OperationTypeId = (int)OperationTypes.ClientTransferToBetShop
            };
            operation.OperationItems.Add(item);
            item = new OperationItem
            {
                AccountTypeId = (int)AccountTypes.PartnerBalance,
                ObjectId = resp.PartnerId,
                ObjectTypeId = (int)ObjectTypes.Partner,
                Amount = resp.RequestAmount,
                CurrencyId = resp.CurrencyId,
                Type = (int)TransactionTypes.Debit,
                OperationTypeId = (int)OperationTypes.ClientTransferToBetShop
            };
            operation.OperationItems.Add(item);
            item = new OperationItem
            {
                ObjectId = cashDeskId,
                ObjectTypeId = (int)ObjectTypes.CashDesk,
                AccountTypeId = (int)AccountTypes.BetShopDebtToPartner,
                Amount = amount,
                CurrencyId = resp.CurrencyId,
                Type = (int)TransactionTypes.Credit,
                OperationTypeId = (int)OperationTypes.ClientTransferToBetShop
            };
            operation.OperationItems.Add(item);
            item = new OperationItem
            {
                ObjectId = Constants.MainExternalClientId,
                ObjectTypeId = (int)ObjectTypes.Client,
                AccountTypeId = (int)AccountTypes.BetShopDebtToPartner,
                Amount = amount,
                CurrencyId = resp.CurrencyId,
                Type = (int)TransactionTypes.Debit,
                OperationTypeId = (int)OperationTypes.ClientTransferToBetShop
            };
            operation.OperationItems.Add(item);
            documentBl.CreateDocument(operation);
            var pr = Db.PaymentRequests.First(x => x.Id == resp.RequestId);
            pr.CashierId = cashierId;
            pr.CashDeskId = cashDeskId;
            Db.SaveChanges();
            CacheManager.RemoveClientBalance(resp.ClientId);
            return new PayWithdrawFromBetShopOutput
            {
                ObjectLimit = betShop.CurrentLimit,
                ObjectBalance = GetObjectBalanceWithConvertion(
                    (int)ObjectTypes.CashDesk,
                    cashDeskId,
                    betShop.CurrencyId).AvailableBalance
            };
        }
        public Document PayWithdrawFromPaymentSystem(ChangeWithdrawRequestStateOutput resp, DocumentBll documentBl, NotificationBll notificationBl, MerchantRequest mr = null)
        {
            var client = CacheManager.GetClientById(resp.ClientId);
            if (client == null)
                throw CreateException(LanguageId, Constants.Errors.ClientNotFound);

            if (!resp.PartnerPaymentSettingId.HasValue)
                throw CreateException(LanguageId, Constants.Errors.PartnerPaymentSettingNotFound);

            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, resp.PaymentSystemId, resp.CurrencyId, (int)PaymentRequestTypes.Withdraw);
            if (partnerPaymentSetting == null)
                throw CreateException(LanguageId, Constants.Errors.PartnerPaymentSettingNotFound);
            var commission = resp.RequestAmount * partnerPaymentSetting.Commission / 100 + partnerPaymentSetting.FixedFee;

            var operation = new Operation
            {
                Amount = resp.RequestAmount,
                CurrencyId = resp.CurrencyId,
                Type = (int)OperationTypes.TransferFromClientToPaymentSystem,
                ExternalTransactionId = resp.ExternalTransactionId,
                PartnerPaymentSettingId = resp.PartnerPaymentSettingId,
                ClientId = resp.ClientId,
                Info = resp.Info,
                OperationItems = new List<OperationItem>()
            };

            var item = new OperationItem
            {
                ObjectId = client.Id,
                ObjectTypeId = (int)ObjectTypes.Client,
                AccountTypeId = (int)AccountTypes.ClientBooking,
                Amount = resp.RequestAmount - commission,
                CurrencyId = resp.CurrencyId,
                Type = (int)TransactionTypes.Credit,
                OperationTypeId = (int)OperationTypes.TransferFromClientToPaymentSystem
            };
            operation.OperationItems.Add(item);
            if (commission > 0)
            {
                item = new OperationItem
                {
                    ObjectId = client.Id,
                    ObjectTypeId = (int)ObjectTypes.Client,
                    AccountTypeId = (int)AccountTypes.ClientBooking,
                    Amount = commission,
                    CurrencyId = resp.CurrencyId,
                    Type = (int)TransactionTypes.Credit,
                    OperationTypeId = (int)OperationTypes.TransferFromClientToPaymentSystem
                };
                operation.OperationItems.Add(item);

                item = new OperationItem
                {
                    AccountTypeId = (int)AccountTypes.PartnerBalance,
                    ObjectId = client.PartnerId,
                    ObjectTypeId = (int)ObjectTypes.Partner,
                    Amount = commission,
                    CurrencyId = resp.CurrencyId,
                    Type = (int)TransactionTypes.Debit,
                    OperationTypeId = (int)OperationTypes.TransferFromClientToPaymentSystem
                };
                operation.OperationItems.Add(item);
            }

            item = new OperationItem
            {
                AccountTypeId = (int)AccountTypes.ClientUsedBalance,
                ObjectId = Constants.MainExternalClientId,
                ObjectTypeId = (int)ObjectTypes.Client,
                Amount = resp.RequestAmount - commission,
                CurrencyId = resp.CurrencyId,
                Type = (int)TransactionTypes.Debit,
                OperationTypeId = (int)OperationTypes.TransferFromClientToPaymentSystem
            };
            operation.OperationItems.Add(item);

            var document = documentBl.CreateDocument(operation);
            Db.SaveChanges();
            CacheManager.RemoveClientBalance(client.Id);
            if (mr != null)
            {
                Db.MerchantRequests.Add(mr);
                Db.SaveChanges();
            }
            notificationBl.WithdrawAffiliateNotification(client, resp.RequestAmount - resp.CommissionAmount, resp.RequestId);
            return document;
        }

        private void CheckWithdrawLimits(PaymentLimit paymentLimit, DateTime currentTime, decimal withdrawAmount)
        {
            var lastDayStartTime = currentTime.AddDays(-1);
            var lastWeekStartTime = currentTime.AddDays(-7);
            var lastMonthStartTime = currentTime.AddMonths(-1);

            var states = new List<int>
            {
                (int)PaymentRequestStates.CanceledByUser,
                (int)PaymentRequestStates.CanceledByClient,
                (int)PaymentRequestStates.Failed
            };
            if (paymentLimit.MaxWithdrawAmount.HasValue && paymentLimit.MaxWithdrawAmount.Value < withdrawAmount)
                throw CreateException(LanguageId, Constants.Errors.ClientMaxLimitExceeded);

            var maxTotalWithdrawsAmountPerDay = (from pr in Db.PaymentRequests where pr.ClientId == paymentLimit.ClientId && pr.Type == (int)PaymentRequestTypes.Withdraw && !states.Contains(pr.Status) && pr.CreationTime >= lastDayStartTime select (decimal?)pr.Amount).Sum() ?? 0;
            maxTotalWithdrawsAmountPerDay += withdrawAmount;

            if (paymentLimit.MaxTotalWithdrawsAmountPerDay.HasValue && paymentLimit.MaxTotalWithdrawsAmountPerDay.Value < maxTotalWithdrawsAmountPerDay)
                throw CreateException(LanguageId, Constants.Errors.ClientMaxLimitExceeded);

            var maxTotalWithdrawsAmountPerWeek = (from pr in Db.PaymentRequests where pr.ClientId == paymentLimit.ClientId && pr.Type == (int)PaymentRequestTypes.Withdraw && !states.Contains(pr.Status) && pr.CreationTime >= lastWeekStartTime select (decimal?)pr.Amount).Sum() ?? 0;
            maxTotalWithdrawsAmountPerWeek += withdrawAmount;

            if (paymentLimit.MaxTotalWithdrawsAmountPerWeek.HasValue && paymentLimit.MaxTotalWithdrawsAmountPerWeek.Value < maxTotalWithdrawsAmountPerWeek)
                throw CreateException(LanguageId, Constants.Errors.ClientMaxLimitExceeded);

            var maxTotalWithdrawsAmountPerMonth = (from pr in Db.PaymentRequests where pr.ClientId == paymentLimit.ClientId && pr.Type == (int)PaymentRequestTypes.Withdraw && !states.Contains(pr.Status) && pr.CreationTime >= lastMonthStartTime select (decimal?)pr.Amount).Sum() ?? 0;
            maxTotalWithdrawsAmountPerMonth += withdrawAmount;

            if (paymentLimit.MaxTotalWithdrawsAmountPerMonth.HasValue && paymentLimit.MaxTotalWithdrawsAmountPerMonth.Value < maxTotalWithdrawsAmountPerMonth)
                throw CreateException(LanguageId, Constants.Errors.ClientMaxLimitExceeded);
        }

        #endregion

        #region Deposit

        public PaymentRequest UploadPaymentForm(PaymentRequest paymentRequest, string paymentForm, string imageName)
        {
            var client = CacheManager.GetClientById(paymentRequest.ClientId);
            CheckPermission(Constants.Permissions.CreateDepositFromPaymentSystem);
            var checkClientPermission = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewClient,
                ObjectTypeId = (int)ObjectTypes.Client
            });

            var checkPartnerPermission = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });

            var affiliateReferralAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewAffiliateReferral,
                ObjectTypeId = (int)ObjectTypes.AffiliateReferral
            });

            if ((!checkClientPermission.HaveAccessForAllObjects && checkClientPermission.AccessibleObjects.All(x => x != client.Id)) ||
                 (!checkPartnerPermission.HaveAccessForAllObjects && checkPartnerPermission.AccessibleObjects.All(x => x != client.PartnerId)) ||
                 (!affiliateReferralAccess.HaveAccessForAllObjects &&
                 affiliateReferralAccess.AccessibleObjects.All(x => client.AffiliateReferralId.HasValue && x != client.AffiliateReferralId.Value))
               )
                throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            if (paymentRequest.Type == (int)PaymentRequestTypes.Deposit)
            {
                if (!string.IsNullOrEmpty(paymentForm))
                {
                    string[] paths = { Path.GetDirectoryName(WebHostEnvironment.ContentRootPath), "AdminWebApi", "ClientPaymentForms" };
                    var localPath = Path.Combine(paths);
                    var imgName = CommonFunctions.UploadImage(paymentRequest.ClientId, paymentForm, imageName, localPath);
                    var dic = new Dictionary<string, string> { { "PaymentForm", imgName } };
                    paymentRequest.Parameters = JsonConvert.SerializeObject(dic);
                }
                return CreateDepositFromPaymentSystem(paymentRequest);
            }
            else
            {
                using (var documentBl = new DocumentBll(this))
                {
                    using (var notificationBl = new NotificationBll(this))
                    {
                        var partner = CacheManager.GetPartnerById(client.PartnerId);
                        var clientSetting = CacheManager.GetClientSettingByName(client.Id, Constants.ClientSettings.UnusedAmountWithdrawPercent);
                        var uawp = partner.UnusedAmountWithdrawPercent;
                        if (clientSetting != null && clientSetting.Id > 0 && clientSetting.NumericValue != null)
                            uawp = clientSetting.NumericValue.Value;

                        var requestModel = new PaymentRequestModel
                        {
                            Amount = paymentRequest.Amount,
                            ClientId = paymentRequest.ClientId,
                            CurrencyId = client.CurrencyId,
                            Info = paymentRequest.Info,
                            PaymentSystemId = paymentRequest.PaymentSystemId,
                            PartnerId = client.PartnerId,
                            Type = (int)PaymentRequestTypes.Withdraw
                        };
                        var request = CreateWithdrawPaymentRequest(requestModel, uawp, client, documentBl, notificationBl);
                        var paymentSystem = CacheManager.GetPaymentSystemById(request.PaymentSystemId);

                        var partnerAutoConfirmWithdrawMaxAmount = ConvertCurrency(partner.CurrencyId, client.CurrencyId, partner.AutoConfirmWithdrawMaxAmount);

                        if ((partnerAutoConfirmWithdrawMaxAmount > request.Amount && client.IsDocumentVerified) || client.Email == Constants.CardReaderClientEmail ||
                            paymentSystem.Name.ToLower() == Constants.PaymentSystems.IqWallet.ToLower())
                        {
                            ChangeWithdrawRequestState(request.Id, PaymentRequestStates.Confirmed, "", request.CashDeskId, null, false, string.Empty, documentBl, notificationBl);
                        }
                        return request;
                    }
                }
            }
        }

        public PaymentRequest CreateDepositFromPaymentSystem(PaymentRequest request)
        {
            var client = CacheManager.GetClientById(request.ClientId);
            if (client == null)
                throw CreateException(LanguageId, Constants.Errors.ClientNotFound);
            if (client.State == (int)ClientStates.FullBlocked || client.State == (int)ClientStates.Disabled
                || client.State == (int)ClientStates.Suspended || client.State == (int)ClientStates.BlockedForDeposit)
                throw CreateException(LanguageId, Constants.Errors.ClientBlocked);
            if (request.CurrencyId != client.CurrencyId)
                throw CreateException(LanguageId, Constants.Errors.WrongCurrencyId);
            if (!request.PartnerPaymentSettingId.HasValue)
                throw CreateException(LanguageId, Constants.Errors.PartnerPaymentSettingNotFound);
            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, request.PaymentSystemId, request.CurrencyId, (int)PaymentRequestTypes.Deposit);
            if (partnerPaymentSetting == null)
                throw CreateException(LanguageId, Constants.Errors.PartnerPaymentSettingNotFound);
            if (partnerPaymentSetting.State == (int)PartnerPaymentSettingStates.Inactive)
                throw CreateException(LanguageId, Constants.Errors.PartnerPaymentSettingBlocked);
            if (client.CurrencyId != partnerPaymentSetting.CurrencyId)
                throw CreateException(LanguageId, Constants.Errors.ImpermissiblePaymentSetting);
            var isVoucherPayment = !string.IsNullOrEmpty(request.PaymentSystemName) && Constants.VoucherPaymentSystems.Contains(request.PaymentSystemName);
            if (!isVoucherPayment && (request.Amount < partnerPaymentSetting.MinAmount || request.Amount > partnerPaymentSetting.MaxAmount))
                throw CreateException(LanguageId, Constants.Errors.PaymentRequestInValidAmount);
            var commissionAmount = request.Amount* partnerPaymentSetting.Commission/100 + partnerPaymentSetting.FixedFee;
            if (request.Amount - commissionAmount < 0)
                throw CreateException(LanguageId, Constants.Errors.PaymentRequestInValidAmount);

            var clientPaymentSetting = Db.ClientPaymentSettings.FirstOrDefault(x => x.ClientId == request.ClientId &&
                                                                                    x.PartnerPaymentSettingId == partnerPaymentSetting.Id &&
                                                                                    x.State == (int)ClientPaymentStates.Blocked);
            if (clientPaymentSetting != null)
                throw CreateException(LanguageId, Constants.Errors.PartnerPaymentSettingBlocked);

            if (CacheManager.GetConfigKey(client.PartnerId, Constants.PartnerKeys.AMLVerification) == "1")
            {
                var amlVerified = CacheManager.GetClientSettingByName(client.Id, ClientSettings.AMLVerified);
                if (amlVerified != null && amlVerified.Id > 0 && amlVerified.StringValue == "1")
                {
                    var amlStatus = CacheManager.GetClientSettingByName(client.Id, ClientSettings.AMLProhibited);
                    if (amlStatus != null && amlStatus.Id > 0 && amlStatus.StringValue == "2")
                        throw BaseBll.CreateException(LanguageId, Constants.Errors.AMLProhibited);
                }
            }

            var documentSetting = CacheManager.GetConfigKey(client.PartnerId, Constants.PartnerKeys.RequireDocumentForDeposit);
            if (documentSetting == "1")
            {
                if (!client.IsDocumentVerified)
                    throw CreateException(LanguageId, Constants.Errors.DocumentNotVerified);

                var documents = GetClientIdentities(client.Id);
                var documentExpired = (!documents.Any(x => x.Status == (int)KYCDocumentStates.Approved) && documents.Any(x => x.Status == (int)KYCDocumentStates.Expired));
                if (documentExpired)
                    throw CreateException(LanguageId, Constants.Errors.DocumentExpired);
            }
            var suspensionSetting = CacheManager.GetClientSettingByName(client.Id, Constants.ClientSettings.CautionSuspension);
            if (suspensionSetting != null && suspensionSetting.Id > 0 && suspensionSetting.StringValue == "1")
                throw CreateException(LanguageId, Constants.Errors.CautionSuspension);

            var currentTime = GetServerDate();
            if (!isVoucherPayment)
            {
                CheckClientDepositLimits(request.ClientId, request.Amount);
                //var paymentLimit = Db.PaymentLimits.FirstOrDefault(x => x.ClientId == client.Id);
                //if (paymentLimit != null)
                //    CheckDepositLimits(paymentLimit, currentTime, request.Amount); 
            }
            if (!string.IsNullOrEmpty(request.ExternalTransactionId))
            {
                var dbPaymentRequest = Db.PaymentRequests.FirstOrDefault(x => x.PaymentSystemId == request.PaymentSystemId &&
                    x.Type == (int)PaymentRequestTypes.Deposit && x.ExternalTransactionId == request.ExternalTransactionId);
                if (dbPaymentRequest != null)
                    return dbPaymentRequest;
            }

            request.CommissionAmount = commissionAmount;
            request.PaymentSystemId = partnerPaymentSetting.PaymentSystemId;
            request.LastUpdateTime = currentTime;
            request.CreationTime = currentTime;
            request.SessionId = Identity.SessionId;
            request.Type = (int)PaymentRequestTypes.Deposit;
            request.Status = (int)PaymentRequestStates.Pending;
            var date = (long)currentTime.Year * 100000000 + (long)currentTime.Month * 1000000 + (long)currentTime.Day * 10000 +
           (long)currentTime.Hour * 100 + currentTime.Minute;
            request.Date = date;

            if (!string.IsNullOrEmpty(request.Info))
            {
                var info = JsonConvert.DeserializeObject<Common.Models.PaymentInfo>(request.Info);
                /*var classification = Db.ClientClassifications.FirstOrDefault(x => x.ClientId == client.Id &&
                                                                  x.Segment.PaymentSystemId == request.PaymentSystemId);
                if (classification != null)
                    request.SegmentId = classification.SegmentId;
                else
                {
                    var defaultSegment = Db.SegmentSettings.FirstOrDefault(x => x.Name == Constants.SegmentSettings.IsDefault &&
                                                                                x.Segment.PartnerId == client.PartnerId &&
                                                                               (x.Segment.State == (int)PartnerPaymentSettingStates.Active ||
                                                                                x.Segment.State == (int)PartnerPaymentSettingStates.Hidden) &&
                                                                                x.Segment.PaymentSystemId == request.PaymentSystemId);
                    if (defaultSegment != null)
                        request.SegmentId = defaultSegment.Id;
                }*/
                request.Info = JsonConvert.SerializeObject(info, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Ignore
                });
                request.CardNumber = info?.CardNumber;
            }
            else
                request.Info = "{}";
            Db.PaymentRequests.Add(request);
            Db.SaveChanges();
            return request;
        }

        public Document ApproveDepositFromPaymentSystem(PaymentRequest request, bool fromAdmin, string comment = "", ClientPaymentInfo info = null, MerchantRequest mr = null)
        {
            if (fromAdmin)
            {
                CheckPermission(Constants.Permissions.CreateDepositFromPaymentSystem);
            }
            using var documentBl = new DocumentBll(this);
            using var paymentSystemBl = new PaymentSystemBll(this);
            using var notificationBl = new NotificationBll(this);
            using var bonusService = new BonusService(this);
            var currentTime = DateTime.UtcNow;
            var client = CacheManager.GetClientById(request.ClientId);
            if (client == null)
                throw CreateException(LanguageId, Constants.Errors.ClientNotFound);
            if (info == null && !fromAdmin)
            {
                var partnerSetting = CacheManager.GetConfigKey(client.PartnerId, Constants.PartnerKeys.PaymentDetailsValidation);
                if (!string.IsNullOrEmpty(partnerSetting) && partnerSetting == "1")
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.NotAllowed);
            }

            var clientState = client.State;
            if (clientState == (int)ClientStates.FullBlocked || client.State == (int)ClientStates.Disabled)
                throw BaseBll.CreateException(string.Empty, Constants.Errors.ClientBlocked);
            if (!request.PartnerPaymentSettingId.HasValue)
                throw CreateException(LanguageId, Constants.Errors.PartnerPaymentSettingNotFound);
            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, request.PaymentSystemId, request.CurrencyId, (int)PaymentRequestTypes.Deposit);
            if (partnerPaymentSetting == null)
                throw CreateException(LanguageId, Constants.Errors.PartnerPaymentSettingNotFound);
            if (partnerPaymentSetting.State == (int)PartnerPaymentSettingStates.Inactive)
                throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerPaymentSettingBlocked);
            var paymentRequest = paymentSystemBl.GetPaymentRequestById(request.Id);
            if (paymentRequest == null || paymentRequest.Type != (int)PaymentRequestTypes.Deposit)
                throw CreateException(LanguageId, Constants.Errors.PaymentRequestNotFound);
            if (paymentRequest.Status == (int)PaymentRequestStates.Approved || paymentRequest.Status == (int)PaymentRequestStates.ApprovedManually)
                throw CreateException(LanguageId, Constants.Errors.RequestAlreadyPayed, dateTimeInfo: paymentRequest.CreationTime);
            var pureAmount = request.Amount;
            if (partnerPaymentSetting.Commission > 0)
                pureAmount -= request.Amount * partnerPaymentSetting.Commission / 100;
            if (partnerPaymentSetting.FixedFee > 0)
                pureAmount -= partnerPaymentSetting.FixedFee;
            if (pureAmount < 0)
                throw CreateException(LanguageId, Constants.Errors.LowBalance);
            paymentRequest.CommissionAmount = request.Amount - pureAmount;
            var document = documentBl.GetDocumentFromDb(partnerPaymentSetting.Id, request.ExternalTransactionId, (int)OperationTypes.TransferFromPaymentSystemToClient);
            if (document == null)
            {
                var operation = new Operation
                {
                    Amount = request.Amount,
                    CurrencyId = request.CurrencyId,
                    Type = (int)OperationTypes.TransferFromPaymentSystemToClient,
                    PaymentRequestId = request.Id,
                    ExternalTransactionId = request.ExternalTransactionId,
                    PartnerPaymentSettingId = request.PartnerPaymentSettingId,
                    ClientId = request.ClientId,
                    Info = request.Info,
                    ExternalOperationId = null,
                    OperationItems = new List<OperationItem>()
                };
                var item = new OperationItem
                {
                    AccountTypeId = (int)AccountTypes.ExternalClientsAccount,
                    ObjectId = Constants.MainExternalClientId,
                    ObjectTypeId = (int)ObjectTypes.Client,
                    Amount = request.Amount,
                    CurrencyId = request.CurrencyId,
                    Type = (int)TransactionTypes.Credit,
                    OperationTypeId = (int)OperationTypes.TransferFromPaymentSystemToClient
                };
                operation.OperationItems.Add(item);
                if (paymentRequest.CommissionAmount > 0)
                {
                    item = new OperationItem
                    {
                        AccountTypeId = (int)AccountTypes.PartnerBalance,
                        ObjectId = client.PartnerId,
                        ObjectTypeId = (int)ObjectTypes.Partner,
                        Amount = paymentRequest.CommissionAmount.Value,
                        CurrencyId = request.CurrencyId,
                        Type = (int)TransactionTypes.Debit,
                        OperationTypeId = (int)OperationTypes.TransferFromPaymentSystemToClient
                    };
                    operation.OperationItems.Add(item);
                }
                item = new OperationItem
                {
                    ObjectId = client.Id,
                    ObjectTypeId = (int)ObjectTypes.Client,
                    AccountTypeId = (int)AccountTypes.ClientUnusedBalance,
                    Amount = pureAmount,
                    CurrencyId = request.CurrencyId,
                    Type = (int)TransactionTypes.Debit,
                    OperationTypeId = (int)OperationTypes.TransferFromPaymentSystemToClient
                };
                operation.OperationItems.Add(item);
                document = documentBl.CreateDocument(operation);
            }
            paymentRequest.Status = fromAdmin ? (int)PaymentRequestStates.ApprovedManually : (int)PaymentRequestStates.Approved;
            paymentRequest.LastUpdateTime = currentTime;
            if (fromAdmin)
            {
                var history = new PaymentRequestHistory
                {
                    Comment = comment,
                    CreationTime = currentTime,
                    RequestId = paymentRequest.Id,
                    SessionId = Identity.SessionId,
                    Status = paymentRequest.Status
                };
                Db.PaymentRequestHistories.Add(history);
            }
            AddClientJobTrigger(request.ClientId, (int)JobTriggerTypes.ReconsiderSegments);
            Db.SaveChanges();
            CacheManager.RemoveClientBalance(client.Id);
            if (mr != null)
            {
                Db.MerchantRequests.Add(mr);
                Db.SaveChanges();
            }
            CacheManager.RemoveClientDepositCount(request.ClientId);

            var depCount = CacheManager.GetClientDepositCount(request.ClientId);
            var clientBonuses = CacheManager.GetClientNotAwardedCampaigns(client.Id);

            int awardedStatus = 0;
            if (!string.IsNullOrEmpty(request.Info))
            {
                var rInfo = JsonConvert.DeserializeObject<PaymentRequestInfo>(request.Info);
                if (!string.IsNullOrEmpty(rInfo.PromoCode))
                {
                    AutoClaim(bonusService, client.Id, (int)TriggerTypes.PromotionalCode, rInfo.PromoCode, request.Amount, out awardedStatus, depCount, request.PaymentSystemId);
                }
            }

                            var oldBonuses = AutoClaim(bonusService, client.Id, (int)TriggerTypes.NthDeposit, string.Empty, request.Amount, out awardedStatus, depCount, request.PaymentSystemId);
                            if (paymentRequest.ActivatedBonusType.HasValue)
                            {
                                var b = CacheManager.GetBonusById(paymentRequest.ActivatedBonusType.Value);
                                if (b != null && b.Id > 0)
                                {
                                    var bonusInfo = bonusService.GiveCompainToClient(new ClientBonusItem
                                    {
                                        ClientId = paymentRequest.ClientId,
                                        BonusId = paymentRequest.ActivatedBonusType.Value,
                                        ValidForAwarding = DateTime.UtcNow.AddHours(b.ValidForAwarding ?? 0),
                                        BonusType = b.BonusType
                                    }, out bool alreadyGiven);
                                    CacheManager.RemoveClientNotAwardedCampaigns(client.Id);
                                    FairClientBonusTrigger(new ClientTriggerInput
                                    {
                                        ClientId = client.Id,
                                        ClientBonuses = new List<ClientBonusInfo> { bonusInfo },
                                        TriggerType = (int)TriggerTypes.NthDeposit,
                                        SourceAmount = request.Amount,
                                        PaymentSystemId = request.PaymentSystemId,
                                        DepositsCount = depCount
                                    }, out bool alreadyAdded);
                                }
                            }

            foreach (var b in oldBonuses)
            {
                foreach (var s in b.TriggerGroups)
                {
                    foreach (var ts in s.TriggerGroupSettings)
                    {
                        var setting = CacheManager.GetTriggerSettingById(ts.SettingId);
                        if (setting.StartTime <= currentTime && setting.FinishTime > currentTime &&
                            ((setting.Type == (int)TriggerTypes.AnyDeposit && (setting.PaymentSystemIds == null || !setting.PaymentSystemIds.Any() || setting.PaymentSystemIds.Contains(request.PaymentSystemId))) ||
                            (setting.Type == (int)TriggerTypes.NthDeposit && (setting.PaymentSystemIds == null || !setting.PaymentSystemIds.Any() || setting.PaymentSystemIds.Contains(request.PaymentSystemId)) &&
                            depCount.ToString() == setting.Condition)))
                        {
                            awardedStatus = 1;
                            CacheManager.RemoveClientNotAwardedCampaigns(client.Id);
                            FairClientBonusTrigger(new ClientTriggerInput
                            {
                                ClientId = client.Id,
                                ClientBonuses = new List<ClientBonusInfo> { new ClientBonusInfo { BonusId = b.Id, ReuseNumber = 1 } },
                                TriggerType = (int)TriggerTypes.NthDeposit,
                                SourceAmount = request.Amount,
                                PaymentSystemId = request.PaymentSystemId,
                                DepositsCount = depCount
                            }, out bool alreadyAdded);
                        }
                    }
                }
            }

            CacheManager.RemoveClientNotAwardedCampaigns(client.Id);
            ChangeClientDepositInfo(request.ClientId, depCount, request.Amount);
            CacheManager.RemoveTotalDepositAmount(request.ClientId);
            notificationBl.DepositAffiliateNotification(client, request.Amount , request.Id, depCount);
            notificationBl.SendDepositNotification(client.Id, request.Status, request.Amount - (request.CommissionAmount ?? 0), comment);
            return document;
        }

        // Transfer money from BetShop to client
        public PaymentRequest CreateDepositFromBetShop(PaymentRequest transaction)
        {
            var currentTime = DateTime.UtcNow;
            var date = (long)currentTime.Year * 1000000 + (long)currentTime.Month * 10000 + (long)currentTime.Day * 100 + (long)currentTime.Hour;

            CheckPermission(Constants.Permissions.CreateDepositFromBetShop);
            var client = CacheManager.GetClientById(transaction.ClientId);
            if (client == null)
                throw CreateException(LanguageId, Constants.Errors.ClientNotFound);
            if (!transaction.CashDeskId.HasValue)
                throw CreateException(LanguageId, Constants.Errors.CashDeskNotFound);
            var cashDesk = CacheManager.GetCashDeskById(transaction.CashDeskId.Value);
            if (cashDesk == null)
                throw CreateException(LanguageId, Constants.Errors.CashDeskNotFound);

            PaymentRequest paymentRequest;
            using var transactionScope = CommonFunctions.CreateTransactionScope();
            using var partnerBl = new PartnerBll(this);
            using var paymentSystemBl = new PaymentSystemBll(this);
            using var betShopBl = new BetShopBll(this);
            using var documentBl = new DocumentBll(this);
            using var notificationBl = new NotificationBll(this);
            Db.Procedures.sp_GetBetShopLockAsync(cashDesk.BetShopId).Wait();
            var betShop = betShopBl.GetBetShopById(cashDesk.BetShopId, false);
            if (betShop == null)
                throw CreateException(LanguageId, Constants.Errors.BetShopNotFound);

            if (betShop.CurrencyId != client.CurrencyId)
                throw CreateException(LanguageId, Constants.Errors.ImpermissibleBetShop);
            if (betShop.CurrencyId != transaction.CurrencyId)
                throw CreateException(LanguageId, Constants.Errors.WrongCurrencyId);
            if (betShop.PartnerId != client.PartnerId)
                throw CreateException(LanguageId, Constants.Errors.ImpermissibleBetShop);
            betShop.CurrentLimit -= transaction.Amount;
            if (betShop.CurrentLimit < 0)
                throw CreateException(LanguageId, Constants.Errors.BetShopLimitExceeded);

            var partner = partnerBl.GetPartnerById(betShop.PartnerId);
            if (partner == null)
                throw CreateException(LanguageId, Constants.Errors.PartnerNotFound);
            var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.BetShop);
            var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(client.PartnerId, paymentSystem.Id, transaction.CurrencyId, (int)PaymentRequestTypes.Deposit);

            if (partnerPaymentSetting == null)
                throw CreateException(LanguageId, Constants.Errors.PartnerPaymentSettingNotFound);

            if (partnerPaymentSetting.State == (int)PartnerPaymentSettingStates.Inactive)
                throw CreateException(LanguageId, Constants.Errors.PartnerPaymentSettingBlocked);

            if (CacheManager.GetConfigKey(client.PartnerId, Constants.PartnerKeys.AMLVerification) == "1")
            {
                var amlVerified = CacheManager.GetClientSettingByName(client.Id, ClientSettings.AMLVerified);
                if (amlVerified != null && amlVerified.Id > 0 && amlVerified.StringValue == "1")
                {
                    var amlStatus = CacheManager.GetClientSettingByName(client.Id, ClientSettings.AMLProhibited);
                    if (amlStatus != null && amlStatus.Id > 0 && amlStatus.StringValue == "2")
                        throw BaseBll.CreateException(LanguageId, Constants.Errors.AMLProhibited);
                }

                var jcjProhibited = CacheManager.GetClientSettingByName(client.Id, ClientSettings.JCJProhibited);
                if (jcjProhibited != null && jcjProhibited.Id > 0 && jcjProhibited.StringValue == "1")
                    throw BaseBll.CreateException(LanguageId, Constants.Errors.JCJExcluded);

                var blockedForInactivity = CacheManager.GetClientSettingByName(client.Id, ClientSettings.BlockedForInactivity);
                if (blockedForInactivity != null && blockedForInactivity.Id > 0 && blockedForInactivity.StringValue == "1")
                    throw BaseBll.CreateException(LanguageId, Constants.Errors.InactivityBlock);

                var cautionSuspension = CacheManager.GetClientSettingByName(client.Id, ClientSettings.CautionSuspension);
                if (cautionSuspension != null && cautionSuspension.Id > 0 && cautionSuspension.StringValue == "1")
                    throw BaseBll.CreateException(LanguageId, Constants.Errors.CautionSuspension);

                var selfExcluded = CacheManager.GetClientSettingByName(client.Id, ClientSettings.SelfExcluded);
                if (selfExcluded != null && selfExcluded.Id > 0 && selfExcluded.StringValue == "1")
                    throw BaseBll.CreateException(LanguageId, Constants.Errors.SelfExcluded);

                var systemExcluded = CacheManager.GetClientSettingByName(client.Id, ClientSettings.SystemExcluded);
                if (systemExcluded != null && systemExcluded.Id > 0 && systemExcluded.StringValue == "1")
                    throw BaseBll.CreateException(LanguageId, Constants.Errors.SystemExcluded);
            }

            var documentSetting = CacheManager.GetConfigKey(client.PartnerId, Constants.PartnerKeys.RequireDocumentForDeposit);
            if (documentSetting == "1")
            {
                if (!client.IsDocumentVerified)
                    throw CreateException(LanguageId, Constants.Errors.DocumentNotVerified);

                var documents = GetClientIdentities(client.Id);
                var documentExpired = (!documents.Any(x => x.Status == (int)KYCDocumentStates.Approved) && documents.Any(x => x.Status == (int)KYCDocumentStates.Expired));
                if (documentExpired)
                    throw CreateException(LanguageId, Constants.Errors.DocumentExpired);
            }

            var suspensionSetting = CacheManager.GetClientSettingByName(client.Id, Constants.ClientSettings.CautionSuspension);
            if (suspensionSetting != null && suspensionSetting.Id > 0 && suspensionSetting.StringValue == "1")
                throw CreateException(LanguageId, Constants.Errors.CautionSuspension);


            CheckClientDepositLimits(client.Id, transaction.Amount);
            paymentRequest = new PaymentRequest
            {
                Amount = transaction.Amount,
                ClientId = client.Id,
                CurrencyId = transaction.CurrencyId,
                BetShopId = betShop.Id,
                CashierId = transaction.CashierId,
                CashDeskId = transaction.CashDeskId,
                PaymentSystemId = paymentSystem.Id,
                PartnerPaymentSettingId = partnerPaymentSetting.Id,
                ExternalTransactionId = transaction.ExternalTransactionId,
                Status = (int)PaymentRequestStates.Pending,
                Type = (int)PaymentRequestTypes.Deposit,
                LastUpdateTime = currentTime,
                CreationTime = currentTime,
                SessionId = Identity.IsAdminUser ? Identity.SessionId : (long?)null,
                Date = date
            };
            Db.PaymentRequests.Add(paymentRequest);
            Db.SaveChanges();

            if (partner.AutoApproveBetShopDepositMaxAmount == 0)
            {
                return paymentRequest;
            }
            var partnerAutoApproveBetShopDepositMaxAmount = ConvertCurrency(partner.CurrencyId,
                transaction.CurrencyId, partner.AutoApproveBetShopDepositMaxAmount);
            if (partnerAutoApproveBetShopDepositMaxAmount > transaction.Amount)
            {
                ApproveDepositFromBetShop(paymentSystemBl, paymentRequest.Id, null, documentBl, notificationBl);
                paymentRequest.Status = (int)PaymentRequestStates.Approved;
            }

            paymentRequest.CashierBalance = GetObjectBalanceWithConvertion(
                (int)ObjectTypes.CashDesk, cashDesk.Id,
                betShop.CurrencyId).AvailableBalance;
            paymentRequest.ClientBalance = GetObjectBalanceWithConvertion(
                (int)ObjectTypes.Client, client.Id,
                betShop.CurrencyId).AvailableBalance;
            paymentRequest.ObjectLimit = betShop.CurrentLimit;
            transactionScope.Complete();

            return paymentRequest;
        }

        // Approve deposit payment request from BetShop
        public void ApproveDepositFromBetShop(IPaymentSystemBll paymentSystemBl, long requestId, string comment, DocumentBll documentBl, NotificationBll notificationBl)
        {
            CheckPermission(Constants.Permissions.PayDepositFromBetShop);
            using (var transactionScope = CommonFunctions.CreateTransactionScope())
            {
                ChangeDepositRequestState(requestId, PaymentRequestStates.Approved, comment, notificationBl);

                var paymentRequest = paymentSystemBl.GetPaymentRequestById(requestId);
                if (paymentRequest == null)
                    throw CreateException(LanguageId, Constants.Errors.PaymentRequestNotFound);


                if ((!paymentRequest.BetShopId.HasValue || !paymentRequest.CashDeskId.HasValue))
                    throw CreateException(LanguageId, Constants.Errors.WrongPaymentRequest);

                var client = CacheManager.GetClientById(paymentRequest.ClientId);

                var operation = new Operation
                {
                    Amount = paymentRequest.Amount,
                    CurrencyId = paymentRequest.CurrencyId,
                    ClientId = paymentRequest.ClientId,
                    Type = (int)OperationTypes.TransferFromBetShopToClient,
                    PartnerPaymentSettingId = paymentRequest.PartnerPaymentSettingId,
                    PaymentRequestId = paymentRequest.Id,
                    ExternalTransactionId = paymentRequest.Id.ToString(),
                    CashDeskId = paymentRequest.CashDeskId,
                    UserId = paymentRequest.CashierId,
                    OperationItems = new List<OperationItem>()
                };
                var item = new OperationItem
                {
                    AccountTypeId = (int)Common.Enums.AccountTypes.ExternalClientsAccount,
                    ObjectId = Constants.MainExternalClientId,
                    ObjectTypeId = (int)ObjectTypes.Client,
                    Amount = paymentRequest.Amount,
                    CurrencyId = paymentRequest.CurrencyId,
                    Type = (int)TransactionTypes.Credit,
                    OperationTypeId = (int)OperationTypes.TransferFromBetShopToClient
                };
                operation.OperationItems.Add(item);
                item = new OperationItem
                {
                    AccountTypeId = (int)Common.Enums.AccountTypes.BetShopDebtToPartner,
                    ObjectId = paymentRequest.CashDeskId.Value,
                    ObjectTypeId = (int)ObjectTypes.CashDesk,
                    Amount = paymentRequest.Amount,
                    CurrencyId = paymentRequest.CurrencyId,
                    Type = (int)TransactionTypes.Debit,
                    OperationTypeId = (int)OperationTypes.TransferFromBetShopToClient
                };
                operation.OperationItems.Add(item);
                item = new OperationItem
                {
                    AccountTypeId = (int)Common.Enums.AccountTypes.PartnerBalance,
                    ObjectId = client.PartnerId,
                    ObjectTypeId = (int)ObjectTypes.Partner,
                    Amount = paymentRequest.Amount,
                    CurrencyId = paymentRequest.CurrencyId,
                    Type = (int)TransactionTypes.Credit,
                    OperationTypeId = (int)OperationTypes.TransferFromBetShopToClient
                };
                operation.OperationItems.Add(item);
                item = new OperationItem
                {
                    ObjectId = paymentRequest.ClientId,
                    ObjectTypeId = (int)ObjectTypes.Client,
                    AccountTypeId = (int)Common.Enums.AccountTypes.ClientUnusedBalance,
                    Amount = paymentRequest.Amount,
                    CurrencyId = paymentRequest.CurrencyId,
                    Type = (int)TransactionTypes.Debit,
                    OperationTypeId = (int)OperationTypes.TransferFromBetShopToClient
                };
                operation.OperationItems.Add(item);
                documentBl.CreateDocument(operation);
                paymentRequest.Status = (int)PaymentRequestStates.Approved;
                Db.SaveChanges();
                transactionScope.Complete();
                CacheManager.RemoveClientBalance(client.Id);
                CacheManager.RemoveClientDepositCount(paymentRequest.ClientId);
            }
        }


        // Reject deposit payment request from BetShop
        public void CancelDeposit(long requestId, string comment)
        {
            using var transactionScope = CommonFunctions.CreateTransactionScope();
            using var betShopBl = new BetShopBll(this);
            using var paymentSystemBl = new PaymentSystemBll(this);
            using var notificationBl = new NotificationBll(paymentSystemBl);
            ChangeDepositRequestState(requestId, PaymentRequestStates.CanceledByUser, comment, notificationBl);
            var paymentRequest = paymentSystemBl.GetPaymentRequestById(requestId);
            if (paymentRequest == null)
                throw CreateException(LanguageId, Constants.Errors.PaymentRequestNotFound);
            if (paymentRequest.BetShopId.HasValue)
            {
                Db.Procedures.sp_GetBetShopLockAsync(paymentRequest.BetShopId).Wait();
                var betShop = betShopBl.GetBetShopById(paymentRequest.BetShopId.Value, false);
                if (betShop == null)
                    throw CreateException(LanguageId, Constants.Errors.BetShopNotFound);
                betShop.CurrentLimit += ConvertCurrency(paymentRequest.CurrencyId, betShop.CurrencyId,
                    paymentRequest.Amount);
                Db.SaveChanges();
            }
            transactionScope.Complete();
        }

        public void ChangeDepositRequestState(long requestId, PaymentRequestStates state, string comment,
            NotificationBll notificationBl, bool checkPermission = false)
        {
            if (checkPermission)
            {
                CheckPermission(Constants.Permissions.CreateDepositFromPaymentSystem);
            }
            using (var scope = CommonFunctions.CreateTransactionScope())
            {
                var request =
                Db.PaymentRequests.Include(x => x.PartnerPaymentSetting).FirstOrDefault(x => x.Id == requestId);
                if (request == null)
                    throw CreateException(LanguageId, Constants.Errors.PaymentRequestNotFound);
                var client = CacheManager.GetClientById(request.ClientId);
                if (client == null)
                    throw CreateException(LanguageId, Constants.Errors.ClientNotFound);
                var currentDate = GetServerDate();
                var history = new PaymentRequestHistory
                {
                    Comment = string.IsNullOrEmpty(comment) ? string.Empty : comment.Length > 499 ? comment.Substring(0, 499) : comment,
                    CreationTime = currentDate,
                    RequestId = request.Id,
                    SessionId = Identity.IsAdminUser ? Identity.SessionId : (long?)null,
                    Status = (int)state
                };
                Db.PaymentRequestHistories.Add(history);

                if (state == PaymentRequestStates.Approved &&
                    request.Status == (int)PaymentRequestStates.Failed)
                    throw CreateException(LanguageId, Constants.Errors.CanNotPayFailedRequest);
                if (state == PaymentRequestStates.Approved &&
                   (request.Status == (int)PaymentRequestStates.Approved || request.Status == (int)PaymentRequestStates.ApprovedManually))
                    throw CreateException(LanguageId, Constants.Errors.CanNotPayPayedRequest);
                if ((state == PaymentRequestStates.Failed || state == PaymentRequestStates.Deleted || state == PaymentRequestStates.CanceledByClient) &&
                    (request.Status == (int)PaymentRequestStates.Approved || request.Status == (int)PaymentRequestStates.ApprovedManually ||
                     request.Status == (int)PaymentRequestStates.Deleted || request.Status == (int)PaymentRequestStates.Failed ||
                     request.Status == (int)PaymentRequestStates.PayPanding  ||  request.Status == (int)PaymentRequestStates.CanceledByUser))
                    throw CreateException(LanguageId, Constants.Errors.CanNotCancelPayedRequest);

                request.Status = (int)state;
                request.LastUpdateTime = currentDate;
                Db.SaveChanges();
                notificationBl.SendDepositNotification(client.Id, request.Status, request.Amount, comment);
                scope.Complete();
            }
        }

        private void CheckDepositLimits(PaymentLimit paymentLimit, DateTime currentTime, decimal depositAmount)
        {
            var lastDayStartTime = currentTime.AddDays(-1);
            var lastWeekStartTime = currentTime.AddDays(-7);
            var lastMonthStartTime = currentTime.AddMonths(-1);

            var states = new List<int>
            {
                (int)PaymentRequestStates.CanceledByUser,
                (int)PaymentRequestStates.CanceledByClient,
                (int)PaymentRequestStates.Failed
            };
            if (paymentLimit.MaxDepositAmount.HasValue && paymentLimit.MaxDepositAmount.Value < depositAmount)
                throw CreateException(LanguageId, Constants.Errors.ClientMaxLimitExceeded);
            if (paymentLimit.MaxDepositsCountPerDay.HasValue && paymentLimit.MaxDepositsCountPerDay.Value <=
                Db.PaymentRequests.Count(x => x.ClientId == paymentLimit.ClientId && x.Type == (int)PaymentRequestTypes.Deposit && !states.Contains(x.Status) && x.CreationTime >= lastDayStartTime))
                throw CreateException(LanguageId, Constants.Errors.ClientMaxLimitExceeded);

            var maxTotalDepositsAmountPerDay = (from pr in Db.PaymentRequests where pr.ClientId == paymentLimit.ClientId && pr.Type == (int)PaymentRequestTypes.Deposit && !states.Contains(pr.Status) && pr.CreationTime >= lastDayStartTime select (decimal?)pr.Amount).Sum() ?? 0;
            maxTotalDepositsAmountPerDay += depositAmount;

            if (paymentLimit.MaxTotalDepositsAmountPerDay.HasValue && paymentLimit.MaxTotalDepositsAmountPerDay.Value < maxTotalDepositsAmountPerDay)
                throw CreateException(LanguageId, Constants.Errors.ClientMaxLimitExceeded);

            var maxTotalDepositsAmountPerWeek = (from pr in Db.PaymentRequests where pr.ClientId == paymentLimit.ClientId && pr.Type == (int)PaymentRequestTypes.Deposit && !states.Contains(pr.Status) && pr.CreationTime >= lastWeekStartTime select (decimal?)pr.Amount).Sum() ?? 0;
            maxTotalDepositsAmountPerWeek += depositAmount;

            if (paymentLimit.MaxTotalDepositsAmountPerWeek.HasValue && paymentLimit.MaxTotalDepositsAmountPerWeek.Value < maxTotalDepositsAmountPerWeek)
                throw CreateException(LanguageId, Constants.Errors.ClientMaxLimitExceeded);

            var maxTotalDepositsAmountPerMonth = (from pr in Db.PaymentRequests where pr.ClientId == paymentLimit.ClientId && pr.Type == (int)PaymentRequestTypes.Deposit && !states.Contains(pr.Status) && pr.CreationTime >= lastMonthStartTime select (decimal?)pr.Amount).Sum() ?? 0;
            maxTotalDepositsAmountPerMonth += depositAmount;

            if (paymentLimit.MaxTotalDepositsAmountPerMonth.HasValue && paymentLimit.MaxTotalDepositsAmountPerMonth.Value < maxTotalDepositsAmountPerMonth)
                throw CreateException(LanguageId, Constants.Errors.ClientMaxLimitExceeded);
        }
        #endregion

        public Document CreateDebitCorrectionOnClient(ClientCorrectionInput correction, DocumentBll documentBl, bool checkPermission = true)
        {
            var client = CacheManager.GetClientById(correction.ClientId);
            if (client == null)
                throw CreateException(LanguageId, Constants.Errors.ClientNotFound);
            if (checkPermission)
            {
                CheckPermission(Constants.Permissions.CreateDebitCorrectionOnClient);

                var checkClientPermission = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewClient,
                    ObjectTypeId = (int)ObjectTypes.Client
                });
                var affiliateAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewAffiliateReferral,
                    ObjectTypeId = (int)ObjectTypes.AffiliateReferral
                });
                var checkPartnerPermission = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewPartner,
                    ObjectTypeId = (int)ObjectTypes.Partner
                });
                if (!checkClientPermission.HaveAccessForAllObjects && checkClientPermission.AccessibleObjects.All(x => x != correction.ClientId) ||
                     (!checkPartnerPermission.HaveAccessForAllObjects && checkPartnerPermission.AccessibleObjects.All(x => x != client.PartnerId)) ||
                     (!affiliateAccess.HaveAccessForAllObjects &&
                     affiliateAccess.AccessibleObjects.All(x => client.AffiliateReferralId.HasValue && x != client.AffiliateReferralId.Value)))
                    throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            }
            Document document = null;
            using (var scope = CommonFunctions.CreateTransactionScope())
            {
                correction.CurrencyId = client.CurrencyId;
                var operation = new Operation
                {
                    Type = (int)OperationTypes.DebitCorrectionOnClient,
                    DocumentTypeId = correction.OperationTypeId,
                    Creator = Identity.Id,
                    Info = correction.Info,
                    ClientId = correction.ClientId,
                    Amount = correction.Amount,
                    CurrencyId = correction.CurrencyId,
                    ExternalOperationId = correction.ExternalOperationId,
                    ExternalTransactionId = correction.ExternalTransactionId,
                    ProductId = correction.ProductId,
                    OperationItems = new List<OperationItem>()
                };

                var item = new OperationItem
                {
                    AccountTypeId =
                        (correction.AccountTypeId == null
                            ? (int)AccountTypes.ClientUnusedBalance
                            : correction.AccountTypeId.Value),
                    ObjectId = client.Id,
                    ObjectTypeId = (int)ObjectTypes.Client,
                    Amount = correction.Amount,
                    CurrencyId = correction.CurrencyId,
                    Type = (int)TransactionTypes.Debit,
                    OperationTypeId = (int)OperationTypes.DebitCorrectionOnClient
                };
                if (correction.AccountId.HasValue)
                {
                    var account = GetAccount(correction.AccountId.Value);
                    item.AccountId = account.Id;
                }
                operation.OperationItems.Add(item);
                var user = CacheManager.GetUserById(Identity.Id);
                var userPermissions = CacheManager.GetUserPermissions(Identity.Id);
                var permission = userPermissions.FirstOrDefault(x => x.PermissionId == Constants.Permissions.EditPartnerAccounts || x.IsAdmin);

                item = new OperationItem
                {
                    AccountTypeId = permission != null ? (int)AccountTypes.PartnerBalance : (int)AccountTypes.UserBalance,
                    ObjectId = user.Type == (int)UserTypes.AgentEmployee ? user.ParentId.Value : (permission != null ? user.PartnerId : user.Id),
                    ObjectTypeId = permission != null ? (int)ObjectTypes.Partner : (int)ObjectTypes.User,
                    Amount = correction.Amount,
                    CurrencyId = correction.CurrencyId,
                    Type = (int)TransactionTypes.Credit,
                    OperationTypeId = (int)OperationTypes.DebitCorrectionOnClient
                };
                operation.OperationItems.Add(item);
                document = documentBl.CreateDocument(operation);
                Db.SaveChanges();
                scope.Complete();
                CacheManager.RemoveClientBalance(client.Id);
            }
            return document;
        }

        public Document CreateCreditCorrectionOnClient(ClientCorrectionInput correction, DocumentBll documentBl, bool checkPermission = true)
        {
            var client = CacheManager.GetClientById(correction.ClientId);
            if (client == null)
                throw CreateException(LanguageId, Constants.Errors.ClientNotFound);
            if (checkPermission)
            {
                CheckPermission(Constants.Permissions.CreateCreditCorrectionOnClient);
                var checkClientPermission = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewClient,
                    ObjectTypeId = (int)ObjectTypes.Client
                });

                var checkPartnerPermission = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewPartner,
                    ObjectTypeId = (int)ObjectTypes.Partner
                });
                var affiliateAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewAffiliateReferral,
                    ObjectTypeId = (int)ObjectTypes.AffiliateReferral
                });
                if (!checkClientPermission.HaveAccessForAllObjects && checkClientPermission.AccessibleObjects.All(x => x != correction.ClientId) ||
                    (!checkPartnerPermission.HaveAccessForAllObjects && checkPartnerPermission.AccessibleObjects.All(x => x != client.PartnerId)) ||
                    (!affiliateAccess.HaveAccessForAllObjects &&
                      affiliateAccess.AccessibleObjects.All(x => client.AffiliateReferralId.HasValue && x != client.AffiliateReferralId.Value)))
                    throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            }
            Document document = null;
            using (var scope = CommonFunctions.CreateTransactionScope())
            {
                correction.CurrencyId = client.CurrencyId;
                var operation = new Operation
                {
                    Type = (int)OperationTypes.CreditCorrectionOnClient,
                    DocumentTypeId = correction.OperationTypeId,
                    Creator = Identity.Id == 0 ? (int?)null : Identity.Id,
                    Info = correction.Info,
                    ClientId = correction.ClientId,
                    Amount = correction.Amount,
                    CurrencyId = correction.CurrencyId,
                    ExternalOperationId = correction.ExternalOperationId,
                    ExternalTransactionId = correction.ExternalTransactionId,
                    ProductId = correction.ProductId,
                    OperationItems = new List<OperationItem>()
                };
                var item = new OperationItem
                {
                    AccountTypeId = correction.AccountTypeId,
                    ObjectId = client.Id,
                    ObjectTypeId = (int)ObjectTypes.Client,
                    Amount = correction.Amount,
                    CurrencyId = correction.CurrencyId,
                    Type = (int)TransactionTypes.Credit,
                    OperationTypeId = (int)OperationTypes.CreditCorrectionOnClient
                };
                if (correction.AccountId.HasValue)
                {
                    var account = GetAccount(correction.AccountId.Value);
                    item.AccountId = account.Id;
                }
                operation.OperationItems.Add(item);
                var user = CacheManager.GetUserById(Identity.Id);
                var userPermissions = CacheManager.GetUserPermissions(user.Id);
                var permission = userPermissions.FirstOrDefault(x => x.PermissionId == Constants.Permissions.EditPartnerAccounts || x.IsAdmin);
                item = new OperationItem
                {
                    AccountTypeId = permission != null ? (int)AccountTypes.PartnerBalance : (int)AccountTypes.UserBalance,
                    ObjectId = user.Type == (int)UserTypes.AgentEmployee ? user.ParentId.Value : (permission != null ? user.PartnerId : user.Id),
                    ObjectTypeId = permission != null ? (int)ObjectTypes.Partner : (int)ObjectTypes.User,
                    Amount = correction.Amount,
                    CurrencyId = correction.CurrencyId,
                    Type = (int)TransactionTypes.Debit,
                    OperationTypeId = (int)OperationTypes.CreditCorrectionOnClient
                };
                operation.OperationItems.Add(item);
                document = documentBl.CreateDocument(operation);
                Db.SaveChanges();
                scope.Complete();
                CacheManager.RemoveClientBalance(client.Id);
            }
            return document;
        }

        public Document CreateCreditFromClient(ListOfOperationsFromApi transaction, DocumentBll documentBl)
        {
            var operationTypeId = transaction.OperationTypeId ?? (int)OperationTypes.Bet;
            var product = transaction.ProductId.HasValue ? CacheManager.GetProductById(transaction.ProductId.Value)
                : CacheManager.GetProductByExternalId(transaction.GameProviderId, transaction.ExternalProductId);
            if (product == null)
                throw CreateException(LanguageId, Constants.Errors.ProductNotFound);
            var alreadyDoneTransactionId = documentBl.GetExistingDocumentId(transaction.GameProviderId, transaction.TransactionId, operationTypeId, product.Id);
            if (alreadyDoneTransactionId > 0)
                throw CreateException(LanguageId, Constants.Errors.TransactionAlreadyExists, alreadyDoneTransactionId);
            transaction.ProductId = product.Id;
            Document document = null;
            var operationItemFromProduct = transaction.OperationItems[0];
            var operationAmount = operationItemFromProduct.Amount;
            using (var scope = CommonFunctions.CreateTransactionScope())
            {
                var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(operationItemFromProduct.Client.PartnerId, product.Id);
                if (partnerProductSetting == null)
                    throw CreateException(LanguageId, Constants.Errors.ProductNotAllowedForThisPartner);
                if (partnerProductSetting.State == (int)PartnerProductSettingStates.Blocked)
                    throw CreateException(LanguageId, Constants.Errors.ProductBlockedForThisPartner);
                var clientState = ClientBll.GetClientStateByProduct(operationItemFromProduct.Client, product);
                if (clientState == (int)ClientStates.FullBlocked || clientState == (int)ClientStates.Disabled ||
                    clientState == (int)ClientStates.BlockedForBet || clientState == (int)ClientStates.Suspended)
                    throw CreateException(LanguageId, Constants.Errors.ClientBlocked);
                CheckClientLimit(product.Id, operationItemFromProduct.Client, operationItemFromProduct.Amount, LanguageId, transaction.TicketInfo);
                CheckPartnerProductLimit(product.Id, operationItemFromProduct.Client.PartnerId, operationItemFromProduct.Amount, LanguageId);
                var operation = new Operation
                {
                    Amount = transaction.BonusId > 0 ? 0 : operationAmount,
                    CurrencyId = transaction.CurrencyId,
                    Type = operationTypeId,
                    ExternalTransactionId = transaction.TransactionId,
                    ExternalOperationId = transaction.ExternalOperationId,
                    Info = transaction.Info,
                    ClientId = operationItemFromProduct.Client.Id,
                    GameProviderId = transaction.GameProviderId,
                    PartnerProductId = partnerProductSetting.Id,
                    ProductId = product.Id,
                    DeviceTypeId = operationItemFromProduct.DeviceTypeId,
                    DocumentTypeId = transaction.TypeId,
                    PossibleWin = operationItemFromProduct.PossibleWin,
                    RoundId = transaction.RoundId,
                    TicketInfo = transaction.TicketInfo,
                    State = transaction.State,
                    ParentId = transaction.CreditTransactionId,
                    SessionId = transaction.SessionId,
                    OperationItems = new List<OperationItem>()
                };
                if (operationTypeId == (int)OperationTypes.Bet)
                {
                    if (CacheManager.GetConfigKey(operationItemFromProduct.Client.PartnerId, Constants.PartnerKeys.AMLVerification) == "1")
                    {
                        var amlVerified = CacheManager.GetClientSettingByName(operationItemFromProduct.Client.Id, ClientSettings.AMLVerified);
                        if (amlVerified == null || amlVerified.Id == 0 || amlVerified.StringValue != "1")
                            throw CreateException(LanguageId, Constants.Errors.AMLProhibited);
                    }
                    if (transaction.BonusId > 0)
                    {
                        operation.Info = JsonConvert.SerializeObject(new DocumentInfo { BonusId = transaction.BonusId, ReuseNumber = 0, FromBonusBalance = false });
                        operation.BonusId = transaction.BonusId;
                        operation.FreezeBonusBalance = true;
                    }
                    else
                    {
                        var b = FindWagerBonusByProductId(operationItemFromProduct.Client.Id, product, out decimal percent, out bool freezeBonusBalance);
                        if (b.BonusId > 0)
                        {
                            if (product.Id == Constants.SportsbookProductId &&
                                !CheckWagerAvailability(operationItemFromProduct.Client.Id, b.BonusId, operation.TicketInfo))
                            {
                                operation.Info = string.Empty;
                                operation.BonusId = null;
                            }
                            else
                            {
                                var bonus = Db.ClientBonus.First(x => x.Id == b.Id);
                                bonus.TurnoverAmountLeft -= (operationItemFromProduct.Amount * percent / 100);
                                if (bonus.TurnoverAmountLeft < 0)
                                    bonus.TurnoverAmountLeft = 0;

                                operation.BonusId = b.BonusId;
                                operation.Info = JsonConvert.SerializeObject(new DocumentInfo { BonusId = b.BonusId, ReuseNumber = b.ReuseNumber ?? 0, FromBonusBalance = false });
                            }
                            operation.FreezeBonusBalance = freezeBonusBalance;
                        }
                    }
                }
                var externalPlatformType = CacheManager.GetPartnerSettingByKey(operationItemFromProduct.Client.PartnerId, Constants.PartnerKeys.ExternalPlatform);
                if (externalPlatformType == null || externalPlatformType.NumericValue == null ||
                    externalPlatformType.NumericValue.Value != (int)PartnerTypes.ExternalPlatform || !operationItemFromProduct.Client.UserName.Contains(Constants.ExternalClientPrefix))
                {
                    var item = new OperationItem
                    {
                        AccountTypeId = (int)AccountTypes.ProductDebtToPartner,
                        ObjectId = partnerProductSetting.Id,
                        ObjectTypeId = (int)ObjectTypes.PartnerProduct,
                        Amount = transaction.BonusId > 0 ? 0 : operationAmount,
                        CurrencyId = transaction.CurrencyId,
                        Type = (int)TransactionTypes.Debit,
                        OperationTypeId = operationTypeId
                    };
                    operation.OperationItems.Add(item);
                    item = new OperationItem
                    {
                        ObjectId = operationItemFromProduct.Client.Id,
                        ObjectTypeId = (int)ObjectTypes.Client,
                        Amount = transaction.BonusId > 0 ? 0 : operationAmount,
                        CurrencyId = transaction.CurrencyId,
                        Type = (int)TransactionTypes.Credit,
                        OperationTypeId = operationTypeId
                    };
                    operation.OperationItems.Add(item);
                }
                document = documentBl.CreateDocument(operation);
                if (operationTypeId == (int)OperationTypes.Bet)
                {
                    if (transaction.BonusId > 0)
                    {
                        UpdateClientFreeBetBonus(transaction.BonusId, operationItemFromProduct.Client.Id, operationAmount);
                        document.BonusId = transaction.BonusId;
                    }
                    else
                    {
                        if (!operation.BonusId.HasValue || operation.BonusId.Value == 0)
                        {
                            var clientBonuses = CacheManager.GetClientNotAwardedCampaigns(operationItemFromProduct.Client.Id);

                            FairClientBonusTrigger(new ClientTriggerInput
                            {
                                ClientBonuses = clientBonuses,
                                ClientId = operationItemFromProduct.Client.Id,
                                TriggerType = product.Id == Constants.SportsbookProductId ? (int)TriggerTypes.BetPlacement : (int)TriggerTypes.CrossProductBetPlacement,
                                ProductId = product.Id,
                                TicketInfo = operation.TicketInfo,
                                SourceAmount = operationAmount
                            }, out bool alreadyAdded);
                        }
                        else
                            document.BonusId = operation.BonusId.Value;
                    }
                }
                Db.SaveChanges();
                scope.Complete();
                CacheManager.RemoveClientBalance(operationItemFromProduct.Client.Id);
            }
            CacheManager.UpdateTotalBetAmount(operationItemFromProduct.Client.Id, transaction.BonusId > 0 ? 0 : operationAmount);
            return document;
        }

        public BllClientBonus FindWagerBonusByProductId(int clientId, BllProduct product, out decimal percent, out bool freezeBonusBalance)
        {
            var isFound = false;
            percent = 0;
            freezeBonusBalance = false;
            var cacheBonus = CacheManager.GetActiveWageringBonus(clientId);
            if (cacheBonus.Id > 0)
            {
                var clientBonus = CacheManager.GetBonusById(cacheBonus.BonusId);
                if (clientBonus.BonusType == (int)BonusTypes.CampaignWagerSport)
                {
                    if (product.Id == Constants.SportsbookProductId)
                    {
                        isFound = true;
                        percent = 100;
                    }
                }
                else if (product.Id != Constants.SportsbookProductId)
                {
                    var bonusProducts = CacheManager.GetBonusProducts(cacheBonus.BonusId);
                    while (!isFound)
                    {
                        var pr = bonusProducts.FirstOrDefault(x => x.ProductId == product.Id);
                        if (pr != null)
                        {
                            if (pr.Percent == 0)
                                break;
                            isFound = true;
                            percent = pr.Percent;
                        }
                        else
                        {
                            if (!product.ParentId.HasValue)
                                break;
                            product = CacheManager.GetProductById(product.ParentId.Value);
                        }
                    }
                }
                if (isFound)
                {
                    freezeBonusBalance = clientBonus.FreezeBonusBalance ?? false;
                    return cacheBonus;
                }
            }
            return new BllClientBonus();
        }

        public List<Document> CreateDebitsToClients(ListOfOperationsFromApi transactions, Document creditTransaction, DocumentBll documentBl)
        {
            var documents = new List<Document>();
            var operationTypeId = transactions.OperationTypeId ?? (int)OperationTypes.Win;
            var product = transactions.ProductId.HasValue
                ? CacheManager.GetProductById(transactions.ProductId.Value)
                : CacheManager.GetProductByExternalId(transactions.GameProviderId, transactions.ExternalProductId);
            if (product == null)
                throw CreateException(LanguageId, Constants.Errors.ProductNotFound);
            var alreadyDoneTransactionId = documentBl.GetExistingDocumentId(transactions.GameProviderId, transactions.TransactionId, operationTypeId, product.Id);
            if (alreadyDoneTransactionId > 0)
                throw CreateException(LanguageId, Constants.Errors.TransactionAlreadyExists, alreadyDoneTransactionId);
            transactions.ProductId = product.Id;
            using (var scope = CommonFunctions.CreateTransactionScope())
            {
                foreach (var operationItemFromProduct in transactions.OperationItems)
                {
                    var st = ClientBll.GetClientStateByProduct(operationItemFromProduct.Client, product);
                    if (st == (int)ClientStates.FullBlocked || st == (int)ClientStates.Disabled)
                        throw CreateException(LanguageId, Constants.Errors.ClientBlocked);
                    var partnerProductSetting = CacheManager.GetPartnerProductSettingByProductId(operationItemFromProduct.Client.PartnerId, product.Id);
                    if (partnerProductSetting == null)
                        throw CreateException(LanguageId, Constants.Errors.ProductNotAllowedForThisPartner);
                    if (partnerProductSetting.State == (int)PartnerProductSettingStates.Blocked)
                        throw CreateException(LanguageId, Constants.Errors.ProductBlockedForThisPartner);
                    long? parentDocumentId = null;
                    if (!product.GameProviderId.HasValue)
                        throw CreateException(LanguageId, Constants.Errors.WrongProductId);

                    DocumentInfo documentInfo = null;
                    decimal betAmount = 0;
                    if (creditTransaction != null)
                    {
                        parentDocumentId = transactions.CreditTransactionId.Value;
                        if (transactions.State != null)
                            creditTransaction.State = transactions.State.Value;

                        try
                        {
                            documentInfo = JsonConvert.DeserializeObject<DocumentInfo>(creditTransaction.Info);
                        }
                        catch { }

                        var info = string.IsNullOrEmpty(creditTransaction.TicketInfo) ? new BonusTicketInfo() : 
                            JsonConvert.DeserializeObject<BonusTicketInfo>(creditTransaction.TicketInfo);
                        betAmount = info.BetAmount;
                    }

                    var clientOperation = new ClientOperation
                    {
                        ParentDocumentId = parentDocumentId,
                        GameProviderId = transactions.GameProviderId,
                        OperationTypeId = operationTypeId,
                        State = transactions.State,
                        Amount = transactions.IsFreeBet.HasValue && transactions.IsFreeBet.Value ? 
                                 Math.Max(0, operationItemFromProduct.Amount - betAmount) : operationItemFromProduct.Amount,
                        CurrencyId = transactions.CurrencyId,
                        ClientId = operationItemFromProduct.Client.Id,
                        PartnerProductId = partnerProductSetting.Id,
                        ExternalTransactionId = transactions.TransactionId,
                        ExternalOperationId = transactions.ExternalOperationId,
                        Info = transactions.Info,
                        ProductId = product.Id,
                        RoundId = transactions.RoundId,
                        AccountTypeId = transactions.IsFreeBet.HasValue && transactions.IsFreeBet.Value ? (int)AccountTypes.ClientUsedBalance :
                        ((creditTransaction != null && creditTransaction.OperationTypeId == (int)OperationTypes.Bet && documentInfo != null &&
                            documentInfo.FromBonusBalance) ? (int)AccountTypes.ClientBonusBalance : (int)AccountTypes.ClientUsedBalance),
                        PartnerId = operationItemFromProduct.Client.PartnerId,
                        PossibleWin = operationItemFromProduct.PossibleWin

                    };
                    if (documentInfo != null && documentInfo.BonusId > 0 && clientOperation.AccountTypeId == (int)AccountTypes.ClientBonusBalance)
                    {
                        var bonus = CacheManager.GetClientBonusById(operationItemFromProduct.Client.Id, documentInfo.BonusId);
                        if (bonus != null)
                        {
                            if(bonus.Id > 0 && bonus.Status != (int)BonusStatuses.Active)
                               clientOperation.Amount = 0;

                            if (transactions.State == (int)BetDocumentStates.Returned)
                            {
                                var clientBonus = Db.ClientBonus.First(x => x.Id == bonus.Id);
                                clientBonus.TurnoverAmountLeft += operationItemFromProduct.Amount;
                            }
                        }
                    }
                    if (transactions.State == null)
                    {
                        clientOperation.State = (operationItemFromProduct.Amount == 0
                           ? (int)BetDocumentStates.Lost : (int)BetDocumentStates.Won);
                    }
                    documents.Add(CreateDebitToClient(clientOperation, operationItemFromProduct.Client.Id, 
                        operationItemFromProduct.Client.UserName, documentBl, creditTransaction));
                }
                scope.Complete();
            }
            return documents;
        }

        public Document CreateCreditCorrectionFromJob(Client client, ClientCorrectionInput correction, DocumentBll documentBl)
        {
            correction.CurrencyId = client.CurrencyId;
            var operation = new Operation
            {
                Type = (int)OperationTypes.CreditCorrectionOnClient,
                DocumentTypeId = correction.OperationTypeId,
                Creator = Identity.Id == 0 ? (int?)null : Identity.Id,
                Info = correction.Info,
                ClientId = correction.ClientId,
                Amount = correction.Amount,
                CurrencyId = correction.CurrencyId,
                ExternalOperationId = correction.ExternalOperationId,
                ExternalTransactionId = correction.ExternalTransactionId,
                ProductId = correction.ProductId,
                OperationItems = new List<OperationItem>()
            };
            var item = new OperationItem
            {
                AccountTypeId = correction.AccountTypeId,
                ObjectId = client.Id,
                ObjectTypeId = (int)ObjectTypes.Client,
                Amount = correction.Amount,
                CurrencyId = correction.CurrencyId,
                Type = (int)TransactionTypes.Credit,
                OperationTypeId = (int)OperationTypes.CreditCorrectionOnClient
            };
            if (correction.AccountId.HasValue)
            {
                var account = GetAccount(correction.AccountId.Value);
                item.AccountId = account.Id;
            }
            operation.OperationItems.Add(item);
            item = new OperationItem
            {
                AccountTypeId = (client.UserId == null ? (int)AccountTypes.PartnerBalance : (int)AccountTypes.UserBalance),
                ObjectId = (client.UserId == null ? client.PartnerId : client.UserId.Value),
                ObjectTypeId = (client.UserId == null ? (int)ObjectTypes.Partner : (int)ObjectTypes.User),
                Amount = correction.Amount,
                CurrencyId = correction.CurrencyId,
                Type = (int)TransactionTypes.Debit,
                OperationTypeId = (int)OperationTypes.CreditCorrectionOnClient
            };
            operation.OperationItems.Add(item);
            var document = documentBl.CreateDocumentFromJob(operation);
            Db.SaveChanges();
            return document;
        }

        public Document CreateDebitToClientFromJob(int clientId, ClientOperation transaction, DocumentBll documentBl)
        {
            var operation = new Operation
            {
                Amount = transaction.Amount,
                CurrencyId = transaction.CurrencyId,
                Type = transaction.OperationTypeId,
                ExternalTransactionId = transaction.ExternalTransactionId,
                GameProviderId = transaction.GameProviderId,
                Info = transaction.Info,
                ClientId = transaction.ClientId,
                PartnerProductId = transaction.PartnerProductId,
                ExternalOperationId = transaction.ExternalOperationId,
                ParentId = transaction.ParentDocumentId,
                RoundId = transaction.RoundId,
                ProductId = transaction.ProductId,
                State = transaction.State,
                TicketInfo = transaction.Info,
                Creator = transaction.Creator,
                OperationItems = new List<OperationItem>()
            };

            var item = new OperationItem
            {
                AccountTypeId = transaction.AccountTypeId,
                ObjectId = clientId,
                ObjectTypeId = (int)ObjectTypes.Client,
                Amount = transaction.Amount,
                CurrencyId = transaction.CurrencyId,
                Type = (int)TransactionTypes.Debit,
                OperationTypeId = transaction.OperationTypeId,

            };
            operation.OperationItems.Add(item);
            if (transaction.PartnerProductId.HasValue)
            {
                item = new OperationItem
                {
                    ObjectId = transaction.PartnerProductId.Value,
                    ObjectTypeId = (int)ObjectTypes.PartnerProduct,
                    AccountTypeId = (int)AccountTypes.ProductDebtToPartner,
                    Amount = transaction.Amount,
                    CurrencyId = transaction.CurrencyId,
                    Type = (int)TransactionTypes.Credit,
                    OperationTypeId = transaction.OperationTypeId
                };
            }
            else if (transaction.OperationTypeId == (int)OperationTypes.BonusWin)
            {
                item = new OperationItem
                {
                    AccountTypeId = (int)AccountTypes.ClientBonusBalance,
                    ObjectId = clientId,
                    ObjectTypeId = (int)ObjectTypes.Client,
                    Amount = transaction.Amount,
                    CurrencyId = transaction.CurrencyId,
                    Type = (int)TransactionTypes.Credit,
                    OperationTypeId = transaction.OperationTypeId
                };
            }
            else
            {
                item = new OperationItem
                {
                    ObjectId = transaction.PartnerId,
                    ObjectTypeId = (int)ObjectTypes.Partner,
                    AccountTypeId = (int)AccountTypes.PartnerBalance,
                    Amount = transaction.Amount,
                    CurrencyId = transaction.CurrencyId,
                    Type = (int)TransactionTypes.Credit,
                    OperationTypeId = transaction.OperationTypeId
                };
            }
            operation.OperationItems.Add(item);

            var document = documentBl.CreateDocumentFromJob(operation);
            Db.SaveChanges();
            return document;
        }

        public Document CreateDebitToClient(ClientOperation transaction, int clientId, string userName, DocumentBll documentBl, Document creditTransaction)
        {
            var operation = new Operation
            {
                Amount = transaction.Amount,
                CurrencyId = transaction.CurrencyId,
                Type = transaction.OperationTypeId,
                ExternalTransactionId = transaction.ExternalTransactionId,
                GameProviderId = transaction.GameProviderId,
                Info = transaction.Info,
                ClientId = transaction.ClientId,
                PartnerProductId = transaction.PartnerProductId,
                ExternalOperationId = transaction.ExternalOperationId,
                ParentId = transaction.ParentDocumentId,
                RoundId = transaction.RoundId,
                ProductId = transaction.ProductId,
                State = transaction.State,
                TicketInfo = transaction.Info,
                Creator = transaction.Creator,
                PossibleWin = transaction.PossibleWin,
                OperationItems = new List<OperationItem>()
            };
            var externalPlatformType = CacheManager.GetPartnerSettingByKey(transaction.PartnerId, Constants.PartnerKeys.ExternalPlatform);
            if (externalPlatformType == null || externalPlatformType.NumericValue == null ||
                externalPlatformType.NumericValue.Value != (int)PartnerTypes.ExternalPlatform || !userName.Contains(Constants.ExternalClientPrefix))
            {
                var item = new OperationItem
                {
                    AccountTypeId = transaction.AccountTypeId,
                    ObjectId = clientId,
                    ObjectTypeId = (int)ObjectTypes.Client,
                    Amount = transaction.Amount,
                    CurrencyId = transaction.CurrencyId,
                    Type = (int)TransactionTypes.Debit,
                    OperationTypeId = transaction.OperationTypeId,

                };
                operation.OperationItems.Add(item);
                if (transaction.PartnerProductId.HasValue)
                {
                    item = new OperationItem
                    {
                        ObjectId = transaction.PartnerProductId.Value,
                        ObjectTypeId = (int)ObjectTypes.PartnerProduct,
                        AccountTypeId = (int)AccountTypes.ProductDebtToPartner,
                        Amount = transaction.Amount,
                        CurrencyId = transaction.CurrencyId,
                        Type = (int)TransactionTypes.Credit,
                        OperationTypeId = transaction.OperationTypeId
                    };
                }
                else if (transaction.OperationTypeId == (int)OperationTypes.BonusWin)
                {
                    item = new OperationItem
                    {
                        AccountTypeId = (int)AccountTypes.ClientBonusBalance,
                        ObjectId = clientId,
                        ObjectTypeId = (int)ObjectTypes.Client,
                        Amount = transaction.Amount,
                        CurrencyId = transaction.CurrencyId,
                        Type = (int)TransactionTypes.Credit,
                        OperationTypeId = transaction.OperationTypeId
                    };
                }
                else
                {
                    item = new OperationItem
                    {
                        ObjectId = transaction.PartnerId,
                        ObjectTypeId = (int)ObjectTypes.Partner,
                        AccountTypeId = (int)AccountTypes.PartnerBalance,
                        Amount = transaction.Amount,
                        CurrencyId = transaction.CurrencyId,
                        Type = (int)TransactionTypes.Credit,
                        OperationTypeId = transaction.OperationTypeId
                    };
                }
                operation.OperationItems.Add(item);
            }
            var document = documentBl.CreateDocument(operation);
            Db.SaveChanges();
            if (transaction.OperationTypeId == (int)OperationTypes.Win && transaction.ProductId == Constants.SportsbookProductId)
            {
                var clientBonuses = CacheManager.GetClientNotAwardedCampaigns(clientId);
                FairClientBonusTrigger(new ClientTriggerInput
                {
                    ClientId = clientId,
                    ClientBonuses = clientBonuses,
                    TriggerType = (int)TriggerTypes.BetSettlement,
                    SourceAmount = creditTransaction == null ? (decimal?)null : creditTransaction.Amount,
                    TicketInfo = creditTransaction == null ? string.Empty : creditTransaction.TicketInfo,
                    WinInfo = transaction.Info,
                    ProductId = transaction.ProductId
                }, out _);
            }
            CacheManager.RemoveClientBalance(clientId);
            CacheManager.UpdateTotalLossAmount(clientId, -operation.Amount);
            return document;
        }

        public void ChangeWithdrawPaymentRequestState(long requestId, string comment, int? cashDeskId, int? cashierId, PaymentRequestStates state)
        {
            switch (state)
            {
                case PaymentRequestStates.InProcess:
                    CheckPermission(Constants.Permissions.InProcessPaymentRequest);
                    break;
                case PaymentRequestStates.Frozen:
                    CheckPermission(Constants.Permissions.FrozenPaymentRequest);
                    break;
                case PaymentRequestStates.WaitingForKYC:
                    CheckPermission(Constants.Permissions.KYCPaymentRequest);
                    break;
            }
            using var documentBl = new DocumentBll(this);
            using var notificationBl = new NotificationBll(documentBl);
            ChangeWithdrawRequestState(requestId, state, comment, cashDeskId, cashierId, true, string.Empty, documentBl, notificationBl);
        }

        #endregion

        #region Limit

        public static void CheckPartnerProductLimit(int productId, int partnerId, decimal amount, string languageId)
        {
            var partnerProductLimit = CacheManager.GetPartnerProductLimit(productId, partnerId);
            if (partnerProductLimit.Id == 0)
                return;
            if (partnerProductLimit.MaxLimit.HasValue && partnerProductLimit.MaxLimit.Value < amount)
                throw CreateException(languageId, Constants.Errors.PartnerProductLimitExceeded, decimalInfo: partnerProductLimit.MaxLimit.Value);
        }

        public void CheckClientLimit(int productId, BllClient client, decimal amount, string languageId, string ticketInfo)
        {
            var clientSetting = new ClientCustomSettings();
            var betLimitDaily = CacheManager.GetClientSettingByName(client.Id, nameof(clientSetting.TotalBetAmountLimitDaily));
            var systemBetLimitDaily = CacheManager.GetClientSettingByName(client.Id, nameof(clientSetting.SystemTotalBetAmountLimitDaily));

            if ((betLimitDaily != null && betLimitDaily.NumericValue != null) || (systemBetLimitDaily != null && systemBetLimitDaily.NumericValue != null))
            {
                decimal limitAmount = -1;
                if (betLimitDaily != null && betLimitDaily.NumericValue != null)
                    limitAmount = betLimitDaily.NumericValue.Value;
                if (systemBetLimitDaily != null && systemBetLimitDaily.NumericValue != null)
                    limitAmount = limitAmount == -1 ? systemBetLimitDaily.NumericValue.Value : Math.Min(limitAmount, systemBetLimitDaily.NumericValue.Value);
                var totalBetAmount = CacheManager.GetTotalBetAmounts(client.Id, (int)PeriodsOfTime.Daily);
                if (totalBetAmount + amount > limitAmount)
                    throw BaseBll.CreateException(LanguageId, Constants.Errors.ClientDailyLimitExceeded);
            }

            var betLimitWeekly = CacheManager.GetClientSettingByName(client.Id, nameof(clientSetting.TotalBetAmountLimitWeekly));
            var systemBetLimitWeekly = CacheManager.GetClientSettingByName(client.Id, nameof(clientSetting.SystemTotalBetAmountLimitWeekly));
            if ((betLimitWeekly != null && betLimitWeekly.NumericValue != null) || (systemBetLimitWeekly != null && systemBetLimitWeekly.NumericValue != null))
            {
                decimal limitAmount = -1;
                if (betLimitWeekly != null && betLimitWeekly.NumericValue != null)
                    limitAmount = betLimitWeekly.NumericValue.Value;
                if (systemBetLimitWeekly != null && systemBetLimitWeekly.NumericValue != null)
                    limitAmount = limitAmount == -1 ? systemBetLimitWeekly.NumericValue.Value : Math.Min(limitAmount, systemBetLimitWeekly.NumericValue.Value);

                var totalBetAmount = CacheManager.GetTotalBetAmounts(client.Id, (int)PeriodsOfTime.Weekly);
                if (totalBetAmount + amount > limitAmount)
                    throw BaseBll.CreateException(LanguageId, Constants.Errors.ClientWeeklyLimitExceeded);
            }

            var betLimitMonthly = CacheManager.GetClientSettingByName(client.Id, nameof(clientSetting.TotalBetAmountLimitMonthly));
            var systemBetLimitMonthly = CacheManager.GetClientSettingByName(client.Id, nameof(clientSetting.SystemTotalBetAmountLimitMonthly));
            if ((betLimitMonthly != null && betLimitMonthly.NumericValue != null) || (systemBetLimitMonthly != null && systemBetLimitMonthly.NumericValue != null))
            {
                decimal limitAmount = -1;
                if (betLimitMonthly != null && betLimitMonthly.NumericValue != null)
                    limitAmount = betLimitMonthly.NumericValue.Value;
                if (systemBetLimitMonthly != null && systemBetLimitMonthly.NumericValue != null)
                    limitAmount = limitAmount == -1 ? systemBetLimitMonthly.NumericValue.Value : Math.Min(limitAmount, systemBetLimitMonthly.NumericValue.Value);

                var totalBetAmount = CacheManager.GetTotalBetAmounts(client.Id, (int)PeriodsOfTime.Monthly);
                if (totalBetAmount + amount > limitAmount)
                    throw BaseBll.CreateException(LanguageId, Constants.Errors.ClientMonthlyLimitExceeded);
            }

            var lossLimitDaily = CacheManager.GetClientSettingByName(client.Id, nameof(clientSetting.TotalLossLimitDaily));
            var systemLossLimitDaily = CacheManager.GetClientSettingByName(client.Id, nameof(clientSetting.SystemTotalLossLimitDaily));
            if ((lossLimitDaily != null && lossLimitDaily.NumericValue != null) || (systemLossLimitDaily != null && systemLossLimitDaily.NumericValue != null))
            {
                decimal? limitAmount = null;
                if (lossLimitDaily != null && lossLimitDaily.NumericValue != null)
                    limitAmount = lossLimitDaily.NumericValue.Value;
                if (systemLossLimitDaily != null && systemLossLimitDaily.NumericValue != null)
                    limitAmount = limitAmount == null ? systemLossLimitDaily.NumericValue.Value : Math.Min(limitAmount.Value, systemLossLimitDaily.NumericValue.Value);

                var totalLossAmount = CacheManager.GetTotalLossAmounts(client.Id, (int)PeriodsOfTime.Daily);
                if (totalLossAmount + amount > limitAmount)
                    throw BaseBll.CreateException(LanguageId, Constants.Errors.ClientMaxLimitExceeded);
            }

            var lossLimitWeekly = CacheManager.GetClientSettingByName(client.Id, nameof(clientSetting.TotalLossLimitWeekly));
            var systemLossLimitWeekly = CacheManager.GetClientSettingByName(client.Id, nameof(clientSetting.SystemTotalLossLimitWeekly));
            if ((lossLimitWeekly != null && lossLimitWeekly.NumericValue != null) || (systemLossLimitWeekly != null && systemLossLimitWeekly.NumericValue != null))
            {
                decimal limitAmount = -1;
                if (lossLimitWeekly != null && lossLimitWeekly.NumericValue != null)
                    limitAmount = lossLimitWeekly.NumericValue.Value;
                if (systemLossLimitWeekly != null && systemLossLimitWeekly.NumericValue != null)
                    limitAmount = limitAmount == -1 ? systemLossLimitWeekly.NumericValue.Value : Math.Min(limitAmount, systemLossLimitWeekly.NumericValue.Value);

                var totalLossAmount = CacheManager.GetTotalLossAmounts(client.Id, (int)PeriodsOfTime.Weekly);
                if (totalLossAmount + amount > limitAmount)
                    throw BaseBll.CreateException(LanguageId, Constants.Errors.ClientMaxLimitExceeded);
            }

            var lossLimitMonthly = CacheManager.GetClientSettingByName(client.Id, nameof(clientSetting.TotalLossLimitMonthly));
            var systemLossLimitMonthly = CacheManager.GetClientSettingByName(client.Id, nameof(clientSetting.SystemTotalLossLimitMonthly));
            if ((lossLimitMonthly != null && lossLimitMonthly.NumericValue != null) || (systemLossLimitMonthly != null && systemLossLimitMonthly.NumericValue != null))
            {
                decimal limitAmount = -1;
                if (lossLimitMonthly != null && lossLimitMonthly.NumericValue != null)
                    limitAmount = lossLimitMonthly.NumericValue.Value;
                if (systemLossLimitMonthly != null && systemLossLimitMonthly.NumericValue != null)
                    limitAmount = limitAmount == -1 ? systemLossLimitMonthly.NumericValue.Value : Math.Min(limitAmount, systemLossLimitMonthly.NumericValue.Value);

                var totalLossAmount = CacheManager.GetTotalLossAmounts(client.Id, (int)PeriodsOfTime.Monthly);
                if (totalLossAmount + amount > limitAmount)
                    throw BaseBll.CreateException(LanguageId, Constants.Errors.ClientMaxLimitExceeded);
            }
            var clientProductLimit = CacheManager.GetClientProductLimit(productId, client.Id);
            if (clientProductLimit.Id == 0)
                return;
            if (clientProductLimit.MaxLimit.HasValue && clientProductLimit.MaxLimit.Value < amount)
                throw CreateException(languageId, Constants.Errors.ClientMaxLimitExceeded, decimalInfo: clientProductLimit.MaxLimit.Value);

            if (client.UserId != null && productId == Constants.SportsbookProductId && !string.IsNullOrEmpty(ticketInfo))
            {
                var ticket = JsonConvert.DeserializeObject<BonusTicketInfo>(ticketInfo);
                var cp = CacheManager.GetClientCommissionPlan(client.Id);
                var limit = cp.BetSettings?.FirstOrDefault(x => x.Id == ticket.BetSelections.First().SportId && x.IsParlay == (ticket.BetSelections.Count > 1 ? 1 : 0));
                if (limit != null && limit.PreventBetting)
                        throw CreateException(languageId, Constants.Errors.ClientMaxLimitExceeded, decimalInfo: clientProductLimit.MaxLimit.Value);
            }
        }


        public DAL.Models.ProductLimit CreateProductLimitByClient(DAL.Models.ProductLimit limit)
        {
            var result = Db.ProductLimits.FirstOrDefault(x => x.ObjectTypeId == limit.ObjectTypeId
                        && x.ObjectId == limit.ObjectId &&
                        x.LimitTypeId == (int)LimitTypes.SelfExclusionLimit && !x.ProductId.HasValue && x.RowState == (int)LimitRowStates.Active);
            if (result == null)
            {
                result = new DAL.ProductLimit
                {
                    ObjectId = limit.ObjectId,
                    ObjectTypeId = limit.ObjectTypeId,
                    StartTime = limit.StartTime,
                    EndTime = limit.EndTime,
                    LimitTypeId = (int)LimitTypes.SelfExclusionLimit,
                    MaxLimit = limit.MaxLimit,
                    RowState = (int)LimitRowStates.Active
                };
                Db.ProductLimits.Add(result);
            }
            else
            {
                throw CreateException(LanguageId, Constants.Errors.GeneralException);
            }
            Db.SaveChanges();
            return new DAL.Models.ProductLimit
            {
                Id = result.Id,
                ObjectTypeId = result.ObjectTypeId,
                ObjectId = result.ObjectId,
                MaxLimit = limit.MaxLimit
            };
        }

        public PaymentLimit CreatePaymentLimitByClient(PaymentLimit paymentLimit)
        {
            var limit = Db.PaymentLimits.FirstOrDefault(x => x.ClientId == paymentLimit.ClientId && x.LimitTypeId == (int)LimitTypes.SelfExclusionLimit && x.RowState == (int)LimitRowStates.Active);
            if (limit == null)
            {
                limit = new PaymentLimit
                {
                    ClientId = paymentLimit.ClientId,
                    MaxDepositAmount = paymentLimit.MaxDepositAmount,
                    StartTime = paymentLimit.StartTime,
                    EndTime = paymentLimit.EndTime,
                    RowState = (int)LimitRowStates.Active
                };
                Db.PaymentLimits.Add(limit);
            }
            Db.SaveChanges();
            return new PaymentLimit();
        }
        #endregion

        public List<ClientSetting> GetClientsSettings(int clientId, bool checkPermission)
        {
            var client = CacheManager.GetClientById(clientId);
            if (checkPermission)
            {
                var clientAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewClient,
                    ObjectTypeId = (int)ObjectTypes.Client
                });
                var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
                {
                    Permission = Constants.Permissions.ViewPartner,
                    ObjectTypeId = (int)ObjectTypes.Partner
                });
                if ((!clientAccess.HaveAccessForAllObjects && clientAccess.AccessibleObjects.All(x => x != clientId)) ||
                    (!partnerAccess.HaveAccessForAllObjects && partnerAccess.AccessibleObjects.All(x => x != client.PartnerId)))
                    throw CreateException(LanguageId, Constants.Errors.DontHavePermission);
            }
            var currentDate = DateTime.UtcNow;
            var partner = CacheManager.GetPartnerById(client.PartnerId);
            var clientSettings = Db.ClientSettings.Where(x => x.ClientId == clientId).ToList();
            var resultnames = clientSettings.Select(x => x.Name).ToList();
            var documents = GetClientIdentities(clientId);
            var settingsNames = typeof(Constants.ClientSettings).GetFields().Where(x => !resultnames.Contains(x.Name)).Select(x => x.Name).ToList();
            clientSettings.AddRange(settingsNames.Select(x => new ClientSetting { Name = x, StringValue = string.Empty }).ToList());
            clientSettings.Add(new ClientSetting { Name = "Excluded", StringValue = (client.State == (int)ClientStates.ForceBlock || 
                                                                                     client.State == (int)ClientStates.FullBlocked || 
                                                                                     client.State == (int)ClientStates.Disabled ) ? "1" : "0" });
            clientSettings.Add(new ClientSetting { Name = "DocumentVerified", StringValue = (client.IsDocumentVerified) ? "1" : "0" });
            clientSettings.Add(new ClientSetting
            {
                Name = "DocumentExpired",
                StringValue = (!documents.Any(x => x.Status == (int)KYCDocumentStates.Approved) &&
                documents.Any(x => x.Status == (int)KYCDocumentStates.Expired)) ? "1" : "0"
            });
            clientSettings.Add(new ClientSetting
            {
                Name = "Younger",
                StringValue = (client.BirthDate != DateTime.MinValue &&
                    (currentDate.Year - client.BirthDate.Year < partner.ClientMinAge ||
                    (currentDate.Year - client.BirthDate.Year == partner.ClientMinAge && currentDate.Month - client.BirthDate.Month < 0) ||
                    (currentDate.Year - client.BirthDate.Year == partner.ClientMinAge && currentDate.Month == client.BirthDate.Month && currentDate.Day < client.BirthDate.Day))) ? "1" : "0"
            });
            return clientSettings;
        }

        public List<ClientSetting> UpdateClientSettings(int clientId, List<ClientSetting> input)
        {
            var currentTime = DateTime.UtcNow;
            var client = CacheManager.GetClientById(clientId);
            if (client == null)
                throw CreateException(LanguageId, Constants.Errors.ClientNotFound);
            foreach (var setting in input)
            {
                var dbSetting = Db.ClientSettings.Where(x => x.ClientId == clientId && x.Name == setting.Name).FirstOrDefault();
                var initialValue = string.Empty;
                if (dbSetting != null)
                {
                    initialValue = dbSetting.StringValue;
                    dbSetting.StringValue = setting.StringValue;
                    dbSetting.NumericValue = setting.NumericValue;
                }
                else
                {
                    dbSetting = new ClientSetting
                    {
                        ClientId = clientId,
                        Name = setting.Name,
                        StringValue = setting.StringValue,
                        NumericValue = setting.NumericValue,
                        CreationTime = currentTime,
                        LastUpdateTime = currentTime
                    };
                    Db.ClientSettings.Add(dbSetting);
                }

                if (setting.Name == ClientSettings.CautionSuspension && setting.StringValue == "1" && initialValue != "1")
                    LogoutClientById(clientId, (int)LogoutTypes.Admin);
            }
            var oldSettings = GetClientsSettings(client.Id, false).Select(x => new
            {
                x.Name,
                StringValue = string.IsNullOrEmpty(x.StringValue) ?
                               (x.NumericValue.HasValue ? x.NumericValue.Value.ToString() : String.Empty) : x.StringValue,
                DateValue = x.DateValue == null ? x.CreationTime : x.DateValue,
                LastUpdateTime = x.LastUpdateTime
            }).ToList();
            SaveChangesWithHistory((int)ObjectTypes.ClientSetting, client.Id, JsonConvert.SerializeObject(oldSettings), string.Empty);

            return input;
        }

        public ClientCustomSettings SaveClientSetting(ClientCustomSettings clientCustomSettings)
        {
            var settingsNames = clientCustomSettings.GetType().GetProperties().Select(x => x.Name).ToList();
            var editingSettings = new List<string>();
            var currentTime = DateTime.UtcNow;
            var oldSettings = JsonConvert.SerializeObject(GetClientsSettings(clientCustomSettings.ClientId, false).Select(x => new
            {
                Name = x.Name,
                StringValue = string.IsNullOrEmpty(x.StringValue) ?
                                              (x.NumericValue.HasValue ? x.NumericValue.Value.ToString() : String.Empty) : x.StringValue,
                DateValue = x.DateValue ?? x.CreationTime,
                LastUpdateTime = x.LastUpdateTime
            }).ToList());

            var dbSettings = Db.ClientSettings.Where(x => settingsNames.Contains(x.Name) &&
                                                          x.ClientId == clientCustomSettings.ClientId).ToList();
            if (clientCustomSettings.AllowDoubleCommission.HasValue)
            {
                var allowDoubleCommission = dbSettings.FirstOrDefault(x => x.Name == nameof(clientCustomSettings.AllowDoubleCommission));
                editingSettings.Add(nameof(clientCustomSettings.AllowDoubleCommission));
                if (allowDoubleCommission != null)
                {
                    allowDoubleCommission.NumericValue = clientCustomSettings.AllowDoubleCommission.Value ? 1 : 0;
                    allowDoubleCommission.LastUpdateTime = currentTime;
                }
                else
                    Db.ClientSettings.Add(new ClientSetting
                    {
                        ClientId = clientCustomSettings.ClientId,
                        Name = nameof(clientCustomSettings.AllowDoubleCommission),
                        NumericValue = Convert.ToDecimal(clientCustomSettings.AllowDoubleCommission),
                        CreationTime = currentTime,
                        LastUpdateTime = currentTime
                    });
            }
            if (clientCustomSettings.AllowOutright.HasValue)
            {
                var allowOutright = dbSettings.FirstOrDefault(x => x.Name == nameof(clientCustomSettings.AllowOutright));
                editingSettings.Add(nameof(clientCustomSettings.AllowOutright));
                if (allowOutright != null)
                {
                    allowOutright.NumericValue = clientCustomSettings.AllowOutright.Value ? 1 : 0;
                    allowOutright.LastUpdateTime = currentTime;
                }
                else
                {
                    Db.ClientSettings.Add(new ClientSetting
                    {
                        ClientId = clientCustomSettings.ClientId,
                        Name = nameof(clientCustomSettings.AllowOutright),
                        NumericValue = Convert.ToDecimal(clientCustomSettings.AllowOutright),
                        CreationTime = currentTime,
                        LastUpdateTime = currentTime
                    });
                }
            }
            if (clientCustomSettings.ParentState.HasValue)
            {
                var ParentState = dbSettings.FirstOrDefault(x => x.Name == nameof(clientCustomSettings.ParentState));
                editingSettings.Add(nameof(clientCustomSettings.ParentState));
                if (ParentState != null)
                {
                    ParentState.NumericValue = clientCustomSettings.ParentState.Value;
                    ParentState.LastUpdateTime = currentTime;
                }
                else
                {
                    Db.ClientSettings.Add(new ClientSetting
                    {
                        ClientId = clientCustomSettings.ClientId,
                        Name = nameof(clientCustomSettings.ParentState),
                        NumericValue = Convert.ToDecimal(clientCustomSettings.ParentState),
                        CreationTime = currentTime,
                        LastUpdateTime = currentTime
                    });
                }
            }
            if (clientCustomSettings.MaxCredit.HasValue)
            {
                var maxCredit = dbSettings.FirstOrDefault(x => x.Name == nameof(clientCustomSettings.MaxCredit));
                if (maxCredit != null)
                {
                    maxCredit.NumericValue = clientCustomSettings.MaxCredit;
                    maxCredit.LastUpdateTime = currentTime;
                    editingSettings.Add(nameof(clientCustomSettings.MaxCredit));
                }
                else
                    Db.ClientSettings.Add(new ClientSetting
                    {
                        ClientId = clientCustomSettings.ClientId,
                        Name = nameof(clientCustomSettings.MaxCredit),
                        NumericValue = clientCustomSettings.MaxCredit,
                        CreationTime = currentTime,
                        LastUpdateTime = currentTime
                    });
            }
            if (!string.IsNullOrEmpty(clientCustomSettings.TermsConditionsAcceptanceVersion))
            {
                var currentDate = currentTime.Year * 100000000 + currentTime.Month * 1000000 + currentTime.Day * 10000 + currentTime.Hour * 100 + currentTime.Minute;
                var termsConditionsAcceptanceVersion = dbSettings.FirstOrDefault(x => x.Name == nameof(clientCustomSettings.TermsConditionsAcceptanceVersion));
                editingSettings.Add(nameof(clientCustomSettings.TermsConditionsAcceptanceVersion));
                if (termsConditionsAcceptanceVersion != null)
                {
                    termsConditionsAcceptanceVersion.StringValue = clientCustomSettings.TermsConditionsAcceptanceVersion;
                    termsConditionsAcceptanceVersion.NumericValue = currentDate;
                    termsConditionsAcceptanceVersion.LastUpdateTime = currentTime;
                    termsConditionsAcceptanceVersion.DateValue = currentTime;
                }
                else
                    Db.ClientSettings.Add(new ClientSetting
                    {
                        ClientId = clientCustomSettings.ClientId,
                        Name = nameof(clientCustomSettings.TermsConditionsAcceptanceVersion),
                        StringValue = clientCustomSettings.TermsConditionsAcceptanceVersion,
                        NumericValue = currentDate,
                        DateValue = currentTime,
                        CreationTime = currentTime,
                        LastUpdateTime = currentTime
                    });
            }
            if (clientCustomSettings.PasswordChangedDate.HasValue)
            {
                var passwordChangedDate = dbSettings.FirstOrDefault(x => x.Name == nameof(clientCustomSettings.PasswordChangedDate));
                editingSettings.Add(nameof(clientCustomSettings.PasswordChangedDate));
                if (passwordChangedDate != null)
                {
                    passwordChangedDate.NumericValue = clientCustomSettings.PasswordChangedDate;
                    passwordChangedDate.DateValue = new DateTime((int)(clientCustomSettings.PasswordChangedDate.Value / 10000),
                        (int)(clientCustomSettings.PasswordChangedDate.Value / 100) % 100, (int)clientCustomSettings.PasswordChangedDate.Value % 100);
                    passwordChangedDate.LastUpdateTime = currentTime;
                }
                else
                    Db.ClientSettings.Add(new ClientSetting
                    {
                        ClientId = clientCustomSettings.ClientId,
                        Name = nameof(clientCustomSettings.PasswordChangedDate),
                        NumericValue = clientCustomSettings.PasswordChangedDate,
                        DateValue = new DateTime((int)(clientCustomSettings.PasswordChangedDate.Value / 10000),
                            (int)(clientCustomSettings.PasswordChangedDate.Value / 100) % 100, (int)clientCustomSettings.PasswordChangedDate.Value % 100),
                        CreationTime = currentTime,
                        LastUpdateTime = currentTime
                    });
            }
            if (clientCustomSettings.ReferralType.HasValue)
            {
                var referralType = dbSettings.FirstOrDefault(x => x.Name == nameof(clientCustomSettings.ReferralType));
                editingSettings.Add(nameof(clientCustomSettings.ReferralType));
                if (referralType != null)
                {
                    referralType.NumericValue = clientCustomSettings.ReferralType;
                    referralType.DateValue = currentTime;
                    referralType.LastUpdateTime = currentTime;
                }
                else
                    Db.ClientSettings.Add(new ClientSetting
                    {
                        ClientId = clientCustomSettings.ClientId,
                        Name = nameof(clientCustomSettings.ReferralType),
                        NumericValue = clientCustomSettings.ReferralType,
                        DateValue = currentTime,
                        CreationTime = currentTime,
                        LastUpdateTime = currentTime
                    });
            }
            SaveChangesWithHistory((int)ObjectTypes.ClientSetting, clientCustomSettings.ClientId, oldSettings, string.Empty);
            foreach (var setting in editingSettings)
                CacheManager.RemoveClientSetting(clientCustomSettings.ClientId, setting);
            return clientCustomSettings;
        }

        public ClientCustomSettings SaveClientLimitSettings(ClientCustomSettings clientCustomSettings, int? userId)
        {
            var settingsNames = clientCustomSettings.GetType().GetProperties().Select(x => x.Name).ToList();
            var editingSettings = new List<string>();
            var currentTime = DateTime.UtcNow;
            var dbSettings = Db.ClientSettings.Where(x => settingsNames.Contains(x.Name) &&
                                              x.ClientId == clientCustomSettings.ClientId).ToList();
            if (userId == null)
            {
                CheckInputLimits(clientCustomSettings.DepositLimitDaily, clientCustomSettings.DepositLimitWeekly, clientCustomSettings.DepositLimitMonthly);
                CheckInputLimits(clientCustomSettings.TotalBetAmountLimitDaily, clientCustomSettings.TotalBetAmountLimitWeekly, clientCustomSettings.TotalBetAmountLimitMonthly);
                CheckInputLimits(clientCustomSettings.TotalLossLimitDaily, clientCustomSettings.TotalLossLimitWeekly, clientCustomSettings.TotalLossLimitMonthly);

                SetSetting(dbSettings, clientCustomSettings.ClientId, nameof(clientCustomSettings.DepositLimitDaily), clientCustomSettings.DepositLimitDaily, userId);
                editingSettings.Add(nameof(clientCustomSettings.DepositLimitDaily));

                SetSetting(dbSettings, clientCustomSettings.ClientId, nameof(clientCustomSettings.DepositLimitWeekly), clientCustomSettings.DepositLimitWeekly, userId);
                editingSettings.Add(nameof(clientCustomSettings.DepositLimitWeekly));

                SetSetting(dbSettings, clientCustomSettings.ClientId, nameof(clientCustomSettings.DepositLimitMonthly), clientCustomSettings.DepositLimitMonthly, userId);
                editingSettings.Add(nameof(clientCustomSettings.DepositLimitMonthly));

                SetSetting(dbSettings, clientCustomSettings.ClientId, nameof(clientCustomSettings.TotalBetAmountLimitDaily), clientCustomSettings.TotalBetAmountLimitDaily, userId);
                editingSettings.Add(nameof(clientCustomSettings.TotalBetAmountLimitDaily));

                SetSetting(dbSettings, clientCustomSettings.ClientId, nameof(clientCustomSettings.TotalBetAmountLimitWeekly), clientCustomSettings.TotalBetAmountLimitWeekly, userId);
                editingSettings.Add(nameof(clientCustomSettings.TotalBetAmountLimitWeekly));

                SetSetting(dbSettings, clientCustomSettings.ClientId, nameof(clientCustomSettings.TotalBetAmountLimitMonthly), clientCustomSettings.TotalBetAmountLimitMonthly, userId);
                editingSettings.Add(nameof(clientCustomSettings.TotalBetAmountLimitMonthly));

                SetSetting(dbSettings, clientCustomSettings.ClientId, nameof(clientCustomSettings.TotalLossLimitDaily), clientCustomSettings.TotalLossLimitDaily, userId);
                editingSettings.Add(nameof(clientCustomSettings.TotalLossLimitDaily));

                SetSetting(dbSettings, clientCustomSettings.ClientId, nameof(clientCustomSettings.TotalLossLimitWeekly), clientCustomSettings.TotalLossLimitWeekly, userId);
                editingSettings.Add(nameof(clientCustomSettings.TotalLossLimitWeekly));

                SetSetting(dbSettings, clientCustomSettings.ClientId, nameof(clientCustomSettings.TotalLossLimitMonthly), clientCustomSettings.TotalLossLimitMonthly, userId);
                editingSettings.Add(nameof(clientCustomSettings.TotalLossLimitMonthly));

                SetSetting(dbSettings, clientCustomSettings.ClientId, nameof(clientCustomSettings.SessionLimit), clientCustomSettings.SessionLimit, userId);
                editingSettings.Add(nameof(clientCustomSettings.SessionLimit));
            }
            else
            {
                SetSetting(dbSettings, clientCustomSettings.ClientId, nameof(clientCustomSettings.SystemSessionLimit), clientCustomSettings.SystemSessionLimit, userId);
                editingSettings.Add(nameof(clientCustomSettings.SystemSessionLimit));

                SetSetting(dbSettings, clientCustomSettings.ClientId, nameof(clientCustomSettings.SystemDepositLimitDaily), clientCustomSettings.SystemDepositLimitDaily, userId);
                editingSettings.Add(nameof(clientCustomSettings.SystemDepositLimitDaily));

                SetSetting(dbSettings, clientCustomSettings.ClientId, nameof(clientCustomSettings.SystemDepositLimitWeekly), clientCustomSettings.SystemDepositLimitWeekly, userId);
                editingSettings.Add(nameof(clientCustomSettings.SystemDepositLimitWeekly));

                SetSetting(dbSettings, clientCustomSettings.ClientId, nameof(clientCustomSettings.SystemDepositLimitMonthly), clientCustomSettings.SystemDepositLimitMonthly, userId);
                editingSettings.Add(nameof(clientCustomSettings.SystemDepositLimitMonthly));

                SetSetting(dbSettings, clientCustomSettings.ClientId, nameof(clientCustomSettings.SystemTotalBetAmountLimitDaily), clientCustomSettings.SystemTotalBetAmountLimitDaily, userId);
                editingSettings.Add(nameof(clientCustomSettings.SystemTotalBetAmountLimitDaily));

                SetSetting(dbSettings, clientCustomSettings.ClientId, nameof(clientCustomSettings.SystemTotalBetAmountLimitWeekly), clientCustomSettings.SystemTotalBetAmountLimitWeekly, userId);
                editingSettings.Add(nameof(clientCustomSettings.SystemTotalBetAmountLimitWeekly));

                SetSetting(dbSettings, clientCustomSettings.ClientId, nameof(clientCustomSettings.SystemTotalBetAmountLimitMonthly), clientCustomSettings.SystemTotalBetAmountLimitMonthly, userId);
                editingSettings.Add(nameof(clientCustomSettings.SystemTotalBetAmountLimitMonthly));

                SetSetting(dbSettings, clientCustomSettings.ClientId, nameof(clientCustomSettings.SystemTotalLossLimitDaily), clientCustomSettings.SystemTotalLossLimitDaily, userId);
                editingSettings.Add(nameof(clientCustomSettings.SystemTotalLossLimitDaily));

                SetSetting(dbSettings, clientCustomSettings.ClientId, nameof(clientCustomSettings.SystemTotalLossLimitWeekly), clientCustomSettings.SystemTotalLossLimitWeekly, userId);
                editingSettings.Add(nameof(clientCustomSettings.SystemTotalLossLimitWeekly));

                SetSetting(dbSettings, clientCustomSettings.ClientId, nameof(clientCustomSettings.SystemTotalLossLimitMonthly), clientCustomSettings.SystemTotalLossLimitMonthly, userId);
                editingSettings.Add(nameof(clientCustomSettings.SystemTotalLossLimitMonthly));
            }
            Db.SaveChanges();
            foreach (var setting in editingSettings)
                CacheManager.RemoveClientSetting(clientCustomSettings.ClientId, setting);

            var client = CacheManager.GetClientById(clientCustomSettings.ClientId);
            var partner = CacheManager.GetPartnerById(client.PartnerId);

            var messageTemplate = Db.fn_MessageTemplate(client.LanguageId).Where(y => y.PartnerId == client.PartnerId &&
                              y.ClientInfoType == (int)ClientInfoTypes.ClientLimit).FirstOrDefault();
            if (messageTemplate != null)
            {
                var messageTextTemplate = messageTemplate.Text.Replace("\\n", Environment.NewLine)
                                                           .Replace("{u}", client.UserName)
                                                           .Replace("{w}", partner.SiteUrl.Split(',')[0])
                                                           .Replace("{pc}", client.Id.ToString())
                                                           .Replace("{fn}", client.FirstName)
                                                           .Replace("{e}", client.Email)
                                                           .Replace("{m}", client.MobileNumber)
                                                           .Replace("{dld}", clientCustomSettings.DepositLimitDaily.HasValue ? clientCustomSettings.DepositLimitDaily.Value.ToString() : "-")
                                                           .Replace("{dlw}", clientCustomSettings.DepositLimitWeekly.HasValue ? clientCustomSettings.DepositLimitWeekly.Value.ToString() : "-")
                                                           .Replace("{dlm}", clientCustomSettings.DepositLimitMonthly.HasValue ? clientCustomSettings.DepositLimitMonthly.Value.ToString() : "-")
                                                           .Replace("{tbald}", clientCustomSettings.TotalBetAmountLimitDaily.HasValue ? clientCustomSettings.TotalBetAmountLimitDaily.Value.ToString() : "-")
                                                           .Replace("{tbalw}", clientCustomSettings.TotalBetAmountLimitWeekly.HasValue ? clientCustomSettings.TotalBetAmountLimitWeekly.Value.ToString() : "-")
                                                           .Replace("{tbalm}", clientCustomSettings.TotalBetAmountLimitMonthly.HasValue ? clientCustomSettings.TotalBetAmountLimitMonthly.Value.ToString() : "-")
                                                           .Replace("{tlld}", clientCustomSettings.TotalLossLimitDaily.HasValue ? clientCustomSettings.TotalLossLimitDaily.ToString() : "-")
                                                           .Replace("{tllw}", clientCustomSettings.TotalLossLimitWeekly.HasValue ? clientCustomSettings.TotalLossLimitWeekly.ToString() : "-")
                                                           .Replace("{tllm}", clientCustomSettings.TotalLossLimitMonthly.HasValue ? clientCustomSettings.TotalLossLimitMonthly.ToString() : "-")
                                                           .Replace("{sl}", clientCustomSettings.SessionLimit.HasValue ? clientCustomSettings.SessionLimit.ToString() : "-")
                                                           .Replace("{sdld}", clientCustomSettings.SystemDepositLimitDaily.HasValue ? clientCustomSettings.SystemDepositLimitDaily.Value.ToString() : "-")
                                                           .Replace("{sdlw}", clientCustomSettings.SystemDepositLimitWeekly.HasValue ? clientCustomSettings.SystemDepositLimitWeekly.Value.ToString() : "-")
                                                           .Replace("{sdlm}", clientCustomSettings.SystemDepositLimitMonthly.HasValue ? clientCustomSettings.SystemDepositLimitMonthly.Value.ToString() : "-")
                                                           .Replace("{stbald}", clientCustomSettings.SystemTotalBetAmountLimitDaily.HasValue ? clientCustomSettings.SystemTotalBetAmountLimitDaily.Value.ToString() : "-")
                                                           .Replace("{stbalw}", clientCustomSettings.SystemTotalBetAmountLimitWeekly.HasValue ? clientCustomSettings.SystemTotalBetAmountLimitWeekly.Value.ToString() : "-")
                                                           .Replace("{stbalm}", clientCustomSettings.SystemTotalBetAmountLimitMonthly.HasValue ? clientCustomSettings.SystemTotalBetAmountLimitMonthly.Value.ToString() : "-")
                                                           .Replace("{stlld}", clientCustomSettings.SystemTotalLossLimitDaily.HasValue ? clientCustomSettings.SystemTotalLossLimitDaily.ToString() : "-")
                                                           .Replace("{stllw}", clientCustomSettings.SystemTotalLossLimitWeekly.HasValue ? clientCustomSettings.SystemTotalLossLimitWeekly.ToString() : "-")
                                                           .Replace("{stllm}", clientCustomSettings.SystemTotalLossLimitMonthly.HasValue ? clientCustomSettings.SystemTotalLossLimitMonthly.ToString() : "-")
                                                           .Replace("{ssl}", clientCustomSettings.SessionLimit.HasValue ? clientCustomSettings.SystemSessionLimit.ToString() : "-");

                using var notificationBl = new NotificationBll(this);
                notificationBl.SaveEmailMessage(client.PartnerId, client.Id, client.Email, partner.Name, messageTextTemplate, messageTemplate.Id);
            }
            return clientCustomSettings;
        }

        public ClientCustomSettings GetClientLimitSettings(int? userId)
        {
            var editingSettings = new List<string>();
            var currentTime = DateTime.UtcNow;
            var dbSettings = Db.ClientSettings.Where(x => x.ClientId == Identity.Id).ToList();
            var sessionLimit = dbSettings.FirstOrDefault(x => x.Name == "SessionLimit")?.NumericValue;
            var systemSessionLimit = dbSettings.FirstOrDefault(x => x.Name == "SystemSessionLimit")?.NumericValue;
            var resp = new ClientCustomSettings
            {
                DepositLimitDaily = dbSettings.FirstOrDefault(x => x.Name == "DepositLimitDaily")?.NumericValue,
                DepositLimitWeekly = dbSettings.FirstOrDefault(x => x.Name == "DepositLimitWeekly")?.NumericValue,
                DepositLimitMonthly = dbSettings.FirstOrDefault(x => x.Name == "DepositLimitMonthly")?.NumericValue,
                TotalBetAmountLimitDaily = dbSettings.FirstOrDefault(x => x.Name == "TotalBetAmountLimitDaily")?.NumericValue,
                TotalBetAmountLimitWeekly = dbSettings.FirstOrDefault(x => x.Name == "TotalBetAmountLimitWeekly")?.NumericValue,
                TotalBetAmountLimitMonthly = dbSettings.FirstOrDefault(x => x.Name == "TotalBetAmountLimitMonthly")?.NumericValue,
                TotalLossLimitDaily = dbSettings.FirstOrDefault(x => x.Name == "TotalLossLimitDaily")?.NumericValue,
                TotalLossLimitWeekly = dbSettings.FirstOrDefault(x => x.Name == "TotalLossLimitWeekly")?.NumericValue,
                TotalLossLimitMonthly = dbSettings.FirstOrDefault(x => x.Name == "TotalLossLimitMonthly")?.NumericValue,
                SessionLimit = sessionLimit == null ? (int?)null : Convert.ToInt32(sessionLimit.Value)
            };
            if (userId != null)
            {
                resp.SystemDepositLimitDaily = dbSettings.FirstOrDefault(x => x.Name == "SystemDepositLimitDaily")?.NumericValue;
                resp.SystemDepositLimitWeekly = dbSettings.FirstOrDefault(x => x.Name == "SystemDepositLimitWeekly")?.NumericValue;
                resp.SystemDepositLimitMonthly = dbSettings.FirstOrDefault(x => x.Name == "SystemDepositLimitMonthly")?.NumericValue;
                resp.SystemTotalBetAmountLimitDaily = dbSettings.FirstOrDefault(x => x.Name == "SystemTotalBetAmountLimitDaily")?.NumericValue;
                resp.SystemTotalBetAmountLimitWeekly = dbSettings.FirstOrDefault(x => x.Name == "SystemTotalBetAmountLimitWeekly")?.NumericValue;
                resp.SystemTotalBetAmountLimitMonthly = dbSettings.FirstOrDefault(x => x.Name == "SystemTotalBetAmountLimitMonthly")?.NumericValue;
                resp.SystemTotalLossLimitDaily = dbSettings.FirstOrDefault(x => x.Name == "SystemTotalLossLimitDaily")?.NumericValue;
                resp.SystemTotalLossLimitWeekly = dbSettings.FirstOrDefault(x => x.Name == "SystemTotalLossLimitWeekly")?.NumericValue;
                resp.SystemTotalLossLimitMonthly = dbSettings.FirstOrDefault(x => x.Name == "SystemTotalLossLimitMonthly")?.NumericValue;
                resp.SystemSessionLimit = systemSessionLimit == null ? (int?)null : Convert.ToInt32(systemSessionLimit.Value);

                var client = CacheManager.GetClientById(Identity.Id);
                var partnerSetting = CacheManager.GetConfigKey(client.PartnerId, Constants.PartnerKeys.SelfExclusionPeriod);
                if (int.TryParse(partnerSetting, out int selfExclusionPeriod))
                    resp.SelfExclusionPeriod = selfExclusionPeriod;
                else
                    resp.SelfExclusionPeriod = 1;
            }
            return resp;
        }

        private void CheckInputLimits(decimal? dailyLimit, decimal? weeklyLimit, decimal? monthlyLimit)
        {
            if (dailyLimit.HasValue)
            {
                if (weeklyLimit.HasValue && (weeklyLimit.Value < 0 || dailyLimit.Value < 0 || weeklyLimit.Value < dailyLimit.Value))
                    throw CreateException(LanguageId, Errors.WrongInputParameters);
                if (monthlyLimit.HasValue && (monthlyLimit.Value < 0 || dailyLimit.Value < 0 || monthlyLimit.Value < dailyLimit.Value))
                    throw CreateException(LanguageId, Errors.WrongInputParameters);
            }
            if (weeklyLimit.HasValue && monthlyLimit.HasValue && (monthlyLimit.Value < 0 || weeklyLimit.Value < 0 ||
                monthlyLimit.Value < weeklyLimit.Value))
                throw CreateException(LanguageId, Errors.WrongInputParameters);
        }
        private void SetSetting(List<ClientSetting> dbSettings, int clientId, string settingName, decimal? settingValue, int? userId)
        {
            var currentTime = DateTime.UtcNow;
            if (settingValue != null)
            {
                if (settingName.Contains(PeriodsOfTime.Daily.ToString()))
                {
                    var weeklyLimit = dbSettings.FirstOrDefault(x => x.Name == settingName.Replace(PeriodsOfTime.Daily.ToString(), PeriodsOfTime.Weekly.ToString()));
                    if (weeklyLimit != null && weeklyLimit.NumericValue < settingValue)
                        throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);

                    var monthlyLimit = dbSettings.FirstOrDefault(x => x.Name == settingName.Replace(PeriodsOfTime.Daily.ToString(), PeriodsOfTime.Monthly.ToString()));
                    if (monthlyLimit != null && monthlyLimit.NumericValue < settingValue)
                        throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);
                }
                else if (settingName.Contains(PeriodsOfTime.Weekly.ToString()))
                {
                    var dailyLimit = dbSettings.FirstOrDefault(x => x.Name == settingName.Replace(PeriodsOfTime.Weekly.ToString(), PeriodsOfTime.Daily.ToString()));
                    if (dailyLimit != null && dailyLimit.NumericValue > settingValue)
                        throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);

                    var monthlyLimit = dbSettings.FirstOrDefault(x => x.Name == settingName.Replace(PeriodsOfTime.Weekly.ToString(), PeriodsOfTime.Monthly.ToString()));
                    if (monthlyLimit != null && monthlyLimit.NumericValue < settingValue)
                        throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);
                }
                else if (settingName.Contains(PeriodsOfTime.Monthly.ToString()))
                {
                    var dailyLimit = dbSettings.FirstOrDefault(x => x.Name == settingName.Replace(PeriodsOfTime.Monthly.ToString(), PeriodsOfTime.Daily.ToString()));
                    if (dailyLimit != null && dailyLimit.NumericValue > settingValue)
                        throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);

                    var weeklyLimit = dbSettings.FirstOrDefault(x => x.Name == settingName.Replace(PeriodsOfTime.Monthly.ToString(), PeriodsOfTime.Weekly.ToString()));
                    if (weeklyLimit != null && weeklyLimit.NumericValue > settingValue)
                        throw CreateException(LanguageId, Constants.Errors.WrongInputParameters);
                }
            }
            var setting = dbSettings.FirstOrDefault(x => x.Name == settingName);
            if (setting != null)
            {
                if (userId == null && (currentTime - (setting.LastUpdateTime ?? currentTime.AddDays(-2))).TotalHours < 24)
                    throw CreateException(LanguageId, Constants.Errors.NotAllowed);
                setting.NumericValue = settingValue;
                setting.LastUpdateTime = currentTime;
            }
            else
                Db.ClientSettings.Add(new ClientSetting
                {
                    ClientId = clientId,
                    Name = settingName,
                    NumericValue = settingValue,
                    UserId = userId,
                    CreationTime = currentTime,
                    LastUpdateTime = currentTime
                });
        }

        public string AddOrUpdateClientSetting(int clientId, string settingName, decimal? numericValue, string stringValue, DateTime? dateValue, int? userId, string comment)
        {
            var currentTime = DateTime.UtcNow;
            var setting = Db.ClientSettings.FirstOrDefault(x => x.ClientId == clientId && x.Name == settingName);
            if (setting != null)
            {
                var oldSettings = JsonConvert.SerializeObject(GetClientsSettings(clientId, false).Select(x => new
                {
                    Name = x.Name,
                    StringValue = string.IsNullOrEmpty(x.StringValue) ?
                                                (x.NumericValue.HasValue ? x.NumericValue.Value.ToString() : String.Empty) : x.StringValue,
                    DateValue = x.DateValue ?? x.CreationTime,
                    LastUpdateTime = x.LastUpdateTime
                }).ToList());
                if (setting.NumericValue != numericValue || setting.StringValue != stringValue || setting.DateValue != dateValue)
                {
                    setting.NumericValue = numericValue;
                    setting.StringValue = stringValue;
                    setting.DateValue = dateValue;
                    setting.UserId = userId;
                    setting.LastUpdateTime = currentTime;
                    SaveChangesWithHistory((int)ObjectTypes.ClientSetting, clientId, oldSettings, comment);
                }
            }
            else
            {
                setting = new ClientSetting
                {
                    ClientId = clientId,
                    Name = settingName,
                    NumericValue = numericValue,
                    StringValue = stringValue,
                    DateValue = dateValue,
                    UserId = userId,
                    CreationTime = currentTime,
                    LastUpdateTime = currentTime
                };
                Db.ClientSettings.Add(setting);
                Db.SaveChanges();
            }

            return setting.StringValue;
        }

        public List<UserBalance> GetClientsAccountsInfo(int userId)
        {
            using var userBl = new UserBll(this);
            var result = new List<UserBalance>();
            var clients = Db.Clients.Include(x => x.ClientSessions).Where(x => x.UserId == userId).ToList();
            var parentAvailablBlance = new UserAccount();
            var parentUser = CacheManager.GetUserById(userId);
            if (parentUser.Type != (int)UserTypes.AdminUser)
                parentAvailablBlance = userBl.GetUserBalance(userId);
            clients.ForEach(x =>
            {
                var clientBalance = BaseBll.GetObjectBalance((int)ObjectTypes.Client, x.Id).AvailableBalance;
                var mc = CacheManager.GetClientSettingByName(x.Id, ClientSettings.MaxCredit);
                var ss = CacheManager.GetClientSettingByName(x.Id, ClientSettings.ParentState);
                var state = x.State;
                if (ss.NumericValue.HasValue && CustomHelper.Greater((ClientStates)ss.NumericValue, (ClientStates)state))
                    state = Convert.ToInt32(ss.NumericValue.Value);
                DateTime? lastLoginDate = null;
                if (x.ClientSessions.Any())
                    lastLoginDate = x.ClientSessions.First().StartTime;
                result.Add(new UserBalance
                {
                    UserId = x.Id,
                    ObjectId = x.Id,
                    ObjectTypeId = (int)ObjectTypes.Client,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    Username = x.UserName,
                    Nickname = x.NickName,
                    Status = CustomHelper.MapUserStateToClient.First(y => y.Value == state).Key,
                    CurrencyId = x.CurrencyId,
                    AvailableCredit = clientBalance,
                    ParentAvailableBalance = parentAvailablBlance.Balance,
                    Credit = Convert.ToDecimal(mc == null || mc.Id == 0 ? 0 : (mc.NumericValue ?? 0)),
                    Cash = 0,
                    AvailableCash = 0,
                    YesterdayCash = 0,
                    Outstanding = 0,
                    LastLoginDate = lastLoginDate,
                    LastLoginIp = lastLoginDate != null ? x.ClientSessions.First().Ip : null
                });
            });
            return result;
        }

        public TriggerSetting GetPromocodeTrigger(int partnerId, string promocode)
        {
            var currentTime = DateTime.UtcNow;
            return Db.TriggerSettings.FirstOrDefault(x => x.PartnerId == partnerId && x.Type == (int)TriggerTypes.PromotionalCode &&
                x.BonusSettingCodes == promocode && x.StartTime <= currentTime && x.FinishTime > currentTime);
        }

        public List<Client> FindClients(string clientIdentity, bool fullPath, int? parentId)
        {
            var user = CacheManager.GetUserById(parentId ?? Identity.Id);
            if (user == null)
                throw CreateException(LanguageId, Constants.Errors.UserNotFound);

            return fullPath ? Db.Clients.Where(x => (fullPath && x.User.Path.Contains("/" + user.Id + "/") || (fullPath && x.UserId == user.Id)) &&
                                     (x.UserName.Contains(clientIdentity) ||
                                         x.FirstName.Contains(clientIdentity) ||
                                         x.LastName.Contains(clientIdentity) ||
                                         (!string.IsNullOrEmpty(x.NickName) && x.NickName.Contains(clientIdentity)))).ToList() :
                              Db.Clients.Where(x => x.UserId == user.Id && (x.UserName.Contains(clientIdentity) ||
                                         x.FirstName.Contains(clientIdentity) ||
                                         x.LastName.Contains(clientIdentity) ||
                                         (!string.IsNullOrEmpty(x.NickName) && x.NickName.Contains(clientIdentity)))).ToList();
        }

        public void UpdateClientAMLStatus(int clientId, AMLStatus amlStatus, string comment)
        {
            var clientSetting = Db.ClientSettings.Where(x => x.ClientId == clientId).ToList();
            var currentDate = DateTime.UtcNow;
            var amlVerified = clientSetting.FirstOrDefault(x => x.Name == ClientSettings.AMLVerified);
            int amlVerifiedNew = Convert.ToInt32(amlStatus.IsVerified);
            if (amlStatus.Status == AMLStatuses.NA || amlStatus.Status == AMLStatuses.BLOCK)
                amlVerifiedNew = 1;

            if (amlVerified != null)
            {
                amlVerified.StringValue = amlVerifiedNew.ToString();
                amlVerified.NumericValue = amlVerifiedNew;
                amlVerified.LastUpdateTime = currentDate;
            }
            else
                Db.ClientSettings.Add(new ClientSetting
                {
                    Name = ClientSettings.AMLVerified,
                    StringValue = amlVerifiedNew.ToString(),
                    NumericValue = amlVerifiedNew,
                    CreationTime = currentDate,
                    LastUpdateTime = currentDate
                });
            var amlProhibited = clientSetting.FirstOrDefault(x => x.Name == ClientSettings.AMLProhibited);
            if (amlProhibited != null)
            {
                amlProhibited.StringValue = ((int)amlStatus.Status).ToString();
                amlProhibited.LastUpdateTime = currentDate;
            }
            else
                Db.ClientSettings.Add(new ClientSetting
                {
                    Name = ClientSettings.AMLProhibited,
                    StringValue = ((int)amlStatus.Status).ToString(),
                    CreationTime = currentDate,
                    LastUpdateTime = currentDate
                });
            var amlPercent = clientSetting.FirstOrDefault(x => x.Name == ClientSettings.AMLPercent);
            if (amlPercent != null)
            {
                amlPercent.NumericValue = amlStatus.Percentage;
                amlPercent.StringValue = amlStatus.Percentage.ToString();
                amlPercent.LastUpdateTime = currentDate;
            }
            else
                Db.ClientSettings.Add(new ClientSetting
                {
                    Name = ClientSettings.AMLPercent,
                    NumericValue = amlStatus.Percentage,
                    StringValue = amlStatus.Percentage.ToString(),
                    CreationTime = currentDate,
                    LastUpdateTime = currentDate
                });
            var oldSettings = GetClientsSettings(clientId, false).Select(x => new
            {
                Name = x.Name,
                StringValue = string.IsNullOrEmpty(x.StringValue) ?
                                               (x.NumericValue.HasValue ? x.NumericValue.Value.ToString() : String.Empty) : x.StringValue,
                DateValue = x.DateValue ?? x.CreationTime,
                LastUpdateTime = x.LastUpdateTime
            }).ToList();
            SaveChangesWithHistory((int)ObjectTypes.ClientSetting, clientId, JsonConvert.SerializeObject(oldSettings), comment);
        }

        public GetSessionInfoOutput GetSessionInfo(string currencyId)
        {
            var client = CacheManager.GetClientById(Identity.Id);
            var balance = GetObjectBalanceWithConvertion((int)ObjectTypes.Client, client.Id,
                string.IsNullOrEmpty(currencyId) ? client.CurrencyId : currencyId);

            var mc = CacheManager.GetClientSettingByName(client.Id, ClientSettings.MaxCredit);
            var credit = Convert.ToDecimal(mc == null || mc.Id == 0 ? 0 : (mc.NumericValue ?? 0));
            var response = new GetSessionInfoOutput
            {
                GivenCredit = credit,
                AvailableCredit = balance.AvailableBalance,
                CashBalance = balance.AvailableBalance - credit,
                OutstandingBalance = 0,
                LastLoginDate = Identity.StartTime
            };

            var clientPasswordExpiryPeriod = CacheManager.GetConfigKey(client.PartnerId, Constants.PartnerKeys.ClientPasswordExpiryPeriod);
            if (clientPasswordExpiryPeriod != null && Int32.TryParse(clientPasswordExpiryPeriod, out int period) && period > 0)
            {
                var clientPasswordChangedDate = CacheManager.GetClientSettingByName(client.Id, ClientSettings.PasswordChangedDate);
                if (clientPasswordChangedDate == null || clientPasswordChangedDate.Id == 0)
                    response.PasswordExpiryDate = DateTime.UtcNow;
                else
                {
                    var date = Convert.ToInt32(clientPasswordChangedDate.NumericValue);
                    response.PasswordExpiryDate = (new DateTime(date / 10000, (date / 100) % 100, date % 100)).AddDays(period);
                }
            }

            return response;
        }

        public PaymentLimit GetClientPaymentLimits(int clientId)
        {
            return Db.PaymentLimits.FirstOrDefault(x => x.ClientId == clientId);
        }

        public List<int> GetClientSecurityQuestions(int clientId)
        {
            return Db.ClientSecurityAnswers.Where(x => x.ClientId == clientId).Select(x => x.SecurityQuestionId).ToList();
        }

        public void UpdateClientSecurityAnswers(int clientId, ApiClientSecurityModel apiClientSecurityModel)
        {
            var client = CacheManager.GetClientById(clientId);
            if (string.IsNullOrEmpty(apiClientSecurityModel.SMSCode))
                throw CreateException(LanguageId, Constants.Errors.WrongVerificationKey);
            VerifyClientMobileNumber(apiClientSecurityModel.SMSCode, client.MobileNumber, client.Id, client.PartnerId, true, null, false);
            if (apiClientSecurityModel.SecurityQuestions == null || apiClientSecurityModel.SecurityQuestions.Count < 3)
                throw CreateException(LanguageId, Constants.Errors.WrongSecurityQuestionAnswer);
            var partnerSecurityQuestions = CacheManager.GetPartnerSecurityQuestions(client.PartnerId, LanguageId).Select(x => x.Id).ToList();
            var notChangedAnswers = apiClientSecurityModel.SecurityQuestions.Where(x => string.IsNullOrEmpty(x.Answer)).Select(x => x.Id).ToList();
            if (apiClientSecurityModel.SecurityQuestions.Any(x => !partnerSecurityQuestions.Contains(x.Id)) ||
                apiClientSecurityModel.SecurityQuestions.Count != 3 || notChangedAnswers.Count == 3)
                throw CreateException(LanguageId, Constants.Errors.WrongSecurityQuestionAnswer);
            var currentAnswers = Db.ClientSecurityAnswers.Where(x => x.ClientId == clientId).ToList();
            var currentAnswersIds = currentAnswers.Select(x => x.SecurityQuestionId).ToList();
            if (notChangedAnswers.Any(x => !currentAnswersIds.Contains(x)))
                throw CreateException(LanguageId, Constants.Errors.WrongSecurityQuestionAnswer);
            var ids = apiClientSecurityModel.SecurityQuestions.Select(x => x.Id);
            Db.ClientSecurityAnswers.Where(x => x.ClientId == clientId && !ids.Contains(x.SecurityQuestionId)).DeleteFromQuery();
            foreach (var sq in apiClientSecurityModel.SecurityQuestions)
            {
                if (string.IsNullOrEmpty(sq.Answer))
                    continue;
                var answer = currentAnswers.FirstOrDefault(x => x.SecurityQuestionId == sq.Id);
                if (answer == null)
                    Db.ClientSecurityAnswers.Add(new ClientSecurityAnswer
                    {
                        ClientId = clientId,
                        SecurityQuestionId = sq.Id,
                        Answer = sq.Answer
                    });
                else
                    answer.Answer = sq.Answer;
            }
            Db.SaveChanges();
        }

        #region PaymentSegments

        public List<ClientPaymentSegment> GetClientPaymentSegments(int clientId, int? paymentSystemId)
        {
            return new List<ClientPaymentSegment>();
        }

        public PagedModel<fnSegmentClient> GetSegmentClients(FilterfnSegmentClient filter)
        {
            var viewSegmentAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewSegment
            });
            var partnerAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewPartner,
                ObjectTypeId = (int)ObjectTypes.Partner
            });
            var clientAccess = GetPermissionsToObject(new CheckPermissionInput
            {
                Permission = Constants.Permissions.ViewClient,
                ObjectTypeId = (int)ObjectTypes.Client
            });
            filter.CheckPermissionResuts = new List<CheckPermissionOutput<fnSegmentClient>>
                {
                    new CheckPermissionOutput<fnSegmentClient>
                    {
                        AccessibleObjects = clientAccess.AccessibleObjects,
                        HaveAccessForAllObjects = clientAccess.HaveAccessForAllObjects,
                        Filter = x => clientAccess.AccessibleObjects.AsEnumerable().Contains(x.Id)
                    },
                    new CheckPermissionOutput<fnSegmentClient>
                    {
                        AccessibleObjects = partnerAccess.AccessibleObjects,
                        HaveAccessForAllObjects = partnerAccess.HaveAccessForAllObjects,
                        Filter = x => partnerAccess.AccessibleObjects.AsEnumerable().Contains(x.PartnerId)
                    }
                };
            Func<IQueryable<fnSegmentClient>, IOrderedQueryable<fnSegmentClient>> orderBy;

            if (filter.OrderBy.HasValue)
            {
                if (filter.OrderBy.Value)
                {
                    orderBy = QueryableUtilsHelper.OrderByFunc<fnSegmentClient>(filter.FieldNameToOrderBy, true);
                }
                else
                {
                    orderBy = QueryableUtilsHelper.OrderByFunc<fnSegmentClient>(filter.FieldNameToOrderBy, false);
                }
            }
            else
            {
                orderBy = clients => clients.OrderByDescending(x => x.Id);
            }

            return new PagedModel<fnSegmentClient>
            {
                Entities = filter.FilterObjects(Db.fn_SegmentClient(), orderBy),
                Count = filter.SelectedObjectsCount(Db.fn_SegmentClient())
            };
        }
        #endregion

        public void GiveComplimentaryPoint(List<Bet> bets)
        {
            var partnerGroupedList = bets.GroupBy(x => x.Client.PartnerId);
            foreach (var partnerBets in partnerGroupedList)
            {
                var partnerConfig = CacheManager.GetConfigKey(partnerBets.Key, Constants.PartnerKeys.CheckComplimentaryPoints);
                if (string.IsNullOrEmpty(partnerConfig) || partnerConfig == "0")
                    continue;

                var clientBets = partnerBets.GroupBy(x => x.ClientId).ToList();
                foreach (var cBets in clientBets)
                {
                    var betsByProduct = cBets.GroupBy(x => x.ProductId).ToList();
                    var currencyId = cBets.First().CurrencyId;
                    foreach (var b in betsByProduct)
                    {
                        var complimentaryPoint = CacheManager.GetComplimentaryPointRate(partnerBets.Key, b.Key, b.First().CurrencyId);
                        if (complimentaryPoint.Rate > 0)
                        {
                            var comp = Math.Round(b.Sum(x => x.BetAmount) / complimentaryPoint.Rate, 4);
                            if (comp >= 0)
                                AddClientJobTrigger(cBets.Key.Value, (int)JobTriggerTypes.AddComplimentaryPoint, comp);
                        }
                    }
                }
            }
        }

        public void CancelComplimentaryPoint(List<Bet> deletedBets)
        {
            var partnerGroupedList = deletedBets.GroupBy(x => x.PartnerId);
            foreach (var partnerBets in partnerGroupedList)
            {
                var partnerConfig = CacheManager.GetConfigKey(partnerBets.Key, Constants.PartnerKeys.CheckComplimentaryPoints);
                if (string.IsNullOrEmpty(partnerConfig) || partnerConfig == "0")
                    continue;

                var clientBets = partnerBets.GroupBy(x => x.ClientId).ToList();
                foreach (var cBets in clientBets)
                {
                    var betsByProduct = cBets.GroupBy(x => x.ProductId).ToList();
                    var currencyId = cBets.First().CurrencyId;
                    foreach (var b in betsByProduct)
                    {
                        var complimentaryPoint = CacheManager.GetComplimentaryPointRate(partnerBets.Key, b.Key, b.First().CurrencyId);
                        if (complimentaryPoint.Rate > 0)
                        {
                            var comp = Math.Round(b.Sum(x => x.BetAmount) / complimentaryPoint.Rate, 4);
                            if (comp >= 0)
                                AddClientJobTrigger(cBets.Key.Value, (int)JobTriggerTypes.RemoveComplimentaryPoint, comp);
                        }
                    }
                }
            }
        }

        public void IncreaseJackpot(List<Bet> bets)
        {
            /*var currentDate = DateTime.UtcNow;
            var dbJackpots = Db.Jackpots.Where(x => !x.WinnerId.HasValue && Enum.IsDefined(typeof(JackpotTypes), x.Type) &&
                                                    x.FinishTime > currentDate).ToList();
            if (dbJackpots.Any())
            {
                var productCategories = Db.Products.Where(x => x.Level < 4).ToList();
                var percentByProduct = new Dictionary<int, decimal>();
                bets.Select(x => x.ProductId).Distinct().ToList()
                    .ForEach(p =>
                    {
                        var product = Db.Products.FirstOrDefault(x => x.Id == p);
                        var prodTreeIds = product.Traverse(x => productCategories.Where(y => y.Id == x.ParentId)).Select(x => x.Id).ToList();
                        var percentByProducts = Db.JackpotSettings.Where(x => prodTreeIds.Contains(x.ProductId))
                                                        .OrderByDescending(x => x.Product.Level).FirstOrDefault()?.Percent;
                        percentByProduct.Add(p, percentByProducts / 100 ?? 0);
                    });
                foreach (var jackp in dbJackpots)
                {
                    var partner = CacheManager.GetPartnerById(jackp.PartnerId ?? Constants.MainPartnerId);
                    var partnerBets = bets.Where(x => (!jackp.PartnerId.HasValue || x.PartnerId == jackp.PartnerId) &&
                                                    percentByProduct[x.ProductId] != 0).ToList();
                    var winAmount = jackp.Type == (int)JackpotTypes.Progressive ?
                                          Convert.ToDecimal(AESEncryptHelper.DecryptString(jackp.Id.ToString(), jackp.WinAmount)) :
                                          Convert.ToDecimal(jackp.WinAmount);
                    foreach (var bet in partnerBets)
                    {
                        var betAmount = BaseBll.ConvertCurrency(bet.CurrencyId, partner.CurrencyId, bet.BetAmount) * percentByProduct[bet.ProductId];
                        if (jackp.Type == (int)JackpotTypes.Progressive)
                            jackp.Amount += betAmount;
                        else if (jackp.Type == (int)JackpotTypes.Fixed)
                            winAmount += betAmount;
                        if ((jackp.Type == (int)JackpotTypes.Progressive && winAmount <= jackp.Amount) ||
                            (jackp.Type == (int)JackpotTypes.Fixed && winAmount == jackp.Amount))
                        {
                            jackp.WinnerId = bet.ClientId;
                            var jobTrigger = new JobTrigger
                            {
                                ClientId = bet.ClientId.Value,
                                Type = (int)JobTriggerTypes.JackpotWin,
                                JackpotId = jackp.Id
                            };
                            Db.JobTriggers.AddIfNotExists(jobTrigger, x => x.ClientId == bet.ClientId.Value && x.Type == (int)JobTriggerTypes.JackpotWin &&
                                                                      x.JackpotId == jackp.Id);
                            break;
                        }
                    }
                    if (jackp.Type == (int)JackpotTypes.Fixed)
                        jackp.WinAmount = winAmount.ToString();
                    Db.SaveChanges();
                }
            }*/
        }
    }
}