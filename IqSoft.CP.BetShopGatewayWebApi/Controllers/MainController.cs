using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Web.Http;
using System.Web.Http.Cors;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Common.Helpers;
using IqSoft.CP.DAL.Filters;
using IqSoft.CP.DAL.Filters.PaymentRequests;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.BetShopGatewayWebApi.Models;
using IqSoft.CP.BetShopGatewayWebApi.Helpers;
using IqSoft.CP.BetShopGatewayWebApi.Models.Reports;
using Newtonsoft.Json;
using IqSoft.CP.DAL.Models.Clients;
using IqSoft.CP.Common.Models.CacheModels;
using System.Threading;
using IqSoft.CP.Integration.Platforms.Helpers;
using IqSoft.CP.DAL.Models.Payments;
using IqSoft.CP.Common.Models;
using static IqSoft.CP.Common.Constants;
using ApiRequestBase = IqSoft.CP.BetShopGatewayWebApi.Models.ApiRequestBase;

namespace IqSoft.CP.BetShopGatewayWebApi.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "POST")]
    public class MainController : ApiController
    {
        [HttpPost]
        public ApiResponseBase GetApiRestrictions(ApiRequestBase input)
        {
            try
            {
                var partner = CacheManager.GetPartnerById(input.PartnerId) ??
                    throw BaseBll.CreateException(Constants.DefaultLanguageId, Constants.Errors.PartnerNotFound);
                return new ApiResponseBase
                {
                    ResponseObject = PartnerBll.GetApiPartnerRestrictions(partner.Id, SystemModuleTypes.BetShop)
                };
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = ex.Detail.Id,
                    Description = ex.Detail.Message
                };
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(response));
                return response;
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                var response = new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return response;
            }
        }

        [HttpPost]
        public IHttpActionResult CardReaderAuthorization(AuthorizationInput input)
        {
            try
            {
                WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
                using (var clientBll = new ClientBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var betShopBl = new BetShopBll(clientBll))
                    {
                        if (string.IsNullOrEmpty(input.CardNumber))
                            throw BaseBll.CreateException(input.LanguageId, Constants.Errors.ClientNotFound);
                        var authBase = Helpers.Helpers.CheckEncodedData(input.PartnerId, input.ExternalId, input.Hash, betShopBl);
                        var cashDesk = CacheManager.GetCashDeskById(authBase.Id);
                        if (cashDesk == null)
                            throw BaseBll.CreateException(input.LanguageId, Constants.Errors.CashDeskNotFound);

                        var token = ClientBll.LoginClientByCard(input.PartnerId, input.CardNumber, input.Password, input.Ip, input.LanguageId,
                            cashDesk, out int clientId);
                        Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.ClientSessions, clientId));
                        Helpers.Helpers.InvokeMessage("RemoveClient", clientId);
                        var client = CacheManager.GetClientById(clientId);
                        var verificationPlatform = CacheManager.GetConfigKey(client.PartnerId, Constants.PartnerKeys.VerificationPlatform);
                        if (!string.IsNullOrEmpty(verificationPlatform) && int.TryParse(verificationPlatform, out int verificationPatformId))
                        {
                            switch (verificationPatformId)
                            {
                                case (int)VerificationPlatforms.Insic:
                                    OASISHelpers.CheckClientStatus(client, null, input.LanguageId, new SessionIdentity(), WebApiApplication.DbLogger);
                                    var thread = new Thread(() => InsicHelpers.PlayerLogin(client.PartnerId, client.Id, WebApiApplication.DbLogger));
                                    thread.Start();
                                    break;
                                default:
                                    break;
                            }
                        }
                        return Ok(new ApiResponseBase { ResponseObject = new { Token = token } });
                    }
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new
                {
                    ResponseCode = ex.Detail.Id.ToString(),
                    Description = ex.Detail.Message
                };
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(response));
                return Ok(response);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                return Ok(new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                });
            }
        }

        [HttpPost]
        public IHttpActionResult Authorization(AuthorizationInput input)
        {
            try
            {
                using (var userBl = new UserBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var betShopBl = new BetShopBll(userBl))
                    {
                        var authBase = Helpers.Helpers.CheckCashDeskHash(input.PartnerId, input.Hash, betShopBl);
                        var loginInput = new LoginInput
                        {
                            PartnerId = input.PartnerId,
                            Identifier = input.UserName,
                            Password = input.Password,
                            Ip = input.Ip,
                            LanguageId = input.LanguageId,
                            UserType = (int)UserTypes.Cashier,
                            CashDeskId = authBase.CashDeskId
                        };
                        var session = userBl.LoginUser(loginInput, out string imageData);
                        var user = userBl.GetUserById(session.Id);
                        if (user.Type != (int)UserTypes.Cashier)
                            throw BaseBll.CreateException(input.LanguageId, Constants.Errors.UserIsNotCashier);

                        var cashDesk = CacheManager.GetCashDeskById(authBase.CashDeskId) ??
                            throw BaseBll.CreateException(input.LanguageId, Constants.Errors.CashDeskNotFound);
                        var betShop = betShopBl.GetBetShopById(cashDesk.BetShopId, true, user.Id) ??
                            throw BaseBll.CreateException(input.LanguageId, Constants.Errors.BetShopNotFound);
                        var response = new AuthorizationOutput
                        {
                            CashierId = user.Id,
                            CashierFirstName = user.FirstName,
                            CashierLastName = user.LastName,
                            CashDeskId = authBase.CashDeskId,
                            CashDeskName = cashDesk.Name,
                            PartnerId = user.PartnerId,
                            BetShopCurrencyId = betShop.CurrencyId,
                            BetShopId = betShop.Id,
                            BetShopAddress = betShop.Address,
                            BetShopName = betShop.Name,
                            Token = session.Token,
                            CurrentLimit = betShop.CurrentLimit,
                            PrintLogo = betShop.PrintLogo,
                            AnonymousBet = BetShopBll.GetBetShopAllowAnonymousBet(betShop.Id),
                            AllowCashout = BetShopBll.GetBetShopAllowCashout(betShop.Id),
                            AllowLive = BetShopBll.GetBetShopAllowLive(betShop.Id),
                            UsePin = BetShopBll.GetBetShopUsePin(betShop.Id),
                            Restrictions = cashDesk.Restrictions,
                            PaymentSystems = string.IsNullOrEmpty(betShop.PaymentSystems) ? new List<ApiPaymentSystem>() :
                                betShop.PaymentSystems.Split(',').Select(x => new ApiPaymentSystem
                                {
                                    Id = Convert.ToInt32(x)
                                }).ToList()
                        };
                        foreach (var ps in response.PaymentSystems)
                        {
                            ps.Name = CacheManager.GetPaymentSystemById(ps.Id).Name;
                        }
                        var balance = betShopBl.UpdateShifts(userBl, user.Id, cashDesk.Id);
                        response.Balance = balance.AvailableBalance;
                        return Ok(new ApiResponseBase
                        {
                            ResponseObject = response
                        });
                    }
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = Convert.ToInt32(ex.Detail.Id),
                    Description = ex.Detail.Message
                };
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(input) + "_" + JsonConvert.SerializeObject(response));
                return Ok(response);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                return Ok(new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                });
            }
        }

        [HttpPost]
        public IHttpActionResult Login(AuthorizationInput input)
        {
            try
            {
                using (var userBl = new UserBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    using (var betShopBl = new BetShopBll(userBl))
                    {
                        var authBase = Helpers.Helpers.CheckEncodedData(input.PartnerId, input.ExternalId, input.Hash, betShopBl);
                        var loginInput = new LoginInput
                        {
                            PartnerId = input.PartnerId,
                            Identifier = input.UserName,
                            Password = input.Password,
                            Ip = input.Ip,
                            LanguageId = input.LanguageId,
                            UserType = (int)UserTypes.Cashier,
                            CashDeskId = authBase.Id
                        };
                        var session = userBl.LoginUser(loginInput, out string imagaeData);
                        var user = userBl.GetUserById(session.Id);
                        if (user.Type != (int)UserTypes.Cashier)
                            throw BaseBll.CreateException(input.LanguageId, Constants.Errors.UserIsNotCashier);

                        var cashDesk = CacheManager.GetCashDeskById(authBase.Id);
                        if (cashDesk == null)
                            throw BaseBll.CreateException(input.LanguageId, Constants.Errors.CashDeskNotFound);

                        var betShop = betShopBl.GetBetShopById(cashDesk.BetShopId, true, user.Id);
                        if (betShop == null)
                            throw BaseBll.CreateException(input.LanguageId, Constants.Errors.BetShopNotFound);

                        var response = new AuthorizationOutput
                        {
                            CashierId = user.Id,
                            CashierFirstName = user.FirstName,
                            CashierLastName = user.LastName,
                            CashDeskId = authBase.Id,
                            CashDeskName = cashDesk.Name,
                            PartnerId = user.PartnerId,
                            BetShopCurrencyId = betShop.CurrencyId,
                            BetShopId = betShop.Id,
                            BetShopAddress = betShop.Address,
                            BetShopName = betShop.Name,
                            Token = session.Token,
                            CurrentLimit = betShop.CurrentLimit,
                            PrintLogo = betShop.PrintLogo,
                            AnonymousBet = BetShopBll.GetBetShopAllowAnonymousBet(betShop.Id),
                            AllowCashout = BetShopBll.GetBetShopAllowCashout(betShop.Id),
                            AllowLive = BetShopBll.GetBetShopAllowLive(betShop.Id),
                            UsePin = BetShopBll.GetBetShopUsePin(betShop.Id),
                            Restrictions = cashDesk.Restrictions,
                            PaymentSystems = string.IsNullOrEmpty(betShop.PaymentSystems) ? new List<ApiPaymentSystem>() :
                                betShop.PaymentSystems.Split(',').Select(x => new ApiPaymentSystem
                                {
                                    Id = Convert.ToInt32(x)
                                }).ToList()
                        };
                        foreach (var ps in response.PaymentSystems)
                        {
                            ps.Name = CacheManager.GetPaymentSystemById(ps.Id).Name;
                        }

                        var balance = betShopBl.UpdateShifts(userBl, user.Id, cashDesk.Id);
                        if (cashDesk.Type == (int)CashDeskTypes.Terminal)
                            response.Balance = balance.Balances.Where(x => x.TypeId == (int)AccountTypes.TerminalBalance).
                                Select(x => x.Balance).DefaultIfEmpty(0).Sum();
                        else
                            response.Balance = balance.AvailableBalance;

                        return Ok(new ApiResponseBase
                        {
                            ResponseObject= response
                        });
                    }
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = Convert.ToInt32(ex.Detail.Id),
                    Description = ex.Detail.Message
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                return Ok(new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                });
            }
        }

        [HttpPost]
        public IHttpActionResult GetCashierSessionByToken(ApiRequestBase input)
        {
            try
            {
                using (var userBl = new UserBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    var session = userBl.GetUserSession(input.Token, false);
                    return Ok(new ApiResponseBase
                    {
                        ResponseObject = session.ToApiCashierSession()
                    });
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = Convert.ToInt32(ex.Detail.Id),
                    Description = ex.Detail.Message
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                return Ok(new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                });
            }
        }

        [HttpPost]
        public IHttpActionResult GetCashierSessionByProductId(ApiRequestBase input)
        {
            try
            {
                var identity = CheckToken(input.Token, input.CashDeskId, true, false);
                using (var userBl = new UserBll(identity, WebApiApplication.DbLogger))
                {
                    if (identity.ParentId == null)
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.PartnerNotFound);
                    var session = userBl.GetUserSessionById(identity.ParentId.Value);
                    return Ok(new ApiResponseBase
                    {
                        ResponseObject = session.ToApiCashierSession()
                    });
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = Convert.ToInt32(ex.Detail.Id),
                    Description = ex.Detail.Message
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                return Ok(new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                });
            }
        }

        [HttpPost]
        public IHttpActionResult GetCashDeskInfo(ApiRequestBase apiRequestBase) // not using
        {
            try
            {
                var identity = CheckToken(apiRequestBase.Token, apiRequestBase.CashDeskId);
                using (var betShopBl = new BetShopBll(identity, WebApiApplication.DbLogger))
                {
                    var cashDesk = CacheManager.GetCashDeskById(identity.CashDeskId) ??
                        throw BaseBll.CreateException(apiRequestBase.LanguageId, Constants.Errors.CashDeskNotFound);
                    var betShop = betShopBl.GetBetShopById(cashDesk.BetShopId, false) ??
                        throw BaseBll.CreateException(apiRequestBase.LanguageId, Constants.Errors.BetShopNotFound);
                    var balance = betShopBl.GetObjectBalanceWithConvertion((int)ObjectTypes.CashDesk,
                        cashDesk.Id,
                        betShop.CurrencyId);
                    return Ok(new ApiResponseBase
                    {
                        ResponseObject = new GetCashDeskInfoOutput
                        {
                            CurrencyId = balance.CurrencyId,
                            Balance = balance.AvailableBalance,
                            CashDeskId = (int)balance.ObjectId,
                            CurrentLimit = betShop.CurrentLimit
                        }
                    });
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = Convert.ToInt32(ex.Detail.Id),
                    Description = ex.Detail.Message
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                return Ok(new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                });
            }
        }

        [HttpPost]
        public IHttpActionResult GetBetShopPlayersData(ApiRequestBase apiRequestBase)
        {
            try
            {
                var identity = CheckToken(apiRequestBase.Token, apiRequestBase.CashDeskId);
                using (var betShopBl = new BetShopBll(identity, WebApiApplication.DbLogger))
                using (var clientBll = new ClientBll(betShopBl))
                {
                    var cashDesk = CacheManager.GetCashDeskById(identity.CashDeskId) ??
                        throw BaseBll.CreateException(apiRequestBase.LanguageId, Constants.Errors.CashDeskNotFound);
                    var betShop = betShopBl.GetBetShopById(cashDesk.BetShopId, false) ??
                        throw BaseBll.CreateException(apiRequestBase.LanguageId, Constants.Errors.BetShopNotFound);

                    return Ok(new ApiResponseBase
                    {
                        ResponseObject = new
                        {
                            betShop.CurrencyId,
                            Balance = clientBll.GetShopWalletClients(new FilterShopWalletClient(), betShop.Id).Entities.Select(x => x.Balance).DefaultIfEmpty(0).Sum() ?? 0,
                            TotalOpenPayouts = 0, //????
                            CashDeskId = cashDesk.Id,
                        }
                    });
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = Convert.ToInt32(ex.Detail.Id),
                    Description = ex.Detail.Message
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                return Ok(new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                });
            }
        }

        [HttpPost]
        public IHttpActionResult GetProductSession(ApiRequestBase apiRequestBase)
        {
            try
            {
                var identity = CheckToken(apiRequestBase.Token, apiRequestBase.CashDeskId);
                var input = JsonConvert.DeserializeObject<ApiProductInput>(apiRequestBase.RequestObject);
                using (var userBl = new UserBll(identity, WebApiApplication.DbLogger))
                {
                    var product = CacheManager.GetProductById(input.ProductId ?? 0) ??
                        throw BaseBll.CreateException(apiRequestBase.LanguageId, Constants.Errors.ProductNotFound);
                    var productSession = userBl.CreateProductSession(identity, product.Id);
                    return Ok(new ApiResponseBase
                    {
                        ResponseObject = new GetProductSessionOutput
                        {
                            ProductId = input.ProductId ?? 0,
                            ProductToken = productSession.Token,
                            LaunchUrl = ProductHelpers.GetProductLaunchUrl(product.Id, productSession.Token, identity)
                        }
                    });
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = Convert.ToInt32(ex.Detail.Id),
                    Description = ex.Detail.Message
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                return Ok(new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                });
            }
        }

        [HttpPost]
        public IHttpActionResult LogoutUser(ApiRequestBase apiRequestBase)
        {
            try
            {
                var input = JsonConvert.DeserializeObject<ApiProductInput>(apiRequestBase.RequestObject);
                var identity = CheckToken(apiRequestBase.Token, apiRequestBase.CashDeskId);
                using (var userBl = new UserBll(identity, WebApiApplication.DbLogger))
                {
                    if (input.ProductId.HasValue && identity.ProductId != input.ProductId)
                        throw BaseBll.CreateException(apiRequestBase.LanguageId, Constants.Errors.WrongProductId);
                    userBl.LogoutUser(apiRequestBase.Token);
                    return Ok(new ApiResponseBase());
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = Convert.ToInt32(ex.Detail.Id),
                    Description = ex.Detail.Message
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                return Ok(new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                });
            }
        }

        [HttpPost]
        public IHttpActionResult RegisterClient(ApiRequestBase apiRequestBase)
        {
            try
            {
                var identity = CheckToken(apiRequestBase.Token, apiRequestBase.CashDeskId);
                var input = JsonConvert.DeserializeObject<ClientModel>(apiRequestBase.RequestObject);

                bool generatedUsername = false;
                if (string.IsNullOrEmpty(input.UserName?.Trim()))
                {
                    input.UserName = CommonFunctions.GetRandomString(10);
                    generatedUsername = true;
                }
                input.PartnerId = identity.PartnerId;
                input.CurrencyId = identity.CurrencyId;
                using (var clientBl = new ClientBll(identity, WebApiApplication.DbLogger))
                {
                    DAL.Client existingClient = null;
                    var partnerSetting = CacheManager.GetConfigKey(input.PartnerId, Constants.PartnerKeys.FirstLastBirthUnique);
                    if (partnerSetting == "1")
                    {
                        var clientBirthDate = new DateTime(input.BirthYear ?? DateTime.MinValue.Year, input.BirthMonth ?? DateTime.MinValue.Month,
                                              input.BirthDay ?? DateTime.MinValue.Day);
                        existingClient = clientBl.GetClientByName(input.PartnerId, input.FirstName, input.LastName, clientBirthDate);
                    }
                    if (existingClient == null)
                    {
                        var clientRegistrationInput = new ClientRegistrationInput
                        {
                            ClientData = input.MapToClient(),
                            RegistrationType = (int)Constants.RegisterTypes.Full,
                            IsFromAdmin = true,
                            GeneratedUsername = generatedUsername,
                            BetShopId = identity.BetShopId,
                            BetShopPaymentSystems = input.BetShopPaymentSystems
                        };
                        clientRegistrationInput.ClientData.RegistrationIp = apiRequestBase.Ip ?? Constants.DefaultIp;
                        var client = clientBl.RegisterClient(clientRegistrationInput);
                        var response = client.MapToApiLoginClientOutput(apiRequestBase.TimeZone);
                        var verificationPlatform = CacheManager.GetConfigKey(client.PartnerId, Constants.PartnerKeys.VerificationPlatform);
                        if (!string.IsNullOrEmpty(verificationPlatform) && int.TryParse(verificationPlatform, out int verificationPatformId))
                        {
                            switch (verificationPatformId)
                            {
                                case (int)VerificationPlatforms.Insic:
                                    var thread = new Thread(() => InsicHelpers.CreateClientOnAllPlatforms(client, true, identity, WebApiApplication.DbLogger));
                                    thread.Start();
                                    break;
                                default:
                                    break;
                            }
                        }
                        return Ok(new ApiResponseBase { ResponseObject = response });
                    }
                    else
                    {
                        clientBl.RegisterClientAccounts(existingClient.Id, existingClient.CurrencyId, identity.BetShopId, input.BetShopPaymentSystems);
                        var response = existingClient.MapToApiLoginClientOutput(apiRequestBase.TimeZone);
                        return Ok(new ApiResponseBase { ResponseObject = response });
                    }
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = Convert.ToInt32(ex.Detail.Id),
                    Description = ex.Detail.Message
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                return Ok(new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                });
            }
        }

        [HttpPost]
        public IHttpActionResult GetClientRegistrationFields(ApiRequestBase apiRequestBase)
        {
            try
            {
                var identity = CheckToken(apiRequestBase.Token, apiRequestBase.CashDeskId);
                using (var contentBl = new ContentBll(identity, WebApiApplication.DbLogger))
                {
                    return Ok(new ApiResponseBase
                    {
                        ResponseObject = contentBl.GetClientRegistrationFields(identity.PartnerId, (int)SystemModuleTypes.BetShop)
                    });
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = Convert.ToInt32(ex.Detail.Id),
                    Description = ex.Detail.Message
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                return Ok(new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                });
            }
        }


        [HttpPost]
        public IHttpActionResult GetClients(ApiRequestBase apiRequestBase)
        {
            try
            {
                var identity = CheckToken(apiRequestBase.Token, apiRequestBase.CashDeskId);
                var input = JsonConvert.DeserializeObject<ApiClientFilter>(apiRequestBase.RequestObject);
                var cashDesk = CacheManager.GetCashDeskById(apiRequestBase.CashDeskId) ??
                    throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.CashDeskNotFound);
                using (var clientBl = new ClientBll(identity, WebApiApplication.DbLogger))
                {
                    var clients = clientBl.GetShopWalletClients(new FilterShopWalletClient
                    {
                        Id = input.ClientId,
                        UserName = input.UserName,
                        DocumentNumber = input.DocumentNumber,
                        Email = input.Email,
                        FirstName = input.FirstName,
                        LastName = input.LastName,
                        Info = input.Info
                    }, cashDesk.BetShopId);
                    return Ok(new ApiResponseBase
                    {
                        ResponseObject = new PagedModel<object>
                        {
                            Count = clients.Count,
                            Entities = clients.Entities.ToList()
                        }
                    });
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = Convert.ToInt32(ex.Detail.Id),
                    Description = ex.Detail.Message
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                return Ok(new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                });
            }
        }

        [HttpPost]
        public IHttpActionResult GetClient(ApiRequestBase apiRequestBase)
        {
            try
            {
                var identity = CheckToken(apiRequestBase.Token, apiRequestBase.CashDeskId);
                var input = JsonConvert.DeserializeObject<ApiClientFilter>(apiRequestBase.RequestObject);
                if (!input.ClientId.HasValue)
                    throw BaseBll.CreateException(apiRequestBase.LanguageId, Constants.Errors.ClientNotFound);
                using (var clientBl = new ClientBll(identity, WebApiApplication.DbLogger))
                {
                    var client = clientBl.GetClientById(input.ClientId.Value) ??
                        throw BaseBll.CreateException(apiRequestBase.LanguageId, Constants.Errors.ClientNotFound);
                    var accounts = clientBl.GetClientAccounts(client.Id, true);
                    var response = client.ToGetClientOutput();
                    response.Settings = clientBl.GetClientSettings(client.Id, true);
                    response.Accounts = accounts.Select(x => x.MapToApiFnAccount()).ToList();
                    return Ok(new ApiResponseBase { ResponseObject = response });
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = Convert.ToInt32(ex.Detail.Id),
                    Description = ex.Detail.Message
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                return Ok(new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                });
            }
        }

        [HttpPost]
        public IHttpActionResult EditClient(ApiRequestBase apiRequestBase)
        {
            try
            {
                var identity = CheckToken(apiRequestBase.Token, apiRequestBase.CashDeskId);
                var input = JsonConvert.DeserializeObject<ClientModel>(apiRequestBase.RequestObject);
                using (var clientBl = new ClientBll(identity, WebApiApplication.DbLogger))
                {
                    clientBl.ChangeClientDataFromBetShop(input.MapToChangeClientFieldsInput());
                    return Ok(new ApiResponseBase());
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = Convert.ToInt32(ex.Detail.Id),
                    Description = ex.Detail.Message
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                return Ok(new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                });
            }
        }

        [HttpPost]
        public IHttpActionResult DepositToInternetClient(ApiRequestBase apiRequestBase)
        {
            try
            {
                var identity = CheckToken(apiRequestBase.Token, apiRequestBase.CashDeskId);
                var input = JsonConvert.DeserializeObject<DepositToInternetClientInput>(apiRequestBase.RequestObject);
                using (var clientBl = new ClientBll(identity, WebApiApplication.DbLogger))
                {
                    var cashDesk = CacheManager.GetCashDeskById(apiRequestBase.CashDeskId);
                    var betShop = CacheManager.GetBetShopById(cashDesk.BetShopId);
                    var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.BetShop);

                    var client = CacheManager.GetClientById(input.ClientId);
                    if (client == null || betShop.PartnerId != client.PartnerId)
                        throw BaseBll.CreateException(apiRequestBase.LanguageId, Constants.Errors.ClientNotFound);
                    var paymentRequest = new DAL.PaymentRequest
                    {
                        Amount = input.Amount,
                        ClientId = client.Id,
                        CurrencyId = input.CurrencyId,
                        CashDeskId = apiRequestBase.CashDeskId,
                        CashierId = input.CashierId,
                        ExternalTransactionId = input.TransactionId
                    };
                    var verificationPlatform = CacheManager.GetConfigKey(client.PartnerId, Constants.PartnerKeys.VerificationPlatform);
                    if (!string.IsNullOrEmpty(verificationPlatform) && int.TryParse(verificationPlatform, out int verificationPatformId))
                    {
                        switch (verificationPatformId)
                        {
                            case (int)VerificationPlatforms.Insic:
                                OASISHelpers.CheckClientStatus(client, betShop.Id, apiRequestBase.LanguageId, identity, WebApiApplication.DbLogger);
                                InsicHelpers.PaymentModalityRegistration(client.PartnerId, paymentRequest.ClientId.Value, paymentSystem.Id,
                                                                         identity, WebApiApplication.DbLogger);
                                InsicHelpers.PaymentRequest(client.PartnerId, paymentRequest.ClientId.Value, paymentRequest.Id, paymentRequest.Type,
                                                            paymentRequest.Amount, WebApiApplication.DbLogger);
                                break;
                            default:
                                break;
                        }
                    }
                    var paymentRequestDocument = clientBl.CreateDepositFromBetShop(paymentRequest);
                    if (paymentRequestDocument.Status == (int)PaymentRequestStates.Approved)
                    {
                        Helpers.Helpers.InvokeMessage("ClientDepositWithBonus", client.Id);
                        Helpers.Helpers.InvokeMessage("PaymentRequst", paymentRequestDocument.Id);
                    }
                    return Ok(new ApiResponseBase
                    {
                        ResponseObject = new DepositToInternetClientOutput
                        {
                            TransactionId = paymentRequestDocument.Id,
                            CurrentLimit = paymentRequestDocument.ObjectLimit,
                            CashierBalance = paymentRequestDocument.CashierBalance,
                            ClientBalance = paymentRequestDocument.ClientBalance,
                            ClientUserName = client.UserName,
                            ClientId = client.Id,
                            CurrencyId = client.CurrencyId,
                            DocumentNumber = client.DocumentNumber,
                            Amount = input.Amount,
                            Status = paymentRequestDocument.Status,
                            DepositDate = paymentRequestDocument.CreationTime
                        }
                    });
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                ApiResponseBase response;
                if (ex.Detail == null)
                {
                    response = new ApiResponseBase
                    {
                        ResponseCode = Constants.Errors.GeneralException,
                        Description = ex.Message
                    };
                }
                else
                {
                    response = new ApiResponseBase
                    {
                        ResponseCode = Convert.ToInt32(ex.Detail.Id),
                        Description = ex.Detail.Message
                    };
                }
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(response));
                return Ok(response);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                return Ok(new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                });
            }
        }

        [HttpPost]
        public IHttpActionResult GetPaymentRequests(ApiRequestBase apiRequestBase)
        {
            try
            {
                var identity = CheckToken(apiRequestBase.Token, apiRequestBase.CashDeskId);
                var input = JsonConvert.DeserializeObject<ApiClientInput>(apiRequestBase.RequestObject);
                using (var paymentSystemBll = new PaymentSystemBll(identity, WebApiApplication.DbLogger))
                {
                    var cashDesk = CacheManager.GetCashDeskById(apiRequestBase.CashDeskId);
                    if (cashDesk == null)
                        throw BaseBll.CreateException(apiRequestBase.LanguageId, Constants.Errors.CashDeskNotFound);

                    var filter = new FilterfnPaymentRequest
                    {
                        States = new FiltersOperation
                        {
                            IsAnd = true,
                            OperationTypeList = new List<FiltersOperationType>
                                {
                                    new FiltersOperationType
                                    {
                                        OperationTypeId = (int)FilterOperations.IsEqualTo,
                                        IntValue = (int) PaymentRequestStates.Confirmed
                                    }
                                }
                        },
                        Type = (int)PaymentRequestTypes.Withdraw,
                        BetShopIds = new FiltersOperation
                        {
                            IsAnd = true,
                            OperationTypeList = new List<FiltersOperationType>
                                {
                                    new FiltersOperationType
                                    {
                                        OperationTypeId = (int)FilterOperations.IsEqualTo,
                                        IntValue = cashDesk.BetShopId
                                    }
                                }
                        }
                    };
                    if (input.ClientId != 0)
                        filter.ClientIds = new FiltersOperation
                        {
                            IsAnd = true,
                            OperationTypeList = new List<FiltersOperationType>
                                {
                                    new FiltersOperationType
                                    {
                                        OperationTypeId = (int)FilterOperations.IsEqualTo,
                                        IntValue = input.ClientId
                                    }
                                }
                        };

                    var paymentRequests = paymentSystemBll.GetPaymentRequests(filter, false)/*.Where(x => x.ClientDocumentNumber == input.DocumentNumber)*/.ToList();

                    return Ok(new ApiResponseBase
                    {
                        ResponseObject =  new GetPaymentRequestsOutput
                        {
                            PaymentRequests = paymentRequests.Select(x =>
                            new PaymentRequest
                            {
                                Id = x.Id,
                                Amount = Math.Floor((x.Amount - x.CommissionAmount ?? 0) * 100) / 100,
                                ClientId = x.ClientId,
                                ClientFirstName = x.FirstName,
                                ClientLastName = x.LastName,
                                ClientEmail = x.Email,
                                UserName = x.UserName,
                                DocumentNumber = x.ClientDocumentNumber,
                                CreationTime = x.CreationTime,
                                CurrencyId = x.CurrencyId,
                                Info = x.Info
                            }).ToList()
                        }
                    });
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = Convert.ToInt32(ex.Detail.Id),
                    Description = ex.Detail.Message
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                return Ok(new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                });
            }
        }

        [HttpPost]
        public IHttpActionResult PayPaymentRequest(ApiRequestBase apiRequestBase)
        {
            try
            {
                var identity = CheckToken(apiRequestBase.Token, apiRequestBase.CashDeskId);
                var input = JsonConvert.DeserializeObject<PayPaymentRequestInput>(apiRequestBase.RequestObject);
                using (var clientBl = new ClientBll(identity, WebApiApplication.DbLogger))
                {
                    using (var documentBl = new DocumentBll(clientBl))
                    {
                        using (var notificationBl = new NotificationBll(documentBl))
                        {
                            clientBl.ChangeWithdrawRequestState(input.PaymentRequestId, PaymentRequestStates.PayPanding,
                            input.Comment, apiRequestBase.CashDeskId, input.CashierId, true, string.Empty, documentBl, notificationBl);
                            var resp = clientBl.ChangeWithdrawRequestState(input.PaymentRequestId, PaymentRequestStates.Approved,
                            input.Comment, apiRequestBase.CashDeskId, input.CashierId, true, string.Empty, documentBl, notificationBl, false, true);

                            clientBl.PayWithdrawFromBetShop(resp, apiRequestBase.CashDeskId, input.CashierId, documentBl);
                            Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.ClientBalance, resp.ClientId));
                            Helpers.Helpers.InvokeMessage("PaymentRequst", input.PaymentRequestId);
                            return Ok(new ApiResponseBase
                            {
                                ResponseObject = new PayPaymentRequestOutput
                                {
                                    CurrentLimit = resp.ObjectLimit,
                                    CashierBalance = resp.CashierBalance,
                                    ClientBalance = resp.ClientBalance,
                                    CurrencyId = resp.CurrencyId,
                                    TransactionId = resp.RequestId,
                                    ClientId = resp.ClientId,
                                    UserName = resp.ClientUserName,
                                    DocumentNumber = resp.ClientDocumentNumber,
                                    Amount = resp.RequestAmount,
                                    PayDate = clientBl.GetServerDate()
                                }
                            });
                        }
                    }
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = Convert.ToInt32(ex.Detail.Id),
                    Description = ex.Detail.Message
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                return Ok(new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                });
            }
        }

        [HttpPost]
        public IHttpActionResult GetBetByBarcode(ApiRequestBase apiRequestBase)
        {
            try
            {
                var identity = CheckToken(apiRequestBase.Token, apiRequestBase.CashDeskId);
                var input = JsonConvert.DeserializeObject<ReportByBetInput>(apiRequestBase.RequestObject);
                using (var reportBl = new ReportBll(identity, WebApiApplication.DbLogger))
                {
                    var bet = reportBl.GetBetByBarcode(apiRequestBase.CashDeskId, input.Barcode.Value);
                    return Ok(new ApiResponseBase
                    {
                        ResponseObject =  new GetBetShopBetsOutput
                        {
                            Bets = bet == null ? new List<BetShopBet>() : new List<BetShopBet> { bet.ToBetShopBet() }
                        }
                    });
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = Convert.ToInt32(ex.Detail.Id),
                    Description = ex.Detail.Message
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                return Ok(new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                });
            }
        }

        [HttpPost]
        public IHttpActionResult GetBetByDocumentId(GetBetByDocumentIdInput input)
        {
            try
            {
                var identity = CheckToken(input.Token, input.CashDeskId);
                using (var betShopBl = new BetShopBll(identity, WebApiApplication.DbLogger))
                {
                    var bet = betShopBl.GetBetShopBetByDocumentId(input.DocumentId, input.IsForPrint);
                    return Ok(new ApiResponseBase { ResponseObject = bet?.ToApiBetShopTicket() ?? new ApiBetShopTicket() });
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = Convert.ToInt32(ex.Detail.Id),
                    Description = ex.Detail.Message
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                return Ok(new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                });
            }
        }

        [HttpPost]
        public IHttpActionResult PayWin(ApiRequestBase apiRequestBase)
        {
            try
            {
                var identity = CheckToken(apiRequestBase.Token, apiRequestBase.CashDeskId);
                var input = JsonConvert.DeserializeObject<PayWinInput>(apiRequestBase.RequestObject);
                using (var betShopBl = new BetShopBll(identity, WebApiApplication.DbLogger))
                {
                    using (var documentBl = new DocumentBll(betShopBl))
                    {
                        var clientOperation = new ClientOperation
                        {
                            ClientId = input.CashierId,
                            CashDeskId = apiRequestBase.CashDeskId,
                            ParentDocumentId = input.BetDocumentId,
                            ExternalTransactionId = input.ExternalTransactionId
                        };
                        var document = betShopBl.PayWinFromBetShop(clientOperation, documentBl);
                        var cashDesk = CacheManager.GetCashDeskById(apiRequestBase.CashDeskId);
                        var betShop = betShopBl.GetBetShopById(cashDesk.BetShopId, false);
                        return Ok(new ApiResponseBase
                        {
                            ResponseObject = new FinOperationResponse
                            {
                                CurrentLimit = betShop.CurrentLimit,
                                CashierBalance = betShopBl.GetObjectBalanceWithConvertion((int)ObjectTypes.CashDesk,
                                    apiRequestBase.CashDeskId, document.CurrencyId).AvailableBalance,
                                CurrencyId = document.CurrencyId
                            }
                        });
                    }
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = Convert.ToInt32(ex.Detail.Id),
                    Description = ex.Detail.Message
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                return Ok(new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                });
            }
        }

        [HttpPost]
        public IHttpActionResult CreateDebitCorrectionOnCashDesk(Models.CashDeskCorrectionInput input)
        {
            try
            {
                using (var betShopBl = new BetShopBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    var correction = new DAL.Models.CashDeskCorrectionInput
                    {
                        Amount = input.Amount,
                        CurrencyId = input.CurrencyId,
                        CashDeskId = input.CashDeskId,
                        Info = input.Info,
                        CashierId = input.CashierId,
                        ExternalTransactionId = input.ExternalTransactionId
                    };
                    betShopBl.CreateDebitCorrectionOnCashDesk(correction);
                    return Ok(new ApiResponseBase());
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = Convert.ToInt32(ex.Detail.Id),
                    Description = ex.Detail.Message
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                return Ok(new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                });
            }
        }

        [HttpPost]
        public IHttpActionResult CreateCreditCorrectionOnCashDesk(Models.CashDeskCorrectionInput input)
        {
            try
            {
                using (var betShopBl = new BetShopBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    var correction = new DAL.Models.CashDeskCorrectionInput
                    {
                        Amount = input.Amount,
                        CurrencyId = input.CurrencyId,
                        CashDeskId = input.CashDeskId,
                        Info = input.Info,
                        CashierId = input.CashierId,
                        ExternalTransactionId = input.ExternalTransactionId
                    };
                    betShopBl.CreateCreditCorrectionOnCashDesk(correction);
                    return Ok(new ApiResponseBase());
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = Convert.ToInt32(ex.Detail.Id),
                    Description = ex.Detail.Message
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                return Ok(new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                });
            }
        }

        [HttpPost]
        public IHttpActionResult GetCashDesksBalancesByDate(GetCashiersBalanceIntput input) // not using
        {
            try
            {
                using (var partnerBl = new BetShopBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    var response = new GetCashDesksBalanceOutput
                    {
                        CashDeskBalances = new List<CashDeskBalanceOutput>()
                    };
                    foreach (var cashier in input.CashDesks)
                    {
                        var cashierBalance = partnerBl.GetAccountsBalances((int)ObjectTypes.CashDesk,
                            cashier.CashDeskId, input.BalanceDate);
                        response.CashDeskBalances.Add(new CashDeskBalanceOutput
                        {
                            CashDeskId = cashier.CashDeskId,
                            Balance = cashierBalance.Sum(x => x.Balance)
                        });
                    }
                    return Ok(new ApiResponseBase { ResponseObject = response });
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = Convert.ToInt32(ex.Detail.Id),
                    Description = ex.Detail.Message
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                return Ok(new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                });
            }
        }

        [HttpPost]
        public IHttpActionResult GetCashDesks(ApiRequestBase apiRequestBase)
        {
            try
            {
                var identity = CheckToken(apiRequestBase.Token, apiRequestBase.CashDeskId);
                var input = JsonConvert.DeserializeObject<ApiFilterCashDesk>(apiRequestBase.RequestObject);
                using (var betShopBl = new BetShopBll(identity, WebApiApplication.DbLogger))
                {
                    var cashDesk = CacheManager.GetCashDeskById(apiRequestBase.CashDeskId) ??
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.CashDeskNotFound);
                    input.BetShopId = identity.BetShopId;
                    var cashDesks = betShopBl.GetCashDesksPagedModel(input.MapToFilterfnCashDesk(), false);
                    var grouped = cashDesks.Entities.GroupBy(x => new { x.Id, x.BetShopId, x.CreationTime, x.LastUpdateTime, x.Name, x.Type, x.State });
                    return Ok(new ApiResponseBase
                    {
                        ResponseObject = new PagedModel<CashDesk>
                        {
                            Count = grouped.Count(),
                            Entities = grouped.Select(x => new CashDesk
                            {
                                Id = x.Key.Id,
                                BetShopId = x.Key.BetShopId,
                                CreationTime = x.Key.CreationTime,
                                LastUpdateTime = x.Key.LastUpdateTime,
                                Name = x.Key.Name,
                                Balance = x.Where(y => y.AccountTypeId == (int)AccountTypes.CashDeskBalance).Select(y => y.Balance).DefaultIfEmpty(0).Sum(),
                                TerminalBalance = x.Where(y => y.AccountTypeId == (int)AccountTypes.TerminalBalance).Select(y => y.Balance).DefaultIfEmpty(0).Sum(),
                                Type = x.Key.Type,
                                State = x.Key.State,
                            }).ToList()
                        }
                    });
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = Convert.ToInt32(ex.Detail.Id),
                    Description = ex.Detail.Message
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                return Ok(new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                });
            }
        }

        [HttpPost]
        public IHttpActionResult GetBalance(ApiRequestBase apiRequestBase)
        {
            try
            {
                var identity = CheckToken(apiRequestBase.Token, apiRequestBase.CashDeskId);
                using (var betShopBl = new BetShopBll(identity, WebApiApplication.DbLogger))
                {
                    var cashDesk = CacheManager.GetCashDeskById(apiRequestBase.CashDeskId) ??
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.CashDeskNotFound);
                    var betShop = betShopBl.GetBetShopById(cashDesk.BetShopId, false) ??
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.BetShopNotFound);

                    var cashierBalance = betShopBl.GetObjectBalanceWithConvertion((int)ObjectTypes.CashDesk, apiRequestBase.CashDeskId, betShop.CurrencyId);
                    return Ok(new ApiResponseBase
                    {
                        ResponseObject = new GetCashDeskCurrentBalanceOutput
                        {
                            Balance = cashierBalance.AvailableBalance,
                            TerminalBalance = cashierBalance.Balances.Where(x => x.TypeId == (int)AccountTypes.TerminalBalance).Sum(x => x.Balance),
                            CashDeskBalance = cashierBalance.Balances.Where(x => x.TypeId == (int)AccountTypes.CashDeskBalance).Sum(x => x.Balance),
                            CurrentLimit = betShop.CurrentLimit
                        }
                    });
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = Convert.ToInt32(ex.Detail.Id),
                    Description = ex.Detail.Message
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                return Ok(new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                });
            }
        }

        [HttpPost]
        public IHttpActionResult CloseShift(ApiRequestBase input)
        {
            try
            {
                var identity = CheckToken(input.Token, input.CashDeskId, false);
                using (var betshopBl = new BetShopBll(identity, WebApiApplication.DbLogger))
                {
                    using (var userBl = new UserBll(betshopBl))
                    {
                        using (var documentBl = new DocumentBll(userBl))
                        {
                            using (var reportBl = new ReportBll(documentBl))
                            {
                                WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(input));
                                var session = userBl.GetUserSession(input.Token, false);
                                session.State = (int)SessionStates.Inactive;
                                var result = betshopBl.CloseShift(documentBl, reportBl, input.CashDeskId, session.UserId ?? 0,
                                    session.Id).MapToApiCloseShiftOutput(input.TimeZone);
                                WebApiApplication.DbLogger.Info(JsonConvert.SerializeObject(result));

                                return Ok(new ApiResponseBase { ResponseObject = result });
                            }
                        }
                    }
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = Convert.ToInt32(ex.Detail.Id),
                    Description = ex.Detail.Message
                };
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(response));
                return Ok(response);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                return Ok(new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                });
            }
        }

        [HttpPost]
        public IHttpActionResult ChangeCashierPassword(ApiRequestBase apiRequestBase)
        {
            try
            {
                var identity = CheckToken(apiRequestBase.Token, apiRequestBase.CashDeskId, false);
                var input = JsonConvert.DeserializeObject<ChangePasswordInput>(apiRequestBase.RequestObject);
                using (var userBl = new UserBll(identity, WebApiApplication.DbLogger))
                {
                    userBl.ChangeUserPassword(identity.Id, input.OldPassword, input.NewPassword);
                    return Ok(new ApiResponseBase());
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = Convert.ToInt32(ex.Detail.Id),
                    Description = ex.Detail.Message
                };
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(response));
                return Ok(response);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                return Ok(new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                });
            }
        }

        [HttpPost]
        public IHttpActionResult AssignPin(ApiRequestBase apiRequestBase)
        {
            try
            {
                var identity = CheckToken(apiRequestBase.Token, apiRequestBase.CashDeskId, false);
                var input = JsonConvert.DeserializeObject<ApiClientInput>(apiRequestBase.RequestObject);
                using (var userBl = new UserBll(identity, WebApiApplication.DbLogger))
                {
                    var pin = userBl.AssignPin(identity.BetShopId ?? 0, identity.Id, input.ClientId);
                    return Ok(new ApiResponseBase { ResponseObject = pin });
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = Convert.ToInt32(ex.Detail.Id),
                    Description = ex.Detail.Message
                };
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(response));
                return Ok(response);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                return Ok(new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                });
            }
        }

        [HttpPost]
        public IHttpActionResult DepositToTerminal(ApiRequestBase apiRequestBase)
        {
            try
            {
                var identity = CheckToken(apiRequestBase.Token, apiRequestBase.CashDeskId);
                var input = JsonConvert.DeserializeObject<DepositToInternetClientInput>(apiRequestBase.RequestObject);
                using (var betShopBl = new BetShopBll(identity, WebApiApplication.DbLogger))
                {
                    var cashDesk = CacheManager.GetCashDeskById(apiRequestBase.CashDeskId);
                    if (cashDesk.Type != (int)CashDeskTypes.Terminal)
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.CashDeskNotFound);
                    var betShop = CacheManager.GetBetShopById(cashDesk.BetShopId);
                    var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.BetShop);
                    var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(betShop.PartnerId, paymentSystem.Id, betShop.CurrencyId, (int)PaymentRequestTypes.Deposit) ??
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.PartnerPaymentSettingNotFound);
                    var currentTime = DateTime.UtcNow;

                    var paymentRequest = new DAL.PaymentRequest
                    {
                        Amount = input.Amount,
                        CurrencyId = betShop.CurrencyId,
                        CashDeskId = apiRequestBase.CashDeskId,
                        BetShopId = betShop.Id,
                        CashierId = identity.Id,
                        ExternalTransactionId = input.TransactionId,
                        PaymentSystemId = paymentSystem.Id,
                        PartnerPaymentSettingId = partnerPaymentSetting.Id,
                        Status = (int)PaymentRequestStates.Approved,
                        Type = (int)PaymentRequestTypes.Deposit,
                        LastUpdateTime = currentTime,
                        CreationTime = currentTime,
                        SessionId = identity.SessionId,
                        Date = (long)currentTime.Year * 100000000 + (long)currentTime.Month * 1000000 + (long)currentTime.Day * 10000 + (long)currentTime.Hour * 100 + currentTime.Minute
                    };
                    var paymentRequestDocument = betShopBl.CreateDepositToTerminal(paymentRequest);

                    return Ok(new ApiResponseBase
                    {
                        ResponseObject = new DepositToInternetClientOutput
                        {
                            TransactionId = paymentRequestDocument.Id,
                            CurrencyId = input.CurrencyId,
                            Amount = input.Amount,
                            Status = paymentRequestDocument.State,
                            DepositDate = paymentRequestDocument.CreationTime
                        }
                    });
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = Convert.ToInt32(ex.Detail.Id),
                    Description = ex.Detail.Message
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                return Ok(new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                });
            }
        }

        [HttpPost]
        public IHttpActionResult WithdrawTerminalFunds(ApiRequestBase apiRequestBase)
        {
            try
            {
                WebApiApplication.DbLogger.Info("WithdrawTerminalFunds_" + JsonConvert.SerializeObject(apiRequestBase));
                var identity = CheckToken(apiRequestBase.Token, apiRequestBase.CashDeskId);
                using (var betShopBl = new BetShopBll(identity, WebApiApplication.DbLogger))
                {
                    using (var documentBl = new DocumentBll(betShopBl))
                    {
                        var cashDesk = CacheManager.GetCashDeskById(apiRequestBase.CashDeskId);
                        if (cashDesk.Type != (int)CashDeskTypes.Terminal)
                            throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.CashDeskNotFound);
                        var betShop = CacheManager.GetBetShopById(cashDesk.BetShopId);
                        var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.BetShop);
                        var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(betShop.PartnerId, paymentSystem.Id,
                            betShop.CurrencyId, (int)PaymentRequestTypes.Deposit);
                        var currentTime = DateTime.UtcNow;

                        var paymentRequest = new DAL.PaymentRequest
                        {
                            CurrencyId = betShop.CurrencyId,
                            CashDeskId = apiRequestBase.CashDeskId,
                            BetShopId = betShop.Id,
                            CashierId = identity.Id,
                            PaymentSystemId = paymentSystem.Id,
                            PartnerPaymentSettingId = partnerPaymentSetting.Id,
                            Status = (int)PaymentRequestStates.Confirmed,
                            Type = (int)PaymentRequestTypes.Withdraw,
                            LastUpdateTime = currentTime,
                            CreationTime = currentTime,
                            SessionId = identity.SessionId,
                            Date = (long)currentTime.Year * 100000000 + (long)currentTime.Month * 1000000 +
                                (long)currentTime.Day * 10000 + (long)currentTime.Hour * 100 + currentTime.Minute
                        };
                        var document = betShopBl.CreateWithdrawFromTerminal(paymentRequest);

                        return Ok(new ApiResponseBase
                        {
                            ResponseObject = new DepositToInternetClientOutput
                            {
                                TransactionId = paymentRequest.Id,
                                CurrencyId = paymentRequest.CurrencyId,
                                Amount = paymentRequest.Amount,
                                Status = paymentRequest.Status
                            }
                        });
                    }
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = Convert.ToInt32(ex.Detail.Id),
                    Description = ex.Detail.Message
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                return Ok(new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                });
            }
        }

        [HttpPost]
        public IHttpActionResult CreateWithdrawPaymentRequest(ApiRequestBase apiRequestBase)
        {
            try
            {
                var identity = CheckToken(apiRequestBase.Token, apiRequestBase.CashDeskId);
                var input = JsonConvert.DeserializeObject<CreatePaymentRequest>(apiRequestBase.RequestObject);
                using (var clientBll = new ClientBll(identity, WebApiApplication.DbLogger))
                {
                    using (var documentBll = new DocumentBll(clientBll))
                    {
                        using (var notificationBll = new NotificationBll(clientBll))
                        {
                            var cashDesk = CacheManager.GetCashDeskById(apiRequestBase.CashDeskId);
                            var betShop = CacheManager.GetBetShopById(cashDesk.BetShopId);
                            var client = CacheManager.GetClientById(input.ClientId);
                            var paymentSystem = CacheManager.GetPaymentSystemByName(Constants.PaymentSystems.BetShop);
                            var partnePaymentSetting = CacheManager.GetPartnerPaymentSettings(betShop.PartnerId, paymentSystem.Id, betShop.CurrencyId, (int)PaymentRequestTypes.Deposit);
                            var partner = CacheManager.GetPartnerById(betShop.PartnerId);
                            var currentTime = DateTime.UtcNow;

                            var paymentRequest = new PaymentRequestModel
                            {
                                ClientId = input.ClientId,
                                Amount = input.Amount,
                                CurrencyId = betShop.CurrencyId,
                                CashDeskId = apiRequestBase.CashDeskId,
                                CashierId = input.CashierId,
                                BetShopId = betShop.Id,
                                PaymentSystemId = paymentSystem.Id,
                                Type = (int)PaymentRequestTypes.Withdraw,
                                CashCode = "BetShopWithdraw",
                                PartnerId = betShop.PartnerId,
                                LastUpdateTime = currentTime,
                                CreationTime = currentTime,
                                Info = "{}"
                            };
                            var document = clientBll.CreateWithdrawPaymentRequest(paymentRequest, 0, client, documentBll, notificationBll);
                            var autoConfirmWithdrawMaxAmount = BaseBll.ConvertCurrency(partner.CurrencyId, betShop.CurrencyId, partner.AutoConfirmWithdrawMaxAmount);
                            var resp = new ChangeWithdrawRequestStateOutput();
                            if (autoConfirmWithdrawMaxAmount > input.Amount)
                                resp = clientBll.ChangeWithdrawRequestState(document.Id, PaymentRequestStates.Confirmed, "", cashDesk.Id, null, true, string.Empty, documentBll, notificationBll);
                            //resp = clientBll.ChangeWithdrawRequestState(document.Id, PaymentRequestStates.PayPanding, "", cashDesk.Id, null, true, string.Empty, documentBll, notificationBll);
                            //resp = clientBll.ChangeWithdrawRequestState(document.Id, PaymentRequestStates.Approved, "", cashDesk.Id, null, true, string.Empty, documentBll, notificationBll, false, true);
                            //clientBll.PayWithdrawFromPaymentSystem(resp, documentBll, notificationBll);
                            Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.ClientBalance, resp.ClientId));
                            Helpers.Helpers.InvokeMessage("PaymentRequst", document.Id);
                            return Ok(new ApiResponseBase
                            {
                                ResponseObject = new PayPaymentRequestOutput
                                {
                                    CurrentLimit = resp.ObjectLimit,
                                    CashierBalance = resp.CashierBalance,
                                    ClientBalance = resp.ClientBalance,
                                    CurrencyId = resp.CurrencyId,
                                    TransactionId = resp.RequestId,
                                    ClientId = resp.ClientId,
                                    UserName = resp.ClientUserName,
                                    DocumentNumber = resp.ClientDocumentNumber,
                                    Amount = resp.RequestAmount,
                                    PayDate = clientBll.GetServerDate()
                                }
                            });
                        }
                    }
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = Convert.ToInt32(ex.Detail.Id),
                    Description = ex.Detail.Message
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                return Ok(new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                });
            }
        }

        [HttpPost]
        public IHttpActionResult GetErrorById(GetErrorInput input)
        {
            try
            {
                var message = CacheManager.GetfnErrorTypes(input.LanguageId).Where(x => x.Id == input.ErrorId).FirstOrDefault()?.Message;
                return Ok(new ApiResponseBase
                {
                    Description = message
                });
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = Convert.ToInt32(ex.Detail.Id),
                    Description = ex.Detail.Message
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                return Ok(new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                });
            }
        }

        #region Reporting

        [HttpPost]
        public IHttpActionResult GetBetShopBets(ApiRequestBase apiRequestBase)
        {
            try
            {
                var identity = CheckToken(apiRequestBase.Token, apiRequestBase.CashDeskId);
                var input = JsonConvert.DeserializeObject<ReportByBetInput>(apiRequestBase.RequestObject);
                var fromDate = input.FromDate.GetGMTDateFromUTC(apiRequestBase.TimeZone);
                var toDate = input.ToDate.GetGMTDateFromUTC(apiRequestBase.TimeZone);
                if (fromDate < DateTime.UtcNow.AddDays(-7))
                    throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongParameters);

                using (var reportBl = new ReportBll(identity, WebApiApplication.DbLogger))
                {
                    var bets =
                        reportBl.GetBetshopBetsForCashier(fromDate.Value, toDate.Value, apiRequestBase.CashDeskId, identity.Id,
                            input.ProductId, input.State);
                    return Ok(new ApiResponseBase
                    {
                        ResponseObject = new GetBetShopBetsOutput
                        {
                            Bets = bets.Select(x => x.MapToBetShopBet(apiRequestBase.TimeZone)).ToList()
                        }
                    });
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = Convert.ToInt32(ex.Detail.Id),
                    Description = ex.Detail.Message
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                return Ok(new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                });
            }
        }

        [HttpPost]
        public IHttpActionResult GetBetShopOperations(ApiRequestBase apiRequestBase)
        {
            try
            {
                var identity = CheckToken(apiRequestBase.Token, apiRequestBase.CashDeskId);
                var input = JsonConvert.DeserializeObject<ReportByBetInput>(apiRequestBase.RequestObject);
                using (var repaymentSystemBl = new PaymentSystemBll(identity, WebApiApplication.DbLogger))
                {
                    var fromDate = input.FromDate.Value.GetUTCDateFromGmt(apiRequestBase.TimeZone);
                    var toDate = input.ToDate.Value.GetUTCDateFromGmt(apiRequestBase.TimeZone);
                    if ((toDate - fromDate).TotalDays > 30)
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongParameters);
                    var cashDesk = CacheManager.GetCashDeskById(apiRequestBase.CashDeskId);
                    var filter = new FilterfnPaymentRequest
                    {
                        FromDate = (long)fromDate.Year * 100000000 + (long)fromDate.Month * 1000000 + (long)fromDate.Day * 10000 + (long)fromDate.Hour * 100 + fromDate.Minute,
                        ToDate = (long)toDate.Year * 100000000 + (long)toDate.Month * 1000000 + (long)toDate.Day * 10000 + (long)toDate.Hour * 100 + toDate.Minute,
                        BetShopIds =
                            new FiltersOperation
                            {
                                OperationTypeList =
                                    new List<FiltersOperationType>
                                    {
                                        new FiltersOperationType
                                        {
                                            OperationTypeId = (int)FilterOperations.IsEqualTo,
                                            IntValue = cashDesk.BetShopId
                                        }
                                    }
                            },
                        States =
                            new FiltersOperation
                            {
                                IsAnd = false,
                                OperationTypeList =
                                    new List<FiltersOperationType>
                                    {
                                        new FiltersOperationType
                                        {
                                            OperationTypeId = (int)FilterOperations.IsEqualTo,
                                            IntValue = (int) PaymentRequestStates.Confirmed
                                        },
                                        new FiltersOperationType
                                        {
                                            OperationTypeId = (int)FilterOperations.IsEqualTo,
                                            IntValue = (int) PaymentRequestStates.Approved
                                        }
                                    }
                            },
                    };
                    if (input.Barcode != null)
                    {
                        long id = (input.Barcode.Value / 10) % 100000000000;
                        filter.Ids = new FiltersOperation
                        {
                            OperationTypeList =
                                    new List<FiltersOperationType>
                                    {
                                        new FiltersOperationType
                                        {
                                            OperationTypeId = (int)FilterOperations.IsEqualTo,
                                            IntValue = id
                                        }
                                    }
                        };
                    }

                    var operations =
                        repaymentSystemBl.GetPaymentRequestsPaging(filter, false, true)
                            .Entities.OrderByDescending(x => x.Id)
                            .ToList();
                    return Ok(new ApiResponseBase { ResponseObject = operations.MapToBetShopOperations(apiRequestBase.TimeZone) });
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = ex.Detail.Id,
                    Description = ex.Detail.Message
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                return Ok(new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                });
            }
        }

        [HttpPost]
        public IHttpActionResult GetShiftReport(ApiRequestBase apiRequestBase)
        {
            try
            {
                var identity = CheckToken(apiRequestBase.Token, apiRequestBase.CashDeskId);
                var input = JsonConvert.DeserializeObject<ApiCashierInput>(apiRequestBase.RequestObject);
                using (var reportBl = new ReportBll(identity, WebApiApplication.DbLogger))
                {
                    var startTime = input.FromDate.GetUTCDateFromGmt(apiRequestBase.TimeZone);
                    var endTime = input.ToDate.GetUTCDateFromGmt(apiRequestBase.TimeZone);
                    if (startTime < DateTime.UtcNow.AddDays(-8))
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongParameters);

                    var response = reportBl.GetShifts(startTime.Value, endTime.Value, apiRequestBase.CashDeskId, input.CashierId);

                    return Ok(new ApiResponseBase
                    {
                        ResponseObject = new ApiGetShiftReportOutput
                        {
                            Shifts = response.Select(x => x.ToApiShift(apiRequestBase.TimeZone))
                                             .OrderByDescending(x => x.Id)
                                             .ToList()
                        }
                    });
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = Convert.ToInt32(ex.Detail.Id),
                    Description = ex.Detail.Message
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                return Ok(new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                });
            }
        }

        [HttpPost]
        public IHttpActionResult GetCashDeskOperations(ApiRequestBase apiRequestBase)
        {
            try
            {
                var identity = CheckToken(apiRequestBase.Token, apiRequestBase.CashDeskId);
                var input = JsonConvert.DeserializeObject<CashDeskOperations>(apiRequestBase.RequestObject);
                using (var reportBl = new ReportBll(identity, WebApiApplication.DbLogger))
                {
                    using (var betshopBl = new BetShopBll(reportBl))
                    {
                        var currentTime = DateTime.UtcNow;
                        var fromDate = (input.FromDate == null || input.FromDate == DateTime.MinValue) ? currentTime.AddDays(-1) :
                            input.FromDate.GetUTCDateFromGmt(apiRequestBase.TimeZone);

                        var toDate = (input.ToDate == null || input.ToDate == DateTime.MinValue) ? currentTime.AddDays(1) :
                            input.ToDate.GetUTCDateFromGmt(apiRequestBase.TimeZone);
                        if (input.LastShiftsNumber != null)
                        {
                            if (input.LastShiftsNumber < 1 || input.LastShiftsNumber > 3)
                                throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongParameters);
                            var cd = betshopBl.GetCashDeskById(identity.CashDeskId);
                            if (cd != null)
                            {
                                var shift = betshopBl.GetShift(identity.CashDeskId, input.LastShiftsNumber == 1 ? (int?)null :
                                    Math.Max(0, (cd.CurrentShiftNumber ?? 0) - input.LastShiftsNumber.Value + 2));

                                if (shift != null)
                                    fromDate = shift.StartTime;
                            }
                        }
                        else if (fromDate < currentTime.AddDays(-4))
                            throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongParameters);

                        var filter = new FilterCashDeskTransaction
                        {
                            CashierIds = new FiltersOperation
                            {
                                IsAnd = true,
                                OperationTypeList = new List<FiltersOperationType>
                                {
                                    new FiltersOperationType
                                    {
                                        OperationTypeId = (int)FilterOperations.IsEqualTo,
                                        IntValue = input.CashierId
                                    }
                                }
                            },
                            CashDeskIds = new FiltersOperation
                            {
                                IsAnd = true,
                                OperationTypeList = new List<FiltersOperationType>
                                {
                                    new FiltersOperationType
                                    {
                                        OperationTypeId = (int)FilterOperations.IsEqualTo,
                                        IntValue = apiRequestBase.CashDeskId
                                    }
                                }
                            },
                            States = new FiltersOperation
                            {
                                IsAnd = true,
                                OperationTypeList = new List<FiltersOperationType>
                                {
                                    new FiltersOperationType
                                    {
                                        OperationTypeId = (int)FilterOperations.IsNotEqualTo,
                                        IntValue = (int)BetDocumentStates.Deleted
                                    }
                                }
                            },
                            FromDate = fromDate.Value,
                            ToDate = toDate.Value
                        };
                        var transactions = reportBl.GetCashDeskTransactions(filter, apiRequestBase.LanguageId);
                        return Ok(new ApiResponseBase
                        {
                            ResponseObject = new CashDeskOperationsOutput
                            {
                                Operations = transactions.Select(x => x.MapToCashDeskOperation(apiRequestBase.TimeZone)).ToList(),
                                StartTime = fromDate.Value.GetGMTDateFromUTC(apiRequestBase.TimeZone),
                                EndTime = toDate.Value.GetGMTDateFromUTC(apiRequestBase.TimeZone)
                            }
                        });
                    }
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = ex.Detail.Id,
                    Description = ex.Detail.Message,
                    ResponseObject = new
                    {
                        Operations = new List<CashDeskOperation>()
                    }
                };
                WebApiApplication.DbLogger.Error(JsonConvert.SerializeObject(response));
                return Ok(response);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                return Ok(new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                });
            }
        }

        #endregion

        [HttpPost]
        public IHttpActionResult GetUserProductSessions(GetCashDeskInfoInput input) // not using
        {
            try
            {
                var identity = CheckToken(input.Token, input.CashDeskId);
                using (var userBl = new UserBll(identity, WebApiApplication.DbLogger))
                {
                    var product = CacheManager.GetProductById(input.ProductId);
                    if (product == null)
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.ProductNotFound);
                    var session = userBl.GetUserProductSession(identity.SessionId, product.Id);
                    if (session == null)
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.SessionNotFound);
                    return Ok(new ApiResponseBase { ResponseObject =  session.ToProductSessionOutput() });
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = Convert.ToInt32(ex.Detail.Id),
                    Description = ex.Detail.Message
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                return Ok(new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                });
            }
        }

        private SessionIdentity CheckToken(string token, int cashDeskId, bool checkExpiration = true, bool isForCashier = true)
        {
            using (var userBl = new UserBll(new SessionIdentity(), WebApiApplication.DbLogger))
            {
                var session = userBl.GetUserSession(token, checkExpiration);
                var user = userBl.GetUserById(session.UserId.Value);

                var partnerId = user.PartnerId;
                var currencyId = user.CurrencyId;
                int? betShopId = null;
                if (isForCashier)
                {
                    if (!session.CashDeskId.HasValue)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongToken);
                    if (session.CashDeskId != cashDeskId)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongToken);

                    var cashDesk = CacheManager.GetCashDeskById(cashDeskId);
                    var betShop = CacheManager.GetBetShopById(cashDesk.BetShopId);
                    currencyId = betShop.CurrencyId;
                    partnerId = betShop.PartnerId;
                    betShopId = betShop.Id;
                }

                var userIdentity = new SessionIdentity
                {
                    Id = session.UserId.Value,
                    LoginIp = session.Ip,
                    LanguageId = session.LanguageId,
                    SessionId = session.Id,
                    Token = session.Token,
                    ProductId = session.ProductId ?? 0,
                    PartnerId = partnerId,
                    CurrencyId = currencyId,
                    CashDeskId = session.CashDeskId ?? 0,
                    IsAdminUser = true,
                    ParentId = session.ParentId,
                    BetShopId = betShopId
                };
                return userIdentity;
            }
        }

        [HttpPost]
        public IHttpActionResult GetRegions(ApiRequestBase apiRequestBase)
        {
            try
            {
                var input = JsonConvert.DeserializeObject<Common.Models.WebSiteModels.ApiFilterRegion>(apiRequestBase.RequestObject);
                using (var regionBl = new RegionBll(new SessionIdentity(), WebApiApplication.DbLogger))
                {
                    return Ok(new ApiResponseBase
                    {
                        ResponseObject = new
                        {
                            Entities = regionBl.GetfnRegions(new FilterRegion { ParentId = input.ParentId, TypeId = input.TypeId },
                            apiRequestBase.LanguageId, false, apiRequestBase.PartnerId).Where(x => x.State == (int)RegionStates.Active).OrderBy(x => x.Name)
                            .Select(x => new
                            {
                                x.Id,
                                x.Name,
                                x.NickName,
                                x.IsoCode,
                                x.IsoCode3,
                            })
                        }
                    });
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = Convert.ToInt32(ex.Detail.Id),
                    Description = ex.Detail.Message
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                return Ok(new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                });
            }
        }

        [HttpPost]
        public IHttpActionResult ResetClientPassword(ApiRequestBase apiRequestBase)
        {
            try
            {
                var identity = CheckToken(apiRequestBase.Token, apiRequestBase.CashDeskId);
                var input = JsonConvert.DeserializeObject<ApiClientInput>(apiRequestBase.RequestObject);
                var cashDesk = CacheManager.GetCashDeskById(apiRequestBase.CashDeskId);
                using (var clientBl = new ClientBll(identity, WebApiApplication.DbLogger))
                {
                    var newpassword = clientBl.ResetClientPassword(input.ClientId, cashDesk.BetShopId, apiRequestBase.LanguageId, out string clientUserName);
                    return Ok(new ApiResponseBase { ResponseObject =  new { UserName = clientUserName, Password = newpassword } });
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = Convert.ToInt32(ex.Detail.Id),
                    Description = ex.Detail.Message
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                return Ok(new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                });
            }
        }

        [HttpPost]
        public IHttpActionResult GetDocuments(ApiRequestBase apiRequestBase)
        {
            try
            {
                var identity = CheckToken(apiRequestBase.Token, apiRequestBase.CashDeskId);
                var cashDesk = CacheManager.GetCashDeskById(apiRequestBase.CashDeskId);
                using (var contentBl = new ContentBll(identity, WebApiApplication.DbLogger))
                {
                    return Ok(new ApiResponseBase { ResponseObject =  contentBl.GetDocuments(identity.PartnerId, (int)DeviceTypes.BetShop) });
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = Convert.ToInt32(ex.Detail.Id),
                    Description = ex.Detail.Message
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                WebApiApplication.DbLogger.Error(ex);
                return Ok(new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                });
            }
        }
    }
}