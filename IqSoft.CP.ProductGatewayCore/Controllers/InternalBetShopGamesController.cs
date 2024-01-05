using System;
using System.Linq;
using System.Collections.Generic;
using System.ServiceModel;
using Microsoft.AspNetCore.Mvc;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.BLL.Services;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Integration.ProductsIntegration;
using Newtonsoft.Json;
using IqSoft.CP.ProductGateway.Models.IqSoft;
using IqSoft.CP.DAL.Models.Cache;
using IqSoft.CP.ProductGateway.Helpers;
using IqSoft.CP.Common.Models.CacheModels;

namespace IqSoft.CP.ProductGateway.Controllers
{
    [ApiController]
    public class InternalBetShopGamesController : ControllerBase
    {
		private int providerId = CacheManager.GetGameProviderByName(Constants.GameProviders.Internal).Id;

        [HttpPost]
        [Route("{partnerId}/api/InternalBetShopGames/Authorization")]
        public IActionResult Authorization(int partnerId, AuthorizationInput input)
        {
            try
            {
                using (var userBl = new UserBll(new SessionIdentity(), Program.DbLogger))
                {
                    var session = userBl.GetUserSession(input.Token, false);
                    if (!session.CashDeskId.HasValue)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongToken);
                    var cashDesk = CacheManager.GetCashDeskById(session.CashDeskId.Value);
                    var betShop = CacheManager.GetBetShopById(cashDesk.BetShopId);

                    var user = CacheManager.GetUserById(session.UserId);
                    var response = new AuthorizationOutput
                    {
                        ClientId = user.Id.ToString(),
                        Token = input.Token,
                        NickName = user.UserName,
                        CurrencyId = user.CurrencyId,
                        BetShopCurrencyId = betShop.CurrencyId,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Gender = user.Gender,
                        BetShopId = betShop.Id,
                        CashDeskId = cashDesk.Id,
                        BetShopName = betShop.Name,
                        BetShopAddress = betShop.Address
                    };
                    return Ok(response);
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = ex.Detail == null
                    ? new ResponseBase
                    {
                        ResponseCode = Constants.Errors.GeneralException,
                        Description = ex.Message
                    }
                    : new ResponseBase
                    {
                        ResponseCode = ex.Detail.Id,
                        Description = ex.Detail.Message
                    };
                Program.DbLogger.Error(JsonConvert.SerializeObject(response));
                return Ok(response);
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new ResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return Ok(response);
            }
        }

        [HttpPost]
        [Route("{partnerId}/api/InternalBetShopGames/GetBalance")]
        public IActionResult GetBalance(int partnerId, GetBalanceInput input)
        {
            try
            {
                var userSession = CheckUserSession(input.Token, input.ClientId, true);
                using (var userBl = new UserBll(userSession, Program.DbLogger))
                {
                    var balance = userBl.GetObjectBalanceWithConvertion((int) ObjectTypes.CashDesk,
                        userSession.CashDeskId, input.CurrencyId);

                    var response = new GetBalanceOutput
                    {
                        CurrencyId = balance.CurrencyId,
                        AvailableBalance = balance.AvailableBalance
                    };
                    return Ok(response);
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = ex.Detail == null
                    ? new
                    {
                        ResponseCode = Constants.Errors.GeneralException,
                        Description = ex.Message,
                        Info = (decimal?)0
                    }
                    : new 
                    {
                        ResponseCode = ex.Detail.Id,
                        Description = ex.Detail.Message,
                        Info = ex.Detail.DecimalInfo
                    };
                Program.DbLogger.Error(JsonConvert.SerializeObject(response) + " " + JsonConvert.SerializeObject(input));
                return Ok(response);
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new ResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return Ok(response);
            }
        }

        [HttpPost]
        [Route("{partnerId}/api/InternalBetShopGames/Credit")]
        public IActionResult Credit(int partnerId, ApiFinOperationInput input)
        {
			var typeId = (input.TypeId == null
                ? null
                : (int?) IqSoftHelpers.BetTypesMapping.First(x => x.Key == input.TypeId.Value).Value);

            try
            {
                var sessionIdentity = CheckUserSession(input.Token, input.ClientId, true);
				using (var betShopBl = new BetShopBll(sessionIdentity, Program.DbLogger))
				{
					using (var documentBl = new DocumentBll(betShopBl))
					{
						var response = new FinOperationOutput
						{
							OperationItems = new List<FinOperationOutputItem>()
						};
						var operationsFromProduct = new ListOfOperationsFromApi
						{
							CurrencyId = input.CurrencyId,
							RoundId = input.RoundId,
							GameProviderId = providerId,
							ExternalOperationId = null,
							ExternalProductId = input.GameId,
							TransactionId = input.TransactionId,
							TypeId = typeId,
							Info = input.Info,
							OperationItems = new List<OperationItemFromProduct>
							{
								new OperationItemFromProduct
								{
									CashierId = input.ClientId,
									CashDeskId = sessionIdentity.CashDeskId,
									Amount = input.Amount,
									Type = input.Type,
									PossibleWin = input.PossibleWin
								}
							}
						};

						var documents = betShopBl.CreateBetsFromBetShop(operationsFromProduct, documentBl);
						foreach (var bet in documents.Documents)
						{
							var cashDesk = CacheManager.GetCashDeskById(bet.CashDeskId.Value);
							var betShop = betShopBl.GetBetShopById(cashDesk.BetShopId, false);
							var outputItem =
								new FinOperationOutputItem
								{
									BarCode = bet.Barcode,
									BetId = bet.Id,
									TicketNumber = bet.TicketNumber,
									ClientId = bet.UserId.Value,
									CashDeskId = bet.CashDeskId.Value,
									CurrentLimit = betShop.CurrentLimit,
									Balance = betShopBl.GetObjectBalanceWithConvertion(
											(int)ObjectTypes.CashDesk,
											bet.CashDeskId.Value, bet.CurrencyId).AvailableBalance,
									CurrencyId = betShop.CurrencyId
								};
							response.OperationItems.Add(outputItem);
						}
						return Ok(response);
					}
				}
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = ex.Detail == null
                    ? new FinOperationOutput
                    {
                        ResponseCode = Constants.Errors.GeneralException,
                        Description = ex.Message,
                        OperationItems = new List<FinOperationOutputItem>()
                    }
                    : new FinOperationOutput
                    {
                        ResponseCode = ex.Detail.Id,
                        Description = ex.Detail.Message,
                        OperationItems = new List<FinOperationOutputItem>()
                    };
                Program.DbLogger.Error(JsonConvert.SerializeObject(response));
                return Ok(response);
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new FinOperationOutput
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message,
                    OperationItems = new List<FinOperationOutputItem>()
                };
                return Ok(response);
            }
        }

        [HttpPost]
        [Route("{partnerId}/api/InternalBetShopGames/Debit")]
        public IActionResult Debit(int partnerId, ApiFinOperationInput input)
        {
            try
            {
				using (var betShopBl = new BetShopBll(new SessionIdentity(), Program.DbLogger))
				{
					using (var documentBl = new DocumentBll(betShopBl))
					{
						var response = new FinOperationOutput
						{
							OperationItems = new List<FinOperationOutputItem>()
						};

						var product = CacheManager.GetProductByExternalId(providerId, input.GameId);

						var creditTransaction = betShopBl.GetDocumentByExternalId(input.CreditTransactionId,
								product.Id, providerId, null, (int)OperationTypes.Bet);

						if (creditTransaction == null)
							throw BaseBll.CreateException(string.Empty,
								Constants.Errors.CanNotConnectCreditAndDebit);

						if (input.BetState != null)
							creditTransaction.State = input.BetState.Value;
						var operationsFromProduct = new ListOfOperationsFromApi
						{
							CurrencyId = input.CurrencyId,
							RoundId = input.RoundId,
							GameProviderId = providerId,
							CreditTransactionId = creditTransaction.Id,
							ExternalOperationId = null,
							OperationTypeId = input.OperationTypeId,
							ExternalProductId = input.GameId,
							TransactionId = input.TransactionId,
							Info = input.Info,
							State = input.BetState,
							OperationItems = new List<OperationItemFromProduct>
						{
							new OperationItemFromProduct
							{
								CashierId = input.ClientId,
								CashDeskId = input.CashDeskId ?? 0,
								Amount = input.Amount
							}
						}
						};

						var documents = betShopBl.CreateWinsToBetShop(operationsFromProduct, documentBl);
						foreach (var win in documents.Documents)
						{
							var outputItem =
								new FinOperationOutputItem
								{
									BarCode = win.Barcode,
									BetId = win.Id,
									ClientId = win.UserId.Value,
									CashDeskId = win.CashDeskId.Value
								};
							response.OperationItems.Add(outputItem);
						}
						return Ok(response);
					}
				}
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = ex.Detail == null
                    ? new ResponseBase
                    {
                        ResponseCode = Constants.Errors.GeneralException,
                        Description = ex.Message
                    }
                    : new ResponseBase
                    {
                        ResponseCode = ex.Detail.Id,
                        Description = ex.Detail.Message
                    };
                Program.DbLogger.Error(JsonConvert.SerializeObject(response));
                return Ok(response);
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new ResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return Ok(response);
            }
        }

        [HttpPost]
        [Route("{partnerId}/api/InternalBetShopGames/RollBack")]
        public IActionResult RollBack(int partnerId, ApiFinOperationInput input)
        {
            try
            {
                Program.DbLogger.Info(JsonConvert.SerializeObject(input));
                using (var documentBl = new DocumentBll(new SessionIdentity(), Program.DbLogger))
                {
                    var operationsFromProduct = new ListOfOperationsFromApi
                    {
                        GameProviderId = providerId,
                        TransactionId = input.TransactionId,
                        ExternalProductId = input.GameId,
                        ExternalOperationId = null,
                        Info = input.Info
                    };
                    documentBl.RollbackProductTransactions(operationsFromProduct);
                    BaseHelpers.RemoveClientBalanceFromeCache(input.ClientId);
                    var response = new ResponseBase();
                    return Ok(response);
                }
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = ex.Detail == null
                    ? new ResponseBase
                    {
                        ResponseCode = Constants.Errors.GeneralException,
                        Description = ex.Message
                    }
                    : new ResponseBase
                    {
                        ResponseCode = ex.Detail.Id,
                        Description = ex.Detail.Message
                    };
                Program.DbLogger.Error(JsonConvert.SerializeObject(response));
                return Ok(response);
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new ResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return Ok(response);
            }
        }

        [HttpPost]
        [Route("{partnerId}/api/InternalBetShopGames/CheckCashierToken")]
        public IActionResult CheckCashierToken(int partnerId, TokenOperationInput input)
        {
            try
            {
                var response = new ResponseBase();
                using (var userBl = new UserBll(new SessionIdentity(), Program.DbLogger))
                {
                    userBl.GetUserSession(input.Token, false);
                }
                return Ok(response);
            }
            catch (FaultException<BllFnErrorType> ex)
            {
                var response = ex.Detail == null
                    ? new ResponseBase
                    {
                        ResponseCode = Constants.Errors.GeneralException,
                        Description = ex.Message
                    }
                    : new ResponseBase
                    {
                        ResponseCode = ex.Detail.Id,
                        Description = ex.Detail.Message
                    };
                Program.DbLogger.Error(JsonConvert.SerializeObject(response));
                return Ok(response);
            }
            catch (Exception ex)
            {
                Program.DbLogger.Error(ex);
                var response = new ResponseBase
                {
                    ResponseCode = Constants.Errors.GeneralException,
                    Description = ex.Message
                };
                return Ok(response);
            }
        }

        private SessionIdentity CheckUserSession(string token, int cashierId, bool checkExpiration)
        {
            using (var userBl = new UserBll(new SessionIdentity(), Program.DbLogger))
            {
                var session = userBl.GetUserSession(token, checkExpiration);
                if (session.UserId != cashierId)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongClientId);
                if (session.CashDeskId == null)
                    throw BaseBll.CreateException(string.Empty, Constants.Errors.WrongClientId);

                var user = CacheManager.GetUserById(session.UserId);
                var userIdentity = new SessionIdentity
                {
                    LanguageId = session.LanguageId,
                    LoginIp = session.Ip,
                    PartnerId = user.PartnerId,
                    SessionId = session.Id,
                    Token = session.Token,
                    Id = session.UserId,
                    CurrencyId = user.CurrencyId,
                    IsAdminUser = false,
                    CashDeskId = session.CashDeskId.Value
                };
                return userIdentity;
            }
        }
    }
}