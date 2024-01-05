using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
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
using IqSoft.CP.Common.Models.UserModels;
using IqSoft.CP.DAL.Models.Clients;
using Microsoft.AspNetCore.Mvc;
using IqSoft.CP.BetShopGatewayCore;
using IqSoft.CP.Common.Models.CacheModels;
using Microsoft.AspNetCore.Hosting;

namespace IqSoft.CP.BetShopGatewayWebApi.Controllers
{
    public class MainController : ControllerBase
    {
        private IWebHostEnvironment HostEnvironment;
        public MainController(IWebHostEnvironment _environment)
        {
            HostEnvironment = _environment;
        }

        [HttpPost]
        public IActionResult CardReaderAuthorization([FromQuery] RequestInfo info, AuthorizationInput input)
        {
            try
            {
                Program.DbLogger.Info(JsonConvert.SerializeObject(input));
                using var clientBll = new ClientBll(new SessionIdentity(), Program.DbLogger);
                using var betShopBl = new BetShopBll(clientBll);
                var authBase = Helpers.Helpers.CheckCardReaderHash(info.PartnerId, input.Hash, betShopBl);
                var token = ClientBll.LoginClientByCard(info.PartnerId, input.CardNumber, input.Ip, info.LanguageId, out int clientId);
                Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.ClientSessions, clientId));
                Helpers.Helpers.InvokeMessage("RemoveClient", clientId);
                var cashDesk = CacheManager.GetCashDeskById(authBase.CashDeskId);
                if (cashDesk == null)
                    throw BaseBll.CreateException(info.LanguageId, Constants.Errors.CashDeskNotFound);


                var response = new
                {
                    ResponseCode = "0",
                    Description = string.Empty,
                    Token = token
                };
                return Ok(response);
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new
                {
                    ResponseCode = ex.Detail.Id.ToString(),
                    Description = ex.Detail.Message
                };
                Program.DbLogger.Error(JsonConvert.SerializeObject(response));
                return Ok(response);
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new
                {
                    ResponseCode = Constants.Errors.GeneralException.ToString(),
                    Description = ex.Message
                };
                return Ok(response);
            }
        }

        [HttpPost]
        public IActionResult Authorization([FromQuery] RequestInfo info, AuthorizationInput input)
        {
            try
            {
                if (input.UserName == "tarazsport")
                    return Ok(new ApiResponseBase
                    {
                        ResponseCode = Constants.Errors.GeneralException,
                        Description = ""
                    });

                Program.DbLogger.Info(JsonConvert.SerializeObject(input));
                using var userBl = new UserBll(new SessionIdentity(), Program.DbLogger);
                using var betShopBl = new BetShopBll(userBl);
                var authBase = Helpers.Helpers.CheckCashDeskHash(info.PartnerId, input.Hash, betShopBl);
                var loginInput = new LoginUserInput
                {
                    PartnerId = info.PartnerId,
                    UserName = input.UserName,
                    Password = input.Password,
                    Ip = input.Ip,
                    LanguageId = info.LanguageId,
                    UserType = (int)UserTypes.Cashier,
                    CashDeskId = authBase.CashDeskId
                };
                var session = userBl.LoginUser(loginInput, out string imagaeData);
                var user = userBl.GetUserById(session.Id);
                if (user.Type != (int)UserTypes.Cashier)
                    throw BaseBll.CreateException(info.LanguageId, Constants.Errors.UserIsNotCashier);

                var cashDesk = CacheManager.GetCashDeskById(authBase.CashDeskId);
                if (cashDesk == null)
                    throw BaseBll.CreateException(info.LanguageId, Constants.Errors.CashDeskNotFound);

                var betShop = betShopBl.GetBetShopById(cashDesk.BetShopId, true, user.Id);
                if (betShop == null)
                    throw BaseBll.CreateException(info.LanguageId, Constants.Errors.BetShopNotFound);

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
                    PrintLogo = betShop.PrintLogo
                };

                var balance = betShopBl.UpdateShifts(userBl, user.Id, cashDesk.Id);
                response.Balance = balance;

                return Ok(response);
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
                Program.DbLogger.Error(ex);
                var response = new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return Ok(response);
            }
        }

        [HttpPost]
        public IActionResult GetCashierSessionByToken( ApiGetCashierSessionInput input)
        {
            try
            {
                using var userBl = new UserBll(new SessionIdentity(), Program.DbLogger);
                var session = userBl.GetUserSession(input.SessionToken, false);
                return Ok(session.ToApiCashierSession());
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
                Program.DbLogger.Error(ex);
                var response = new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return Ok(response);
            }
        }

        [HttpPost]
        public IActionResult GetCashierSessionByProductId(ApiGetCashierSessionInput input)
        {
            try
            {
                var identity = CheckToken(input.SessionToken, input.CashDeskId, true, false);
                using var userBl = new UserBll(identity, Program.DbLogger);
                if (identity.ParentId == null)
                    throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.PartnerNotFound);
                var session = userBl.GetUserSessionById(identity.ParentId.Value);
                return Ok(session.ToApiCashierSession());
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
                Program.DbLogger.Error(ex);
                var response = new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return Ok(response);
            }
        }

        [HttpPost]
        public IActionResult GetCashDeskInfo([FromQuery] RequestInfo info, GetCashDeskInfoInput input)
        {
            try
            {
                var identity = CheckToken(input.Token, input.CashDeskId);
                using var betShopBl = new BetShopBll(identity, Program.DbLogger);
                var cashDesk = CacheManager.GetCashDeskById(identity.CashDeskId);
                if (cashDesk == null)
                    throw BaseBll.CreateException(info.LanguageId, Constants.Errors.CashDeskNotFound);
                var betShop = betShopBl.GetBetShopById(cashDesk.BetShopId, false);
                if (betShop == null)
                    throw BaseBll.CreateException(info.LanguageId, Constants.Errors.BetShopNotFound);
                var balance = betShopBl.GetObjectBalanceWithConvertion((int)ObjectTypes.CashDesk,
                    cashDesk.Id,
                    betShop.CurrencyId);
                var response = new GetCashDeskInfoOutput
                {
                    CurrencyId = balance.CurrencyId,
                    Balance = balance.AvailableBalance,
                    CashDeskId = (int)balance.ObjectId,
                    CurrentLimit = betShop.CurrentLimit
                };
                return Ok(response);
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
                Program.DbLogger.Error(ex);
                var response = new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return Ok(response);
            }
        }

        [HttpPost]
        public IActionResult GetProductSession([FromQuery] RequestInfo info, GetProductSessionInput input)
        {
            try
            {
                var identity = CheckToken(input.Token, input.CashDeskId);
                using var userBl = new UserBll(identity, Program.DbLogger);
                var product = CacheManager.GetProductById(input.ProductId);
                if (product == null)
                    throw BaseBll.CreateException(info.LanguageId, Constants.Errors.ProductNotFound);
                var productSession = userBl.CreateProductSession(identity, product.Id);
                var response = new GetProductSessionOutput
                {
                    ProductId = input.ProductId,
                    ProductToken = productSession.Token
                };
                return Ok(response);
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
                Program.DbLogger.Error(ex);
                var response = new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return Ok(response);
            }
        }

        [HttpPost]
        public IActionResult CloseSession([FromQuery] RequestInfo info, CloseSessionInput input)
        {
            try
            {
                var identity = CheckToken(input.Token, input.CashDeskId);
                using var userBl = new UserBll(identity, Program.DbLogger);
                if (input.ProductId.HasValue && identity.ProductId != input.ProductId)
                    throw BaseBll.CreateException(info.LanguageId, Constants.Errors.WrongProductId);
                userBl.LogoutUser(input.Token);
                return Ok(new ApiResponseBase());
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
                Program.DbLogger.Error(ex);
                var response = new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return Ok(response);
            }
        }
        [HttpPost]
        public IActionResult RegisterClient([FromQuery] RequestInfo info, ClientModel input)
        {
            try
            {
                input.PartnerId = info.PartnerId;
                input.LanguageId = info.LanguageId;
                bool generatedUsername = false;
                if (string.IsNullOrWhiteSpace(input.UserName))
                {
                    input.UserName = CommonFunctions.GetRandomString(10);
                    generatedUsername = true;
                }
                var identity = CheckToken(input.Token, input.CashDeskId);
                using var clientBl = new ClientBll(identity, Program.DbLogger);
                var ip = Request.Headers.ContainsKey("CF-Connecting-IP") ? Request.Headers["CF-Connecting-IP"].ToString() : Constants.DefaultIp;
                var client = input.MapToClient();
                client.RegistrationIp = ip;
                var clientRegistrationInput = new ClientRegistrationInput
                {
                    ClientData = input.MapToClient(),
                    IsQuickRegistration = false,
                    IsFromAdmin = true,
                    GeneratedUsername = generatedUsername
                };
                client = clientBl.RegisterClient(clientRegistrationInput, HostEnvironment);
                var response = client.MapToApiLoginClientOutput(info.TimeZone);
                return Ok(response);
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
                Program.DbLogger.Error(ex);
                var response = new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return Ok(response);
            }
        }

        [HttpPost]
        public IActionResult GetClient([FromQuery] RequestInfo info, ApiGetClientInput input)
        {
            try
            {
                var identity = CheckToken(input.Token, input.CashDeskId);
                using var clientBl = new ClientBll(identity, Program.DbLogger);
                var client = clientBl.GetClients(new FilterClient
                {
                    Id = input.ClientId,
                    UserName = input.UserName,
                    DocumentNumber = input.DocumentNumber,
                    Email = input.Email
                }).FirstOrDefault();
                if (client == null)
                    throw BaseBll.CreateException(info.LanguageId, Constants.Errors.ClientNotFound);

                var result = clientBl.GetClientsSettings(client.Id, true);

                var settings = result.Select(x => new ApiClientSetting
                {
                    Name = x.Name,
                    StringValue = string.IsNullOrEmpty(x.StringValue) ?
                        (x.NumericValue.HasValue ? x.NumericValue.Value.ToString() : String.Empty) : x.StringValue,
                    DateValue = x.DateValue == null ? x.CreationTime : x.DateValue,
                    LastUpdateTime = x.LastUpdateTime
                }).ToList();

                var response = client.ToGetClientOutput();
                response.Settings = settings;

                return Ok(response);
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
                Program.DbLogger.Error(ex);
                var response = new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return Ok(response);
            }
        }

        [HttpPost]
        public IActionResult DepositToInternetClient([FromQuery] RequestInfo info, DepositToInternetClientInput input)
        {
            try
            {
                var identity = CheckToken(input.Token, input.CashDeskId);

                using var clientBl = new ClientBll(identity, Program.DbLogger);
                var cashDesk = CacheManager.GetCashDeskById(input.CashDeskId);
                var betShop = CacheManager.GetBetShopById(cashDesk.BetShopId);

                var client = CacheManager.GetClientById(input.ClientId);
                if (client == null || betShop.PartnerId != client.PartnerId)
                    throw BaseBll.CreateException(info.LanguageId, Constants.Errors.ClientNotFound);
                var paymentRequest = new DAL.PaymentRequest
                {
                    Amount = input.Amount,
                    ClientId = client.Id,
                    CurrencyId = input.CurrencyId,
                    CashDeskId = input.CashDeskId,
                    CashierId = input.CashierId,
                    ExternalTransactionId = input.TransactionId
                };
                var paymentRequestDocument = clientBl.CreateDepositFromBetShop(paymentRequest);
                if (paymentRequestDocument.Status == (int)PaymentRequestStates.Approved)
                {
                    Helpers.Helpers.InvokeMessage("ClientDepositWithBonus", client.Id);
                    Helpers.Helpers.InvokeMessage("PaymentRequst", paymentRequestDocument.Id);
                }

                var response = new DepositToInternetClientOutput
                {
                    TransactionId = paymentRequestDocument.Id,
                    CurrentLimit = paymentRequestDocument.ObjectLimit,
                    CashierBalance = paymentRequestDocument.CashierBalance,
                    ClientBalance = paymentRequestDocument.ClientBalance,
                    ClientUserName = client.UserName,
                    CurrencyId = input.CurrencyId,
                    ClientId = client.Id,
                    DocumentNumber = client.DocumentNumber,
                    Amount = input.Amount,
                    Status = paymentRequestDocument.Status,
                    DepositDate = paymentRequestDocument.CreationTime
                };
                return Ok(response);
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
                Program.DbLogger.Error(JsonConvert.SerializeObject(response));
                return Ok(response);
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return Ok(response);
            }
        }

        [HttpPost]
        public IActionResult GetPaymentRequests([FromQuery] RequestInfo info, GetPaymentRequestsInput input)
        {
            try
            {
                var identity = CheckToken(input.Token, input.CashDeskId);
                using var paymentSystemBll = new PaymentSystemBll(identity, Program.DbLogger);
                var cashDesk = CacheManager.GetCashDeskById(input.CashDeskId);
                if (cashDesk == null)
                    throw BaseBll.CreateException(info.LanguageId, Constants.Errors.CashDeskNotFound);

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
                if (input.ClientId != null)
                    filter.ClientIds = new FiltersOperation
                    {
                        IsAnd = true,
                        OperationTypeList = new List<FiltersOperationType>
                                {
                                    new FiltersOperationType
                                    {
                                        OperationTypeId = (int)FilterOperations.IsEqualTo,
                                        IntValue = input.ClientId.Value
                                    }
                                }
                    };

                var paymentRequests = paymentSystemBll.GetPaymentRequests(filter, false).Where(x => x.ClientDocumentNumber == input.DocumentNumber).ToList();

                var response = new GetPaymentRequestsOutput
                {
                    PaymentRequests = new List<PaymentRequest>()
                };
                if (paymentRequests.Any())
                {
                    var paymentRequest = paymentRequests.FirstOrDefault(x => x.CashCode == input.CashCode);
                    if (paymentRequest == null)
                        throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongCashCode);
                    var request = new PaymentRequest
                    {
                        Id = paymentRequest.Id,
                        Amount = Math.Floor((paymentRequest.Amount - paymentRequest.CommissionAmount ?? 0) * 100) / 100,
                        ClientId = paymentRequest.ClientId,
                        ClientFirstName = paymentRequest.FirstName,
                        ClientLastName = paymentRequest.LastName,
                        ClientEmail = paymentRequest.Email,
                        UserName = paymentRequest.UserName,
                        DocumentNumber = paymentRequest.ClientDocumentNumber,
                        CreationTime = paymentRequest.CreationTime,
                        CurrencyId = paymentRequest.CurrencyId,
                        Info = paymentRequest.Info
                    };
                    response.PaymentRequests.Add(request);
                }
                return Ok(response);
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
                Program.DbLogger.Error(ex);
                var response = new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return Ok(response);
            }
        }

        [HttpPost]
        public IActionResult PayPaymentRequest(PayPaymentRequestInput input)
        {
            try
            {
                var identity = CheckToken(input.Token, input.CashDeskId);
                using var clientBl = new ClientBll(identity, Program.DbLogger);
                using var documentBl = new DocumentBll(clientBl);
                using var notificationBl = new NotificationBll(clientBl);
                var resp = clientBl.ChangeWithdrawRequestState(input.PaymentRequestId,
                PaymentRequestStates.Approved,
                input.Comment, input.CashDeskId, input.CashierId, true, string.Empty, documentBl, notificationBl, true);

                clientBl.PayWithdrawFromBetShop(resp, input.CashDeskId, input.CashierId, documentBl);
                Helpers.Helpers.InvokeMessage("RemoveKeyFromCache", string.Format("{0}_{1}", Constants.CacheItems.ClientBalance, resp.ClientId));
                Helpers.Helpers.InvokeMessage("PaymentRequst", input.PaymentRequestId);
                var response = new PayPaymentRequestOutput
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
                };
                return Ok(response);
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
                Program.DbLogger.Error(ex);
                var response = new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return Ok(response);
            }
        }

        [HttpPost]
        public IActionResult GetBetByBarcode([FromQuery] RequestInfo info, GetBetByBarcodeInput input)
        {
            try
            {
                var identity = CheckToken(input.Token, input.CashDeskId);
                using var reportBl = new ReportBll(identity, Program.DbLogger);
                var bet = reportBl.GetBetByBarcode(input.CashDeskId, input.Barcode);
                var response = new GetBetShopBetsOutput
                {
                    Bets = bet == null ? new List<BetShopBet>() : new List<BetShopBet> { bet.ToBetShopBet() }
                };
                return Ok(response);
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
                Program.DbLogger.Error(ex);
                var response = new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return Ok(response);
            }
        }

        [HttpPost]
        public IActionResult GetBetByDocumentId(GetBetByDocumentIdInput input)
        {
            try
            {
                var identity = CheckToken(input.Token, input.CashDeskId);
                using var betShopBl = new BetShopBll(identity, Program.DbLogger);
                var bet = betShopBl.GetBetShopBetByDocumentId(input.DocumentId, input.IsForPrint);
                var response = (bet == null ? new ApiBetShopTicket() : bet.ToApiBetShopTicket());
                return Ok(response);
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
                Program.DbLogger.Error(ex);
                var response = new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return Ok(response);
            }
        }

        [HttpPost]
        public IActionResult PayWin([FromQuery] RequestInfo info, PayWinInput input)
        {
            try
            {
                var identity = CheckToken(input.Token, input.CashDeskId);
                using var betShopBl = new BetShopBll(identity, Program.DbLogger);
                using var documentBl = new DocumentBll(betShopBl);
                var clientOperation = new ClientOperation
                {
                    ClientId = input.CashierId,
                    CashDeskId = input.CashDeskId,
                    ParentDocumentId = input.BetDocumentId,
                    ExternalTransactionId = input.ExternalTransactionId
                };
                var document = betShopBl.PayWinFromBetShop(clientOperation, documentBl);
                var cashDesk = CacheManager.GetCashDeskById(input.CashDeskId);
                var betShop = betShopBl.GetBetShopById(cashDesk.BetShopId, false);
                var response = new FinOperationResponse
                {
                    CurrentLimit = betShop.CurrentLimit,
                    CashierBalance = betShopBl.GetObjectBalanceWithConvertion((int)ObjectTypes.CashDesk,
                            input.CashDeskId, document.CurrencyId).AvailableBalance,
                    CurrencyId = document.CurrencyId
                };
                return Ok(response);
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
                Program.DbLogger.Error(ex);
                var response = new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return Ok(response);
            }
        }

        [HttpPost]
        public IActionResult CreateDebitCorrectionOnCashDesk(Models.CashDeskCorrectionInput input)
        {
            try
            {
                using var betShopBl = new BetShopBll(new SessionIdentity(), Program.DbLogger);
                var correction = new DAL.Models.CashDeskCorrectionInput
                {
                    Amount = input.Amount,
                    CurrencyId = input.CurrencyId,
                    CashDeskId = input.CashDeskId,
                    Info = input.Info,
                    ExternalOperationId = input.ExternalOperationId,
                    CashierId = input.CashierId,
                    ExternalTransactionId = input.ExternalTransactionId
                };
                betShopBl.CreateDebitCorrectionOnCashDesk(correction);
                return Ok(new ApiResponseBase());
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
                Program.DbLogger.Error(ex);
                var response = new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return Ok(response);
            }
        }

        [HttpPost]
        public IActionResult CreateCreditCorrectionOnCashDesk(Models.CashDeskCorrectionInput input)
        {
            try
            {
                using var betShopBl = new BetShopBll(new SessionIdentity(), Program.DbLogger);
                var correction = new DAL.Models.CashDeskCorrectionInput
                {
                    Amount = input.Amount,
                    CurrencyId = input.CurrencyId,
                    CashDeskId = input.CashDeskId,
                    Info = input.Info,
                    ExternalOperationId = input.ExternalOperationId,
                    CashierId = input.CashierId,
                    ExternalTransactionId = input.ExternalTransactionId
                };
                betShopBl.CreateCreditCorrectionOnCashDesk(correction);
                return Ok(new ApiResponseBase());
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
                Program.DbLogger.Error(ex);
                var response = new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return Ok(response);
            }
        }

        [HttpPost]
        public IActionResult GetCashDesksBalancesByDate([FromQuery] RequestInfo info, GetCashiersBalanceIntput input)
        {
            try
            {
                using var partnerBl = new BetShopBll(new SessionIdentity(), Program.DbLogger);
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
                return Ok(response);
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
                Program.DbLogger.Error(ex);
                var response = new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return Ok(response);
            }
        }

        [HttpPost]
        public IActionResult GetCashDeskCurrentBalance(GetCashDeskCurrentBalanceInput input)
        {
            try
            {
                var identity = CheckToken(input.Token, input.CashDeskId);
                using var betShopBl = new BetShopBll(identity, Program.DbLogger);
                var cashDesk = CacheManager.GetCashDeskById(input.CashDeskId);
                if (cashDesk == null)
                    throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.CashDeskNotFound);
                var betShop = betShopBl.GetBetShopById(cashDesk.BetShopId, false);
                if (betShop == null)
                    throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.BetShopNotFound);

                var cashierBalance = betShopBl.GetObjectBalanceWithConvertion((int)ObjectTypes.CashDesk,
                    input.CashDeskId, betShop.CurrencyId);
                var response = new GetCashDeskCurrentBalanceOutput
                {
                    Balance = cashierBalance.AvailableBalance,
                    CurrentLimit = betShop.CurrentLimit
                };
                return Ok(response);
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
                Program.DbLogger.Error(ex);
                var response = new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return Ok(response);
            }
        }

        [HttpPost]
        public IActionResult CloseShift([FromQuery] RequestInfo info, ApiCloseShiftInput input)
        {
            try
            {
                var identity = CheckToken(input.Token, input.CashDeskId, false);
                using var betshopBl = new BetShopBll(identity, Program.DbLogger);
                using var userBl = new UserBll(betshopBl);
                using var documentBl = new DocumentBll(userBl);
                using var reportBl = new ReportBll(documentBl);
                Program.DbLogger.Info(JsonConvert.SerializeObject(input));
                var session = userBl.GetUserSession(input.Token, false);
                session.State = (int)SessionStates.Inactive;
                var result = betshopBl.CloseShift(documentBl, reportBl, input.CashDeskId, input.CashierId,
                    session.Id).MapToApiCloseShiftOutput(info.TimeZone);
                Program.DbLogger.Info(JsonConvert.SerializeObject(result));
                return Ok(result);
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = Convert.ToInt32(ex.Detail.Id),
                    Description = ex.Detail.Message
                };
                Program.DbLogger.Error(JsonConvert.SerializeObject(response));
                return Ok(response);
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return Ok(response);
            }
        }

        [HttpPost]
        public IActionResult ChangeCashierPassword(ChangePasswordInput input)
        {
            try
            {
                var identity = CheckToken(input.Token, input.CashDeskId, false);
                using var userBl = new UserBll(identity, Program.DbLogger);
                userBl.ChangeUserPassword(identity.Id, input.OldPassword, input.NewPassword);
                return Ok(new ApiResponseBase());
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new ApiResponseBase
                {
                    ResponseCode = Convert.ToInt32(ex.Detail.Id),
                    Description = ex.Detail.Message
                };
                Program.DbLogger.Error(JsonConvert.SerializeObject(response));
                return Ok(response);
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return Ok(response);
            }
        }

        #region Reporting

        [HttpPost]
        public IActionResult GetBetShopBets([FromQuery] RequestInfo info, GetReportByBetInput input)
        {
            try
            {
                var identity = CheckToken(input.Token, input.CashDeskId);
                using var reportBl = new ReportBll(identity, Program.DbLogger);
                var betDateFrom = input.BetDateFrom.GetUTCDateFromGmt(info.TimeZone);
                var betDateBefore = input.BetDateBefore.GetUTCDateFromGmt(info.TimeZone);

                if (betDateFrom < DateTime.UtcNow.AddDays(-4))
                    throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongParameters);

                var bets =
                    reportBl.GetBetshopBetsForCashier(betDateFrom, betDateBefore, input.CashDeskId, input.CashierId,
                        input.ProductId, input.State);
                var response = new GetBetShopBetsOutput
                {
                    Bets = bets.Select(x => x.MapToBetShopBet(info.TimeZone)).ToList()
                };
                return Ok(response);
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
                Program.DbLogger.Error(ex);
                var response = new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return Ok(response);
            }
        }

        [HttpPost]
        public IActionResult GetBetShopOperations([FromQuery] RequestInfo info, GetBetShopOperationsInput input)
        {
            try
            {
                var identity = CheckToken(input.Token, input.CashDeskId);
                using var repaymentSystemBl = new PaymentSystemBll(identity, Program.DbLogger);
                var fromDate = input.FromDate.GetUTCDateFromGmt(info.TimeZone);
                var toDate = input.ToDate.GetUTCDateFromGmt(info.TimeZone);
                if ((toDate - fromDate).TotalDays > 30)
                    throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongParameters);
                var filter = new FilterfnPaymentRequest
                {
                    FromDate = (long)fromDate.Year * 1000000 + (long)fromDate.Month * 10000 + (long)fromDate.Day * 100 + (long)fromDate.Hour,
                    ToDate = (long)toDate.Year * 1000000 + (long)toDate.Month * 10000 + (long)toDate.Day * 100 + (long)toDate.Hour,
                    CashierIds =
                        new FiltersOperation
                        {
                            OperationTypeList =
                                new List<FiltersOperationType>
                                {
                                        new FiltersOperationType
                                        {
                                            OperationTypeId = (int)FilterOperations.IsEqualTo,
                                            IntValue = input.CashierId
                                        }
                                }
                        },
                    CashDeskIds =
                        new FiltersOperation
                        {
                            OperationTypeList =
                                new List<FiltersOperationType>
                                {
                                        new FiltersOperationType
                                        {
                                            OperationTypeId = (int)FilterOperations.IsEqualTo,
                                            IntValue = input.CashDeskId
                                        }
                                }
                        },
                    States =
                        new FiltersOperation
                        {
                            OperationTypeList =
                                new List<FiltersOperationType>
                                {
                                        new FiltersOperationType
                                        {
                                            OperationTypeId = (int)FilterOperations.IsEqualTo,
                                            IntValue = (int) PaymentRequestStates.Approved
                                        }
                                }
                        },
                };

                var operations =
                    repaymentSystemBl.GetPaymentRequestsPaging(filter, false, true)
                        .Entities.OrderByDescending(x => x.Id)
                        .ToList();
                var response = operations.MapToBetShopOperations(info.TimeZone);
                return Ok(response);
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
                Program.DbLogger.Error(ex);
                var response = new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return Ok(response);
            }
        }

        [HttpPost]
        public IActionResult GetShiftReport([FromQuery] RequestInfo info, GetShiftReportInput input)
        {
            try
            {
                var identity = CheckToken(input.Token, input.CashDeskId);
                using var reportBl = new ReportBll(identity, Program.DbLogger);
                var startTime = input.StartTime.GetUTCDateFromGmt(info.TimeZone);
                var endTime = input.EndTime.GetUTCDateFromGmt(info.TimeZone);
                if (startTime < DateTime.UtcNow.AddDays(-8))
                    throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.WrongParameters);

                var response = reportBl.GetShifts(startTime, endTime, input.CashDeskId, input.CashierId);

                return Ok(new ApiGetShiftReportOutput
                {
                    Shifts =
                        response.Select(x => x.ToApiShift(info.TimeZone))
                            .OrderByDescending(x => x.Id)
                            .ToList()
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
                Program.DbLogger.Error(ex);
                var response = new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return Ok(response);
            }
        }

        [HttpPost]
        public IActionResult GetCashDeskOperations([FromQuery] RequestInfo info, GetCashDeskOperationsInput input)
        {
            try
            {
                var identity = CheckToken(input.Token, input.CashDeskId);
                using var reportBl = new ReportBll(identity, Program.DbLogger);
                using var betshopBl = new BetShopBll(reportBl);
                var currentTime = DateTime.UtcNow;
                var createdFrom = (input.StartTime == null || input.StartTime == DateTime.MinValue) ? currentTime.AddDays(-1) :
                    input.StartTime.Value.GetUTCDateFromGmt(info.TimeZone);

                var createdBefore = (input.EndTime == null || input.EndTime == DateTime.MinValue) ? currentTime.AddDays(1) :
                    input.EndTime.Value.GetUTCDateFromGmt(info.TimeZone);
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
                            createdFrom = shift.StartTime;
                    }
                }
                else if (createdFrom < currentTime.AddDays(-4))
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
                                        IntValue = input.CashDeskId
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
                    FromDate = createdFrom,
                    ToDate = createdBefore
                };
                var transactions = reportBl.GetCashDeskTransactions(filter, info.LanguageId);
                var response = new CashDeskOperationsOutput
                {
                    Operations = transactions.Select(x => x.MapToCashDeskOperation(info.TimeZone)).ToList(),
                    StartTime = createdFrom.GetGMTDateFromUTC(info.TimeZone),
                    EndTime = createdBefore.GetGMTDateFromUTC(info.TimeZone)
                };
                return Ok(response);
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = new CashDeskOperationsOutput
                {
                    ResponseCode = ex.Detail.Id,
                    Description = ex.Detail.Message,
                    Operations = new List<CashDeskOperation>()
                };
                Program.DbLogger.Error(JsonConvert.SerializeObject(response));
                return Ok(response);
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new CashDeskOperationsOutput
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message,
                    Operations = new List<CashDeskOperation>()
                };
                return Ok(response);
            }
        }

        #endregion

        [HttpPost]
        public IActionResult GetUserProductSessions([FromQuery] RequestInfo info, GetCashDeskInfoInput input)
        {
            try
            {
                var identity = CheckToken(input.Token, input.CashDeskId);
                using var userBl = new UserBll(identity, Program.DbLogger);
                var product = CacheManager.GetProductById(identity.ProductId);
                if (product == null)
                    throw BaseBll.CreateException(identity.LanguageId, Constants.Errors.ProductNotFound);
                var response = userBl.GetUserProductSession(identity.Id, identity.ProductId).ToProductSessionOutput();
                return Ok(response);
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
                Program.DbLogger.Error(ex);
                var response = new ApiResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return Ok(response);
            }
        }

        private static SessionIdentity CheckToken(string token, int cashDeskId, bool checkExpiration = true, bool isForCashier = true)
        {
            using var userBl = new UserBll(new SessionIdentity(), Program.DbLogger);
            var session = userBl.GetUserSession(token, checkExpiration);
            var user = userBl.GetUserById(session.UserId);
            var currencyId = user.CurrencyId;
            var partnerId = user.PartnerId;
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
            }

            return new SessionIdentity
            {
                Id = session.UserId,
                LoginIp = session.Ip,
                LanguageId = session.LanguageId,
                SessionId = session.Id,
                Token = session.Token,
                ProductId = session.ProductId ?? 0,
                PartnerId = partnerId,
                CurrencyId = currencyId,
                CashDeskId = session.CashDeskId ?? 0,
                IsAdminUser = true,
                ParentId = session.ParentId
            };
        }
    }
}