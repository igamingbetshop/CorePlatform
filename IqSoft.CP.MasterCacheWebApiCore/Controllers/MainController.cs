using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Helpers;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.Common.Models;
using IqSoft.CP.Common.Models.WebSiteModels;
using IqSoft.CP.Common.Models.WebSiteModels.Bets;
using IqSoft.CP.Common.Models.WebSiteModels.Bonuses;
using IqSoft.CP.Common.Models.WebSiteModels.Clients;
using IqSoft.CP.Common.Models.WebSiteModels.Filters;
using IqSoft.CP.Common.Models.WebSiteModels.Products;
using IqSoft.CP.DAL.Filters;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.DAL.Models.Clients;
using IqSoft.CP.DAL.Models.Integration.ProductsIntegration;
using IqSoft.CP.Integration.Platforms.Helpers;
using IqSoft.CP.MasterCacheWebApi.ControllerClasses;
using IqSoft.CP.MasterCacheWebApi.Helpers;
using IqSoft.CP.MasterCacheWebApiCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.ServiceModel;
using System.Text;
using static IqSoft.CP.Common.Constants;
using IqSoft.CP.Common.Models.CacheModels;
using System.Net.Http;
using IqSoft.CP.Integration.Payments.Helpers;
using IqSoft.CP.DAL.Models.Notification;

namespace IqSoft.CP.MasterCacheWebApi.Controllers
{
    [ApiController]
    [Route("{partnerId}/api/[controller]/[action]")]
    public class MainController : BaseController
    {
        private IWebHostEnvironment HostEnvironment;
        public MainController(IWebHostEnvironment _environment)
        {
            HostEnvironment = _environment;
        }

        [HttpPost]
        public ApiResponseBase OpenGame(OpenGameInput input)
        {
            var response = new ApiResponseBase();
            string authResponse = String.Empty;
            try
            {
                Program.DbLogger.Info("OpenGame_" + JsonConvert.SerializeObject(input));
                bool isForDemo = string.IsNullOrWhiteSpace(input.Token);
                AuthorizationOutput authOutput = null;
                HttpRequestInput requestData = null;
                GetProductSessionOutput productSession = null;
                var partner = CacheManager.GetPartnerById(input.PartnerId);
                var clientSession = new SessionIdentity
                {
                    LanguageId = input.LanguageId,
                    Domain = string.IsNullOrEmpty(input.Domain) ? partner.SiteUrl.Split(',')[0] : input.Domain,
                    CurrencyId = partner.CurrencyId
                };
                int clientId = 0;
                if (String.IsNullOrEmpty(input.LanguageId))
                    input.LanguageId = Constants.DefaultLanguageId;

                var getUrlInput = new GetProductUrlInput
                {
                    PartnerId = input.PartnerId,
                    LanguageId = input.LanguageId,
                    IsForDemo = isForDemo,
                    DeviceType = input.IsForMobile ? (int)DeviceTypes.Mobile : (int)DeviceTypes.Desktop,
                    IsForMobile = input.IsForMobile,
                    ProductId = input.GameId
                };

                var product = input.GameId == Constants.SportsbookExternalId ? CheckProductAvailability(input.PartnerId, Constants.SportsbookProductId) :
                    CheckProductAvailability(input.PartnerId, input.GameId);
                var provider = CacheManager.GetGameProviderById(product.GameProviderId.Value);

                if (product.Id == Constants.SportsbookProductId)
                {
                    response.ResponseObject = InternalHelpers.GetUrl(product, partner.Id, input.Token, provider, getUrlInput, clientSession);
                    return response;
                }
                var platformToken = string.Empty;
                var token = string.Empty;
                if (!isForDemo)
                {
                    var url = string.Format(CacheManager.GetPartnerSettingByKey(input.PartnerId, Constants.PartnerKeys.ExternalPlatformUrl).StringValue,
                        "Authorization");
                    requestData = new HttpRequestInput
                    {
                        Url = url,
                        ContentType = Constants.HttpContentTypes.ApplicationJson,
                        RequestMethod = HttpMethod.Post,
                        PostData = JsonConvert.SerializeObject(new
                        {
                            input.Token,
                            input.PartnerId,
                            ProductId = product.ExternalId,
                            InternalProductId = product.Id,
                            LanguageId = input.LanguageId
                        })
                    };
                    authResponse = CommonFunctions.SendHttpRequest(requestData, out _);
                    authOutput = JsonConvert.DeserializeObject<AuthorizationOutput>(authResponse);
                    if (authOutput.ResponseCode != 0)
                        throw BaseBll.CreateException(input.LanguageId, authOutput.ResponseCode);
                    using (var clientBll = new ClientBll(clientSession, Program.DbLogger))
                    {
                        authOutput.UserName = Constants.ExternalClientPrefix + authOutput.ClientId;
                        var client = clientBll.GetClientByUserName(input.PartnerId, authOutput.UserName);
                        if (client == null)
                        {
                            client = clientBll.RegisterClient(new DAL.Client
                            {
                                CurrencyId = authOutput.CurrencyId,
                                UserName = authOutput.UserName,
                                PartnerId = input.PartnerId,
                                Gender = authOutput.Gender,
                                BirthDate = authOutput.BirthDate,
                                FirstName = authOutput.FirstName,
                                LastName = authOutput.LastName
                            });
                        }
                        if (authOutput.Token.Length < 100)
                            platformToken = authOutput.Token;
                        var session = ClientBll.CreateNewPlatformSession(client.Id, input.LanguageId, Constants.DefaultIp, null, platformToken,
                            (int)DeviceTypes.Desktop, string.Empty, String.IsNullOrEmpty(platformToken) ? authOutput.Token : null);
                        platformToken = session.Token;
                        Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.ClientSessions, client.Id));
                        Helpers.Helpers.InvokeMessage("RemoveClient", client.Id);

                        clientSession.SessionId = session.Id;
                        clientSession.Id = client.Id;
                        clientSession.LoginIp = Constants.DefaultIp;
                        clientSession.CurrencyId = client.CurrencyId;

                        productSession = ProductController.GetProductSession(input.GameId, input.IsForMobile ? (int)DeviceTypes.Mobile :
                            (int)DeviceTypes.Desktop, clientSession);

                        clientId = client.Id;
                    }
                    if (productSession != null)
                        token = productSession.ProductToken;
                    getUrlInput.ClientId = clientId;
                    getUrlInput.Token = platformToken;
                }
                Program.DbLogger.Info("GetUrlInput_" + JsonConvert.SerializeObject(getUrlInput));
                response.ResponseObject = CreateUrl(getUrlInput, product, provider, clientSession, token);
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                Program.DbLogger.Error(JsonConvert.SerializeObject(fex.Detail) + "_" + authResponse);
                response.ResponseCode = fex.Detail.Id;
                response.Description = fex.Detail.Message;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                Program.DbLogger.Error(authResponse);
                response.ResponseCode = Constants.Errors.GeneralException;
                response.Description = ex.Message;
            }
            return response;
        }

        [HttpPost]
        public ApiResponseBase GetGameReport(OpenGameInput input)
        {
            var response = new ApiResponseBase();
            try
            {
                var product = CheckProductAvailability(input.PartnerId, input.GameId);
                var provider = CacheManager.GetGameProviderById(product.GameProviderId.Value);
                response.ResponseObject = Integration.Products.Helpers.InternalHelpers.GetReportPerRound(product.Id, input.RoundId, input.LanguageId);
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                Program.DbLogger.Error(JsonConvert.SerializeObject(fex.Detail));
                response.ResponseCode = fex.Detail.Id;
                response.Description = fex.Detail.Message;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                response.ResponseCode = Constants.Errors.GeneralException;
                response.Description = ex.Message;
            }
            return response;
        }

        [HttpPost]
        public ApiResponseBase GetPartnerProductInfo(ApiProductInfoInput input)
        {
            var response = new ApiResponseBase();
            try
            {
                var session = new SessionIdentity { LanguageId = input.LanguageId, Domain = input.Domain };
                var partner = CacheManager.GetPartnerById(input.PartnerId);
                if (partner == null || !partner.SiteUrl.Split(',').Any(x => x == input.Domain))
                    throw BaseBll.CreateException(input.LanguageId, Constants.Errors.PartnerNotFound);
                if (input.ClientId.HasValue)
                    session = Helpers.Helpers.CheckToken(input.Token, input.ClientId.Value, input.TimeZone);
                using var clientBl = new ClientBll(session, Program.DbLogger);
                response.ResponseObject = clientBl.GetPartnerProductInfo(input.ClientId, input.ProductId, input.PartnerId);
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                Program.DbLogger.Error(JsonConvert.SerializeObject(fex.Detail));
                response.ResponseCode = fex.Detail.Id;
                response.Description = fex.Detail.Message;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                response.ResponseCode = Constants.Errors.GeneralException;
                response.Description = ex.Message;
            }
            return response;
        }
        [HttpPost]
        public ApiResponseBase GetBetInfo(RequestBase request)
        {
            var response = new ApiResponseBase();
            try
            {
                var bet = DocumentController.GetBetInfo(Convert.ToInt64(request.RequestData), request.ProductId,
                    new SessionIdentity { LanguageId = request.LanguageId, PartnerId = request.PartnerId }, Program.DbLogger);
                return bet;
            }
            catch (FaultException<BllFnErrorType> fex)
            {
                Program.DbLogger.Error(JsonConvert.SerializeObject(fex.Detail));
                response.ResponseCode = fex.Detail.Id;
                response.Description = fex.Detail.Message;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                response.ResponseCode = Constants.Errors.GeneralException;
                response.Description = ex.Message;
            }
            return response;
        }

        [HttpPost]
        public ApiResponseBase ApiRequest(RequestBase request)
        {
            try
            {
                var session = Helpers.Helpers.CheckToken(request.Token, request.ClientId, request.TimeZone);
                var partner = CacheManager.GetPartnerById(request.PartnerId);
                if (partner == null || !partner.SiteUrl.Split(',').Any(x => x == request.Domain))
                    throw BaseBll.CreateException(request.LanguageId, Constants.Errors.PartnerNotFound);
                session.PartnerId = request.PartnerId;
                session.Domain = request.Domain;
                session.Source = request.Source;
                return GetResponse(request, session);
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = ex.Detail == null ? Constants.Errors.GeneralException : ex.Detail.Id,
                    Description = ex.Detail == null ? ex.Message : ex.Detail.Message
                };
                if (ex.Detail != null && (ex.Detail.Id == Constants.Errors.SessionExpired || ex.Detail.Id == Constants.Errors.SessionNotFound))
                    response.ResponseObject = CacheManager.GetClientSessionByToken(request.Token, Constants.PlatformProductId, false)?.LogoutType;
                Program.DbLogger.Error(new Exception(JsonConvert.SerializeObject(response)));
                return response;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return response;
            }
        }

        private ApiResponseBase GetResponse(RequestBase request, SessionIdentity session)
        {
            switch (request.Controller)
            {
                case Enums.Controllers.Client:
                    return ClientController.CallFunction(request, session, Program.DbLogger, HostEnvironment);
                case Enums.Controllers.Document:
                    return DocumentController.CallFunction(request, session, Program.DbLogger, HostEnvironment);
                case Enums.Controllers.Util:
                    return UtilController.CallFunction(request, session, Program.DbLogger);
                case Enums.Controllers.Product:
                    return ProductController.CallFunction(request, session, Program.DbLogger);
                default:
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.ControllerNotFound);
            }
        }

        [HttpPost]
        public GetClientAccountsOutput GetClientAccounts(RequestBase request)
        {
            try
            {
                var session = Helpers.Helpers.CheckToken(request.Token, request.ClientId, request.TimeZone);
                session.TimeZone = request.TimeZone;
                var response = ClientController.GetClientAccounts(request.ClientId, session, Program.DbLogger);
                return response;
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new GetClientAccountsOutput
                {
                    ResponseCode = ex.Detail == null ? Constants.Errors.GeneralException : ex.Detail.Id,
                    Description = ex.Detail == null ? ex.Message : ex.Detail.Message
                };
                if (ex.Detail.Id == Constants.Errors.SessionExpired || ex.Detail.Id == Constants.Errors.SessionNotFound)
                    response.ResponseObject = CacheManager.GetClientSessionByToken(request.Token, Constants.PlatformProductId, false)?.LogoutType;
                Program.DbLogger.Error(new Exception(JsonConvert.SerializeObject(response)));
                return response;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new GetClientAccountsOutput
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return response;
            }
        }

        [HttpPost]
        public GetClientStatesOutput GetClientStates(RequestBase request)
        {
            try
            {
                var session = Helpers.Helpers.CheckToken(request.Token, request.ClientId, request.TimeZone);
                session.TimeZone = request.TimeZone;
                var response = ClientController.GetClientStates(request.ClientId, session, Program.DbLogger);
                return response;
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new GetClientStatesOutput
                {
                    ResponseCode = ex.Detail == null ? Constants.Errors.GeneralException : ex.Detail.Id,
                    Description = ex.Detail == null ? ex.Message : ex.Detail.Message
                };
                if (ex.Detail.Id == Constants.Errors.SessionExpired || ex.Detail.Id == Constants.Errors.SessionNotFound)
                    response.ResponseObject = CacheManager.GetClientSessionByToken(request.Token, Constants.PlatformProductId, false)?.LogoutType;
                Program.DbLogger.Error(new Exception(JsonConvert.SerializeObject(response)));
                return response;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new GetClientStatesOutput
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return response;
            }
        }

        [HttpPost]
        public GetPartnerBetShopsOutput GetBetShopsByClientId(RequestBase request)
        {
            try
            {
                var session = Helpers.Helpers.CheckToken(request.Token, request.ClientId, request.TimeZone);
                session.TimeZone = request.TimeZone;
                var response = ClientController.GetBetShopsByClientId(request.ClientId, session, Program.DbLogger);
                return response;
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new GetPartnerBetShopsOutput
                {
                    ResponseCode = ex.Detail == null ? Constants.Errors.GeneralException : ex.Detail.Id,
                    Description = ex.Detail == null ? ex.Message : ex.Detail.Message
                };
                Program.DbLogger.Error(new Exception(JsonConvert.SerializeObject(response)));
                if (ex.Detail.Id == Constants.Errors.SessionExpired || ex.Detail.Id == Constants.Errors.SessionNotFound)
                    response.ResponseObject = CacheManager.GetClientSessionByToken(request.Token, Constants.PlatformProductId, false)?.LogoutType;
                return response;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new GetPartnerBetShopsOutput
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return response;
            }
        }

        [HttpPost]
        public GetBetsHistoryOutput GetBetsHistory(RequestBase request)
        {
            try
            {
                var session = Helpers.Helpers.CheckToken(request.Token, request.ClientId, request.TimeZone);
                session.TimeZone = request.TimeZone;
                var response = DocumentController.GetBetsHistory(JsonConvert.DeserializeObject<ApiFilterInternetBet>(request.RequestData),
                       request.PartnerId, request.ClientId, session, Program.DbLogger);
                return response;
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new GetBetsHistoryOutput
                {
                    ResponseCode = ex.Detail == null ? Constants.Errors.GeneralException : ex.Detail.Id,
                    Description = ex.Detail == null ? ex.Message : ex.Detail.Message
                };
                if (ex.Detail.Id == Constants.Errors.SessionExpired || ex.Detail.Id == Constants.Errors.SessionNotFound)
                    response.ResponseObject = CacheManager.GetClientSessionByToken(request.Token, Constants.PlatformProductId, false)?.LogoutType;
                Program.DbLogger.Error(new Exception(JsonConvert.SerializeObject(response)));
                return response;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new GetBetsHistoryOutput
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return response;
            }
        }

        [HttpPost]
        public GetTransactionHistoryOutput GetTransactionHistory(RequestBase request)
        {
            try
            {
                var session = Helpers.Helpers.CheckToken(request.Token, request.ClientId, request.TimeZone);
                session.TimeZone = request.TimeZone;
                var response = DocumentController.GetTransactionHistory(JsonConvert.DeserializeObject<ApiFilterTransaction>(request.RequestData),
                            request.ClientId, session, Program.DbLogger);
                return response;
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new GetTransactionHistoryOutput
                {
                    ResponseCode = ex.Detail == null ? Constants.Errors.GeneralException : ex.Detail.Id,
                    Description = ex.Detail == null ? ex.Message : ex.Detail.Message
                };
                if (ex.Detail.Id == Constants.Errors.SessionExpired || ex.Detail.Id == Constants.Errors.SessionNotFound)
                    response.ResponseObject = CacheManager.GetClientSessionByToken(request.Token, Constants.PlatformProductId, false)?.LogoutType;
                Program.DbLogger.Error(new Exception(JsonConvert.SerializeObject(response)));
                return response;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new GetTransactionHistoryOutput
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return response;
            }
        }

        [HttpPost]
        public GetOperationTypesOutput GetOperationTypes(RequestBase request)
        {
            try
            {
                var session = Helpers.Helpers.CheckToken(request.Token, request.ClientId, request.TimeZone);
                session.TimeZone = request.TimeZone;
                session.LanguageId = request.LanguageId;
                var response = DocumentController.GetOperationTypes(session, Program.DbLogger);
                return response;
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new GetOperationTypesOutput
                {
                    ResponseCode = ex.Detail == null ? Constants.Errors.GeneralException : ex.Detail.Id,
                    Description = ex.Detail == null ? ex.Message : ex.Detail.Message
                };
                if (ex.Detail.Id == Constants.Errors.SessionExpired || ex.Detail.Id == Constants.Errors.SessionNotFound)
                    response.ResponseObject = CacheManager.GetClientSessionByToken(request.Token, Constants.PlatformProductId, false)?.LogoutType;
                Program.DbLogger.Error(new Exception(JsonConvert.SerializeObject(response)));
                return response;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new GetOperationTypesOutput
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return response;
            }
        }

        [HttpPost]
        public CancelPaymentRequestOutput CancelPaymentRequest(RequestBase request)
        {
            try
            {
                var session = Helpers.Helpers.CheckToken(request.Token, request.ClientId, request.TimeZone);
                session.TimeZone = request.TimeZone;
                var response = DocumentController.CancelPaymentRequest(JsonConvert.DeserializeObject<CancelPaymentRequestInput>(request.RequestData),
                            request.ClientId, session, Program.DbLogger);
                return response;
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new CancelPaymentRequestOutput
                {
                    ResponseCode = ex.Detail == null ? Constants.Errors.GeneralException : ex.Detail.Id,
                    Description = ex.Detail == null ? ex.Message : ex.Detail.Message
                };
                if (ex.Detail.Id == Constants.Errors.SessionExpired || ex.Detail.Id == Constants.Errors.SessionNotFound)
                    response.ResponseObject = CacheManager.GetClientSessionByToken(request.Token, Constants.PlatformProductId, false)?.LogoutType;
                Program.DbLogger.Error(new Exception(JsonConvert.SerializeObject(response)));
                return response;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new CancelPaymentRequestOutput
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return response;
            }
        }

        [HttpPost]
        public GetPaymentRequestsOutput GetPaymentRequests(RequestBase request)
        {
            try
            {
                var session = Helpers.Helpers.CheckToken(request.Token, request.ClientId, request.TimeZone);
                session.TimeZone = request.TimeZone;
                var response = DocumentController.GetPaymentRequests(JsonConvert.DeserializeObject<ApiFilterPaymentRequest>(request.RequestData),
                            request.ClientId, session, Program.DbLogger);
                return response;
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new GetPaymentRequestsOutput
                {
                    ResponseCode = ex.Detail == null ? Constants.Errors.GeneralException : ex.Detail.Id,
                    Description = ex.Detail == null ? ex.Message : ex.Detail.Message
                };
                if (ex.Detail.Id == Constants.Errors.SessionExpired || ex.Detail.Id == Constants.Errors.SessionNotFound)
                    response.ResponseObject = CacheManager.GetClientSessionByToken(request.Token, Constants.PlatformProductId, false)?.LogoutType;
                Program.DbLogger.Error(new Exception(JsonConvert.SerializeObject(response)));
                return response;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new GetPaymentRequestsOutput
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return response;
            }
        }

        [HttpPost]
        public GetPaymentRequestsOutput GetPendingPaymentRequests(RequestBase request)
        {
            try
            {
                var session = Helpers.Helpers.CheckToken(request.Token, request.ClientId, request.TimeZone);
                session.TimeZone = request.TimeZone;
                var response = DocumentController.GetPendingPaymentRequests(JsonConvert.DeserializeObject<ApiFilterPaymentRequest>(request.RequestData),
                            request.ClientId, session, Program.DbLogger);
                return response;
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new GetPaymentRequestsOutput
                {
                    ResponseCode = ex.Detail == null ? Constants.Errors.GeneralException : ex.Detail.Id,
                    Description = ex.Detail == null ? ex.Message : ex.Detail.Message
                };
                if (ex.Detail.Id == Constants.Errors.SessionExpired || ex.Detail.Id == Constants.Errors.SessionNotFound)
                    response.ResponseObject = CacheManager.GetClientSessionByToken(request.Token, Constants.PlatformProductId, false)?.LogoutType;
                Program.DbLogger.Error(new Exception(JsonConvert.SerializeObject(response)));
                return response;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new GetPaymentRequestsOutput
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return response;
            }
        }

        [HttpPost]
        public Common.Models.WebSiteModels.GetBalanceOutput GetClientBalance(RequestBase request)
        {
            try
            {
                Helpers.Helpers.CheckToken(request.Token, request.ClientId, request.TimeZone);
                return DocumentController.GetClientBalance(request.ClientId);
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new Common.Models.WebSiteModels.GetBalanceOutput
                {
                    ResponseCode = ex.Detail.Id,
                    Description = ex.Detail.Message
                };
                if (ex.Detail.Id == Constants.Errors.SessionExpired || ex.Detail.Id == Constants.Errors.SessionNotFound)
                    response.ResponseObject = CacheManager.GetClientSessionByToken(request.Token, Constants.PlatformProductId, false)?.LogoutType;
                Program.DbLogger.Error(JsonConvert.SerializeObject(response));
                return response;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new Common.Models.WebSiteModels.GetBalanceOutput
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return response;
            }
        }

        [HttpPost]
        public ApiLoginClientOutput QuickSmsRegistration(QuickClientModel input)
        {
            try
            {
                bool generatedUsername = false;
                if (string.IsNullOrWhiteSpace(input.UserName))
                {
                    input.UserName = CommonFunctions.GetRandomString(10);
                    generatedUsername = true;
                }
                else
                    input.UserName = input.UserName.Replace(" ", string.Empty);
                input.MobileNumber = input.MobileNumber.Replace(" ", string.Empty);
                if (!BaseBll.IsMobileNumber(input.MobileNumber))
                    throw BaseBll.CreateException(input.LanguageId, Constants.Errors.InvalidMobile);
                using (var clientBl = new ClientBll(new SessionIdentity { LanguageId = input.LanguageId, Domain = input.Domain }, Program.DbLogger))
                {
                    var quickRegistrationInput = new QuickRegistrationInput
                    {
                        UserName = input.UserName,
                        Password = input.Password,
                        EmailOrMobile = input.MobileNumber,
                        CurrencyId = input.CurrencyId,
                        PartnerId = input.PartnerId,
                        ReCaptcha = input.ReCaptcha ?? string.Empty,
                        IsMobile = true,
                        Ip = input.Ip,
                        FirstName = input.FirstName,
                        LastName = input.LastName,
                        PromoCode = input.PromoCode,
                        ReferralData = null,
                        CountryCode = input.CountryCode,
                        GeneratedUsername = generatedUsername
                    };
                    if (!string.IsNullOrEmpty(input.SMSCode))
                    {
                        clientBl.VerifyClientMobileNumber(input.SMSCode, input.MobileNumber, null, input.PartnerId, true, null);
                        quickRegistrationInput.IsMobileNumberVerified = true;
                    }
                    if (!string.IsNullOrEmpty(input.AffiliateId) && input.AffiliatePlatformId.HasValue)
                    {
                        quickRegistrationInput.ReferralData = new DAL.AffiliateReferral
                        {
                            AffiliatePlatformId = input.AffiliatePlatformId.Value,
                            AffiliateId = input.AffiliateId,
                            RefId = input.RefId
                        };
                    }
                    if (!string.IsNullOrEmpty(input.AgentCode) && int.TryParse(input.AgentCode, out int agentId))
                    {
                        var agent = CacheManager.GetUserById(agentId);
                        if (agent != null && agent.PartnerId == quickRegistrationInput.PartnerId &&
                           (agent.Type == (int)UserTypes.MasterAgent || agent.Type == (int)UserTypes.Agent))
                        {
                            quickRegistrationInput.UserId = agentId;
                        }
                    }
                    var client = clientBl.QuickRegisteration(quickRegistrationInput, HostEnvironment);
                    var response = client.MapToApiLoginClientOutput(input.TimeZone);
                    return response;
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                Program.DbLogger.Error(ex);
                ApiLoginClientOutput response;
                if (ex.Detail != null)
                    response = new ApiLoginClientOutput
                    {
                        ResponseCode = ex.Detail.Id,
                        Description = ex.Detail.Message
                    };
                else
                    response = new ApiLoginClientOutput
                    {
                        ResponseCode = Constants.Errors.GeneralException,
                        Description = ex.Message
                    };
                return response;
            }
            catch (DbEntityValidationException ex)
            {
                var error = new StringBuilder();
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        error.AppendFormat("Property: {0} Error: {1}",
                            validationError.PropertyName,
                            validationError.ErrorMessage);
                    }
                }
                Program.DbLogger.Error(new Exception(error.ToString()));
                var response = new ApiLoginClientOutput
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return response;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new ApiLoginClientOutput
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return response;
            }
        }

        [HttpPost]
        public ApiLoginClientOutput QuickEmailRegistration(QuickClientModel input)
        {
            try
            {
                bool generatedUsername = false;
                if (string.IsNullOrWhiteSpace(input.UserName))
                {
                    input.UserName = CommonFunctions.GetRandomString(10);
                    generatedUsername = true;
                }
                else
                    input.UserName = input.UserName.Replace(" ", string.Empty);
                input.Email = input.Email.Replace(" ", string.Empty);
                if (!BaseBll.IsValidEmail(input.Email))
                    throw BaseBll.CreateException(input.LanguageId, Constants.Errors.InvalidEmail);

                var quickRegistrationInput = new QuickRegistrationInput
                {
                    UserName = input.UserName,
                    Password = input.Password,
                    EmailOrMobile = input.Email,
                    CurrencyId = input.CurrencyId,
                    PartnerId = input.PartnerId,
                    ReCaptcha = input.ReCaptcha ?? input.ReCaptcha,
                    IsMobile = false,
                    Ip = input.Ip,
                    FirstName = input.FirstName,
                    LastName = input.LastName,
                    PromoCode = input.PromoCode,
                    ReferralData = null,
                    CountryCode = input.CountryCode,
                    GeneratedUsername = generatedUsername
                };
                if (!string.IsNullOrEmpty(input.AffiliateId) && input.AffiliatePlatformId.HasValue)
                {
                    quickRegistrationInput.ReferralData = new DAL.AffiliateReferral
                    {
                        AffiliatePlatformId = input.AffiliatePlatformId.Value,
                        AffiliateId = input.AffiliateId,
                        RefId = input.RefId
                    };
                }
                if (!string.IsNullOrEmpty(input.AgentCode) && int.TryParse(input.AgentCode, out int agentId))
                {
                    var agent = CacheManager.GetUserById(agentId);
                    if (agent != null && agent.PartnerId == quickRegistrationInput.PartnerId &&
                       (agent.Type == (int)UserTypes.MasterAgent || agent.Type == (int)UserTypes.Agent))
                    {
                        quickRegistrationInput.UserId = agentId;
                    }
                }
                using (var clientBl = new ClientBll(new SessionIdentity { LanguageId = input.LanguageId, Domain = input.Domain }, Program.DbLogger))
                {
                    var client = clientBl.QuickRegisteration(quickRegistrationInput, HostEnvironment);
                    var response = client.MapToApiLoginClientOutput(input.TimeZone);
                    return response;
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                ApiLoginClientOutput response;
                if (ex.Detail != null)
                    response = new ApiLoginClientOutput
                    {
                        ResponseCode = ex.Detail.Id,
                        Description = ex.Detail.Message
                    };
                else
                    response = new ApiLoginClientOutput
                    {
                        ResponseCode = Constants.Errors.GeneralException,
                        Description = ex.Message
                    };
                Program.DbLogger.Error(JsonConvert.SerializeObject(response));
                return response;
            }
            catch (DbEntityValidationException ex)
            {
                var error = new StringBuilder();
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        error.AppendFormat("Property: {0} Error: {1}",
                            validationError.PropertyName,
                            validationError.ErrorMessage);
                    }
                }
                Program.DbLogger.Error(new Exception(error.ToString()));
                var response = new ApiLoginClientOutput
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return response;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new ApiLoginClientOutput
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return response;
            }
        }

        [HttpPost]
        public ApiLoginClientOutput RegisterClient(ClientModel input)
        {
            try
            {
                using (var clientBl = new ClientBll(new SessionIdentity { LanguageId = input.LanguageId, Domain = input.Domain, Country = input.CountryCode }, Program.DbLogger))
                {
                    using (var regionBl = new RegionBll(clientBl))
                    {
                        if (input.TermsConditionsAccepted.HasValue && !input.TermsConditionsAccepted.Value) // should have a value
                            throw BaseBll.CreateException(input.LanguageId, Constants.Errors.AcceptTermsConditions);

                        bool generatedUsername = false;
                        if (string.IsNullOrWhiteSpace(input.UserName))
                        {
                            input.UserName = CommonFunctions.GetRandomString(10);
                            generatedUsername = true;
                        }
                        else
                            input.UserName = input.UserName.Replace(" ", string.Empty);

                        if (!string.IsNullOrEmpty(input.Email))
                            input.Email = input.Email.Replace(" ", string.Empty);
                        if (!string.IsNullOrEmpty(input.MobileNumber))
                            input.MobileNumber = input.MobileNumber.Replace(" ", string.Empty);

                        var clientRegistrationInput = new ClientRegistrationInput
                        {
                            ClientData = input.MapToClient(),
                            ReferralType = !string.IsNullOrEmpty(input.ReferralType) ? Convert.ToInt32(input.ReferralType) : null,
                            SecurityQuestions = input.SecurityQuestions,
                            IsQuickRegistration = false,
                            ReCaptcha = input.ReCaptcha ?? string.Empty,
                            PromoCode = input.PromoCode,
                            GeneratedUsername = generatedUsername
                        };
                        if (!string.IsNullOrEmpty(input.SMSCode))
                        {
                            clientBl.VerifyClientMobileNumber(input.SMSCode, input.MobileNumber, null, input.PartnerId, true, null);
                            clientRegistrationInput.ClientData.IsMobileNumberVerified = true;
                        }

                        if (!string.IsNullOrEmpty(input.EmailCode))
                        {
                            clientBl.VerifyClientEmail(input.EmailCode, input.Email, null, input.PartnerId, true, null);
                            clientRegistrationInput.ClientData.IsEmailVerified = true;
                        }

                        if (!string.IsNullOrEmpty(input.IdCardDocumentData))
                            clientRegistrationInput.IdCardDocumentByte = Convert.FromBase64String(input.IdCardDocumentData);
                        if (!string.IsNullOrEmpty(input.UtilityBillDocumentData))
                            clientRegistrationInput.UtilityBillDocumentByte = Convert.FromBase64String(input.UtilityBillDocumentData);
                        if (!string.IsNullOrEmpty(input.PassportDocumentData))
                            clientRegistrationInput.PassportDocumentByte = Convert.FromBase64String(input.PassportDocumentData);
                        if (!string.IsNullOrEmpty(input.DriverLicenseDocumentData))
                            clientRegistrationInput.DriverLicenseDocumentByte = Convert.FromBase64String(input.DriverLicenseDocumentData);


                        if (!string.IsNullOrEmpty(input.AffiliateId) && input.AffiliatePlatformId.HasValue)
                        {
                            clientRegistrationInput.ReferralData = new DAL.AffiliateReferral
                            {
                                AffiliatePlatformId = input.AffiliatePlatformId.Value,
                                AffiliateId = input.AffiliateId,
                                RefId = input.RefId
                            };
                        }
                        if (!string.IsNullOrEmpty(input.AgentCode))
                        {
                            if (!int.TryParse(input.AgentCode, out int agentId))
                                throw BaseBll.CreateException(input.LanguageId, Constants.Errors.WrongUserId);
                            var agent = CacheManager.GetUserById(agentId);
                            if (agent == null || agent.PartnerId != clientRegistrationInput.ClientData.PartnerId ||
                                (agent.Type != (int)UserTypes.MasterAgent &&  agent.Type != (int)UserTypes.Agent))
                                throw BaseBll.CreateException(input.LanguageId, Constants.Errors.UserNotFound);
                            clientRegistrationInput.ClientData.UserId = agentId;
                        }

                        if (clientRegistrationInput.ClientData.RegionId == 0)
                        {
                            var region = regionBl.GetRegionByCountryCode(input.CountryCode);
                            if (region != null)
                                clientRegistrationInput.ClientData.RegionId = region.Id;
                        }

                        var client = clientBl.RegisterClient(clientRegistrationInput, HostEnvironment);
                        return client.MapToApiLoginClientOutput(input.TimeZone);
                    }
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                ApiLoginClientOutput response;
                if (ex.Detail != null)
                    response = new ApiLoginClientOutput
                    {
                        ResponseCode = ex.Detail.Id,
                        Description = ex.Detail.Message
                    };
                else
                    response = new ApiLoginClientOutput
                    {
                        ResponseCode = Constants.Errors.GeneralException,
                        Description = ex.Message
                    };
                Program.DbLogger.Error(JsonConvert.SerializeObject(response));
                return response;
            }
            catch (DbEntityValidationException ex)
            {
                var error = new StringBuilder();
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        error.AppendFormat("Property: {0} Error: {1}",
                            validationError.PropertyName,
                            validationError.ErrorMessage);
                    }
                }
                Program.DbLogger.Error(new Exception(error.ToString()));
                var response = new ApiLoginClientOutput
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return response;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new ApiLoginClientOutput
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return response;
            }
        }

        [HttpPost]
        public ApiLoginClientOutput RegisterAffiliate(ClientModel input)
        {
            try
            {
                using (var clientBl = new ClientBll(new SessionIdentity { LanguageId = input.LanguageId, Domain = input.Domain, Country = input.CountryCode }, Program.DbLogger))
                {
                    using (var regionBl = new RegionBll(clientBl))
                    {
                        if (input.TermsConditionsAccepted.HasValue && !input.TermsConditionsAccepted.Value) // should have a value
                            throw BaseBll.CreateException(input.LanguageId, Constants.Errors.AcceptTermsConditions);

                        if (string.IsNullOrEmpty(input.Email))
                            throw BaseBll.CreateException(input.LanguageId, Constants.Errors.EmailCantBeEmpty);
                        else
                            input.Email = input.Email.Replace(" ", string.Empty);

                        if (!string.IsNullOrEmpty(input.MobileNumber))
                            input.MobileNumber = input.MobileNumber.Replace(" ", string.Empty);

                        var clientRegistrationInput = new ClientRegistrationInput
                        {
                            ClientData = input.MapToClient(),
                            ReCaptcha = input.ReCaptcha ?? string.Empty
                        };

                        if (clientRegistrationInput.ClientData.RegionId == 0)
                        {
                            var region = regionBl.GetRegionByCountryCode(input.CountryCode);
                            if (region != null)
                                clientRegistrationInput.ClientData.RegionId = region.Id;
                        }

                        var client = clientBl.RegisterAffiliate(clientRegistrationInput);
                        return client.MapToApiLoginClientOutput(input.TimeZone);
                    }
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                ApiLoginClientOutput response;
                if (ex.Detail != null)
                    response = new ApiLoginClientOutput
                    {
                        ResponseCode = ex.Detail.Id,
                        Description = ex.Detail.Message
                    };
                else
                    response = new ApiLoginClientOutput
                    {
                        ResponseCode = Constants.Errors.GeneralException,
                        Description = ex.Message
                    };
                Program.DbLogger.Error(JsonConvert.SerializeObject(response) + "_" + JsonConvert.SerializeObject(input));
                return response;
            }
            catch (DbEntityValidationException ex)
            {
                var error = new StringBuilder();
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        error.AppendFormat("Property: {0} Error: {1}",
                            validationError.PropertyName,
                            validationError.ErrorMessage);
                    }
                }
                Program.DbLogger.Error(new Exception(error.ToString()) + "_" + JsonConvert.SerializeObject(input));
                var response = new ApiLoginClientOutput
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return response;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new ApiLoginClientOutput
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return response;
            }
        }

        [HttpPost]
        public ApiLoginClientOutput LoginClient(LoginDetails input)
        {
            int clientId = 0;
            try
            {
                var sessionIdentity = new SessionIdentity
                {
                    Domain = input.Domain,
                    LoginIp = input.Ip,
                    LanguageId = input.LanguageId,
                    TimeZone = input.TimeZone
                };
                if (!string.IsNullOrEmpty(input.TerminalId) && input.BetShopId.HasValue && !string.IsNullOrEmpty(input.Token))
                {
                    var tClient = ClientBll.LoginTerminalClient(input.MapToTerminalClientInput()).MapToApiLoginClientOutput(input.TimeZone);
                    Integration.Products.Helpers.GlobalSlotsHelpers.TransferFromProvider(tClient.Id, sessionIdentity, Program.DbLogger);
                    return tClient;
                }

                input.ClientIdentifier = input.ClientIdentifier.Replace(" ", string.Empty);
                input.Password = input.Password?.Trim();
                var loginInput = new ClientLoginInput
                {
                    PartnerId = input.PartnerId,
                    ClientIdentifier = input.ClientIdentifier,
                    Password = input.Password,
                    Ip = input.Ip,
                    DeviceType = (input.DeviceType == null || input.DeviceType < (int)DeviceTypes.Desktop || input.DeviceType > (int)DeviceTypes.BetShop) ?
                    (int)DeviceTypes.Desktop : input.DeviceType.Value,
                    LanguageId = input.LanguageId,
                    CountryCode = input.CountryCode,
                    Source = input.Source,
                    TimeZone = input.TimeZone
                };
                string newToken;
                var externalPlatformType = CacheManager.GetPartnerSettingByKey(input.PartnerId, Constants.PartnerKeys.ExternalPlatform);
                if ((externalPlatformType != null && externalPlatformType.NumericValue != null &&
                    externalPlatformType.NumericValue.Value == (int)PartnerTypes.ExternalPlatform) || input.ExternalPlatformId.HasValue)
                {
                    loginInput.ExternalPlatformType = input.ExternalPlatformId ?? Convert.ToInt32(externalPlatformType.StringValue);
                    var resp = ExternalPlatformHelpers.CreateClientSession(loginInput, out newToken, out clientId, sessionIdentity, Program.DbLogger);
                    return resp.MapToApiLoginClientOutput(newToken, input.TimeZone);
                }
                BllClient client = null;
                var partnerSetting = CacheManager.GetPartnerSettingByKey(input.PartnerId, Constants.PartnerKeys.IsUserNameGeneratable);
                if (partnerSetting != null && partnerSetting.NumericValue.HasValue && partnerSetting.NumericValue != 0)
                {
                    client = CacheManager.GetClientByNickName(input.PartnerId, input.ClientIdentifier);
                    if (client == null)
                        client = CacheManager.GetClientByUserName(input.PartnerId, input.ClientIdentifier);
                }
                else
                {
                    if (ClientBll.IsValidEmail(input.ClientIdentifier))
                    {
                        client = CacheManager.GetClientByEmail(input.PartnerId, input.ClientIdentifier.ToLower());
                    }
                    else if (ClientBll.IsMobileNumber(input.ClientIdentifier))
                    {
                        input.ClientIdentifier = "+" + input.ClientIdentifier.Replace("+", string.Empty).Replace(" ", string.Empty);
                        client = CacheManager.GetClientByMobileNumber(input.PartnerId, input.ClientIdentifier);
                    }
                    else
                        client = CacheManager.GetClientByUserName(input.PartnerId, input.ClientIdentifier);
                }
                if (client == null)
                    throw BaseBll.CreateException(input.LanguageId, Constants.Errors.WrongLoginParameters);

                clientId = client.Id;
                if (CacheManager.GetConfigKey(client.PartnerId, Constants.PartnerKeys.JCJVerification) == "1" && client.IsDocumentVerified)
                {
                    var res = DigitalCustomerHelpers.GetJCJStatus(client.PartnerId, client.DocumentType ?? 0, client.DocumentNumber,
                        input.LanguageId, Program.DbLogger);

                    using (var clientBl = new ClientBll(sessionIdentity, Program.DbLogger))
                    {
                        var newValue = clientBl.AddOrUpdateClientSetting(client.Id, ClientSettings.JCJProhibited, res, res.ToString(), null, null, "System");
                        if (newValue == "1")
                            throw BaseBll.CreateException(input.LanguageId, Constants.Errors.JCJExcluded);
                    }
                    CacheManager.RemoveClientSetting(clientId, ClientSettings.JCJProhibited);
                    Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}_{2}", Constants.CacheItems.ClientSettings, clientId, ClientSettings.JCJProhibited));
                }
                var result = ClientBll.LoginClient(loginInput, client, out newToken, out RegionTree regionTree, Program.DbLogger);

                Helpers.Helpers.InvokeMessage("LoginClient", result.Id, input.Ip);
                var response = result.MapToApiLoginClientOutput(newToken, input.TimeZone);
                response.RegionId = regionTree.RegionId;
                response.TownId = regionTree.TownId;
                response.CityId = regionTree.CityId;
                response.CountryId = regionTree.CountryId;
                return response;
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                if (ex.Detail.Id == (int)Constants.Errors.ClientForceBlocked)
                {
                    Helpers.Helpers.InvokeMessage("RemoveClient", clientId);
                    Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}_{2}", Constants.CacheItems.ClientSettings, clientId,
                                                                                                     ClientSettings.ParentState));
                }
                else if (ex.Detail.Id == (int)Constants.Errors.WrongPassword)
                    Helpers.Helpers.InvokeMessage("UpdateClientFailedLoginCount", clientId);

                var response = new ApiLoginClientOutput
                {
                    ResponseCode = ex.Detail.Id,
                    Description = ex.Detail.Message
                };
                if (ex.Detail.Id == Constants.Errors.SessionExpired || ex.Detail.Id == Constants.Errors.SessionNotFound)
                    response.ResponseObject = CacheManager.GetClientSessionByToken(input.Token, Constants.PlatformProductId, false)?.LogoutType;

                ClientBll.CreateNewFailedSession(clientId, input.LanguageId, input.Ip, input.CountryCode, null, input.DeviceType ?? (int)DeviceTypes.Desktop, input.Source, ex.Detail.Id);
                Program.DbLogger.Error(JsonConvert.SerializeObject(ex));
                return response;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new ApiLoginClientOutput
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return response;
            }
        }

        public ApiClientInfo GetClientInfo(RequestBase request)
        {
            try
            {
                using (var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger))
                {
                    var session = Helpers.Helpers.CheckToken(request.Token, request.ClientId, request.TimeZone);
                    var client = CacheManager.GetClientById(session.Id);
                    var clientLoginOut = new ClientLoginOut();
                    clientBl.GetClientRegionInfo(client.RegionId, ref clientLoginOut);
                    var questions = clientBl.GetClientSecurityQuestions(client.Id);
                    var responseClient = client.ToApiClientInfo(request.TimeZone, clientLoginOut);
                    responseClient.SecurityQuestions = questions;
                    var clientIdentities = clientBl.GetClientIdentities(client.Id)
                                .OrderByDescending(x => x.Status == (int)KYCDocumentStates.Approved)
                                .ThenByDescending(x => x.Status ==(int)KYCDocumentStates.InProcess)
                                .ThenByDescending(x => x.Status);
                    var utilityBill = clientIdentities.FirstOrDefault(x => x.DocumentTypeId == (int)KYCDocumentTypes.UtilityBill);
                    var idCard = clientIdentities.FirstOrDefault(x => x.DocumentTypeId == (int)KYCDocumentTypes.IDCard);
                    responseClient.AddressVerifiedState = (utilityBill == null || (utilityBill.Status != (int)KYCDocumentStates.InProcess && utilityBill.Status != (int)KYCDocumentStates.Approved)) ?
                                                          (int)VerificationStates.Undefined : (utilityBill.Status == (int)KYCDocumentStates.InProcess ? (int)VerificationStates.Pending : (int)VerificationStates.Verified);

                    responseClient.PersonalDataVerifiedState = (idCard == null || (idCard.Status != (int)KYCDocumentStates.InProcess && idCard.Status != (int)KYCDocumentStates.Approved)) ?
                                      (int)VerificationStates.Undefined : (idCard.Status == (int)KYCDocumentStates.InProcess ? (int)VerificationStates.Pending : (int)VerificationStates.Verified);

                    return responseClient;
                }
            }

            catch (FaultException<BllFnErrorType> ex)
            {
                Program.DbLogger.Error(ex);
                var response = new ApiClientInfo
                {
                    ResponseCode = ex.Detail.Id,
                    Description = ex.Detail.Message
                };
                if (ex.Detail.Id == Constants.Errors.SessionExpired || ex.Detail.Id == Constants.Errors.SessionNotFound)
                    response.ResponseObject = CacheManager.GetClientSessionByToken(request.Token, Constants.PlatformProductId, false)?.LogoutType;
                return response;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new ApiClientInfo
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return response;
            }
        }

        [HttpPost]
        public ApiLoginClientOutput GetClientByToken(RequestBase request)
        {
            try
            {
                using (var clientBl = new ClientBll(new SessionIdentity(), Program.DbLogger))
                {
                    var client = clientBl.GetClientByToken(request.Token, out ClientLoginOut clientLoginOut, request.LanguageId);
                    var dbClient = clientBl.GetClientById(client.Id);
                    var currency = CacheManager.GetCurrencyById(client.CurrencyId);
                    dbClient.CurrencySymbol = currency.Symbol;
                    dbClient.Token = clientLoginOut.NewToken;
                    var responseClient = dbClient.MapToApiLoginClientOutput(request.TimeZone, clientLoginOut);
                    if (request.Token != clientLoginOut.NewToken)
                        Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.ClientSessions, request.Token));
                    return responseClient;
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                Program.DbLogger.Error(ex);
                var response = new ApiLoginClientOutput
                {
                    ResponseCode = ex.Detail.Id,
                    Description = ex.Detail.Message
                };
                if (ex.Detail.Id == Constants.Errors.SessionExpired || ex.Detail.Id == Constants.Errors.SessionNotFound)
                    response.ResponseObject = CacheManager.GetClientSessionByToken(request.Token, Constants.PlatformProductId, false)?.LogoutType;
                return response;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new ApiLoginClientOutput
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return response;
            }
        }

        [HttpPost]
        public GetPartnerBetShopsOutput GetPartnerBetShops(RequestBase request)
        {
            try
            {
                using (var betShopBl = new BetShopBll(new SessionIdentity(), Program.DbLogger))
                {

                    var betShops =
                        betShopBl.GetBetShops(new FilterBetShop { PartnerId = request.PartnerId }, false)
                            .Where(x => x.State == Constants.CashDeskStates.Active);
                    if (!string.IsNullOrEmpty(request.Token))
                    {
                        var session = Helpers.Helpers.CheckToken(request.Token, request.ClientId, request.TimeZone);
                        betShops = betShops.Where(x => x.CurrencyId == session.CurrencyId);
                    }
                    var response = new GetPartnerBetShopsOutput
                    {
                        BetShops = betShops.MapToBetShopModels()
                    };
                    return response;
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new GetPartnerBetShopsOutput
                {
                    ResponseCode = ex.Detail.Id,
                    Description = ex.Detail.Message
                };
                Program.DbLogger.Error(response);
                if (ex.Detail.Id == Constants.Errors.SessionExpired || ex.Detail.Id == Constants.Errors.SessionNotFound)
                    response.ResponseObject = CacheManager.GetClientSessionByToken(request.Token, Constants.PlatformProductId, false)?.LogoutType;
                return response;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new GetPartnerBetShopsOutput
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return response;
            }
        }

        [HttpPost]
        public GetPartnerPaymentSystemsOutput GetPartnerPaymentSystems(ApiFilterPartnerPaymentSetting input)
        {
            try
            {
                var partner = CacheManager.GetPartnerById(input.PartnerId);
                if (partner == null || !partner.SiteUrl.Split(',').Any(x => x == input.Domain))
                    throw BaseBll.CreateException(input.LanguageId, Constants.Errors.PartnerNotFound);
                using var paymentSystemBl = new PaymentSystemBll(new SessionIdentity { LanguageId = input.LanguageId }, Program.DbLogger);
                using var clientBl = new ClientBll(paymentSystemBl);
                var filter = input.MapToFilterPartnerPaymentSystem();
                filter.Status = (int)PartnerPaymentSettingStates.Active;
                if (input.ClientId != 0)
                {
                    var client = CacheManager.GetClientById(input.ClientId);
                    if (client != null)
                    {
                        filter.CurrencyId = client.CurrencyId;
                        filter.CountryId = BLL.Helpers.CommonHelpers.GetCountryId(client.RegionId, input.LanguageId);
                    }
                }

                var paymentSystems = paymentSystemBl.GetfnPartnerPaymentSettings(filter, false);
                var resp = new GetPartnerPaymentSystemsOutput
                {
                    PartnerPaymentSystems = new List<PartnerPaymentSettingModel>()
                };
                var segments = new List<ClientPaymentSegment>();
                DAL.PaymentLimit limits = null;

                if (input.ClientId != 0)
                {
                    segments = clientBl.GetClientPaymentSegments(input.ClientId, null);
                    limits = clientBl.GetClientPaymentLimits(input.ClientId);
                }
                foreach (var ps in paymentSystems)
                {
                    var setting = CacheManager.GetPartnerPaymentSettings(ps.PartnerId, ps.PaymentSystemId, ps.CurrencyId, ps.Type);
                    if (setting != null && setting.Id > 0 &&
                       (setting.OSTypes == null || setting.OSTypes.Count == 0 || setting.OSTypes.Contains(input.OSType)) &&
                       (setting.Countries == null || setting.Countries.Count == 0 || setting.Countries.Contains(filter.CountryId ?? 0)))
                    {
                        var item = ps.MapToPartnerPaymentSettingsModel();
                        if (input.ClientId != 0 && item.Type == (int)PaymentRequestTypes.Deposit)
                        {
                            var settingName = string.Format("{0}_{1}", ClientSettings.PaymentAddress, item.PaymentSystemId);
                            var clientSetting = CacheManager.GetClientSettingByName(input.ClientId, settingName);
                            if (!string.IsNullOrEmpty(clientSetting.Name) && !string.IsNullOrEmpty(clientSetting.StringValue))
                            {
                                var address = clientSetting.StringValue.Split('|');
                                item.Address = address[0];
                                if (address.Length > 1)
                                    item.DestinationTag = address[1];
                            }
                            else
                            {
                                var cryptoAddress = PaymentHelpers.GetClientPaymentAddress(item.PaymentSystemId, input.ClientId, Program.DbLogger);
                                item.Address = cryptoAddress.Address;
                                item.DestinationTag = cryptoAddress.DestinationTag;
                                if (!string.IsNullOrEmpty(item.Address))
                                    clientBl.UpdateClientSettings(input.ClientId, new List<DAL.ClientSetting>
                                            {
                                                new DAL.ClientSetting
                                                {
                                                    Name = settingName,
                                                    StringValue = string.Format("{0}|{1}", item.Address, item.DestinationTag)
                                                }
                                            });
                            }
                        }
                        /*var segment = segments.Where(x => x.PaymentSystemId == item.PaymentSystemId && x.CurrencyId == item.CurrencyId).OrderBy(x => x.Priority).FirstOrDefault();
                        if (segment != null)
                        {
                            if (segment.Status != 1 && item.Type == (int)PaymentSettingTypes.Deposit)
                                continue;
                            if (item.Type == (int)PaymentSettingTypes.Deposit)
                            {
                                item.MinAmount = Math.Max(item.MinAmount, segment.DepositMinAmount);
                                item.MaxAmount = Math.Min(item.MaxAmount, segment.DepositMaxAmount);
                            }
                            else if (item.Type == (int)PaymentSettingTypes.Withdraw)
                            {
                                item.MinAmount = Math.Max(item.MinAmount, segment.WithdrawMinAmount);
                                item.MaxAmount = Math.Min(item.MaxAmount, segment.WithdrawMaxAmount);
                            }
                        }*/
                        if (limits != null)
                        {
                            if (item.Type == (int)PaymentSettingTypes.Deposit && limits.MaxDepositAmount != null)
                                item.MaxAmount = Math.Min(item.MaxAmount, limits.MaxDepositAmount.Value);
                            else if (item.Type == (int)PaymentSettingTypes.Withdraw && limits.MaxWithdrawAmount != null)
                                item.MaxAmount = Math.Min(item.MaxAmount, limits.MaxWithdrawAmount.Value);
                        }
                        resp.PartnerPaymentSystems.Add(item);
                    }
                }

                return resp;
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new GetPartnerPaymentSystemsOutput
                {
                    ResponseCode = ex.Detail.Id,
                    Description = ex.Detail.Message
                };
                Program.DbLogger.Info(JsonConvert.SerializeObject(response));
                return response;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                return new GetPartnerPaymentSystemsOutput
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
            }
        }

        [HttpPost]
        public RecoverPasswordOutput RecoverPassword(ClientPasswordRecovery input)
        {
            try
            {
                using var clientBl = new ClientBll(new SessionIdentity { LanguageId = input.LanguageId, Domain = input.Domain }, Program.DbLogger);
                var response = clientBl.RecoverPassword(input.PartnerId, input.RecoveryToken, input.NewPassword, input.LanguageId, input.SecurityQuestions);
                CacheManager.RemoveClientFromCache(response.Id);
                Helpers.Helpers.InvokeMessage("RemoveClient", response.Id);
                Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}_{2}", Constants.CacheItems.ClientSettings, response.Id, "PasswordChangedDate"));
                Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}_{2}", Constants.CacheItems.ClientSettings, response.Id, "BlockedForInactivity"));
                Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}_{2}", Constants.CacheItems.ClientSettings, response.Id, "ParentState"));
                Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.ClientFailedLoginCount, response.Id));

                return new RecoverPasswordOutput
                {
                    ClientId = response.Id,
                    ClientEmail = response.Email,
                    ClientFirstName = response.FirstName,
                    ClientLastName = response.LastName
                };
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new RecoverPasswordOutput
                {
                    ResponseCode = ex.Detail.Id,
                    Description = ex.Detail.Message
                };
                Program.DbLogger.Info(JsonConvert.SerializeObject(response));
                return response;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new RecoverPasswordOutput
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return response;
            }
        }

        [HttpPost]
        public ApiResponseBase SendRecoveryToken(ApiSendRecoveryTokenInput input)
        {
            try
            {
                using (var clientBl = new ClientBll(new SessionIdentity { LanguageId = input.LanguageId, Domain = input.Domain }, Program.DbLogger))
                {
                    return new ApiResponseBase
                    {
                        ResponseObject = new { ActivePeriodInMinutes = clientBl.SendRecoveryToken(input.PartnerId, input.LanguageId, input.EmailOrMobile, input.ReCaptcha) }
                    };
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = ex.Detail.Id,
                    Description = ex.Detail.Message
                };
                Program.DbLogger.Error(JsonConvert.SerializeObject(response));
                return response;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return response;
            }
        }

        [HttpPost]
        public GetProductSessionOutput GetProductSession(RequestBase request)
        {
            try
            {
                var clientSession = Helpers.Helpers.CheckToken(request.Token, request.ClientId, request.TimeZone);
                var input = JsonConvert.DeserializeObject<GetProductSessionInput>(request.RequestData);
                return ProductController.GetProductSession(input.ProductId, input.DeviceType, clientSession);
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                Program.DbLogger.Error(ex);
                var response = new GetProductSessionOutput
                {
                    ResponseCode = ex.Detail.Id,
                    Description = ex.Detail.Message
                };
                if (ex.Detail.Id == Constants.Errors.SessionExpired || ex.Detail.Id == Constants.Errors.SessionNotFound)
                    response.ResponseObject = CacheManager.GetClientSessionByToken(request.Token, Constants.PlatformProductId, false)?.LogoutType;
                return response;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new GetProductSessionOutput
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return response;
            }
        }

        [HttpPost]
        public ApiResponseBase GetProductUrl(GetProductUrlInput input)
        {
            var response = new ApiResponseBase();
            try
            {
                if (string.IsNullOrEmpty(input.Token))
                    input.IsForDemo = true;

                var product = CheckProductAvailability(input.PartnerId, input.ProductId);
                if (input.IsForDemo && !product.HasDemo)
                    throw BaseBll.CreateException(input.LanguageId, Constants.Errors.DemoNotSupported);
                var partner = CacheManager.GetPartnerById(input.PartnerId);
                var provider = CacheManager.GetGameProviderById(product.GameProviderId.Value);
                if (string.IsNullOrWhiteSpace(input.LanguageId))
                    input.LanguageId = Constants.DefaultLanguageId;
                GetProductSessionOutput productSession = null;
                var clientSession = new SessionIdentity
                {
                    LanguageId = input.LanguageId,
                    Domain = input.Domain,
                    CurrencyId = partner.CurrencyId,
                    LoginIp = input.Ip,
                    Country = input.CountryCode
                };
                if (!input.IsForDemo)
                {
                    clientSession = Helpers.Helpers.CheckToken(input.Token, input.ClientId, input.TimeZone);
                    clientSession.Domain = input.Domain;
                    var client = CacheManager.GetClientById(input.ClientId);
                    if (client.PartnerId != input.PartnerId)
                        throw BaseBll.CreateException(clientSession.LanguageId, Constants.Errors.WrongPartnerId);
                    if (provider.Name != Constants.GameProviders.EvenBet
                        && provider.Name != Constants.GameProviders.BlueOcean 
                        && provider.Name != Constants.GameProviders.Igrosoft
                        && provider.Name != Constants.GameProviders.TomHorn)
                    {
                        productSession = ProductController.GetProductSession(input.ProductId,
                            (input.IsForMobile.HasValue && input.IsForMobile.Value) ? (int)DeviceTypes.Mobile : (int)DeviceTypes.Desktop,
                            clientSession);
                    }
                }
                response.ResponseObject = CreateUrl(input, product, provider, clientSession, productSession?.ProductToken);
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                response.ResponseCode = ex.Detail.Id;
                response.Description = ex.Detail.Message;
                Program.DbLogger.Error(JsonConvert.SerializeObject(response));
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                response.ResponseCode = Constants.Errors.GeneralException;
                response.Description = ex.Message;
            }
            return response;
        }

        [HttpPost]
        public ApiResponseBase GetProductById(GetProductUrlInput input)
        {
            using (var productBl = new ProductBll(new SessionIdentity(), Program.DbLogger))
            {
                var output = productBl.GetfnProductById(input.ProductId, false, input.LanguageId);
                var resp = new ApiResponseBase
                {
                    ResponseObject = output == null ? new ApiProduct() : output.ToApiProduct()
                };
                return resp;
            }
        }

        [HttpPost]
        public ApiResponseBase GetGameProviderById(GetProductUrlInput input)
        {
            var output = CacheManager.GetGameProviderById(input.ProviderId);
            return new ApiResponseBase
            {
                ResponseObject = output.ToApiGameProvider()
            };
        }

        [HttpPost]
        public ApiResponseBase GetPartnerProductSetting(GetProductUrlInput input)
        {
            var setting = CacheManager.GetPartnerProductSettingByProductId(input.PartnerId, input.ProductId);
            var response = new ApiResponseBase
            {
                ResponseCode = setting == null ? Errors.PartnerProductSettingNotFound : 0,
                ResponseObject = setting == null ? null : setting.ToApiPartnerProductSetting()
            };
            return response;
        }

        [HttpPost]
        public ApiResponseBase GetClientPaymentInfoTypesEnum(ApiRequestBase input)
        {
            try
            {
                return new ApiResponseBase
                {
                    ResponseObject = BaseBll.GetEnumerations(Constants.EnumerationTypes.ClientPaymentInfoTypes, input.LanguageId).Select(x => new
                    {
                        Id = x.Value,
                        Name = x.Text
                    }).ToList()
                };
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                Program.DbLogger.Error(ex);
                var response = new ApiResponseBase
                {
                    ResponseCode = ex.Detail.Id,
                    Description = ex.Detail.Message
                };
                return response;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return response;
            }
        }

        [HttpPost]
        public ApiResponseBase GetBonusStatusesEnum(ApiRequestBase input)
        {
            try
            {
                return new ApiResponseBase
                {
                    ResponseObject = BaseBll.GetEnumerations(Constants.EnumerationTypes.BonusStates, input.LanguageId)
                    .Where(x => x.NickName != BonusStatuses.Finished.ToString())
                    .Select(x => new
                    {
                        Id = x.Value,
                        Name = x.Text
                    }).ToList()
                };
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                Program.DbLogger.Error(ex);
                var response = new ApiResponseBase
                {
                    ResponseCode = ex.Detail.Id,
                    Description = ex.Detail.Message
                };
                return response;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return response;
            }
        }

        [HttpPost]
        public ApiResponseBase GetRegions(ApiFilterRegion input)
        {
            try
            {
                using (var regionBl = new RegionBll(new SessionIdentity(), Program.DbLogger))
                {
                    return new ApiResponseBase
                    {
                        ResponseObject = regionBl.GetfnRegions(new FilterRegion { ParentId = input.ParentId, TypeId = input.TypeId },
                            input.LanguageId, false, input.PartnerId).Where(x => x.State == (int)RegionStates.Active).OrderBy(x => x.Name).MapToRegionModels()
                    };
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                Program.DbLogger.Error(ex);
                var response = new ApiResponseBase
                {
                    ResponseCode = ex.Detail.Id,
                    Description = ex.Detail.Message
                };
                return response;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return response;
            }
        }

        [HttpPost]
        public ApiResponseBase GetJobAreas(ApiRequestBase input)
        {
            try
            {
                var partner = CacheManager.GetPartnerById(input.PartnerId);
                if (partner == null || !partner.SiteUrl.Split(',').Any(x => x == input.Domain))
                    throw BaseBll.CreateException(input.LanguageId, Constants.Errors.PartnerNotFound);

                return new ApiResponseBase
                {
                    ResponseObject = CacheManager.GetJobAreas(input.LanguageId).Select(x => new { x.Id, x.NickName, x.Name, x.Info }).ToList()
                };

            }
            catch (FaultException<BllFnErrorType> ex)
            {
                Program.DbLogger.Error(ex);
                var response = new ApiResponseBase
                {
                    ResponseCode = ex.Detail.Id,
                    Description = ex.Detail.Message
                };
                return response;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return response;
            }
        }

        [HttpPost]
        public ApiResponseBase GetTicketInfoByBarcode(GetTicketInfoByBarcodeInput barcodeInput)
        {
            try
            {
                using (var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger))
                {
                    HttpRequestInput requestObject = null;
                    barcodeInput.Barcode -= 1000000000000;
                    barcodeInput.Barcode /= 10;
                    var document = documentBl.GetDocumentById(barcodeInput.Barcode);
                    if (document == null || document.ProductId == null)
                        throw BaseBll.CreateException(barcodeInput.LanguageId, Constants.Errors.DocumentNotFound);
                    var product = CacheManager.GetProductById(document.ProductId.Value);
                    var provider = CacheManager.GetGameProviderById(product.GameProviderId.Value);
                    var providerName = provider.Name.ToLower();
                    if (providerName == Constants.GameProviders.IqSoft.ToLower() || providerName == Constants.GameProviders.Internal.ToLower())
                    {
                        requestObject = Integration.Products.Helpers.InternalHelpers.GetBetInfo(product, document.ExternalTransactionId, barcodeInput.LanguageId, product.ExternalId);
                    }
                    if (requestObject != null && requestObject.Url != null)
                    {
                        requestObject.RequestHeaders = new Dictionary<string, string>();
                        var response = JsonConvert.DeserializeObject<ApiResponseBase>(CommonFunctions.SendHttpRequest(requestObject, out _));
                        if (response.ResponseCode != Constants.SuccessResponseCode)
                            throw BaseBll.CreateException(barcodeInput.LanguageId, Constants.Errors.GeneralException);
                        var info = JsonConvert.DeserializeObject<ApiGetBetInfoOutput>(response.ResponseObject.ToString());
                        info.BetDate = info.BetDate.GetGMTDateFromUTC(barcodeInput.TimeZone);
                        info.Status = document.State;
                        foreach (var sel in info.BetSelections)
                        {
                            sel.EventDate = sel.EventDate.GetGMTDateFromUTC(barcodeInput.TimeZone);
                        }
                        response.ResponseObject = info;
                        return response;
                    }
                    throw BaseBll.CreateException(barcodeInput.LanguageId, Constants.Errors.WrongDocumentId);
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = ex.Detail.Id,
                    Description = ex.Detail.Message
                };
                Program.DbLogger.Error(JsonConvert.SerializeObject(response));
                return response;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return response;
            }
        }

        [HttpPost]
        public ApiResponseBase GetWelcomeBonus(ApiRegBonusInput input)
        {
            try
            {
                var identity = new SessionIdentity { LanguageId = input.LanguageId, PartnerId = input.PartnerId };
                using (var clientBl = new ClientBll(identity, Program.DbLogger))
                {
                    var info = clientBl.GetClientInfoByKey(input.ActivationKey, (int)ClientInfoTypes.WelcomeBonusActivationKey, true);
                    var client = CacheManager.GetClientById(info.ClientId ?? 0);
                    identity = new SessionIdentity { LanguageId = input.LanguageId, PartnerId = client.PartnerId };
                    using (var bonusBl = new BonusService(identity, Program.DbLogger))
                    {
                        var currentDate = DateTime.UtcNow;
                        var shuffledItems = bonusBl.ShuffleWelcomeBonusItems();
                        if (input.Index >= shuffledItems.Length)
                            throw BaseBll.CreateException(input.LanguageId, Constants.Errors.WrongInputParameters);

                        decimal bonusPrize = shuffledItems[input.Index];
                        if (bonusPrize > 0)
                            bonusBl.CreateClientBonus(client.Id, bonusPrize);

                        return new ApiResponseBase
                        {
                            ResponseObject = shuffledItems
                        };
                    }
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = ex.Detail.Id,
                    Description = ex.Detail.Message
                };
                Program.DbLogger.Error(JsonConvert.SerializeObject(response));
                return response;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return response;
            }
        }


        [HttpPost]
        public ApiResponseBase GetBanners(ApiBannerInput input)
        {
            try
            {
                var partner = CacheManager.GetPartnerById(input.PartnerId);
                if (partner == null || !partner.SiteUrl.Split(',').Any(x => x == input.Domain))
                    throw BaseBll.CreateException(input.LanguageId, Constants.Errors.PartnerNotFound);

                return new ApiResponseBase
                {
                    ResponseObject = CacheManager.GetBanners(input.PartnerId, input.Type, input.LanguageId).Select(x => new ApiBannerOutput
                    {
                        Body = x.Body,
                        Head = x.Head,
                        Link = x.Link,
                        Order = x.Order,
                        Image = x.Image.Split(',')[0],
                        ImageSizes = x.Image.Split(',').Skip(1).ToList(),
                        ShowDescription = x.ShowDescription,
                        Visibility = x.Visibility,
                        ButtonType = x.ButtonType,
                        Segments = x.Segments == null ? new Common.Models.AdminModels.ApiSetting() :
                            new Common.Models.AdminModels.ApiSetting { Type = x.Segments.Type, Ids = x.Segments.Ids }
                    }).OrderBy(x => x.Order).ToList()
                };
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = ex.Detail.Id,
                    Description = ex.Detail.Message
                };
                Program.DbLogger.Error(JsonConvert.SerializeObject(response));
                return response;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return response;
            }
        }

        [HttpPost]
        public ApiResponseBase GetPromotions(ApiRequestBase input)
        {
            try
            {
                var partner = CacheManager.GetPartnerById(input.PartnerId);
                if (partner == null || !partner.SiteUrl.Split(',').Any(x => x == input.Domain))
                    throw BaseBll.CreateException(input.LanguageId, Constants.Errors.PartnerNotFound);

                return new ApiResponseBase
                {
                    ResponseObject = CacheManager.GetPromotions(input.PartnerId, input.LanguageId).Select(x => new ApiPromotion
                    {
                        Id = x.Id,
                        Title = x.Title,
                        Description = x.Description,
                        Type = x.Type,
                        ImageName = x.ImageName,
                        Segments = x.Segments == null ? new Common.Models.AdminModels.ApiSetting() :
                            new Common.Models.AdminModels.ApiSetting { Type = x.Segments.Type, Ids = x.Segments.Ids },
                        Languages = x.Languages == null ? new Common.Models.AdminModels.ApiSetting() :
                            new Common.Models.AdminModels.ApiSetting { Type = x.Languages.Type, Names = x.Languages.Names },
                        Order = x.Order
                    }).ToList()
                };
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = ex.Detail.Id,
                    Description = ex.Detail.Message
                };
                Program.DbLogger.Error(JsonConvert.SerializeObject(response));
                return response;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return response;
            }
        }

        [HttpPost]
        public ApiResponseBase GetTicker(ApiRequestBase input)
        {
            try
            {
                var partner = CacheManager.GetPartnerById(input.PartnerId);
                if (partner == null || !partner.SiteUrl.Split(',').Any(x => x == input.Domain))
                    throw BaseBll.CreateException(input.LanguageId, Constants.Errors.PartnerNotFound);
                return new ApiResponseBase
                {
                    ResponseObject = CacheManager.GetPartnerTicker(input.PartnerId, input.LanguageId)
                };
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = ex.Detail.Id,
                    Description = ex.Detail.Message
                };
                Program.DbLogger.Error(JsonConvert.SerializeObject(response));
                return response;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return response;
            }
        }

        [HttpPost]
        public ApiResponseBase GetGeolocationData([FromRoute] int partnerId, ApiRequestBase input)
        {
            try
            {
                using (var regionBl = new RegionBll(new SessionIdentity(), Program.DbLogger))
                {
                    using (var partnerBl = new PartnerBll(regionBl))
                    {
                        var result = regionBl.GetRegionByCountryCode(input.CountryCode);
                        if (result == null)
                            result = new DAL.Region();

                        var partner = partnerBl.GetpartnerByDomain(input.Domain);
                        if (partner == null)
                        {
                            result.CurrencyId = Constants.DefaultCurrencyId;
                            result.LanguageId = Constants.DefaultLanguageId;
                        }
                        else
                        {
                            var partnerLanguages = partnerBl.GetPartnerLenguages(partner.Id);
                            var partnerCurrencies = partnerBl.GetPartnerCurrencies(partner.Id);
                            if (string.IsNullOrEmpty(result.LanguageId) || !partnerLanguages.Contains(result.LanguageId))
                            {
                                var dl = CacheManager.GetPartnerSettingByKey(partner.Id, "DefaultLanguage");
                                result.LanguageId = string.IsNullOrEmpty(dl.StringValue) ? Constants.DefaultLanguageId : dl.StringValue;
                            }
                            if (string.IsNullOrEmpty(result.CurrencyId) || !partnerCurrencies.Contains(result.CurrencyId))
                            {
                                result.CurrencyId = partner.CurrencyId;
                            }
                        }

                        return new ApiResponseBase
                        {
                            ResponseObject = new
                            {
                                result.Id,
                                input.CountryCode,
                                result.LanguageId,
                                result.CurrencyId
                            }
                        };
                    }
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = ex.Detail.Id,
                    Description = ex.Detail.Message
                };
                Program.DbLogger.Error(JsonConvert.SerializeObject(response));
                return response;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return response;
            }
        }

        [HttpPost]
        public ApiResponseBase SendEmailToPartner([FromRoute] int partnerId, ApiOpenTicketInput input)
        {
            try
            {
                var notificationService = CacheManager.GetPartnerSettingByKey(partnerId, Constants.PartnerKeys.EmailNotificationService);
                if (notificationService == null || !notificationService.NumericValue.HasValue)
                    throw BaseBll.CreateException(input.LanguageId, Constants.Errors.AccountNotFound);

                var notificationServieId = (int)notificationService.NumericValue.Value;
                var partnerEmail = CacheManager.GetNotificationServiceValueByKey(partnerId, Constants.PartnerKeys.NotificationMail, notificationServieId);
                using (var notificationBl = new NotificationBll(new SessionIdentity { Domain = input.Domain }, Program.DbLogger))
                {
                    var messageTemplate = CacheManager.GetPartnerMessageTemplate(partnerId, (int)ClientInfoTypes.EmailToPartner, notificationBl.LanguageId);
                    var messageText = messageTemplate.Text
                                          .Replace("\\n", Environment.NewLine)
                                          .Replace("{ce}", input.Email)
                                          .Replace("{em}", input.Message);

                    notificationBl.RegisterActiveEmail(partnerId, partnerEmail, input.Subject, messageText, messageTemplate.Id);
                }
                return new ApiResponseBase();
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = ex.Detail.Id,
                    Description = ex.Detail.Message
                };
                Program.DbLogger.Error(JsonConvert.SerializeObject(response));
                return response;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return response;
            }
        }


        [HttpPost]
        public ApiResponseBase GetClientData(RequestBase input)
        {
            try
            {
                var session = CacheManager.GetClientPlatformSession(input.ClientId, null, true);
                if (session == null || session.Token != input.Token)
                    throw BaseBll.CreateException(input.LanguageId, Constants.Errors.WrongToken);
                var segments = CacheManager.GetClientClasifications(input.ClientId);

                return new ApiResponseBase
                {
                    ResponseObject = new ApiClientData
                    {
                        ClientId = input.ClientId,
                        Token = session.Token,
                        DepCount = CacheManager.GetClientDepositCount(input.ClientId),
                        Segments = segments.Where(x => x.SegmentId != null).Select(x => x.SegmentId.Value).ToList()
                    }
                };
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = ex.Detail.Id,
                    Description = ex.Detail.Message
                };
                Program.DbLogger.Error(JsonConvert.SerializeObject(response));
                return response;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return response;
            }
        }


        [HttpPost]
        public ApiResponseBase GetApiRestrictions(int partnerId)
        {
            try
            {
                var partner = CacheManager.GetPartnerById(partnerId);
                if (partner == null)
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerNotFound);
                using (var contentBl = new ContentBll(new SessionIdentity(), Program.DbLogger))
                {
                    var registrationLimitPerDay = CacheManager.GetConfigKey(partnerId, Constants.PartnerKeys.RegistrationLimitPerDay);
                    return new ApiResponseBase
                    {
                        ResponseObject = new ApiRestrictionModel
                        {
                            WhitelistedCountries = CacheManager.GetConfigParameters(partnerId, PartnerKeys.WhitelistedCountries).Select(x => x.Value).ToList() ?? new List<string>(),
                            BlockedCountries = CacheManager.GetConfigParameters(partnerId, PartnerKeys.BlockedCountries).Select(x => x.Value).ToList() ?? new List<string>(),
                            WhitelistedIps = CacheManager.GetConfigParameters(partnerId, PartnerKeys.WhitelistedIps).Select(x => x.Value).ToList() ?? new List<string>(),
                            BlockedIps = CacheManager.GetConfigParameters(partnerId, PartnerKeys.BlockedIps).Select(x => x.Value).ToList() ?? new List<string>(),
                            RegistrationLimitPerDay = int.TryParse(registrationLimitPerDay, out int limit) ? limit : (int?)null
                        }
                    };
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = ex.Detail.Id,
                    Description = ex.Detail.Message
                };
                Program.DbLogger.Error(JsonConvert.SerializeObject(response));
                return response;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return response;
            }
        }

        [HttpPost]
        public ApiResponseBase GetErrorType(ApiGetErrorTypeInput input)
        {
            try
            {
                using (var contentBl = new ContentBll(new SessionIdentity(), Program.DbLogger))
                {
                    var error = CacheManager.GetfnErrorTypes(input.LanguageId)?.FirstOrDefault(x => x.Id == input.Id);
                    var resp = new ApiResponseBase();
                    if (error == null)
                        resp.ResponseCode = Constants.Errors.WrongInputParameters;
                    else
                        resp.ResponseObject = new
                        {
                            Id = error.Id,
                            NickName = error.NickName,
                            Message = error.Message
                        };

                    return resp;
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = ex.Detail.Id,
                    Description = ex.Detail.Message
                };
                Program.DbLogger.Error(JsonConvert.SerializeObject(response));
                return response;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return response;
            }
        }
        /*
        [HttpPost]
        public ApiResponseBase SendVerificationCode(ApiNotificationInput request)
        {
            try
            {
                var partner = CacheManager.GetPartnerById(request.PartnerId);
                if (partner == null || !partner.SiteUrl.Split(',').Any(x => x == request.Domain))
                    throw BaseBll.CreateException(request.LanguageId, Constants.Errors.PartnerNotFound);
                var sessionIdentity = new SessionIdentity
                {
                    LanguageId = request.LanguageId,
                    Domain = request.Domain,
                    LoginIp = request.Ip
                };
                if (request.ClientId.HasValue && request.ClientId != 0)
                {
                    var session = Helpers.Helpers.CheckToken(request.Token, request.ClientId.Value, request.TimeZone);
                    var client = CacheManager.GetClientById(session.Id);
                    request.Email = client.Email;
                    request.MobileNumber = client.MobileNumber;
                }
                if (!Enum.IsDefined(typeof(VerificationCodeTypes), request.Type))
                    throw BaseBll.CreateException(request.LanguageId, Constants.Errors.WrongInputParameters);
                var clientInfoType = ((VerificationCodeTypes)request.Type).MapToClientInfoType();
                var isEmail = clientInfoType.ToString().ToLower().Contains("email");
                var notificationLimit = CacheManager.GetPartnerSettingByKey(partner.Id, Constants.PartnerKeyByClientInfoType[(int)clientInfoType]);
                if (notificationLimit.Id > 0 && notificationLimit.NumericValue > 0)
                {
                    var count = CacheManager.UpdateVerifyCodeRequestCount(isEmail ? request.Email : request.MobileNumber);
                    if (count > notificationLimit.NumericValue)
                        throw BaseBll.CreateException(request.LanguageId, Constants.Errors.ClientMaxLimitExceeded);
                }
                using (var notificationBl = new NotificationBll(sessionIdentity, Program.DbLogger))
                {
                    using (var clientBl = new ClientBll(notificationBl))
                    {
                        var partnerSetting = CacheManager.GetConfigKey(partner.Id, Constants.PartnerKeys.VerificationKeyNumberOnly);
                        var codeLength = isEmail ? partner.EmailVerificationCodeLength : partner.MobileVerificationCodeLength;

                        var verificationKey = (!string.IsNullOrEmpty(partnerSetting) && partnerSetting == "1") ? CommonFunctions.GetRandomNumber(codeLength) :
                                                                                                                 CommonFunctions.GetRandomString(codeLength);

                        var activePeriodInMinutes = notificationBl.SendNotificationMessage(new NotificationModel
                        {
                            PartnerId = request.PartnerId,
                            ClientId = request.ClientId,
                            MobileOrEmail =request.MobileNumber,
                            ClientInfoType = (int)clientInfoType,
                            VerificationCode = verificationKey,
                            LanguageId = request.LanguageId,
                            PaymentInfo = request.PaymentInfo?.MapToPaymentNotificationInfo()
                        });
                        return new ApiResponseBase
                        {
                            ResponseObject = new { ActivePeriodInMinutes = activePeriodInMinutes }
                        };
                    }
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = ex.Detail.Id,
                    Description = ex.Detail.Message
                };
                Program.DbLogger.Error(JsonConvert.SerializeObject(response));
                return response;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return response;
            }
        }
        */
        [HttpPost]
        public ApiResponseBase SendSMSCode(ApiNotificationInput request)
        {
            try
            {
                var partner = CacheManager.GetPartnerById(request.PartnerId);
                if (partner == null || !partner.SiteUrl.Split(',').Any(x => x == request.Domain))
                    throw BaseBll.CreateException(request.LanguageId, Constants.Errors.PartnerNotFound);
                var notificationLimit = CacheManager.GetPartnerSettingByKey(partner.Id, Constants.PartnerKeyByClientInfoType[(int)ClientInfoTypes.MobileVerificationKey]);
                if (notificationLimit.Id > 0 && notificationLimit.NumericValue > 0)
                {
                    var count = CacheManager.UpdateVerifyCodeRequestCount(request.MobileNumber);
                    if (count > notificationLimit.NumericValue)
                        throw BaseBll.CreateException(request.LanguageId, Constants.Errors.ClientMaxLimitExceeded);
                }
                var sessionIdentity = new SessionIdentity
                {
                    LanguageId = request.LanguageId,
                    Domain = request.Domain,
                    LoginIp = request.Ip
                };
                using var notificationBl = new NotificationBll(sessionIdentity, Program.DbLogger);
                using var clientBl = new ClientBll(notificationBl);
                var partnerSetting = CacheManager.GetConfigKey(partner.Id, Constants.PartnerKeys.VerificationKeyNumberOnly);
                var verificationKey = (!string.IsNullOrEmpty(partnerSetting) && partnerSetting == "1") ? CommonFunctions.GetRandomNumber(partner.MobileVerificationCodeLength) :
                                                                                                         CommonFunctions.GetRandomString(partner.MobileVerificationCodeLength);
                var activePeriodInMinutes = notificationBl.SendNotificationMessage(new NotificationModel
                {
                    PartnerId = request.PartnerId,
                    ClientId = request.ClientId,
                    MobileOrEmail =request.MobileNumber,
                    ClientInfoType = (int)ClientInfoTypes.MobileVerificationKey,
                    VerificationCode = verificationKey,
                    LanguageId = request.LanguageId
                });
                return new ApiResponseBase();
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                Program.DbLogger.Error(ex.Detail);
                return new ApiResponseBase
                {
                    ResponseCode = ex.Detail.Id,
                    Description = ex.Detail.Message
                };
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                return new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
            }
        }

        [HttpPost]
        public ApiResponseBase SendEmailCode(ApiNotificationInput request)
        {
            try
            {
                var partner = CacheManager.GetPartnerById(request.PartnerId);
                if (partner == null || !partner.SiteUrl.Split(',').Any(x => x == request.Domain))
                    throw BaseBll.CreateException(request.LanguageId, Constants.Errors.PartnerNotFound);
                var notificationLimit = CacheManager.GetPartnerSettingByKey(partner.Id, Constants.PartnerKeyByClientInfoType[(int)ClientInfoTypes.EmailVerificationKey]);
                if (notificationLimit.Id > 0 && notificationLimit.NumericValue > 0)
                {
                    var count = CacheManager.UpdateVerifyCodeRequestCount(request.Email);
                    if (count > notificationLimit.NumericValue)
                        throw BaseBll.CreateException(request.LanguageId, Constants.Errors.ClientMaxLimitExceeded);
                }
                var sessionIdentity = new SessionIdentity
                {
                    LanguageId = request.LanguageId,
                    Domain = request.Domain,
                    LoginIp = request.Ip
                };
                using var notificationBl = new NotificationBll(sessionIdentity, Program.DbLogger);
                using var clientBl = new ClientBll(notificationBl);
                var partnerSetting = CacheManager.GetConfigKey(partner.Id, Constants.PartnerKeys.VerificationKeyNumberOnly);
                var verificationKey = (!string.IsNullOrEmpty(partnerSetting) && partnerSetting == "1") ? CommonFunctions.GetRandomNumber(partner.EmailVerificationCodeLength) :
                                                                                                         CommonFunctions.GetRandomString(partner.EmailVerificationCodeLength);
                var activePeriodInMinutes = notificationBl.SendNotificationMessage(new NotificationModel
                {
                    PartnerId = request.PartnerId,
                    ClientId = request.ClientId,
                    MobileOrEmail =request.Email,
                    ClientInfoType = (int)ClientInfoTypes.EmailVerificationKey,
                    VerificationCode = verificationKey,
                    LanguageId = request.LanguageId
                });
                return new ApiResponseBase();
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                Program.DbLogger.Error(ex.Detail);
                return new ApiResponseBase
                {
                    ResponseCode = ex.Detail.Id,
                    Description = ex.Detail.Message
                };
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                return new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
            }
        }

        [HttpPost]
        public ApiResponseBase VerifySMSCode(ApiNotificationInput request)
        {
            try
            {
                var partner = CacheManager.GetPartnerById(request.PartnerId);
                if (partner == null || !partner.SiteUrl.Split(',').Any(x => x == request.Domain))
                    throw BaseBll.CreateException(request.LanguageId, Constants.Errors.PartnerNotFound);
                var sessionIdentity = new SessionIdentity
                {
                    LanguageId = request.LanguageId,
                    Domain = request.Domain,
                    LoginIp = request.Ip
                };
                using (var clientBl = new ClientBll(sessionIdentity, Program.DbLogger))
                {
                    return new ApiResponseBase
                    {
                        ResponseObject = clientBl.VerifyClientMobileNumber(request.Code, request.MobileNumber, null, request.PartnerId, false, null)
                    };
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = ex.Detail.Id,
                    Description = ex.Detail.Message
                };
                Program.DbLogger.Error(JsonConvert.SerializeObject(response));
                return response;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                return new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
            }
        }

        [HttpPost]
        public ApiResponseBase VerifyEmailCode(ApiNotificationInput request)
        {
            try
            {
                var partner = CacheManager.GetPartnerById(request.PartnerId);
                if (partner == null || !partner.SiteUrl.Split(',').Any(x => x == request.Domain))
                    throw BaseBll.CreateException(request.LanguageId, Constants.Errors.PartnerNotFound);
                var sessionIdentity = new SessionIdentity
                {
                    LanguageId = request.LanguageId,
                    Domain = request.Domain,
                    LoginIp = request.Ip
                };
                using (var clientBl = new ClientBll(sessionIdentity, Program.DbLogger))
                {
                    return new ApiResponseBase
                    {
                        ResponseObject = clientBl.VerifyClientEmail(request.Code, request.Email, null, request.PartnerId, false, null)
                    };
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = ex.Detail.Id,
                    Description = ex.Detail.Message
                };
                Program.DbLogger.Error(JsonConvert.SerializeObject(response));
                return response;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                return new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
            }
        }
        /*
        [HttpPost]
        public ApiResponseBase VerifyRecoveryCode(ApiNotificationInput request)
        {
            try
            {
                var partner = CacheManager.GetPartnerById(request.PartnerId);
                if (partner == null || !partner.SiteUrl.Split(',').Any(x => x == request.Domain))
                    throw BaseBll.CreateException(request.LanguageId, Constants.Errors.PartnerNotFound);
                var sessionIdentity = new SessionIdentity
                {
                    LanguageId = request.LanguageId,
                    Domain = request.Domain,
                    LoginIp = request.Ip
                };
                if (!request.ClientId.HasValue)
                    throw BaseBll.CreateException(request.LanguageId, Constants.Errors.ClientNotFound);

                using (var clientBl = new ClientBll(sessionIdentity, Program.DbLogger))
                {
                    var result = clientBl.VerifyRecoveryCode(request.Code, request.ClientId.Value, request.PartnerId, false, out string mobileOrEmail);
                    return new ApiResponseBase
                    {
                        ResponseObject = new
                        {
                            MobileOrEmail = mobileOrEmail,
                            SeqQuestions = result
                        }
                    };
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = ex.Detail.Id,
                    Description = ex.Detail.Message
                };
                Program.DbLogger.Error(JsonConvert.SerializeObject(response));
                return response;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                return new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
            }
        }
        */
        [HttpPost]
        public ApiResponseBase GetJackpotFeed(ApiRequestBase request)
        {
            try
            {
                var partner = CacheManager.GetPartnerById(request.PartnerId);
                if (partner == null || !partner.SiteUrl.Split(',').Any(x => x == request.Domain))
                    throw BaseBll.CreateException(request.LanguageId, Constants.Errors.PartnerNotFound);
                using (var productBl = new ProductBll(new SessionIdentity { LanguageId = request.LanguageId, Domain = request.Domain }, Program.DbLogger))
                {
                    return new ApiResponseBase
                    {
                        ResponseObject = productBl.GetJackpot(request.PartnerId)
                    };
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = ex.Detail.Id,
                    Description = ex.Detail.Message
                };
                Program.DbLogger.Error(JsonConvert.SerializeObject(response));
                return response;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return response;
            }
        }

        [HttpPost]
        public ApiResponseBase GetJackpots(ApiRequestBase request)
        {
            try
            {
                var partner = CacheManager.GetPartnerById(request.PartnerId);
                if (partner == null || !partner.SiteUrl.Split(',').Any(x => x == request.Domain))
                    throw BaseBll.CreateException(request.LanguageId, Constants.Errors.PartnerNotFound);
                using (var bonusService = new BonusService(new SessionIdentity { LanguageId = request.LanguageId, Domain = request.Domain }, Program.DbLogger))
                {
                    return new ApiResponseBase
                    {
                        ResponseObject = bonusService.GetJackpots(request.PartnerId).Select(x => new { x.Name, x.Amount }).ToList()
                    };
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = ex.Detail.Id,
                    Description = ex.Detail.Message
                };
                Program.DbLogger.Error(JsonConvert.SerializeObject(response));
                return response;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return response;
            }
        }

        [HttpPost]
        public ApiResponseBase GetGames(ApiGetGamesInput input)
        {
            try
            {
                var partner = CacheManager.GetPartnerById(input.PartnerId);
                if (partner == null || !partner.SiteUrl.Split(',').Any(x => x == input.Domain))
                    throw BaseBll.CreateException(input.LanguageId, Constants.Errors.PartnerNotFound);
                using (var productBl = new ProductBll(new SessionIdentity { LanguageId = input.LanguageId, Domain = input.Domain }, Program.DbLogger))
                {
                    var skipCount = input.PageIndex * input.PageSize;
                    var takeCount = input.PageSize;
                    var favoriteProducts = new List<int>();
                    var partnerGameProviderSettings = CacheManager.GetGameProviderSettings((int)ObjectTypes.Partner, partner.Id);
                    var blockedProviders = partnerGameProviderSettings.Where(x => x.State == (int)BaseStates.Inactive).Select(x => x.GameProviderId).ToList();
                    if (!string.IsNullOrEmpty(input.Token))
                    {
                        Helpers.Helpers.CheckToken(input.Token, input.ClientId ?? 0, input.TimeZone);
                        favoriteProducts = CacheManager.GetClientFavoriteProducts(input.ClientId ?? 0).Select(x => x.ProductId).ToList();
                        var clientBlockedProviders = CacheManager.GetGameProviderSettings((int)ObjectTypes.Client, input.ClientId ?? 0)
                                                    .Where(x => x.State == (int)BaseStates.Inactive && !blockedProviders.Contains(x.GameProviderId))
                                                    .Select(x => x.GameProviderId).ToList();
                        blockedProviders.AddRange(clientBlockedProviders);
                    }
                    var restrictedProducts = CacheManager.GetRestrictedProductCountrySettings(input.CountryCode);
                    var games = (input.IsForMobile ? CacheManager.GetPartnerProductSettingsForMobile(input.PartnerId, input.LanguageId) :
                                                     CacheManager.GetPartnerProductSettingsForDesktop(input.PartnerId, input.LanguageId))
                        .Where(x => !blockedProviders.Contains(x.SubproviderId) && !restrictedProducts.Contains(x.ProductId))
                        .AsEnumerable();

                    if (input.CategoryId != null)
                    {
                        if (input.CategoryId == 0)
                            games = games.Where(x => favoriteProducts.Contains(x.ProductId));
                        else
                            games = games.Where(x => x.CategoryId == input.CategoryId);
                    }
                    else
                    {
                        if (input.CategoryIds != null && input.CategoryIds.Any())
                            games = games.Where(x => x.CategoryId != null && input.CategoryIds.Contains(x.CategoryId.Value));
                        else
                        {
                            var casinoMenues = CacheManager.GetCasinoMenues(partner.Id);
                            games = games.Where(x => x.CategoryId != null && casinoMenues.Any(y => y.Type == x.CategoryId.Value.ToString()));
                        }
                    }
                    if (!string.IsNullOrEmpty(input.Name))
                        games = games.Where(x => x.Name.ToLower().Contains(input.Name.ToLower()));
                    games = games.OrderByDescending(x => x.Rating).ThenBy(x => x.Name);
                    var gamesCategories = games.Where(x => x.CategoryId.HasValue).Select(x => x.CategoryId).ToList();
                    var categories = CacheManager.GetProductCategories(input.PartnerId, input.LanguageId, (int)ProductCategoryTypes.ForPartner)
                             .Where(x => x.Type == (int)ProductCategoryTypes.ForPartner && gamesCategories.Contains(x.Id))
                             .Select(x => new { x.Id, x.Name }).ToList();
                    var providers = games.Select(x => x.SubproviderId).Distinct().ToList();
                    if (input.ProviderIds != null && input.ProviderIds.Any())
                        games = games.Where(x => input.ProviderIds.Contains(x.SubproviderId));
                    var gamesCount = games.Count();

                    var providerGamesCount = games.GroupBy(x => x.SubproviderId).ToDictionary(x => x.Key, x => x.Count());
                    games = games.Skip(skipCount).Take(takeCount);
                    var result = new List<ApiPartnerProduct>();
                    foreach (var g in games)
                    {
                        var game = CacheManager.GetProductById(g.ProductId);
                        var imageUrl = input.IsForMobile ? game.MobileImageUrl : game.WebImageUrl;
                        var providerName = CacheManager.GetGameProviderById(game.GameProviderId.Value).Name;

                        result.Add(new ApiPartnerProduct
                        {
                            i = string.IsNullOrEmpty(imageUrl) ? string.Empty : imageUrl,
                            n = g.Name,
                            nn = g.NickName,
                            s = g.SubproviderId,
                            p = g.ProductId,
                            sn = providerName,
                            r = g.Rating ?? 0,
                            o = g.OpenMode ?? (int)GameOpenModes.Small,
                            ss = g.SubproviderId,
                            sp = g.ProviderName,
                            hd = game.HasDemo && g.HasDemo.HasValue ? g.HasDemo.Value : game.HasDemo,
                            jp = game.Jackpot,
                            c = g.CategoryId,
                            f = favoriteProducts.Contains(g.ProductId)
                        });
                    }

                    return new ApiResponseBase
                    {
                        ResponseObject = new
                        {
                            Games = result,
                            Providers = providers.Select(x => new
                            {
                                Id = x,
                                Name = CacheManager.GetGameProviderById(x)?.Name,
                                GamesCount = providerGamesCount.ContainsKey(x) ? providerGamesCount[x] : 0,
                                Order = partnerGameProviderSettings.FirstOrDefault(y => y.GameProviderId == x)?.Order ?? 10000
                            }).OrderBy(x => x.Order).ThenBy(x => x.Name).ToList(),
                            Categories = categories,
                            TotalGamesCount = gamesCount,
                            LeftGamesCount = Math.Max(0, gamesCount - skipCount - takeCount)
                        }
                    };
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = ex.Detail.Id,
                    Description = ex.Detail.Message
                };
                Program.DbLogger.Error(JsonConvert.SerializeObject(response));
                return response;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return response;
            }
        }


        [HttpPost]
        public ApiResponseBase GetGameProviders(ApiGetGamesInput input)
        {
            try
            {
                if (!string.IsNullOrEmpty(input.Pattern))
                    input.Pattern = input.Pattern.ToLower();

                var partner = CacheManager.GetPartnerById(input.PartnerId);
                if (partner == null || !partner.SiteUrl.Split(',').Any(x => x == input.Domain))
                    throw BaseBll.CreateException(input.LanguageId, Constants.Errors.PartnerNotFound);
                using (var productBl = new ProductBll(new SessionIdentity { LanguageId = input.LanguageId, Domain = input.Domain }, Program.DbLogger))
                {
                    var favoriteProducts = new List<int>();
                    var blockedProviders = CacheManager.GetGameProviderSettings((int)ObjectTypes.Partner, partner.Id).Where(x => x.State == (int)BaseStates.Inactive).Select(x => x.GameProviderId).ToList();
                    if (!string.IsNullOrEmpty(input.Token))
                    {
                        Helpers.Helpers.CheckToken(input.Token, input.ClientId ?? 0, input.TimeZone);
                        favoriteProducts = CacheManager.GetClientFavoriteProducts(input.ClientId ?? 0).Select(x => x.ProductId).ToList();
                        blockedProviders.AddRange(CacheManager.GetGameProviderSettings((int)ObjectTypes.Client, input.ClientId ?? 0).Where(x => x.State == (int)BaseStates.Inactive).Select(x => x.GameProviderId).ToList());
                    }
                    var restrictedProducts = CacheManager.GetRestrictedProductCountrySettings(input.CountryCode);
                    var product = (input.IsForMobile ? CacheManager.GetPartnerProductSettingsForMobile(input.PartnerId, input.LanguageId) :
                                                    CacheManager.GetPartnerProductSettingsForDesktop(input.PartnerId, input.LanguageId))
                       .Where(x => !blockedProviders.Contains(x.SubproviderId) && !restrictedProducts.Contains(x.ProductId) &&
                        (input.CategoryIds == null || !input.CategoryIds.Any()|| input.CategoryIds.Contains(x.CategoryId.Value)))
                       .AsEnumerable();
                    var partnerGameProviderSettings = CacheManager.GetGameProviderSettings((int)ObjectTypes.Partner, partner.Id);
                    if (!string.IsNullOrEmpty(input.Token))
                        blockedProviders.AddRange(CacheManager.GetGameProviderSettings((int)ObjectTypes.Client, input.ClientId ?? 0)
                                        .Where(x => x.State == (int)BaseStates.Inactive).Select(x => x.GameProviderId).ToList());
                    var providers = product.Where(x => string.IsNullOrEmpty(input.Pattern) || x.ProviderName.ToLower().Contains(input.Pattern)  &&
                                                       !blockedProviders.Contains(x.SubproviderId))
                                           .Select(x => new
                                           {
                                               Id = x.SubproviderId,
                                               Name = x.ProviderName,
                                               Order = partnerGameProviderSettings.FirstOrDefault(y => y.GameProviderId == x.SubproviderId)?.Order ?? 10000
                                           })
                                           .Distinct().OrderBy(x => x.Order).ThenBy(x => x.Name).ToList();
                    var categories = CacheManager.GetProductCategories(input.PartnerId, input.LanguageId, (int)ProductCategoryTypes.ForPartner)
                                                 .Where(x => x.Type == (int)ProductCategoryTypes.ForPartner && x.Name.ToLower().Contains(input.Pattern ?? string.Empty))
                                                 .Select(x => new { x.Id, x.Name }).ToList();
                    var games = product.Where(x => x.Name.ToLower().Contains(input.Pattern ?? string.Empty)).Take(100).ToList();
                    var resp = new List<object>();
                    foreach (var g in games)
                    {
                        var game = CacheManager.GetProductById(g.ProductId);
                        var imageUrl = input.IsForMobile ? game.MobileImageUrl : game.WebImageUrl;
                        resp.Add(new
                        {
                            Id = g.ProductId,
                            Name = g.Name,
                            ImageUrl = string.IsNullOrEmpty(imageUrl) ? string.Empty : imageUrl,
                            IsFavorite = favoriteProducts.Contains(game.Id),
                            HasDemo = game.HasDemo && g.HasDemo.HasValue ? g.HasDemo.Value : game.HasDemo,
                        });
                    }
                    return new ApiResponseBase
                    {
                        ResponseObject = new
                        {
                            Providers = providers,
                            Games = resp,
                            Categories = categories
                        }
                    };
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = ex.Detail.Id,
                    Description = ex.Detail.Message
                };
                Program.DbLogger.Error(JsonConvert.SerializeObject(response));
                return response;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                return new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
            }
        }

        [HttpPost]
        public ApiResponseBase GetTicketSubjects(ApiRequestBase input)
        {
            try
            {

                var partner = CacheManager.GetPartnerById(input.PartnerId);
                if (partner == null || !partner.SiteUrl.Split(',').Any(x => x == input.Domain))
                    throw BaseBll.CreateException(input.LanguageId, Constants.Errors.PartnerNotFound);
                using (var contentBl = new ContentBll(new SessionIdentity { LanguageId = input.LanguageId, Domain = input.Domain }, Program.DbLogger))
                {
                    var result = contentBl.GetCommentTemplates((int)CommentTemplateTypes.TicketSubject, input.PartnerId, false);
                    return new ApiResponseBase
                    {
                        ResponseObject = result.Select(x => new
                        {
                            x.Id,
                            x.Text,
                            x.PartnerId,
                            x.Type
                        })
                    };
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = ex.Detail.Id,
                    Description = ex.Detail.Message
                };
                Program.DbLogger.Error(JsonConvert.SerializeObject(response));
                return response;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return response;
            }
        }

        [HttpPost]
        public ApiResponseBase GetExternalModuleUrl(ApiExternalApiInput input)
        {
            var partner = CacheManager.GetPartnerById(input.PartnerId);
            if (partner == null || !partner.SiteUrl.Split(',').Any(x => x == input.Domain))
                throw BaseBll.CreateException(input.LanguageId, Constants.Errors.PartnerNotFound);
            var client = CacheManager.GetClientById(input.ClientId);
            var responseUrl = string.Empty;
            switch (input.Type)
            {
                case (int)ExternalApiTypes.TicketingSystem:
                    if (client == null)
                        throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.ClientNotFound);
                    var session = Helpers.Helpers.CheckToken(input.Token, input.ClientId, input.TimeZone);
                    session.PartnerId = input.PartnerId;
                    session.Domain = input.Domain;
                    if (string.IsNullOrEmpty(client.Email))
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.EmailCantBeEmpty);
                    if (client.IsEmailVerified)
                        throw BaseBll.CreateException(session.LanguageId, Constants.Errors.EmailNotVerified);
                    responseUrl = TicketingSystem.CallTicketSystemApi(client.Id, session);
                    break;
                default:
                    break;
            }

            return new ApiResponseBase
            {
                ResponseObject = new { Url = responseUrl }
            };
        }

        [HttpPost]
        public ApiResponseBase GetReferralTypes(ApiRequestBase request)
        {
            try
            {
                var partner = CacheManager.GetPartnerById(request.PartnerId);
                if (partner == null || !partner.SiteUrl.Split(',').Any(x => x == request.Domain))
                    throw BaseBll.CreateException(request.LanguageId, Constants.Errors.PartnerNotFound);

                return new ApiResponseBase
                {
                    ResponseObject = BaseBll.GetEnumerations(nameof(ReferralTypes), request.LanguageId)
                                   .Select(x => x.MapToApiEnumeration()).ToList()
                };
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = ex.Detail.Id,
                    Description = ex.Detail.Message
                };
                Program.DbLogger.Error(JsonConvert.SerializeObject(response));
                return response;
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                return new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
            }
        }
    }
}